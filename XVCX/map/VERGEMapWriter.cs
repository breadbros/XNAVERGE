using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using XNAVERGE;

using TWrite = XVCX.ProcessedMap;

namespace XVCX {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class VERGEMapWriter : ContentTypeWriter<TWrite> {
        protected override void Write(ContentWriter output, TWrite value) {
            output.Write(value.name); // string
            output.Write(value.version); 
            output.Write(value.num_layers); 
            output.Write(value.num_zones);
            output.Write(value.num_ents); 
            output.Write(value.initscript); // string
            output.Write(value.music); // string
            output.Write(value.vsp); // string
            output.Write(value.renderstring); // string
            output.Write(value.start_x);
            output.Write(value.start_y);
            for (int i = 0; i < value.num_layers; i++) write_layer(output, value.layers[i], true);
            write_layer(output, value.obslayer, false);
            write_layer(output, value.zonelayer, false);
            for (int i = 0; i < value.num_zones; i++) write_zone(output, value.zones[i]);
            for (int i = 0; i < value.num_ents; i++) write_ent(output, value.entities[i]);
        }

        private void write_layer(ContentWriter output, ProcessedLayer layer, bool tile_layer) {
            output.Write(layer.name); // string
            output.Write(layer.w);
            output.Write(layer.h);
            if (tile_layer) {
                output.Write(layer.parallax); // vector2                 
                output.Write(layer.alpha); // double
            }
            for (int i = 0; i < layer.tiles.Length; i++) output.Write(layer.tiles[i]);
        }

        private void write_zone(ContentWriter output, Zone zone) {
            output.Write(zone.name); // string
            output.Write(zone._script); // string
            output.Write(zone.chance); // double
            output.Write(zone.adjacent); // bool
        }

        private void write_ent(ContentWriter output, ProcessedEntity ent) {
            output.Write(ent.name); // string
            output.Write(ent.chr); // string
            output.Write(ent.start.X);
            output.Write(ent.start.Y);
            output.Write(ent.actscript); // string
            output.Write(ent.speed);
            output.Write(ent.facing); 
            output.Write(ent.autoface); // bool
            output.Write(ent.obstructs); // bool
            output.Write(ent.obstructable); // bool
            output.Write(ent.movestring); // string
            output.Write(ent.movemode);
            output.Write(ent.delay);
            output.Write(ent.wander_ul.X);
            output.Write(ent.wander_ul.Y);
            output.Write(ent.wander_lr.X);
            output.Write(ent.wander_lr.Y);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) {
            return "XNAVERGE.Content.VERGEMapReader, XNAVERGE";
        }
    }
}
