using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sully {
    public class SaveManager {
        protected const uint SAVE_SIGNATURE = 116845876u; // arbitrary signifier value to identify Sully saves.

        // CURRENT_VERSION should be incremented by 1 every time a breaking change is made in the save format. 
        // Obviously, any additional data you want the save file to store will require a new version. 
        // There are also certain changes that will break all old saves without actually requiring that we
        // write new save code. These include:
        // * Changing PartyMember.MAX_LEVEL
        // * Changing _.NUM_FLAGS
        // * Changing the number or names of PartyMembers
        // * Changing XP required at each level
        // * Changing the names, ordering, or number of character equipment slots.
        // For now we needn't increase the version for changes of this type, but once we release and have
        // saves in the wild, we'll need to.
        public int CURRENT_VERSION = 2;

        // saves are numbered from 0 to 99. You can change this, but note that file naming assumes
        // a limit of 1000 (save number 999).
        public static int MAX_SAVES = 1000;
        public readonly Point THUMBNAIL_SIZE = new Point(64, 48); // if you change this you'll need to move to a new version
        public List<SaveHeader> headers;

        public String save_location {
            get { return Path.Combine(Environment.GetFolderPath(game.user_storage_root), "Sully Chronicles"); }
        }

        private SullyGame game;

        public SaveManager(SullyGame game) {
            this.game = game;
            headers = new List<SaveHeader>();
            read_headers();
        }

        public String get_save_filepath(int save_num) {
            if (save_num < 0 || save_num >= MAX_SAVES) throw new ArgumentOutOfRangeException(String.Format("Save index must be between 0 and {0} (got {1}).", MAX_SAVES, save_num));
            return Path.Combine(save_location, "sully" + save_num.ToString("000") + ".sav");
        }

        public String get_backup_filepath() {
            return Path.Combine(save_location, "$$BACKUP.sav");
        }

        // Reads headers from the save files into SaveHeader structs and puts them in the headers list.
        // The headers are loaded in order, but missing files are skipped when building the lists.
        public void read_headers() {           
            for (int i = 0; i < MAX_SAVES; i++) read_header(i);                                            
        }

        protected void read_header(int save_num) {            
            SaveHeader header;           
            int temp;
            string filename = get_save_filepath(save_num); ;

            if (File.Exists(filename)) {
                using (FileStream fs = new FileStream(filename, FileMode.Open)) {
                    using (BinaryReader reader = new BinaryReader(fs)) {

                        header = new SaveHeader(this, save_num);
                        header.save_idx = save_num;

                        if (reader.ReadInt32() != SAVE_SIGNATURE) throw new FormatException("Not a Sully Chronicles save file: " + filename);
                        temp = reader.ReadInt32(); // version (ignore for now)
                        header.screencap = read_thumbnail_screencap(reader);
                        header.playtime = new TimeSpan(reader.ReadInt64());
                        temp = reader.ReadInt32();
                        for (int j = 0; j < temp; j++) header.party.Add(reader.ReadString());
                        header.location = reader.ReadString(); // location name (currently unimplemented);                        
                    }
                }
            }
        }

        // Takes a tiny screencap of the current map renderstack (ignoring mcgrender layers) 
        // and sticks it into the given stream. 
        protected void write_thumbnail_screencap(Stream stream) {
            using (RenderTarget2D screencap = new RenderTarget2D(game.GraphicsDevice, game.screen.width, game.screen.height)) {                             
                game.GraphicsDevice.SetRenderTarget(screencap);
                game.map.renderstack.Draw();
                game.GraphicsDevice.SetRenderTarget(null);
                screencap.SaveAsPng(stream, THUMBNAIL_SIZE.X, THUMBNAIL_SIZE.Y);                
            }            
        }

        protected Texture2D read_thumbnail_screencap(BinaryReader reader) {
            Texture2D image;
            using (MemoryStream ms = new MemoryStream(reader.ReadBytes(reader.ReadInt32()))) {
                image = Texture2D.FromStream(_.sg.GraphicsDevice, ms);
            }
            return image;
        }

        // Creates the game subdirectory if it's not already there
        private void verify_directory() {
            if (!Directory.Exists(save_location)) Directory.CreateDirectory(save_location);
        }        

        public void save(int save_num) {            
            String save_path, backup_path;
            bool completed = false;            

            save_path = get_save_filepath(save_num);
            backup_path = get_backup_filepath();
            verify_directory();

            if (File.Exists(save_path)) File.Copy(save_path,backup_path,true);
            using (BinaryWriter writer = new BinaryWriter(File.Create(save_path))) {
                try {
                    completed = _write_to_save(writer, CURRENT_VERSION);
                }
                catch (IOException e) {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Failed to save at index " + save_num + ". Exception details:\n\t" + e.Message);
                    // add more diagnostics here if it becomes a problem
                }
            }
            if (!completed) {
                if (File.Exists(backup_path)) File.Copy(backup_path, save_path, true);
            }
            else {
                read_header(save_num);
            }
        }        

        // Always returns true.
        protected bool _write_to_save(BinaryWriter writer, int version) {
            PartyMember[] cur_party;

            int temp;
            long old_pos, new_pos;
            
            // SAVE HEADER
            // -----------

            writer.Write(SAVE_SIGNATURE); // write save file signature
            writer.Write(CURRENT_VERSION);

            old_pos = writer.BaseStream.Position;
            writer.Write(0); // reserve this space for an int            
            write_thumbnail_screencap(writer.BaseStream);
            new_pos = writer.BaseStream.Position;
            writer.Seek((int)old_pos, SeekOrigin.Begin);
            writer.Write((int)(new_pos - old_pos - 4)); // how far you need to skip to bypass the screenshot
            writer.Seek((int)new_pos, SeekOrigin.Begin);

            writer.Write(game.total_time.Ticks); // playtime (1 TimeSpan tick = 100 nanoseconds) as a long. 
            // Note that this is NOT the same as the Stopwatch class's "ticks", 
            // whose length is hardware-dependent.
            cur_party = game.party.getMembers();
            writer.Write(cur_party.Length);
            foreach (PartyMember p in cur_party) writer.Write(p.name); // Order of characters in current party
            writer.Write("location name"); // not supported yet

            // PARTY DATA
            // ----------            
            writer.Write(PartyData.partymemberData.Values.Count);
            foreach (PartyMember p in PartyData.partymemberData.Values) {
                writer.Write(p.name);
                writer.Write(p.cur_xp);
                writer.Write(p.cur_hp);
                writer.Write(p.cur_mp);                

                writer.Write(0); // reserve this space for an int
                temp = 0;
                foreach (KeyValuePair<string, EquipmentSlot> kvp in p.equipment) {
                    if (kvp.Value.getItem() != null) { // only save slots with stuff in them
                        temp++;
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.getItem().name);
                    }
                }
                // Now go back to indicate how many slots were saved.
                new_pos = writer.BaseStream.Position;
                writer.BaseStream.Seek(old_pos, SeekOrigin.Begin);
                writer.Write(temp);
                writer.BaseStream.Seek(new_pos, SeekOrigin.Begin);
            }

            // INVENTORY DATA
            // --------------         
            temp = 0;
            foreach (List<ItemSlot> list in game.inventory.item_sets) temp += list.Count;
            writer.Write(temp);
            foreach (List<ItemSlot> list in game.inventory.item_sets) {                
                foreach (ItemSlot slot in list) {
                    writer.Write(slot.item.name);
                    writer.Write(slot.quant);
                }
            }

            // SCENARIO DATA
            // -------------
            foreach (int f in _.flags) writer.Write(f);
            // eventually the step counter will go here

            // MAP DATA 
            // --------
            // Note that most map state, such as NPC and follower positioning, is thrown away.
            
            writer.Write(game.map.asset); // record map asset name for lookup on load
            writer.Write(game.player.x);  // coordinates in pixels
            writer.Write(game.player.y); 

            // CONFIGURATION DATA does not yet exist but will go here.
            
            return true;
        }

        public void load(int save_num) {
            String save_path, mapname = null;
            Point player_coords = default(Point);
            save_path = get_save_filepath(save_num);
            if (!File.Exists(save_path)) throw new FileNotFoundException(save_path + " was not found.");

            using (FileStream fs = new FileStream(save_path, FileMode.Open)) {
                using (BinaryReader reader = new BinaryReader(fs)) {
                    _read_from_save(reader, CURRENT_VERSION, ref mapname, ref player_coords);
                }
            }            
            _.MapSwitch(mapname, player_coords.X, player_coords.Y, true);
        }

        protected void _read_from_save(BinaryReader reader, int version, ref string mapname, ref Point player_coords) {
            List<String> cur_party_names;
            PartyMember cur_char;
            int temp, temp2;            

            // LOAD HEADER
            // -----------

            if (reader.ReadInt32() != SAVE_SIGNATURE) throw new FormatException("Not a Sully Chronicles save file.");
            temp = reader.ReadInt32();
            if (temp != CURRENT_VERSION) throw new FormatException("Wrong save file format. This is a version " +
                                                       temp + " save file, but a version " + CURRENT_VERSION +
                                                       " file is required."); // TODO: add version converters 
            temp = reader.ReadInt32(); // offset to end of screencap
            reader.BaseStream.Seek(temp, SeekOrigin.Current);
            
            game.saved_time = new TimeSpan(reader.ReadInt64());
            
            temp = reader.ReadInt32(); // number of current party members
            cur_party_names = new List<String>();
            for (int i = 0; i < temp; i++) cur_party_names.Add(reader.ReadString());

            reader.ReadString(); // location name -- currently unused

            // LOAD PARTY DATA
            // ---------------
            game.inventory.ClearInventory();                        
            temp = reader.ReadInt32(); // # of characters total
            for (int i = 0; i < temp; i++) {
                cur_char = PartyData.partymemberData[reader.ReadString().ToLower()];
                cur_char._loadState(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()); // current xp/hp/mp

                temp2 = reader.ReadInt32(); // number of slots with things in them
                foreach (EquipmentSlot slot in cur_char.equipment.Values) {
                    if (slot.getItem() != null) slot.Dequip(game.inventory, true);
                }
                for (int j = 0; j < temp2; j++) {
                    cur_char.equipment[reader.ReadString()].Equip(reader.ReadString(), game.inventory);
                }
            }

            foreach (string character in cur_party_names) {
                game.party.AddPartyMember(character, PartyData.partymemberData[character.ToLower()].level);
            }

            // LOAD INVENTORY DATA
            // -------------------            
            temp = reader.ReadInt32();
            for (int i = 0; i < temp; i++) {
                game.inventory.AddItem(reader.ReadString(), reader.ReadInt32());
            }

            // LOAD SCENARIO DATA
            // ------------------
            temp = _.flags.Length;
            for (int i = 0; i < temp; i++) _.flags[i] = reader.ReadInt32();

            // LOAD MAP DATA 
            // -------------
            mapname = reader.ReadString();
            player_coords.X = reader.ReadInt32();
            player_coords.Y = reader.ReadInt32();

        }
       
    }
}
