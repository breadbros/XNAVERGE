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
        public ScriptBank global;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            BoundedSpace<Entity>.BoundedElementSet ent_enum;            
            Point prev_player_coords, cur_player_coords, facing_pixel;
            Entity ent, old_player;            

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            _tick++;
            input.Update();            

            old_player = player;

            if (old_player != null) { 
                prev_player_coords = player.hitbox.Center;
                prev_player_coords.X /= map.tileset.tilesize;
                prev_player_coords.Y /= map.tileset.tilesize;
            }
            else prev_player_coords = default(Point);

            if (map != null) {

                // HANDLE MOVEMENT AND COLLISIONS
                // ------------------------------

                for (int i = 0; i < map.num_entities; i++) {
                    ent_enum = entity_space.elements_within_bounds(map.entities[i].hitbox, true, map.entities[i]);
                    while (ent_enum.GetNext(out ent))
                    {                        
                        //if (ent != map.entities[i] && ent == player) Console.WriteLine("player hit " + i);
                    }
                    map.entities[i].Update();
                }

                if (player == old_player && player != null) { // update player zone
                    cur_player_coords = player.hitbox.Center;
                    cur_player_coords.X /= map.tileset.tilesize;
                    cur_player_coords.Y /= map.tileset.tilesize;
                    if (cur_player_coords != prev_player_coords && map.within_bounds(cur_player_coords.X, cur_player_coords.Y, true)) {
                        map.zones[map.zone_layer.data[cur_player_coords.X][cur_player_coords.Y]].maybe_activate(cur_player_coords.X, cur_player_coords.Y);
                    }
                }

                // HANDLE ENTITY/ZONE ACTIVATION VIA BUTTON PRESS                
                if (player != null && player_controllable && action.confirm.pressed) {                    
                    facing_pixel = player.facing_coordinates(false);
                    ent_enum = entity_space.elements_within_bounds(new Rectangle(facing_pixel.X, facing_pixel.Y, 1, 1), true, player);
                    if (ent_enum.GetNext(out ent)) // just take the first match arbitrarily
                        ent.activate();
                }

            }

            base.Update(gameTime);
        }

        // Tries to find a delegate matching the specified type in either the map or global script banks.
        // Checks map scripts first, then global scripts. Returns null if T is not a delegate or if the
        // script does not exist. Throws AmbiguousMatchException if the script is overloaded within a
        // single script bank.
        // TODO: Maybe allow for multiple global script banks?
        public virtual T script<T>(String name) where T : class {
            if (String.IsNullOrEmpty(name)) return null;
            T script = VERGEGame.game.map.scripts.get_script<T>(name);
            if (script == null) return global.get_script<T>(name);
            return script;
        }

    }    
}
