using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace XNAVERGE {
    //    Represents a chain of entities trailing behind the player. Those entities will not be updated 
    // normally (their movement and obstruction handling isn't even called, so they can neither be
    // obstructed nor obstruct others). They mimic the player's movements exactly, onward down the 
    // chain, even when player_controllable is false. 
    //    The first entity in the list follows the player, the second follows that entity, and so on. 
    public class FollowerChain {
        public Entity leader { get; private set; }
        protected List<FollowerData> list;         
        public bool is_empty { get { return (list.Count == 0); } }

        // A ring buffer storing the player's recent path. The length of the buffer is determined by 
        // the number of followers and the entity's hitbox size.
        protected MovementData[] path_buffer;         
        protected int first, last; // start and end of the buffer (which may not use the entire array)
        protected int step; // approximate distance between entities in the chain, in pixels         
        protected Vector2 prev_leader_position;        

        public FollowerChain(Entity leader) {
            this.leader = leader;
            list = new List<FollowerData>();
            if (leader != null) {
                step = Math.Max(leader.hitbox.Width, leader.hitbox.Height);
                reset_buffer();
            }
        }

        // This is called before entities are moved, so the list knows how the leader moved.
        public void store_leader_position() {
            if (leader != null) prev_leader_position = leader.exact_pos;
        }

        public void Update() {
            Vector2 exact_pos;
            Point old_pos, new_pos, move_step;
            Direction dir;
            int distance, goal_dist, cur_idx, num_followers = list.Count;
            if (num_followers == 0) return;
            exact_pos = leader.exact_pos - prev_leader_position;

            old_pos = new Point((int)prev_leader_position.X, (int)prev_leader_position.Y);
            new_pos = leader.hitbox.Location;
            new_pos.X -= old_pos.X;
            new_pos.Y -= old_pos.Y;
            Console.WriteLine("!!!");
            dir = Utility.direction_from_signs(Math.Sign(exact_pos.X), Math.Sign(exact_pos.Y), false);            
            while (new_pos != Point.Zero) {
                _inc_first();
                move_step = new Point(Math.Sign(new_pos.X), Math.Sign(new_pos.Y));                
                old_pos.X += move_step.X;
                old_pos.Y += move_step.Y;
                new_pos.X -= move_step.X;
                new_pos.Y -= move_step.Y;
                path_buffer[first].position = old_pos;
                path_buffer[first].distance = Math.Abs(move_step.X) + Math.Abs(move_step.Y);
                path_buffer[first].direction = dir;
            }

            cur_idx = first;
            distance = 0;
            for (int i = 0; i < num_followers; i++) {
                goal_dist = (i+1)*step;
                old_pos = list[i].entity.hitbox.Location;
                while (distance < goal_dist) {                                        
                    distance += path_buffer[cur_idx].distance;
                    cur_idx = _next_idx(cur_idx);
                    if (cur_idx == last) distance = Int32.MaxValue;
                }
                new_pos = path_buffer[cur_idx].position;
                if (old_pos == new_pos) {
                    if (list[i].entity.moving && !leader.moving) list[i].entity.set_walk_state(false);
                }
                else {
                    list[i].entity.x = new_pos.X;
                    list[i].entity.y = new_pos.Y;
                    if (!list[i].entity.moving) list[i].entity.set_walk_state(true);
                    list[i].entity.facing = path_buffer[cur_idx].direction;
                }                
            }
            last = cur_idx;
            
        }

        protected void reset_buffer() {
            if (list.Count == 0) path_buffer = new MovementData[1];
            else {
                path_buffer = new MovementData[list.Count * step + 1];
            }
            first = 0;
            last = 0;
            path_buffer[0].position = leader.hitbox.Location;
        }

        protected void _inc_first() {            
            first++;
            if (first >= path_buffer.Length) first = 0;            
            if (first == last) {
                last++;
                if (last >= path_buffer.Length) last = 0;
            }
        }

        protected int _prev_idx(int idx) {
            if (idx == first) return idx; // can't go any farther
            idx++;
            if (idx >= path_buffer.Length) return 0;
            return idx;
        }

        protected int _next_idx(int idx) {            
            if (idx == last) return idx; // can't go any farther
            idx--;
            if (idx < 0) return (path_buffer.Length - 1);
            return idx;
        }

        // -------------------------------
        //  Follower management functions
        // -------------------------------
        // These functions change the follower list, which always wipes out the path data. 
        // Thus, you should stack all current and future followers on top of the leader 
        // before you add or remove followers.

        // Adds a new follower to the end of the follow chain. Returns false if the entity
        // given was already in the chain. This sets the follower to non-obstructing and
        // unobstructable.
        public bool add(Entity ent) {
            FollowerData fd;
            if (ent == leader || position_of(ent) >= 0) return false;
            fd = new FollowerData(ent);            
            list.Add(fd);
            reset_buffer();
            ent.obstructable = false;
            ent.obstructing = false;
            return true;
        }

        public bool remove(Entity ent) {
            int idx = position_of(ent);
            if (idx < 0) return false;
            list.RemoveAt(idx);
            return true;
        }


        public int position_of(Entity ent) {
            int result = -1;
            if (ent == leader) return -1;
            for (int i = 0; i < list.Count; i++) {
                if (list[i].entity == ent) {
                    result = i;
                    break;
                }
            }
            return result;
        }

        protected struct MovementData {
            public Point position;
            public Direction direction; // may be different than offset implies, due to subpixel movement
            public int distance;
        }

        protected class FollowerData {
            public Entity entity;
            public int original_speed;
            public FollowerData(Entity e) { 
                entity = e;
                original_speed = entity.speed;
            }
        }

    }



    public class FollowException : Exception { public FollowException(string msg) : base("FollowChain error: " + msg) {} }
}



