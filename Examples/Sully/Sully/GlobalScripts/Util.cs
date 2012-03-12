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

        public static void init( SullyGame sg ) {
            _.sg = sg;
            _.flags = new int[1500];
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
        public static void DrawIcon( int idx, int x, int y ) {
            if( iconAtlas == null ) {
                iconAtlas = _.sg.Content.Load<Texture2D>( "ItemIcons" );
            }

            _.sg.spritebatch.Draw( iconAtlas, new Rectangle(x,y,16,16), icon_get( idx ), Color.White );
        }

        public static Boolean ItemIsConsumable( Item i ) { return true; }
        public static Boolean ItemIsEquipment( Item i ) { return false; }
        public static Boolean ItemIsKey( Item i ) { return false; }
    }
}
