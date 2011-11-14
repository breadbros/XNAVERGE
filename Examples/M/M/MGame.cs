using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XNAVERGE;

namespace M
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    public partial class MGame : VERGEGame {
        static void Main(string[] args) {

            (new MGame()).Run();
            
        }

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            base.LoadContent();
            map = new VERGEMap("Content\\raw\\town01.map");
            map.tileset = Content.Load<Tileset>("town tiles");
            y_range = map.height * map.tileset.tilesize + _screen.Height * 2;
            setup_tile_destinations();
            camera = new Camera(map);
            camera.mode = CameraMode.FollowPlayer;

            // ---------------------
            // BEGIN SILLINESS

            player = map.spawn_entity(12, 16, "Content\\raw\\chap.chr");
            Entity e;
            e = map.spawn_entity(21, 14, "Content\\raw\\chap.chr");
            e.set_movestring("L2D2R2U2B");
            e.speed = 80;

            for (int x = 0; false && x < map.width; x++) {
                for (int y = 0; y < map.height; y++) {
                    e = map.spawn_entity(x, y, "Content\\raw\\chap.chr");
                    e.set_movestring("L1D1R1U1B");

                }
            }

            // END SILLINESS
            // ---------------------
        }

        protected override void UnloadContent()
        {
 	        base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }
    }
}
