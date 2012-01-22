using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XNAVERGE;

namespace M
{

    public partial class MGame : VERGEGame {
        static void Main(string[] args) {            
            //(new MGame()).Exit();
            (new MGame()).Run();            
        }        

        protected override void Initialize() {            

            main_assembly = System.Reflection.Assembly.GetExecutingAssembly(); // tell the library where to find map scripts
            global = new MGlobalScripts();
            base.Initialize();                        
        }

        protected override void LoadContent() {
            base.LoadContent();
            init_ui();
            VERGEMap.switch_map("town01");
            this.hook_render = script<RenderLayerDelegate>("draw_UI");            

            // ---------------------
            // BEGIN SILLINESS

            map.zones[1].adjacent = true;
            player = map.spawn_entity(12, 16, "chap");
            player.obstructing = true;            
            Entity e;
            e = map.spawn_entity(21, 14, "chap");
            e.set_movestring("L2D2R2U2B");
            e.speed = 80;            

            for (int x = 0; false && x < map.width; x++) {
                for (int y = 0; y < map.height; y++) {
                    if (rand.NextDouble() < 0.5) {
                        e = map.spawn_entity(x, y, "chap");
                        e.speed = 100;
                        e.obstructing = true;
                        e.obstructable = true;
                        e.set_movestring("L1D1R1U1B");
                    }
                }
            }

            global.get_script<BasicDelegate>("testing")();
            map.zones[1].script = script<ZoneDelegate>("zonetrigger");
            
            system_font = Content.Load<SpriteFont>("Garamond");
        }        

        private void init_ui() {
            Textbox.image = Content.Load<Texture2D>("textbox");
            Textbox.bounds = new Rectangle(0, 0, Textbox.image.Width, Textbox.image.Height);
            Textbox.bounds.Offset((screen.width - Textbox.bounds.Width) / 2, screen.height / 3 - Textbox.bounds.Height);
            Textbox.inner_bounds = Textbox.bounds; // copy value            
            Textbox.inner_bounds.Inflate(-Textbox.horizontal_padding, -Textbox.vertical_padding);
        }
        
        protected override void UnloadContent()
        {
 	        base.UnloadContent();
        }
        
        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (Textbox.state != TextboxState.Hidden) Textbox.Update();
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public void textbox(String str_1, String str_2, String str_3) {
            VERGEGame.game.lock_player();
            Textbox.reset();
            Textbox.lines.Add(str_1);
            Textbox.lines.Add(str_2);
            Textbox.lines.Add(str_3);
            Textbox.state = TextboxState.Printing;            
        }
    }
}
