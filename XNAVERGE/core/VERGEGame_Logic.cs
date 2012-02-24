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
        public static int tick_length { // tick length, in milliseconds. Can be altered during play to speed up/slow down the engine.
            get { return _tick_length_in_timespan_ticks; }
            set { _tick_length_in_timespan_ticks = value * 10000; } // 10000 timespan "ticks" in a millisecond
        }
        internal static int _tick_length_in_timespan_ticks; // tick length, in timespan "ticks", which are each 100 nanoseconds
        public int tick { get { return _tick; } }
        protected int _tick;
        protected long _last_tick_time; // Time of the last tick, measured in System.TimeSpan's "ticks" (each 100 nanoseconds)        
        public ScriptBank global;

        public const bool PLAYER_CONTROLLABLE_DEFAULT = true;
        public bool player_controllable; // true if player responds to input
        public Stack<bool> player_controllable_stack; // a stack of previous player_controllable states

        public EntityMovementDelegate default_entity_handler; // the movement handler assigned to new entities

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            BoundedSpace<Entity>.BoundedElementSet ent_enum;            
            Point prev_player_coords, cur_player_coords, facing_coords;
            Entity ent, old_player;
            int elapsed;

            // Back button instantly quits. TODO: take this out when it transitions from convenient to obnoxious.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();            
            elapsed = (int)(gameTime.TotalGameTime.Ticks - _last_tick_time); // won't overflow unless Updates are more than 2 seconds apart            
            if (elapsed >= _tick_length_in_timespan_ticks) {
                // IF TICKS HAVE ELAPSED, DO THE ACTUAL UPDATING
                // okay, you don't need to yell
                
                elapsed /= _tick_length_in_timespan_ticks; // convert elapsed from ms to ticks, rounding down
                _tick += elapsed;
                _last_tick_time += elapsed * _tick_length_in_timespan_ticks;                

                if (map != null) {
                    input.Update();                    

                    // HANDLE MOVEMENT AND COLLISIONS
                    // ------------------------------

                    if (player != null) {
                        prev_player_coords = player.hitbox.Center;
                        prev_player_coords.X /= map.tileset.tilesize;
                        prev_player_coords.Y /= map.tileset.tilesize;
                    }
                    else prev_player_coords = default(Point);
                    for (int i = 0; i < map.num_entities; i++) {
                        ent = map.entities[i];                                                
                        ent.Update();
                    }
                    
                    /*

                    for (int i = 0; i < map.num_entities; i++) {
                        ent_enum = entity_space.elements_within_bounds(map.entities[i].hitbox, true, map.entities[i]);
                        while (ent_enum.GetNext(out ent)) {
                            //if (ent != map.entities[i] && ent == player) Console.WriteLine("player hit " + i);
                        }
                        map.entities[i].Update();
                    }
                    */
                    if (player != null) { // update player zone
                        cur_player_coords = player.hitbox.Center;
                        cur_player_coords.X /= map.tileset.tilesize;
                        cur_player_coords.Y /= map.tileset.tilesize;
                        if (cur_player_coords != prev_player_coords && map.within_bounds(cur_player_coords.X, cur_player_coords.Y, true)) {
                            map.zones[map.zone_layer.data[cur_player_coords.X][cur_player_coords.Y]].maybe_activate(cur_player_coords.X, cur_player_coords.Y);
                        }

                        // HANDLE ENTITY/ZONE ACTIVATION VIA BUTTON PRESS                
                        if (player_controllable && action.confirm.pressed) {
                            facing_coords = player.facing_coordinates(false);

                            // Entity activation
                            ent_enum = entity_space.elements_within_bounds(new Rectangle(facing_coords.X, facing_coords.Y, 1, 1), true, player);
                            if (ent_enum.GetNext(out ent)) // just take the first match arbitrarily
                                ent.activate();

                            // Zone activation
                            // Convert facing_coords to tile coordinates
                            facing_coords.X /= map.tileset.tilesize;
                            facing_coords.Y /= map.tileset.tilesize;
                            if (facing_coords != cur_player_coords && map.within_bounds(facing_coords.X, facing_coords.Y, true)
                                && map.zone_at(facing_coords.X, facing_coords.Y, true).adjacent)
                                map.zone_at(facing_coords.X, facing_coords.Y, true).activate(facing_coords.X, facing_coords.Y, true);
                        }
                    }
                    
                    // END OF UPDATING
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

        // Suppresses player input. This sets player_controllable to false, but unlike doing that 
        // directly, it also saves the previous state of that variable in the player_controllable_stack.
        // That previous state is returned by lock_player.
        public virtual bool lock_player() {
            bool previous = player_controllable;
            player_controllable_stack.Push(previous);
            player_controllable = false;
            return previous;
        }

        // Restores player_controllable to the state it was in when lock_player was last called. 
        // This pops the top player_controllable value off the stack into player_controllable, 
        // and returns that value for good measure. If the stack is empty, it sets the value to
        // match PLAYER_CONTROLLABLE_DEFAULT.
        public virtual bool unlock_player() {
            if (player_controllable_stack.Count > 0) player_controllable = player_controllable_stack.Pop();
            else player_controllable = PLAYER_CONTROLLABLE_DEFAULT;
            return player_controllable;
        }

    }    
}
