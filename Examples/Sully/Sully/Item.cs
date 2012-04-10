using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;


namespace Sully {

    public class Inventory {       
        public const int NUM_ITEM_SETS = 3; // Consumables, Inventory, Key. Needed since Xbox doesn't support Enum.GetValues.

        public List<ItemSlot> consumables { get { return item_sets[(int)ItemType.Consumable]; } }
        public List<ItemSlot> equipment { get { return item_sets[(int)ItemType.Equipment]; } }
        public List<ItemSlot> key { get { return item_sets[(int)ItemType.Key]; } }

        public List<ItemSlot>[] item_sets;
        public Dictionary<Item, ItemSlot> slotIndex;

        public Inventory() {
            item_sets = new List<ItemSlot>[NUM_ITEM_SETS];
            for (int i=0; i<item_sets.Length; i++) item_sets[i] = new List<ItemSlot>();
            slotIndex = new Dictionary<Item, ItemSlot>();
        }

        // Removes everything from inventory permanently
        public void ClearInventory() {
            foreach (List<ItemSlot> set in item_sets) set.Clear();
            slotIndex.Clear();
        }

        public Boolean HasItem(String name) { return HasItem(Item.masterItemList[name.ToLower()]); }
        public Boolean HasItem(Item i) { return slotIndex.ContainsKey(i); }
        
        public void AddItem(String name, int quant) { AddItem(Item.masterItemList[name.ToLower()], quant); }
        public void AddItem( Item i, int quant) {            
            if (quant <= 0) throw new ArgumentOutOfRangeException("You can only add a positive number of items. To remove items, use TakeItem.");
            _adjustItemQuantity(i,quant);
        }

        public void TakeItem(String name, int quant) { TakeItem(Item.masterItemList[name.ToLower()], quant); }
        public void TakeItem(Item i, int quant) {
            if (quant <= 0) throw new ArgumentOutOfRangeException("You can only take away a positive number of items. To add items, use AddItem.");
            _adjustItemQuantity(i, -quant);
        }

        public Inventory GetWearableEquipmentSet( Klass klass, EquipSlotType slot ) {

            Inventory ret = new Inventory();

            foreach( ItemSlot sl in this.equipment ) {
                if( sl.item.equip_slot == slot && sl.item.equip_classes.Contains( klass ) ) {
                    ret.AddItem( sl.item, sl.quant );
                }
            }

            if( slot != EquipSlotType.RightHand && slot != EquipSlotType.Body ) { // can't unequip from RH or Body slots
                ret.AddItem( Item.none, 1 );
            }

            return ret;
        }
  
        protected void _adjustItemQuantity(Item i, int quant) {
            ItemSlot slot;
            if (slotIndex.ContainsKey(i)) {
                slot = slotIndex[i];
                slot.quant += quant;
            }
            else {
                slotIndex[i] = slot = new ItemSlot(i, quant);
                item_sets[(int)i.type].Add(slot);
            }
            if (slot.quant > ItemSlot.MAX_QUANT) slot.quant = ItemSlot.MAX_QUANT;
            if (slot.quant <= 0) {
                slotIndex.Remove(i);
                item_sets[(int)i.type].Remove(slot);
            }
            
        }

        /*
        public void RemoveItem( Item item, int quant ) {
            if( quant <= 0 ) {
                throw new System.InvalidOperationException( "You can only remove positive numbers of items to your inventory." );
            }

            foreach( ItemSlot slot in items ) {
                if( slot.item.name == item.name ) {

                    if( quant <= slot.quant ) {
                        slot.quant -= quant;
                    } else {
                        throw new System.InvalidOperationException( "You can not remove more items than you had (had " + slot.quant + ", tried to remove "+quant+")." );
                    }

                    if( slot.quant == 0 ) {
                        items.Remove( slot );
                    }
                    return;
                }
            }
        }
        */ 
    }

    public class ItemSlot {
        public const int MAX_QUANT = 99;
        public Item item;
        public int quant;

        public ItemSlot( Item i, int quant ) {
            this.item = i;
            this.quant = quant;
        }
    }

    public enum ItemType { Consumable, Equipment, Key }

    public class Item {
        public static readonly Item none = new Item( "(None)", "Unequips current item." );

        public string name, description;
        public ItemType type;
        public int icon, price;
        public bool use_battle, use_menu;
        public string func_targetting, func_effect;
        public Klass[] equip_classes;
        public string equip_modcode;
        public EquipSlotType equip_slot;

        public string[] ELEMENT, ADD, IMMUNE, AUTO, HALVE, NEGATE, DOUBLE, ABSORB, ENABLE;

        public Dictionary<Stat, int> equip_stats;

        public static Dictionary<string, Item> masterItemList;

        public static Item get( string key ) {
            return masterItemList[key.ToLower()];
        }

        private Dictionary<Stat, int> _statsHelper( string modcode ) {

            if( modcode.Length == 0 ) {
                return null;
            }

            Dictionary<Stat, int> d = new Dictionary<Stat, int>();

            string[] pairs = modcode.Split( ';' );
            foreach( string line in pairs ) {
                string[] s = line.Split( ',' );
                if( s.Length == 2 ) {
                    d.Add( _.getStat( s[0] ), int.Parse( s[1] ) );
                } else if( s.Length == 1 && s[0].Equals( "" ) ) {

                } else {
                    throw new Exception( "Invalid parse. '" + modcode + "'" );
                }
            }

            return d;
        }

        public Item( string name, string description ) {
            this.name = name;
            this.equip_slot = EquipSlotType.NONE;
            this.description = description;
        }

