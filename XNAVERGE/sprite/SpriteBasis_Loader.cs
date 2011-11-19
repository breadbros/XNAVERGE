using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public partial class SpriteBasis {
        // load from CHR file
        public SpriteBasis(String filename) {            
            Stream stream = null;
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            String cur_str;
            int cur_int, bit_depth, compressed_size, decompressed_size, dim;
            uint transparency;
            byte[] inbuf, outbuf;

            animations = new Dictionary<string,SpriteAnimation>();

            if (!File.Exists(filename)) throw new FileNotFoundException("The file \"" + filename + "\" does not exist.");

            try { 
                
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                bin_reader = new BinaryReader(stream);
                str_reader = new StreamReader(stream, Encoding.UTF8, false, 256);

                // For legacy reasons, many of these strings have fixed length and are padded by nulls, so we know
                // exactly how many bytes to read.

                // ...check header to confirm filetype                                                 

                cur_str = Utility.read_known_length_string(str_reader, 4);
                if (cur_str != "CHR") throw new IOException(filename + " is not a VERGE CHR file.");

                // ...check version (currently only v5 is supported)
                bin_reader.BaseStream.Seek(4, SeekOrigin.Begin);
                cur_int = bin_reader.ReadInt32();
                if (cur_int != 5) throw new Exception(filename + " is a version " + cur_int + " CHR. Currently only version 5 is supported.");

                bit_depth = bin_reader.ReadInt32();
                if (bit_depth == 32) throw new Exception("Oh wow, this is a 32bpp chr file! I thought those were an urban legend. You'd better talk to Gayo about this.");
                else if (bit_depth != 24) throw new Exception(filename + " lists an invalid bit depth. (" + bit_depth + " given, must be 24 or 32)");

                bin_reader.ReadInt32(); // the enigmatic "tcol" field, which is inconsistently filled and universally ignored.                

                // Next three bytes are R, G, B values for the transparency colour. We want these in XNA's ABGR format.
                transparency = Utility.convert_rgb_to_abgr(bin_reader.ReadBytes(3), 0, 0);

                bin_reader.ReadByte(); // this is supposed to be the alpha byte of the transparent colour, but it seems to always be set to 0, so I'm ignoring it.

                default_hitbox = new Rectangle(bin_reader.ReadInt32(), bin_reader.ReadInt32(), bin_reader.ReadInt32(), bin_reader.ReadInt32()); // entity hotspot

                _frame_width = bin_reader.ReadInt32();
                if (_frame_width <= 0) throw new Exception(filename + " specifies a frame width of " + _frame_width);
                _frame_height = bin_reader.ReadInt32();
                if (_frame_height <= 0) throw new Exception(filename + " specifies a frame width of " + _frame_height);
                _num_frames = bin_reader.ReadInt32();
                if (_num_frames <= 0) throw new Exception(filename + " says it has " + _num_frames + " frames.");

                // Idle frames. These are converted to one-frame, non-looping "animations."
                load_idle_frame_from_chr("Idle Down", bin_reader);
                load_idle_frame_from_chr("Idle Up", bin_reader);
                load_idle_frame_from_chr("Idle Left", bin_reader);
                load_idle_frame_from_chr("Idle Right", bin_reader);

                // Movement animation patterns. 
                load_animation_from_chr("Walk Down", bin_reader, str_reader);
                load_animation_from_chr("Walk Up", bin_reader, str_reader);
                load_animation_from_chr("Walk Left", bin_reader, str_reader);
                load_animation_from_chr("Walk Right", bin_reader, str_reader);
                load_animation_from_chr("Walk DownLeft", bin_reader, str_reader);
                load_animation_from_chr("Walk DownRight", bin_reader, str_reader);
                load_animation_from_chr("Walk UpLeft", bin_reader, str_reader);
                load_animation_from_chr("Walk UpRight", bin_reader, str_reader);

                cur_int = bin_reader.ReadInt32(); // number of custom animations (these have never been supported/used)
                if (cur_int != 0) throw new Exception(filename + " uses custom animation patterns, which are not yet supported.");
                bin_reader.ReadInt32(); // compression method. this is inconsistently set but consistently ignored -- all chrs use zlib.

                // Extract compressed frame data.
                decompressed_size = bin_reader.ReadInt32();
                compressed_size = bin_reader.ReadInt32();
                inbuf = new byte[compressed_size];
                outbuf = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                Inflater inflater = new Inflater(false);
                inflater.SetInput(inbuf);
                inflater.Inflate(outbuf);
                dim = Utility.smallest_bounding_square(_frame_width, _frame_height, _num_frames);
                _per_row = dim / _frame_width;                
                uint[] pixels = new uint[dim * dim];
                int pixels_per_frame = _frame_width * _frame_height;

                int x, y;
                frame_box = new Rectangle[_num_frames];
                for (int cur_frame = 0; cur_frame < _num_frames; cur_frame++) {
                    frame_box[cur_frame] = new Rectangle((cur_frame % _per_row) * _frame_width, (cur_frame / _per_row) * _frame_height, _frame_width, _frame_height);
                    for (int cur_pixel = 0; cur_pixel < pixels_per_frame; cur_pixel++) {
                        x = (cur_frame % _per_row) * _frame_width + (cur_pixel % _frame_width);
                        y = (cur_frame / _per_row) * _frame_height + (cur_pixel / _frame_width);
                        pixels[y * dim + x] = Utility.convert_rgb_to_abgr(outbuf, (cur_frame * pixels_per_frame + cur_pixel) * 3, transparency); // 3 bytes per pixel
                    }
                }

                image = new Texture2D(VERGEGame.game.GraphicsDevice, dim, dim);
                image.SetData<uint>(pixels);

                //using (FileStream fs = new FileStream("test.png", FileMode.Create, FileAccess.Write)) {
                //image.SaveAsPng(fs, image.Width, image.Height);
                //}

            }
            catch (EndOfStreamException) {
                throw new Exception(filename + " was shorter than expected.");
            }
            finally {
                if (stream != null) stream.Dispose();
                if (bin_reader != null) bin_reader.Dispose();
                if (str_reader != null) str_reader.Dispose();
            }

        }

        private void load_animation_from_chr(String anim_name, BinaryReader bread, StreamReader sread) {
            SpriteAnimation animation;
            int len = bread.ReadInt32() + 1; // +1 for the null byte at the end, not included in the length
            sread.DiscardBufferedData();            
            animation = new SpriteAnimation(anim_name, _num_frames, Utility.read_known_length_string(sread, len));
            bread.BaseStream.Seek(len - 256, SeekOrigin.Current); // streamreader will have gone too far to fill its buffer, so back up a bit
            animations.Add(anim_name, animation);
        }

        private void load_idle_frame_from_chr(String anim_name, BinaryReader bread) {
            int idleframe = bread.ReadInt32();
            animations.Add(anim_name, new SpriteAnimation(anim_name, _num_frames, "F" + idleframe.ToString(), AnimationStyle.Once));
        }
        
    }
}
