using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;

namespace Sully {

    //{Sully.Script_paradise_isle2}
    public class Script_underwater : MapScriptBank {

        public void start() {
            _.SaveDisable();

            _.InitMap();

            _.PlayMusic( "troupe_-_cabedge_sailing" );
        }

        public Script_underwater( VERGEMap map )
            : base( map ) {
            Console.WriteLine( "Undersea Cavern scripts loaded." );
        }

        public void Lucent_0( int x, int y, bool adj ) { ((TileLayer)_.sg.map.renderstack.list[6]).alpha = 1; }
        public void Lucent_10( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.9; }
        public void Lucent_20( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.80; }
        public void Lucent_30( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.70; }
        public void Lucent_40( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.60; }
        public void Lucent_50( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.50; }
        public void Lucent_60( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.40; }
        public void Lucent_70( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.30; }
        public void Lucent_80( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.20; }
        public void Lucent_90( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0.10; }
        public void Lucent_100( int x, int y, bool adj ) { ( (TileLayer)_.sg.map.renderstack.list[6] ).alpha = 0; }

        public void warp_paradise( int x, int y, bool adj ) {
            _.MapSwitch( "paradise_isle2", 63, 59 );
        }
                  
        public void warp_world( int x, int y, bool adj ) { }                  
        public void warp_switch( int x, int y, bool adj ) { }                  
        public void warp_lever( int x, int y, bool adj ) { }                  
        public void warp_zone( int x, int y, bool adj ) { }
        public void flood_door( int x, int y, bool adj ) { }                  
        public void chestA( int x, int y, bool adj ) { }                  
        public void chestB( int x, int y, bool adj ) { }                  
        public void SavePoint( int x, int y, bool adj ) { }                  
        public void SaveDisable( int x, int y, bool adj ) { }                  
        public void warp_zone_unstep( int x, int y, bool adj ) { }                  
        public void green_zone_unstep( int x, int y, bool adj ) { }                  
        public void green_zone( int x, int y, bool adj ) { }                  
        public void red_zone( int x, int y, bool adj ) { }                  
        public void red_zone_unstep( int x, int y, bool adj ) { }                  
        public void to_red_cliff( int x, int y, bool adj ) { }                  
        public void to_green_cliff( int x, int y, bool adj ) { }                  
        public void bert( int x, int y, bool adj ) { }

