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
             
            private Texture2D image;
            private int last_x, last_y;
            private int x, y;
            private Rectangle bounds, color_bounds;
            private McgNode rendernode;

            private int cursor;
            private ControlDelegate OnControlUpdate;
            private RenderDelegate OnDraw;

            private MenuBox _mb;

            public Rectangle getBounds() {
                return _mb.bounds;
            }

            public Rectangle getColorBounds() {
                return _mb.color_bounds;
            }

            public Texture2D getImage() {
                return _mb.image;
            }

            public void onControlUpdate( DirectionalButtons dir, VERGEActions action ) {
                this.OnControlUpdate(dir, action);
            }

            public void setRendernode( McgNode node ) {
                this.rendernode = node;
            }

            public McgNode getRendernode() {
                return this.rendernode;
            }

            public int getCursor() {
                return _mb.cursor;
            }

            public void setCursor( int c ) {
                _mb.cursor = c;
            }

            public MenuBox( MenuBox parent, ControlDelegate onControlUpdate, RenderDelegate onRender ) {
                _mb = parent;
                OnControlUpdate = onControlUpdate;

                image = _mb.image;
                x = _mb.x;
                y = _mb.y;
                bounds = _mb.bounds;
                color_bounds = _mb.color_bounds;

                RenderDelegate myDraw = ( int x1, int y1 ) => {
                    _mb.OnDraw( x1, y1 );
                    onRender( x1, y1 );
                };

                OnDraw = myDraw;
            }

            public MenuBox( Texture2D _img, int _x, int _y, int start_x, int start_y, ControlDelegate onControlUpdate, RenderDelegate onRender ) {
                _mb = this;
                 
                image = _img;
                OnControlUpdate = onControlUpdate;
                x = _x;
                y = _y;
                bounds = new Rectangle( x, y, image.Width, image.Height );
                color_bounds = new Rectangle( x, y, image.Width, image.Height );
                color_bounds.Inflate( -2, -2 );
                OnDraw = onRender;
                McgLayer l = _.sg.renderstack.GetLayer( "menu" );

                this.setRendernode(
                    l.AddNode( new McgNode( onRender, l, start_x, start_y, x, y, Menu.delay ) )
                );
            }

             public void UpdateBounds( int x, int y ) {
                 if( x != _mb.last_x || y != _mb.last_y ) {
                     _mb.last_x = x;
                     _mb.last_y = y;

                     _mb.bounds = new Rectangle( x, y, _mb.image.Width, _mb.image.Height );
                     _mb.color_bounds = new Rectangle( x, y, _mb.image.Width, _mb.image.Height );
                     _mb.color_bounds.Inflate( -2, -2 );
                 }
             }

             public void PrintText( string s, int rx, int ry ) {

                 int x = _mb.color_bounds.Location.X + rx;
                 int y = _mb.color_bounds.Location.Y + ry;

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

        public MenuBox activeMenu;

        public static int delay = 200;

        public Dictionary<string, MenuBox> menus = new Dictionary<string, MenuBox>();
        public String[] menuOrder;
        public Menu() {

            Color[] boxcolors = new Color[3];
            boxcolors[0] = new Color( 0, 0, 0 );
            boxcolors[1] = new Color( 112, 112, 112 );
            boxcolors[2] = new Color( 144, 144, 144 );

            activeBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            activeBgColor.SetData( new[] { new Color( new Vector4( 140, 0, 140, 63 ) ) } );

            inactiveBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            inactiveBgColor.SetData( new[] { new Color( new Vector4( 0, 0, 0, 63 ) ) } );

            ControlDelegate updateCommand = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    DismissMenu();
                }

                if( action.confirm.pressed ) {
                    activeMenu = menus[menuOrder[commandBox.getCursor()]];
                }

                if( dir.up.DelayPress() ) {
                    commandBox.setCursor( commandBox.getCursor() - 1 );
                    if( commandBox.getCursor() < 0 ) commandBox.setCursor( menuOrder.Length-1 );
                } else if( dir.down.DelayPress() ) {
                    commandBox.setCursor( commandBox.getCursor() + 1 );
                    if( commandBox.getCursor() > 5 ) commandBox.setCursor( 0 );
                }
            };

            SullyGame game = _.sg;

            RenderDelegate drawMainBox = ( int x, int y ) => {
                mainBox.UpdateBounds( x, y );
                game.spritebatch.Draw( inactiveBgColor, mainBox.getColorBounds(), Color.White * .5f );
                game.spritebatch.Draw( mainBox.getImage(), mainBox.getBounds(), Color.White );
            };

            RenderDelegate drawCommandBox = ( int x, int y ) => {
                commandBox.UpdateBounds( x, y );
                game.spritebatch.Draw( activeBgColor, commandBox.getColorBounds(), Color.White * .5f );
                game.spritebatch.Draw( commandBox.getImage(), commandBox.getBounds(), Color.White );

                int mx = 15;
                int my = 15;
                int yOffs = 5;
                int mi = 0;

                commandBox.PrintText( ">", mx - 10, yOffs + my * commandBox.getCursor() );

                for( int i = 0; i < menuOrder.Length; i++ ) {
                    commandBox.PrintText( menuOrder[i], mx, yOffs + my * mi++ );
                }

            };

            RenderDelegate drawSmallBox = ( int x, int y ) => {
                smallBox.UpdateBounds( x, y );
                game.spritebatch.Draw( inactiveBgColor, smallBox.getColorBounds(), Color.White * .5f );
                game.spritebatch.Draw( smallBox.getImage(), smallBox.getBounds(), Color.White );
            };

            mainBox = new MenuBox( _.MakeBox( 220, 220, boxcolors ), 10, 10, -220, 10, null, drawMainBox );
            commandBox = new MenuBox( _.MakeBox( 70, 160, boxcolors ), 240, 10, 320, 10, updateCommand, drawCommandBox );
            smallBox = new MenuBox( _.MakeBox( 70, 50, boxcolors ), 240, 180, 320, 180, null, drawSmallBox );

            this.activeMenu = this.commandBox;
            state = MenuState.Active;

            menuOrder = new String[6];
            menuOrder[0] = "ITEM";
            menuOrder[1] = "SKILL";
            menuOrder[2] = "EQUIP";
            menuOrder[3] = "STATUS";
            menuOrder[4] = "OPTION";
            menuOrder[5] = "SAVE";


            ControlDelegate updateItems = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            ControlDelegate updateSkills = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            ControlDelegate updateEquip = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            ControlDelegate updateStatus = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            ControlDelegate updateOption = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            ControlDelegate updateSave = ( DirectionalButtons dir, VERGEActions action ) => {

            };

            RenderDelegate drawItems = ( int x, int y ) => {

            };

            RenderDelegate drawSkills = ( int x, int y ) => {

            };

            RenderDelegate drawEquip = ( int x, int y ) => {

            };

            RenderDelegate drawStatus = ( int x, int y ) => {

            };

            RenderDelegate drawOption = ( int x, int y ) => {

            };

            RenderDelegate drawSave = ( int x, int y ) => {

            };

            menus[menuOrder[0]] = new MenuBox( mainBox, updateItems, drawItems );
            menus[menuOrder[1]] = new MenuBox( mainBox, updateSkills, drawSkills );
            menus[menuOrder[2]] = new MenuBox( mainBox, updateEquip, drawEquip );
            menus[menuOrder[3]] = new MenuBox( mainBox, updateStatus, drawStatus );
            menus[menuOrder[4]] = new MenuBox( mainBox, updateOption, drawOption );
            menus[menuOrder[5]] = new MenuBox( mainBox, updateSave, drawSave );
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
            this.activeMenu = this.commandBox;

            mainBox.getRendernode().Reverse();
            commandBox.getRendernode().Reverse();
            smallBox.getRendernode().Reverse();

            mainBox.getRendernode().OnStop += _enterMenuEvent;
            mainBox.getRendernode().OnStop -= _exitMenuEvent;
        }

        public void DismissMenu() {

            mainBox.getRendernode().OnStop -= _enterMenuEvent;
            mainBox.getRendernode().OnStop += _exitMenuEvent;

            mainBox.getRendernode().Reverse();
            commandBox.getRendernode().Reverse();
            smallBox.getRendernode().Reverse();

            commandBox.setCursor(0);
        }

        public void HandleInput( DirectionalButtons dir, VERGEActions action ) {
            this.activeMenu.onControlUpdate( dir, action );
        }

        public void Update() { }
    }

    public enum MenuState { Hidden, Active, Animating }
}
