﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sully {
    public class SullyGlobalScripts : ScriptBank {
        SullyGame sg;
        public SullyGlobalScripts( SullyGame sg ) {
            this.sg = sg;
        }
        
        public void testing() {
            Console.WriteLine( "SullyGlobalScripts.testing() was called." );
        }

        public void draw_UI(ScriptRenderLayer layer, Rectangle clipping_region) {
            //sg.mainMenu.Draw( this.sg );
            //sg.textbox.Draw();
        }

    }
}
