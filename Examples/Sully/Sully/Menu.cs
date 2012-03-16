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

            public int cursor;
            public ControlDelegate OnControlUpdate;
            public RenderDelegate OnDraw;
            public MenuBox child = null;

            public MenuBox( ControlDelegate onControlUpdate, RenderDelegate onDraw ) {
                OnControlUpdate = onControlUpdate;
                OnDraw = onDraw;
            }

            public MenuBox( Texture2D _img, int final_x, int final_y, int start_x, int start_y, ControlDelegate onControlUpdate, RenderDelegate onDraw ) {
                image = _img;
                OnControlUpdate = onControlUpdate;
                OnDraw = onDraw;
                x = final_x;
                y = final_y;
                bounds = new Rectangle( x, y, image.Width, image.Height );
                color_bounds = new Rectangle( x, y, image.Width, image.Height );
                color_bounds.Inflate( -2, -2 );

                McgLayer l = _.sg.renderstack.GetLayer( "menu" );
                rendernode = l.AddNode(
                    new McgNode( onDraw, l, start_x, start_y, final_x, final_y, Menu.delay )
                );
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
                PrintText( s, rx, ry, Color.White );
            }

            public void PrintTextRight( string s, int rx, int ry ) {
                PrintTextRight( s, rx, ry, Color.White );
            }

            public void PrintText( string s, int rx, int ry, Color c ) {

                int x = color_bounds.Location.X + rx;
                int y = color_bounds.Location.Y + ry;

                SullyGame game = (SullyGame)VERGEGame.game;
                game.print_string( s, x, y + 1, Color.Black, false );
                game.print_string( s, x + 1, y + 1, Color.Black, false );
                game.print_string( s, x + 1, y, Color.Black, false );
                game.print_string( s, x, y, c, false );
            }

            public void PrintTextRight( string s, int rx, int ry, Color c ) {
                int x = color_bounds.Location.X + rx;
                int y = color_bounds.Location.Y + ry;

                SullyGame game = (SullyGame)VERGEGame.game;
                game.print_right( s, x, y + 1, Color.Black, false );
                game.print_right( s, x + 1, y + 1, Color.Black, false );
                game.print_right( s, x + 1, y, Color.Black, false );
                game.print_right( s, x, y, c, false );
            }
        }

        public int itemSubmenu = 0;

        public MenuBox mainBox, commandBox, smallBox; // statusBox;
        public MenuBox itemBox, skillBox, equipBox, statusBox, optionBox, saveBox;
        public MenuBox partyBox;

        public static Texture2D activeBgColor, inactiveBgColor;

        public MenuState state;

        public MenuBox activeMenu, highlightedMenu;

        public Dictionary<string, MenuBox> menus = new Dictionary<string, MenuBox>();
        public String[] menuOrder;

        public Texture2D GetHighlightBg( bool isOn ) {
            if( isOn ) {
                return activeBgColor;
            }

            return inactiveBgColor;
        }

        public static string getFormattedTime( TimeSpan ts ) {

            string t = _.sg.stopWatch.Elapsed.Hours + ":";

            if( _.sg.stopWatch.Elapsed.Minutes < 10 ) {
                t += "0";
            }
            t += _.sg.stopWatch.Elapsed.Minutes + ":";

            if( _.sg.stopWatch.Elapsed.Seconds < 10 ) {
                t += "0";
            }
            t += _.sg.stopWatch.Elapsed.Seconds;

            return t;
        }

        public void LeaveMainMenu() {
            activeMenu = commandBox;
            highlightedMenu = commandBox;
            mainBox.child = partyBox;
        }

        public Menu() {
            Color[] boxcolors = new Color[3];
            boxcolors[0] = new Color( 0, 0, 0 );
            boxcolors[1] = new Color( 112, 112, 112 );
            boxcolors[2] = new Color( 144, 144, 144 );

            activeBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            activeBgColor.SetData( new[] { new Color( new Vector4( 140, 0, 140, 63 ) ) } );

            inactiveBgColor = new Texture2D( _.sg.GraphicsDevice, 1, 1, false, SurfaceFormat.Color );
            inactiveBgColor.SetData( new[] { new Color( new Vector4( 0, 0, 0, 63 ) ) } );


            ControlDelegate cd1 = ( DirectionalButtons dir, VERGEActions action ) => {
                if( commandBox.child != null ) {
                    commandBox.child.OnControlUpdate( dir, action );
                } else {
                    throw new System.InvalidOperationException( "The commandbox is always supposed to have a child set." );
                }
            };

            ControlDelegate updateCommand = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    DismissMenu();
                }

                if( action.confirm.pressed ) {
                    mainBox.child = activeMenu = menus[menuOrder[commandBox.cursor]];
                    highlightedMenu = mainBox;
                }

                if( dir.up.DelayPress() ) {
                    commandBox.cursor--;
                    if( commandBox.cursor < 0 ) commandBox.cursor = 5;
                } else if( dir.down.DelayPress() ) {
                    commandBox.cursor++;
                    if( commandBox.cursor > 5 ) commandBox.cursor = 0;
                }
            };

            SullyGame game = _.sg;
            
            RenderDelegate drawMainbox = ( int x, int y ) => {

                int mainBoxMarginX = 8;
                int mainBoxMarginY = 5;

                mainBox.UpdateBounds( x, y );

                game.spritebatch.Draw( GetHighlightBg(highlightedMenu == mainBox), mainBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( mainBox.image, mainBox.bounds, Color.White );

                if( mainBox.child != null ) {
                    mainBox.child.OnDraw( x + mainBoxMarginX, y + mainBoxMarginY );
                } else {

                }
            };

            RenderDelegate drawCommandbox = ( int x, int y ) => {
                commandBox.UpdateBounds( x, y );
                game.spritebatch.Draw( GetHighlightBg(highlightedMenu == commandBox), commandBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( commandBox.image, commandBox.bounds, Color.White );

                int mx = 15;
                int my = 15;
                int yOffs = 5;
                int mi = 0;

                commandBox.PrintText( ">", mx - 10, yOffs + my * commandBox.cursor );

                for( int i = 0; i < menuOrder.Length; i++ ) {
                    commandBox.PrintText( menuOrder[i], mx, yOffs + my * mi++ );
                }
            };

            RenderDelegate drawSmallbox = ( int x, int y ) => {

                smallBox.UpdateBounds( x, y );
                game.spritebatch.Draw( GetHighlightBg(highlightedMenu == commandBox), smallBox.color_bounds, Color.White * .5f );
                game.spritebatch.Draw( smallBox.image, smallBox.bounds, Color.White );

                int y1 = 0;
                string m = "" + _.sg.getMoney();

                smallBox.PrintText( "Clams:", 6, y1 ); y1 += 10;
                smallBox.PrintTextRight( m, 61, y1 ); y1 += 20;
                smallBox.PrintText( Menu.getFormattedTime(_.sg.stopWatch.Elapsed), 6, y1 );
            };

            mainBox = new MenuBox( _.MakeBox( 220, 220, boxcolors ), 10, 10, -220, 10, cd1, drawMainbox ); 
            commandBox = new MenuBox( _.MakeBox( 70, 160, boxcolors ), 240, 10, 320, 10, updateCommand, drawCommandbox ); 
            smallBox = new MenuBox( _.MakeBox( 70, 50, boxcolors ), 240,180, 320, 180, null, drawSmallbox );

            this.activeMenu = this.commandBox;
            this.highlightedMenu = this.commandBox;
            state = MenuState.Active;

            menuOrder = new String[6];
            menuOrder[0] = "ITEM";
            menuOrder[1] = "SKILL";
            menuOrder[2] = "EQUIP";
            menuOrder[3] = "STATUS";
            menuOrder[4] = "OPTION";
            menuOrder[5] = "SAVE";

            RenderDelegate drawItem = ( int x, int y ) => { 

                switch(itemSubmenu) {
                    case 0:
                        itemBox.PrintText( "Supplies", 0, 0 );
                        break;
                    case 1:
                        itemBox.PrintText( "Equipment", 0, 0 );
                        break;
                    case 2:
                        itemBox.PrintText( "Key", 0, 0 );
                        break;
                }

                if(  _.sg.inventory.consumables.items.Count == 0 ) {
                    itemBox.PrintText( "No items.", 0, 0 );
                } else {

                    itemBox.PrintText( ">", x, y + itemBox.cursor * 10 );
                    
                    for( int i = 0; i < _.sg.inventory.consumables.items.Count; i++ ) {

                        if( i != itemBox.cursor ) {
                            _.DrawIcon( i, x + 10, y + 4 + ( i * 10 ), i != itemBox.cursor );
                        }

                        itemBox.PrintText(  _.sg.inventory.consumables.items[i].item.name, x + 30, y + ( i * 10 ) );
                        itemBox.PrintTextRight( "" + _.sg.inventory.consumables.items[i].quant, x + 200, y + ( i * 10 ) ); 
                    }

                    int j = itemBox.cursor;
                    _.DrawIcon( j, x + 10, y + ( j * 10 ), false );
                }
            };

            RenderDelegate drawSkill = ( int x, int y ) => { 
                itemBox.PrintText( "Skill...", x, y );
            };

            RenderDelegate drawEquip = ( int x, int y ) => { 
                itemBox.PrintText( "Equip...", x, y );
            };

            RenderDelegate drawStatus = ( int x, int y ) => { 
                itemBox.PrintText( "Status...", x, y );
            };

            RenderDelegate drawOption = ( int x, int y ) => { 
                itemBox.PrintText( "Option...", x, y );
            };

            RenderDelegate drawSave = ( int x, int y ) => { 
                itemBox.PrintText( "Save...", x, y );
            };

            RenderDelegate drawParty = ( int x, int y ) => {

                int i = 0;
                foreach( PartyMember pm in _.sg.party.getMembers() ) {
                    int _y = y + ( 43 * i );
                    int _x = x - 8;

                    pm.ent.DrawAt( new Rectangle( x, _y, 16, 32 ), 0 );
                    partyBox.PrintText( pm.name, _x + 32, _y );
                    partyBox.PrintText( pm.klass, _x + 90, _y, Color.LightGray );
                    partyBox.PrintTextRight( "LV.    ", x + 200, _y, Color.LightGray );
                    partyBox.PrintTextRight( "" + pm.level, x + 200, _y, Color.LightGray );

                    partyBox.PrintText( "HP: " + pm.cur_hp + "/" + pm.getStat( Stat.HP ) , _x + 33, _y + 10 );
                    partyBox.PrintText( "MP: " + pm.cur_mp + "/" + pm.getStat( Stat.MP ), _x + 32, _y + 20 );

                    i++;
                }

            };

            ControlDelegate updateItem = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }

                int menuMax = 0;
                switch( itemSubmenu ) {
                    case 0:
                        menuMax = _.sg.inventory.consumables.items.Count;
                        break;
                    case 1:
                        menuMax = _.sg.inventory.equipment.items.Count;
                        break;
                    case 2:
                        menuMax = _.sg.inventory.key.items.Count;
                        break;
                }

                if( dir.up.DelayPress() ) {
                    itemBox.cursor--;
                    if( itemBox.cursor < 0 ) itemBox.cursor = menuMax-1;
                } else if( dir.down.DelayPress() ) {
                    itemBox.cursor++;
                    if( itemBox.cursor >= menuMax ) itemBox.cursor = 0;
                }

                if( dir.left.DelayPress() ) {
                    itemSubmenu--;
                    itemBox.cursor = 0;
                    if( itemSubmenu < 0 ) itemSubmenu = 2;
                } else if( dir.right.DelayPress() ) {
                    itemSubmenu++;
                    itemBox.cursor = 0;
                    if( itemSubmenu > 2 ) itemSubmenu = 0;
                }
  
            };

            ControlDelegate updateSkill = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };

            ControlDelegate updateEquip = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };

            ControlDelegate updateStatus = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };
            
            ControlDelegate updateOption = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };

            ControlDelegate updateSave = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };

            ControlDelegate updateParty = ( DirectionalButtons dir, VERGEActions action ) => {
                if( action.cancel.pressed ) {
                    LeaveMainMenu();
                }
            };

            itemBox = new MenuBox(updateItem, drawItem );
            skillBox = new MenuBox( updateSkill, drawSkill );
            equipBox = new MenuBox( updateEquip, drawEquip );
            statusBox = new MenuBox( updateStatus, drawStatus );
            optionBox = new MenuBox( updateOption, drawOption );
            saveBox = new MenuBox( updateSave, drawSave );

            menus[menuOrder[0]] = itemBox;
            menus[menuOrder[1]] = skillBox;
            menus[menuOrder[2]] = equipBox;
            menus[menuOrder[3]] = statusBox;
            menus[menuOrder[4]] = optionBox;
            menus[menuOrder[5]] = saveBox;

            partyBox = new MenuBox( updateParty, drawParty );
            mainBox.child = partyBox;
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

            commandBox.cursor = 0;
        }

        public void HandleInput( DirectionalButtons dir, VERGEActions action ) {
            this.activeMenu.OnControlUpdate( dir, action );
        }

        public void Update() { }

        public static int delay = 200;
    }

    public enum MenuState { Hidden, Active, Animating }
}
