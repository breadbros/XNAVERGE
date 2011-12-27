using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace XNAVERGE {
    public partial class VERGEGame {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spritebatch;
        public Camera camera;
        public Screen screen;
        protected RenderLayerDelegate hook_render;
        public SpriteFont system_font;

        private static Vector2 NONSENSE_PARALLAX = new Vector2(float.NegativeInfinity, float.NegativeInfinity); // garbage vector used as a sentinel value for parallax

        int old_s = 0, fps = 0;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {            
            base.Draw(gameTime); // not sure if this actually does anything            
            if (map == null) return;

            Rectangle blit_rect = new Rectangle(0, 0, screen.width, screen.height);
            
            draw_background();
            camera.update();            

            if (gameTime.TotalGameTime.Seconds != old_s) {
                Window.Title = fps.ToString();
                fps = 0;
                old_s = gameTime.TotalGameTime.Seconds;
            }
            fps++;            

            // Update entity frames
            for (int i = 0; i < map.num_entities; i++) { map.entities[i].advance_frame(); }


            // Draw to native-size buffer at 1x size
            GraphicsDevice.SetRenderTarget(screen.true_size_buffer);
            map.renderstack.Draw(); 
            
            // Copy native buffer to backbuffer, scaled to true window size
            GraphicsDevice.SetRenderTarget(null);
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, screen.scaling_matrix);
            spritebatch.Draw(screen.true_size_buffer, blit_rect, Color.White);
            spritebatch.End();

        }

        
        // Draws an unfilled pixel-wide rectangle. This is extremely inefficient, so other methods (e.g. DrawPrimitives)
        // should be used if you need to draw a large number of rectangles every frame. However, it should suffice for the 
        // common case that you need to draw a few rectangles per frame for UI purposes.
        protected VertexPositionColor[] rect_vertices = new VertexPositionColor[5];
        public virtual void draw_rect(Rectangle rect, Color color) {
            Rectangle blit_rect = new Rectangle(0,0,screen.width,screen.height);
            rect_vertices[0].Position = new Vector3(rect.Left, rect.Top, 0);
            rect_vertices[1].Position = new Vector3(rect.Right - 1, rect.Top, 0);
            rect_vertices[2].Position = new Vector3(rect.Right - 1, rect.Bottom - 1, 0);
            rect_vertices[3].Position = new Vector3(rect.Left, rect.Bottom - 1, 0);
            rect_vertices[0].Color = rect_vertices[1].Color = rect_vertices[2].Color = rect_vertices[3].Color = color;
            rect_vertices[4] = rect_vertices[0];
            
            foreach (EffectPass pass in screen.effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, rect_vertices, 0, 4, VertexPositionColor.VertexDeclaration);                
            }                        
        }

        public virtual void draw_background() {
            GraphicsDevice.Clear(Color.Black);
        }

        public virtual void print_string(String str, int x, int y, SpriteFont font, Color color, bool new_batch) {
            if (new_batch) spritebatch.Begin();
            spritebatch.DrawString(font, str, new Vector2(x,y),color);
            if (new_batch) spritebatch.End();
        }
        public virtual void print_string(String str, int x, int y, Color color, bool new_batch) { 
            verify_system_font();
            print_string(str, x, y, system_font, color, new_batch);
        }

        public virtual void print_right(String str, int x, int y, SpriteFont font, Color color, bool new_batch) { 
            print_string(str, x - ((int)font.MeasureString(str).X), y, font, color, new_batch);
        }
        public virtual void print_right(String str, int x, int y, Color color, bool new_batch) {
            verify_system_font();
            print_right(str, x, y, system_font, color, new_batch);
        }

        public virtual void print_center(String str, int y, SpriteFont font, Color color, bool new_batch) {
            print_string(str, (screen.width - (int)font.MeasureString(str).X)/2, y, font, color, new_batch);
        }
        public virtual void print_center(String str, int y, Color color, bool new_batch) {
            verify_system_font();
            print_center(str, y, system_font, color, new_batch);
        }

        // Throws an exception if the system font has not been set.
        protected virtual void verify_system_font() {
            if (system_font == null) throw new Exception("System font is not set. You must add a SpriteFont to your game's content project and load it into system_font before you can use this functionality.");
        }

        internal virtual void call_render_hook(ScriptRenderLayer layer, Rectangle clipping_region) {
            if (hook_render != null) hook_render(layer, clipping_region); // this is very wasteful!
        }
        
    }
}
