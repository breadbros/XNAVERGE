using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    // Represents a bounded 2D space: a rectangular plane, in other words. Takes integer coordinates only.
    public class BoundedSpace<T> : IBounded where T : class, IBounded {
        public const int DEFAULT_REGION_WIDTH = 128;
        public const int DEFAULT_REGION_HEIGHT = 128;

        readonly public Rectangle bounds;
        Rectangle IBounded.bounds { get { return bounds; } }

        private Dictionary<T, bool>[][] regions;
        private Dictionary<T, Rectangle> old_positions; // stores the old positions, but adjusted to be in region-units for easy array lookup
        private int num_x, num_y, region_w, region_h;
        private int change; // used by enumerators to detect a change in the underlying collection 

        public BoundedSpace(Rectangle bounding_rect, int partition_width, int partition_height) {
            bounds = bounding_rect;
            change = 0;
            region_w = partition_width;
            region_h = partition_height;
            num_x = (bounds.Width - 1) / partition_width + 1;
            num_y = (bounds.Height - 1) / partition_height + 1;
            regions = new Dictionary<T, bool>[num_x][];
            old_positions = new Dictionary<T, Rectangle>();
            for (int x = 0; x < num_x; x++) {
                regions[x] = new Dictionary<T, bool>[num_y];
                for (int y = 0; y < num_y; y++) {
                    regions[x][y] = new Dictionary<T, bool>();
                }
            }
        }
        public BoundedSpace(int x, int y, int w, int h, int partition_w, int partition_h) : this(new Rectangle(x, y, w, h), partition_w, partition_h) { }        
        public BoundedSpace(int x, int y, int w, int h) : this(new Rectangle(x, y, w, h), DEFAULT_REGION_WIDTH, DEFAULT_REGION_WIDTH) { }
        public BoundedSpace(Rectangle bounding_rect) : this(bounding_rect, DEFAULT_REGION_WIDTH, DEFAULT_REGION_WIDTH) { }

        // called when the collection changes (meaning something has been added or removed, or the underlying items have 
        private void changed() { 
            if (change == Int32.MaxValue) change = Int32.MinValue;
            else change++;
        }

        public BoundedElementSet elements_within_bounds(Rectangle bound_rect, bool strict_inclusion) {
            return new BoundedElementSet(this, true, bound_rect);
        }
        public BoundedElementSet elements_within_bounds(Rectangle bound_rect, bool strict_inclusion, T exclude) {
            return new BoundedElementSet(this, true, bound_rect, exclude);
        }

        public void Add(T element) {
            Rectangle intersection;
            if (old_positions.ContainsKey(element)) throw new InvalidOperationException("That item has already been added.");            
            
            intersection = Rectangle.Intersect(bounds, element.bounds);
            
            intersection.Width = (intersection.X + intersection.Width - 1 - bounds.X) / region_w; // currently holds the rightmost covered region
            intersection.Height = (intersection.Y + intersection.Height - 1 - bounds.Y) / region_h; // currently holds the bottom covered region
            intersection.X = (intersection.X - bounds.X) / region_w;
            intersection.Y = (intersection.Y - bounds.Y) / region_h;
            intersection.Width -= intersection.X - 1; // width = rightside - leftside + 1
            intersection.Height -= intersection.Y - 1; // height = bottom - top + 1
            old_positions.Add(element, intersection); 
           
            for (int x = intersection.X; x < intersection.X + intersection.Width; x++) {
                for (int y = intersection.Y; y < intersection.Y + intersection.Height; y++) {
                    // value = false for no particular reason. We're just using the dictionary as a set of T, since XNA doesn't support HashSet.
                    regions[x][y].Add(element, false);                    
                }
            }
            changed();
        }

        public bool Remove(T element) {
            Rectangle old;
            if (!old_positions.ContainsKey(element)) return false;

            old = old_positions[element];
            old_positions.Remove(element);

            for (int x = old.X; x < old.X + old.Width; x++) {
                for (int y = old.Y; y < old.Y + old.Height; y++) {
                    // value = false for no particular reason. We're just using the dictionary as a set of T, since XNA doesn't support HashSet.
                    regions[x][y].Remove(element);
                }
            }
            changed();
            return true;
        }

        // Account for bounds shifting in the given element since the collection was last updated.
        // Throws an ArgumentException if the element is not in the space.
        public void Update(T element) {
            Rectangle old, intersection;
            if (!old_positions.ContainsKey(element)) throw new ArgumentException("The given element is not in the space. Either it was never added, or it has been removed.");
            
            old = old_positions[element];
            intersection = Rectangle.Intersect(bounds, element.bounds);
            intersection.Width = (intersection.X + intersection.Width - 1 - bounds.X) / region_w; // currently holds the rightmost covered region
            intersection.Height = (intersection.Y + intersection.Height - 1 - bounds.Y) / region_h; // currently holds the bottom covered region
            intersection.X = (intersection.X - bounds.X) / region_w;
            intersection.Y = (intersection.Y - bounds.Y) / region_h;
            intersection.Width -= intersection.X - 1; // width = rightside - leftside + 1
            intersection.Height -= intersection.Y - 1; // height = bottom - top + 1
            if (intersection == old) return; // still in all the right sectors

            Remove(element); // this is guaranteed to call changed(), so we needn't call it elsewhere in the function.
            
            // And this part is a flagrant duplication of Add, just to avoid recalculating the intersection or incurring overhead from passing it.
            old_positions.Add(element, intersection);
            for (int x = intersection.X; x < intersection.X + intersection.Width; x++) {
                for (int y = intersection.Y; y < intersection.Y + intersection.Height; y++) {
                    // value = false for no particular reason. We're just using the dictionary as a set of T, since XNA doesn't support HashSet.
                    regions[x][y].Add(element, false);                    
                }
            }            
        }

        public class BoundedElementSet : IEnumerator<T> {
            private BoundedSpace<T> space;
            private bool strict_inclusion, single_region;
            private int x, y, left_bound, right_bound, upper_bound, lower_bound;
            private Rectangle intersection; // intersection of given boundary and overall space boundary
            private int signature;            
            private T cur_element, excluded;
            private Dictionary<T, bool> cur_region, seen; 
            private Dictionary<T, bool>.KeyCollection.Enumerator elements;

            Object System.Collections.IEnumerator.Current { get { return Current; } }
            public T Current {
                get {
                    if (signature == space.change && cur_element != null) return cur_element;
                    if (signature != space.change) throw new InvalidOperationException("The underlying collection has changed, so the enumerator is now invalid.");
                    throw new InvalidOperationException("No current value.");
                }
            }

            internal BoundedElementSet(BoundedSpace<T> _space, bool strict, Rectangle bound_rect) {                
                space = _space;
                intersection = Rectangle.Intersect(_space.bounds, bound_rect);
                signature = _space.change;
                strict_inclusion = strict;
                // Get region bounds in terms of array cells
                left_bound = (intersection.X - _space.bounds.X) / _space.region_w;
                upper_bound = (intersection.Y - _space.bounds.Y) / _space.region_h;              
                right_bound = (intersection.X + intersection.Width - 1 - _space.bounds.X) / _space.region_w; 
                lower_bound = (intersection.Y + intersection.Height - 1 - _space.bounds.Y) / _space.region_h;
                single_region = ((left_bound == right_bound) && (upper_bound == lower_bound));
                cur_element = null;
                cur_region = null;
                excluded = null;
                elements = default(Dictionary<T, bool>.KeyCollection.Enumerator);
                if (!single_region) seen = new Dictionary<T, bool>();
                x = left_bound;
                y = upper_bound;                
            }
            internal BoundedElementSet(BoundedSpace<T> _space, bool strict, Rectangle bound_rect, T exclude) : 
                this(_space, strict, bound_rect) {
                    
                excluded = exclude;
            }

            // this just combines MoveNext and Current for tidiness.
            public bool GetNext(out T current) { 
                if (MoveNext()) {
                    current = cur_element;
                    return true;
                }
                current = null;
                return false;
            }

            public bool MoveNext() { 
                cur_element = null;
                while (cur_element == null) {                    
                    while (cur_region == null) {                        
                        if (x <= right_bound) {
                            if (y <= lower_bound) { // get next region                                
                                cur_region = space.regions[x][y];
                                elements = cur_region.Keys.GetEnumerator();
                                y++;
                            }
                            else { // end of column
                                x++;
                                y = upper_bound;
                            }
                        }
                        else { // done enumerating                           
                            return false;
                        }
                    }
                    if (elements.MoveNext()) {
                        cur_element = elements.Current;
                        if (cur_element == excluded) cur_element = null;
                        else {
                            if (!single_region && seen.ContainsKey(cur_element)) cur_element = null; // we already did this one in a different region
                            else {
                                if (!single_region) seen.Add(cur_element, false);
                                if (strict_inclusion) { // if strict, confirm that it's actually inside the region
                                    if (!cur_element.bounds.Intersects(intersection)) cur_element = null;
                                }
                            }
                        }
                    }
                    else {
                        cur_region = null;
                    }

                }
                return true;
            }
            
            public void Reset() {
                cur_element = null;
                cur_region = null;
                elements = default(Dictionary<T, bool>.KeyCollection.Enumerator);
                x = left_bound;
                y = upper_bound;
            }
            
            public void Dispose() { }             
        }

    }

    public interface IBounded {
        Rectangle bounds { get; }
    }
}
