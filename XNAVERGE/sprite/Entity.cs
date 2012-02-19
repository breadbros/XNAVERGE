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
        

        public Entity(SpriteBasis _basis, String ent_name) : base(_basis, "Idle Down") {
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
        public Entity(SpriteBasis _basis) : this(_basis, "") { }
        public Entity(String asset_name, String ent_name) : this(VERGEGame.game.MapContent.Load<SpriteBasis>(asset_name), ent_name) { }
        public Entity(String asset_name) : this(asset_name, asset_name) { }

        // Attempt to load an entity by inferring its asset name from its chr filename.
        // First checks if the filename matches an asset exactly, then if the filename
        // sans extension matches, then if the entity name matches (if one is given).
        public static Entity load_from_chr_filename(String filename) { return Entity.load_from_chr_filename(filename, ""); }
        public static Entity load_from_chr_filename(String filename, String ent_name) {
            int pos;
            SpriteBasis spr = null;
            filename = Utility.strip_path(filename);
            try { // there doesn't seem to be a way to check if content exists without trying to load it, so let's do that
                spr = VERGEGame.game.MapContent.Load<SpriteBasis>(filename);
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException e) {
                // OK, the filename doesn't correspond to an asset name. Let's try it without the extension
                try {
                    pos = filename.LastIndexOf(".");
                    if (pos < 0) throw e;
                    spr = VERGEGame.game.MapContent.Load<SpriteBasis>(filename.Substring(0, pos));
                }
                catch (Microsoft.Xna.Framework.Content.ContentLoadException) { // That didn't work either. Check for a default tileset to use.
                    if (!String.IsNullOrEmpty(ent_name)) {
                        try {
                            spr = VERGEGame.game.MapContent.Load<SpriteBasis>(ent_name);
                        }
                        catch (Microsoft.Xna.Framework.Content.ContentLoadException) {
                            throw new ArgumentException("Couldn't find a sprite asset named " + filename +
                                ", with or without extension, or one matching the entity name \"" + ent_name + "\".");
                        }
                    }
                    else {
                            throw new ArgumentException("Couldn't find a sprite asset named " + filename +
                                ", with or without extension.");
                    }
                }
            }

            if (String.IsNullOrEmpty(ent_name)) return new Entity(spr, filename);
            else return new Entity(spr, ent_name);
        }

        public virtual void idle() { set_animation(idle_animation_prefix + facing); }
        public virtual void walk() { set_animation(move_animation_prefix + facing); }
        
        public virtual void set_walk_state(bool val) {
            _moving = val;
            if (val) walk();
            else idle();
        }

        // Set the script based on the given string. If no string is given, resets the script to the value stored in
        // script_name, which is generally what it was set to in the map, or nothing for entities spawned later.
        // Note that the script delegate is freely accessible, so it can also be set by hand.
        public void set_script() { set_script(script_name); } // set using original name
        public void set_script(String act_script_name) {
            act_script = VERGEGame.game.script<EntityActivationDelegate>(act_script_name);
            if (act_script == null && !String.IsNullOrEmpty(act_script_name))
                System.Diagnostics.Debug.WriteLine("DEBUG: Couldn't find a \"" + act_script_name + "\" EntityActivationDelegate for entity #" + index +
                    " (" + name + "). Defaulting to a null script.");
        }

        public void activate() {            
            if (act_script == null) return;
            else act_script(this);
        }

        // Returns the pixel just ahead of the entity's front side, aligned with the center of that side. The "front" side is taken to be whatever
        // side the entity is facing, irrespective of its movement direction.
        public Point facing_coordinates(bool include_diagonals) {            
            Point signs = Utility.signs_from_direction(facing, true);
            return new Point( hitbox.X + signs.X + (hitbox.Width + signs.X*hitbox.Width)/2, hitbox.Y + signs.Y + (hitbox.Height + signs.Y*hitbox.Height)/2 );
        }


        public override void handle_movement() {
            float elapsed = (float)(VERGEGame.game.tick - last_logic_tick);
            Vector2 velocity_change, path;
            velocity_change = elapsed * acceleration;
            path = elapsed * (velocity + velocity_change / 2); // s = A(t^2)/2 + Vt + s_old, assuming constant A            
            velocity += velocity_change;            
            if (false && this == VERGEGame.game.player) {
                path = try_to_move(path);                
            }
            _exact_pos += path;
        }

        // Draws the entity. This can be used to blit the entity at weird times (for instance, during a render script), but it's mainly used
        // for standard entity blitting. The elaborate y-sorting term will be ignored if you draw outside the entity render phase.
        public override void Draw() {
            VERGEGame game = VERGEGame.game;

            game.spritebatch.Draw(basis.image, destination, basis.frame_box[current_frame], Color.White, 0, Vector2.Zero, SpriteEffects.None, 
                // Oh god, what's all this about? For XNA to handle the y-sorting, we need to map y-values to a float ranging from 0 to 1. 
                // We can divide the pixel coordinate by the range of plausible y-values, but this leads to floating-point flicker when 
                // entities have the same y-value and overlap. 
                // Thus, a fractional offset based on the number of entities (to ensure uniqueness) is added to the sort depth.
                (((float)foot - game.entity_space.bounds.X)*game.map.num_entities - index) / (game.entity_space.bounds.Height*game.map.num_entities)
            );
        }


    }

}

