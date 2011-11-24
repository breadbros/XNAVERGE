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
            VERGEMap.switch_map("town01");
            // ---------------------
            // BEGIN SILLINESS

            player = map.spawn_entity(12, 16, "chap");
            Entity e;
            e = map.spawn_entity(21, 14, "chap");
            e.set_movestring("L2D2R2U2B");
            e.speed = 80;

            for (int x = 0; true && x < map.width; x++) {
                for (int y = 0; y < map.height; y++) {
                    if (rand.NextDouble() < 0.2) {
                        e = map.spawn_entity(x, y, "chap");
                        e.set_movestring("L1D1R1U1B");
                    }
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
