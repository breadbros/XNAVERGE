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

namespace XNAVERGE {
    public class VERGEGame : Game {
        public static VERGEGame game; // points to the last VERGEGame initialized (presumably there would only be one)
        public static Random rand = new Random();
        public GraphicsDeviceManager graphics;
        public SpriteBatch spritebatch;
        public Matrix scaling_matrix;
        
        public VERGEMap map;
        public Entity player;
        public bool player_controllable; // true if player responds to input
        public bool player_tile_obstruction; // true if player uses tile-based, rather than pixel-based, obstruction
        
        public int tick { get { return _tick; } }
        private int _tick; // Centiseconds since app start. This will overflow if you leave the game on for 248 days.        

        public Camera camera;
        private Rectangle[][] dest_rect;
        public Rectangle screen { get { return _screen; } }
        private Rectangle _screen;
        public float y_range; // the total vertical distance in which sprites are drawable, centred on the middle of the map (necessary for technical reasons)

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
            _screen = new Rectangle(0, 0, 320, 240);
            set_scaling_factor(2);
            Content.RootDirectory = "Content";

            // Set up input
            input = new InputManager();            
            initialize_buttons();

            // Initialize other variables
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
            map = new VERGEMap("Content\\raw\\town01.map");
            y_range = map.height * map.tileset.tilesize + _screen.Height * 2;
            setup_tile_destinations();                        
            camera = new Camera(map);
            camera.mode = CameraMode.FollowPlayer;

            // ---------------------
            // BEGIN SILLINESS

            player = map.spawn_entity(12, 16, "Content\\raw\\chap.chr");            
            Entity e;
            e = map.spawn_entity(21, 14, "Content\\raw\\chap.chr");
                        e.set_movestring("L2D2R2U2B");
            e.speed = 80;

            for (int x = 0; false && x < map.width; x++) {
                for (int y = 0; y < map.height; y++) {
                    e = map.spawn_entity(x, y, "Content\\raw\\chap.chr");
                    e.set_movestring("L1D1R1U1B");

                }
            }

            // END SILLINESS
            // ---------------------
            
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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _tick++;
            input.Update();
            //if (player_controllable && player != null) control_player();
            for (int i = 0; i < map.num_entities; i++) {
                map.entities[i].Update();
            }

            base.Update(gameTime);            
        }


        private static Vector2 NONSENSE_PARALLAX = new Vector2(float.NegativeInfinity, float.NegativeInfinity); // garbage vector used as a sentinel value for parallax

        int old_s, fps;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            int min_tile_x, min_tile_y, tiles_to_draw_x, tiles_to_draw_y, offset_x, offset_y, parallaxed_camera_x, parallaxed_camera_y;
            min_tile_x = min_tile_y = tiles_to_draw_x = tiles_to_draw_y = offset_x = offset_y = parallaxed_camera_x = parallaxed_camera_y = 0;
            int tilesize = map.tileset.tilesize;
            int mapwidth = map.width;
            int mapheight = map.height;
            GraphicsDevice.Clear(Color.Black);
            camera.update();

            _screen.X = camera.x;
            _screen.Y = camera.y;
            // We need a range of valid y-values for sprites, since XNA does its sorting using floats between 0 and 1. Sprites can still be visible
            // when they're off the edge of the map, so to be sure we catch them all, we'll define the drawable sprites as those within a screen's
            // length of the top/bottom of the map.
            // This is totally arbitrary, but it's fairly generous; if you need more than that, what on earth are you doing?

            if (gameTime.TotalGameTime.Seconds != old_s) {
                Window.Title = fps.ToString();
                fps = 0;
            }
            fps++;
            old_s = gameTime.TotalGameTime.Seconds;

            Vector2 current_parallax = NONSENSE_PARALLAX; // initialize to impossible value to ensure things get set on first iteration

            Matrix neutral_parallax_transform = offset_matrix(camera.x, camera.y, tilesize) * scaling_matrix; // this is the most common case so let's save it

            // Update entity frames
            for (int i = 0; i < map.num_entities; i++) { map.entities[i].advance_frame(); }

