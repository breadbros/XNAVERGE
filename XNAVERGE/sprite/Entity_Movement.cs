using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public partial class Entity {
        public const int NO_NUMBER = Int32.MinValue; // indicates no number is associated with a movecode
        public const int NO_MOVESCRIPT = Int32.MinValue; // indicates that a character is done moving or has no movescript
        public const int DEFAULT_MOVE_ARRAY_LENGTH = 10; // starting movestring action array size (will be expanded as necessary).
        public const bool DEFAULT_TO_TILE_MOVEMENT = true; // assume tile movement if a movestring does not specify

        // constants for the weird custom format this uses
        public const int MOVESTRING_FACECODE_UP = 1;
        public const int MOVESTRING_FACECODE_DOWN = 0;
        public const int MOVESTRING_FACECODE_LEFT = 2;
        public const int MOVESTRING_FACECODE_RIGHT = 3;
        // VERGE only has 1-3, but we can safely add diagonals as well
        public const int MOVESTRING_FACECODE_DOWNLEFT = 4;
        public const int MOVESTRING_FACECODE_DOWNRIGHT = 5;
        public const int MOVESTRING_FACECODE_UPLEFT = 6;
        public const int MOVESTRING_FACECODE_UPRIGHT = 7;

        public static Regex movestring_regex = new Regex(
                @"([ulrdxyptzwfb])\s*(\d*)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // MOVESTRING-SPECIFIC VARIABLES
        public MovestringCommand[] move_actions;
        public int[] move_params;
        public bool tile_movement;
        
        // align_to_tile influences how an entity behaves when told to move a fixed number of tiles -- that is, when 
        // it's given a U/D/L/R move action in tile-based movement mode. 
        //    * If align_to_grid is false, it will move a number of pixels equal to 
        //      (tiles you told it to move)*(map's tilesize). 
        //    * If align_to_grid is true, it will move (tiles you told it to move-1)*(map's tilesize), then move
        //      however much farther is necessary for it to end on a tile boundary. 
        // In other words, the modes are equivalent when the entity is on a tile boundary to begin with. When 
        // align_to_grid is true, the entity may move less distance than you tell it to, but never by more than
        // (tilesize - 1) pixels.
        // This is false by default, since it's slower and will almost never come up under the default configuration.
        public bool align_to_grid; 
        
        protected int wait_time; // time left to wait, in hundredths of (speed-adjusted) ticks. 0 if not waiting.
        protected int cur_move_action; // current position in the move_actions/move_params array. 
        
        protected int movement_left; // the remaining movement allotment when moving automatically
        protected Direction movement_direction; // movement direction, which may differ from facing direction (e.g. Michael Jackson's Moonwalker Gaiden)

        // Sets the default values for the many, many movement-related members each entity possesses.
        protected virtual void initialize_movement_attributes() {
            speed = DEFAULT_SPEED;
            _moving = false;
            move_actions = new MovestringCommand[DEFAULT_MOVE_ARRAY_LENGTH];
            move_params = new int[DEFAULT_MOVE_ARRAY_LENGTH];
            tile_movement = DEFAULT_TO_TILE_MOVEMENT;
            set_movestring("");
            align_to_grid = false;
            wait_time = 0;            
        }

        // gets the appropriate distance to move (in hundredths of pixels) when in tile mode with align_to_grid true, assuming the 
        // given starting point (in pixels) and nominal move distance (in tiles). The function also takes a boolean to indicate if 
        // the movement is in the positive (down/right) or negative (left/up) direction.
        // The way modulus is implemented means that different math needs to be used depending on whether the starting point
        // is positive or negative. If both x and y are positive, x % y == -(-x % y).
        private int grid_align_move_distance(int start, int dist, bool positive) {
            int tilesize = VERGEGame.game.map.tileset.tilesize;
            if (start >= 0) {
                if (positive) return 100 * (dist * tilesize - start % tilesize);
                else return 100 * ((dist - 1) * tilesize + start % tilesize);
            }
            else {
                if (positive) return 100 * ((dist - 1) * tilesize - start % tilesize);
                else return 100 * (dist * tilesize + start % tilesize);
            }                
        }

        // Doubles the length of the movestring storage arrays to accomodate a longer string.
        protected void expand_move_arrays() {
            MovestringCommand[] new_actions = new MovestringCommand[move_actions.Length * 2];
            int[] new_params = new int[new_actions.Length];
            move_actions.CopyTo(new_actions, 0);
            move_params.CopyTo(new_params, 0);
            move_actions = new_actions;
            move_params = new_params;
        }        

        public void set_movestring(String str) {
            if (String.IsNullOrEmpty(str)) {
                move_actions[0] = MovestringCommand.Stop;
                set_walk_state(false);
                cur_move_action = NO_MOVESCRIPT;
                return;
            }
            MatchCollection matches = movestring_regex.Matches(str);
            GroupCollection groups;
            int parameter, len, step;
            
            len = move_actions.Length;
            step = 0;
            foreach (Match match in matches) {
                groups = match.Groups;
                parameter = NO_NUMBER;
                if (!String.IsNullOrEmpty(groups[2].Value)) parameter = Int32.Parse(groups[2].Value);
                move_params[step] = parameter;
                switch (groups[1].Value) {
                    case "B": // loop back to start. 
                        // This normally takes no number parameter, but unlike VERGE, you can include one, in which case
                        // it will loop that many times, then stop.
                        if (parameter > 0 || parameter == NO_NUMBER) move_actions[step] = MovestringCommand.Loop;
                        else move_actions[step] = MovestringCommand.Stop;
                        break;
                    case "T": // switch to pixel coordinates
                        move_actions[step] = MovestringCommand.TileMode;
                        break;
                    case "P": // switch to tile coordinates
                        move_actions[step] = MovestringCommand.PixelMode;
                        break;
                    case "Z": // frame switch (may or may not have a number parameter)
                        // This locks the entity into the specified frame, suppressing animation during movement. In VERGE, setting it to 0
                        // restored the entity to normal animation, but this is a problem if you want to lock it at frame 0. Thus, I'm changing
                        // the rule: Z0 locks the entity at frame 0, and Z with no number after it restores normal animation.
                        // Be warned that this may mess up the occasional .MAP-embedded movestring.
                        move_actions[step] = MovestringCommand.Frame;                        
                        break;
                    case "F": // face (not necessary, since a distance-0 move accomplishes the same thing)
                        move_actions[step] = MovestringCommand.Face;                        
                        break;
                    case "U": // move up
                        move_actions[step] = MovestringCommand.Up;
                        break;
                    case "D": // move down
                        move_actions[step] = MovestringCommand.Down;
                        break;
                    case "L": // move left
                        move_actions[step] = MovestringCommand.Left;
                        break;
                    case "R": // move right
                        move_actions[step] = MovestringCommand.Right;
                        break;
                    case "W": // wait
                        move_actions[step] = MovestringCommand.Wait;
                        break;
                    case "X": // walk straight to specific x
                        move_actions[step] = MovestringCommand.ToX;
                        break;
                    case "Y": // walk straight to specific y
                        move_actions[step] = MovestringCommand.ToY;
                        break;
                    default:
                        break;
                }
                step++;
                if (step >= len) expand_move_arrays();
            }
            move_actions[step] = MovestringCommand.Stop;
            if (step == 0) {
                cur_move_action = NO_MOVESCRIPT; // no movescript (obviously something was passed, but the regexp didn't catch any of it)
                set_walk_state(false);
            }
            else {
                cur_move_action = -1;
                wait_time = 0;
                movement_left = 0;
            }
            tile_movement = DEFAULT_TO_TILE_MOVEMENT;
        }

        public override void Update() {
            movement_handler();
            VERGEGame.game.entity_space.Update(this);
        }


        // This handler manages the actual movement, and is one of the most important codeblocks in the whole engine.
        // It is called every time the main game's Update() function runs, usually once per tick.
        protected virtual void movement_handler() {
            int elapsed;
            
            if (cur_move_action == NO_MOVESCRIPT && this != VERGEGame.game.player) {
                last_logic_tick = VERGEGame.game.tick;                
                return;
            }
            
            elapsed = speed * (VERGEGame.game.tick - last_logic_tick); 
            last_logic_tick = VERGEGame.game.tick;
            if (this == VERGEGame.game.player) {                
                control_player(elapsed);            
                return;
            }

            // player branch goes here
            while (elapsed > 0) {
                if (movement_left > 0) {                    
                    elapsed = try_to_move(elapsed, false); // try_to_move will decrement movement_left as appropriate
                    //Console.WriteLine(elapsed);
                    if (elapsed <= 0) return;
                }
                else if (wait_time > 0) {
                    elapsed -= wait_time;
                    if (elapsed <= 0) { // if wait_time was larger than the time elapsed, keep waiting
                        wait_time = -elapsed; // reduce waiting time
                        // wait_time is now reduced by the original elapsed amount: 
                        //     wait_time = -elapsed = -(orig_elapsed - wait_time) = wait_time - orig_elapsed;                    
                        return;
                    }
                }                
                cur_move_action++;
                // discard any fractional-pixel movement so far
                _exact_x = hitbox.X * 100; 
                _exact_y = hitbox.Y * 100; 
                //Console.WriteLine("{0} (entity {1}), move action {2}: {3} {4}", name, index, cur_move_action, move_actions[cur_move_action], move_params[cur_move_action]);                
                switch (move_actions[cur_move_action]) {
                    case MovestringCommand.Stop:
                        cur_move_action = NO_MOVESCRIPT;
                        set_walk_state(false);
                        tile_movement = DEFAULT_TO_TILE_MOVEMENT;
                        // TODO: callbacks go here
                        return;
                    case MovestringCommand.Loop:
                        if (move_params[cur_move_action] > 0) { // if it's a finite-iteration loop...
                            move_params[cur_move_action]--;
                            if (move_params[cur_move_action] <= 0) move_actions[cur_move_action] = MovestringCommand.Stop; // this will be the last time
                        }
                        cur_move_action = -1;
                        tile_movement = DEFAULT_TO_TILE_MOVEMENT;
                        break;
                    case MovestringCommand.Wait:
                        set_walk_state(false);
                        wait_time = move_params[cur_move_action]*100;
                        break;
                    case MovestringCommand.PixelMode:                        
                        tile_movement = false;
                        break;
                    case MovestringCommand.TileMode:
                        tile_movement = true;
                        break;
                    case MovestringCommand.Face:
                        facing = Utility.movestring_face_to_direction(move_params[cur_move_action]);
                        break;
                    case MovestringCommand.Frame:                        
                        if (move_params[cur_move_action] >= 0) set_frame(move_params[cur_move_action]);
                        else resume_animation();
                        break;
                    case MovestringCommand.Up:
                        facing = movement_direction = Direction.Up;
                        if (move_params[cur_move_action] > 0) {
                            set_walk_state(true);                            
                            if (tile_movement) {
                                if (align_to_grid) movement_left = grid_align_move_distance(hitbox.Y, move_params[cur_move_action], false);
                                else movement_left = 100 * move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize;
                            }
                            else movement_left = 100 * move_params[cur_move_action];
                        }
                        break;
                    case MovestringCommand.Down:
                        facing = movement_direction = Direction.Down;
                        if (move_params[cur_move_action] > 0) {
                            set_walk_state(true);                            
                            if (tile_movement) {
                                if (align_to_grid) movement_left = grid_align_move_distance(hitbox.Y, move_params[cur_move_action], true);
                                else movement_left = 100 * move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize;
                            }
                            else movement_left = 100 * move_params[cur_move_action];
                        }
                        break;
                    case MovestringCommand.Left:
                        facing = movement_direction = Direction.Left;
                        if (move_params[cur_move_action] > 0) {
                            set_walk_state(true);                            
                            if (tile_movement) {
                                if (align_to_grid) movement_left = grid_align_move_distance(hitbox.X, move_params[cur_move_action], false);
                                else movement_left = 100 * move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize;
                            }
                            else movement_left = 100 * move_params[cur_move_action];
                        }
                        break;
                    case MovestringCommand.Right:
                        facing = movement_direction = Direction.Right;
                        if (move_params[cur_move_action] > 0) {
                            set_walk_state(true);                            
                            if (tile_movement) {
                                if (align_to_grid) movement_left = grid_align_move_distance(hitbox.X, move_params[cur_move_action], true);
                                else movement_left = 100 * move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize;
                            }
                            else movement_left = 100 * move_params[cur_move_action];
                        }
                        break;
                    case MovestringCommand.ToX:
                        if (tile_movement) movement_left = 100 * (hitbox.X - move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize);
                        else movement_left = movement_left * (hitbox.X - move_params[cur_move_action]); // will be positive if moving left

                        if (movement_left > 0) {
                            facing = movement_direction = Direction.Left;
                            set_walk_state(true);                            
                        }
                        else if (hitbox.X < movement_left) {
                            facing = movement_direction = Direction.Right;
                            movement_left = -movement_left;
                            set_walk_state(true);
                        }
                        // Note that nothing happens if the entity is already on the target.
                        break;
                    case MovestringCommand.ToY:
                        if (tile_movement) movement_left = 100 * (hitbox.Y - move_params[cur_move_action] * VERGEGame.game.map.tileset.tilesize);
                        else {                            
                            movement_left = 100 * (hitbox.Y - move_params[cur_move_action]); // will be positive if moving left
                        }                        
                        if (movement_left > 0) {
                            facing = movement_direction = Direction.Up;
                            set_walk_state(true);                            
                        }
                        else if (hitbox.Y < movement_left) {
                            facing = movement_direction = Direction.Down;
                            movement_left = -movement_left;
                            set_walk_state(true);
                        }
                        // Note that nothing happens if the entity is already on the target.
                    break;
                    default:
                    throw new Exception("Entity #" + ((int)index).ToString() + "(" + name + ") has an invalid move action code ("
                            + ((int)move_actions[cur_move_action]).ToString() + ") at move_actions[] index " + cur_move_action.ToString()
                            + ". If you haven't been changing these values manually, this is some kind of engine bug.");
                }
            }
        }

        // Moves the character as far as it can toward its destination. in the elapsed time (hundredths of ticks), and returns 
        // what remains of the time allotted to it (the elapsed parameter). If the character hits an obstruction it will block,
        // using up all the elapsed time while accomplishing nothing.
        protected virtual int try_to_move(int elapsed, bool player_control) {
            int maxmove, actualmove;
            int dist_to_next_pixel_x = 0, dist_to_next_pixel_y = 0, extra_dist, tilesize;
            Point signs;
            tilesize = VERGEGame.game.map.tileset.tilesize;
            if (player_control) maxmove = elapsed;
            else maxmove = Math.Min(elapsed, movement_left);
            signs = Utility.signs_from_direction(movement_direction, true);

            if (signs.X < 0) dist_to_next_pixel_x = _exact_x - hitbox.X * 100;
            else if (signs.X > 0) dist_to_next_pixel_x = (hitbox.X + 1) * 100 - _exact_x - 1;
            if (signs.Y < 0) dist_to_next_pixel_y = _exact_y - hitbox.Y * 100;
            else if (signs.Y > 0) dist_to_next_pixel_y = (hitbox.Y + 1) * 100 - _exact_y - 1;

            if ((int)movement_direction < 4)  // cardinal-direction movement
                extra_dist = Math.Max(dist_to_next_pixel_x, dist_to_next_pixel_y); // one of these will be set, and we want that one
            else // main diagonal movement
                extra_dist = Math.Min(dist_to_next_pixel_x, dist_to_next_pixel_y); // both are set, and we want the smaller one (even if it's zero)

            if (extra_dist < maxmove) {
                actualmove = extra_dist + VERGEGame.game.map.max_unobstructed_distance(maxmove - extra_dist, signs.X, signs.Y, this);
            }
            else actualmove = maxmove;

            exact_x += signs.X * actualmove;
            exact_y += signs.Y * actualmove;
            if (!player_control) movement_left -= actualmove;
            if (maxmove > actualmove) return 0; //hit an obstruction: waste all remaining time
            return elapsed - actualmove;
        }

    }

    
    // An enumeration of movestring actions. "Stop" is not actually used in movestrings -- it indicates a non-looping end and is inserted by the loader.
    public enum MovestringCommand { Up, Down, Left, Right, Wait, Frame, Face, Loop, PixelMode, TileMode, ToX, ToY, Stop } 

    // An enumeration of wander styles. The first, "scripted", covers both the "static" and "scripted" modes in normal VERGE and denotes an entity
    // that does not wander at random.
    public enum WanderMode { Scripted, Zone, Rectangle };

    public class MalformedMovestringException : Exception {
        public MalformedMovestringException(String movestring) : base("\"" + movestring + "\" is not a valid movestring. Each term must be one of U, D, L, R, W, Z, F, or B followed by a nonnegative number, or one of Z, B, P, or T by itself. For more information, consult http://verge-rpg.com/docs/the-verge-3-manual/entity-functions/entitymove/.") {}
    }


}
