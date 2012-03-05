using System;
using System.Reflection;
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

        // The assembly/namespace to search for map scripts. By default, this is assumed to be the assembly and namespace of the method
        // that calls the VERGEGame constructor. If you want something different you'll have to set it before VERGEGame.Initialize().
        public Assembly main_assembly; 
        public String main_namespace;

        public VERGEMap map;
        public Entity player;        
        public bool player_tile_obstruction; // true if player uses tile-based, rather than pixel-based, obstruction

        public InputManager input;
        public DirectionalButtons dir;
        public VERGEActions action;        

        internal BoundedSpace<Entity> entity_space;

        public McGrenderStack renderstack;

        public void setMcGrender( McGrenderStack mcg ) {
            renderstack = mcg;
        }

        public System.Diagnostics.Stopwatch stopWatch;
        public VERGEGame() : base() {
            System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace();
            stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            // the assembly/namespace to search for script classes defaults to the one 
            // from which this constructor was called.
            Type sourcetype = stack.GetFrame(1).GetMethod().DeclaringType; 
            main_assembly = sourcetype.Assembly;
            main_namespace = sourcetype.Namespace;

            VERGEGame.game = this;
            Default_Handlers.game = this;

            // Set up timing
            this.IsFixedTimeStep = false;
            
            tick_length = 10; // In milliseconds. VERGE standard is 100 ticks per second
            _last_tick_time = 0;
            _tick = 0;            
            
            // Set up graphics
            graphics = new GraphicsDeviceManager(this);

            // Uncomment this line to remove fps throttling: 
            graphics.SynchronizeWithVerticalRetrace = false;
            
            camera = null;            
            hook_render = null;
            system_font = null;
            MapContent = new ContentManager(Services, "Content");
            Content.RootDirectory = "Content";

            // Set up input
            input = new InputManager();            
            initialize_buttons();

            // Initialize other variables
            global = new ScriptBank();
            map = null;
            player = null;
            player_controllable_stack = new Stack<bool>();
            player_controllable = PLAYER_CONTROLLABLE_DEFAULT;
            player_tile_obstruction = true;
            default_entity_handler = Default_Handlers.omnibus_vergestyle_handler;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {             
            screen = new Screen(320, 240, 2);
            RasterizerState rstate = new RasterizerState();
            rstate.CullMode = CullMode.None; // culling not needed for 2D
            rstate.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rstate;
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
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            map.tileset.image.Dispose();
        }


        public virtual void init_map() { // TODO: dehackify this, move to VERGEMap                         

            entity_space = new BoundedSpace<Entity>(-screen.width, -screen.height, map.pixel_width + 2*screen.width, map.pixel_height + 2*screen.height);
            for (int i = 0; i < map.num_entities; i++) {
                entity_space.Add(map.entities[i]);
                map.entities[i].set_script();
            }
            for (int i = 0; i < map.num_zones; i++) 
                map.zones[i].set_script();            
            camera = new Camera();
            //camera.bounded = false;
            camera.mode = CameraMode.FollowPlayer;
            if (map.scripts != null) {
                map.scripts.initialize();
                // TODO: in-transition here
                map.scripts.do_after_transition();
            }

            BasicDelegate initscript = VERGEGame.game.script<BasicDelegate>( map.initscript );
            if( initscript != null )
                initscript();
        }
    }

    public class DirectionalButtons {
        public SemanticButton up, down, left, right;
    }

    public class VERGEActions {
        public SemanticButton confirm, cancel, menu;
    }
}

