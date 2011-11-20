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
        public float y_range; // the total vertical distance in which sprites are drawable, centred on the middle of the map (necessary for technical reasons)

        private static Vector2 NONSENSE_PARALLAX = new Vector2(float.NegativeInfinity, float.NegativeInfinity); // garbage vector used as a sentinel value for parallax

        int old_s, fps;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            int min_tile_x, min_tile_y, tiles_to_draw_x, tiles_to_draw_y, offset_x, offset_y, parallaxed_camera_x, parallaxed_camera_y;
            min_tile_x = min_tile_y = tiles_to_draw_x = tiles_to_draw_y = offset_x = offset_y = parallaxed_camera_x = parallaxed_camera_y = 0;
            Rectangle draw_rect;
            if (map == null) return;            

            draw_background();

            int tilesize = map.tileset.tilesize;
            int mapwidth = map.width;
            int mapheight = map.height;     

            camera.update();
            draw_rect = camera.rect; 

            // We need a range of valid y-values for sprites, since XNA does its sorting using floats between 0 and 1. Sprites can still be visible
            // when they're off the edge of the map, so to be sure we catch them all, we'll define the drawable sprites as those within a screen's
            // length of the top/bottom of the map.
            // This is totally arbitrary, but it's fairly generous; if you need more than that, what on earth are you doing?

            if (gameTime.TotalGameTime.Seconds != old_s) {
                Window.Title = fps.ToString();
                fps = 0;
            }
            fps++;
            old_s = gameTime.TotalGameTime.Seconds;

            Vector2 current_parallax = NONSENSE_PARALLAX; // initialize to impossible value to ensure things get set on first iteration

            Matrix neutral_parallax_transform = offset_matrix(camera.x, camera.y, tilesize) * screen.scaling_matrix; // this is the most common case so let's save it

            // Update entity frames
            for (int i = 0; i < map.num_entities; i++) { map.entities[i].advance_frame(); }

            foreach (RenderLayer rl in map.renderstack.list) {
                if (rl.visible) {
                    if (rl.type == LayerType.Tile) {
                        if (rl.parallax != current_parallax) {
                            if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();
                            current_parallax = rl.parallax;
                            if (current_parallax == VERGEMap.NEUTRAL_PARALLAX) {
                                parallaxed_camera_x = camera.x;
                                parallaxed_camera_y = camera.y;
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, neutral_parallax_transform);
                            }
                            else if (current_parallax == VERGEMap.FIXED_PARALLAX) {
                                parallaxed_camera_x = 0;
                                parallaxed_camera_y = 0;
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, screen.scaling_matrix);
                            }
                            else {
                                parallaxed_camera_x = (int)(camera.x * current_parallax.X);
                                parallaxed_camera_y = (int)(camera.y * current_parallax.Y);
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, offset_matrix(parallaxed_camera_x, parallaxed_camera_y, tilesize) * screen.scaling_matrix);
                            }
                            min_tile_x = parallaxed_camera_x / tilesize;
                            min_tile_y = parallaxed_camera_y / tilesize;
                            tiles_to_draw_x = screen.width / tilesize + 1;
                            tiles_to_draw_y = screen.height / tilesize + 1;
                        }
                        for (int x = Math.Max(0, -min_tile_x); x < Math.Min(tiles_to_draw_x, mapwidth - min_tile_x); x++) {
                            for (int y = Math.Max(0, -min_tile_y); y < Math.Min(tiles_to_draw_y, mapheight - min_tile_y); y++) {
                                spritebatch.Draw(map.tileset.image, dest_rect[x][y], map.tileset.tile_frame[((TileLayer)rl).data[min_tile_x + x][min_tile_y + y]], Color.White);
                            }
                        }
                    }
                    else if (rl.type == LayerType.Entity && map.num_entities > 0) {
                        if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();
                        current_parallax = NONSENSE_PARALLAX; // entity layer ignores parallax. this ensures any tile layers above this will be aligned properly.                        
                        spritebatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-camera.x, -camera.y, 0) * screen.scaling_matrix);
                        Entity ent;
                        for (int i = 0; i < map.num_entities; i++) {
                            ent = map.entities[i];
                            if (!ent.deleted && ent.visible && ent.destination.Intersects(draw_rect)) {
                                ent.Draw();
                            }
                        }
                        spritebatch.End();
                    }
                }
            }

            if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();

            base.Draw(gameTime);
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
