using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace Sully {
    public partial class _ {

        public static Action _PlayerMove_Action;

        public static void PlayerMove( string movescript ) {
            if( sg.player_controllable ) {
                VERGEGame.game.lock_player();
            }

            sg.player.movestring = new Movestring(movescript);

            _PlayerMove_Action = () => {
                //sg.player_controllable = true;
                VERGEGame.game.unlock_player();
                sg.player.movestring.OnDone -= _PlayerMove_Action;
            };

            /// when the movestring is done... return control to the player. 
            sg.player.movestring.OnDone += _PlayerMove_Action;
        }

        public static int TNONE = 0;
        public static int TBLACK = 1;
        public static int TWHITE = 2;
        public static int TCROSS = 3;
        public static int TBOX = 4;
        public static int TWIPE = 5;
        public static int TCIRCLE = 6;

        public static void Warp( int x, int y, int fadeMode ) {
            sg.player.move_to_tile( x, y );
        }

        public static void ChangeMap( string mapAssetname, int x, int y, int fademode ) {
            VERGEMap.switch_map( mapAssetname );
            sg.player.move_to_tile( x, y );
        }
    }
}
