﻿using System;
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
            T_BIRD = 9,
            T_BUBBA = 10,
            T_SANCHO = 11,
            T_LANCE = 12,
            T_PAXTON = 13;

        public static void init( SullyGame sg ) {
            _.sg = sg;
            _.flags = new int[1500];
        }

        public static void TextBox( int port, string s1, string s2, string s3 ) {
            talk( s1, s2, s3, port );
        }

        public static void TextBox( string s1, string s2, string s3 ) {
            talk( s1, s2, s3 );
        }

        public static void SetTile( int layer, int x, int y, int tile ) {
            sg.map.tiles[layer].data[x][y] = tile;
        }

        public static void talk( String str_1, String str_2, String str_3, int speechIdx = 0 ) {
            VERGEGame.game.lock_player();
            sg.textbox.reset();
            sg.textbox.lines.Add( str_1 );
            sg.textbox.lines.Add( str_2 );
            sg.textbox.lines.Add( str_3 );
            sg.textbox.activeSpeechIdx = speechIdx;
            sg.textbox.state = TextboxState.Printing;
        }
    }
}
