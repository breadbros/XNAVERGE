using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;

namespace Sully {

    //{Sully.Script_paradise_isle2}
    public class Script_paradise_isle2 : MapScriptBank {

        public void start() {
            _.SaveDisable();

            //if Crystal's already joined, we don't want to see the version of her that's
            // pre-built into the map...
            if( _.getFlagB( _.F_CRYS_JOIN ) ) {

                this.map.entities[0].x = 30000;
                this.map.entities[0].speed = 0;
            }

            //if Sully's shown the way
            if( _.getFlagB( _.F_SULLY_OPEN_PEARL ) ) {
                open_tree_door();
            }

            // This is the code for the flashback sequence triggered in bumsville.
            if( _.getFlag( _.F_BUM_NIGHT ) == 1 ) {
                do_flashback();
            }

            _.InitMap();

            _.PlayMusic( "troupe_-_cabedge_sailing" );
            // V1_StartMusic( "res/music/MEDIOEVA.MOD" );
            //Banner( "Paradise Isle", 300 );
        }

        public Script_paradise_isle2( VERGEMap map )
            : base( map ) {
            Console.WriteLine( "Paradise Isle scripts loaded." );
        }

        public void zonetrigger( int x, int y, bool adj ) {
            Console.WriteLine( "({0}, {1})", x, y );
        }

        public void sully( int x, int y, bool adj ) {
            //if Galfrey hasn't joined yet, we default to the intro text.
            if( !_.getFlagB( _.F_HECK_GALFREY_JOIN ) ) {
                sully_intro(x,y,adj);
            } else {
                if( _.getFlagB( _.F_SULLY_OPEN_PEARL ) ) {
                    _.TextBox( _.T_SULLY, "Please leave me be.",
                                          "I'm just a silly little clam.",
                                          "I have no objects of value for you!" );
                } else {

                    // If we got this far, we're doing the sully-scene!
                    //

                    _.TextBox( _.T_SULLY, "Hi! My name is Sully Clam.",
                                        "I welcome you back to Paradise Isle.", "" );
                    _.TextBox( _.T_GALFREY, "Enough small talk, clam.",
                                        "Tell us where to find the [Pearl of Truth]",
                                        "before I rough you up!" );
                    _.TextBox( _.T_SARA, "I thought Pearls came from oysters, not",
                                        "clams.", "" );
                    _.TextBox( _.T_CRYSTAL, "Sully, is there something you haven't told",
                                        "us?", "" );
                    _.TextBox( _.T_SULLY, "Alright, I give up! ",
                                        "The ancient race of cyclops once lived on",
                                        "this very island." );
                    _.TextBox( _.T_SULLY, "There's a sacred hall beneath here that",
                                        "holds the [Pearl of Truth].", "" );
                    _.TextBox( _.T_GALFREY, "Grand! Show us the entrance or I'll teach",
                                        "you a whole new meaning of shellfish",
                                        "abuse!" );
                    _.TextBox( _.T_SULLY, "Okee dokee. Let me just try to remember",
                                        "the location...", "" );

                    _.TextBox( _.T_GALFREY, "Stop stalling, clam!",
                                        "I need to get back inside the Castle Heck",
                                        "right away!" );

                    open_tree_door();

                    _.TextBox( _.T_SULLY, "Oh, I remember! It's in the tall palm tree",
                                        "in the ring of stones.",
                                        "I have revealed the entrance!" );
                    _.TextBox( _.T_DARIN, "Thanks, Sully. We owe ya one!", "", "" );

                    _.setFlag( _.F_SULLY_OPEN_PEARL, 1 );
                }
            }

        }

        public void sully_intro( int x, int y, bool adj ) {
            _.TextBox( _.T_SULLY, "\"when a girl walks in with an itty-bitty", "waist and a round thing in your face", "you get sprung.\"" );
            _.TextBox( _.T_DARIN, "This should be replaced with a real intro,", "yeah?", "" );
            _.TextBox( _.T_SULLY, "...", "...", "...yeah." );
            _.TextBox( _.T_SULLY, ":(", "", "" );

            _.setFlag( _.F_SULLY_INTRO, 1 );
        }

        public void sancho( int x, int y, bool adj ) {
            _.TextBox( _.T_SANCHO, "*SIGH*", "", "...hi." );
        }

        public void undersea( int x, int y, bool adj ) {
            _.ChangeMap( "underwater", 30, 11, _.TCIRCLE );
        }

        public void pearl_cave( int x, int y, bool adj ) {
            if( _.getFlagB( _.F_SULLY_OPEN_PEARL ) ) {
                _.setFlag( _.F_SEA_IN_CYCLOPS_HALL, 1 );

                _.ChangeMap( "undersea", 47, 21, _.TWHITE );

            } else {
                _.TextBox( _.T_DARIN,
                    "Something about this tree always seemed",
                    "weird to me.",
                    "" );
            }
        }

        public void enter_house( int x, int y, bool adj ) {
            _.Warp( 99, 13, _.TNONE );
        }

        public void exit_house( int x, int y, bool adj ) {
            _.Warp( 10, 56, _.TNONE );
        }

        public void chest_a( int x, int y, bool adj ) {
            _.TextBox( _.T_DARIN, "Mmmmph!", "", "" );
            _.TextBox( _.T_DARIN, "...it's locked.", "", "" );
        }

        public void normal_tree( int x, int y, bool adj ) {
            _.TextBox( _.T_DARIN, "This is a normal tree. It has to work", "twice as hard as a palm tree for half", "the recognition." );
        }

