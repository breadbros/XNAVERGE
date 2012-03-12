using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace Sully {
    
/* noodling without internet; this is all syntactically wrong.
  
    interface Sellable : Item {
        public int cost;
        Action onSell;
    }

    interface Usable : Item {
        Action onUse;
    }

    interface Equipment : Item {
        Action onEquip, onDequip;
    }
*/

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
            if( _.ItemIsConsumable( i ) ) {
                consumables.AddItem( i, quant );
            } else if( _.ItemIsEquipment( i ) ) {
                equipment.AddItem( i, quant );
            } else if( _.ItemIsKey( i ) ) {
                key.AddItem( i, quant );
            } else {
                throw new System.InvalidOperationException( "Invalid item type." );
            }
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

            items.Add( new ItemSlot(item, quant) );
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
    }

    class EquipmentSlot {
//        Equipment equipped;

        public EquipmentSlot() {

        }
    }

    enum Stat {
        STR, END, MAG, MGR, HIT, DOD, STK, FER, REA, CTR, ATK, DEF
    };

    class PartyMember {

        Dictionary<Stat, int> basestats;
        Entity ent;

        public PartyMember( Entity e ) {
            ent = e;
            basestats = new Dictionary<Stat, int>();
        }
        
        public int getStat( Stat s ) {
            return (int)s;
        }
    }

    class Party {
        List<Entity> party;

        public Party() {
            party = new List<Entity>();
        }

        public void AddPartyMember() {

        }
    }
}
