using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // represents the internal game screen to which stuff is drawn, before any letterboxing or scaling.
    // TODO: add support functions for funky screen sizes and letterboxing
    public class Screen : IDisposable { 
        public int width { get { return _width; } }
        public int height { get { return _height; } }
        public int scaling_factor {
            get { return _scaling_factor; }
            set {
                if (value < 1) throw new ArgumentOutOfRangeException("scaling_factor", "The scaling factor must be a positive integer.");
                _scaling_factor = value;
                VERGEGame.game.graphics.PreferredBackBufferWidth = _width * value;
                VERGEGame.game.graphics.PreferredBackBufferHeight = _height * value;
                VERGEGame.game.graphics.ApplyChanges();
                scaling_matrix = Matrix.CreateScale(value, value, 1);
                projection_matrix = Matrix.CreateOrthographic(_width * _scaling_factor, _height * _scaling_factor, 0.0f, -1.0f);

                _effect.World = scaling_matrix;
                _effect.View = Matrix.Identity;
                _effect.Projection = projection_matrix;
                _effect.VertexColorEnabled = true;
                // TODO: resize window here, if necessary
            }
        }
        public float aspect_ratio { get { return ((float)_width) / _height; } }
        public BasicEffect effect { get { return _effect; } }
        internal Matrix scaling_matrix, projection_matrix;
        protected BasicEffect _effect;
        protected int _width, _height, _scaling_factor;

        public Screen(int w, int h, int scalingfactor) {
            _width = w;
            _height = h;
            _effect = new BasicEffect(VERGEGame.game.GraphicsDevice);
            scaling_factor = scalingfactor;
        }
        public Screen(int w, int h) : this(w, h, 1) { }

        public void Dispose() {
            _effect.Dispose();
            GC.SuppressFinalize(this);
        }

        ~Screen() {
            Dispose();
        }

    }
}
