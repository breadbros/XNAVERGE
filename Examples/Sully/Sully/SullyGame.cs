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
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            main_assembly = System.Reflection.Assembly.GetExecutingAssembly(); // tell the library where to find map scripts
            global = new SullyGlobalScripts();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Console.WriteLine( "SullyGmae::LoadContent(), mothafuckas!" );

            // always do this first
            base.LoadContent();

            // music stuff
            //MediaPlayer.IsRepeating = true;
            //Song song = Content.Load<Song>( "troupe_-_cabedge_sailing" );
            //MediaPlayer.Play( song );

            // load up the map
            VERGEMap.switch_map( "paradise_isle2" );

            /// spawn the player
            player = map.spawn_entity( 24, 12, "darin" );

            //
            this.hook_render = script<RenderLayerDelegate>( "draw_UI" );

            system_font = Content.Load<SpriteFont>( "Garamond" );
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
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public void textbox( String str_1, String str_2, String str_3 ) {
            VERGEGame.game.lock_player();
            Textbox.reset();
            Textbox.lines.Add( str_1 );
            Textbox.lines.Add( str_2 );
            Textbox.lines.Add( str_3 );
            Textbox.state = TextboxState.Printing;
        }
    }
}
