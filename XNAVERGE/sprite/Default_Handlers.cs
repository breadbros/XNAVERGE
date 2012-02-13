using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {

    // This class holds default processing functions to be used for customizable XNAVERGE hooks.
    public static class Default_Handlers {
        internal static VERGEGame game;

        public static void Entity_Movescript_Handler(Entity ent) {
            int cur_param, elapsed, adjusted_time;
            float movedist;
            Movestring movestring;
            elapsed = game.tick - ent.last_logic_tick;

            ent.velocity = Vector2.Zero;
            ent.acceleration = Vector2.Zero;
            adjusted_time = ent.speed * elapsed; // hundredths of "virtual" ticks elapsed (accounting for speed)

            if (game.player == ent) { return; }
            movestring = ent.movestring;

            while (adjusted_time > 0) {
                //Console.WriteLine("{0} {1}", ent.index, adjusted_time);
                adjusted_time = movestring.ready(adjusted_time);                
                    
                if (ent.movement_left <= 0) { // If not currently walking
                    cur_param = movestring.parameters[movestring.step];

                    switch (movestring.commands[movestring.step]) {
                        case MovestringCommand.Face:
                            ent.facing = Utility.movestring_face_to_direction(cur_param);
                            movestring.step++;
                            break;
                        case MovestringCommand.Frame:
                            if (cur_param >= 0) ent.set_frame(cur_param);
                            else ent.resume_animation();
                            movestring.step++;
                            break;
                        case MovestringCommand.ToX:
                            if (movestring.tile_movement) cur_param *= game.map.tileset.tilesize;
                            cur_param -= ent.x; // convert to relative position
                            if (cur_param < 0) // Target is to the left of entity
                                set_up_cardinal_movement(ent, Direction.Left, -cur_param, false);
                            else // Target is to the right of entity
                                set_up_cardinal_movement(ent, Direction.Right, cur_param, false);
                            break;
                        case MovestringCommand.ToY:
                            if (movestring.tile_movement) cur_param *= game.map.tileset.tilesize;
                            cur_param -= ent.y; // convert to relative position
                            if (cur_param < 0) // Target is above entity
                                set_up_cardinal_movement(ent, Direction.Up, -cur_param, false);
                            else // Target is below entity
                                set_up_cardinal_movement(ent, Direction.Down, cur_param, false);
                            break;
                        case MovestringCommand.Up:
                            set_up_cardinal_movement(ent, Direction.Up, cur_param, movestring.tile_movement);
                            break;
                        case MovestringCommand.Down:
                            set_up_cardinal_movement(ent, Direction.Down, cur_param, movestring.tile_movement);
                            break;
                        case MovestringCommand.Left:
                            set_up_cardinal_movement(ent, Direction.Left, cur_param, movestring.tile_movement);
                            break;
                        case MovestringCommand.Right:
                            set_up_cardinal_movement(ent, Direction.Right, cur_param, movestring.tile_movement);
                            break;
                    }                

                }

                if (ent.movement_left > 0) {                                                
                    if (ent.movement_left <= adjusted_time) { // Can move farther than is needed in the remaining time                                                    
                        ent.velocity += Utility.velocity_from_direction(ent.movement_direction, ent.movement_left/100f, elapsed);
                        adjusted_time -= ent.movement_left;
                        ent.movement_left = 0;
                        movestring.step++;
                    }
                    else { // Entity will spend the entire period moving                        
                        ent.velocity += Utility.velocity_from_direction(ent.movement_direction, ent.speed/100f, 1);
                        ent.movement_left -= adjusted_time;                            
                        adjusted_time = 0;
                    }
                }
            }
        }

        // A helper function for entity movement. Sets up the entity to go a certain distance in a certain cardinal direction.
        // The distance is either pixels or tiles, depending on the value of in_tiles.
        public static void set_up_cardinal_movement(Entity ent, Direction dir, int distance, bool in_tiles) {
            ent.facing = ent.movement_direction = dir;
            if (distance > 0) {
                ent.movement_left = distance * 100;
                if (in_tiles) ent.movement_left *= game.map.tileset.tilesize;
            }
            else { // distance 0: no movement, just change facing
                ent.movement_left = 0;
                ent.movestring.step++;
            }
        }
    }

}
