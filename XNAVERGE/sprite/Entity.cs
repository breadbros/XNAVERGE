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
        public String chr_name { get { return _chrfile; } } // TODO: add a setter that loads a new chr
        public String on_activation; // activation script        
        public bool autoface;
        public int index; // Index in the map.entities array. Don't twiddle with it! This is needed for a stupid, stupid reason.

        private string _chrfile;

        // MOBILITY AND ANIMATION ATTRIBUTES
        public virtual Direction facing {
            get { return _facing; }
            set {
                if (_facing == value) return;
                _facing = value;
                if (_moving) walk();
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

        public String move_animation_prefix, idle_animation_prefix; // used to determine which animations to load. Generally "Walk " and "Idle ".

        public int speed { // Speed in pixels per second. For entities, this also determines animation rate, as per VERGE.
            get { return _speed; }
            set {
                _speed = value;
                rate = ((float)value) / DEFAULT_SPEED;
            }
        }

        protected int _speed;
        // Each point of speed translates to 1/100th of a pixel movement per tick, so rather than futzing with doubles         
        

        // TODO: Cache SpriteBasis for reuse
        public Entity(String chr_file, String ent_name) : base(new SpriteBasis(chr_file), "Idle Down") {
            _chrfile = chr_file;            

            _facing = Direction.Down;
            obstructing = false;
            obstructable = true;
            tile_obstruction = true;
            autoface = true;
            visible = true;            
            name = ent_name;
            
            index = -1; // it's up to the map to maintain this            
            move_animation_prefix = "Walk ";
            idle_animation_prefix = "Idle ";
            initialize_movement_attributes(); 
            // Entities come from chrs, so they all have walk and idle animations defined.

        }
        public Entity(String chr_file) : this(chr_file, chr_file) { }

        public virtual void idle() { set_animation(idle_animation_prefix + facing); }
        public virtual void walk() { set_animation(move_animation_prefix + facing); }

        protected virtual void set_walk_state(bool val) {
            _moving = val;
            if (val) walk();
            else idle();
        }


        // Draws the entity. This can be used to blit the entity at weird times (for instance, during a render script), but it's mainly used
        // for standard entity blitting. The elaborate y-sorting term will be ignored if you draw outside the entity render phase.
        public override void Draw() {            
            VERGEGame.game.spritebatch.Draw(basis.image, destination, basis.frame_box[current_frame], Color.White, 0, Vector2.Zero, SpriteEffects.None, 
                // Oh god, what's all this about? For XNA to handle the y-sorting, we need to map y-values to a float ranging from 0 to 1. 
                // We can divide the pixel coordinate by the range of plausible y-values, but this leads to floating-point flicker when 
                // entities have the same y-value and overlap. 
                // Thus, a fractional offset based on the number of entities (to ensure uniqueness) is added to the sort depth.
                (((float)foot + VERGEGame.game.screen.Height)*VERGEGame.game.map.num_entities - index) / (VERGEGame.game.y_range*VERGEGame.game.map.num_entities)
            );
        }


    }

}
