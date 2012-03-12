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

        public EntityMovementDelegate handler;
        public Object move_state; // any special state needed by the movement handler goes here 
        public Movestring movestring;

        public virtual void move(String movestring) { this.move(movestring, null, Movestring.NEVER_TIMEOUT); }
        public virtual void move(String movestring, MovestringEndingDelegate callback) { this.move(movestring, callback, Movestring.NEVER_TIMEOUT); }
        public virtual void move(String movestring, MovestringEndingDelegate callback, int timeout) {
            this.movestring = new Movestring(movestring, callback, this, timeout);
        }

        // Changes the entity's wander state. If given a single integer, sets the entity to wander within its zone
        // with the given delay. If given five integers, the first four are tile coordinates defining a wander
        // rectangle, and the last is the delay.
        public virtual void wander(int delay) {
            WanderState state = (WanderState)move_state;            
            state.mode = WanderMode.Zone;
            state.delay = Math.Max(0,delay);
            Default_Handlers.entity_wander_callback(this, true);
        }
        public virtual void wander(int tx1, int ty1, int tx2, int ty2, int delay) {
            WanderState state = (WanderState)move_state;
            state.mode = WanderMode.Rectangle ;
            state.delay = Math.Max(0, delay);
            state.rect = new Rectangle(tx1, ty1, tx2 - tx1 + 1, ty2 - ty1 + 2);
            Default_Handlers.entity_wander_callback(this, true);
        }

        public virtual void try_to_move(ref EntityMovementData data) {
            try_to_move_obs(ref data);
            try_to_move_ent(ref data);

            return;
        }

        // Checks how far this entity can move along the path without hitting a map obstruction. If an
        // obstruction is encountered, it edits the movement data to account for it.
        protected virtual void try_to_move_obs(ref EntityMovementData data) {            
            Point closest_obs = hitbox.Location;
            int max_distance;
            Point pixel_path, sign, farthest;            
            // The target pixel is where the character's upper-left hitbox pixel will be if it 
            // moves the full possible distance.
            pixel_path = new Point(((int)Math.Floor(_exact_pos.X + data.attempted_path.X)) - hitbox.X, 
                                   ((int)Math.Floor(_exact_pos.Y + data.attempted_path.Y)) - hitbox.Y);
            sign = new Point(Math.Sign(pixel_path.X), Math.Sign(pixel_path.Y));
            if (sign.X == 0 && sign.Y == 0) return; // no between-pixel movement;

            farthest = new Point(hitbox.X + pixel_path.X, hitbox.Y + pixel_path.Y);
            max_distance = Int32.MaxValue;

            if (sign.X != 0) { // moving horizontally -- check collision with left or right side
                _try_move_project_side(pixel_path, true, ref max_distance, ref farthest);
            }
            if (sign.Y != 0) { // moving vertically -- check collision with top or bottom
                _try_move_project_side(pixel_path, false, ref max_distance, ref farthest);
            }

            if (max_distance < Int32.MaxValue) { // obstructed                             
                data.collided = true;
                data.actual_path = new Vector2((float)farthest.X - hitbox.X, (float)farthest.Y - hitbox.Y);
                return;
            }            
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

            distance = (box_nose.Y - drag_nose.Y)/vpath.Y;
            vertical_side = (sign.X > 0) ^ (box_nose.X < drag_nose.X + path.X * distance);

            //if (this == VERGEGame.game.player) Console.WriteLine("{0},{1}", sign.X, sign.Y);            


            if (vertical_side) { // collision is between the left and right sides of the rectangles
                path.X = box_nose.X - sign.X - drag_nose.X;
                path.Y = (int)Math.Round(path.X * old_path.Y / (double)old_path.X);                
            }
            else { // collision is between the top and bottom sides of the rectangles
                path.Y = box_nose.Y - sign.Y - drag_nose.Y;
                path.X = (int)Math.Round(path.Y * old_path.Y / (double)old_path.X);                
            }
            old_path = path;
            return true;
        }

        // A helper for try_to_move(). This checks the prospective path for obstructing 
        // entities and returns a new path indicating how far the entity can move without
        // hitting one. 
        protected virtual void try_to_move_ent(ref EntityMovementData data) {
            Point pixel_path, best_path, cur_path;
            Rectangle goal;
            int best_distance, cur_distance;
            VERGEMap map = VERGEGame.game.map;
            BoundedSpace<Entity>.BoundedElementSet ent_enum;
            Entity ent, nearest_ent = null;
            
            // The target pixel is where the character's upper-left hitbox pixel will be if it 
            // moves the full possible distance.
            pixel_path = new Point(((int)Math.Floor(_exact_pos.X + data.attempted_path.X)) - hitbox.X,
                                               ((int)Math.Floor(_exact_pos.Y + data.attempted_path.Y)) - hitbox.Y);
                                
            best_distance = Math.Abs(pixel_path.X) + Math.Abs(pixel_path.Y);
            if (best_distance <= 0) return; // no between-pixel movement;                        
            best_path = pixel_path;
            goal = hitbox;
            goal.Offset(pixel_path.X, pixel_path.Y);
             
            ent_enum = VERGEGame.game.entity_space.elements_within_bounds(Rectangle.Union(hitbox, goal), true, this);
            while (ent_enum.GetNext(out ent)) {
                cur_path = pixel_path;
                if (test_collision(ent.hitbox, ref cur_path)) {                    
                    cur_distance = Math.Abs(cur_path.X) + Math.Abs(cur_path.Y);
                    System.Diagnostics.Debug.Assert(cur_distance < Math.Abs(pixel_path.X) + Math.Abs(pixel_path.Y));                    
                    if (cur_distance < best_distance) {
                        best_distance = cur_distance;
                        best_path = cur_path;
                        nearest_ent = ent;
                    }
                }
            }
            
            if (nearest_ent != null) {
                data.collided = true;
                data.collided_entity = nearest_ent;
                data.actual_path.X = (float)best_path.X;
                data.actual_path.Y = (float)best_path.Y; 
            }            
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
                
        public Direction movement_direction; // movement direction, which may differ from facing direction (e.g. in Michael Jackson's Moonwalker Gaiden)

        // Sets the default values for the many, many movement-related members each entity possesses.
        protected virtual void initialize_movement_attributes() {
            speed = DEFAULT_SPEED;
            _moving = false;
            handler = VERGEGame.game.default_entity_handler;
            time_pushing = 0;
            movestring = new Movestring("");
            initialize_move_state();
        }

        protected virtual void initialize_move_state() {
            move_state = new WanderState(this);
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

        */
        
    }




}
