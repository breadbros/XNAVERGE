using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sully {
    // This class holds save headers so that they can be browsed in the UI.
    public class SaveHeader {
        protected SaveManager manager;

        public int save_idx;        
        public TimeSpan playtime;
        public Texture2D screencap;
        public List<String> party; // should this include level?
        public String location; // this can be used for a location name associated with the save

        public SaveHeader(SaveManager mgr, int idx) {
            manager = mgr;
            save_idx = idx;
            party = new List<String>();
        }
    }
}
