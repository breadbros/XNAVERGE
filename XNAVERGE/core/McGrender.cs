using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {

    public class McgNode {
        McgLayer layer;

        float final_x, final_y;
        float cur_x, cur_y;
        float? tick_x, tick_y;
        int? delay, running_time, final_time;
        Boolean isMoving;
        public event Action OnStop = null;
        public event Action OnDraw = null;
        Texture2D image = null;
        Rectangle im_bounds;

        public McgNode( Action act, McgLayer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
            OnDraw = act;
            _Node( l, start_x, start_y, end_x, end_y, delay );
        }

        public McgNode( Texture2D im, Rectangle bounds, McgLayer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
            image = im;
            im_bounds = bounds;
            _Node( l, start_x, start_y, end_x, end_y, delay );
        }

        private void _Node( McgLayer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
            layer = l;

            if( end_x == null ) {
                cur_x = final_x = start_x;
                cur_y = final_y = start_y;
                tick_y = tick_x = delay = final_time = null;
                isMoving = false;
            } else {
                cur_x = start_x;
                cur_y = start_y;
                final_x = (float)end_x;
                final_y = (float)end_y;
                this.delay = delay;
                tick_x = ( final_x - cur_x ) / (float)delay;
                tick_y = ( final_y - cur_y ) / (float)delay;
                final_time = layer.stack.systime + delay;
                isMoving = true;
            }
        }

        public Boolean IsMoving() {
            return isMoving;
        }

        public void Update( int ticksSinceLastUpdate ) {
            if( isMoving ) {
                if( ticksSinceLastUpdate > 0 ) {
                    this.cur_x += ( (float)tick_x * (float)ticksSinceLastUpdate );
                    this.cur_y += ( (float)tick_y * (float)ticksSinceLastUpdate );
                    running_time += ticksSinceLastUpdate;

                    if( running_time >= final_time ) {
                        isMoving = false;
                        this.cur_x = this.final_x;
                        this.cur_y = this.final_y;

                        if( OnStop != null ) {
                            OnStop();
                            OnStop = null;
                        }
                    }
                }
            }
        }

        public void Draw() {

            if( image != null ) {
                layer.stack.spritebatch.Draw( image, im_bounds, Color.White );
            } else if( OnDraw != null ) {
                OnDraw();
            }
        }
    }



    public class McgLayer {

        public McGrenderStack stack;
        public Boolean isRendering;
        public List<McgNode> nodes;
        public string name;

        public McgLayer( McGrenderStack s, string n ) {
            stack = s;
            nodes = new List<McgNode>();
            isRendering = true;
            name = n;
        }

        public void AddNode( McgNode n ) {
            nodes.Add( n );
        }

        public void Update( int ticksSinceLastUpdate ) {
            for( int i = 0; i < nodes.Count; i++ ) {
                nodes[i].Update( ticksSinceLastUpdate );
            }
        }

        public void Draw() {
            for( int i = 0; i < nodes.Count; i++ ) {
                nodes[i].Draw();
            }
        }
    }

    public class McGrenderStack {    
        public List<McgLayer> layers;
        public SpriteBatch spritebatch;
        public int systime;

        public void setSpritebatch( SpriteBatch sb ) {
            spritebatch = sb;
        }

        public void setSystime( int st ) {
            systime = st;
        }

        public McGrenderStack() {
            layers = new List<McgLayer>();
        }

        public McgLayer AddLayer( string name ) {
            McgLayer l = new McgLayer(this, name);
            layers.Add( l );
            return l;
        }

        public void Update( int ticksSinceLastUpdate ) {
            for( int i = 0; i < layers.Count; i++ ) {
                layers[i].Update( ticksSinceLastUpdate );
            }
        }

        public void Draw() {
            // Screen screen = game.screen;
            // Rectangle blit_rect = new Rectangle( 0, 0, screen.width, screen.height );
            // game.GraphicsDevice.SetRenderTarget( screen.true_size_buffer );

            spritebatch.Begin();
            for( int i = 0; i < layers.Count; i++ ) {
                layers[i].Draw();
            }
            spritebatch.End();

            /*
            game.GraphicsDevice.SetRenderTarget( null );
            game.spritebatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, screen.scaling_matrix );
            game.spritebatch.Draw( screen.true_size_buffer, blit_rect, Color.White );
            game.spritebatch.End();
        
             */ 
        }
    }
}
