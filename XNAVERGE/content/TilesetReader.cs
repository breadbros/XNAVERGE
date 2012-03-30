using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// TODO: replace this with the type you want to read.
using TRead = XNAVERGE.Tileset;

namespace XNAVERGE.Content {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class TilesetReader : ContentTypeReader<TRead> {
        protected override TRead Read(ContentReader input, TRead nobody_seems_to_know_what_this_argument_is_for) {
            int ver, tilesize, numtiles, numobs, numanim, cur_int;
            byte[] pixels;
            Texture2D image;
            Tileset tileset;

            ver = input.ReadInt32();
            tilesize = input.ReadInt32();
            numtiles = input.ReadInt32();
            numobs = input.ReadInt32();
            numanim = input.ReadInt32();

            cur_int = input.ReadInt32(); // texture dimensions (it's square, so one side will do)            
            image = new Texture2D(VERGEGame.game.GraphicsDevice, cur_int, cur_int);
            pixels = input.ReadBytes(cur_int * cur_int * 4);
            image.SetData(pixels);

            tileset = new Tileset(tilesize, numtiles, image, numobs, input.ReadBytes(tilesize * tilesize * numobs), numanim);
            tileset.version = ver;

            for (int i = 0; i < numanim; i++)
                tileset.add_animation(input.ReadString(), (TilesetAnimationMode)input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());

            return tileset;
        }
    }
}
