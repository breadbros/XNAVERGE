using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAVERGE {
    public class Zone {
        public String name;
        public String script; // activation script
        public double chance; // activation chance for non-adjacent activation (between 0 and 1)
        public bool adjacent; // true if the zone can be triggered by standing next to it and hitting the confirm/use button

        public Zone(String zone_name, String act_script, double act_chance, bool adj_act) {
            name = zone_name;
            script = act_script;
            chance = act_chance;
            adjacent = adj_act;
        }
        public Zone(String zone_name) : this(zone_name, String.Empty, 1.0, false) { }
        public Zone(String zone_name, String act_script) : this(zone_name, act_script, 1.0, false) { }
        public Zone(String zone_name, String act_script, double act_chance) : this(zone_name, act_script, act_chance, false) { }

        // This function rolls against the random activation chance and calls the activation script if appropriate.
        // It returns true if it activated.
        public bool maybe_activate() {
            if (chance >= 1.0) {
                // activate here
                return true;
            }
            if (chance <= 0.0) return false;
            if (VERGEGame.rand.NextDouble() < chance) {
                // activate here
                return true;
            }
            return false;
        }

        public void activate() {
            // Call the script immediately. TODO: make this work.
        }
    }
}
