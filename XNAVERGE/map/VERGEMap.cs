using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // This segment of the class contains the main map functionality. The grody .MAP file loading code is segregated in VERGEMap_Loader.cs.
    public partial class VERGEMap {
        // when true, an exception is thrown on an illegal tile index. When false, loads them as 0. 
        // This is false by default because some versions of maped3 have a bug that occasionally
        // saves unobstructed tiles with illegal values, and there are many such maps "in the wild".
        public static bool STRICT_TILE_LOADING = false;

        public const int STARTING_ENTITY_ARRAY_SIZE = 64; // the default size of the entity array. If you exceed this it will initiate an array copy, so set it as low as is practical.

        public static Vector2 NEUTRAL_PARALLAX = new Vector2(1.0f);
        public static Vector2 FIXED_PARALLAX = new Vector2(0.0f);
        

        protected int _num_layers;

        public int start_x, start_y, version;
        public virtual int width { get { return tiles[0].width; } } // width of master layer        
        public virtual int height { get { return tiles[0].height; } } // height of master layer
        public virtual int pixel_width { get { return width * tileset.tilesize; } } // width (in pixels) of master layer (not updated automatically!)
        public virtual int pixel_height { get { return height * tileset.tilesize; } } // height (in pixels) of master layer (not updated automatically!)
        public virtual int num_layers { get { return _num_layers; } }

        public String name, initscript, default_music;
        public RenderStack renderstack;
        public Tileset tileset;
        public TileLayer[] tiles; // these are ordered according to their order in the MAP file, not their rendering order.
        public TileLayer obstruction_layer;
        public TileLayer zone_layer;
        protected static String tileset_override; // when set, the next map to be instantiated uses this tileset, then unsets the variable. Used by switch_map.

        protected int _num_entities;
        public Entity[] entities;
        public int num_entities { get { return _num_entities; } }
            
        protected int _num_zones;
        public Zone[] zones;
        public int num_zones { get { return _num_zones; } }
        


        // builds a new renderstack from a verge-style renderstring. Optionally, you can use a delimiter other than
        // the comma. Remember, layer numbering is 1-offset in the renderstring!
        // Note that you can also manipulate the renderstack directly, if you like.
        public void set_renderstring(String new_renderstring, Char delim) {
            renderstack = new RenderStack(this, new_renderstring, delim);
        }

        public void set_renderstring(String new_renderstring) { set_renderstring(new_renderstring, ','); }

        // ---------------------------------
        // ENTITY MANAGEMENT

        public Entity get_entity(String ent_name) {            
            for (int i = 0; i < _num_entities; i++) {
                if (entities[i].name == ent_name) return entities[i];
            }
            return null;
        }

        // Returns a (possibly empty) list of all entities whose hitboxes intersect with the given rectangle.
        public List<Entity> entities_in_region(Rectangle region) {
            List<Entity> list = new List<Entity>(_num_entities); // initial list capacity = number of entities on map
            for (int i = 0; i < _num_entities; i++) {
                if (entities[i].hitbox.Intersects(region)) list.Add(entities[i]);
            }
            return list;
        }

        
        public Entity spawn_entity(String ent_name, int x_coord, int y_coord, Direction facing, String asset_name, String animation) {
            Entity ent = new Entity(asset_name, ent_name);            
            ent.move_to_tile(x_coord, y_coord);
            ent.facing = facing;            

            // First check for any lazily-deleted entities whose array position can be usurped.
            for (int i = 0; i < _num_entities; i++) {
                if (entities[i].deleted) {
                    entities[i] = ent;
                    ent.index = i;
                    return ent;
                }
            }

            // No free positions, so we'll have to add one. Need we expand the array?
            if (_num_entities >= entities.Length) { 
                Entity[] new_array;
                new_array = new Entity[entities.Length * 2];
                entities.CopyTo(new_array, 0);
                entities = new_array;
            }
            entities[_num_entities] = ent;
            ent.index = _num_entities;
            _num_entities++;
            //Console.WriteLine("{0} is #{1}", ent.name, ent.index);
            return ent;
        }
        // overload ALL the things!
        public Entity spawn_entity(int x_coord, int y_coord, Direction facing, String asset_name, String animation) {
            return spawn_entity("", x_coord, y_coord, facing, asset_name, animation);
        }
        public Entity spawn_entity(int x_coord, int y_coord, String asset_name, String animation) {
            return spawn_entity("", x_coord, y_coord, Direction.Down, asset_name, animation);
        }
        public Entity spawn_entity(int x_coord, int y_coord, Direction facing, String asset_name) {
            return spawn_entity("", x_coord, y_coord, facing, asset_name, String.Empty);
        }
        public Entity spawn_entity(int x_coord, int y_coord, String asset_name) {
            return spawn_entity("", x_coord, y_coord, Direction.Down, asset_name, String.Empty);
        }
        public Entity spawn_entity(String ent_name, int x_coord, int y_coord, String asset_name, String animation) {
            return spawn_entity(ent_name, x_coord, y_coord, Direction.Down, asset_name, animation);
        }
        public Entity spawn_entity(String ent_name, int x_coord, int y_coord, Direction facing, String asset_name) {
            return spawn_entity(ent_name, x_coord, y_coord, facing, asset_name, "");
        }
        public Entity spawn_entity(String ent_name, int x_coord, int y_coord, String asset_name) {
            return spawn_entity(ent_name, x_coord, y_coord, Direction.Down, asset_name, "");
        }

        // Lazy-deletes an entity from the entities array, returning true if successful. 
        // If the entity wasn't in there, does nothing and returns false.
        // If you deleted the player this unsets the "player" variable, but it won't generally
        // clean up other references, and you can't assume the entity won't be GCed eventually.
        public bool delete_entity(Entity entity) {
            for (int i = 0; i < _num_entities; i++) {
                if (entities[i] == entity) {
                    entity.deleted = true;
                    if (VERGEGame.game.player == entity) VERGEGame.game.player = null;
                    return true;
                }
            }
            return false;
        }

        // Given an entity, a direction, and a distance, returns how far the entity can move 
        // in that direction before being obstructed, to a maximum of the distance given.
        // Does not check whether the entity is obstructable beforehand.
        // NOTE: both the distance passed and the distance returned are in hundredths of pixels.
        public int max_unobstructed_distance(int intended_distance, int xs, int ys, Entity ent) {
            VERGEGame game = VERGEGame.game;
            bool tile_based;            
            int cur_x = 0, cur_y = 0, targ_x, targ_y, tilesize, edge_bound; 
            
            if (intended_distance <= 0 || (xs == 0 && ys == 0)) return 0;
            tilesize = tileset.tilesize;            
            if (ent == game.player) tile_based = game.player_tile_obstruction; // player's obstruction type overrides entity's
            else tile_based = ent.tile_obstruction;
            //if (ent == game.player) Console.Write Line(intended_distance);
            //Console.WriteLine("Intended Distance {0}. Expecting at most {1} iterations.", intended_distance, intended_distance / 100);

            if (xs < 0) cur_x = ent.x - 1;
            else if (xs > 0) cur_x = ent.x + ent.hitbox.Width;
            if (ys < 0) cur_y = ent.y - 1;
            else if (ys > 0) cur_y = ent.y + ent.hitbox.Height;

            if (xs == 0) { // vertical movement
                cur_x = ent.x;
                targ_x = ent.x + ent.hitbox.Width - 1;
                targ_y = cur_y + ys * ((intended_distance-1) / 100 + 1); // technically, this is one pixel past the target, to simplify the loop                
                for (int y = cur_y; y != targ_y; y += ys) {                    
                    for (int x = cur_x; x <= targ_x; x++) {
                        if (obs_at_pixel(x, y)) {
                            //Console.WriteLine("Hit at ({0},{1}). Returning value of {2} instead of optimal {3}", x, y, Math.Abs(cur_y - y) - 1, intended_distance);
                            return Math.Abs(cur_y - y) - 1;
                        }
                    }
                }
            }
            else {
                if (ys == 0) { // horizontal movement
                    cur_y = ent.y;
                    targ_y = ent.y + ent.hitbox.Height - 1;
                    targ_x = cur_x + xs * ((intended_distance - 1) / 100 + 1);  // technically, this is one pixel past the target, to simplify the loop                    
                    for (int x = cur_x; x != targ_x; x += xs) {                        
                        for (int y = cur_y; y <= targ_y; y++) {
                            if (obs_at_pixel(x, y)) {
                                //Console.WriteLine("Hit at ({0},{1}). Returning value of {2} instead of optimal {3}", x, y, Math.Abs(cur_x - x) - 1, intended_distance);
                                return Math.Abs(cur_x - x) * 100;
                            }
                        }
                    }
                }
                else { // main diagonal movement
                    // technically, this is one pixel past the target, to simplify the loop
                    targ_x = cur_x + xs * ((intended_distance - 1) / 100 + 1);
                    // We're moving the same distance in both dimensions, so we can just loop over x and update y manually.
                    for (int x = cur_x; x != targ_x; x += xs) {
                        // check the horizontal edge (top or bottom) for collisions
                        edge_bound = x - ent.hitbox.Width * xs;
                        for (int edge_x = x; edge_x != edge_bound; edge_x -= xs) {
                            if (obs_at_pixel(edge_x, cur_y)) { 
                                //Console.WriteLine("Hit at ({0},{1}). Returning value of {2} instead of optimal {3}", x, y, Math.Abs(cur_x - x) - 1, intended_distance);
                                return Math.Abs(cur_x - x) * 100;
                            }
                        }
                        // check the vertical edge (left or right side) for collisions
                        edge_bound = cur_y - ent.hitbox.Height * ys;
                        for (int edge_y = cur_y + ys; edge_y != edge_bound; edge_y -= ys) { // we don't need to recheck (x, cur_y), so start after that one
                            if (obs_at_pixel(x, edge_y)) {
                                //Console.WriteLine("Hit at ({0},{1}). Returning value of {2} instead of optimal {3}", x, y, Math.Abs(cur_x - x) - 1, intended_distance);
                                return Math.Abs(cur_x - x) * 100;
                            }
                        }
                        cur_y += ys; // also update y, since the loop only handles x            
                    }



                }
            }

            return intended_distance;
        }

        // Returns true if the specified pixel is obstructed.
        public bool obs_at_pixel(int x, int y) {
            int tile_x, tile_y, pixel_x, pixel_y, tile, tilesize;
            tilesize = tileset.tilesize;
            if (x < 0 || y < 0 || x >= width*tilesize || y >= height*tilesize) return false;
            tile_x = Utility.DivRem(x, tilesize, out pixel_x); // tile_x = x/tilesize, pixel_x = x%tilesize
            tile_y = Utility.DivRem(y, tilesize, out pixel_y); // as above but for y
            tile = obstruction_layer.data[tile_x][tile_y];
            if (tile == 0) return false;
            return tileset.obs[tile][pixel_x][pixel_y];
        }

        // ---------------------------------
        // ZONE MANAGEMENT
        

        // Creates a new type of zone (which won't exist on the map at the time it's created, of course)
        public Zone create_zone(String name, String script, double chance, bool adj) {
            Zone zone = new Zone(name, script, chance, adj);
            
            if (_num_zones >= zones.Length) {
                Zone[] new_arr = new Zone[zones.Length * 2];
                zones.CopyTo(new_arr, 0);
                zones = new_arr;
            }

            zones[_num_zones] = zone;
            _num_zones++;
            return zone;
        }

        // ---------------------------------
        // MAP HANDLING

        public static Tileset default_tileset { get { return _default_tileset; } }
        protected static Tileset _default_tileset = null;


        public static Tileset set_default_tileset(String asset_name) {
            try {
                _default_tileset = VERGEGame.game.MapContent.Load<Tileset>("asset_name");
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException e) { // not found
                _default_tileset = null;
                throw e;
            }
            return _default_tileset;
        }

        public static void switch_map(String new_map, String tileset_override_name) {
            tileset_override = tileset_override_name;
            switch_map(new_map);
        }        
        public static void switch_map(String new_map) {
            VERGEGame game = VERGEGame.game;
            game.MapContent.Unload();
            game.map = VERGEGame.game.MapContent.Load<VERGEMap>(new_map);
            tileset_override = null;
            game.init_map();
        }

        // Attempts to load the map's tileset. If there is a manual tileset override specified, it looks for
        // that and errors if it's not found. Otherwise, it first checks for any content with a name matching 
        // the map's vsp filename, then tries to match the filename without the .vsp part, then loads the
        // game's default tileset, erroring out if there isn't one.
        public Tileset load_tileset(String filename) {
            int pos;
            Tileset ts = null;            
            if (!String.IsNullOrEmpty(tileset_override)) {
                ts = VERGEGame.game.MapContent.Load<Tileset>(tileset_override);                
            }
            else {
                try { // there doesn't seem to be a way to check if content exists without trying to load it, so let's do that
                    ts = VERGEGame.game.MapContent.Load<Tileset>(filename);
                }
                catch (Microsoft.Xna.Framework.Content.ContentLoadException e) {
                    // OK, the filename doesn't correspond to an asset name. Let's try it without the extension
                    try {
                        pos = filename.LastIndexOf(".");
                        if (pos < 0) throw e;
                        ts = VERGEGame.game.MapContent.Load<Tileset>(filename.Substring(0, pos));
                    }
                    catch (Microsoft.Xna.Framework.Content.ContentLoadException) { // That didn't work either. Check for a default tileset to use.
                        if (VERGEMap._default_tileset != null) {
                            System.Diagnostics.Debug.WriteLine("Couldn't match legacy vsp name to an asset; using the default tileset.");
                            ts = VERGEMap._default_tileset;
                        }
                        else {
                            throw new ArgumentException("Couldn't find a tileset asset named " + filename +
                                ", with or without extension, and there was no default tileset or override given.");
                        }
                    }
                }
            }
            tileset = ts;
            return ts;
        }

    }




    public class Camera {        
        public CameraMode mode;
        public int x, y;
        public Rectangle rect { get { return new Rectangle(x, y, VERGEGame.game.screen.width, VERGEGame.game.screen.height); } }
        public CameraBounds bounds;
        public bool bounded;        
        public Sprite following { 
            get {
                if (mode == CameraMode.Manual) return null;
                if (mode == CameraMode.FollowPlayer) return VERGEGame.game.player;
                return _following;
            } 
        }
        protected Sprite _following;

        public Camera() {
            x = 0;
            y = 0;
            mode = CameraMode.Manual;
            _following = null;
            bounded = true;            
            bounds = new CameraBounds();
            bounds.inherit_from_map();
        }

        public void follow(Entity target) {
            mode = CameraMode.FollowTarget;
            _following = target;
        }

        public void set(int new_x, int new_y) {
            x = new_x;
            y = new_y;
        }

        // Puts the selected point at the center of the screen, if possible. 
        // If the camera bounds prevent it, just get as close as you can.
        public void center_at(Point pt) {
            x = pt.X - VERGEGame.game.screen.width / 2;
            y = pt.Y - VERGEGame.game.screen.height / 2;
        }

        // Centers the camera over the specified sprite's hitbox (subject to bounds; see center_at() for details).
        public void focus_on(Sprite sprite) {
            center_at(sprite.hitbox.Center);
        }

        // Updates the camera coordinates to follow its target sprite. If the camera is in Manual mode, does nothing.         
        public void update() {
            if (following != null) center_at(following.hitbox.Center);
            if (bounded) {
                if (x < bounds.x1) x = bounds.x1;
                else if (x + VERGEGame.game.screen.width > bounds.x2) x = bounds.x2 - VERGEGame.game.screen.width;
                if (y < bounds.y1) y = bounds.y1;
                else if (y + VERGEGame.game.screen.height > bounds.y2) y = bounds.y2 - VERGEGame.game.screen.height;
            }

        }
    }


    // Represents the bounds of the camera when Camera.bounded is true. Bounds have to be given for all four sides, 
    // but if you want to bound specific sides only, you can just set the others really huge.
    // If camera is outside the bounds when bounded is set to true, it will "jump" inside. If the screen is too
    // small to respect the bounds, it will be aligned to the upper-left corner of the bounds.
    public class CameraBounds {
        public int x1 {
            get { return bound_rect.X; }
            set {
                if (value >= x2) throw new ArgumentOutOfRangeException("x1", "Can't set lefthand bound (" + value + ") greater than or equal to righthand bound (" + x2 + ").");
                bound_rect.X = value; 
            }
        }
        public int y1 {
            get { return bound_rect.Y; }
            set {
                if (value >= y2) throw new ArgumentOutOfRangeException("y1", "Can't set upper bound (" + value + ") greater than or equal to lower bound (" + y2 + ").");
                bound_rect.Y = value;
            }
        }
        public int x2 {
            get { return bound_rect.X + bound_rect.Width; }
            set {
                if (value <= x1) throw new ArgumentOutOfRangeException("x2", "Can't set righthand bound (" + value + ") less than or equal to lefthand bound (" + x1 + ").");
                bound_rect.Width = value - bound_rect.X;
            }
        }
        public int y2 {
            get { return bound_rect.Y + bound_rect.Height; }
            set {
                if (value <= y1) throw new ArgumentOutOfRangeException("y2", "Can't set lower bound (" + value + ") less than or equal to upper bound (" + y1 + ").");
                bound_rect.Height = value - bound_rect.Y;
            }
        }
        public int width {
            get { return bound_rect.Width; }
        }
        public int height {
            get { return bound_rect.Height; }
        }
        protected Rectangle bound_rect;

        public CameraBounds() {
            bound_rect = new Rectangle(Int32.MinValue/2, Int32.MinValue/2, Int32.MaxValue, Int32.MaxValue);
        }

        public void set(int x1c, int y1c, int x2c, int y2c) {
            if (x2c <= x1c) throw new ArgumentException("Right-hand bound (" + x2c + ") is less than or equal to left-hand bound (" + x1c + ").");
            if (y2c <= y1c) throw new ArgumentException("Lower bound (" + y2c + ") is less than or equal to upper bound (" + y1c + ").");
            bound_rect = new Rectangle(x1c, y1c, x2c - x1c, y2c - y1c);
        }

        public void shift(int x_shift, int y_shift) {
            bound_rect.X += x_shift;
            bound_rect.Y += y_shift;
        }

        public Rectangle as_rect() {
            return bound_rect;
        }

        // Sets the bounds to the absolute bounds of the current map.
        public void inherit_from_map() {
            bound_rect = new Rectangle(0, 0, VERGEGame.game.map.width * VERGEGame.game.map.tileset.tilesize - 1, VERGEGame.game.map.height * VERGEGame.game.map.tileset.tilesize - 1); 
        }
    }

    public enum CameraMode { Manual, FollowPlayer, FollowTarget };
}

