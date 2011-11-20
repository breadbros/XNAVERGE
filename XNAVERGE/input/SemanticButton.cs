using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAVERGE {
    // This class defines an abstract "button" linked to a particular semantic operation (for instance, "cancel"). 
    // It can be mapped to arbitrarily many physical keys/buttons from the mouse, keyboard, and gamepad.
    // Each physical key/button can be mapped to only a single semantic "button", however.
    public class SemanticButton {        
        public String name { get { return _name; } }
        public ButtonInputSet input, default_input;


        // "Down" is true if the button is down, as you'd expect. "Pressed" and "released" indicate that the button has become down/up 
        // since the last time the button's Update() was called (which is generally once per tick). 
        // Down-ness is OR-ed across all the physical keys mapped to this button, while pressed/released-ness is AND-ed.
        public bool down { get { return _down; } }
        public bool pressed { get { return _pressed; } }
        public bool released { get { return _released; } }
        protected bool _down, _pressed, _released, _locked;

        // A locked button cannot be pressed. unpress() is called when the button is locked.
        public bool locked {
            get { return _locked; }
            set {
                _locked = value;
                if (value) unpress();
            }
        }
        

        // ticks_held is the number of ticks since the button was pressed. It's normally 0 if the button isn't pressed, but it retains its value
        // for the moment the button is released (when released == true) so that you can use it in your unpress logic. Thus, you shouldn't assume
        // the key is down just because ticks_held is nonzero.
        public int ticks_held; 

        protected String _name;

        public SemanticButton(String description, ButtonInputSet defaults) {
            _name = description;
            input = default_input = defaults;
            _down = _pressed = _released = _locked = false;
            ticks_held = 0;
        }

        // Forcibly sets the button to up. Unpress purges pressed, released, and ticks_held, so it's as if the button was never pressed at all.
        public void unpress() {
            _down = _pressed = _released = false;
            ticks_held = 0;
        }

        // time_elapsed is a count of the ticks since the InputManager last updated (generally 1).
        public void Update(InputManager manager, int time_elapsed) {
            bool down_now;
            if (_locked) return;

            _pressed = _released = false;

            // Determine current state of semantic button (it's down if anything mapped to the button is down)

            if (manager.gp_state.IsConnected && input.gamepad_buttons.Any(manager.gp_state.IsButtonDown)) down_now = true;
            else if (input.keys.Any(manager.kb_state.IsKeyDown)) down_now = true;
           

            
            else {
                // TODO: other input types here
                down_now = false;
            }

            if (down_now != _down) {
                _down = down_now;
                if (down_now) _pressed = true;
                else _released = true;
            }

            if (down_now || _released) ticks_held += time_elapsed;
            else ticks_held = 0;
        }
    }    

    // Encapsulates a bunch of physical input buttons (regardless of device) that all do the same thing.
    public class ButtonInputSet {
        private const int KEY_LIST_CAPACITY = 6; // initial capacity of key list (list will be expanded if it's exceeded)
        private const int BUTTON_LIST_CAPACITY = 2; // initial capacity of button list (list will be expanded if it's exceeded)
        public List<Keys> keys;
        public List<Buttons> gamepad_buttons;
        // TODO: mouse buttons

        public ButtonInputSet() {
            keys = new List<Keys>(KEY_LIST_CAPACITY);
            gamepad_buttons = new List<Buttons>(BUTTON_LIST_CAPACITY);
        }

    }
}
