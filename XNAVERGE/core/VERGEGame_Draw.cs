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
        public BasicEffect basic_effect;

        private static Vector2 NONSENSE_PARALLAX = new Vector2(float.NegativeInfinity, float.NegativeInfinity); // garbage vector used as a sentinel value for parallax

        int old_s = 0, fps = 0;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime); // not sure if this actually does anything
            if (map == null) return;
            
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

            map.renderstack.Draw();            
        }

        // Draws an unfilled pixel-wide rectangle. This is extremely inefficient, so other methods (e.g. DrawPrimitives)
        // should be used if you need to draw a large number of rectangles every frame. However, it should suffice for the 
        // common case that you need to draw a few rectangles per frame for UI purposes.
        protected VertexPositionColor[] rect_vertices = new VertexPositionColor[5];
        public virtual void draw_rect(Rectangle rect, Color color) {
            rect_vertices[0].Position = new Vector3(rect.Left, rect.Top, 0);
            rect_vertices[1].Position = new Vector3(rect.Right - 1, rect.Top, 0);
            rect_vertices[2].Position = new Vector3(rect.Right - 1, rect.Bottom - 1, 0);
            rect_vertices[3].Position = new Vector3(rect.Left, rect.Bottom - 1, 0);
            rect_vertices[0].Color = rect_vertices[1].Color = rect_vertices[2].Color = rect_vertices[3].Color = color;
            rect_vertices[4] = rect_vertices[0];

            //screen.effect.
            //GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, rect_vertices, 0, 4, VertexPositionColor.VertexDeclaration);
        }

        public virtual void draw_background() {
            GraphicsDevice.Clear(Color.Black);
        }

        internal virtual void call_render_hook(ScriptRenderLayer layer, Rectangle clipping_region) {
            if (hook_render != null) hook_render(layer, clipping_region); // this is very wasteful!
        }

    }
}
