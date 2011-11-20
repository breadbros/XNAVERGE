using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = XNAVERGE.VERGEMap;

namespace XNAVERGE.Content {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class VERGEMapReader : ContentTypeReader<TRead> {
        protected override TRead Read(ContentReader input, TRead nobody_seems_to_know_what_this_argument_is_for) {
            String vsp, rstring;
            VERGEMap map = new VERGEMap(input.ReadString(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
            map.initscript = input.ReadString();
            map.default_music = input.ReadString();
            vsp = input.ReadString();
            rstring = input.ReadString();
            map.start_x = input.ReadInt32();
            map.start_y = input.ReadInt32();
            
            map.tiles = new TileLayer[map.num_layers];
            map.load_tileset(vsp);
            for (int i = 0; i < map.num_layers; i++) map.tiles[i] = read_layer(input, true, map.tileset.num_tiles);
            
            map.obstruction_layer = read_layer(input, false, map.tileset.num_obs_tiles);
            
            map.zone_layer = read_layer(input, false, map.num_zones);
            map.zones = new Zone[map.num_zones + 2]; // the +2 gives a bit of room for expansion before the array needs to be expanded
            for (int i = 0; i < map.num_zones; i++) map.zones[i] = read_zone(input);
            
            if (map.num_entities <= VERGEMap.STARTING_ENTITY_ARRAY_SIZE) map.entities = new Entity[VERGEMap.STARTING_ENTITY_ARRAY_SIZE];
            else map.entities = new Entity[map.num_entities + 2]; // As with zones, the +2 gives some room for low-cost expansion
            for (int i = 0; i < map.num_entities; i++) map.entities[i] = read_ent(input, map);
            
            map.renderstack = new RenderStack(map, rstring);

            return map;
        }

        private TileLayer read_layer(ContentReader input, bool tile_layer, int num_tiles) {
            int w, h, cur;
            String name;            
            name = input.ReadString();
            w = input.ReadInt32();
            h = input.ReadInt32();
            TileLayer layer = new TileLayer(w, h, name);
            if (tile_layer) { // layer holds art tiles (as opposed to obstruction tiles or zones)
                layer.parallax = input.ReadVector2();
                layer.alpha = input.ReadDouble();
            }
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    cur = input.ReadInt32();
                    if (cur < 0 || cur >= num_tiles) { // illegal tile index
                        if (VERGEMap.STRICT_TILE_LOADING) throw new InvalidTileIndexException(x, y, name, cur, num_tiles);
                        else layer.data[x][y] = 0; // *whistles nonchalantly*
                    }
                    else layer.data[x][y] = cur;
                }
            }
            return layer;
        }

        private Zone read_zone(ContentReader input) {
            return new Zone(input.ReadString(), input.ReadString(), input.ReadDouble(), input.ReadBoolean());
        }

        private Entity read_ent(ContentReader input, VERGEMap map) {
            Entity ent;
            String name, chr, movestring;            
            name = input.ReadString();
            chr = input.ReadString();
            ent = Entity.load_from_chr_filename(chr, name);
            ent.x = input.ReadInt32() * map.tileset.tilesize;
            ent.y = input.ReadInt32() * map.tileset.tilesize;
            ent.on_activation = input.ReadString();
            ent.speed = input.ReadInt32();
            ent.facing = (Direction)input.ReadInt32();
            ent.autoface = input.ReadBoolean();
            ent.obstructing = input.ReadBoolean();
            ent.obstructable = input.ReadBoolean();
            movestring = input.ReadString();
            // unimplemented stuff:
            input.ReadInt32(); // movemode (int, cast to WanderMode)
            input.ReadInt32(); // wander delay (int)
            input.ReadInt32(); input.ReadInt32(); input.ReadInt32(); input.ReadInt32(); // wander x1, y1, x2, y2 (four ints)
            
            ent.set_movestring(movestring);
            return ent;
        }        

    }

    public class InvalidTileIndexException : Exception {
        public InvalidTileIndexException(int x, int y, String name, int tile_idx, int num_tiles) :
            base("Invalid tile index at (" + x + ", " + y + ") of layer \"" + name + "\". The tile has index " +
                tile_idx + ", but this layer requires values between 0 and " + (num_tiles - 1).ToString() +
                ".\n\nThis usually means that you are either loading a nonstandard tileset or the map file has been" +
                " corrupted.\n\nMAPED3 is known to corrupt isolated tiles here and there; in that case you will need" +
                " to either fix the problem manually, or set VERGEMap.STRICT_TILE_LOADING to false.") { }
    }
}
