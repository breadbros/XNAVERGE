using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using XNAVERGE;

using TWrite = XVCX.ProcessedSprite;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class CHRWriter : ContentTypeWriter<TWrite> {        
        protected override void Write(ContentWriter output, TWrite value) {           
            output.Write(value.fw);
            output.Write(value.fh);
            output.Write(value.frames);
            output.Write(value.per_row);
            output.Write(value.hitbox.X);
            output.Write(value.hitbox.Y);
            output.Write(value.hitbox.Width);
            output.Write(value.hitbox.Height);
            output.Write(value.texture_dim);            
            for (int i = 0; i < value.pixels.Length; i++)
                output.Write(value.pixels[i]);
            output.Write(value.num_anim);
            for (int i = 0; i < value.num_anim; i++)
                write_sprite_anim(output, value.anim[i]);
        }

        private void write_sprite_anim(ContentWriter output, SpriteAnimation anim) {
            output.Write(anim.name);            
            output.Write(anim.pattern);
            output.Write((int)anim.style);
            output.Write(""); // animation to transition to after this animation ends, when using AnimationStyle.Transition. Never exists for a CHR.
            output.Write(anim.length);
            for (int i=0; i<anim.length; i++) 
                output.Write(anim.frame[i]);
            for (int i=0; i<anim.length; i++) 
                output.Write(anim.delay[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return "XNAVERGE.Content.SpriteReader, XNAVERGE";
        }
    }
}
