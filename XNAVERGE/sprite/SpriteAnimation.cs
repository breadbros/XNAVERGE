using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public class SpriteAnimation {
        public String name; 
        public readonly String pattern; // the base animation string (not used after the initial parsing)
        public AnimationStyle style;
        public SpriteAnimation transition_to; // Which animation to switch to when this animation ends. Only used with the "Transition" style.        
        public int length;
        public int[] frame, delay;
        
        protected const String FRAME = "F";
        protected const String WAIT = "W";


        public SpriteAnimation( String anim_name, int num_frames, System.Collections.Generic.List<System.Object> anim_pattern, AnimationStyle anim_style ) {

            name = anim_name;
            style = anim_style;
            length = anim_pattern.Count;
            transition_to = null;
            frame = new int[length];
            delay = new int[length];
            for( int i = 0; i < length; i++ ) {

                System.Collections.Generic.List<System.Object> item = (System.Collections.Generic.List<System.Object>)anim_pattern[i];

                frame[i] = (int)(System.Int64)item[0];
                delay[i] = (int)(System.Int64)item[1];
            }
        }

        public SpriteAnimation(String anim_name, int num_frames, String anim_pattern) : this(anim_name, num_frames, anim_pattern, AnimationStyle.Looping) { }
        public SpriteAnimation(String anim_name, int num_frames, String anim_pattern, AnimationStyle anim_style) {
            int cur_pos, next_pos, len, cur_val;
            bool expecting_frame;

            // temporary queues for storing data while reading through the movestring            
            Queue<int> frame_queue = new Queue<int>();
            Queue<int> wait_queue = new Queue<int>();
            
            name = anim_name;
            style = anim_style;          
            pattern = SpriteAnimation.clean_pattern(anim_pattern);            
            len = pattern.Length;
            transition_to = null;

            if (pattern.Substring(0, 1) != SpriteAnimation.FRAME) throw new MalformedAnimationPatternException(pattern, "Patterns must begin with \"F\""); 
            cur_pos = 1; 
            expecting_frame = false; 
            while (cur_pos < len) {
                if (expecting_frame) next_pos = pattern.IndexOf(SpriteAnimation.FRAME, cur_pos);
                else next_pos = pattern.IndexOf(SpriteAnimation.WAIT, cur_pos);
                if (next_pos == -1) next_pos = len;
                try {
                    cur_val = Int32.Parse(pattern.Substring(cur_pos, next_pos - cur_pos));
                }
                catch (Exception) {
                    throw new MalformedAnimationPatternException(pattern, "Patterns must alternate between F# and W# terms, where each # is a positive integer.");
                }
                if (expecting_frame) {
                    //Console.WriteLine("Wait {0}", cur_val);
                    if (cur_val < 0) throw new MalformedAnimationPatternException(pattern, "Negative wait specified."); 
                    wait_queue.Enqueue(cur_val);
                }
                else {
                    //Console.WriteLine("Frame {0}", cur_val);
                    if (cur_val < 0) throw new MalformedAnimationPatternException(pattern, "Negative frame specified."); 
                    if (cur_val >= num_frames) throw new MalformedAnimationPatternException(pattern, "Frame " + cur_val + " specified, but the sprite only has " + num_frames + " frames. Note that frames begin at 0."); 
                    frame_queue.Enqueue(cur_val);
                }
                expecting_frame = !expecting_frame;
                cur_pos = next_pos + 1;
            }
            
            // It's legal not to specify the last frame's wait time. If it's left blank, give it a wait time of 1.
            if (wait_queue.Count < frame_queue.Count) wait_queue.Enqueue(1);

            length = frame_queue.Count;
            frame = new int[length];
            delay = new int[length];
            for (int i = 0; i < length; i++) {
                frame[i] = frame_queue.Dequeue();
                delay[i] = wait_queue.Dequeue();
            }
        }
        // If the frame and delay array have already been generated (as is the case with sprites saved to file), this is the constructor 
        // to use. It makes no effort to check that the pattern actually matches the animation arrays given.
        public SpriteAnimation(String anim_name, String anim_pattern, int[] frame_arr, int[] delay_arr, AnimationStyle anim_style) {
            name = anim_name;
            pattern = anim_pattern;
            frame = frame_arr;
            delay = delay_arr;
            length = frame_arr.Length;
            style = anim_style;
            transition_to = null;
        }


        // This converts an animation pattern string to all uppercase and strips whitespace. CHR animations are supposed to have this done
        // already, but it can't hurt to be careful, and it's useful if you want to specify custom patterns at runtime.
        public static String clean_pattern(String raw_pattern) {
            return Utility.strip_whitespace(raw_pattern.ToUpper());
        }
    }

    public enum AnimationStyle { Once, Looping, Transition, BackAndForth }
    // Once: The animation plays to the end, then stops on that frame indefinitely.
    // Looping: The animation plays to the end and then begins again from the start.
    // Transition: The animation plays to the end, then switches to the "transition_to" animation.
    // BackAndForth: The animation goes from start to finish, then from finish back to start, and so on.

    public class MalformedAnimationPatternException : Exception {
        public MalformedAnimationPatternException(String rstring, String message) : base("Invalid animation pattern \"" + rstring + "\": " + message) { }
    }
}
