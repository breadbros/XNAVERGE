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
        public RenderTarget2D true_size_buffer; // this buffer reflects the true internal size of the screen, before scaling
        public int scaling_factor {
            get { return _scaling_factor; }
            set {                
                if (value < 1) throw new ArgumentOutOfRangeException("scaling_factor", "The scaling factor must be a positive integer.");
                _scaling_factor = value;
                VERGEGame.game.graphics.PreferredBackBufferWidth = _width * value;
                VERGEGame.game.graphics.PreferredBackBufferHeight = _height * value;
                VERGEGame.game.graphics.ApplyChanges();
                scaling_matrix = Matrix.CreateScale(value, value, 1);
                // TODO: resize window here, if necessary
            }
        }
        public float aspect_ratio { get { return ((float)_width) / _height; } }
        
        public BasicEffect effect { get { return _effect; } }
        public Matrix scaling_matrix; //was internal
        protected BasicEffect _effect;
        protected int _width, _height, _scaling_factor;

        public Screen(int w, int h, int scalingfactor) {            
            _width = w;
            _height = h;
            _effect = new BasicEffect(VERGEGame.game.GraphicsDevice);            
            _effect.World = Matrix.Identity;
            _effect.View = Matrix.CreateLookAt(new Vector3(_width / 2, _height / 2, 0.0f), new Vector3(_width / 2, _height / 2, 1.0f), Vector3.Down);
            _effect.Projection = Matrix.CreateOrthographic(_width, _height, 1.0f, 0.0f);
            _effect.VertexColorEnabled = true;
            true_size_buffer = new RenderTarget2D(VERGEGame.game.GraphicsDevice, w, h);
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
