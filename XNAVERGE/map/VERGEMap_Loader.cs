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
    public partial class VERGEMap {
        // when true, the editor will error on an illegal tile index. When false, loads them as 0. 
        // This is false by default because some versions of maped3 have a bug that occasionally
        // saves unobstructed tiles with illegal values, and there are many such maps "in the wild".
        public static bool STRICT_TILE_LOADING = false; 

        public VERGEMap(String filename) {            
            Stream stream = null;
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            String cur_str;
            Vector2 cur_parallax;
            int cur_int;

            if (!File.Exists(filename)) throw new FileNotFoundException("The file \"" + filename + "\" does not exist.");

            try { // Load the map                

                stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                bin_reader = new BinaryReader(stream);
                str_reader = new StreamReader(stream, Encoding.UTF8, false, 256);

                // For legacy reasons, many of these strings have fixed length and are padded by nulls, so we know
                // exactly how many bytes to read.

                // ----------------------------------------------------
                // READ HEADER 

                cur_str = Utility.read_known_length_string(str_reader, 6); // verify signature
                if (cur_str != "V3MAP") throw new IOException(filename + " is not a VERGE MAP file.");

                // ...check version (currently only v2 is supported)
                bin_reader.BaseStream.Seek(6, SeekOrigin.Begin);
                _version = bin_reader.ReadInt32();
                if (_version != 2) throw new Exception(filename + " is a version " + _version + " MAP. Currently only version 2 is supported.");

                // this is the offset that denotes the beginning of the compiled vc code. Since the engine 
                // can't execute vc code, it's discarded.
                bin_reader.ReadInt32();

                str_reader.DiscardBufferedData();
                name = Utility.read_known_length_string(str_reader, 256);
                _vsp = Utility.read_known_length_string(str_reader, 256);
                _music = Utility.read_known_length_string(str_reader, 256);
                renderstring = Utility.read_known_length_string(str_reader, 256);
                initscript = Utility.read_known_length_string(str_reader, 256);

                // default entity starting coordinates
                start_x = (int)bin_reader.ReadUInt16();
                start_y = (int)bin_reader.ReadUInt16();

                // ----------------------------------------------------
                // LOAD TILE DATA

                // Load the tileset, assuming it's in the same directory
                cur_int = Math.Max(filename.LastIndexOf("\\"), filename.LastIndexOf("/"));
                if (cur_int < 0) cur_str = "";
                else cur_str = filename.Substring(0, cur_int + 1);
                _tileset = new Tileset(cur_str + _vsp);     

                _num_layers = bin_reader.ReadInt32();
                if (_num_layers <= 0) throw new Exception(filename + " is specified to have " + _num_layers + " layers.");
                parallax = new Vector2[_num_layers];
                tiles = new TileLayer[_num_layers];
                int lw, lh;
                for (int i = 0; i < _num_layers; i++) {
                    cur_str = Utility.read_known_length_string(str_reader, 256);

                    cur_parallax = new Vector2((float)bin_reader.ReadDouble(), (float)bin_reader.ReadDouble());

                    lw = (int)bin_reader.ReadUInt16();
                    lh = (int)bin_reader.ReadUInt16();
                    bin_reader.ReadByte(); // TODO: lucency (0 = opaque, 100 = transparent)

                    tiles[i] = new TileLayer(lw, lh, cur_parallax, cur_str);
                    decompress_layer_data(tiles[i], bin_reader, tileset.num_tiles, 0, true);
                }

                           

                // ----------------------------------------------------
                // LOAD OBSTRUCTION DATA

                obstruction_layer = new TileLayer(this.width, this.height, "Obstructions");
                decompress_layer_data(obstruction_layer, bin_reader, tileset.num_obs_tiles, 0, false);

                // ----------------------------------------------------
                // LOAD ZONE DATA

                zone_layer = new TileLayer(this.width, this.height, "Zones");
                decompress_layer_data(zone_layer, bin_reader, UInt16.MaxValue, 0, true); 
                _num_zones = bin_reader.ReadInt32(); // number of zones

                // Okay, decompress_layer_data validated all zones as being nonnegative, but we didn't know how many zones there were
                // when we loaded them, so we couldn't do upper bounds-checking. We have to do that part here separately.
                // This may seem frivolous, but Maped has some data corruption issues, so it's worth the trouble.
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        if (zone_layer.data[x][y] >= _num_zones) { // illegal value
                            if (STRICT_TILE_LOADING) throw new Exception("Bad zone found at coordinates (" + x + "," + y +
                                "). Expected a value between 0 and " + _num_zones + " but got " + zone_layer.data[x][y] + ". There is a known Maped3 bug " +
                                "that sometimes produces files with invalid tile data; alternatively, you may have specified the wrong tileset for this map." +
                                "You will have to either fix the map manually or set VERGEMap.STRICT_TILE_LOADING to false.");
                            else zone_layer.data[x][y] = 0;
                        }
                    }
                }

                zones = new Zone[_num_zones + 2]; // +2 to give a little room for expansion without needing an array copy
                
                for (int i = 0; i < _num_zones; i++) {
                    load_zone_from_file(i, bin_reader, str_reader);
                }

                // ----------------------------------------------------
                // LOAD ENTITY DATA

                _num_entities = bin_reader.ReadInt32();                

                if (_num_entities <= STARTING_ENTITY_ARRAY_SIZE) entities = new Entity[STARTING_ENTITY_ARRAY_SIZE];
                else entities = new Entity[_num_entities + 2]; // The +2 is just to give you a little leeway to add without IMMEDIATELY initiating an array copy

                for (int i = 0; i < _num_entities; i++) {
                    load_entity_from_file(i, bin_reader, str_reader);
                }

                // the rest of the file consists of the compiled vc for the map scripts, which we are ignoring.



                renderstack = new RenderStack(this, renderstring);
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

        // If as_shorts is true, the data are read in as little-endian unsigned shorts. 
        // If it's false, the data is read as bytes. Either way, it's expanded to integers.
        private void decompress_layer_data(TileLayer layer, BinaryReader reader, int number_of_types, int default_value, bool as_shorts) {
            int decompressed_size = reader.ReadInt32();
            int compressed_size = reader.ReadInt32();
            byte[] inbuf = new byte[compressed_size];
            byte[] outbuf = new byte[decompressed_size];
            reader.Read(inbuf, 0, inbuf.Length);
            Inflater inflater = new Inflater(false);
            inflater.SetInput(inbuf);
            inflater.Inflate(outbuf);
            for (int x = 0; x < layer.width; x++) {
                for (int y = 0; y < layer.height; y++) {
                    if (as_shorts) // data is little-endian shorts
                        layer.data[x][y] = ((int)outbuf[(x + y * layer.width) * 2]) | (((int)outbuf[(x + y * layer.width) * 2 + 1]) << 8);
                    else // data is bytes
                        layer.data[x][y] = (int)outbuf[x + y * layer.width];
                    if (layer.data[x][y] < 0 || layer.data[x][y] >= number_of_types) { // illegal value
                        if (STRICT_TILE_LOADING) throw new Exception("Bad tile (possibly obstile or zone) found at coordinates (" + x + "," + y +
                            "). Expected a value between 0 and " + number_of_types + " but got " + layer.data[x][y] + ". There is a known Maped3 bug " +
                            "that sometimes produces files with invalid tile data; alternatively, you may have specified the wrong tileset for this map." +
                            "You will have to either fix the map manually or set VERGEMap.STRICT_TILE_LOADING to false.");
                        else layer.data[x][y] = default_value;
                    }
                }
            }
        }

        private void load_zone_from_file(int number, BinaryReader bin_reader, StreamReader str_reader) {
            Zone zone;
            String name, script;
            byte chance, adj;
            name = Utility.read_known_length_string(str_reader, 256); // name
            script = Utility.read_known_length_string(str_reader, 256); // event
            chance = bin_reader.ReadByte(); // activation chance
            bin_reader.ReadByte(); // delay (ignored because it's redundant and maped doesnt support it)
            adj = bin_reader.ReadByte(); // adjacent activation mode (1 or 0)
            zone = new Zone(name, script, ((double)chance)/255, (adj == 1));
            zones[number] =zone;
        }

        private void load_entity_from_file(int number, BinaryReader bin_reader, StreamReader str_reader) {
            Entity ent;            
            int x, y, facing, speed;
            bool obstructable, obstructing, autoface;
            String filename, ent_name, movestring;
            
            x = bin_reader.ReadUInt16() * _tileset.tilesize; // x
            y = bin_reader.ReadUInt16() * _tileset.tilesize; // y            
            facing = bin_reader.ReadByte(); // initial facing direction 
            obstructable = (bin_reader.ReadByte() == 0); // obstructable
            obstructing = (bin_reader.ReadByte() == 0); // obstructs others
            autoface = (bin_reader.ReadByte() == 0); // autoface on adjacent activation
            speed = bin_reader.ReadUInt16(); // speed (pixels moved per second)
            bin_reader.ReadByte(); // activation mode (unused)
            bin_reader.ReadByte(); // movement mode
            bin_reader.ReadUInt16(); // wander rect x
            bin_reader.ReadUInt16(); // wander rect y
            bin_reader.ReadUInt16(); // wander rect x2
            bin_reader.ReadUInt16(); // wander rect y2
            bin_reader.ReadUInt16(); // wander delay (centiseconds)
            bin_reader.ReadInt32(); // "expand" (unused)            
            
            movestring = Utility.read_known_length_string(str_reader, 256); // movestring
            filename = Utility.read_known_length_string(str_reader, 256); // chr file
            
            ent_name = Utility.read_known_length_string(str_reader, 256); // description (chr name)                        
            ent = new Entity(filename, ent_name);             
            ent.x = x;
            ent.y = y;
            ent.facing = (Direction)facing;
            ent.obstructable = obstructable;
            ent.obstructing = obstructing;
            ent.autoface = autoface;
            ent.speed = speed;
            ent.on_activation = Utility.read_known_length_string(str_reader, 256); // activation event                
            ent.set_movestring(movestring); 
            entities[number] = ent;
            ent.index = number;
        }
    }
}
