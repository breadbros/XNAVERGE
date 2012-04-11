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
        public string name; // a unique name used as a key (generally a file or asset name)
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

        // Pregenerate bounding boxes for each frame. Called once when first building the basis.
        // Outer pad: pixels of padding at the left and top border of the image
        // Inner pad: pixels of padding between adjacent frames of the image
        public void generate_bounding_boxes (int outer_pad, int inner_pad) {
            for (int cur_frame = 0; cur_frame < num_frames; cur_frame++) {
                frame_box[cur_frame] = new Rectangle(
                    outer_pad + (cur_frame % frames_per_row) * (inner_pad + frame_width),
                    outer_pad + (cur_frame / frames_per_row) * (inner_pad + frame_height), 
                    frame_width, frame_height);
            }
        }
    }
    
}
