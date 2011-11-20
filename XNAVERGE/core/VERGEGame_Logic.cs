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
        public int tick { get { return _tick; } }
        protected int _tick; // Centiseconds since app start. This will overflow if you leave the game on for 248 days.        

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _tick++;
            input.Update();
            //if (player_controllable && player != null) control_player();
            if (map != null) {
                for (int i = 0; i < map.num_entities; i++) {
                    for (int j = i + 1; j < map.num_entities; j++) {
                        map.entities[i].hitbox.Intersects(map.entities[j].hitbox);
                    }
                    map.entities[i].Update();
                }
            }

            base.Update(gameTime);
        }
    }
}
