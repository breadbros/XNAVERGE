using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // Because entities have a lot of baggage and fiddly pieces, the movement-related part of the class is segregated in Entity_Movement.cs.
    public partial class Entity : Sprite {
        public const int DEFAULT_SPEED = 100;

        // GENERAL ATTRIBUTES
        public String name;
        public EntityActivationDelegate act_script; // activation script        
        public bool autoface;
        internal int index; // Index in the map.entities array. Don't twiddle with it! This is needed for a stupid, stupid reason.
        internal String script_name; // default activation script assigned during creation

        // MOBILITY AND ANIMATION ATTRIBUTES
        public virtual Direction facing {
            get { return _facing; }
            set {
                if( _facing == value ) return;
                _facing = value;
                if( _moving ) walk();
                else idle();
            }
        }
        protected Direction _facing;

        // obstructable is self-explanatory. obstructing  means the entity's hitbox obstructs entities with obstructing true.
        // When tile_obstruction is true the entity uses tile-based rather than pixel-based obstructions, treating every
        // obstile except 0 as obstructed. Note that this does nothing unless obstructable is true, however!
        // For the player entity, tile_obstruction is ignored and overridden by VERGE.game's player_tile_obstruction.
        public bool obstructing, obstructable, tile_obstruction;

        public virtual bool moving { // true if the entity is walking.
            get { return _moving; }
        }
        protected bool _moving;

        public virtual bool pushing {
            get { return ( time_pushing > 0 ); }
        }
        public float time_pushing; // length of time the entity has been pushing against an obs, in ticks
        public FollowerChain follow; 

        public String move_animation_prefix, idle_animation_prefix; // used to determine which animations to load. Generally "Walk " and "Idle ".

        public int speed { // Speed in pixels per second. For entities, this also determines animation rate, as per VERGE.
            get { return _speed; }
            set {
                _speed = value;
                rate = ( (float)value ) / DEFAULT_SPEED;
            }
        }

        protected int _speed;

        // Strips path data from asset names, because they're generally incorrect legacy paths baked into .MAP files.
        // To specify a path to search for CHR files, edit Sprite.CHR_LOCATION.
        public static String _clean_name(String filename) {
            if (System.IO.Path.GetExtension(filename).ToLower() != ".json") // assume a CHR asset
                filename = System.IO.Path.GetFileName(filename); 
            return filename;
        }

        public Entity(String asset_or_json_name, String ent_name)
            : base( _clean_name(asset_or_json_name), "Idle Down" ) {
            _facing = Direction.Down;
            obstructing = false;
            obstructable = true;
            tile_obstruction = true;
            autoface = true;
            visible = true;
            follow = null;
            name = ent_name;

            index = -1; // it's up to the map to maintain this
            move_animation_prefix = "Walk ";
            idle_animation_prefix = "Idle ";
            initialize_movement_attributes();
            // Entities come from chrs, so they all have walk and idle animations defined.
        }

        public Entity(String asset_name) : this( asset_name, asset_name ) { }        
        
        public virtual void idle() { set_animation( idle_animation_prefix + facing ); }
        public virtual void walk() { set_animation( move_animation_prefix + facing ); }

        public virtual void set_walk_state( bool val ) {
            _moving = val;
            if( val ) walk();
            else idle();
        }

        // Set the script based on the given string. If no string is given, resets the script to the value stored in
        // script_name, which is generally what it was set to in the map, or nothing for entities spawned later.
        // Note that the script delegate is freely accessible, so it can also be set by hand.
        public void set_script() { set_script( script_name ); } // set using original name
        public void set_script( String act_script_name ) {
            act_script = VERGEGame.game.script<EntityActivationDelegate>( act_script_name );
            if( act_script == null && !String.IsNullOrEmpty( act_script_name ) )
                System.Diagnostics.Debug.WriteLine( "DEBUG: Couldn't find a \"" + act_script_name + "\" EntityActivationDelegate for entity #" + index +
                    " (" + name + "). Defaulting to a null script." );
        }

        public void activate() {
            if( act_script == null ) return;
            else act_script( this );
        }

        // Returns the pixel just ahead of the entity's front side, aligned with the center of that side. The "front" side is taken to be whatever
        // side the entity is facing, irrespective of its movement direction.
        public Point facing_coordinates( bool include_diagonals ) {
            Point signs = Utility.signs_from_direction( facing, true );
            return new Point( hitbox.X + signs.X + ( hitbox.Width + signs.X * hitbox.Width ) / 2, hitbox.Y + signs.Y + ( hitbox.Height + signs.Y * hitbox.Height ) / 2 );
        }


        public override void Update() {
            // This function, like the movement handlers, mostly works with "speed-adjusted" rather than real time.
            // A unit difference in the time variables it employs corresponds to a difference of 1/Entity.speed ticks.
            int elapsed;
            float time_factor;
            double root1, root2;
            Vector2 velocity_change;
            EntityMovementData data = new EntityMovementData();

            if (!visible) {
                last_logic_tick = VERGEGame.game.tick;
                return;
            }

            data.time = ( VERGEGame.game.tick - last_logic_tick ) * speed;
            data.time_shortfall = 0;
            data.first_call = true;
            data.collided = false;
            data.collided_entity = null;
            data.actual_path = data.attempted_path = Vector2.Zero;
            data.starting_point = _exact_pos;
            time_factor = 0f;

            // Now for the gross part. Maybe this should be moved to another function?
            while( data.time > 0 ) {
                data.time_shortfall = elapsed = data.time - handler( this, move_state, ref data );
                time_factor = ( (float)elapsed ) / speed; // time passed this step, in ticks, as a float.
                velocity_change = time_factor * acceleration;
                data.actual_path = data.attempted_path = time_factor * ( velocity + velocity_change / 2 ); // cur_pos = old_pos + v*t + a*(t^2)/2

                if( this.obstructable ) {
                    if( data.attempted_path != Vector2.Zero ) {
                        data.collided = false;
                        data.collided_entity = null;
                        try_to_move( ref data ); // maybe truncate actual_path
                        if( data.collided ) {
                            // If the entity was interrupted by a collision, "give back" some of its spent time                        
                            if( data.actual_path == Vector2.Zero ) {
                                elapsed = 0;
                                velocity_change = Vector2.Zero;
                            }
                                // If acceleration is 0, we can just interpolate.
                            else if( acceleration == Vector2.Zero ) {
                                // we only need to look at one dimension, so we'll take the bigger one
                                if( Math.Abs( data.actual_path.X ) > Math.Abs( data.actual_path.Y ) )
                                    elapsed = (int)Math.Ceiling( elapsed * data.actual_path.X / data.attempted_path.X );
                                else
                                    elapsed = (int)Math.Ceiling( elapsed * data.actual_path.Y / data.attempted_path.Y );
                            } else {
                                // Okay, hunker down because this is the gross part. When acceleration is a nonzero 
                                // constant, we can't simply interpolate but must instead solve a quadratic equation
                                // in the elapsed time: new_pos = A/2*t^2 + V*t + old_pos. The constraints of the 
                                // scenario ensure that there is at least one positive root, but there could also be 
                                // two, in which case we want the smallest positive root, which represents the earliest
                                // time in the future that the obstruction will be hit.

                                // we only need to look at one dimension, so we'll take the bigger one
                                if( Math.Abs( data.actual_path.X ) > Math.Abs( data.actual_path.Y ) ) {
                                    root1 = Math.Sqrt( velocity.X * velocity.X + 2 * acceleration.X * data.actual_path.X );
                                    root2 = ( -velocity.X + root1 ) / acceleration.X;
                                    root1 = ( -velocity.X - root1 ) / acceleration.X;
                                } else {
                                    root1 = Math.Sqrt( velocity.Y * velocity.Y + 2 * acceleration.Y * data.actual_path.Y );
                                    root2 = ( -velocity.Y + root1 ) / acceleration.Y;
                                    root1 = ( -velocity.Y - root1 ) / acceleration.Y;
                                }
                                // Set root1 to the root we actually want (smallest positive).
                                if( root1 < 0 || root1 > root2 && root2 > 0 ) root1 = root2;

                                elapsed = (int)Math.Ceiling( root1 * speed );
                                velocity_change = ( (float)elapsed ) / speed * acceleration;
                            }
                        }
                    } else if( elapsed < data.time ) {
                        data.collided = false;
                        data.collided_entity = null;
                    }
                }
                if( data.actual_path != Vector2.Zero ) {
                    exact_x += data.actual_path.X;
                    exact_y += data.actual_path.Y;
                }

                velocity += velocity_change;
                data.time_shortfall -= elapsed;
                data.time -= elapsed;
                data.first_call = false;
            }

            if( data.collided ) {
                time_pushing += time_factor;
                // Pushing-related triggers happen here.
                if( movestring != null ) {
                    if( movestring.timeout != Movestring.NEVER_TIMEOUT && time_pushing > movestring.timeout )
                        movestring.do_timeout();
                }
            } else time_pushing = 0f;

            VERGEGame.game.entity_space.Update( this );
            last_logic_tick = VERGEGame.game.tick;
        }

        // Draws the entity. This can be used to blit the entity at weird times (for instance, during a render script), but it's mainly used
        // for standard entity blitting. The elaborate y-sorting term will be ignored if you draw outside the entity render phase.

        // Oh god, what's all this about? For XNA to handle the y-sorting, we need to map y-values to a float ranging from 0 to 1. 
        // We can divide the pixel coordinate by the range of plausible y-values, but this leads to floating-point flicker when 
        // entities have the same y-value and overlap. 
        // Thus, a fractional offset based on the number of entities (to ensure uniqueness) is added to the sort depth.
        protected virtual float _ysort_value() {
            VERGEGame game = VERGEGame.game;
            return (((float)foot - game.entity_space.bounds.Y) * game.map.num_entities - index) / (game.entity_space.bounds.Height * game.map.num_entities);
        }

        public override void Draw(int frame) {            
            Point center;
            Rectangle ad_hoc_dest;
            if (angle == 0f)
                VERGEGame.game.spritebatch.Draw(basis.image, destination, basis.frame_box[frame], Color.White, 0, Vector2.Zero, SpriteEffects.None, _ysort_value());
            else {
                center = hitbox.Center;
                center.X -= destination.X;
                center.Y -= destination.Y;
                ad_hoc_dest = destination;
                ad_hoc_dest.Offset(center.X, center.Y);
                VERGEGame.game.spritebatch.Draw(basis.image, ad_hoc_dest, basis.frame_box[frame], Color.White, angle, new Vector2((float)center.X, (float)center.Y), SpriteEffects.None, _ysort_value());
            }
        }

        public override void DrawAt(int px, int py, int frame) {            
            Point center;
            Rectangle ad_hoc_dest = destination;
            if (angle == 0f) {
                ad_hoc_dest.Location = new Point(px + destination.X - hitbox.X, py + destination.Y - hitbox.Y);            
                VERGEGame.game.spritebatch.Draw(basis.image, ad_hoc_dest, basis.frame_box[frame], Color.White);
            }
            else {
                center = hitbox.Center;
                center.X -= destination.X;
                center.Y -= destination.Y;
                ad_hoc_dest.Location = new Point(px + destination.X - hitbox.X + center.X, py + destination.Y - hitbox.Y + center.Y); 
                VERGEGame.game.spritebatch.Draw(basis.image, ad_hoc_dest, basis.frame_box[frame], Color.White, angle, new Vector2((float)center.X, (float)center.Y), SpriteEffects.None, 1.0f);
            }
        }
    }
}

