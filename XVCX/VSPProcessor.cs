using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
using TOutput = XVCX.ProcessedVSP;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "VERGE VSP Processor")]
    public class VSPProcessor : ContentProcessor<TInput, TOutput> {
        public const int VSP_HEADER = 5264214; // the first four bytes of a VERGE VSP file is set to this value

        public override TOutput Process(TInput input, ContentProcessorContext context) {
            ProcessedVSP tileset = new ProcessedVSP();
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            Inflater inflater;
            int cur_int, decompressed_size, compressed_size;
            byte[] inbuf, outbuf;
            String cur_str;
 
            try {
                context.Logger.LogMessage("Processing VSP file.");
                bin_reader = new BinaryReader(input);
                str_reader = new StreamReader(input, Encoding.ASCII, false, 256);

                // ----------------------------------------------------
                // READ HEADER

                context.Logger.LogMessage("Reading header...");

                // ...verify file signature
                cur_int = bin_reader.ReadInt32();
                if (cur_int != VSP_HEADER) throw new PipelineException("Not a VSP file.");

                // ...check version (currently only v6 is supported)
                tileset.version = bin_reader.ReadInt32();
                if (tileset.version != 6) throw new PipelineException("This is a version " + tileset.version + " VSP. Currently only version 6 is supported.");

                tileset.tilesize = bin_reader.ReadInt32();
                if (tileset.tilesize <= 0) throw new PipelineException("The tile size is specified as " + tileset.tilesize + ".");
               
                bin_reader.ReadInt32(); // format field. currently unused.

                // ----------------------------------------------------
                // LOAD TILE DATA                

                tileset.num_tiles = bin_reader.ReadInt32();
                if (tileset.num_tiles <= 0) throw new PipelineException("The number of tiles is specified as " + tileset.num_tiles + ".");                

                bin_reader.ReadInt32(); // compression field. unused and unreliable -- assume zlib regardless.

                context.Logger.LogMessage("Loading compressed tile atlas...");

                // decompress tile data
                decompressed_size = bin_reader.ReadInt32();
                if (decompressed_size != tileset.num_tiles * tileset.tilesize * tileset.tilesize)
                    throw new PipelineException("Expected " + tileset.num_tiles + "x" + tileset.tilesize + "*" + tileset.tilesize +
                                        " = " + (tileset.num_tiles * tileset.tilesize * tileset.tilesize).ToString() + " bytes of tile data, but got "
                                        + decompressed_size + " bytes.");
                compressed_size = bin_reader.ReadInt32();
                inbuf = new byte[compressed_size];
                outbuf = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                inflater = new Inflater(false);
                context.Logger.LogMessage("Decompressing...");
                inflater.SetInput(inbuf);                
                inflater.Inflate(outbuf);

                tileset.texture_dim = Utility.smallest_bounding_square(tileset.tilesize, tileset.tilesize, tileset.num_tiles); // side dimension of the entire texture                
                tileset.tiledata = new uint[tileset.texture_dim*tileset.texture_dim];
                context.Logger.LogMessage("Converting pixels to XNA's ABGR format...");
                // Load pixel data. In the vsp, pixels are ordered left to right, top to bottom, one tile at a time, in 24bpp.
                // Once loaded they are in 32bpp, and the transparency colour has been converted to 0x0.
                for (int i = 0; i < tileset.tiledata.Length;i++) 
                    tileset.tiledata[i] = Utility.convert_rgb_to_abgr(outbuf, i*3, 0xFFFF00FFU); // 3 bytes per pixel

                // ----------------------------------------------------
                // LOAD ANIMATION DATA

                context.Logger.LogMessage("Loading animations...");
                
                tileset.num_animations = bin_reader.ReadInt32();
                if (tileset.num_animations < 0) throw new PipelineException("The number of animations is specified as " + tileset.num_animations + ".");                
                else if (tileset.num_animations == 0) context.Logger.LogMessage("...no animations in this tileset.");
                else {
                    tileset.animations = new ProcessedTileAnimation[tileset.num_animations];
                    for (int i = 0; i < tileset.num_animations; i++) {
                        tileset.animations[i] = new ProcessedTileAnimation();
                        tileset.animations[i].name = Utility.read_known_length_string(str_reader, 256);
                        tileset.animations[i].start = bin_reader.ReadInt32();
                        if (tileset.animations[i].start < 0) throw new PipelineException("Animations #" + i + "(" + tileset.animations[i].name + ") lists its starting index as " + tileset.animations[i].start + ".");
                        tileset.animations[i].end = bin_reader.ReadInt32();
                        if (tileset.animations[i].end < 0) throw new PipelineException("Animations #" + i + "(" + tileset.animations[i].name + ") lists its ending index as " + tileset.animations[i].end + ".");
                        tileset.animations[i].delay = bin_reader.ReadInt32();
                        if (tileset.animations[i].delay <= 0) throw new PipelineException("Animations #" + i + "(" + tileset.animations[i].name + ") has a non-positive delay (" + tileset.animations[i].delay + ").");
                        tileset.animations[i].mode = bin_reader.ReadInt32(); // We won't bother trying to validate this right now
                    }
                }

                // ----------------------------------------------------
                // LOAD OBSTRUCTION DATA

                tileset.num_obs_tiles = bin_reader.ReadInt32();                

                if (tileset.num_tiles <= 0) throw new PipelineException("The number of obstruction tiles is specified as " + tileset.num_obs_tiles + ".");

                bin_reader.ReadInt32(); // compression field. unused and unreliable -- assume zlib regardless.

                context.Logger.LogMessage("Loading compressed obstruction tile atlas...");

                // decompress tile data
                decompressed_size = bin_reader.ReadInt32();
                if (decompressed_size != tileset.num_obs_tiles * tileset.tilesize * tileset.tilesize)
                    throw new PipelineException("Expected " + tileset.num_obs_tiles + "x" + tileset.tilesize + "*" + tileset.tilesize +
                        " = " + (tileset.num_obs_tiles * tileset.tilesize * tileset.tilesize).ToString() + " bytes of obstruction tile data, but got "
                        + decompressed_size + " bytes.");
                
                compressed_size = bin_reader.ReadInt32();
                inbuf = new byte[compressed_size];
                tileset.obsdata = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                inflater = new Inflater(false);
                context.Logger.LogMessage("Decompressing...");
                inflater.SetInput(inbuf);
                inflater.Inflate(tileset.obsdata); // keep this as a byte array for now
            }
            catch (EndOfStreamException) {
                throw new PipelineException("VSP file was shorter than expected.");
            }
            finally {
                if (bin_reader != null) bin_reader.Dispose();                
            }

            return tileset;
        }
    }

    // Stores processed and validated VSP data suitable for writing to binary.
    public class ProcessedVSP {
        public int version, tilesize, num_tiles, num_obs_tiles, num_animations, texture_dim;
        public byte[] obsdata;
        public uint[] tiledata;
        public ProcessedTileAnimation[] animations;
    }

    public struct ProcessedTileAnimation {
        public String name;
        public int start, end, delay, mode;
    }
}