using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

// TODO: replace this with the type you want to write out.
using TWrite = XVCX.ProcessedVSP;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class VSPWriter : ContentTypeWriter<TWrite> {
        protected override void Write(ContentWriter output, TWrite value) {
            output.Write(value.version);
            output.Write(value.tilesize);
            output.Write(value.num_tiles);
            output.Write(value.num_obs_tiles);
            output.Write(value.num_animations);
            output.Write(value.texture_dim);
            for (int i=0;i<value.tiledata.Length;i++)
                output.Write(value.tiledata[i]);
            output.Write(value.obsdata);
            for (int i = 0; i < value.num_animations; i++) {
                output.Write(value.animations[i].name);
                output.Write(value.animations[i].start);
                output.Write(value.animations[i].end);
                output.Write(value.animations[i].delay);
                output.Write(value.animations[i].mode);
            }

        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return "XNAVERGE.Content.TilesetReader, XNAVERGE";
        }
    }
}
