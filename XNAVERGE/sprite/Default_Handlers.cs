﻿using System;
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

        // A general purpose VERGE-emulation handler that defers to various other handlers depending on
        // the entity's state.
        public static int omnibus_vergestyle_handler(Entity ent, ref EntityMovementData data) {
            if (ent == VERGEGame.game.player && VERGEGame.game.player_controllable)
                return vergestyle_player_movement_handler(ent, ref data);
            else
                return entity_movescript_handler(ent, ref data);
        }

        // Moves the entity in accordance with the player's inputs. This works like VERGE player input
        // EXCEPT that, when diagonals are enabled, it moves the player at the same speed overall 
        // (unlike VERGE, which moved them both vertically and horizontally as fast as if they were 
        //  moving in that direction alone)
        public static int vergestyle_player_movement_handler(Entity ent, ref EntityMovementData data) {
            int x = 0, y = 0;
            float factor;
            ent.velocity = ent.acceleration = Vector2.Zero;
            ent.acceleration = Vector2.Zero;
            if (data.first_call) {
                if (VERGEGame.game.dir.left.down) x--;
                if (VERGEGame.game.dir.right.down) x++;
                if (VERGEGame.game.dir.up.down) y--;
                if (VERGEGame.game.dir.down.down) y++;

                if (x == 0 && y == 0) {
                    if (ent.moving) ent.set_walk_state(false);
                }
                else {
                    ent.velocity.X = (float)x;
                    ent.velocity.Y = (float)y;
                    if (!ent.moving) ent.set_walk_state(true);
                    ent.facing = Utility.direction_from_signs(x, y, false);
                    ent.movement_direction = Utility.direction_from_signs(x, y, true);
                    factor = ent.speed / 100f;
                    if (Math.Abs(x) + Math.Abs(y) == 2) factor *= Utility.INV_SQRT2; // diagonal movement
                    ent.velocity *= factor;
                }                
            }
            else if (data.collided) {
                // sliding goes here                
            }
            return 0;
        }

        public static int entity_movescript_handler(Entity ent, ref EntityMovementData data) {
            int cur_param, time_left;
            Movestring movestring;            

            ent.velocity = Vector2.Zero;
            ent.acceleration = Vector2.Zero;
            time_left = data.time;
            movestring = ent.movestring;

            // Movement failed. As with VERGE, the standard policy is just to mash up against the obstruction forever
            // and hope it goes away somehow.
            if (data.collided) {
                if (movestring.movement_left == Int32.MinValue) movestring.movement_left = data.time_shortfall; 
                else movestring.movement_left += data.time_shortfall;
                return 0;
            }

            while (time_left > 0) {
                if (movestring.movement_left == Int32.MinValue) {
                    movestring.movement_left = 0;
                    movestring.step++;
                }
                time_left = movestring.ready(time_left);
                if (time_left <= 0 && ent.moving) ent.set_walk_state(false);

                if (movestring.movement_left <= 0) { // If not currently walking
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

                if (movestring.movement_left > 0) {
                    if (movestring.movement_left <= time_left) { // Can move farther than is needed in the remaining time                                                    
                        ent.velocity += Utility.velocity_from_direction(ent.movement_direction, movestring.movement_left / 100f, 
                            ((float)movestring.movement_left) / ent.speed);
                        time_left -= movestring.movement_left;
                        movestring.movement_left = Int32.MinValue;
                        return time_left;
                    }
                    else { // Entity will spend the entire period moving                        
                        ent.velocity += Utility.velocity_from_direction(ent.movement_direction, ent.speed, 100f);
                        movestring.movement_left -= time_left;
                        return 0;
                    }
                }
            }

            return 0;
        }


        // A helper function for entity movement. Sets up the entity to go a certain distance in a certain cardinal direction.
        // The distance is either pixels or tiles, depending on the value of in_tiles.
        public static void set_up_cardinal_movement(Entity ent, Direction dir, int distance, bool in_tiles) {
            ent.facing = ent.movement_direction = dir;
            if (distance > 0) {
                ent.movestring.movement_left = distance * 100;
                ent.set_walk_state(true);
                if (in_tiles) ent.movestring.movement_left *= game.map.tileset.tilesize;
            }
            else { // distance 0: no movement, just change facing
                ent.movestring.movement_left = 0;
                ent.movestring.step++;
            }
        }
    }

}
