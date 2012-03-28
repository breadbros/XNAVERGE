using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sully {

    public class Inventory {
        public ItemSet consumables, equipment, key;

        public Inventory() {
            consumables = new ItemSet();
            equipment = new ItemSet();
            key = new ItemSet();
        }

        public Boolean HasItem( Item i ) {
            return HasItem( i.name );
        }

        public Boolean HasItem( String s ) {
            foreach( ItemSlot slot in consumables.items ) {
                if( slot.item.name == s ) return true;
            }

            foreach( ItemSlot slot in equipment.items ) {
                if( slot.item.name == s ) return true;
            }

            foreach( ItemSlot slot in key.items ) {
                if( slot.item.name == s ) return true;
            }

            return false;
        }


        public void AddItem( Item i, int quant ) {
            if( i.is_supply ) {
                consumables.AddItem( i, quant );
            } else if( i.is_key ) {
                key.AddItem( i, quant );
            } else if( i.is_equipment ) {
                equipment.AddItem( i, quant );
            } else {
                throw new System.InvalidOperationException( "Invalid item type." );
            }
        }

        public ItemSet GetWearableEquipmentSet( String klass, EquipSlotType slot ) {

            ItemSet ret = new ItemSet();

            foreach( ItemSlot sl in equipment.items ) {
                if( sl.item.equip_slot == slot && sl.item.equip_classes.Contains( klass ) ) {
                    ret.AddItem( sl.item, sl.quant );
                }
            }

            return ret;
        }
    }

    public class ItemSet {
        public List<ItemSlot> items;

        public ItemSet() {
            items = new List<ItemSlot>();
        }

        public void AddItem( Item item, int quant ) {
            if( quant <= 0 ) {
                throw new System.InvalidOperationException( "You can only add positive numbers of items to your inventory." );
            }

            foreach( ItemSlot slot in items ) {
                if( slot.item.name == item.name ) {
                    slot.quant += quant;
                    return;
                }
            }

            items.Add( new ItemSlot( item, quant ) );
        }
    }

    public class ItemSlot {
        public Item item;
        public int quant;

        public ItemSlot( Item i, int quant ) {
            this.item = i;
            this.quant = quant;
        }
    }


    public class Item {
        public string name, description;
        public bool is_supply, is_key, is_equipment;
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

            is_equipment = ( equip_classes != null );
            is_key = (price == 0);
            is_supply = (!is_key && !is_equipment); 
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

        public void Equip(Item i) {
            if( equipped != null ) {
                throw new Exception( "Tried to equip ("+i.name+") without first removing ("+equipped.name+")" );
            }

            equipped = i;
        }

        public Item Dequip() {
            if( equipped == null ) {
                throw new Exception( "Tried to Dequip when nothing was equipped." );
            }

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
