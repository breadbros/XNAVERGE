using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sully {

    public class Inventory {       
        public List<ItemSlot> consumables { get { return item_sets[(int)ItemType.Consumable]; } }
        public List<ItemSlot> equipment { get { return item_sets[(int)ItemType.Equipment]; } }
        public List<ItemSlot> key { get { return item_sets[(int)ItemType.Key]; } }

        public List<ItemSlot>[] item_sets;
        public Dictionary<Item, ItemSlot> slotIndex;

        public Inventory() {
            item_sets = new List<ItemSlot>[Enum.GetValues(typeof(ItemType)).Length];
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

        public Inventory GetWearableEquipmentSet( String klass, EquipSlotType slot ) {

            Inventory ret = new Inventory();

            foreach( ItemSlot sl in this.equipment ) {
                if( sl.item.equip_slot == slot && sl.item.equip_classes.Contains( klass ) ) {
                    ret.AddItem( sl.item, sl.quant );
                }
            }

            if( slot != EquipSlotType.RightHand && slot != EquipSlotType.Body ) {
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
            if (slot.quant < 0) slot.quant = 0;
            if (slot.quant > ItemSlot.MAX_QUANT) slot.quant = ItemSlot.MAX_QUANT ;
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
        public string[] equip_classes;
        public string equip_modcode;
        public EquipSlotType equip_slot; 

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

            name = ((string)d["name"]).Replace( '_', ' ' );
            description = (string)d["description"];
            icon = int.Parse( (string)d["icon"] );
            price = int.Parse( (string)d["price"] );
            use_battle = int.Parse( (string)d["use_battle"] ) > 0;
            use_menu = int.Parse( (string)d["use_menu"] ) > 0;

            func_targetting = (string)d["func_targetting"];
            func_effect = (string)d["func_effect"];

            try {
                equip_slot = (EquipSlotType)Enum.Parse( typeof( EquipSlotType ), (string)d["equip_slot"], true );
            } catch( ArgumentException ) {
                equip_slot = EquipSlotType.NONE;
            }
            equip_modcode = (string)d["equip_modcode"];
            equip_stats = _statsHelper( equip_modcode );

            ArrayList al = d["equip_by"] as ArrayList;

            if( al.Count > 0 ) {
                equip_classes = (string[])al.ToArray( typeof( string ) );
            } else {
                equip_classes = null;
            }

            if (equip_classes != null) type = ItemType.Equipment;
            else if (price == 0) type = ItemType.Key;
            else type = ItemType.Consumable;
        }

        public static void initItems() {

            masterItemList = new Dictionary<string, Item>();

            string output = System.IO.File.ReadAllText( "content/dat/Items.json" );
           
            ArrayList items = fastJSON.JSON.Instance.Parse(output) as ArrayList;

            foreach( Dictionary<string, Object> d in items  ) {
                Item i = new Item(d);
                
                masterItemList.Add( ((string)d["name"]).ToLower(), i );
            }
        }
    }

    public class EquipmentSlot {
        Item equipped;
        EquipSlotType slotType;

        public EquipmentSlot( EquipSlotType est ) {
            equipped = null;
            slotType = est;
        }

        public void Equip(Item i, Inventory inv) {
            if( equipped != null ) {
                throw new Exception( "Tried to equip ("+i.name+") without first removing ("+equipped.name+")" );
            }

            if( i.type != ItemType.Equipment ) {
                throw new Exception( "Tried to equip a non-equipment." );
            }

            inv.TakeItem( i, 1 );

            equipped = i;
        }

        public Item Dequip( Inventory inv ) {
            if( equipped == null ) {
                return null;
            }

            inv.AddItem( equipped, 1 );

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
