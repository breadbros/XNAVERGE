using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        public TileAnimation[] animations;

        public Texture2D image;

        public Tileset(int new_tilesize, int new_num_tiles, Texture2D tile_texture_atlas, int new_num_obs_tiles, byte[] obsdata, int num_animations) {
            set_tile_data(new_num_tiles, new_tilesize, tile_texture_atlas);
            set_obs_data(new_num_obs_tiles, obsdata);
            _num_animations = 0;
            animations = new TileAnimation[Math.Min(1,num_animations)];
        }

        public void add_animation(string name, TilesetAnimationMode mode, int start, int end, int delay) {
            if (start < 0) throw new ArgumentOutOfRangeException("start", "Error in Tileset.add_animation: The animation's starting index was specified as " + start + ".");
            else if (end < 0) throw new ArgumentOutOfRangeException("end", "Error in Tileset.add_animation: The animation's ending index (" + end + ") is less than its starting index (" + start + ").");
            if (delay < 0) throw new ArgumentOutOfRangeException("delay", "Error in Tileset.add_animation: The animation's frame delay was specified as " + delay + ".");
            
            if (_num_animations >= animations.Length) { // Outgrown the original array. Copy to a larger one.
                TileAnimation[] new_array = new TileAnimation[animations.Length * 2];
                animations.CopyTo(new_array, 0);
                animations = new_array;
            }
            animations[_num_animations] = new TileAnimation(name, mode, start, end, delay);
            _num_animations++;
        }

        protected void set_tile_data(int new_num_tiles, int new_tilesize, Texture2D new_image) {
            int x=0, y=0;
            image = new_image;
            _num_tiles = new_num_tiles;
            tile_frame = new Rectangle[new_num_tiles];
            _tilesize = new_tilesize;
            _per_row = new_image.Width / new_tilesize;
            for (int cur_tile = 0; cur_tile < new_num_tiles; cur_tile++) { // set up source rects for use in tile blitting                
                tile_frame[cur_tile] = new Rectangle(x, y, new_tilesize, new_tilesize);
                x += new_tilesize;
                if (x > new_image.Width - new_tilesize) {
                    x = 0;
                    y += new_tilesize;
                }
            }
        }
        
        protected void set_obs_data(int new_num_obs, byte[] new_obs) {
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
        protected void set_obs_data(int new_num_obs, bool[] new_obs) {
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

    
    // Defines a VERGE-style tile animation, which must have a fixed framerate and run through a contiguous sequence of tiles in the tile atlas.
    // An animation with a delay of 0 is ignored by the system.
    public struct TileAnimation {
        public string name;
        public TilesetAnimationMode mode;
        public int delay, start, end; // these aren't validated because I'm lazy
        public TileAnimation(string name, TilesetAnimationMode mode, int start, int end, int delay) {
            this.name = name;
            this.mode = mode;
            this.start = start;
            this.end = end;
            this.delay = delay;
        }
    }

    public enum TilesetAnimationMode { Forward, Reverse, Random, BackAndForth }
}
