using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using XNAVERGE;

namespace Sully {
    public partial class _ {
        public static SullyGame sg;



        public static string[] explode( string input, char exploder ) {

            List<string> ret = new List<string>();
            string[] tmp = input.Split( exploder );

            foreach( string s in tmp ) {
                if( s.Length > 0 ) {
                    ret.Add( s );
                }
            }

            return ret.ToArray();
        }


        public static void SavePoint( int x, int y, bool adj ) { }
        public static void SaveDisable() { }

        public static void PlayMusic( string s ) {
            // music stuff
            MediaPlayer.IsRepeating = true;
            Song song = sg.Content.Load<Song>( s );
            MediaPlayer.Play( song );
        }

        public static int
            T_DARIN = 1,
            T_SARA = 2,
            T_DEXTER = 3,
            T_CRYSTAL = 4,
            T_GALFREY = 5,
            T_STAN = 6,
            T_SULLY = 7,
            T_BUNNY = 8,
            T_BERT = 9,
            T_RAT = 10,
            T_BIRD = 11,
            T_BUBBA = 12,
            T_SANCHO = 13,
            T_LANCE = 14,
            T_PAXTON = 15;

        public const int NUM_FLAGS = 1500;
        public static void init( SullyGame sg ) {
            _.sg = sg;
            _.flags = new int[NUM_FLAGS];
            _.pixel = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color ); 
            _.pixel.SetData( new[] { new Color( new Vector4( 255, 255, 255, 255 )) } );
        }

        public static void TextBox( int port, string s1, string s2, string s3 ) {
            sg.textbox.addBox( s1, s2, s3, port );
        }

        public static void TextBox( string s1, string s2, string s3 ) {
            sg.textbox.addBox( s1, s2, s3 );
        }

        public static void SetTile( int layer, int x, int y, int tile ) {
            sg.map.tiles[layer].data[x][y] = tile;
        }


        public static int xFromFlat( int flatval, int yMax ) {
            return flatval % yMax;
        }

        public static int yFromFlat( int flatval, int yMax ) {
            flatval = flatval - xFromFlat( flatval, yMax );
            return flatval / yMax;
        }

        public static readonly int ICON_WIDTH = 16;
        public static readonly int ICON_HEIGHT = 16;

        public static Rectangle icon_get( int idx ) {

            int x, y;
            int _icon_cols = 18; //specific to this specific file.

            x = xFromFlat( idx, _icon_cols );
            y = yFromFlat( idx, _icon_cols );

            x = ( x * ( ICON_WIDTH + 1 ) ) + 1;
            y = ( y * ( ICON_HEIGHT + 1 ) ) + 1;


            return new Rectangle( x, y, ICON_WIDTH, ICON_HEIGHT );
        }

        static Texture2D iconAtlas = null;
        public static void DrawIcon( int idx, int x, int y, Boolean tiny ) {
            if( iconAtlas == null ) {
                iconAtlas = _.sg.Content.Load<Texture2D>( "ItemIcons" );
            }

            if( tiny ) _.sg.spritebatch.Draw( iconAtlas, new Rectangle( x, y, 8, 8 ), icon_get( idx ), Color.White );
            else _.sg.spritebatch.Draw( iconAtlas, new Rectangle(x,y,16,16), icon_get( idx ), Color.White );
        }












/*
        // A generic control method for 2 dimensions
        public static int MenuControlTwoArrows( Integer int_one, int lim_one, Integer int_two, int lim_two ) {
            int change = 0;
            if( ButtonUp( 1 ) ) {
                if( lastpress + MENU_PRESS_DELAY < timer ) {
                    SetInt( int_one, ( GetInt( int_one ) - 1 + lim_one ) % lim_one );
                    lastpress = timer;
                    change = 1;
                }
            } else if( ButtonDown( 1 ) ) {
                if( lastpress + MENU_PRESS_DELAY < timer ) {
                    SetInt( int_one, ( GetInt( int_one ) + 1 ) % lim_one );
                    change = 1;
                }
            } else lastpress = 0;

            if( ButtonLeft( 1 ) || ButtonLeftTop( 1 ) ) {
                SetInt( int_two, ( GetInt( int_two ) - 1 + lim_two ) % lim_two );
                change += 2;

                CallFunction( _m2ASnd2 ); //call the sound function defined with Menu2ArrowSetSounds()
            } else if( ButtonRight( 1 ) || ButtonRightTop( 1 ) ) {
                SetInt( int_two, ( GetInt( int_two ) + 1 ) % lim_two );
                change += 2;

                CallFunction( _m2ASnd2 ); //call the sound function defined with Menu2ArrowSetSounds()
            }

            return change;
        }
*/




    }
}
