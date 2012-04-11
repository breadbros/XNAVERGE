using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XNAVERGE;

namespace Sully {

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SullyGame : VERGEGame {

        public Party party;
        public Inventory inventory;
        public Textbox textbox;
        public MainMenu mainMenu;
        public Color[] boxcolors;
        public Color menuColor;
        public SaveManager saves;

        public McGrenderStack mcg;

        // this is the time elapsed when the current game was loaded. The total time elapsed is this plus
        // the session timer's elapsed time. 
        public TimeSpan saved_time;
        public TimeSpan total_time { get { return saved_time + stopWatch.Elapsed; } } // exact total playtime as of now

        public int money = 0;
        public int getMoney() {
            return money;
        }

        public bool inMenu = false;
       
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            _.init( this );
            main_assembly = System.Reflection.Assembly.GetExecutingAssembly(); // tell the library where to find map scripts
            global = new SullyGlobalScripts(this);

            SkillType.initSkillTypes();
            Klass.initClasses();
            PartyData.InitializePartyData();

            Element.initElements();
            Status.initStatuses();

            Skill.initSkills();
            Item.initItems();

            mcg = new McGrenderStack();
            mcg.AddLayer( "menu" );
            mcg.AddLayer( "textbox" );
            this.setMcGrender( mcg );
            
            this.game_input_handler = () => {

                if( this.mainMenu.CanSummonMenu() && action.menu.pressed ) {
                    this.mainMenu.SummonMenu();
                } 
                
                if( this.mainMenu.IsInMenu() ) {
                    this.mainMenu.HandleInput( dir, action );
                }

                if( mainMenu.IsInMenu() ) {
                    return false;
                }

                return true;
            };


            boxcolors = new Color[3];
            boxcolors[0] = new Color( 0, 0, 0 );
            boxcolors[1] = new Color( 112, 112, 112 );
            boxcolors[2] = new Color( 144, 144, 144 );

            menuColor = new Color( 128,0,128 );

            inventory = new Inventory();
            saves = new SaveManager(this);

            Random random = new Random();
            foreach( String key in Item.masterItemList.Keys ) {
                inventory.AddItem( Item.masterItemList[key], random.Next( 1, 98 ) );
            }

            saves = new SaveManager(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public Sprite dspr;
        protected override void LoadContent() {

            Console.WriteLine( "SullyGame::LoadContent() called here" );

            // always do this first
            base.LoadContent();

            //load the tetxbox resources
            init_textbox();

            system_font = Content.Load<SpriteFont>( "RondaSeven" );
            //system_font = Content.Load<SpriteFont>( "TitilliumBold" );

            //initFont();


            this.party = new Party( Content );
            party.AddPartyMember( "Darin", 3 );
            party.AddPartyMember("Lance", 1);

            Item i = Item.get( "Mace" );
            inventory.AddItem( i, 1 );
            party.getMembers()[0].equipment["r. hand"].Equip( i, inventory );

            i = Item.get( "Buckler" );
            inventory.AddItem( i, 1 );
            party.getMembers()[0].equipment["l. hand"].Equip( i, inventory );

            i = Item.get( "Jaunty Cap" );
            inventory.AddItem( i, 1 );
            party.getMembers()[0].equipment["acc. 2"].Equip( i, inventory );

            /// spawn the player

                                    
            _.MapSwitch( "paradise_isle2", 13, 19);


            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            ///////////  here's some dumb sprite-demoing stuff!
            dspr = new Sprite("darin.json", "Attack");
            AnimationEndingDelegate del = null;
            del = (Sprite s) => {
                s.set_animation("Idle");
                s.on_done_animating -= del;
            };
            dspr.on_done_animating += del;
            dspr.move_to(150, 150);
            dspr.visible = true;
            hook_render = script<RenderLayerDelegate>("draw_darin");
            dspr.acceleration = new Vector2(-0.0008f, -0.001f);
            ////////// end of sprite demo
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////


            //VERGEMap.switch_map( "underwater" );
            //player = map.spawn_entity( 29, 12, "darin" );            
            //player = map.spawn_entity( 63, 59, "darin" );       // paradise isle debug     
            //saves.save(12);

            player.speed += 100;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            //int new_ticks = gameTime.ElapsedGameTime.Milliseconds;
            _.systime = gameTime.TotalGameTime.Milliseconds;

            base.Update(gameTime);
            if( textbox.state != TextboxState.Hidden ) textbox.Update();
            if( mainMenu.state != MenuState.Hidden ) mainMenu.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) { 
            base.Draw(gameTime);
        }

        private void init_textbox() {
            Texture2D speechPortraits = Content.Load<Texture2D>( "speech" );
            textbox = new Textbox( screen.width, screen.height, speechPortraits, this );
            mainMenu = new MainMenu();
        }
    }
}