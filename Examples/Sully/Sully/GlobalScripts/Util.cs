using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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
    }
}
