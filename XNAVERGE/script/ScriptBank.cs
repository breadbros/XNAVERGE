using System;
using System.Reflection;
using System.Collections.Generic;

using XNAVERGE;

namespace XNAVERGE {
    public class ScriptBank {
        private Dictionary<String, Delegate> lookup;

        
        public ScriptBank() {
            lookup = new Dictionary<String,Delegate>();
        }

        // Fetch a delegate of the appropriate type, returning null if none exists. It's not possible
        // to constrain the generic type to only delegates, so in theory you can pass a non-delegate
        // type, but this will yield null.
        public T get_script<T>(String name) where T : class {
            return get_typed_delegate(name, typeof(T)) as T;
        }       

        // Return a copy of the named delegate if it exists and is of the specified type, returning 
        // null otherwise. If the delegate is not in the lookup dictionary this will attempt to find
        // and add it. Note that, since delegates are immutable, altering the delegate you get from
        // this function will not do anything to the internal copy.
        // Although get_typed_delegate returns null in most failure cases, it will throw an
        // AmbiguousMatchException if the given method is overloaded.               
        private Delegate get_typed_delegate(String name, Type type) {
            Delegate del = null;
            MethodInfo method;
            if (lookup.ContainsKey(name)) {
                del = lookup[name];
                if (del.GetType() != type) del = null; // wrong type, never mind
            }
            else { // not in dictionary
                method = this.GetType().GetMethod(name); // null if not found
                if (method == null) return null;
                try {
                    del = Delegate.CreateDelegate(type, method);
                }
                catch (Exception) { // binding error. usually means the signature or permissions are wrong
                    return null;
                }
                lookup.Add(name, del); 
            }
            return del;
        }

    }

    public class MapScriptBank : ScriptBank {
        public readonly VERGEMap map;

        public MapScriptBank(VERGEMap map) : base() {
            this.map = map;
        }

        // Called as soon as the map is loaded
        public virtual void initialize() {
        }

        // Called once the transition into the map has ended (always after initialize())
        public virtual void do_after_transition() {
        }

        // Called just before switching to another map, after the transition out.
        // This is NOT a finalizer -- it won't be called if the map is 
        // unloaded for other reasons.
        public virtual void do_on_exit() {
        }

    }
}
