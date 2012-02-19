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
    public class _ {
        public static SullyGame sg;

        public static int
            T_DARIN = 1,
            T_SARA = 2,
            T_DEXTER = 3,
            T_CRYSTAL = 4,
            T_GALFREY = 5,
            T_STAN = 6,
            T_SULLY = 7,
            T_BUNNY = 8,
            T_BIRD = 9,
            T_BUBBA = 10,
            T_SANCHO = 11,
            T_LANCE = 12,
            T_PAXTON = 13;

        public static void init( SullyGame sg ) {
            _.sg = sg;
        }

        public static void textbox( int port, string s1, string s2, string s3 ) {
            sg.textbox( s1, s2, s3, port );
        }

        public static void textbox( string s1, string s2, string s3 ) {
            sg.textbox( s1, s2, s3 );
        }
    }

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
            _.init( this );
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

            //load the tetxbox resources
            init_textbox();

            // music stuff
            MediaPlayer.IsRepeating = true;
            Song song = Content.Load<Song>( "troupe_-_cabedge_sailing" );
            MediaPlayer.Play( song );

            // load up the map
            VERGEMap.switch_map( "paradise_isle2" );

            /// spawn the player
            player = map.spawn_entity( 13, 19, "darin" );

            //
            this.hook_render = script<RenderLayerDelegate>( "draw_UI" );

            system_font = Content.Load<SpriteFont>( "RondaSeven" );
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
            if( Textbox.state != TextboxState.Hidden ) Textbox.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public void textbox( String str_1, String str_2, String str_3, int speechIdx = 0 ) {
            VERGEGame.game.lock_player();
            Textbox.reset();
            Textbox.lines.Add( str_1 );
            Textbox.lines.Add( str_2 );
            Textbox.lines.Add( str_3 );
            Textbox.activeSpeechIdx = speechIdx;
            Textbox.state = TextboxState.Printing;
        }

        private void init_textbox() {
            Textbox.image = Content.Load<Texture2D>( "textbox" );
            Textbox.speechPortraits = Content.Load<Texture2D>( "speech" );
            Textbox.bounds = new Rectangle( 0, 0, Textbox.image.Width, Textbox.image.Height );

            int yloc = screen.height - Textbox.bounds.Height - 4;

            Textbox.bounds.Offset( ( screen.width - Textbox.bounds.Width ) / 2, yloc );

            Textbox.speech_bounds = new Rectangle( 2, yloc - 33, 32, 32 ); 

            Textbox.inner_bounds = Textbox.bounds; // copy value            
            Textbox.color_bounds = Textbox.bounds;
            Textbox.inner_bounds.Inflate( -Textbox.horizontal_padding, -Textbox.vertical_padding );
            Textbox.color_bounds.Inflate( -2, -2 );
        }
    }
}