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
             public int last_x, last_y;
             public int x, y;
             public Rectangle bounds, color_bounds;
             public McgNode rendernode;

             public MenuBox( Texture2D _img, int _x, int _y ) {
                 image = _img;
                 x = _x;
                 y = _y;
                 bounds = new Rectangle( x, y, image.Width, image.Height );
                 color_bounds = new Rectangle( x, y, image.Width, image.Height );
                 color_bounds.Inflate( -2, -2 );
             }

             public void UpdateBounds( int x, int y ) {
                 if( x != last_x || y != last_y ) {
                     last_x = x;
                     last_y = y;

                     bounds = new Rectangle( x, y, image.Width, image.Height );
                     color_bounds = new Rectangle( x, y, image.Width, image.Height );
                     color_bounds.Inflate( -2, -2 );
                 }
             }

             public void PrintText( string s, int rx, int ry ) {

                 int x = color_bounds.Location.X + rx;
                 int y = color_bounds.Location.Y + ry;

                 SullyGame game = (SullyGame)VERGEGame.game;
                 game.print_string( s, x, y + 1, Color.Black, false );
                 game.print_string( s, x + 1, y + 1, Color.Black, false );
                 game.print_string( s, x + 1, y, Color.Black, false );
                 game.print_string( s, x, y, Color.White, false );
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

        public bool CanSummonMenu() {
            return !IsInMenu();
        }

        bool isInMenu = true;
        public bool IsInMenu() {
            return isInMenu;
        }

        public void _enterMenuEvent() {
            this.isInMenu = true;
        }

        public void _exitMenuEvent() {
            this.isInMenu = false;
        }

        public void SummonMenu() {
            mainBox.rendernode.Reverse();
            commandBox.rendernode.Reverse();
            smallBox.rendernode.Reverse();

            mainBox.rendernode.OnStop += _enterMenuEvent;
            mainBox.rendernode.OnStop -= _exitMenuEvent;
        }

        public void DismissMenu() {

            mainBox.rendernode.OnStop -= _enterMenuEvent;
            mainBox.rendernode.OnStop += _exitMenuEvent;

            mainBox.rendernode.Reverse();
            commandBox.rendernode.Reverse();
            smallBox.rendernode.Reverse();
        }

        public void HandleInput(DirectionalButtons dir, VERGEActions action) {
            if( action.cancel.pressed ) {
                DismissMenu();
            }
        }

        public void Update() { }

        //public void Draw( SullyGame game ) { }

        public void _initMenu( SullyGame game ) {
            McgLayer l = game.renderstack.GetLayer( "menu" );

            RenderDelegate a1 = ( int x, int y ) => {
                mainBox.UpdateBounds( x, y );
                game.spritebatch.Draw( inactiveBgColor, mainBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( mainBox.image, mainBox.bounds, Color.White );
            };

            RenderDelegate a2 = ( int x, int y ) => {
                commandBox.UpdateBounds( x, y );
                game.spritebatch.Draw( activeBgColor, commandBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( commandBox.image, commandBox.bounds, Color.White );

                int mx = 15;
                int my = 15;
                int yOffs = 5;
                int mi = 0;

                commandBox.PrintText( "ITEM", mx, yOffs + my * mi++ );
                commandBox.PrintText( "SKILL", mx, yOffs + my * mi++ );
                commandBox.PrintText( "EQUIP", mx, yOffs + my * mi++ );
                commandBox.PrintText( "STATUS", mx, yOffs + my * mi++ );
                commandBox.PrintText( "OPTION", mx, yOffs + my * mi++ );
                commandBox.PrintText( "SAVE", mx, yOffs + my * mi++ );
            };

            RenderDelegate a3 = ( int x, int y ) => {
                smallBox.UpdateBounds( x, y );
                game.spritebatch.Draw( inactiveBgColor, smallBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( smallBox.image, smallBox.bounds, Color.White ); 
            };

            int delay = 200;

            mainBox.rendernode = l.AddNode(
                new McgNode( a1, l, -220, 10, 10, 10, delay )
            );
            //n.DEBUG = true;

            commandBox.rendernode = l.AddNode(
                new McgNode( a2, l, 320, 10, 240, 10, delay )
            );

            smallBox.rendernode = l.AddNode(
                new McgNode( a3, l, 320, 180, 240, 180, delay )
            );
        }
    }

    public enum MenuState { Hidden, Active, Animating }
}
