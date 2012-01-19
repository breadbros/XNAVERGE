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
        public static int cur_line, cur_pos, vertical_padding, horizontal_padding;
        public static int short_step, long_step;
        public static BasicDelegate callback;

        public static List<String> lines;

        private static int last_anim_tick;
        
        static Textbox() {
            lines = new List<String>(3);
            vertical_padding = 1;
            horizontal_padding = 4; 
            long_step = 6;
            short_step = 2;
            reset();
        }

        public static void reset() {
            lines.Clear();
            callback = null;
            state = TextboxState.Hidden;
            cur_line = cur_pos = 0;
            last_anim_tick = VERGEGame.game.tick;
        }        

        public static void Update() {
            MGame game = (MGame)VERGEGame.game;
            int step;
            switch (Textbox.state) {
                case TextboxState.Waiting: // The textbox has finished scrolling and is awaiting input
                    if (game.action.confirm.pressed || game.action.cancel.pressed) {
                        reset();
                        if (callback != null) callback();
                        VERGEGame.game.player_controllable = true;
                    }
                    break;
                case TextboxState.Printing: { // The textbox is currently scrolling text
                    if (lines.Count == 0 || game.action.cancel.pressed) {
                        state = TextboxState.Waiting;
                        last_anim_tick = game.tick;
                    }
                    else {
                        if (game.action.confirm.down) step = short_step;
                        else step = long_step;
                        while (game.tick - last_anim_tick >= step) {
                            last_anim_tick += step;
                            cur_pos++;
                            while (cur_line < lines.Count && cur_pos >= lines[cur_line].Length) {
                                cur_line++;
                                cur_pos = 0;
                            }
                            if (cur_line >= lines.Count) {
                                state = TextboxState.Waiting;
                            }
                        }
                    }
                }
                break;
            }
        }

        public static void Draw() {
            MGame game = (MGame)VERGEGame.game;
            int height, length;
            length = lines.Count;
            height = game.system_font.LineSpacing;
            if (state != TextboxState.Hidden) {                
                game.spritebatch.Begin();
                game.spritebatch.Draw(Textbox.image, Textbox.bounds, Color.White);

                if (state == TextboxState.Waiting) { // finished printing the full contents
                    for (int i = 0; i < length; i++) {
                        game.print_string(lines[i], Textbox.inner_bounds.X, Textbox.inner_bounds.Y + i * height, Color.White, false);
                    }
                }
                else { // still scrolling text
                    for (int i = 0; i < cur_line; i++) {
                        game.print_string(lines[i], Textbox.inner_bounds.X, Textbox.inner_bounds.Y + i * height, Color.White, false);
                    }
                    game.print_string(lines[cur_line].Substring(0,cur_pos+1), Textbox.inner_bounds.X, Textbox.inner_bounds.Y + cur_line * height, Color.White, false);
                }          

                game.spritebatch.End();
            }
        }
    }

    public enum TextboxState { Hidden, Printing, Waiting }
}
