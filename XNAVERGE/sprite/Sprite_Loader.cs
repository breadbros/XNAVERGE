using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace XNAVERGE {
    public partial class Sprite {
        public const String def_location = @"content\dat\sprites";

        // Gets a SpriteBasis from a name, where the name is either an asset name or a JSON file name.
        // If it's a JSON file, you must include the extension (which is a good idea anyway, since
        // it ensures there won't be a collision with asset names).
        public static SpriteBasis get_basis_from_name(string name) {
            SpriteBasis b;
            string filename;
            if (!Sprite.basis_cache.TryGetValue(name, out b)) {
                filename = Path.Combine(def_location, name);
                if (File.Exists(filename)) { // clearly this is a definition file
                    b = _load_basis_by_json_file(filename);
                }
            }
            return b;
        }

        public static SpriteBasis _load_basis_by_asset_name(string asset_name) {
            try {
                return VERGEGame.game.MapContent.Load<SpriteBasis>(asset_name);
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException) {
                return VERGEGame.game.MapContent.Load<SpriteBasis>(@"chrs\" + asset_name);
            }
        }
        
        public static SpriteBasis _load_basis_by_json_file(string filepath) {
            SpriteBasis b;
            //System.Collections.ArrayList data;
            //string raw = File.ReadAllText(filepath);
            //data = (System.Collections.ArrayList) (fastJSON.JSON.Instance.Parse(output));
            return null;
        }
    }
}
