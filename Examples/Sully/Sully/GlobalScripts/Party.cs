using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace Sully {
    public partial class _ {

        public static Action _PlayerMove_Action;

        public static int _ms_x = -1, _ms_y = -1;
        public static void MapSwitch( string map, int x, int y ) {
            _ms_x = x;
            _ms_y = y;

            BasicDelegate bs = () => {
                if( _ms_x >= 0 && _ms_y >= 0 ) {

                    PartyMember[] pm = _.sg.party.getMembers();

                    pm[0].ent = _.sg.player = _.sg.map.spawn_entity(_ms_x, _ms_y, pm[0].normal_chr );
                    _.sg.followers.clear();
                
                    for( int i = 1; i<pm.Length; i++ ) {
                        pm[i].ent = _.sg.map.spawn_entity(_ms_x, _ms_y, pm[i].normal_chr );
                        _.sg.followers.add( pm[i].ent );
                    }
                
                }

                _ms_x = -1;
                _ms_y = -1;
            };

            VERGEMap.switch_map( map, bs );
        }

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