        public void girlfriend_check( int x, int y, bool adj ) {
            if( !_.getFlagB( _.F_CRYS_JOIN ) ) {
                _.TextBox( _.T_DARIN, "Hrm... Crystal might get suspicious if I", "sneak off the island without her...", "" );
                _.TextBox( _.T_DARIN, "...again.", "", "" );

                _.sg.textbox.OnDone += () => {
                    _.PlayerMove( "U2" );
                };
            }
        }

        public void crystal_event( Entity ent ) {
            /*
                FadeOut(100);
    
                //fill the vclayer with black to serve as a black matte for the art cells
                FillVCLayer( RGB(0,0,0) );
    
                //put the art cel    on the black matte.
                VCPutIMG( "res/images/cells/cryssand.gif", 158,20 );
    
                // At this point we haven't changed anything about the screen.
                // Now we want to fade in the vclayer we created (which is now an 
                // image of crystal on a black background that fills the screen).
                //
                // This function, FYI, is not a system function, but rather defined in
                // ./vc_lib/util/v1_maineffects.vc
                FadeInImg( 100, v1_vclayer );
    

                // Now we do the talking!
                TextBoxM(T_CRYSTAL, "Darin.... Hello. How are you today? I",
                                    "needed to talk to you about something..." ,"");
                TextBox(T_CRYSTAL,  "I... I've been thinking and I've decided",
                                    "we need to leave Paradise Island.", "");
                TextBox(T_DARIN,    "But Crystal! What's wrong? I kinda like",
                                    "this blue lagoon thing we have going on.","");
            
                TextBox(T_CRYSTAL,  "No... it has to be this way. We'll go",
                                    "insane if we don't have some adventure", 
                                    "soon!");
                TextBox(T_DARIN,    "I suppose you're right. I'd like to go",
                                    "exploring some caves and stuff.", "");
    
    

                //Fade out so we can switch from the vclayer to regular map-rendering
                // without anyone seeing the old switcheroo!
                FadeOut(50);
    
                //We're done with the vclayer for now, so let's wipe it clean
                ClearVCLayer();
    
                //now that the vclayer is clear, trigger the map-rendering.
                Render();
                FadeIn(30);
                TextBox(T_CRYSTAL,  "I'm glad you understand. I suppose we",
                                    "can use the undersea passage. Let's go!", "");

                // We're about to want to have darin's entity index, so let's fetch it.
                int darin = GetPartyEntity( "Darin" );
                Wait(20);
                // let's move the on-map crystal 'onto' Darin.
                // The trigger.beforeEntityScript has turned off crystal's movement,
                // so we need to restore it before she can move. We needn't turn it
                // off again after since we're done with her.
                // We know crystal is entity 0 because we placed her first on the map.
                entity.speed[0] = 100; // 100 is the "average" speed
                AlignStalker( 0, darin );

                // let's wait until the on-map crystal has stopped moving
                WaitForEntity(0);
        
                //now we add the in-party Crystal, which will appear right on top 
                //of darin...
                JoinParty("Crystal");
                FullHeal(IsCharacter("Crystal")); // joinparty doesn't heal, so do that here
    
                //and we move the on-map crystal way off-screen.  We've successfully added
                //her to the party!
                entity.x[0] = 30000;
    
                //let's save a flag that Crystal's joined.
                flags[F_CRYS_JOIN] = 1;
*/
        }

        void do_flashback() {
            /*
                        StopMusic();
                        MenuOff();
                        //SetWeather(WEATHER_NIGHT);
                        sfx_waves = LoadSound( "res/sfx/water03.wav" );
                        wavetimer = systemtime + 680;
                        HookTimer( "island_flashback_timer" );

                        PlaySound( sfx_waves, 100 );
                        entity.x[0] = 41 * 16;
                        entity.y[0] = 25 * 16;
                        entity.face[0] = 1;
                        entity.speed[0] = 0;

                        InitMap();

                        entity.face[1] = 1;
                        FadeIn( 200 );
                        Wait( 300 );
                        TextBox( T_CRYSTAL, "It's so beautiful how the moonlight",
                                  "reflects off the ocean, don't you think?", "" );
                        TextBox( T_DARIN, "Yes, it is. But its beauty pales in",
                                  "comparison to your sparkling eyes.", "" );
                        entity.face[0] = 3;
                        TextBox( 0, "Oh, Darin...", "", "" );
                        entity.face[1] = 4;
                        TextBox( 0, "Oh, Crystal...", "", "" );
                        Wait( 200 );
                        entity.face[0] = 1;
                        Wait( 50 );
                        entity.face[1] = 1;
                        wait( 100 );
                        TextBox( T_CRYSTAL, "I just wish this could go on forever,",
                                  "don't you? I hope absolutely nothing",
                                  "changes." );
                        entity.face[1] = 4;
                        TextBox( T_DARIN, "Don't worry, Crystal. I'll never let anyone",
                                  "or anything disturb our happiness!", "" );
                        entity.face[1] = 1;
                        Wait( 500 );
                        FadeOut( 200 );
                        HookTimer( "" );
                        FreeSound( sfx_waves );

                        flags[F_BUM_NIGHT] = 2; //we set it to 2 so it doesn't trigger
                        // this flashback anymore, yet the flag 
                        // still counts as true!

                        // we'll pass the love the the 
                        // bumsville map autoexec function to continue the 
                        // cutscene...  see you there!
                        V1_MapSwitch( "bumville.map", 106, 24, TNONE );
             */
        }

        void open_tree_door() {
            /*            
                        AlterFTile( 59, 17, 663, 0 );
                        AlterFTile( 60, 17, 664, 0 );
                        SetTile( 59, 16, 2, 643 );
                        SetTile( 60, 16, 2, 644 );
            */
        }
    }
}
