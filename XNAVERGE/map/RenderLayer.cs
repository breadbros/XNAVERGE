using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public class TileLayer : RenderLayer {
        public int[][] data;
        public int width { get { return _width; } } // dimensions (in tiles)
        public int height { get { return _height; } }
        protected int _width, _height;
        public double alpha {
            get { return ((float)blending.BlendFactor.A) / 255; }
            set {                
                if (value < 0.0 || value > 1.0) throw new ArgumentOutOfRangeException();
                blend_color.R = blend_color.G = blend_color.B = blend_color.A = (byte)(value * 0xFF);
            }
        }
        protected Color blend_color;

        public TileLayer(int w, int h, Vector2 parallax_vector, String layer_name) : base(parallax_vector, layer_name, BlendState.AlphaBlend) {
            _width = w;
            _height = h;
            data = new int[w][];
            blend_color = Color.White; // copied by value
            alpha = 1.0;
            for (int x = 0; x < w; x++) {
                data[x] = new int[h];
            }                        
        }
        public TileLayer(int w, int h, String layer_name) : this(w, h, VERGEMap.NEUTRAL_PARALLAX, layer_name) { }

        // Indexer, allowing you to access the layer's tile values without going through the "data" member.
        // DO NOT DO THIS FOR ANYTHING PERFORMANCE-CRITICAL. Accessing data via the indexer takes much longer.
        public int[] this[int x] {
            get { return data[x]; }
            set { 
                data[x] = value; 
            }
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

        public override void Draw() {
            SpriteBatch spritebatch = VERGEGame.game.spritebatch;                     
            
            int min_x, min_y, tiles_per_row, tiles_per_column;
            Tileset tileset = VERGEGame.game.map.tileset;
            Camera camera = VERGEGame.game.camera;
            Screen screen = VERGEGame.game.screen;
            Rectangle dest_rect = default(Rectangle);            

            dest_rect.Width = dest_rect.Height = tileset.tilesize;
            Vector2 offset = new Vector2(camera.x * parallax.X, camera.y * parallax.Y); // parallax-adjusted camera coordinates for this layer            
            min_x = Math.Max(0, (int)(offset.X/tileset.tilesize)); // round down to integer
            min_y = Math.Max(0, (int)(offset.Y/tileset.tilesize));
            
            tiles_per_row = Math.Min(VERGEGame.game.map.width - min_x, (screen.width + tileset.tilesize - 1) / tileset.tilesize + 1);
            tiles_per_column = Math.Min(VERGEGame.game.map.height - min_y, (screen.height + tileset.tilesize - 1) / tileset.tilesize + 1);
            
            spritebatch.Begin(SpriteSortMode.Deferred, blending, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-offset.X, -offset.Y, 0.0f));

            dest_rect.Y = min_y * dest_rect.Height;
            for (int y = 0; y < tiles_per_column; y++) {
                dest_rect.X = min_x * dest_rect.Width;
                for (int x = 0; x < tiles_per_row; x++) {                    
                    spritebatch.Draw(tileset.image, dest_rect, tileset.tile_frame[data[min_x + x][min_y + y]], blend_color);
                    dest_rect.X += dest_rect.Width;
                }
                dest_rect.Y += dest_rect.Height;
            }

            spritebatch.End();
        }
    }

    public class ScriptRenderLayer : RenderLayer {
        public RenderLayerDelegate script;
        public Object data; // miscellaneous data storage, in case you need state        

        public ScriptRenderLayer() : base(VERGEMap.FIXED_PARALLAX, "Hook Render") {
            script = new RenderLayerDelegate(VERGEGame.game.call_render_hook);
            data = null;
        }
        public ScriptRenderLayer(RenderLayerDelegate script, String name) : this(script, VERGEMap.FIXED_PARALLAX, name) { }
        public ScriptRenderLayer(RenderLayerDelegate script, bool fixed_pos, String name) : 
            this(script, ( fixed_pos ? VERGEMap.FIXED_PARALLAX : VERGEMap.NEUTRAL_PARALLAX ), name ) { }
        public ScriptRenderLayer(RenderLayerDelegate script, Vector2 parallax, String name) : base(parallax, name) {
            this.script = script;
            data = null;
        }

        public override void Draw() {            
            if (script == null) return;

            Rectangle clipping_rect = VERGEGame.game.camera.rect; // copied by value
            if (parallax == VERGEMap.FIXED_PARALLAX) {
                clipping_rect.X = 0;
                clipping_rect.Y = 0;
            }
            else if (parallax != VERGEMap.NEUTRAL_PARALLAX) {
                clipping_rect.X = (int)(clipping_rect.X * parallax.X);
                clipping_rect.Y = (int)(clipping_rect.Y * parallax.Y);
            }

            script(this, clipping_rect);
        }

    }

    public enum LayerType { Tile, Entity, Script }

    public class EntityLayer : RenderLayer {
        public EntityLayer() : base(VERGEMap.NEUTRAL_PARALLAX, "Entities") {
            _type = LayerType.Entity;
        }

        public override void Draw() { // disregards parallax value (always assumes parallax of (1.0, 1.0))
            SpriteBatch spritebatch = VERGEGame.game.spritebatch;
            Rectangle draw_rect = VERGEGame.game.camera.rect;
            Entity cur_ent;
            Entity[] ents = VERGEGame.game.map.entities;
            int num_ents = VERGEGame.game.map.num_entities;

            spritebatch.Begin(SpriteSortMode.FrontToBack, blending, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-VERGEGame.game.camera.x, -VERGEGame.game.camera.y, 0.0f));

            for (int i = 0; i < num_ents; i++) {
                cur_ent = ents[i];
                if (!cur_ent.deleted && cur_ent.visible && cur_ent.destination.Intersects(draw_rect))
                    cur_ent.Draw();
            }

            spritebatch.End();
        }
    }

    public abstract class RenderLayer {        
        public LayerType type { get { return _type; } }
        protected LayerType _type;
        public bool visible;
        public String name;
        public Vector2 parallax;
        
        public BlendState blending;
        
        public RenderLayer(Vector2 parallax_vector, String layer_name) : this(parallax_vector, layer_name, BlendState.AlphaBlend) { }
        public RenderLayer(Vector2 parallax_vector, String layer_name, BlendState blend_state) {
            name = layer_name;
            visible = true;
            parallax = parallax_vector;
            blending = blend_state;
        }
    
        public RenderLayer(String layer_name) : this(VERGEMap.NEUTRAL_PARALLAX, layer_name) {}

        public abstract void Draw();
    }


}