        /*
        /// The autoexec function for  underwater.map
        void start()
        {
            SaveDisable(); //cannot save in dungeons.
            SetBattleArea("Undersea_Cave");
            SetBattleBackground("caves.png");
            if (0) DisableBattles(); // before the mascot fight, there are no random encounters...
            else EnableBattles();    // ...but after the mascot fight, the whole cave is fair game!
    
            arMapTemp[0] = 0;   //warp unstep
            arMapTemp[1] = 0;   //red unstep
            arMapTemp[2] = 0;   //green unstep

            if( Flags[F_SEA_WARPON] )
            {
                SetTile(20,26, 2, 601);
                SetLeverAndZone();
            }
    
            UpdateDoorTiles();
            SetLeverAndZone();

            if( Flags[F_SEA_LEVER_1] )
                SetTile(50,30, 4, 606);
            if( Flags[F_SEA_LEVER_2] )
                SetTile(51,33, 4, 606);
            if( Flags[F_SEA_LEVER_3] )
                SetTile(52,36, 4, 606);

            if (flags[CHEST_UNDERSEA_A]) SetTile(36, 14, 2, 568);
            if (flags[CHEST_UNDERSEA_B]) SetTile(90, 75, 2, 568);   

            //do all the map init stuff (defined in system.vc)
            InitMap();
    
    
            V1_StartMusic("res/music/AQUASUB.IT");
        }

        /// the lameass cheesy layer lucency fade!
        void lucent_0() { layer.lucent[4] = 0; }
        void lucent_10() { layer.lucent[4] = 10; }
        void lucent_20() { layer.lucent[4] = 20; }
        void lucent_30() { layer.lucent[4] = 30; }
        void lucent_40() { layer.lucent[4] = 40; }
        void lucent_50() { layer.lucent[4] = 50; }
        void lucent_60() { layer.lucent[4] = 60; }
        void lucent_70() { layer.lucent[4] = 70; }
        void lucent_80() { layer.lucent[4] = 80; }
        void lucent_90() { layer.lucent[4] = 90; }
        void lucent_100() { layer.lucent[4] = 100; }


        /// Treasure: 3 whole herbs!
        /// @see OpenTreasure()
        /// @see FindItem()
        void chestA()
        {
            if( OpenTreasure(CHEST_UNDERSEA_A, 36, 14, 568, 2) )
            {
                FindItem( "Herb", 3 );
            }
        }

        /// Treasure: 400 clams
        /// @see OpenTreasure()
        /// @see FindMoney()
        void chestB()
        {   
            if( OpenTreasure(CHEST_UNDERSEA_B, 90, 75, 568, 2) )
            {
                FindMoney( 200 );
            }
        }

        void warp_switch()
        {
            if( !Flags[F_SEA_WARPON] )
            {
                SetTile(20,26, 2, 601);
                SoundSwitch();
                Flags[F_SEA_WARPON] = 1;
                SetLeverAndZone();
            }
        }

        /// master lever switch for this map
        void warp_lever()
        {
            if( !Flags[F_SEA_WARPON] )
            {
        
                TextBox( T_DARIN,   "Uuurgh!",
                            "",
                            "");
                TextBox( T_DARIN,   "It won't budge!",
                            "",
                            "");
                TextBox( T_CRYSTAL, "Can't move a lever?",
                            "...",
                            "");
                TextBox( T_CRYSTAL, "...",
                            "",
                            "");
                TextBox( T_CRYSTAL, "Don't worry honey.",
                            "I still respect you for your mind...",
                            "");
        
            }
            else
            {
                SoundSwitch();
                switch( flags[F_SEA_WARP] )
                {
                    case 0: 
                        flags[F_SEA_WARP] = 1;
                    case 1:
                        flags[F_SEA_WARP] = 2;
                    case 2:
                        flags[F_SEA_WARP] = 0;
                }
            }
    
            SetLeverAndZone();
        }

        /// Sets the lever and warp zone tiles as appropriate for the present flags[F_SEA_WARP] value
        void SetLeverAndZone()
        {
            if( Flags[F_SEA_WARPON] )
            {
                switch( flags[F_SEA_WARP] )
                {
                    case 1: // red
                        SetTile(24, 24, 2, 603);    //change the lever tile
                        SetTile(26, 25, 2, 515);    //change the warpzone tile

                    case 2: // green
                        SetTile(24, 24, 2, 604);    //change the lever tile
                        SetTile(26, 25, 2, 535);    //change the warpzone tile

                    case 0: // blue
                        SetTile(24, 24, 2, 602);    //change the lever tile
                        SetTile(26, 25, 2, 555);    //change the warpzone tile
                }
            }
        }

        void warp_zone_unstep()
        {
            arMapTemp[0] = 1;
        }

        void red_zone_unstep()
        {
            arMapTemp[1] = 1;
        }

        void green_zone_unstep()
        {
            arMapTemp[2] = 1;
        }

        void warp_zone()
        {
            if( Flags[F_SEA_WARPON] )
            {
                if( arMapTemp[0] )
                {
                    arMapTemp[0] = 0;

                    SoundWarpZone();
                    switch (flags[F_SEA_WARP])
                    {
                        case 0: V1_MapSwitch("underwater_2.map", 83, 45, TBOX);
                        case 1: Warp(37, 82,  TBOX);
                        case 2: Warp(91, 67, TBOX);
                    }
                }
            }
        }

        void red_zone()
        {
            if( arMapTemp[1] ) 
            {
                arMapTemp[1] = 0;
        
                SoundWarpZone();
                Warp(26,25,  TBOX);
            }
        }

        void green_zone()
        {
            if( arMapTemp[2] )
            {
                arMapTemp[2] = 0;
        
                SoundWarpZone();
                Warp(26,25,  TBOX);
        
            }
    
        }

        void flood_door()
        {
    
            switch( Flags[F_SEA_LEVER_CNT] )
            {
                case 0: 
                    TextBox( T_DARIN,   "Hrm.  When'd this flood door get installed?",
                                "",
                                "");
                    TextBox( T_CRYSTAL, "'Brought to you by the Paradise Isle",
                                "Chamber of Commerce'?!",
                                "");
                    TextBox( T_DARIN,   "Let's find how to open this!",
                                "",
                                "");
                case 1: 
                    TextBox( T_DARIN,   "Hrm, that switch opened it a little.",
                                "...maybe there are more switches?",
                                "");
                case 2: 
                    TextBox( T_DARIN,   "Almost there!",
                                "",
                                "");
            }
    
        }

        void UpdateDoorTiles()
        {
            switch( Flags[F_SEA_LEVER_CNT] )
            {
                case 1: 
                    SetTile(29,24, 3, 368); 
                    SetTile(29,25, 3, 388); 
                    SetTile(29,26, 3, 408); 
                case 2: 
                    SetTile(29,24, 3, 369); 
                    SetTile(29,25, 3, 389); 
                    SetTile(29,26, 3, 409); 
                case 3: 
                    SetTile(29,24, 3, 0);   
                    SetTile(29,25, 3, 0);   
                    SetTile(29,26, 3, 0);   
                    SetObs(29,26,0);
            }
        }

        void warp_paradise()
        {
            V1_MapSwitch("paradise_isle2.map", 63, 60, TBLACK);
        }


        void warp_world()
        {
            V1_MapSwitch("simpletype_overworld.map", 57,60, TBLACK);
        }

        void to_red_cliff()
        {
            V1_MapSwitch("underwater_1.map", 34,19, TNONE);
        }

        void to_green_cliff()
        {
            V1_MapSwitch("underwater_1.map", 35,22, TNONE);
        }

        void bert()
        {
    
    
            if( flags[F_SEA_BERT] <= 0 ) // initial meeting
            {
                TextBox(T_MANTA,    "Hi there! My name is Bert Stingray, and",
                                    "I'm here to tell you about the battle",
                                    "system!");
                
                TextBox(T_DARIN, "Oh yeah, I heard that they finally", "implemented it!", "I can't wait!");
        
                TextBoxM(T_MANTA, "Actually, that's what I'm here to tell","you.","");        
                TextBoxM(T_MANTA, "The battle system is basically finished,","but they found some pretty bad bugs","at the last minute, so it's been disabled");
                TextBox(T_MANTA, "for the time being.","","");
                TextBox(T_DARIN, "You know, I can only be teased so much", "before it stops being fun.", "");
                TextBox(T_MANTA, "We'll see about that!","","");
        
                flags[F_SEA_BERT] = 1; //trip this conversation flag.
            }
            else if (flags[F_SEA_BERT] == 1) // before surprise battle, but after talking to Bert.
            {
                TextBox(T_DARIN, "You know, I can only be teased so much", "before it stops being fun.", "");
                TextBox(T_MANTA, "We'll see about that!","","");
            }    
    
        } 
          
         */

    }
}
