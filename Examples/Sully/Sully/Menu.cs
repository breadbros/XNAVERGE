﻿using System;
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
            public bool has_party_select = false;

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


            // Draws a sub window with vertical scroll bar and cursor, but draws none of the contents
            // This function is a god amoungst men
            public void MenuDrawSubWindow( int x1, int y1, int x2, int y2, int current_cursor_pos, int entry_y_height, int total_entry_count, int entry_start, int y_fuzz_factor )
                // Pass: Dimensions of box by top left and bottom right coords, current selected entry (pass negative for none),
                //          height of one entry, total number of entries, entry at top of window, and size modifier
                // No error checking, and can display strangely if odd values are passed
            {
                _.setDrawTarget(_.sg.spritebatch);

                int ydiff = y2 - y1 - 8;
                int entry_fit = ( y2 - y1 ) / entry_y_height;
                if( total_entry_count < entry_fit ) entry_fit = total_entry_count;

                _.DrawRect( x1, y1, x2, y2, _.sg.boxcolors[2] );
                _.DrawRect( x2 - 10, y1 + 2, x2 - 2, y2 - 2, _.sg.boxcolors[2] );

                _.DrawRectFill( x2 - 8, y1 + 4 + ( ( ydiff * entry_start ) / total_entry_count ),
                 x2 - 4, y1 + 4 + ( ( ydiff * ( entry_fit + entry_start ) ) / total_entry_count ), _.sg.boxcolors[2] );
                if( current_cursor_pos >= 0 ) {
                    _.DrawRect( x1 + 4, y1 + 4 + ( entry_y_height * ( current_cursor_pos - entry_start ) ), x1 + 10, y1 + entry_y_height - y_fuzz_factor + ( entry_y_height * ( current_cursor_pos - entry_start ) ), _.sg.boxcolors[2] );
                    _.DrawRectFill( x1 + 6, y1 + 6 + ( entry_y_height * ( current_cursor_pos - entry_start ) ), x1 + 8, y1 + entry_y_height - y_fuzz_factor - 2 + ( entry_y_height * ( current_cursor_pos - entry_start ) ), _.sg.boxcolors[2] );
                }
            }

            // This really shouldn't be in the submenu.  Meeeh, porting is fun.
            public void MenuPrintStat( int x, int y, Stat stat, int value ) {
                // Current HP/MP aren't stats in the same sense as the maximums, so they're not drawn by
                // this function. They get drawn by a separate function, MenuBlitCast in menu_system.vc.
                // It's kind of weird, but that's how Zip coded it.
                if( stat == Stat.HP ) { // HP gets drawn in its own place  
                    //PrintTextRight( x + 115 + TextWidth( menu_font[0], "MP:000/000" ), y + 10, screen, menu_font[0], str( value ) );
                    return;
                }
                if( stat == Stat.MP ) { // MP gets drawn in its own place  
                    //PrintTextRight( x + 115 + TextWidth( menu_font[0], "MP:000/000" ), y + 20, screen, menu_font[0], str( value ) );
                    return;
                }
                // Other stats get printed in order in a two-line block. 
                int xpos = ((int)stat) / 2; // This ensures that the stats are printed across two lines.
                int ypos = ((int)stat) % 2; // This ensures that even-numbered stats go on the top row while odd-numbered stats go on the bottom   
                PrintText( stat.ToString(), x + ( 32 * xpos ) - 32, y + 35 + ( 24 * ypos ) ); // print name
                PrintText( ""+value, x + ( 32 * xpos ) - 32, y + 45 + ( 24 * ypos ) );  // print value
            }
        }

        public int itemSubmenu = 0;
        public int partyCursor = -1;

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
            this.partyCursor = -1;
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

                    /// turn on the partyCursor. (-1 is the off state.)
                    if( activeMenu.has_party_select ) {
                        this.partyCursor = 0;
                        activeMenu.cursor = -1; 
                    }

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

                int menu_start = 0;
                if( menu_start + 9 < itemBox.cursor ) {
                    menu_start = itemBox.cursor - 9;
                } else if( menu_start > itemBox.cursor && itemBox.cursor >= 0 ) {
                    menu_start = itemBox.cursor - 1;
                }

                itemBox.PrintText( "Supply", 20, 18, ( itemSubmenu == 0 ) ? Color.White : Color.DarkGray );
                itemBox.PrintText( "Equip", 100, 18, ( itemSubmenu == 1 ) ? Color.White : Color.DarkGray );
                itemBox.PrintText( "Key", 186, 18, ( itemSubmenu == 2 ) ? Color.White : Color.DarkGray );

                int _x = x;
                int _y = y + 20;

                int lineSize = 14;
                int displayNumber = 10;

                ItemSet curInv = _.sg.inventory.consumables;
                switch( itemSubmenu ) {
                    case 0:
                        curInv = _.sg.inventory.consumables;
                        break;
                    case 1:
                        curInv = _.sg.inventory.equipment;
                        break;
                    case 2:
                        curInv = _.sg.inventory.key;
                        break;
                }

                if( curInv.items.Count == 0 ) {
                    itemBox.MenuDrawSubWindow( _x, _y, _x + 200, _y + 180, itemBox.cursor, 13, 1, 0, 0 );
                    itemBox.PrintText( "No items.", _x + 32, _y );

                } else {

                    itemBox.MenuDrawSubWindow( _x, _y, _x + 200, _y + 150, itemBox.cursor, lineSize, curInv.items.Count, menu_start, 4 );
                    _y += 4;

                    for( int i = menu_start; i < curInv.items.Count && menu_start + displayNumber > i; i++ ) {
/*

                        if( master_items[supply_inventory[i].item_ref].use_flag & USE_MENU ) use = 0;
                        else use = 1;
                        PrintString( 55, 56 + ( 13 * ( i - menu_start ) ), screen, menu_font[use], master_items[supply_inventory[i].item_ref].name );
                        PrintRight( 205, 56 + ( 13 * ( i - menu_start ) ), screen, menu_font[use], str( supply_inventory[i].quant ) );
                        use = icon_get( master_items[supply_inventory[i].item_ref].icon );
                        if( i == menu_item ) TBlit( 35, 54 + ( 13 * ( i - menu_start ) ), use, screen );
                        else TScaleBlit( 35, 58 + ( 13 * ( i - menu_start ) ), 8, 8, use, screen );
                        FreeImage( use );
                        if( menu_start + 10 <= i ) i = _supply_count + 1;
*/
                        if( i != itemBox.cursor ) {
                            _.DrawIcon( curInv.items[i].item.icon, _x + 14, _y + 4 + ( lineSize * ( i - menu_start ) ), i != itemBox.cursor );
                        }

                        itemBox.PrintText( curInv.items[i].item.name, _x + 32, _y + ( lineSize * ( i - menu_start ) ), curInv.items[i].item.use_menu ? Color.White : Color.DarkGray );
                        itemBox.PrintTextRight( "" + curInv.items[i].quant, _x + 180, _y + ( lineSize * ( i - menu_start ) ) );
                    }

                    int j = itemBox.cursor;
                    _.DrawIcon( curInv.items[j].item.icon, _x + 14, _y + ( lineSize * ( j - menu_start ) ), false );

                    itemBox.PrintText( curInv.items[j].item.description, _x, _y + 154 );
                }
            };

            RenderDelegate drawSkill = ( int x, int y ) => { 
                itemBox.PrintText( "Skill...", x, y );
            };

            RenderDelegate drawEquip = ( int x, int y ) => { 
                itemBox.PrintText( "Equip...", x, y );
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

                    if( this.partyCursor == i ) {
                        partyBox.PrintText( ">", x-3, _y );
                    }

                    Color lightColor = ( this.partyCursor >= 0 && this.partyCursor == i ) ? Color.White : Color.DarkGray;
                    Color darkColor = ( this.partyCursor >= 0 && this.partyCursor == i ) ? Color.LightGray : Color.DarkGray;

                    pm.ent.DrawAt( new Rectangle( x, _y, 16, 32 ), 0 );
                    partyBox.PrintText( pm.name, _x + 32, _y, lightColor );
                    partyBox.PrintText( pm.klass, _x + 90, _y, darkColor );
                    partyBox.PrintTextRight( "LV.    ", x + 200, _y, darkColor );
                    partyBox.PrintTextRight( "" + pm.level, x + 200, _y, darkColor );

                    partyBox.PrintText( "HP: " + pm.cur_hp + "/" + pm.getStat( Stat.HP ), _x + 33, _y + 10, lightColor );
                    partyBox.PrintText( "MP: " + pm.cur_mp + "/" + pm.getStat( Stat.MP ), _x + 32, _y + 20, lightColor );

                    i++;
                }
            };

            RenderDelegate drawStatus = ( int x, int y ) => {

                if( this.partyCursor >= 0 && this.partyCursor == statusBox.cursor ) {

                    PartyMember pm = _.sg.party.getMembers()[this.partyCursor];

                    int _x = x + 4;
                    int _y = y + 4;

                    pm.ent.DrawAt( new Rectangle( _x, _y, 16, 32 ), 0 );
                    statusBox.PrintText( pm.name, _x + 24, _y  );
                    statusBox.PrintText( pm.klass, _x + 32, _y + 12 );

                    statusBox.PrintText( "Level:", _x + 112, _y );      statusBox.PrintTextRight( ""+pm.level, _x + 190, _y );
                    statusBox.PrintText( "HP:", _x + 112, _y + 12 );    statusBox.PrintTextRight( "" + pm.cur_hp + "/" + pm.getStat(Stat.HP), _x + 190, _y+12 );
                    statusBox.PrintText( "MP:", _x + 112, _y + 24 );    statusBox.PrintTextRight( "" + pm.cur_mp + "/" + pm.getStat( Stat.MP ), _x + 190, _y + 24 );

                    //

                    foreach( Stat s in Enum.GetValues(typeof(Stat)) ) {
                        statusBox.MenuPrintStat(_x+8, _y+12, s, pm.getStat(s));
                    }

                    statusBox.PrintText( "EXP " + pm.cur_xp, _x + 8, _y + 100 );
                    statusBox.PrintText( "NEXT " + pm.getXpUntilNextLevel(), _x + 104, _y + 100 );

                    int autotext;
                    statusBox.PrintText( "DESCRIPTION - FIX WITH AUTOTEXT LATER", _x, _y + 170 );
                    statusBox.PrintText( pm.description, _x, _y + 182 );


                } else {
                    drawParty( x, y );
                }
            };

            ControlDelegate updatePartySelect = ( DirectionalButtons dir, VERGEActions action ) => {

                int partySize = _.sg.party.getMembers().Length;

                if( dir.up.DelayPress() || dir.left.DelayPress() ) {
                    this.partyCursor--;
                    if( this.partyCursor < 0 ) this.partyCursor = partySize - 1;
                } else if( dir.down.DelayPress() || dir.right.DelayPress() ) {
                    this.partyCursor++;
                    if( this.partyCursor >= partySize ) this.partyCursor = 0;
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

                if( this.partyCursor >= 0 && statusBox.cursor < 0 ) {

                    updatePartySelect( dir, action );

                    if( action.confirm.pressed ) {
                        statusBox.cursor = this.partyCursor;
                    }
                    
                    if( action.cancel.pressed ) {
                        LeaveMainMenu();
                    }
                
                } else {
                    if( action.cancel.pressed ) {
                        statusBox.cursor = -1;
                    }
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

            skillBox.has_party_select = true;
            equipBox.has_party_select = true;
            statusBox.has_party_select = true;

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
        bool isLeavingMenu = false;
        bool isEnteringMenu = false;
        public bool IsInMenu() {
            return isInMenu;
        }

        public void _enterMenuEvent() {
            this.isInMenu = true;
            this.isEnteringMenu = false;
        }

        public void _exitMenuEvent() {
            this.isInMenu = false;
            isLeavingMenu = false;
        }

        public void SummonMenu() {
            isLeavingMenu = false;
            if( isEnteringMenu ) {
                return;
            }

            isEnteringMenu = true;

            this.activeMenu = this.commandBox;

            mainBox.rendernode.Reverse();
            commandBox.rendernode.Reverse();
            smallBox.rendernode.Reverse();

            mainBox.rendernode.OnStop += _enterMenuEvent;
            mainBox.rendernode.OnStop -= _exitMenuEvent;
        }

        public void DismissMenu() {

            if( isLeavingMenu ) {
                return;
            }

            isLeavingMenu = true;

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
