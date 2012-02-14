using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;


namespace XNAVERGE {
    public static class Utility {
        public static readonly float SQRT2;
        public static readonly float INV_SQRT2;

        static Utility() {
            SQRT2 = (float)Math.Sqrt(2);
            INV_SQRT2 = 1 / SQRT2;
        }


        // Mimics the Math.DivRem function, which Xbox XNA lacks.
        public static int DivRem(int num, int denom, out int rem) {
            rem = num % denom;
            return num / denom;
        }

        // Returns the velocity needed to travel the given distance in the given number of ticks
        // along the given direction, assuming zero acceleration.
        // TODO: Make this less stupid? Maybe someday. Who can say what the future holds
        public static Vector2 velocity_from_direction(Direction dir, float distance, int ticks) {
            Vector2 value;
            float factor;
            switch (dir) {
                case Direction.Up:
                    value = new Vector2(0, -1);
                    break;
                case Direction.Down:
                    value = new Vector2(0, 1);
                    break;
                case Direction.Left:
                    value = new Vector2(-1, 0);
                    break;
                case Direction.Right:
                    value = new Vector2(1, 0);
                    break;
                case Direction.UpLeft:
                    value = new Vector2(-1, -1);
                    break;
                case Direction.UpRight:
                    value = new Vector2(1, -1);
                    break;
                case Direction.DownLeft:
                    value = new Vector2(-1, 1);
                    break;
                case Direction.DownRight:
                    value = new Vector2(1, 1);
                    break;
                default:
                    value = Vector2.Zero;
                    break;
            }
            if ((int)dir < 4) factor = distance;  // cardinal direction
            else factor = distance * INV_SQRT2;    // diagonal direction

            if (ticks != 1) factor /= ticks;
            return value * factor;
        }

        // Converts a uint of the form 0x00RRGGBB to one of the form 0xAABBGGRR.
        // The former is what packed VERGE assets use, while the latter is what
        // XNA expects when writing to a Texture2D.
        public static uint convert_rgb_to_abgr(uint pixel, uint transparency) {
            if ((pixel & 0x00FFFFFF) == (transparency & 0x00FFFFFF)) return 0x00000000U;
            return ((pixel << 8) | 0xFF000000);
        }

        // Converts R, G, B bytes to a uint of the form 0xAABBGGRR.
        // The former is what packed VERGE assets use, while the latter is what
        // XNA expects when writing to a Texture2D.
        // This form of the function accepts a byte array and a starting point, and
        // reads three bytes from that point.
        public static uint convert_rgb_to_abgr(byte[] array, int start_idx, uint transparency) {
            if (array.Length < start_idx + 2) throw new System.IO.IOException("The byte array is only " + array.Length + " bytes long, so there aren't three bytes to be read from a starting index of " + start_idx + ".");
            uint pixel = 0xFF000000 | ((uint)array[start_idx + 2] << 16) | ((uint)array[start_idx + 1] << 8) | (uint)array[start_idx];
            if ((pixel & 0x00FFFFFF) == (transparency & 0x00FFFFFF)) return 0x00000000U;
            return 0xFF000000U | pixel;
        }

        // Given a number of frames and the width and height of each, returns the smallest power of 2 that can be used as the 
        // side-length of a square capable of containing all the frames. Used for efficient creation of texture
        // atlases.
        // Be warned that XNA will not permit textures larger than 4096*4096, so if you end up with something that
        // gigantic you're going to want to dice it up.
        public static int smallest_bounding_square(int fw, int fh, int num_frames) {
            int exponent;
            double side;
            bool done = false;

            // start with the smallest possible power-of-2, given the total area of the texture atlas.
            exponent = (int)Math.Floor(Math.Log(Math.Sqrt(fw * fh * num_frames), 2));
            side = Math.Pow(2, exponent);

            // XNA has a hardcoded limit of 2^12.            
            while (!done && exponent <= 12) {
                if (Math.Floor((double)side / fw) * Math.Floor((double)side / fh) >= num_frames)
                    done = true;
                else side *= 2;
                exponent++;
            }
            if (!done) throw new Exception("Too much image data to place in a single texture. Textures are limited to 4096x4096.");
            return (int)side;
        }

        private static char[] nul = { '\0' };
        // Reads a string of known length from a given StreamReader and returns it with any trailing null characters removed.
        // VERGE assets make extensive use of this format for legacy reasons.        
        public static String read_known_length_string(StreamReader reader, int len) {
            char[] buffer = new char[len];
            int read = reader.ReadBlock(buffer, 0, len);
            return (new String(buffer)).TrimEnd(Utility.nul);
        }

