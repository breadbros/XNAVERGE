using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    public partial class _ {

        public static Texture2D pixel;
        public static SpriteBatch sb;

        public static void setDrawTarget( SpriteBatch _sb ) {
            sb = _sb;
        }

        public static void DrawPixel( int x, int y, Color color, Texture2D target ) {
            //DrawLine( x, y, x+1, y+1, color, target );
                
            sb.Draw(pixel, new Vector2(x,y), color);        
        }

        public static void DrawLine( int x1, int y1, int w, int h, Color color) {
            sb.Draw( pixel, new Rectangle( x1,y1, w,h ), color );
        }

        public static void DrawRect( int x1, int y1, int x2, int y2, Color color ) {
            int w = x2 - x1;
            int h = y2 - y1;

            DrawLine( x1, y1, 1, h, color );
            DrawLine( x1, y1, w, 1, color );
            DrawLine( x2, y1, 1, h, color );
            DrawLine( x1, y2, w, 1, color );
        }

        public static void DrawRectFill( int x1, int y1, int x2, int y2, Color color ) {
            int w = x2 - x1;
            int h = y2 - y1;

            sb.Draw( pixel, new Rectangle(x1,y1,w,h), color );
        }

        public static Texture2D MakeBox( int w, int h, Color[] boxcolors ) {

            RenderTarget2D dest = new RenderTarget2D( _.sg.GraphicsDevice, w, h, true, SurfaceFormat.Color, DepthFormat.Depth24 );

            sb = new SpriteBatch( _.sg.GraphicsDevice );
            _.sg.GraphicsDevice.SetRenderTarget( dest );
            _.sg.GraphicsDevice.Clear( ClearOptions.Target, Color.Transparent, 0, 0 );
            sb.Begin();

            Color _0 = boxcolors[0];
            Color _1 = boxcolors[1];
            Color _2 = boxcolors[2];

            int x1 = 0;
            int y1 = 0;
            
            int x2 = x1 + w;
            int y2 = y1 + h;

            
            DrawLine( x1, y1 + 2, 1, (y2 - 2)-(y1 + 2), _0 ); // TL -> BL
            DrawLine( x1 + 2, y1, (x2 - 2)-(x1 + 2), 1, _0 ); // TL -> TR

            DrawLine( x2 - 1, y1 + 2, 1, (y2 - 2)-(y1 + 2), _0 ); // BR -> TR
            DrawLine( x1 + 2, y2 - 1, (x2 - 2)-(x1 + 2), 1, _0 ); // BR -> BL

            DrawRect( x1 + 1, y1 + 1, x2 - 2, y2 - 2, _1 );
                DrawPixel( x1 + 1, y1 + 1, _0, dest ); // TL
                DrawPixel( x2 - 2, y1 + 1, _0, dest ); // TR
                DrawPixel( x1 + 1, y2 - 2, _0, dest ); // BL
                DrawPixel( x2 - 2, y2 - 2, _0, dest ); // BR

            DrawRect( x1 + 2, y1 + 2, x2 - 3, y2 - 3, _2 );            
                DrawPixel( x1 + 2, y1 + 2, _1, dest ); // TL
                DrawPixel( x2 - 3, y1 + 2, _1, dest ); // TR
                DrawPixel( x1 + 2, y2 - 3, _1, dest ); // BL
                DrawPixel( x2 - 3, y2 - 3, _1, dest ); // BR
             
            DrawRect( x1 + 3, y1 + 3, x2 - 4, y2 - 4, _0 );            
                DrawPixel( x1 + 3, y1 + 3, _2, dest ); // TL
                DrawPixel( x2 - 4, y1 + 3, _2, dest ); // TR
                DrawPixel( x1 + 3, y2 - 4, _2, dest ); // BL
                DrawPixel( x2 - 4, y2 - 4, _2, dest ); // BR
             
            sb.End();
            _.sg.GraphicsDevice.SetRenderTarget( null );

            return dest;
        }
    }
}