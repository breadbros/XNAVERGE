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
        // * Changing a PartyMember's name (only breaks saves with that character)
        // For now we needn't increase the version for changes of this type, but once we release and have
        // saves in the wild, we'll need to.
        public int CURRENT_VERSION = 1;

        // saves are numbered from 0 to 99. You can change this, but note that file naming assumes
        // a limit of 1000 (save number 999).
        public static int MAX_SAVES = 999;
        public readonly Point THUMBNAIL_SIZE = new Point(64, 48); // if you change this you'll need to move to a new version

        public String save_location {
            get { return Path.Combine(Environment.GetFolderPath(game.user_storage_root), "Sully Chronicles"); }
        }

        private SullyGame game;

        public SaveManager(SullyGame game) {
            this.game = game;            
        }

        public String get_save_filepath(int save_num) {
            if (save_num < 0 || save_num >= MAX_SAVES) throw new ArgumentOutOfRangeException(String.Format("Save index must be between 0 and {0} (got {1}).", MAX_SAVES, save_num));
            return Path.Combine(save_location, "sully" + save_num.ToString("000") + ".sav");
        }

        public String get_backup_filepath() {
            return Path.Combine(save_location, "$$BACKUP.sav");
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
        }        

        // Always returns true.
        protected bool _write_to_save(BinaryWriter writer, int version) {
            PartyMember[] party;
            
            // SAVE HEADER
            // -----------

            writer.Write(SAVE_SIGNATURE); // write save file signature
            writer.Write(CURRENT_VERSION);
            write_thumbnail_screencap(writer.BaseStream);
            writer.Write(game.total_time.Ticks); // playtime (1 TimeSpan tick = 100 nanoseconds) as a long. 
            // Note that this is NOT the same as the Stopwatch class's "ticks", 
            // whose length is hardware-dependent.
            party = game.party.getMembers();
            writer.Write(party.Length);
            foreach (PartyMember p in party) writer.Write(p.name); // Only the names go in the header
            writer.Write("location name"); // not supported yet

            // PARTY DATA (in same order as the list of names)
            // -----------------------------------------------

            foreach (PartyMember p in party) {
                writer.Write(p.level);                
                writer.Write(p.cur_hp);
                writer.Write(p.cur_mp);
                writer.Write(p.cur_xp);
                // other stuff to go here in the future: status effects, equipment
            }

            // INVENTORY DATA (in same order as the list of names)
            // -----------------------------------------------
            List<ItemSlot>[] sets = { game.inventory.consumables.items, game.inventory.equipment.items, game.inventory.key.items };
            foreach (List<ItemSlot> list in sets) {
                writer.Write(list.Count);
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

    }
}
