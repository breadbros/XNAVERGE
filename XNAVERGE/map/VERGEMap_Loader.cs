using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public partial class VERGEMap {


        public VERGEMap(String mapname, int ver, int numlayers, int numzones, int numents) {
            name = mapname;
            version = ver;
            _num_layers = numlayers;
            _num_zones = numzones;
            _num_entities = numents;
        }

        // there used to be a lot more stuff here!
    }


}