            foreach (RenderLayer rl in map.renderstack.list) {
                if (rl.visible) {
                    if (rl.type == LayerType.Tile) {
                        if (rl.parallax != current_parallax) {
                            if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();
                            current_parallax = rl.parallax;
                            if (current_parallax == VERGEMap.NEUTRAL_PARALLAX) {
                                parallaxed_camera_x = camera.x;
                                parallaxed_camera_y = camera.y;
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, neutral_parallax_transform);
                            }
                            else if (current_parallax == VERGEMap.FIXED_PARALLAX) {
                                parallaxed_camera_x = 0;
                                parallaxed_camera_y = 0;
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, scaling_matrix);
                            }
                            else {
                                parallaxed_camera_x = (int)(camera.x * current_parallax.X);
                                parallaxed_camera_y = (int)(camera.y * current_parallax.Y);
                                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, offset_matrix(parallaxed_camera_x, parallaxed_camera_y, tilesize) * scaling_matrix);
                            }
                            min_tile_x = parallaxed_camera_x / tilesize;
                            min_tile_y = parallaxed_camera_y / tilesize;
                            tiles_to_draw_x = screen.Width / tilesize + 1;
                            tiles_to_draw_y = screen.Height / tilesize + 1;
                        }
                        for (int x = Math.Max(0, -min_tile_x); x < Math.Min(tiles_to_draw_x, mapwidth - min_tile_x); x++) {
                            for (int y = Math.Max(0, -min_tile_y); y < Math.Min(tiles_to_draw_y, mapheight - min_tile_y); y++) {
                                spritebatch.Draw(map.tileset.image, dest_rect[x][y], map.tileset.tile_frame[((TileLayer)rl).data[min_tile_x + x][min_tile_y + y]], Color.White);
                            }
                        }
                    }
                    else if (rl.type == LayerType.Entity && map.num_entities > 0) {
                        if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();
                        current_parallax = NONSENSE_PARALLAX; // entity layer ignores parallax. this ensures any tile layers above this will be aligned properly.
                        spritebatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-camera.x, -camera.y, 0) * scaling_matrix);
                        Entity ent;
                        for (int i = 0; i < map.num_entities; i++) {
                            ent = map.entities[i];
                            if (!ent.deleted && ent.visible && ent.destination.Intersects(screen)) {
                                ent.Draw();
                            }
                        }
                        spritebatch.End();
                    }
                }
            }

            if (current_parallax != NONSENSE_PARALLAX) spritebatch.End();

            base.Draw(gameTime);
        }

        // this needs to be called whenever the resolution or tile size changes.
        // TODO: Make it actually accomodate different resolutions
        public void setup_tile_destinations() {
            dest_rect = new Rectangle[map.width][];
            int tilesize = map.tileset.tilesize;
            for (int x = 0; x < screen.Width / tilesize + 1; x++) {
                dest_rect[x] = new Rectangle[map.height];
                for (int y = 0; y < screen.Height / tilesize + 1; y++) {
                    dest_rect[x][y] = new Rectangle(x * tilesize, y * tilesize, tilesize, tilesize);
                }
            }
        }

        // Returns a translation matrix representing the number of pixels the tile boundary is to the left of and above
        // the screen edges. This is used internally for determining where to begin drawing the tiles, since the
        // leftmost/topmost tiles will often be partially off-screen.
        private Matrix offset_matrix(int upper_left_x, int upper_left_y, int tilesize) {
            return Matrix.CreateTranslation(-(upper_left_x % tilesize), -(upper_left_y % tilesize), 0);
        }

        public void set_scaling_factor(int factor) {
            graphics.PreferredBackBufferWidth = screen.Width * factor;
            graphics.PreferredBackBufferHeight = screen.Height * factor;

            scaling_matrix = Matrix.CreateScale(factor, factor, 1);
            // TODO: resize window here, if necessary

        }

    }

    public class DirectionalButtons {
        public SemanticButton up, down, left, right;
    }

    public class VERGEActions {
        public SemanticButton confirm, cancel, menu;
    }
}

