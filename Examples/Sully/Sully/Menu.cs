using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully {
    
    public class Menu {

        public class MenuBox {
             
             public Texture2D image;
             public int x, y;
             public Rectangle bounds, color_bounds;

             public MenuBox( Texture2D _img, int _x, int _y ) {
                 image = _img;
                 x = _x;
                 y = _y;
                 bounds = new Rectangle( x, y, image.Width, image.Height );
                 color_bounds = new Rectangle( x, y, image.Width, image.Height );
                 color_bounds.Inflate( -2, -2 );
             }
        }

        public MenuBox mainBox, commandBox, smallBox; // statusBox;

        public Texture2D activeBgColor, inactiveBgColor;

        public MenuState state;

        public Menu() {

            Color[] boxcolors = new Color[3];
            boxcolors[0] = new Color( 0, 0, 0 );
            boxcolors[1] = new Color( 112, 112, 112 );
            boxcolors[2] = new Color( 144, 144, 144 );

            activeBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            activeBgColor.SetData( new[] { new Color( new Vector4( 140, 0, 140, 63 ) ) } );

            inactiveBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            inactiveBgColor.SetData( new[] { new Color( new Vector4( 0, 0, 0, 63 ) ) } );

            mainBox = new MenuBox( _.MakeBox( 220, 220, boxcolors ), 10, 10); 
            commandBox = new MenuBox(_.MakeBox( 70, 160, boxcolors ), 240,10); 
            smallBox = new MenuBox(_.MakeBox( 70, 50, boxcolors ), 240,180);    

            state = MenuState.Active;    
        }

        public void Update() { }

        public void Draw( SullyGame game ) { }

        public void _initMenu( SullyGame game ) {
            McgLayer l = game.renderstack.AddLayer("menu");
            
            Action a1 = () => {
                game.spritebatch.Draw( inactiveBgColor, mainBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( mainBox.image, mainBox.bounds, Color.White );
            };

            Action a2 = () => {
                game.spritebatch.Draw( activeBgColor, commandBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( commandBox.image, commandBox.bounds, Color.White );
            };

            Action a3 = () => {
                game.spritebatch.Draw( inactiveBgColor, smallBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( smallBox.image, smallBox.bounds, Color.White );
            };
                
            l.AddNode(
                new McgNode( a1, l, 0, 0, 300, 300, 3000 )
            );

            l.AddNode(
                new McgNode( a2, l, 0, 0)
            );

            l.AddNode(
                new McgNode( a3, l, 0, 0)
            );
        }

    }

    public enum MenuState { Hidden, Active, Animating }
}
