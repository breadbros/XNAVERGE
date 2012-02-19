using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;

namespace Sully {
    public class SullyGlobalScripts : ScriptBank {
        public void testing() {
            Console.WriteLine( "SullyGlobalScripts.testing() was called." );
        }

        public void draw_UI(ScriptRenderLayer layer, Rectangle clipping_region) {
            Textbox.Draw();
        }

        public void SavePoint( int x, int y, bool adj ) { }
        public void SaveDisable( int x, int y, bool adj ) { }
    }

    //{Sully.Script_paradise_isle2}
    public class Script_paradise_isle2 : MapScriptBank {

        public Script_paradise_isle2( VERGEMap map ) : base( map ) {
           Console.WriteLine( "Paradise Isle scripts loaded." );
        }

        public void zonetrigger(int x, int y, bool adj) {
            Console.WriteLine("({0}, {1})", x, y);
        }
/*
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
            
        }
 */
        public void sully( int x, int y, bool adj ) {
            ( (SullyGame)VERGEGame.game ).textbox( "vargulfs vargulfs", "", "vargulfs?" );
        }

        public void sancho( int x, int y, bool adj ) {
            ( (SullyGame)VERGEGame.game ).textbox( "vargulfs vargulfs", "", "vargulfs?" );
        }

        public void undersea( int x, int y, bool adj ) {

        }

        public void pearl_cave( int x, int y, bool adj ) { 
        
        }

        public void enter_house( int x, int y, bool adj ) { 
        
        }

        public void exit_house( int x, int y, bool adj ) { }

        public void chest_a( int x, int y, bool adj ) { }

        public void normal_tree( int x, int y, bool adj ) { }

        public void girlfriend_check( int x, int y, bool adj ) { }

        public void crystal_event( Entity ent ) {

        }
    
    }

}
