using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = XNAVERGE.VERGEMap;

namespace XNAVERGE.content {
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
            for (int i = 0; i < map.num_layers; i++) map.tiles[i] = read_layer(input, true);
            map.obstruction_layer = read_layer(input, false);
            map.zone_layer = read_layer(input, false);
            map.zones = new Zone[map.num_zones + 2]; // the +2 gives a bit of room for expansion before the array needs to be expanded
            for (int i = 0; i < map.num_zones; i++) map.zones[i] = read_zone(input);
            


            // probably need to get the actual tileset here

            // entity definitions
            return map;
        }

        private TileLayer read_layer(ContentReader input, bool tile_layer) {
            int w, h;
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
                for (int x = 0; x < w; x++)
                    layer.data[x][y] = input.ReadInt32();                
            }
            return layer;
        }

        private Zone read_zone(ContentReader input) {
            return new Zone(input.ReadString(), input.ReadString(), input.ReadDouble(), input.ReadBoolean());
        }

        private Entity read_ent(ContentReader input, VERGEMap map) {
            Entity ent;
            String name, chr, movestring;
            int x, y;
            name = input.ReadString();
            chr = input.ReadString();
            ent = new Entity("Content\\raw\\" + chr, name); // haaaack
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
            // movemode (int, cast to WanderMode)
            // wander delay (int)
            // wander x1, y1, x2, y2 (four ints)

            return ent;
        }
    }
}
