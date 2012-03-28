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
        public string name, description;
        public ItemType type;
        public int icon, price;
        public bool use_battle, use_menu;
        public string func_targetting, func_effect;
        public string[] equip_classes;
        public string equip_slot, equip_modcode;

        public static Dictionary<string, Item> masterItemList;

        public Item( Dictionary<string, Object> d ) {

            name = ((string)d["name"]).Replace( '_', ' ' );
            description = (string)d["description"];
            icon = int.Parse( (string)d["icon"] );
            price = int.Parse( (string)d["price"] );
            use_battle = int.Parse( (string)d["use_battle"] ) > 0;
            use_menu = int.Parse( (string)d["use_menu"] ) > 0;

            func_targetting = (string)d["func_targetting"];
            func_effect = (string)d["func_effect"];

            equip_slot = (string)d["equip_slot"];
            equip_modcode = (string)d["equip_modcode"];

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

    [Serializable]
    class EquipmentSlot {
        //        Equipment equipped;

        public EquipmentSlot() {

        }
    }

}
