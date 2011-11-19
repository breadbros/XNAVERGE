using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = XNAVERGE.SpriteBasis;

namespace XNAVERGE.Content {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class SpriteReader : ContentTypeReader<TRead> {
        protected override TRead Read(ContentReader input, TRead nobody_seems_to_know_what_this_argument_is_for) {
            int dim, num_anim;
            uint[] pixels;
            SpriteBasis spr = new SpriteBasis(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            spr.default_hitbox = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            dim = input.ReadInt32();
            Texture2D image = new Texture2D(VERGEGame.game.GraphicsDevice, dim, dim);
            pixels = new uint[dim * dim];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = input.ReadUInt32();
            image.SetData<uint>(pixels);
            spr.image = image;

            // Pregenerate bounding boxes for each frame
            for (int cur_frame=0; cur_frame < spr.num_frames; cur_frame++) {
                spr.frame_box[cur_frame] = new Rectangle((cur_frame % spr.frames_per_row) * spr.frame_width, (cur_frame / spr.frames_per_row) * spr.frame_height, spr.frame_width, spr.frame_height);
            }

            // load animations
            num_anim = input.ReadInt32();            
            for (int i = 0; i < num_anim; i++)
                read_animation(input, spr.animations);

            return spr;
        }

        public void read_animation(ContentReader input, Dictionary<String, SpriteAnimation> dict) {
            String name, pattern, transition;
            int style, len;
            int[] frame, delay;
            SpriteAnimation anim;
            name = input.ReadString();
            pattern = input.ReadString();
            style = input.ReadInt32();
            transition = input.ReadString(); // TODO: figure out how to set this up, I guess (maybe better to keep it as a string?)
            len = input.ReadInt32();
            frame = new int[len];
            delay = new int[len];
            for (int i=0;i<len;i++)
                frame[i] = input.ReadInt32();
            for (int i=0;i<len;i++)
                delay[i] = input.ReadInt32();
            anim = new SpriteAnimation(name, pattern, frame, delay, (AnimationStyle)style);
            dict.Add(name, anim);
        }
    }
}
