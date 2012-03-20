using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace XNAVERGE {
    //    Represents a chain of entities trailing behind the player. Those entities will not be updated 
    // normally (their movement and obstruction handling isn't even called, so they can neither be
    // obstructed nor obstruct others). They mimic the player's movements exactly, onward down the 
    // chain, even when player_controllable is false. If the player is in the list of followers, it will 
    // simply be treated as though it weren't.
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
            FollowerData cur;
            Vector2 exact_pos;
            Point old_pos, new_pos;            
            int distance, goal_dist, num_followers = list.Count;
            if (num_followers == 0) return;
            exact_pos = leader.exact_pos - prev_leader_position;

            old_pos = new Point((int)prev_leader_position.X, (int)prev_leader_position.Y);
            new_pos = leader.hitbox.Location;
            
            new_pos.X -= old_pos.X;
            new_pos.Y -= old_pos.Y;

            distance = Math.Abs(new_pos.X) + Math.Abs(new_pos.Y);

            if (distance > 0) {
                _inc_first();
                path_buffer[first].offset = new_pos;
                path_buffer[first].direction = Utility.direction_from_signs(Math.Sign(exact_pos.X), Math.Sign(exact_pos.Y), false);
            }

            for (int i = 0; i < num_followers; i++) {                
                goal_dist = (1+i)*step;
                cur = list[i];
                old_pos = cur.entity.hitbox.Location;
                cur.dist += distance;                
                while (cur.dist > goal_dist && cur.idx != first) {
                    cur.entity.last_logic_tick = VERGEGame.game.tick;                    
                    if (i == (num_followers - 1)) { // bringing up the rear
                        _inc_last();
                        cur.idx = last;
                    }
                    else cur.idx = _next_idx(cur.idx);                    
                    new_pos = path_buffer[cur.idx].offset;
                    cur.entity.x += new_pos.X;
                    cur.entity.y += new_pos.Y;
                    cur.dist -= Math.Abs(new_pos.X) + Math.Abs(new_pos.Y);                    
                }

                new_pos = cur.entity.hitbox.Location;
                if (old_pos == new_pos) {
                    if (cur.entity.moving && !leader.moving) cur.entity.set_walk_state(false);
                }
                else {
                    if (!cur.entity.moving) cur.entity.set_walk_state(true);
                    cur.entity.facing = path_buffer[cur.idx].direction;
                }

            }
        }

        protected void reset_buffer() {
            if (list.Count == 0) path_buffer = new MovementData[1];
            else {
                path_buffer = new MovementData[list.Count * step + 2];
                foreach (FollowerData d in list) {
                    d.idx = 0;
                    d.dist = Math.Abs(leader.x - d.entity.x) + Math.Abs(leader.y - d.entity.y);
                }
            }
            first = 0;
            last = 0;
        }

        protected void _inc_first() {            
            first++;
            if (first >= path_buffer.Length) first = 0;
            System.Diagnostics.Debug.Assert(first != last);
        }

        protected void _inc_last() {
            if (last != first) {
                last++;
                if (last >= path_buffer.Length) last = 0;
            }
        }

        protected int _next_idx(int idx) {            
            if (idx == first) return idx; // can't go any farther
            idx++;
            if (idx >= path_buffer.Length) return 0;
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
            public Point offset;
            public Direction direction; // may be different than offset implies, due to subpixel movement
        }

        protected class FollowerData {
            public Entity entity;
            public int dist;
            public int idx;

            public FollowerData(Entity e) { entity = e; }
        }

    }



    public class FollowException : Exception { public FollowException(string msg) : base("FollowChain error: " + msg) {} }
}



