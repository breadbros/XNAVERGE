using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // This segment of the class contains the main map functionality. The grody .MAP file loading code is segregated in VERGEMap_Loader.cs.
    public partial class VERGEMap {
        public const int STARTING_ENTITY_ARRAY_SIZE = 64; // the default size of the entity array. If you exceed this it will initiate an array copy, so set it as low as is practical.
        public const String SCRIPT_CLASS_PREFIX = "Script_"; // prefixed to asset names when searching for mapscript classes

        public static readonly Vector2 NEUTRAL_PARALLAX = new Vector2(1.0f);
        public static readonly Vector2 FIXED_PARALLAX = new Vector2(0.0f);

        // when true, an exception is thrown on an illegal tile index. When false, loads them as 0. 
        // This is false by default because some versions of maped3 have a bug that occasionally
        // saves unobstructed tiles with illegal values, and there are many such maps "in the wild".        
        public static bool strict_tile_loading = false;

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
        
        public MapScriptBank scripts;

        public VERGEMap(String mapname, int ver, int numlayers, int numzones, int numents) {
            name = mapname;
            version = ver;
            _num_layers = numlayers;
            _num_zones = numzones;
            _num_entities = numents;

            tiles = new TileLayer[numlayers];
            zones = new Zone[num_zones + 2]; // the +2 gives a bit of room for expansion before the array needs to be expanded
            if (numents <= VERGEMap.STARTING_ENTITY_ARRAY_SIZE) entities = new Entity[VERGEMap.STARTING_ENTITY_ARRAY_SIZE];
            else entities = new Entity[numents + 2];
        }

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
            VERGEGame.game.entity_space.Add(ent);
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
            VERGEGame.game.entity_space.Remove(entity);
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
            BoundedSpace<Entity>.BoundedElementSet ent_enum;//VERGEGame.game.entity_space.elements_within_bounds
            Entity collider;
            Rectangle collision_zone;
            bool tile_based;
            int leading_x, leading_y, pixel_distance, best_distance;
            pixel_distance = 1 + (intended_distance - 1) / 100;
            leading_x = ent.hitbox.X + (ent.hitbox.Width - 1 + xs * (ent.hitbox.Width + 1)) / 2; // one pixel beyond the leading sides
            leading_y = ent.hitbox.Y + (ent.hitbox.Height - 1 + ys * (ent.hitbox.Height + 1)) / 2;

            best_distance = _collision_pixel_tester(pixel_distance, xs, ys, leading_x, leading_y, ent); // haaaaack

            collision_zone = ent.hitbox;
            if (xs != 0) {
                collision_zone.Width += pixel_distance;
                if (xs < 0) collision_zone.X -= pixel_distance;
            }
            if (ys != 0) {
                collision_zone.Height += pixel_distance;
                if (ys < 0) collision_zone.Y -= pixel_distance;
            }

            ent_enum = VERGEGame.game.entity_space.elements_within_bounds(collision_zone, true, ent);
            
            while (ent_enum.GetNext(out collider)) {
                if (collider.obstructing) {
                    // this is the worst hack. TODO: burn everything.
                    collision_zone = ent.hitbox; 
                    for (int cur_dist = 0; cur_dist < best_distance; cur_dist++) {
                        collision_zone.Offset(xs, ys);
                        if (collision_zone.Intersects(collider.hitbox)) best_distance = cur_dist;
                    }
                }
            }

            
            if (best_distance == pixel_distance) return intended_distance; // as many pixels as possible, plus however much more
            return best_distance; // exactly as many pixels as possible
        }

        protected virtual int _collision_pixel_tester(int pixel_distance, int xs, int ys, int leading_x, int leading_y, Entity ent) {
            for (int cur_dist = 0; cur_dist < pixel_distance; cur_dist++) {
                if (xs != 0) { // Check for obstructions along the horizontal axis
                    for (int y = ent.hitbox.Top; y < ent.hitbox.Bottom; y++) {
                        if (obs_at_pixel(leading_x, y)) return cur_dist * 100;
                    }
                }
                if (ys != 0) { // Check for obstructions along the vertical axis
                    for (int x = ent.hitbox.Left; x < ent.hitbox.Right; x++) {
                        if (obs_at_pixel(x, leading_y)) return cur_dist * 100;
                    }
                }

                leading_x += xs;
                leading_y += ys;
            }
            return pixel_distance;
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

        // returns true if the x, y pair is within the defined map region. If tile_coordinates is true,
        // x and y are taken to be tiles, otherwise they're taken to be pixels.
        public bool within_bounds(int x, int y, bool tile_coordinates) {
            if (tile_coordinates) {
                if (x < 0 || y < 0 || x >= width || y >= height) return false;
                else return true;
            }
            else {
                if (x < 0 || y < 0 || x >= pixel_width || y >= pixel_height) return false;
                else return true;
            }
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

