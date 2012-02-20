using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sully {
    public partial class _ {
        public static void InitMap() {

            // from the Simpletype library... this makes
            // sure that your party is on the map!
            //
            // note: mapswitchx, mapswitchy are parts of the v1_rpg library
            //       set by v1_MpaSwitch()

            /*
                        if( !v1_party_init )
                        {
                            if( __initmap_overworld_flag )
                            {
                                _overworld_mode = 1;
                                SpawnPartyOverworld( mapswitchx, mapswitchy );
                            }
                            else
                            {
                                _overworld_mode = 0;
                                SpawnParty( mapswitchx, mapswitchy );
                            }

                            v1_party_init = 1;
                        }
            */
            // from the menu library, this makes sure that if you're in
            // menu-off mode, it stays off, or vica versa.
            /*
                        if (_menu_on) MenuOn();
                        else MenuOff();
            */
            // from the v1_rpg effects library
            // this makes sure that the second half of any transition effect
            // started by v1_MapSwitch() is completed!
            // v1_InitMap();
        }
    }
}