        public Item( Dictionary<string, Object> d ) {
            String slot;

            name = ((string)d["name"]).Replace( '_', ' ' );
            description = (string)d["description"];
            icon = (int)((Int64)d["icon"]);
            price = (int)((Int64)d["price"]);
            use_battle = (int)((Int64)d["use_battle"]) > 0;
            use_menu = (int)((Int64)d["use_menu"]) > 0;

            func_targetting = (string)d["func_targetting"];
            func_effect = (string)d["func_effect"];

            slot = (string)d["equip_slot"];
            if (String.IsNullOrEmpty(slot)) equip_slot = EquipSlotType.NONE;
            else {
                try {
                    equip_slot = (EquipSlotType)Enum.Parse(typeof(EquipSlotType), (string)d["equip_slot"], true);
                }
                catch (ArgumentException) { // slot is defined, but the name is illegal!
                    equip_slot = EquipSlotType.NONE;
                }
            }
            equip_modcode = (string)d["equip_modcode"];
            equip_stats = _statsHelper( equip_modcode );

            List<Object> equippable = (List<Object>)d["equip_by"];
            List<Klass> equip_klasses = new List<Klass>();
            foreach( string equip_klass in equippable ) {
                if( equip_klass != "" ) {
                    equip_klasses.Add( Klass.get(equip_klass) );
                }
            }

            if( equip_klasses.Count > 0 ) {
                equip_classes = equip_klasses.ToArray();
            }
            
            if( equip_classes != null ) {
                type = ItemType.Equipment;
                _EquipmentFinalizer( d );
            } else if( price == 0 ) {
                type = ItemType.Key;
            } else {
                type = ItemType.Consumable;
            }
        }

        private void _EquipmentFinalizer( Dictionary<string, Object> d ) {
            object o;
            if( d.TryGetValue( "ELEMENT", out o ) ) {

                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.ELEMENT = list.ToArray();
            }


            if( d.TryGetValue( "ADD", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.ADD = list.ToArray();
            }

            if( d.TryGetValue( "IMMUNE", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.IMMUNE = list.ToArray();
            }

            if( d.TryGetValue( "AUTO", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.AUTO = list.ToArray();
            }

            if( d.TryGetValue( "HALVE", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.HALVE = list.ToArray();
            }

            if( d.TryGetValue( "NEGATE", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.NEGATE = list.ToArray();
            }

            if( d.TryGetValue( "DOUBLE", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.DOUBLE = list.ToArray();
            }

            if( d.TryGetValue( "ABSORB", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.ABSORB = list.ToArray();
            }

            if( d.TryGetValue( "ENABLE", out o ) ) {
                List<String> list = new List<String>();

                foreach( object s in o as List<object> ) {
                    list.Add( s as string );
                }

                this.ENABLE = list.ToArray();
            }

        }

        public static void initItems() {
            Item item;
            List<Object> templist = (List<Object>)(Utility.parse_JSON(@"content\dat\Items.json"));
            masterItemList = new Dictionary<string, Item>();

            foreach (Object obj in templist) {
                item = new Item((Dictionary<String, Object>)obj);
                masterItemList.Add(item.name.ToLower(), item);
            }
           
        }
    }

    public enum EquipSlotType { Accessory, RightHand, LeftHand, Body, NONE };

    public class EquipmentSlot {
        Item equipped;
        EquipSlotType slotType;
        public static readonly string[] names = {"r. hand", "l. hand", "body", "acc. 1", "acc. 2"};

        public static EquipSlotType typeFromName(string name) {
            switch (Array.IndexOf<string>(names, name)) {
                case 0: return EquipSlotType.RightHand;
                case 1: return EquipSlotType.LeftHand;
                case 2: return EquipSlotType.Body;
                case 3: return EquipSlotType.Accessory;
                case 4: return EquipSlotType.Accessory;
                default: return EquipSlotType.NONE;
            }
        }

        public EquipmentSlot( EquipSlotType est ) {
            equipped = null;
            slotType = est;
        }

        // Equips an item. If force = true, the item will destroy anything currently equipped in that slot!
        public void Equip(string name, Inventory inv) { Equip(Item.masterItemList[name.ToLower()], inv, false); }
        public void Equip(Item i, Inventory inv) { Equip(i, inv, false); }
        public void Equip(string name, Inventory inv, bool force) { Equip(Item.masterItemList[name.ToLower()], inv, force); }
        public void Equip(Item i, Inventory inv, bool force) {
            if(!force && equipped != null ) {
                throw new Exception( "Tried to equip ("+i.name+") without first removing ("+equipped.name+")" );
            }

            if( i.type != ItemType.Equipment ) {
                throw new Exception( "Tried to equip a non-equipment." );
            }

            inv.TakeItem( i, 1 );

            equipped = i;
        }

        // Removes an item. If discard = true, the item will be destroyed rather than sent to inventory!
        public Item Dequip(Inventory inv) { return Dequip(inv, false); }
        public Item Dequip( Inventory inv, bool discard ) {
            if( equipped == null ) {
                return null;
            }

            if (!discard) inv.AddItem( equipped, 1 );

            Item i = equipped;
            equipped = null;
            return i;
        }

        public Item getItem() {
            return equipped;
        }

        public EquipSlotType getSlotType() {
            return slotType;
        }

        public int getStatMod( Stat s ) {
            if( equipped == null ) return 0;

            int value;
            if( equipped.equip_stats.TryGetValue( s, out value ) )
                return value;

            return 0;
        }
    }
}
