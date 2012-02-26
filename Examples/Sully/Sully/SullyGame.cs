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
            base.Update(gameTime);
            if( textbox.state != TextboxState.Hidden ) textbox.Update();
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
            textbox = new Textbox();

            //textbox.image = Content.Load<Texture2D>( "textbox" );
            
            Color[] boxcolors = new Color[3];
            boxcolors[0] = Color.Red;
            boxcolors[1] = Color.Green;
            boxcolors[2] = Color.Blue;

            textbox.image = _.MakeBox( 300, 100, boxcolors ); 
            
            textbox.speechPortraits = Content.Load<Texture2D>( "speech" );
            textbox.bounds = new Rectangle( 0, 0, textbox.image.Width, textbox.image.Height );

            int yloc = screen.height - textbox.bounds.Height - 4;

            textbox.bounds.Offset( ( screen.width - textbox.bounds.Width ) / 2, yloc );

            textbox.speech_bounds = new Rectangle( 2, yloc - 33, 32, 32 );

            textbox.inner_bounds = textbox.bounds; // copy value            
            textbox.color_bounds = textbox.bounds;
            textbox.inner_bounds.Inflate( -textbox.horizontal_padding, -textbox.vertical_padding );
            textbox.color_bounds.Inflate( -2, -2 );
        }
    }
}