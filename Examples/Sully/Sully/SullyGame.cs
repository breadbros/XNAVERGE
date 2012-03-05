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

namespace Sully
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SullyGame : VERGEGame
    {
        public Textbox textbox;
        public Menu mainMenu;
        McGrenderStack mcg;

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


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {

            Console.WriteLine( "SullyGmae::LoadContent(), mothafuckas!" );

            // always do this first
            base.LoadContent();

            //load the tetxbox resources
            init_textbox();

            system_font = Content.Load<SpriteFont>( "RondaSeven" );

            // load up the map
            VERGEMap.switch_map( "paradise_isle2" );

            this.hook_render = script<RenderLayerDelegate>( "draw_UI" );

            /// spawn the player

            //player = map.spawn_entity( 13, 19, "darin" );     
            player = map.spawn_entity( 63, 59, "darin" );
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
        protected override void Update(GameTime gameTime)
        {
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
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        private void init_textbox() {
            Texture2D speechPortraits = Content.Load<Texture2D>( "speech" );
            textbox = new Textbox( screen.width, screen.height, speechPortraits, this );
            mainMenu = new Menu();
        }
    }
}