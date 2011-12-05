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
        protected Rectangle[][] dest_rect;                
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

        // this needs to be called whenever the resolution or tile size changes.
        // TODO: Make it actually accomodate different resolutions
        public void setup_tile_destinations() {
            dest_rect = new Rectangle[map.width][];
            int tilesize = map.tileset.tilesize;
            for (int x = 0; x < screen.width / tilesize + 1; x++) {
                dest_rect[x] = new Rectangle[map.height];
                for (int y = 0; y < screen.height / tilesize + 1; y++) {
                    dest_rect[x][y] = new Rectangle(x * tilesize, y * tilesize, tilesize, tilesize);
                }
            }
        }

        // Returns a translation matrix representing the number of pixels the tile boundary is to the left of and above
        // the screen edges. This is used internally for determining where to begin drawing the tiles, since the
        // leftmost/topmost tiles will often be partially off-screen.
        private Matrix offset_matrix(int upper_left_x, int upper_left_y, int tilesize) {
            return Matrix.CreateTranslation(-(upper_left_x % tilesize), -(upper_left_y % tilesize), 0);
        }

    }
}
