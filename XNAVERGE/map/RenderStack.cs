using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public class RenderStack {
        private VERGEMap map;
        public RenderLayer[] list;

        public RenderStack(VERGEMap vmap, String rstring) : this(vmap, rstring, ',') {} // MAP file renderstrings are always comma-delimited

        private static RenderLayer MapToken (VERGEMap vergeMap, string token) {
            switch (token) {
                case "R": // rendering script layer (defaults to hook_render and fixed parallax)
                    return new ScriptRenderLayer();
                case "E": // entity layer
                    return new EntityLayer();
                default: // tile layer
                    int layer_number;
                    if (Int32.TryParse(token, out layer_number) && (layer_number > 0))
                        return vergeMap.tiles[layer_number - 1];
                    else
                        throw new MalformedRenderstringException("Invalid token '" + token + "' in renderstring");
            }
        }

        public RenderStack(VERGEMap vmap, String rstring, Char delim) {
            var tokens = rstring.Trim().ToUpper().Split(delim);
            list = (
                from token in tokens
                select MapToken(vmap, token.Trim())
            ).ToArray();                
        }

        // sets all layers in the stack to visible
        public void show_all() {
            foreach (RenderLayer layer in list)
                if (layer.type == LayerType.Tile) {
                    layer.visible = true;
                }
        }

        public void Draw() {
            for( int i = 0; i < list.Length; i++ ) {
                if( list[i].visible ) {

                    if( i == 0 ) {
                        list[i].DrawBaseLayer();
                    } else {
                        list[i].Draw();
                    }
                }
            }
        }
    }

    public class MalformedRenderstringException : Exception {
        public MalformedRenderstringException(String rstring) : base("\"" + rstring + "\" is not a valid renderstring.") {}
    }
}
