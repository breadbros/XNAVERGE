using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNAVERGE {
    public partial class VERGEGame : Game {
        public static VERGEGame game; // points to the last VERGEGame initialized (presumably there would only be one)
        public static Random rand = new Random();               
        public ContentManager MapContent; // this is used rather than the standard "Content" contentmanager to handle map-specific data        
        
        public VERGEMap map;
        public Entity player;
        public bool player_controllable; // true if player responds to input
        public bool player_tile_obstruction; // true if player uses tile-based, rather than pixel-based, obstruction

        public InputManager input;
        public DirectionalButtons dir;
        public VERGEActions action;

        public VERGEGame() : base() {
            VERGEGame.game = this;

            // Set up timing
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10); // 100 ticks per second
            _tick = 0;            
            
            // Set up graphics
            graphics = new GraphicsDeviceManager(this);
            camera = null;
            screen = new Screen(320, 240, 2);
            MapContent = new ContentManager(Services, "Content");
            Content.RootDirectory = "Content";

            // Set up input
            input = new InputManager();            
            initialize_buttons();

            // Initialize other variables
            map = null;
            player = null;
            player_controllable = true;
            player_tile_obstruction = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here            
            base.Initialize();            

        }

        protected virtual void initialize_buttons() {
            dir = new DirectionalButtons();
            action = new VERGEActions();
            ButtonInputSet set;
            dir = new DirectionalButtons();
            set = new ButtonInputSet();
            set.keys.Add(Keys.Left);
            set.gamepad_buttons.Add(Buttons.DPadLeft);
            set.gamepad_buttons.Add(Buttons.LeftThumbstickLeft);
            dir.left = input.register_semantic_button("Left", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.Right);
            set.gamepad_buttons.Add(Buttons.DPadRight);
            set.gamepad_buttons.Add(Buttons.LeftThumbstickRight);
            dir.right = input.register_semantic_button("Right", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.Up);
            set.gamepad_buttons.Add(Buttons.DPadUp);
            set.gamepad_buttons.Add(Buttons.LeftThumbstickUp);
            dir.up = input.register_semantic_button("Up", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.Down);
            set.gamepad_buttons.Add(Buttons.DPadDown);
            set.gamepad_buttons.Add(Buttons.LeftThumbstickDown);
            dir.down = input.register_semantic_button("Down", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.Enter);
            set.keys.Add(Keys.Z);
            set.keys.Add(Keys.Space);
            set.gamepad_buttons.Add(Buttons.A);
            action.confirm = input.register_semantic_button("Confirm", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.LeftAlt);
            set.keys.Add(Keys.RightAlt);
            set.keys.Add(Keys.X);
            set.gamepad_buttons.Add(Buttons.B);
            action.cancel = input.register_semantic_button("Cancel", set);
            set = new ButtonInputSet();
            set.keys.Add(Keys.Escape);
            set.gamepad_buttons.Add(Buttons.Y);
            action.menu = input.register_semantic_button("Menu", set);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spritebatch = new SpriteBatch(GraphicsDevice);
            // load a map here
            
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            map.tileset.image.Dispose();
        }




        public void init_map() { // TODO: dehackify this, move to VERGEMap 
            y_range = map.height * map.tileset.tilesize + screen.height * 2;            
            setup_tile_destinations();
            camera = new Camera();
            camera.mode = CameraMode.FollowPlayer;
        }
    }

    public class DirectionalButtons {
        public SemanticButton up, down, left, right;
    }

    public class VERGEActions {
        public SemanticButton confirm, cancel, menu;
    }
}

