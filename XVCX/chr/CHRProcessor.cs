using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using XNAVERGE;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using TInput = System.IO.MemoryStream;
using TOutput = XVCX.ProcessedSprite;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.

    /// </summary>
    [ContentProcessor(DisplayName = "VERGE CHR Processor")]
    public class CHRProcessor : ContentProcessor<TInput, TOutput> {
        public static readonly String[] CHR_DIRECTIONS = { "Down", "Up", "Left", "Right", "DownLeft", "DownRight", "UpLeft", "UpRight" }; // chr uses nonstandard ordering for these

        public override TOutput Process(TInput input, ContentProcessorContext context) {
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            String cur_str;
            int cur_int, cur_anim, bit_depth, compressed_size, decompressed_size, pixels_per_frame, x, y;
            uint transparency; // the pixel to use for transparency when processing this sprite. 
            byte[] inbuf, outbuf;
            ProcessedSprite spr = new ProcessedSprite();

            try {
                bin_reader = new BinaryReader(input);
                str_reader = new StreamReader(input, Encoding.UTF8, false, 256);

                // ----------------------------------------------------
                // READ HEADER 

                context.Logger.LogMessage("Reading header...");

                cur_str = Utility.read_known_length_string(str_reader, 4);
                if (cur_str != "CHR") throw new PipelineException("This is not a VERGE CHR file.");

                // ...check version (currently only v5 is supported)
                bin_reader.BaseStream.Seek(4, SeekOrigin.Begin);
                cur_int = bin_reader.ReadInt32();
                if (cur_int != 5) throw new PipelineException("This is a version " + cur_int + " CHR. Currently only version 5 is supported.");

                bit_depth = bin_reader.ReadInt32();
                if (bit_depth == 32) throw new PipelineException("Oh wow, this is a 32bpp chr file! I thought those were an urban legend. You'd better talk to Gayo about this.");
                else if (bit_depth != 24) throw new PipelineException("Invalid bit depth. (" + bit_depth + " given, must be 24 or 32)");

                bin_reader.ReadInt32(); // the enigmatic "tcol" field, which is inconsistently filled and universally ignored.

                // Next three bytes are R, G, B values for the transparency colour. We want these in XNA's ABGR format.
                transparency = Utility.convert_rgb_to_abgr(bin_reader.ReadBytes(3), 0, 0);

                bin_reader.ReadByte(); // this is supposed to be the alpha byte of the transparent colour, but it seems to always be set to 0, so we'll ignore it.

                spr.hitbox = new Rectangle(bin_reader.ReadInt32(), bin_reader.ReadInt32(), bin_reader.ReadInt32(), bin_reader.ReadInt32());

                spr.fw = bin_reader.ReadInt32();
                if (spr.fw <= 0) throw new PipelineException("The frame width is specified as " + spr.fw);
                spr.fh = bin_reader.ReadInt32();
                if (spr.fh <= 0) throw new PipelineException("The frame height is specified as " + spr.fh);
                spr.frames = bin_reader.ReadInt32();
                if (spr.frames <= 0) throw new PipelineException("The file says it has " + spr.frames + " frames.");

                // ----------------------------------------------------
                // READ ANIMATIONS

                context.Logger.LogMessage("Reading idle frames...");

                spr.num_anim = 8;
                spr.anim = new SpriteAnimation[spr.num_anim];
                cur_anim = 0;
                for (int i=0; i < 4; i++) {
                    spr.anim[cur_anim] = load_idle_frame_from_chr(spr, "Idle " + CHR_DIRECTIONS[i], bin_reader);
                    cur_anim++;
                }
                context.Logger.LogMessage("Reading walk animations...");
                for (int i = 0; i < 4; i++) {
                    // CHRs have diagonal walks, but they're just copies of the horizontal walks and can't be specified, 
                    // so there's no point in saving them here.
                    spr.anim[cur_anim] = load_animation_from_chr(spr, "Walk " + CHR_DIRECTIONS[i], bin_reader, str_reader);
                    cur_anim++;
                }
                for (int i = 0; i < 4; i++) {
                    // Discard the redundant "diagonal" walks.
                    load_animation_from_chr(spr, "Walk " + CHR_DIRECTIONS[i], bin_reader, str_reader);                    
                }
                cur_int = bin_reader.ReadInt32(); // number of custom animations (these have never been supported/used)
                if (cur_int != 0) throw new PipelineException("This CHR uses custom animation patterns, which are not yet supported.");
                bin_reader.ReadInt32(); // compression method. this is inconsistently set but consistently ignored -- all chrs use zlib.                

                // ----------------------------------------------------
                // READ IMAGE DATA

                context.Logger.LogMessage("Loading compressed tile atlas...");
                decompressed_size = bin_reader.ReadInt32();
                compressed_size = bin_reader.ReadInt32();
                inbuf = new byte[compressed_size];
                outbuf = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                Inflater inflater = new Inflater(false);
                context.Logger.LogMessage("Decompressing...");
                inflater.SetInput(inbuf);
                inflater.Inflate(outbuf);
                spr.texture_dim = Utility.smallest_bounding_square(spr.fw, spr.fh, spr.frames);
                spr.per_row = spr.texture_dim / spr.fw;                
                pixels_per_frame = spr.fw * spr.fh;
                spr.pixels = new uint[spr.texture_dim * spr.texture_dim]; // unused (excess) pixels will stay at their initial value of 0
                
                context.Logger.LogMessage("Converting and realigning pixels...");
                // Load pixel data. In the vsp, pixels are ordered left to right, top to bottom, one tile at a time, in 24bpp.
                // Once loaded they are in 32bpp, they're ordered left to right and top to bottom for the entire texture 
                // (rather than for each tile in order), and the transparency colour has been converted to 0x0.                
                for (int cur_frame = 0; cur_frame < spr.frames; cur_frame++) {                    
                    for (int cur_pixel = 0; cur_pixel < pixels_per_frame; cur_pixel++) {
                        x = (cur_frame % spr.per_row) * spr.fw + (cur_pixel % spr.fw);
                        y = (cur_frame / spr.per_row) * spr.fh + (cur_pixel / spr.fw);
                        spr.pixels[y * spr.texture_dim + x] = Utility.convert_rgb_to_abgr(outbuf, (cur_frame * pixels_per_frame + cur_pixel) * 3, transparency); // 3 bytes per pixel
                    }
                }
            }
            catch (EndOfStreamException) {
                throw new Exception("The CHR file was shorter than expected.");
            }
            finally {
                if (bin_reader != null) bin_reader.Dispose();
                if (str_reader != null) str_reader.Dispose();
            }

            return spr;
        }

        private SpriteAnimation load_animation_from_chr(ProcessedSprite spr, String anim_name, BinaryReader bread, StreamReader sread) {
            SpriteAnimation animation;
            int len = bread.ReadInt32() + 1; // +1 for the null byte at the end, not included in the length
            sread.DiscardBufferedData();
            animation = new SpriteAnimation(anim_name, spr.frames, Utility.read_known_length_string(sread, len));
            bread.BaseStream.Seek(len - 256, SeekOrigin.Current); // streamreader will have gone too far to fill its buffer, so back up a bit
            return animation;
        }

        private SpriteAnimation load_idle_frame_from_chr(ProcessedSprite spr, String anim_name, BinaryReader bread) {
            int idleframe = bread.ReadInt32();
            return new SpriteAnimation(anim_name, spr.frames, "F" + idleframe.ToString(), AnimationStyle.Once);
        }

    }

    public class ProcessedSprite {
        public uint[] pixels;
        public int frames, per_row, fw, fh, texture_dim, num_anim;
        public Rectangle hitbox;
        public SpriteAnimation[] anim;
    }
}