using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public partial class Entity {
        
        public const int NO_MOVESCRIPT = Int32.MinValue; // indicates that a character is done moving or has no movescript
        public const int DEFAULT_MOVE_ARRAY_LENGTH = 10; // starting movestring action array size (will be expanded as necessary).        

        public Movestring movestring;

        public bool test;

        // Returns the path up to the first obstruction (or the original path, if unobstructed)
        public virtual Vector2 try_to_move(Vector2 path) {
            return try_to_move_ent(try_to_move_obs(path));            
        }

        // A helper for try_to_move(). This checks the prospective path for map obstructions 
        // and returns a new path indicating how far the entity can move before being
        // obstructed. 
        protected virtual Vector2 try_to_move_obs(Vector2 path) {
            Vector2 target;
            Point closest_obs = hitbox.Location;
            int max_distance;
            Point pixel_path, sign, farthest;
            target = _exact_pos + path;
            // The target pixel is where the character's upper-left hitbox pixel will be if it 
            // moves the full possible distance.
            pixel_path = new Point(((int)Math.Floor(target.X)) - hitbox.X, ((int)Math.Floor(target.Y)) - hitbox.Y);
            sign = new Point(Math.Sign(pixel_path.X), Math.Sign(pixel_path.Y));
            if (sign.X == 0 && sign.Y == 0) return path; // no between-pixel movement;            

            farthest = new Point(hitbox.X + pixel_path.X, hitbox.Y + pixel_path.Y);
            max_distance = Int32.MaxValue;

            if (sign.X != 0) { // moving horizontally -- check collision with left or right side
                _try_move_project_side(pixel_path, true, ref max_distance, ref farthest);
            }
            if (sign.Y != 0) { // moving vertically -- check collision with top or bottom
                _try_move_project_side(pixel_path, false, ref max_distance, ref farthest);
            }

            if (max_distance < Int32.MaxValue) { // obstructed                             
                target.X = exact_pos.X + (float)(farthest.X - hitbox.X);
                target.Y = exact_pos.Y + (float)(farthest.Y - hitbox.Y);
                return target - exact_pos;
            }
            else return path;
        }

        // Tests whether, as this entity moves along the vector path, it will at any point cross 
        // the given box. Edits path to represent the farthest distance the entity can go without
        // being obstructed (meaning that it leaves it alone when returning false).
        public virtual bool test_collision(Rectangle box, ref Point old_path) {
            Point path, sign, drag_nose, box_nose;
            float distance;
            bool vertical_side;
            Vector2 vpath;
            Rectangle goal;
            path = old_path;
            sign = new Point(Math.Sign(path.X), Math.Sign(path.Y));
            goal = hitbox;
            goal.Offset(path.X, path.Y);
            
            // Do a preliminary check against the rectangle defined by the path's extremes
            if (!box.Intersects(Rectangle.Union(hitbox,goal))) return false;
            
            // If the boxes intersect before dragging, we know the path is zero (no movement possible.)
            // Checking this also takes care of a potential edge case later on.
            if (box.Intersects(hitbox)) {
                old_path = Point.Zero;
                return true;
            }

            if (sign.X == 0 && sign.Y == 0) return false;

            vpath = new Vector2((float)path.X, (float)path.Y);

            drag_nose = new Point(hitbox.X + (1 + sign.X) * (hitbox.Width - 1) / 2, hitbox.Y + (1 + sign.Y) * (hitbox.Height - 1) / 2);
            box_nose = new Point(box.X + (1 - sign.X) * (box.Width - 1) / 2, box.Y + (1 - sign.Y) * (box.Height - 1) / 2);


            switch (sign.X * sign.Y) {
                // The easy case: horizontal or vertical movement. The rectangular region
                // we tested above is the actual drag path, so we know that there's a
                // collision.
                case 0:
                    if (this == VERGEGame.game.player) Console.WriteLine("!");
                    if (sign.X != 0) old_path.X = box_nose.X - drag_nose.X - sign.X;
                    else old_path.Y = box_nose.Y - drag_nose.Y - sign.Y;
                    return true;

                // UL-to-DR path (or vice versa)
                case 1:
                    // Check if box is too far left to collide
                    distance = (box.Top - (hitbox.Bottom - 1)) / vpath.Y;
                    if (box.Right - 1 < hitbox.Left + path.X * distance) return false;
                    // Check if box is too far right to collide
                    distance = ((box.Bottom - 1) - hitbox.Top) / vpath.Y;
                    if (box.Left > (hitbox.Right - 1) + path.X * distance) return false;
                    // Collision occurred. Adjust path accordingly.                    

                    break;                    

                // UR-to-DL path (or vice versa)
                case -1:
                    // Check if box is too far left to collide
                    distance = ((box.Bottom - 1) - hitbox.Top) / vpath.Y;
                    if (box.Right - 1 < hitbox.Left + path.X * distance) return false;
                    // Check if box is too far right to collide
                    distance = (box.Top - (hitbox.Bottom - 1)) / vpath.Y;
                    if (box.Left > (hitbox.Right - 1) + path.X * distance) return false;
                    // Collision occurred. Adjust path accordingly.

                    break;

                default: throw new Exception("I can't even begin to guess what happened here.");
            }
                
            // At this point we know there was a collision, and that the dragged box is moving diagonally.

            distance = (box_nose.Y - drag_nose.Y)/((float)vpath.Y);
            vertical_side = (sign.X > 0) ^ (box_nose.X < drag_nose.X + path.X * distance);

            if (vertical_side) { // collision is between the left and right sides of the rectangles
                path.X = box_nose.X - sign.X - drag_nose.X;
                path.Y = path.X * old_path.Y / old_path.X;
            }
            else { // collision is between the top and bottom sides of the rectangles
                path.Y = box_nose.Y - sign.Y - drag_nose.Y;
                path.X = path.Y * old_path.X / old_path.Y;                
            }
            old_path = path;
            return true;
        }

        // A helper for try_to_move(). This checks the prospective path for obstructing 
        // entities and returns a new path indicating how far the entity can move without
        // hitting one. 
        protected virtual Vector2 try_to_move_ent(Vector2 path) {
            Vector2 target;
            Point pixel_path, best_path, cur_path;
            Rectangle goal;
            int best_distance, cur_distance;
            VERGEMap map = VERGEGame.game.map;
            BoundedSpace<Entity>.BoundedElementSet ent_enum;
            Entity ent;

            target = _exact_pos + path;
            // The target pixel is where the character's upper-left hitbox pixel will be if it 
            // moves the full possible distance.
            best_path = pixel_path = new Point(((int)Math.Floor(target.X)) - hitbox.X, ((int)Math.Floor(target.Y)) - hitbox.Y);
            best_distance = Math.Abs(pixel_path.X) + Math.Abs(pixel_path.Y);
            if (best_distance <= 0) return path; // no between-pixel movement;                        
            goal = hitbox;
            goal.Offset(pixel_path.X, pixel_path.Y);
             
            ent_enum = VERGEGame.game.entity_space.elements_within_bounds(Rectangle.Union(hitbox, goal), true, this);
            while (ent_enum.GetNext(out ent)) {
                cur_path = pixel_path;
                if (test_collision(ent.hitbox, ref cur_path)) {
                    cur_distance = Math.Abs(cur_path.X) + Math.Abs(cur_path.Y);
                    if (cur_distance < best_distance) {
                        best_distance = cur_distance;
                        best_path = cur_path;
                    }
                }
            }
            
            if (best_distance < Math.Abs(pixel_path.X) + Math.Abs(pixel_path.Y)) { // couldn't go the full distance                
                path.X = exact_pos.X + (float)(best_path.X - hitbox.X);
                path.Y = exact_pos.Y + (float)(best_path.Y - hitbox.Y); 
            }
            return path;
        }

        // A helper function for _try_to_move_obs. Projects one side of the entity toward the target
        // along the given (integer-valued) path vector. The "vertical" parameter is true if 
        // the side being projected is the left or right (i.e. the vertically aligned sides).
        protected void _try_move_project_side(Point path, bool vertical_side, ref int max_distance, ref Point farthest) {
            VERGEMap map = VERGEGame.game.map;
            Point cur_pixel, ray_goal, ray_result, test;
            int side_length, max_distance_so_far, cur_distance;
            cur_pixel = ray_goal = ray_result = default(Point);

            test = hitbox.Location;

            if (vertical_side) { // Projecting the left or right side of the hitbox
                cur_pixel.X = hitbox.X + (1 + Math.Sign(path.X)) * (hitbox.Width - 1) / 2; // leading side of hitbox                
                cur_pixel.Y = hitbox.Y;
                side_length = hitbox.Height;
            }
            else { // Projecting the top or bottom of the hitbox
                cur_pixel.X = hitbox.X;
                cur_pixel.Y = hitbox.Y + (1 + Math.Sign(path.Y)) * (hitbox.Height - 1) / 2; // leading side of hitbox                                
                side_length = hitbox.Width;
            }
            ray_goal.X = cur_pixel.X + path.X;
            ray_goal.Y = cur_pixel.Y + path.Y;
            max_distance_so_far = max_distance;

            for (int i = 0; i < side_length; i++) {
                cur_distance = max_distance_so_far;
                ray_result = map.cast_ray(cur_pixel, ray_goal, ref cur_distance);
                if (cur_distance < max_distance_so_far) { // obstruction encountered                    
                    max_distance_so_far = cur_distance;
                    // Adjust "farthest" pixel appropriately. Note that, since
                    // the farthest pixel corresponds to the top-left of the 
                    // farthest hitbox, we have to adjust ray_result based on its
                    // location within the hitbox.
                    farthest = new Point(ray_result.X + hitbox.X - cur_pixel.X,
                                         ray_result.Y + hitbox.Y - cur_pixel.Y);
                }
                if (vertical_side) {
                    cur_pixel.Y++;
                    ray_goal.Y++;
                }
                else {
                    cur_pixel.X++;
                    ray_goal.X++;
                }
            }
            if (max_distance_so_far < max_distance) max_distance = max_distance_so_far;
        }

        // CURRENTLY THIS DOES NOTHING AND THE DESCRIPTION OF IT IS ALL LIES
        // BEGIN LIES:
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
        //public bool align_to_grid; 
        // END LIES
        
        public int movement_left; // the remaining movement allotment when moving automatically (measured in hundredths of pixels)
        public Direction movement_direction; // movement direction, which may differ from facing direction (e.g. in Michael Jackson's Moonwalker Gaiden)

        // Sets the default values for the many, many movement-related members each entity possesses.
        protected virtual void initialize_movement_attributes() {
            speed = DEFAULT_SPEED;
            _moving = false;
            
            movestring = new Movestring("");            
        }
        /*
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
                if (VERGEGame.game.player_controllable) control_player(elapsed);            
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

        */
        
    }

    

    // An enumeration of wander styles. The first, "scripted", covers both the "static" and "scripted" modes in normal VERGE and denotes an entity
    // that does not wander at random.
    public enum WanderMode { Scripted, Zone, Rectangle };



}
