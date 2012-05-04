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
using TOutput = XVCX.ProcessedMap;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "VERGE .map File Processor")]
    public class VERGEMapProcessor : ContentProcessor<TInput, TOutput> {
        public override TOutput Process(TInput input, ContentProcessorContext context) {
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            String cur_str;            
            int cur_int;
            ProcessedMap map = new ProcessedMap();

            try {
                context.Logger.LogMessage("Processing MAP file.");
                bin_reader = new BinaryReader(input);
                str_reader = new StreamReader(input, Encoding.ASCII, false, 256);

                // ----------------------------------------------------
                // READ HEADER 

                context.Logger.LogMessage("Reading header...");

                cur_str = Utility.read_known_length_string(str_reader, 6); // verify signature
                if (cur_str != "V3MAP") throw new PipelineException("This is not a VERGE MAP file.");

                // ...check version (currently only v2 is supported)
                bin_reader.BaseStream.Seek(6, SeekOrigin.Begin);
                cur_int = bin_reader.ReadInt32();
                if (cur_int != 2) throw new PipelineException("This is a version " + cur_int + " MAP. Currently only version 2 is supported.");

                // this is the offset that denotes the beginning of the compiled vc code. Since the engine 
                // can't execute vc code, it's discarded.
                bin_reader.ReadInt32();

                str_reader.DiscardBufferedData();
                map.name = Utility.read_known_length_string(str_reader, 256);
                map.vsp = Utility.read_known_length_string(str_reader, 256);
                map.music = Utility.read_known_length_string(str_reader, 256);
                map.renderstring = Utility.read_known_length_string(str_reader, 256);
                map.initscript = Utility.read_known_length_string(str_reader, 256);

                // default entity starting coordinates
                map.start_x = (int)bin_reader.ReadUInt16();
                map.start_y = (int)bin_reader.ReadUInt16();

                // ----------------------------------------------------
                // LOAD TILE DATA

                map.num_layers = bin_reader.ReadInt32();
                if (map.num_layers <= 0) throw new PipelineException("The number of tile layers is specified as " + map.num_layers + ".");
                map.layers = new ProcessedLayer[map.num_layers];

                context.Logger.LogMessage("Reading data for the tile layers...");
                for (int i = 0; i < map.num_layers; i++) {
                    map.layers[i] = new ProcessedLayer(Utility.read_known_length_string(str_reader, 256));
                    map.layers[i].parallax = new Vector2((float)bin_reader.ReadDouble(), (float)bin_reader.ReadDouble());
                    map.layers[i].w = (int)bin_reader.ReadUInt16();
                    map.layers[i].h = (int)bin_reader.ReadUInt16();                    
                    
                    // .map files store layerwide alpha as "lucency," a value between 0 and 100. It goes 
                    // in the opposite direction, so lucency 0 = alpha 1.0 and lucency 100+ = alpha 0.0. 
                    map.layers[i].alpha = ((double)(100 - Math.Min((byte)100,bin_reader.ReadByte())))/100.0;
                    
                    decompress_layer_data(map.layers[i], bin_reader, true);
                }

                // ----------------------------------------------------
                // LOAD OBSTRUCTION DATA

                context.Logger.LogMessage("Loading and decompressing obstruction layer data...");

                map.obslayer = new ProcessedLayer("Obstructions");
                map.obslayer.w = map.layers[0].w; // data layers inherit the size of the first declared tile layer
                map.obslayer.h = map.layers[0].h;
                decompress_layer_data(map.obslayer, bin_reader, false);

                // ----------------------------------------------------
                // LOAD ZONE DATA

                context.Logger.LogMessage("Loading and decompressing zone layer data...");

                map.zonelayer = new ProcessedLayer("Zones");
                map.zonelayer.w = map.layers[0].w; // data layers inherit the size of the first declared tile layer
                map.zonelayer.h = map.layers[0].h;
                decompress_layer_data(map.zonelayer, bin_reader, true);

                context.Logger.LogMessage("Reading zone type specifications...");
                map.num_zones = bin_reader.ReadInt32();
                map.zones = new ProcessedZone[map.num_zones];
                for (int i = 0; i < map.num_zones; i++) map.zones[i] = load_zone_from_file(bin_reader, str_reader);

                // ----------------------------------------------------
                // LOAD ENTITY DATA

                context.Logger.LogMessage("Reading preloaded entity details...");
                map.num_ents = bin_reader.ReadInt32();
                map.entities = new ProcessedEntity[map.num_ents];
                for (int i = 0; i < map.num_ents; i++) map.entities[i] = load_entity_from_file(context, bin_reader, str_reader);

            }
            catch (EndOfStreamException) {
                throw new PipelineException("VSP file was shorter than expected.");
            }
            finally {
                if (bin_reader != null) bin_reader.Dispose();
                if (str_reader != null) str_reader.Dispose();
            }

            // the rest of the file consists of the compiled vc for the map scripts, which we are ignoring.

            context.Logger.LogMessage("Finished processing MAP file.");

            return map;
        }

        // If as_shorts is true, the data are read in as little-endian unsigned shorts. 
        // If it's false, the data is read as bytes. Either way, it's expanded to integers.
        private void decompress_layer_data(ProcessedLayer layer, BinaryReader reader, bool as_shorts) {
            int decompressed_size = reader.ReadInt32();
            int compressed_size = reader.ReadInt32();
            byte[] inbuf = new byte[compressed_size];
            byte[] outbuf = new byte[decompressed_size];
            layer.tiles = new int[layer.w * layer.h];
            if (as_shorts) {
                if (decompressed_size != layer.tiles.Length * 2)
                    throw new PipelineException("Expected " + layer.tiles.Length + "x2 = " + (layer.tiles.Length * 2).ToString() +
                        " bytes of tile data, but got " + decompressed_size + " bytes.");              
            }
            else {
                if (decompressed_size != layer.tiles.Length)
                    throw new PipelineException("Expected " + layer.tiles.Length + " bytes of tile data, but got " + decompressed_size + " bytes.");               
            }
            reader.Read(inbuf, 0, inbuf.Length);
            Inflater inflater = new Inflater(false);
            inflater.SetInput(inbuf);
            inflater.Inflate(outbuf);
            for (int i = 0; i < layer.tiles.Length; i++) {                
                if (as_shorts) // data is little-endian shorts
                    layer.tiles[i] = ((int)outbuf[i*2]) | (((int)outbuf[i*2 + 1]) << 8);
                else // data is bytes
                    layer.tiles[i] = (int)outbuf[i];
            }
        }

        private ProcessedZone load_zone_from_file(BinaryReader bin_reader, StreamReader str_reader) {                        
            ProcessedZone zone = new ProcessedZone();
            zone.name = Utility.read_known_length_string(str_reader, 256); // name
            zone.script = Utility.read_known_length_string(str_reader, 256); // event
            zone.chance = ((double)bin_reader.ReadByte()) / 255; // activation chance
            bin_reader.ReadByte(); // delay (ignored because it's redundant and maped doesnt support it)
            zone.adj = (bin_reader.ReadByte() != 0); // adjacent activation mode (1 or 0)
            return zone;
            
        }

        private ProcessedEntity load_entity_from_file(ContentProcessorContext context, BinaryReader bin_reader, StreamReader str_reader) {
            ProcessedEntity ent = new ProcessedEntity();
            ent.start = new Point((int)bin_reader.ReadUInt16(), (int)bin_reader.ReadUInt16()); // starting tile
            ent.facing = bin_reader.ReadByte(); // initial facing direction 
            ent.obstructable = (bin_reader.ReadByte() != 0); // obstructable
            ent.obstructs = (bin_reader.ReadByte() != 0); // obstructs others
            ent.autoface = (bin_reader.ReadByte() != 0); // autoface on adjacent activation
            ent.speed = bin_reader.ReadUInt16(); // speed (pixels moved per second)
            bin_reader.ReadByte(); // "activation mode" (unused)
            ent.movemode = bin_reader.ReadByte(); // movement mode
            if (ent.movemode > 2) ent.movemode = 0; 
            ent.wander_ul = new Point((int)bin_reader.ReadUInt16(), (int)bin_reader.ReadUInt16()); // wander rect upper left-hand bound
            ent.wander_lr = new Point((int)bin_reader.ReadUInt16(), (int)bin_reader.ReadUInt16()); // wander rect lower right-hand bound
            ent.delay = bin_reader.ReadUInt16(); // wander delay (centiseconds)
            bin_reader.ReadInt32(); // "expand" (unused)            

            ent.movestring = Utility.read_known_length_string(str_reader, 256); // starting movestring
            ent.chr = Utility.read_known_length_string(str_reader, 256); // chr filename
            ent.name = Utility.read_known_length_string(str_reader, 256); // movestring
            ent.actscript = Utility.read_known_length_string(str_reader, 256); // script called when entity is triggered
            
            if (!Enum.IsDefined(typeof(Direction), ent.facing)) {
                context.Logger.LogImportantMessage("Warning: Entity \"{0}\" has a facing value of {1}, which does not correspond to a valid Direction. Facing has been set to 0 (Down).", ent.name, ent.facing);
                ent.facing = (int) Direction.Down;
            }
                            
            return ent;
        }

    }

    public class ProcessedMap {
        public int version, start_x, start_y, num_layers, num_zones, num_ents;
        public string name, vsp, music, initscript, renderstring;        
        public ProcessedLayer[] layers;
        public ProcessedLayer obslayer, zonelayer;
        public ProcessedZone[] zones;
        public ProcessedEntity[] entities;
    }
    public class ProcessedLayer {
        public ProcessedLayer(string layername) { name = layername; }
        public String name;
        public int w, h;
        public Vector2 parallax;
        public double alpha;
        public int[] tiles;
    }
    public class ProcessedEntity {
        public String name, movestring, actscript, chr;
        public bool obstructs, obstructable, autoface;
        public int facing, speed, movemode, delay;
        public Point start, wander_ul, wander_lr;
    }
    public class ProcessedZone {
        public String name, script;
        public double chance;
        public bool adj;
    }
}