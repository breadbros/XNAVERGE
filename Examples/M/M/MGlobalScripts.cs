using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace M {
    public class MGlobalScripts : ScriptBank {
        public void testing() {
            Console.WriteLine("MGlobalScripts.testing() was called.");
        }
        
    }

    public class Script_town01 : MapScriptBank {

        public Script_town01(VERGEMap map) : base(map) { }

        public void zonetrigger(int x, int y, bool adj) {
            Console.WriteLine("({0}, {1})", x, y);
        }

        public void door_1(int x, int y, bool adj) {
            if (adj) Console.WriteLine("This is a door. You activated it adjacently.");
            else Console.WriteLine("This is a door. You activated it by stepping on it.");
        }
        public void door_2(int x, int y, bool adj) { door_1(x, y, adj); }
        public void door_3(int x, int y, bool adj) { door_1(x, y, adj); }
        public void door_4(int x, int y, bool adj) { door_1(x, y, adj); }

        public void t1_southern_exit(int x, int y, bool adj) {
            Console.WriteLine("right now this should not be triggering!");
        }

        public void secret_exit(int x, int y, bool adj) {
            Console.WriteLine("it's a secret to everyone");
        }

        public void queen(Entity ent) {
            Console.WriteLine("queen");
        }
    }
}
