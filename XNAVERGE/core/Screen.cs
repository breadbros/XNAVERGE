using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // represents the internal game screen to which stuff is drawn, before any letterboxing or scaling.
    // TODO: add support functions for funky screen sizes and letterboxing
    public class Screen { 
        public int width { get { return _width; } }
        public int height { get { return _height; } }
        public int scaling_factor {
            get { return _scaling_factor; }
            set {
                if (value < 1) throw new ArgumentOutOfRangeException("scaling_factor", "The scaling factor must be a positive integer.");
                _scaling_factor = value;
                VERGEGame.game.graphics.PreferredBackBufferWidth = _width * value;
                VERGEGame.game.graphics.PreferredBackBufferHeight = _height * value;
                scaling_matrix = Matrix.CreateScale(value, value, 1);
                // TODO: resize window here, if necessary
            }
        }
        public float aspect_ratio { get { return ((float)_width) / _height; } }
        internal Matrix scaling_matrix;
        protected int _width, _height, _scaling_factor;

        public Screen(int w, int h, int scalingfactor) {
            _width = w;
            _height = h;
            scaling_factor = scalingfactor;
        }
        public Screen(int w, int h) : this(w, h, 1) { }

    }
}
