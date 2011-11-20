using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // This class contains all sprite data that stays constant across different instances of the same sprite.
    // The constructor, which loads a new sprite from a .CHR file, is in SpriteBasis_Loader.vc.
    public partial class SpriteBasis {
        public Texture2D image;
        public int num_frames { get { return _num_frames; } }
        public int frame_width { get { return _frame_width; } }
        public int frame_height { get { return _frame_height; } }
        public int frames_per_row { get { return _per_row; } }        
        protected int _num_frames, _frame_height, _frame_width, _per_row;
        public Rectangle default_hitbox;
        public Rectangle[] frame_box;
        public Dictionary<String, SpriteAnimation> animations;

        public SpriteBasis(int frame_w, int frame_h, int frames, int frames_per_row) {
            _num_frames = frames;
            _frame_width = frame_w;
            _frame_height = frame_h;
            _per_row = frames_per_row;
            frame_box = new Rectangle[_num_frames];
            animations = new Dictionary<String,SpriteAnimation>();
        }
    }
    
}
