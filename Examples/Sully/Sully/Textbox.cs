using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sully {

    public class Textbox {
        public TextboxState state;
        public Texture2D image;
        public Rectangle bounds, inner_bounds, color_bounds, speech_bounds;
        public int cur_line, cur_pos, vertical_padding, horizontal_padding;
        public int short_step, long_step;
        public BasicDelegate callback;
        public Texture2D bgColor;
        public Texture2D speechPortraits;
        public int activeSpeechIdx = 0;

        public List<String> lines;

        private static int last_anim_tick;
        
        public Textbox() {
            lines = new List<String>(3);
            vertical_padding = 2;
            horizontal_padding = 7; 
            long_step = 2;
            short_step = 1;

            reset();
        }

        public void reset() {
            lines.Clear();
            callback = null;
            state = TextboxState.Hidden;
            cur_line = cur_pos = 0;
            last_anim_tick = VERGEGame.game.tick;
        }        

        public void Update() {
            SullyGame game = (SullyGame)VERGEGame.game;
            int step;
            switch (state) {
                case TextboxState.Waiting: // The textbox has finished scrolling and is awaiting input
                    if (game.action.confirm.pressed || game.action.cancel.pressed) {
                        reset();
                        if (callback != null) callback();
                        VERGEGame.game.unlock_player();
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

        public void _Draw( string s, int x, int y ) {
            SullyGame game = (SullyGame)VERGEGame.game;
            game.print_string( s, x, y + 1, Color.Black, false );
            game.print_string( s, x + 1, y + 1, Color.Black, false );
            game.print_string( s, x + 1, y, Color.Black, false );
            game.print_string( s, x, y, Color.White, false );
        }

        public void Draw() {
            SullyGame game = (SullyGame)VERGEGame.game;
            int height, length;
            length = lines.Count;
            height = game.system_font.LineSpacing;

            if( bgColor == null ) {
                bgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
                bgColor.SetData( new[] { new Color( new Vector4( 140, 0, 140, 63 )) } );
            }

            if( state != TextboxState.Hidden ) {
                game.spritebatch.Begin();

                game.spritebatch.Draw( bgColor, color_bounds, Color.White * .5f );
                game.spritebatch.Draw( image, bounds, Color.White );

                if( activeSpeechIdx > 0 ) {
                    game.spritebatch.Draw( speechPortraits, 
                                           speech_bounds, 
                                           new Rectangle( 0, 32 * activeSpeechIdx, 32, 32 ), 
                                           Color.White 
                    );
                }

                if( state == TextboxState.Waiting ) { // finished printing the full contents
                    for( int i = 0; i < length; i++ ) {
                        _Draw( lines[i], inner_bounds.X, inner_bounds.Y + i * height );
                    }
                } else { // still scrolling text
                    for( int i = 0; i < cur_line; i++ ) {

                        _Draw( lines[i], inner_bounds.X, inner_bounds.Y + i * height );
                    }

                    _Draw( lines[cur_line].Substring( 0, cur_pos + 1 ), inner_bounds.X, inner_bounds.Y + cur_line * height );
                }

                game.spritebatch.End();
            }
        }
    }

    public enum TextboxState { Hidden, Printing, Waiting }
}
