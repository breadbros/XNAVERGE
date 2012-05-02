using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {
    public partial class Sprite {
        public const String DEF_LOCATION = @"content\sprites"; // This is where JSON definitions (but not CHR xnbs) are
        public const String CHR_LOCATION = @"chrs\"; // If a chr asset isn't found, it tries again prefixing it with this

        // Gets a SpriteBasis from a name, where the name is either an asset name or a JSON file name.
        // If it's a JSON file, you must include the extension (which is a good idea anyway, since
        // it ensures there won't be a collision with asset names).
        public static SpriteBasis get_basis_from_name(string name) {
            SpriteBasis b;
            string filename;
            if (!Sprite.basis_cache.TryGetValue(name, out b)) {
                filename = Path.Combine(DEF_LOCATION, name);
                if (File.Exists(filename)) { // clearly this is a definition file
                    b = _load_basis_by_json_file(filename);
                }
                else b = _load_basis_by_asset_name(name); // otherwise, treat as a chr asset
            }
            return b;
        }

        // Load a SpriteBasis from a CHR XNB file. This searches for the asset four ways, in the following order:
        //   * check the full asset name with CHR_LOCATION prefixed
        //   * check the asset name, sans extension, with CHR_LOCATION prefixed
        //   * check the full asset name alone
        //   * check the asset name, sans extension, alone
        // The extension-removal steps are skipped if there is no extension in the asset name given.
        public static SpriteBasis _load_basis_by_asset_name(string asset_name) {
            SpriteBasis spr = null;            
            string cur, naked_name = Path.GetFileNameWithoutExtension(asset_name);
            int has_extension;
            if (naked_name == asset_name) has_extension = 0;
            else has_extension = 1;

            for (int i = 0; i < 2; i++) {
                for (int j = 0; j <= has_extension; j++) {                    
                    cur = (i == 0 ? CHR_LOCATION : "") + (j == 0 ? asset_name : naked_name);
                    if (File.Exists(@"content\" + cur + ".xnb")) {
                        spr = VERGEGame.game.MapContent.Load<SpriteBasis>(cur);
                        spr.name = cur;
                        i = 999;
                        j = 999;
                    }
                }
            }
            if (spr == null) {
                throw new Microsoft.Xna.Framework.Content.ContentLoadException("Error loading " + asset_name +
                    ": Assumed this was an .xnb file since it didn't end with .json, but couldn't find the asset " +
                    "(looked for an asset named " + CHR_LOCATION + asset_name + ", " + CHR_LOCATION + naked_name + 
                    ", " + asset_name + ", or " + naked_name + ").");
            }
            
            return spr;
        }
        
        // Load a SpriteBasis from a JSON specification. Currently, this won't work for CHRs.
        public static SpriteBasis _load_basis_by_json_file(string filepath) {
            SpriteBasis b;
            String temp;
            Dictionary<String, Object> spec;
            List<Object> arr;
            SpriteAnimation anim;
            int inner_pad, outer_pad;
            
            spec = (Dictionary<String, Object>) Utility.parse_JSON(filepath);
            arr = (List<Object>) spec["dims"];
            b = new SpriteBasis(
                (int)((Int64)arr[0]),
                (int)((Int64)arr[1]),
                (int)((Int64)spec["frames"]),
                (int)((Int64)spec["per_row"]));

            // Assume no padding between frames if unspecified
            inner_pad = spec.ContainsKey("inner_pad") ? (int)((Int64)spec["inner_pad"]) : 0; // # pixels bordering the entire image 
            outer_pad = spec.ContainsKey("outer_pad") ? (int)((Int64)spec["outer_pad"]) : 0; // # pixels between adjacent frames

            temp = Path.Combine(Sprite.DEF_LOCATION, (String)spec["image"]);
            using (FileStream stream = new FileStream(temp, FileMode.Open)) {
                b.image = Texture2D.FromStream(VERGEGame.game.GraphicsDevice, stream);
            }
            b.generate_bounding_boxes(outer_pad, inner_pad);

            if (spec.ContainsKey("hitbox")) {
                arr = (List<Object>)spec["hitbox"];
                b.default_hitbox = new Rectangle((int)((Int64)arr[0]),(int)((Int64)arr[1]),(int)((Int64)arr[2]),(int)((Int64)arr[3]));
            }
            else { // if unspecified, hitbox defaults to frame size
                b.default_hitbox = new Rectangle(0, 0, b.frame_width, b.frame_height);
            }

            spec = (Dictionary<String, Object>)(spec["animations"]);
            foreach (KeyValuePair<String, Object> kvp in spec) {
                arr = (List<Object>)kvp.Value;
                anim = new SpriteAnimation(kvp.Key, b.num_frames, (String)arr[0], 
                    (AnimationStyle)Enum.Parse(typeof(AnimationStyle), (string)arr[1], true));
                b.animations.Add(anim.name, anim);                
            }
            foreach (SpriteAnimation a in b.animations.Values) { // now go back and set up any transitions
                if (a.style == AnimationStyle.Transition) {
                    arr = (List<Object>)spec[a.name];
                    if (arr.Count < 3) throw new FormatException("Error loading \"" +
                        a.name + "\" animation in sprite specification " + filepath + ": If the " +
                        "AnimationStyle is Transition, there must be a third parameter naming the " +
                        "animation to be transitioned to.");
                    if (b.animations.TryGetValue((String)arr[2], out anim))
                        a.transition_to = anim;
                    else { // no transition defined, or invalid transition
                        temp = arr[2] as String;
                        if (String.IsNullOrEmpty(temp)) throw new FormatException("Error loading \"" +
                            a.name + "\" animation in sprite specification " + filepath + ": If the " +
                            "AnimationStyle is Transition, there must be a third parameter naming the " +
                            "animation to be transitioned to.");
                        throw new FormatException("Error loading \"" + a.name + "\" animation in " +
                            "sprite specification " + filepath + ": The specified transition_to " +
                            "animation " + "\"" + temp + "\" is not defined.");
                    }
                }
            }

            return b;
        }
    }
}
