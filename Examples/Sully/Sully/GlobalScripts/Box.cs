using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    public partial class _ {

        public static void DrawPixel( int x, int y, Color color, Texture2D target, Boolean iSSingleOp = true ) {
            DrawLine( x, y, x, y, color, target, iSSingleOp );
        }

        public static void DrawLine( int x1, int y1, int x2, int y2, Color color, Texture2D target, Boolean iSSingleOp = true ) {
            Vector2 point1 = new Vector2( (float)x1, (float)y1 );
            Vector2 point2 = new Vector2( (float)x2, (float)y2 );

            float widthOfLine = (float)1.0;
            float angle = (float)Math.Atan2( point2.Y - point1.Y, point2.X - point1.X );
            float length = Vector2.Distance( point1, point2 );

            _.sg.spritebatch.Draw(
                target, point1, null, color,
                angle, Vector2.Zero, new Vector2( length, widthOfLine ),
                SpriteEffects.None, 0 
            );
        }

        public static void DrawRect( int x1, int y1, int x2, int y2, Color color, Texture2D target, Boolean iSSingleOp = true ) {
            DrawLine( x1, y1, x1, y2, color, target, iSSingleOp );
            DrawLine( x1, y1, x2, y1, color, target, iSSingleOp );
            DrawLine( x2, y1, x2, y2, color, target, iSSingleOp );
            DrawLine( x1, y2, x2, y2, color, target, iSSingleOp );
        }

        public static Texture2D MakeBox( int w, int h , Color[] boxcolors ) {
            Texture2D dest = new Texture2D( _.sg.GraphicsDevice, w, h );

            Color _0 = boxcolors[0];
            Color _1 = boxcolors[1];
            Color _2 = boxcolors[2];

            int x1 = 0;
            int y1 = 0;
            
            int x2 = x1 + w;
            int y2 = y1 + h;

            _.sg.spritebatch.Begin();

            DrawLine( x1, y1 + 2, x1, y2 - 3, _0, dest, false ); // TL -> BL
            DrawLine( x1 + 2, y1, x2 - 3, y1, _0, dest, false ); // TL -> TR

            DrawLine( x2 - 1, y2 - 3, x2 - 1, y1 + 2, _0, dest, false ); // BR -> TR
            DrawLine( x2 - 3, y2 - 1, x1 + 2, y2 - 1, _0, dest, false ); // BR -> BL

            DrawRect( x1 + 1, y1 + 1, x2 - 2, y2 - 2, _1, dest, false );
            DrawPixel( x1 + 1, y1 + 1, _0, dest, false ); // TL
            DrawPixel( x2 - 2, y1 + 1, _0, dest, false ); // TR
            DrawPixel( x1 + 1, y2 - 2, _0, dest, false ); // BL
            DrawPixel( x2 - 2, y2 - 2, _0, dest, false ); // BR

            DrawRect( x1 + 2, y1 + 2, x2 - 3, y2 - 3, _2, dest, false );
            DrawPixel( x1 + 2, y1 + 2, _1, dest, false ); // TL
            DrawPixel( x2 - 3, y1 + 2, _1, dest, false ); // TR
            DrawPixel( x1 + 2, y2 - 3, _1, dest, false ); // BL
            DrawPixel( x2 - 3, y2 - 3, _1, dest, false ); // BR

            DrawRect( x1 + 3, y1 + 3, x2 - 4, y2 - 4, _0, dest, false );
            DrawPixel( x1 + 3, y1 + 3, _2, dest, false ); // TL
            DrawPixel( x2 - 4, y1 + 3, _2, dest, false ); // TR
            DrawPixel( x1 + 3, y2 - 4, _2, dest, false ); // BL
            DrawPixel( x2 - 4, y2 - 4, _2, dest, false ); // BR

            _.sg.spritebatch.End();

            return dest;
        }
    }
}