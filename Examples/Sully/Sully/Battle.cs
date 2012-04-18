using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    class Battle {
        McGrenderStack mcg;

        public static Dictionary<string, Texture2D> masterBackgrounds;

        public static void LoadBackgrounds( ContentManager content ) {
            masterBackgrounds = new Dictionary<string, Texture2D>();

            masterBackgrounds.Add( "grass", content.Load<Texture2D>("battlebg_grass") );
        }

        public static void init() {    

            McgLayer l = _.sg.renderstack.GetLayer( "menu" );


            /*
            McgNode rendernode = l.AddNode(
                new McgNode( onDraw, l, start_x, start_y, final_x, final_y, MainMenu.delay )
            );
            */
        }
    }
}
