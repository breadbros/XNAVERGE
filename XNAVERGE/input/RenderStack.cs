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

        public RenderStack(VERGEMap vmap, String rstring, Char delim) {
            int cur_pos, next_pos, len, layer_number;
            String str = rstring.Trim().ToUpper();            
            String cur_token;
            RenderLayer cur_layer;
            Queue<RenderLayer> layer_queue = new Queue<RenderLayer>(); // Temporary loading queue
            map = vmap;
                
            cur_pos = 0;
            len = str.Length;
            while (cur_pos < len) {
                next_pos = str.IndexOf(delim, cur_pos);
                if (next_pos == -1) next_pos = len;
                cur_token = str.Substring(cur_pos, next_pos - cur_pos).Trim();
                //Console.WriteLine(cur_token);
                switch (cur_token) {
                    case "R": // rendering script layer (defaults to hook_render and fixed parallax)
                        cur_layer = new ScriptRenderLayer();
                        layer_queue.Enqueue(cur_layer);
                        break;
                    case "E": // entity layer
                        cur_layer = new EntityLayer();
                        layer_queue.Enqueue(cur_layer);
                        break;
                    default: // tile layer
                        try {
                            layer_number = Int32.Parse(cur_token);
                            if (layer_number <= 0) throw new Exception();                            
                        }
                        catch (Exception) { throw new MalformedRenderstringException(rstring); } // not a positive integer                        
                        cur_layer = map.tiles[layer_number - 1];
                        layer_queue.Enqueue(cur_layer);
                        break;                        
                }
                cur_pos = next_pos + 1;
            }

            // Collections are slow, so we'll shift the layers into a fixed array now that we've got them all.
            list = new RenderLayer[layer_queue.Count];
            for (int count = 0; count < list.Length; count++) {
                list[count] = layer_queue.Dequeue();
            }
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
