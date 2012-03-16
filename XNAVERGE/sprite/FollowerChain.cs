using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace XNAVERGE {
    //    Represents a chain of entities trailing behind the player. As long as there is a player on the 
    // map, those entities will not be updated normally (their movement and obstruction handling isn't even 
    // called, though they can still obstruct others if set to obstructing = true). They mimic the player's
    // movements exactly, onward down the chain, even when player_controllable is false.
    // If the current player is in the list of followers, it will simply be treated as though it weren't.
    //    The first entity in the list follows the player, the second follows that entity, and so on. 
    class FollowerChain {

        // When true, followers found to be missing or lazy-deleted during Update() will be silently 
        // removed from the chain. When false, an exception will be thrown.
        public static bool CULL_MISSING_FOLLOWERS = true; 

        public List<Entity> list;
        protected FollowerData prev_leader_data;

        public Entity leader { // currently the player has to be the leader
            get {
                Entity player = VERGEGame.game.player;
                if (player != null && !player.deleted) return player;
                return null;
            } 
        }

        public FollowerChain() {
            list = new List<Entity>();
        }

        // True if the entity is in this chain and the leader entity is still on the map.
        public bool has_follower(Entity ent) {
            if (leader == null || ent == leader) return false;
            return list.Contains(ent);
        }

        // This is called before entities are moved, so the list knows how the leader moved.
        public void store_leader_position() {
        }

        public void Update() {
            Entity last = leader;
            if (last == null) return;
            for (int i = 0; i < list.Count; i++) {
                
            }
        }

    }

    public class FollowerData {
        public Point position;
        public Direction facing;
        public bool moving;
    }
}

