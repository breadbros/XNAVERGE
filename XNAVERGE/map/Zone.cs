using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAVERGE {
    public class Zone {
        public String name;        
        public ZoneDelegate script;
        protected readonly String script_name;
        public double chance; // activation chance for non-adjacent activation (between 0 and 1)
        public bool adjacent; // true if the zone can be triggered by standing next to it and hitting the confirm/use button

        public Zone(String zone_name, String act_script, double act_chance, bool adj_act) {
            name = zone_name;
            script_name = act_script;
            chance = act_chance;
            adjacent = adj_act;
            script = null;
        }
        public Zone(String zone_name) : this(zone_name, String.Empty, 1.0, false) { }
        public Zone(String zone_name, String act_script) : this(zone_name, act_script, 1.0, false) { }
        public Zone(String zone_name, String act_script, double act_chance) : this(zone_name, act_script, act_chance, false) { }

        // Set the script based on the given string. If no string is given, resets the script to the value the zone was 
        // initialized with. Even this usage may result in a different script, though, if the ScriptBanks have changed.
        // Note that the script delegate is freely accessible, so it can also be set by hand.
        public void set_script() { set_script(script_name); } // set using original name
        public void set_script(String act_script) {
            script = VERGEGame.game.script<ZoneDelegate>(act_script);
            if (script == null && !String.IsNullOrEmpty(act_script))
                System.Diagnostics.Debug.WriteLine("DEBUG: Couldn't find a \"" + act_script + "\" ZoneDelegate for the " + name +
                    " zone. Defaulting to a null script.");
        }

        // This function rolls against the random activation chance and calls the activation script if appropriate.
        // It returns true if it activated (even if there is no actual script to be called).
        public bool maybe_activate(int tx, int ty) {            
            if (chance >= 1.0) {
                activate(tx, ty, false);
                return true;
            }
            if (chance <= 0.0) return false;
            if (VERGEGame.rand.NextDouble() < chance) {
                activate(tx, ty, false);
                return true;
            }
            return false;
        }

        public void activate(int tx, int ty, bool by_adj) {
            if (script != null) script(tx, ty, by_adj);            
        }
    }
}
