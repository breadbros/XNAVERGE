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
        //internal float x_range, y_range; // the total vertical distance in which sprites are drawable, centred on the middle of the map (necessary for technical reasons)

        private static Vector2 NONSENSE_PARALLAX = new Vector2(float.NegativeInfinity, float.NegativeInfinity); // garbage vector used as a sentinel value for parallax

        int old_s, fps;
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
            }
            fps++;
            old_s = gameTime.TotalGameTime.Seconds;

            // Update entity frames
            for (int i = 0; i < map.num_entities; i++) { map.entities[i].advance_frame(); }

            map.renderstack.Draw();            
        }

        public virtual void draw_background() {
            GraphicsDevice.Clear(Color.Black);
        }

    }
}
