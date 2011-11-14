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
   public class Tileset {        

        public const int VSP_HEADER = 5264214; // the first four bytes of a VERGE VSP file is set to this value
        protected int _tilesize, _num_tiles, _num_obs_tiles, _num_animations, _per_row;
        public Rectangle[] tile_frame;

        
        public int tilesize { get { return _tilesize; } }
        public int version;
        public int num_tiles { get { return _num_tiles; } }
        public int num_obs_tiles { get { return _num_obs_tiles; } }
        public int tiles_per_row { get { return _per_row; } }
        public bool[][][] obs; // [obstile][x][y]

        public int num_animations { get { return _num_animations; } }

        public Texture2D image;

        public Tileset() { }
        public Tileset(String filename) {
            Stream stream = null;
            BinaryReader bin_reader = null;
            StreamReader str_reader = null;
            int dim;

            if (!File.Exists(filename)) throw new FileNotFoundException("The file \"" + filename + "\" does not exist.");

            try { // Load the VSP
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                bin_reader = new BinaryReader(stream);
                str_reader = new StreamReader(stream, Encoding.UTF8, false, 256);

                // ----------------------------------------------------
                // READ HEADER

                // ...verify file signature
                int cur_int = bin_reader.ReadInt32();
                if (cur_int != Tileset.VSP_HEADER) throw new System.IO.IOException(filename + " is not a VSP file.");

                // ...check version (currently only v6 is supported)
                version = bin_reader.ReadInt32();
                if (version != 6) throw new Exception(filename + " is a version " + version + " VSP. Currently only version 6 is supported.");

                _tilesize = bin_reader.ReadInt32();
                if (_tilesize <= 0) throw new Exception(filename + " has its tile size set to " + _tilesize + ".");

                bin_reader.ReadInt32(); // format field. currently unused.

                // ----------------------------------------------------
                // LOAD TILE DATA

                _num_tiles = bin_reader.ReadInt32();
                if (_num_tiles <= 0) throw new Exception(filename + " has its number of tiles size set to " + _num_tiles + ".");
                tile_frame = new Rectangle[num_tiles];

                bin_reader.ReadInt32(); // compression field. unused and unreliable -- assume zlib regardless.

                // decompress tile data
                int decompressed_size = bin_reader.ReadInt32();                
                int compressed_size = bin_reader.ReadInt32();
                byte[] inbuf = new byte[compressed_size];
                byte[] outbuf = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                Inflater inflater = new Inflater(false);
                inflater.SetInput(inbuf);
                inflater.Inflate(outbuf);                
                
                dim = Utility.smallest_bounding_square(_tilesize, _tilesize, _num_tiles); // side dimension of the entire texture
                _per_row = dim / _tilesize; 
                uint[] pixels = new uint[dim*dim];                                
                int pixels_per_tile = _tilesize * _tilesize;

                int x, y;
                for (int cur_tile = 0; cur_tile < _num_tiles; cur_tile++) {
                    tile_frame[cur_tile] = new Rectangle((cur_tile % _per_row) * _tilesize, (cur_tile / _per_row) * _tilesize, _tilesize, _tilesize);
                    for (int cur_pixel = 0; cur_pixel < pixels_per_tile; cur_pixel++) {
                        x = (cur_tile % _per_row)*_tilesize + (cur_pixel % _tilesize);
                        y = (cur_tile / _per_row)*_tilesize + (cur_pixel / _tilesize);                        
                        pixels[y*dim + x] = Utility.convert_rgb_to_abgr(outbuf, (cur_tile*pixels_per_tile + cur_pixel)*3, 0xFFFF00FFU); // 3 bytes per pixel
                    }
                }

                image = new Texture2D(VERGEGame.game.GraphicsDevice, dim, dim);
                image.SetData<uint>(pixels);

                /*using (FileStream fs = new FileStream("test.png", FileMode.Create, FileAccess.Write)) {
                    image.SaveAsPng(fs, image.Width, image.Height);
                }*/

                // ----------------------------------------------------
                // LOAD ANIMATION DATA

                // animations. add this stuff later
                _num_animations = bin_reader.ReadInt32();
                for (int i = 0; i < _num_animations; i++) {
                    Utility.read_known_length_string(str_reader, 256); // name
                    bin_reader.ReadInt32(); // start index
                    bin_reader.ReadInt32(); // end index
                    bin_reader.ReadInt32(); // delay (in ticks)
                    bin_reader.ReadInt32(); // mode (1 = forward, 2 = reverse, 3 = random, 4 = back and forth)
                }

                // ----------------------------------------------------
                // LOAD OBSTRUCTION DATA

                _num_obs_tiles = bin_reader.ReadInt32();
                obs = new bool[_num_obs_tiles][][];                

                // decompress obs data (1 byte per pixel)
                decompressed_size = bin_reader.ReadInt32();
                compressed_size = bin_reader.ReadInt32();
                inbuf = new byte[compressed_size];
                outbuf = new byte[decompressed_size];
                bin_reader.Read(inbuf, 0, inbuf.Length);
                inflater = new Inflater(false);
                inflater.SetInput(inbuf);
                inflater.Inflate(outbuf);
                for (int i = 0; i < _num_obs_tiles; i++) {
                    obs[i] = new bool[tilesize][];
                    for (int ox = 0; ox < tilesize; ox++) {
                        obs[i][ox] = new bool[tilesize];
                        for (int oy = 0; oy < tilesize; oy++) {
                            obs[i][ox][oy] = outbuf[i * pixels_per_tile + oy * tilesize + ox] != 0;
                        }
                    }
                }

            }            
            catch (EndOfStreamException) {                 
                throw new Exception(filename + " was shorter than expected.");
            }
            finally {
                if (bin_reader != null) bin_reader.Dispose();
                else if (stream != null) stream.Dispose();
            }
        }

        public void set_tile_data(int new_num_tiles, int new_tilesize, Texture2D new_image) {
            int x=0, y=0;
            image = new_image;
            _num_tiles = new_num_tiles;
            _tilesize = new_tilesize;
            _per_row = new_image.Width / new_tilesize;
            for (int cur_tile = 0; cur_tile < new_num_tiles; cur_tile++) { // set up source rects for use in tile blitting
                tile_frame[cur_tile] = new Rectangle(x, y, new_tilesize, new_tilesize);
                x += new_tilesize;
                if (x <= new_image.Width) {
                    x = 0;
                    y += new_tilesize;
                }
            }
        }
        
        public void set_obs_data(int new_num_obs, byte[] new_obs) {
            int count = 0;
            _num_obs_tiles = new_num_obs;
            if (new_num_obs * _tilesize * _tilesize != new_obs.Length) 
                throw new ArgumentException("Because the tile size is " + _tilesize + " pixels, set_obs_data() expects an array of exactly " 
                    + new_num_obs + "x" + _tilesize + "x" + _tilesize + " bytes. However, the array passed contains " + new_obs.Length + 
                    " bytes.");
            obs = new bool[new_num_obs][][];
            for (int cur_tile = 0; cur_tile < new_num_obs; cur_tile++) {
                obs[cur_tile] = new bool[_tilesize][];
                // We need to loop over x inside y, but the array is the other way round (i.e. it goes bool[obstile][x][y]), 
                // so we'll run through and initialize the x subarrays for this tile first.
                for (int x = 0; x < _tilesize; x++) obs[cur_tile][x] = new bool[_tilesize];
 
                for (int y = 0; y < _tilesize; y++) {                                        
                    for (int x = 0; x < _tilesize; x++) {
                        obs[cur_tile][x][y] = (new_obs[count] != 0);
                        count++;
                    }
                }
            }
        }
        public void set_obs_data(int new_num_obs, bool[] new_obs) {
            int count = 0;
            _num_obs_tiles = new_num_obs;
            if (new_num_obs * _tilesize * _tilesize != new_obs.Length)
                throw new ArgumentException("Because the tile size is " + _tilesize + " pixels, set_obs_data() expects an array of exactly "
                    + new_num_obs + "x" + _tilesize + "x" + _tilesize + " bytes. However, the array passed contains " + new_obs.Length +
                    " bytes.");
            obs = new bool[new_num_obs][][];
            for (int cur_tile = 0; cur_tile < new_num_obs; cur_tile++) {
                obs[cur_tile] = new bool[_tilesize][];
                // We need to loop over x inside y, but the array is the other way round (i.e. it goes bool[obstile][x][y]), 
                // so we'll run through and initialize the x subarrays for this tile first.
                for (int x = 0; x < _tilesize; x++) obs[cur_tile][x] = new bool[_tilesize];

                for (int y = 0; y < _tilesize; y++) {
                    for (int x = 0; x < _tilesize; x++) {
                        obs[cur_tile][x][y] = new_obs[count];
                        count++;
                    }
                }
            }
        }


    }
}
