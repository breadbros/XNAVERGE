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

        // A general purpose VERGE-emulation handler that defers to various other handlers depending on
        // the entity's state.
        public static int omnibus_vergestyle_handler(Entity ent, Object state, ref EntityMovementData data) {            
            if (ent == VERGEGame.game.player && VERGEGame.game.player_controllable)
                return vergestyle_player_movement_handler(ent, state, ref data);
            else                 
                return entity_movescript_handler(ent, state, ref data);
        }

        // Moves the entity in accordance with the player's inputs. This works like VERGE player input
        // EXCEPT that, when diagonals are enabled, it moves the player at the same speed overall 
        // (unlike VERGE, which moved them both vertically and horizontally as fast as if they were 
        //  moving in that direction alone)
        public static int vergestyle_player_movement_handler(Entity ent, Object state, ref EntityMovementData data) {
            int x = 0, y = 0;            
            float factor;
            Vector2 ofs, old_velocity;
            VERGEMap map;
            Point pt;
            Rectangle box;
            old_velocity = ent.velocity;
            ent.velocity = ent.acceleration = Vector2.Zero;
            ent.acceleration = Vector2.Zero;
            
            if (data.first_call) {
                if (VERGEGame.game.dir.left.down) x--;
                if (VERGEGame.game.dir.right.down) x++;
                if (VERGEGame.game.dir.up.down) y--;
                if (VERGEGame.game.dir.down.down) y++;
            }
            else if (data.collided) {
                // Hacky sliding code. Clean this shit up sometime!
                map = VERGEGame.game.map;                
                x = Math.Sign(data.attempted_path.X);
                y = Math.Sign(data.attempted_path.Y);

                if (data.obstructed_by_entity) {
                    box = data.collided_entity.hitbox;
                    if (box.Top < ent.hitbox.Bottom && box.Bottom > ent.hitbox.Top) x = 0;
                    if (box.Left < ent.hitbox.Right && box.Right > ent.hitbox.Left) y = 0;
                    if (x != 0 && y != 0) { x = 0; y = 0; }
                }

                if (x == 0 && y != 0) {
                    pt = new Point(ent.hitbox.X + 1,
                                   ent.hitbox.Y + y + (1 + y) * (ent.hitbox.Height - 1) / 2);
                    for (int i = 0; i < ent.hitbox.Width - 2; i++) { // first check all but the ends
                        if (map.obs_at_pixel(pt.X, pt.Y)) {
                            y = 0;
                            break;
                        }
                        pt.X++;
                    }

                    if (y != 0) {
                        if (map.obs_at_pixel(ent.hitbox.Left, pt.Y)) {
                            if (!map.obs_at_pixel(ent.hitbox.Right - 1, pt.Y)) x = 1;
                            else y = 0;
                        }
                        else if (map.obs_at_pixel(ent.hitbox.Right - 1, pt.Y)) {
                            if (!map.obs_at_pixel(ent.hitbox.Left, pt.Y)) x = -1;
                            else y = 0;
                        }
                        if (x != 0) {
                            pt.X = ent.hitbox.X + x + (1 + x) * (ent.hitbox.Width - 1) / 2;

                            for (int i = 0; i < ent.hitbox.Height + 1; i++) {
                                if (map.obs_at_pixel(pt.X, pt.Y)) {
                                    x = 0;
                                    y = 0;
                                    break;
                                }
                                pt.Y -= y;
                            }
                        }
                    }
                }
                else if (y == 0 && x != 0) {                    
                    pt = new Point(ent.hitbox.X + x + (1 + x) * (ent.hitbox.Width - 1) / 2,
                                   ent.hitbox.Y + 1);
                                   
                    for (int i = 0; i < ent.hitbox.Height - 2; i++) { // first check all but the ends
                        if (map.obs_at_pixel(pt.X, pt.Y)) {
                            x = 0;
                            break;
                        }
                        pt.Y++;
                    }

                    if (x != 0) {
                        if (map.obs_at_pixel(pt.X, ent.hitbox.Top)) {
                            if (!map.obs_at_pixel(pt.X, ent.hitbox.Bottom - 1)) y = 1;
                            else x = 0;
                        }
                        else if (map.obs_at_pixel(pt.X, ent.hitbox.Bottom - 1)) {
                            if (!map.obs_at_pixel(pt.X, ent.hitbox.Top)) y = -1;
                            else x = 0;
                        }
                        if (y != 0) {
                            pt.Y = ent.hitbox.Y + y + (1 + y) * (ent.hitbox.Height - 1) / 2;

                            for (int i = 0; i < ent.hitbox.Width + 1; i++) {
                                if (map.obs_at_pixel(pt.X, pt.Y)) {
                                    x = 0;
                                    y = 0;
                                    break;
                                }
                                pt.X -= x;
                            }
                        }
                    }                    
                }
                else if (x != 0 && y != 0) {
                    pt = new Point(ent.hitbox.X + x + (1 + x) * (ent.hitbox.Width - 1) / 2,
                                   ent.hitbox.Y + (1 + y) * (ent.hitbox.Height - 1) / 2);
                    for (int i=0; i < ent.hitbox.Height; i++) {
                        if (map.obs_at_pixel(pt.X, pt.Y)) {
                            x = 0;
                            break;
                        }
                        pt.Y -= y;
                    }
                    pt = new Point(ent.hitbox.X + (1 + x) * (ent.hitbox.Width - 1) / 2,
                                   ent.hitbox.Y + y + (1 + y) * (ent.hitbox.Height - 1) / 2);
                    for (int i = 0; i < ent.hitbox.Width; i++) {
                        if (map.obs_at_pixel(pt.X, pt.Y)) {
                            y = 0;
                            break;
                        }
                        pt.X -= x;
                    }

                    if (x != 0 && y != 0 && 
                        map.obs_at_pixel(ent.hitbox.X + x + (1 + x) * (ent.hitbox.Width - 1) / 2,
                                         ent.hitbox.Y + y + (1 + y) * (ent.hitbox.Height - 1) / 2)) {
                        if (Math.Abs(data.attempted_path.X) > Math.Abs(data.attempted_path.Y)) y = 0;
                        else if (Math.Abs(data.attempted_path.X) < Math.Abs(data.attempted_path.Y)) x = 0; 
                        else {x = 0; y = 0; }
                    }

                    if (x == 0 && y == 0) return 0;
                    
                }
                if (x != 0 && y != 0) { // adjust alignment for diagonal movement
                    ofs = new Vector2( Math.Abs((float) (ent.hitbox.X + (1 + x) / 2) - ent.exact_x),
                                       Math.Abs((float) (ent.hitbox.Y + (1 + y) / 2) - ent.exact_y));
                    if (ofs.X < ofs.Y) ent.exact_y += y * (ofs.Y - ofs.X);
                    else if (ofs.X > ofs.Y) ent.exact_x += x * (ofs.X - ofs.Y);

                }
            }

            
            if (x == 0 && y == 0) {
                if (ent.moving) ent.set_walk_state(false);
            }
            else {
                ent.velocity.X = (float)x;
                ent.velocity.Y = (float)y;

                // ugly hack to ensure sub-pixel alignment -- only works reliably with a small number of movement
                // directions, and is incompatible with acceleration. 
                if (data.first_call && ent.acceleration == Vector2.Zero && ent.velocity != old_velocity) {
                    ent.x = ent.x; // these setters will put the entity in the middle of the pixel interval
                    ent.y = ent.y;
                }

                if (!ent.moving) ent.set_walk_state(true);
                factor = ent.speed / 100f;
                ent.movement_direction = Utility.direction_from_signs(x, y, true);
                if (!data.collided) { // Moving normally 
                    ent.facing = Utility.direction_from_signs(x, y, false);                    
                }
                
                if (Math.Abs(x) + Math.Abs(y) == 2) factor *= Utility.INV_SQRT2; // diagonal movement
                ent.velocity *= factor;

                if (data.collided) { // if sliding, move at most one pixel in this handler iteration
                    // figure out how much time it will take to move one pixel
                    factor = data.time - ent.speed;
                    if (Math.Abs(x) + Math.Abs(y) == 2) factor *= Utility.SQRT2;
                    return Math.Max((int)Math.Floor(factor), 0);                    
                }
            }
            return 0;
        }

        public static int entity_movescript_handler(Entity ent, Object state, ref EntityMovementData data) {
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

        public static void entity_wander_callback(Entity ent, bool aborted) {
            int tilesize;
            WanderState state = (WanderState)(ent.move_state);
            Direction dir = (Direction)VERGEGame.rand.Next(4); // random cardinal direction
            Point goal = ent.hitbox.Center;
            Point ofs = Utility.signs_from_direction(dir, false);
            tilesize = VERGEGame.game.map.tileset.tilesize;
            goal.X = goal.X / tilesize + ofs.X;
            goal.Y = goal.Y / tilesize + ofs.Y;
            if (state.can_wander_to(goal.X, goal.Y)) {
                switch (dir) {
                    case Direction.Up:
                        ent.move("TW" + state.delay + "U1", Default_Handlers.entity_wander_callback, 0);
                        break;
                    case Direction.Down:
                        ent.move("TW" + state.delay + "D1", Default_Handlers.entity_wander_callback, 0);
                        break;
                    case Direction.Left:
                        ent.move("TW" + state.delay + "L1", Default_Handlers.entity_wander_callback, 0);
                        break;
                    case Direction.Right:
                        ent.move("TW" + state.delay + "R1", Default_Handlers.entity_wander_callback, 0);
                        break;
                }
            }
            else ent.move("W" + state.delay, Default_Handlers.entity_wander_callback, 0);
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
