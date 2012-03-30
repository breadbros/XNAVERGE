using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    public partial class _ {
        public struct PrimitiveVertex {
            public Vector2 Position;
            public Color Color;

            public PrimitiveVertex (int x, int y, Color color) {
                Position = new Vector2(x, y);
                Color = color;
            }
        }

        public static VertexDeclaration PrimitiveVertexDeclaration;
        public static BasicEffect PrimitiveEffect;

        public static Texture2D pixel;
        public static SpriteBatch sb;

        private static void initBox (SullyGame sg) {
            PrimitiveVertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
            PrimitiveEffect = new BasicEffect(sg.GraphicsDevice);
        }

        private static void SetupForPrimitiveDraw (GraphicsDevice gd) {
            PrimitiveEffect.LightingEnabled = false;
            PrimitiveEffect.FogEnabled = false;
            PrimitiveEffect.VertexColorEnabled = true;
            PrimitiveEffect.TextureEnabled = false;
            PrimitiveEffect.World = Matrix.Identity;
            PrimitiveEffect.View = Matrix.Identity;
            PrimitiveEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, gd.Viewport.Width, gd.Viewport.Height, 0, 1, -1
            );

            PrimitiveEffect.CurrentTechnique.Passes.First().Apply();

            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullNone;
        }

        public static void DrawPixel( GraphicsDevice gd, int x, int y, Color color, Texture2D target ) {
            SetupForPrimitiveDraw(gd);
            gd.DrawUserPrimitives(
                PrimitiveType.LineList, new [] {
                    new PrimitiveVertex(x, y, color),
                    new PrimitiveVertex(x + 1, y, color)
                }, 0, 1, PrimitiveVertexDeclaration
            );
        }

        public static void DrawLine (GraphicsDevice gd, int x1, int y1, int w, int h, Color color) {
            SetupForPrimitiveDraw(gd);

            int x2, y2;
            if ((w >= 1) && (h == 1)) {
                x2 = x1 + w;
                y2 = y1;
            } else if ((h >= 1) && (w == 1)) {
                x2 = x1;
                y2 = y1 + h;
            } else {
                throw new Exception("Must be horizontal or vertical");
            }

            gd.DrawUserPrimitives(
                PrimitiveType.LineList, new[] {
                    new PrimitiveVertex(x1, y1, color),
                    new PrimitiveVertex(x2, y2, color)
                }, 0, 1, PrimitiveVertexDeclaration
            );
        }

        public static void DrawRect (GraphicsDevice gd, int x1, int y1, int x2, int y2, Color color) {
            int w = x2 - x1;
            int h = y2 - y1;

            DrawLine(gd, x1, y1, 1, h, color );
            DrawLine(gd, x1, y1, w, 1, color);
            DrawLine(gd, x2, y1, 1, h + 1, color);
            DrawLine(gd, x1, y2, w, 1, color);
        }

        public static void DrawRectFill (GraphicsDevice gd, int x1, int y1, int x2, int y2, Color color) {
            x2 += 1;
            y2 += 1;

            SetupForPrimitiveDraw(gd);
            gd.DrawUserPrimitives(
                PrimitiveType.TriangleStrip, new[] {
                    new PrimitiveVertex(x1, y1, color),
                    new PrimitiveVertex(x2, y1, color),
                    new PrimitiveVertex(x1, y2, color),
                    new PrimitiveVertex(x2, y2, color),
                    new PrimitiveVertex(x2, y1, color)
                }, 0, 2, PrimitiveVertexDeclaration
            );
        }

        public static Texture2D MakeBox( int w, int h, Color[] boxcolors ) {

            RenderTarget2D dest = new RenderTarget2D( _.sg.GraphicsDevice, w, h, true, SurfaceFormat.Color, DepthFormat.Depth24 );

            _.sg.GraphicsDevice.SetRenderTarget( dest );
            _.sg.GraphicsDevice.Clear( ClearOptions.Target, Color.Transparent, 0, 0 );

            Color _0 = boxcolors[0];
            Color _1 = boxcolors[1];
            Color _2 = boxcolors[2];

            int x1 = 0;
            int y1 = 0;
            
            int x2 = x1 + w;
            int y2 = y1 + h;

            var gd = _.sg.GraphicsDevice;
            
            DrawLine( gd, x1, y1 + 2, 1, (y2 - 2)-(y1 + 2), _0 ); // TL -> BL
            DrawLine(gd, x1 + 2, y1, (x2 - 2) - (x1 + 2), 1, _0); // TL -> TR

            DrawLine(gd, x2 - 1, y1 + 2, 1, (y2 - 2) - (y1 + 2), _0); // BR -> TR
            DrawLine(gd, x1 + 2, y2 - 1, (x2 - 2) - (x1 + 2), 1, _0); // BR -> BL

            DrawRect(gd, x1 + 1, y1 + 1, x2 - 2, y2 - 2, _1);
            DrawPixel(gd, x1 + 1, y1 + 1, _0, dest); // TL
            DrawPixel(gd, x2 - 2, y1 + 1, _0, dest); // TR
            DrawPixel(gd, x1 + 1, y2 - 2, _0, dest); // BL
            DrawPixel(gd, x2 - 2, y2 - 2, _0, dest); // BR

            DrawRect(gd, x1 + 2, y1 + 2, x2 - 3, y2 - 3, _2);
            DrawPixel(gd, x1 + 2, y1 + 2, _1, dest); // TL
            DrawPixel(gd, x2 - 3, y1 + 2, _1, dest); // TR
            DrawPixel(gd, x1 + 2, y2 - 3, _1, dest); // BL
            DrawPixel(gd, x2 - 3, y2 - 3, _1, dest); // BR

            DrawRect(gd, x1 + 3, y1 + 3, x2 - 4, y2 - 4, _0);
            DrawPixel(gd, x1 + 3, y1 + 3, _2, dest); // TL
            DrawPixel(gd, x2 - 4, y1 + 3, _2, dest); // TR
            DrawPixel(gd, x1 + 3, y2 - 4, _2, dest); // BL
            DrawPixel(gd, x2 - 4, y2 - 4, _2, dest); // BR
             
            _.sg.GraphicsDevice.SetRenderTarget( null );

            return dest;
        }
    }
}