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
    }
}
