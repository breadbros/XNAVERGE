using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    class Battle {

        public static Dictionary<string, Texture2D> masterBackgrounds;
        public static Dictionary<string, Sprite> masterPCs;
        public static Dictionary<string, Sprite> masterEnemies;

        public static void LoadBackgrounds( ContentManager content ) {
            masterBackgrounds = new Dictionary<string, Texture2D>();
            masterPCs = new Dictionary<string, Sprite>();
            masterEnemies = new Dictionary<string, Sprite>();

            masterBackgrounds.Add( "grass", content.Load<Texture2D>("battlebg_grass") );


            Sprite s = new Sprite( "darin.json", "Idle" );
            masterPCs.Add( "Darin", s );

            s = new Sprite( "bandit.json", "Idle" );
            masterEnemies.Add( "Bandit", s );
        }

        public static void init() {

            McgLayer l = _.sg.renderstack.GetLayer( "battle_background" );
            McgNode node;

            node = l.AddNode(
                new McgNode( masterBackgrounds["grass"], new Rectangle(0,0,320,240), l, 0, 0 )
            );

            l = _.sg.renderstack.GetLayer( "battle_sprites" );

            node = l.AddNode(
                new McgNode( masterPCs["Darin"], l, 250, 132 )
            );

            node = l.AddNode(
                new McgNode( masterEnemies["Bandit"], l, 75, 132 )
            );            

        }
    }
}
