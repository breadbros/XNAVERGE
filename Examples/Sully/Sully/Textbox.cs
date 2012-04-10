using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {

    public class Textbox {
        public TextboxState state;
        public Texture2D image;
        public Rectangle bounds, inner_bounds, color_bounds, speech_bounds;
        public int cur_line, cur_pos, vertical_padding, horizontal_padding;
        public int short_step, long_step;
        
        public BasicDelegate callback;
        public BasicDelegate OnDone;

        public Texture2D bgColor;
        public Texture2D speechPortraits;
        public int activeSpeechIdx = 0;
        public Boolean isStarted = false;
        public Boolean dontReleasePlayerOnDone = false;

        public List<object> boxes_of_text;
        public List<string> currently_rendering_text;

        private static int last_anim_tick;

        private SullyGame game;

        public Textbox( int screen_width, int screen_height, Texture2D speechPorts, SullyGame g ) {
            game = g;
            boxes_of_text = new List<object>();
            currently_rendering_text = new List<string>();
            vertical_padding = 2;
            horizontal_padding = 7; 
            long_step = 2;
            short_step = 1;

            full_reset();

            //textbox.image = Content.Load<Texture2D>( "textbox" );
            
            image = _.MakeBox( 317, 50, _.sg.boxcolors );

            speechPortraits = speechPorts;

            bounds = new Rectangle( 0, 0, image.Width, image.Height );

            int yloc = screen_height - bounds.Height - 4;

            bounds.Offset( ( screen_width - bounds.Width ) / 2, yloc );

            speech_bounds = new Rectangle( 2, yloc - 33, 32, 32 );

            inner_bounds = bounds; // copy value            
            color_bounds = bounds;
            inner_bounds.Inflate( -horizontal_padding, -vertical_padding );
            color_bounds.Inflate( -2, -2 );

            bgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            bgColor.SetData(new[] { new Color(140, 0, 140) });



            McgLayer l = game.renderstack.GetLayer( "textbox" );

            RenderDelegate a1 = ( int x, int y ) => {
                //game.spritebatch.Draw( inactiveBgColor, mainBox.color_bounds, Color.White * .5f );
                //game.spritebatch.Draw( mainBox.image, mainBox.bounds, Color.White );
                Draw();
            };

            l.AddNode(
                new McgNode( a1, l, 0, 0, 300, 300, 3000 )
            );
        }

        public void setPlayerAutorelease( Boolean b ) {
            this.dontReleasePlayerOnDone = !b;
        }

        public void addBox(String str_1, String str_2, String str_3) { addBox(str_1, str_2, str_3, 0); }
        public void addBox( String str_1, String str_2, String str_3, int speechIdx) {

            List<object> lines = new List<object>();

            lines.Add( speechIdx );
            lines.Add( str_1 );
            lines.Add( str_2 );
            lines.Add( str_3 );
            boxes_of_text.Add( lines );

            maybe_start();
        }

        public void full_reset() {
            callback = null;
            OnDone = null;
            state = TextboxState.Hidden;
            cur_line = cur_pos = 0;
            last_anim_tick = VERGEGame.game.tick;
            isStarted = false;
        }

        public void box_reset() {
            last_anim_tick = VERGEGame.game.tick;
            cur_line = cur_pos = 0;
            this.currently_rendering_text.Clear();

            List<object> curbox = (List<object>)boxes_of_text[0];
            activeSpeechIdx = (int)curbox[0];

            this.currently_rendering_text.Add( (string)curbox[1] );
            this.currently_rendering_text.Add( (string)curbox[2] );
            this.currently_rendering_text.Add( (string)curbox[3] );
            state = TextboxState.Printing;
        }

        public void maybe_start() {
            if( !isStarted ) {
                VERGEGame.game.lock_player();
                box_reset();
                state = TextboxState.Printing;
                isStarted = true;
            }
        }

        public void Update() {
            SullyGame game = (SullyGame)VERGEGame.game;
            int step;
            switch (state) {
                case TextboxState.Waiting: // The textbox has finished scrolling and is awaiting input
                    if (game.action.confirm.pressed || game.action.cancel.pressed) {

                        boxes_of_text.Remove( boxes_of_text[0] );

                        if( boxes_of_text.Count == 0 ) {
                            if( callback != null ) {
                                callback();
                            }

                            if( OnDone != null ) {
                                OnDone();
                            }

                            full_reset();
                            
                            if( this.dontReleasePlayerOnDone ) {
                                this.dontReleasePlayerOnDone = false;
                            } else {
                                VERGEGame.game.unlock_player();
                            }

                        } else {
                            box_reset();
                        }
                    }
                    break;
                case TextboxState.Printing: { // The textbox is currently scrolling text
                    if( currently_rendering_text.Count == 0 || game.action.cancel.pressed ) {
                        
                        state = TextboxState.Waiting;
                        last_anim_tick = game.tick;
                    
                    } else {
                        if (game.action.confirm.down) step = short_step;
                        else step = long_step;
                        while (game.tick - last_anim_tick >= step) {
                            last_anim_tick += step;
                            cur_pos++;
                            while( cur_line < currently_rendering_text.Count && cur_pos >= currently_rendering_text[cur_line].Length ) {
                                cur_line++;
                                cur_pos = 0;
                            }
                            if( cur_line >= currently_rendering_text.Count ) {
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
            
            int height, length;
            length = currently_rendering_text.Count;
            height = game.system_font.LineSpacing;

            if( state != TextboxState.Hidden ) {
//game.spritebatch.Begin();

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
                        _Draw( currently_rendering_text[i], inner_bounds.X, inner_bounds.Y + i * height );
                    }
                } else { // still scrolling text
                    for( int i = 0; i < cur_line; i++ ) {

                        _Draw( currently_rendering_text[i], inner_bounds.X, inner_bounds.Y + i * height );
                    }

                    _Draw( currently_rendering_text[cur_line].Substring( 0, cur_pos + 1 ), inner_bounds.X, inner_bounds.Y + cur_line * height );
                }

//game.spritebatch.End();
            }
        }
    }

    public enum TextboxState { Hidden, Printing, Waiting }
}
