using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework.Content;

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
        HP, MP, STR, END, MAG, MGR, HIT, DOD, STK, FER, REA, CTR, ATK, DEF
    };

    class PartyMember {

        Dictionary<Stat, int> basestats;
        public Entity ent;
        public string name, klass, normal_chr, overworld_chr, battle_spr, statfile, description;  

        public PartyMember() {
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

        public void AddPartyMember( PartyMember pm ) {
            
        }



        public static Dictionary<string, PartyMember> partymemberData;

        public static void InitializePartyData(  ) {
            
            partymemberData = new Dictionary<string, PartyMember>();

            string output = System.IO.File.ReadAllText( "content/dat/cast.txt" );

            string[] lines = output.Split( '\n' );

            foreach( string line in lines ) {
                
                string[] particles = line.Split( '\t' );

                if( particles.Length == 2 ) {
                    string description = particles[1];
                    string[] words = _.explode( particles[0], ' ' );

                    if( words.Length == 6 ) {
                        PartyMember pm = new PartyMember();
                        pm.name             = words[0];
                        pm.klass            = words[1];
                        pm.normal_chr       = words[2];
                        pm.overworld_chr    = words[3];
                        pm.battle_spr       = words[4];
                        pm.statfile         = words[5];
                        pm.description = description;

                        partymemberData.Add( pm.name.ToLower(), pm );
                    }
                }
            }

            if( partymemberData.Count != 8 ) {
                throw new Exception( "Currently expect 8 party members, got: " + partymemberData.Count );
            }

            foreach( string key in partymemberData.Keys ) {
                PartyMember pm = partymemberData[key];
            }

            int i = 1;
            /*
            string s = cm.Load<string>( "dat/cast.dat" );

            using( TextReader rdr = new StreamReader( s ) ) {
                string line;

                while( ( line = rdr.ReadLine() ) != null ) {
                    // use line here
                }
            }
             * */
        }

        public static PartyMember GetPartyMember( string name ) {

            return null;
        }

    }
}