        private const String WHITESPACE = @"\s";
        public static String strip_whitespace(String str) {
            return System.Text.RegularExpressions.Regex.Replace(str, WHITESPACE, "");
        }

        private static readonly char[] path_sep = {'/', '\\'};
        public static String strip_path(String str) {
            int pos = Math.Max(0, str.LastIndexOfAny(path_sep));
            return str.Substring(pos);
        }


        // Returns a Direction based on the signs of the x and y values given. ++ is DownRight, -0 is Left, etc.
        // If both signs are 0, returns Down. 
        public static Direction direction_from_signs(int x, int y, bool include_diagonals) {
            if (x == 0) {
                if (y >= 0) return Direction.Down; // when centered, default to Down
                else return Direction.Up;
            }
            else if (x > 0) {
                if (!include_diagonals || y == 0) return Direction.Right;
                else if (y > 0) return Direction.DownRight;
                else return Direction.UpRight;
            }
            else {
                if (!include_diagonals || y == 0) return Direction.Left;
                else if (y > 0) return Direction.DownLeft;
                else return Direction.UpLeft;
            }
        }

        // The inverse of direction_from_signs. Returns a point whose X and Y coordinates are each either
        // +1, or -1, or 0, according to whether the direction corresponds to an increase, decrease, or lack
        // of change in the X and Y value. For instance, Up yields (0, -1).
        public static Point signs_from_direction(Direction dir, bool include_diagonals) {
            switch (dir) {
                case Direction.Up:
                    return new Point(0, -1);
                case Direction.Down:
                    return new Point(0, 1);
                case Direction.Left:
                    return new Point(-1, 0);
                case Direction.Right:
                    return new Point(1, 0);
                case Direction.UpLeft:
                    if (include_diagonals) return new Point(-1, -1);
                    else return new Point(-1, 0);
                case Direction.UpRight:
                    if (include_diagonals) return new Point(1, -1);
                    else return new Point(1, 0);
                case Direction.DownLeft:
                    if (include_diagonals) return new Point(-1, 1);
                    else return new Point(-1, 0);
                case Direction.DownRight:
                    if (include_diagonals) return new Point(1, 1);
                    else return new Point(1, 0);
                default: throw new ArgumentException("Invalid direction passed to direction_to_movestring_face.");
            }
        }

        // Converts the nonintuitive movestring facecode pattern to directions. 
        // For converting the other way, see direction_to_movestring_face(), below.
        public static Direction movestring_face_to_direction(int facing) {
            switch (facing) {
                case Movestring.FACECODE_DOWN:
                    return Direction.Down;                   
                case Movestring.FACECODE_UP:
                    return Direction.Up;
                case Movestring.FACECODE_LEFT:
                    return Direction.Left;
                case Movestring.FACECODE_RIGHT:
                    return Direction.Right;/*
                case Movestring.FACECODE_DOWNLEFT:
                    return Direction.DownLeft;
                case Movestring.FACECODE_DOWNRIGHT:
                    return Direction.DownRight;
                case Movestring.FACECODE_UPLEFT:
                    return Direction.UpLeft;
                case Movestring.FACECODE_UPRIGHT:
                    return Direction.UpRight;*/
            }
            throw new ArgumentException("Movestring facing is only defined for numbers 0-7 (was passed " + facing + ".");
        }

        // Gets the (hard-to-remember) movestring facecodes from Directions.
        // For converting the other way, see movestring_face_to_direction(), below.
        public static int direction_to_movestring_face(Direction dir) {
            switch (dir) {
                case Direction.Down:
                    return Movestring.FACECODE_DOWN;                    
                case Direction.Up:
                    return Movestring.FACECODE_UP;
                case Direction.Left:
                    return Movestring.FACECODE_LEFT;
                case Direction.Right:
                    return Movestring.FACECODE_RIGHT;
                /*case Direction.DownLeft:
                    return Movestring.FACECODE_DOWNLEFT;
                case Direction.DownRight:
                    return Movestring.FACECODE_DOWNRIGHT;
                case Direction.UpLeft:
                    return Movestring.FACECODE_UPLEFT;
                case Direction.UpRight:
                    return Movestring.FACECODE_UPRIGHT;*/
            }
            throw new ArgumentException("Invalid direction passed to direction_to_movestring_face.");
        }
    
    
    
    }  
    
    // VERGE uses 3 or 4 different direction-numbering conventions. This seemed like one of the tidier ones.
    public enum Direction { Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight }
}
