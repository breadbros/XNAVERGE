using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAVERGE {
    public class InputManager {
        public Dictionary<String, SemanticButton> button_dict;
        public Dictionary<String, SemanticButton>.ValueCollection semantic_buttons;
        public Dictionary<Keys,SemanticButton> keymappings; 
        // These stick mappings are used if you want to treat the sticks as "fake d-pads", with four directionals each.
        public Dictionary<Buttons,SemanticButton> gamepad_button_mappings; 

        public KeyboardState kb_state;
        public GamePadState gp_state;
        protected int last_updated;
       


        public InputManager() {
            button_dict = new Dictionary<string, SemanticButton>();
            semantic_buttons = button_dict.Values;
            keymappings = new Dictionary<Keys,SemanticButton>();
            gamepad_button_mappings = new Dictionary<Buttons, SemanticButton>();
            kb_state = Keyboard.GetState();
            gp_state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            last_updated = VERGEGame.game.tick;
        }

        public SemanticButton register_semantic_button(String name, ButtonInputSet defaults) {
            SemanticButton button = new SemanticButton(name, defaults);
            if (button_dict.ContainsKey(name)) throw new Exception("The operation \"" + name + "\" has already been registered as a semantic button.");
            button_dict[name] = button;
            return button;
        }

        // Returns the SemanticButton to which the given key/button is mapped. If safe is false, throws an exception 
        // when the key is unmapped. If safe is true, returns null.
        public SemanticButton target(Keys key, bool safe) {
            if (safe)
                if (!keymappings.ContainsKey(key)) return null;            
            return keymappings[key]; 
        }
        public SemanticButton target(Buttons gamepad_button, bool safe) {
            if (safe)
                if (!gamepad_button_mappings.ContainsKey(gamepad_button)) return null;
                return gamepad_button_mappings[gamepad_button]; 

        }

        public void Update() {
            kb_state = Keyboard.GetState();
            gp_state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            int cur_time = VERGEGame.game.tick;
            foreach (SemanticButton button in semantic_buttons) button.Update(this, cur_time - last_updated);
            last_updated = cur_time;
        }
    }
}
