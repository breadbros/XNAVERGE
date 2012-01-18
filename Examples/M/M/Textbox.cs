using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace M {
    public static class Textbox {
        public static TextboxState state;
        public static Texture2D image;
        public static Rectangle bounds, inner_bounds;
        public static int num_lines, cur_line, cur_pos, vertical_padding, horizontal_padding;
        public static int short_step, long_step;

        public static List<String> lines;
        public static String line_printing;
        
        static Textbox() {
            lines = new List<String>(3);
            vertical_padding = 1;
            horizontal_padding = 4; 
            long_step = 20;
            short_step = 4;
            reset();
        }

        public static void reset() {
            lines.Clear();
            state = TextboxState.Hidden;
            cur_line = cur_pos = 0;
        }        

        public static void Update() {

        }

        public static void Draw() {
            MGame game = (MGame)VERGEGame.game;
            int height, length;
            length = lines.Count;
            if (state != TextboxState.Hidden) {
                height = game.system_font.LineSpacing;

                game.spritebatch.Begin();
                game.spritebatch.Draw(Textbox.image, Textbox.bounds, Color.White);

                for (int i=0; i<length; i++) {
                    game.print_string(lines[i], Textbox.inner_bounds.X, Textbox.inner_bounds.Y+i*height, Color.White, false);
                }                
                game.spritebatch.End();
            }
        }
    }

    public enum TextboxState { Hidden, Printing, Waiting }
}
