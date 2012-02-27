using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sully {
    class Menu {

        public Texture2D mainBox, commandBox, smallBox; // statusBox;

        public Texture2D activeBgColor, inactiveBgColor;

        public Menu() {

            Color[] boxcolors = new Color[3];
            boxcolors[0] = new Color( 0, 0, 0 );
            boxcolors[1] = new Color( 112, 112, 112 );
            boxcolors[2] = new Color( 144, 144, 144 );
            
            mainBox = _.MakeBox( 220, 220, boxcolors );   // resting offset, 10,10
            commandBox = _.MakeBox( 50, 160, boxcolors ); // resting offset, 240,10
            smallBox = _.MakeBox( 50, 50, boxcolors );    // resting offset, 240,180        
        }
    }
}
