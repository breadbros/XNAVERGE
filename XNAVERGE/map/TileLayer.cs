using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public class TileLayer : RenderLayer {                
        public int[][] data;
        public int width { get { return _width; } }
        public int height { get { return _height; } }
        protected int _width, _height;
        

        public TileLayer(int w, int h, Vector2 parallax_vector, String layer_name) : base(parallax_vector, layer_name) {
            _width = w;
            _height = h;
            data = new int[w][];
            for (int x = 0; x < w; x++) {
                data[x] = new int[h];                
            }            
        }

        public TileLayer(int w, int h, String layer_name) : this(w, h, VERGEMap.NEUTRAL_PARALLAX, layer_name) { }

        // Indexer, allowing you to access the layer's tile values without going through the "data" member.
        // DO NOT DO THIS FOR ANYTHING PERFORMANCE-CRITICAL. Accessing data via the indexer takes much longer.
        public int[] this[int x] {
            get { return data[x]; }
            set { data[x] = value; }
        }

        // Sets every tile in the layer to the same value. 
        public void set_every_value(int val) {
            int xlen = data.Length;
            int ylen = data[0].Length;
            for (int xc = 0; xc < xlen; xc++) {
                for (int yc = 0; yc < ylen; yc++) {
                    data[xc][yc] = val;
                }
            }
        }
    }

    public enum LayerType { Tile, Entity, Script }

    public class EntityLayer : RenderLayer {
        public EntityLayer() : base(VERGEMap.NEUTRAL_PARALLAX, "Entities") {
            _type = LayerType.Entity;
        }
    }

    public class RenderLayer {        
        public LayerType type { get { return _type; } }
        protected LayerType _type;
        public bool visible;
        public String name;
        public Vector2 parallax;

        public RenderLayer(Vector2 parallax_vector, String layer_name) {
            name = layer_name;
            visible = true;
            parallax = parallax_vector;
        }
    
        public RenderLayer(String layer_name) : this(VERGEMap.NEUTRAL_PARALLAX, layer_name) {}
    }
}
