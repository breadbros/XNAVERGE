using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XNAVERGE;

namespace Sully.McGrenderStack {

    public class Node {
        Layer layer;

        float final_x, final_y;
        float cur_x, cur_y;
        float? tick_x, tick_y;
        int? delay, running_time, final_time;
        Boolean isMoving;
        public event Action OnStop = null;
        public event Action OnDraw = null;
        Texture2D image = null;
        Rectangle im_bounds;

        public Node( Action act, Layer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
            OnDraw = act;
            _Node( l, start_x, start_y, end_x, end_y, delay );
        }

        public Node( Texture2D im, Rectangle bounds, Layer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
            image = im;
            im_bounds = bounds;
            _Node( l, start_x, start_y, end_x, end_y, delay );
        }

        private void _Node( Layer l, int start_x, int start_y, int? end_x = null, int? end_y = null, int? delay = null ) {
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
                final_time = _.systime + delay;
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
                _.sg.spritebatch.Draw( image, im_bounds, Color.White );
            } else if( OnDraw != null ) {
                OnDraw();
            }
        }
    }



    public class Layer {

        public McGrenderStack stack;
        public Boolean isRendering;
        public List<Node> nodes;
        public string name;

        public Layer( McGrenderStack s, string n ) {
            stack = s;
            nodes = new List<Node>();
            isRendering = true;
            name = n;
        }

        public void AddNode( Node n ) {
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
        SullyGame game;
        public List<Layer> layers;

        public McGrenderStack( SullyGame sg ) {
            game = sg;
            layers = new List<Layer>();
        }

        public Layer AddLayer( string name ) {
            Layer l = new Layer(this, name);
            layers.Add( l );
            return l;
        }

        public void Update( int ticksSinceLastUpdate ) {
            for( int i = 0; i < layers.Count; i++ ) {
                layers[i].Update( ticksSinceLastUpdate );
            }
        }

        public void Draw() {
            game.spritebatch.Begin();
            for( int i = 0; i < layers.Count; i++ ) {
                layers[i].Draw();
            }
            game.spritebatch.End();
        }
    }
}
