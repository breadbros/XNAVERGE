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

        public PartyMember( Entity e ) {
            basestats = new Dictionary<Stat, int>();
            ent = e;
        }

        public PartyMember() {
            basestats = new Dictionary<Stat, int>();
        }

        public int getStat( Stat s ) {
            return (int)s;
        }
    }

    class LevelUpData {
        public int level;
        public int xp;
        public Dictionary<Stat, int> stat_increases;

        public LevelUpData( string[] data ) {

            stat_increases = new Dictionary<Stat, int>();

            if( data.Length != 14 ) {
                throw new Exception( "Sully Level up data expects 14 particles passed in..." );
            }

            try {
                level = Int32.Parse( data[0] );
                xp = Int32.Parse( data[1] );

                //hp  mp  str end mag mgr hit dod stk fer rea ctr
                stat_increases.Add( Stat.HP,  Int32.Parse( data[2] ) );
                stat_increases.Add( Stat.MP,  Int32.Parse( data[3] ) );
                stat_increases.Add( Stat.STR, Int32.Parse( data[4] ) );
                stat_increases.Add( Stat.END, Int32.Parse( data[5] ) );
                stat_increases.Add( Stat.MAG, Int32.Parse( data[6] ) );
                stat_increases.Add( Stat.MGR, Int32.Parse( data[7] ) );
                stat_increases.Add( Stat.HIT, Int32.Parse( data[8] ) );
                stat_increases.Add( Stat.DOD, Int32.Parse( data[9] ) );
                stat_increases.Add( Stat.STK, Int32.Parse( data[10] ) );
                stat_increases.Add( Stat.FER, Int32.Parse( data[11] ) );
                stat_increases.Add( Stat.REA, Int32.Parse( data[12] ) );
                stat_increases.Add( Stat.CTR, Int32.Parse( data[13] ) );

            } catch( FormatException e ) {
                throw new Exception( "Sully Level up data expects 14 well-formed int particles passed in..." );
            }

        }
    }

    class Party {
        List<Entity> party;

        public Party() {
            party = new List<Entity>();
        }

        public void AddPartyMember( string name ) {
            
        }

        public static PartyMember GetPartyMember( string name ) {

            return null;
        }
    }

    
    class PartyData {

        public static Dictionary<string, LevelUpData[]> partyLevelUpData;
        public static Dictionary<string, PartyMember> partymemberData;

        public static void InitializePartyData() {

            partymemberData = new Dictionary<string, PartyMember>();
            partyLevelUpData = new Dictionary<string, LevelUpData[]>();

            {
                string output = System.IO.File.ReadAllText( "content/dat/cast.txt" );

                string[] lines = output.Split( '\n' );

                foreach( string line in lines ) {

                    string[] particles = line.Split( '\t' );
                    
                    if( particles.Length == 2 ) {
                        string description = particles[1];
                        string[] words = _.explode( particles[0], ' ' );

                        if( words.Length == 6 ) {
                            PartyMember pm = new PartyMember();
                            pm.name = words[0];
                            pm.klass = words[1];
                            pm.normal_chr = words[2];
                            pm.overworld_chr = words[3];
                            pm.battle_spr = words[4];
                            pm.statfile = words[5];
                            pm.description = description;

                            partymemberData.Add( pm.name.ToLower(), pm );
                        }
                    }
                }
            }

            if( partymemberData.Count != 8 ) {
                throw new Exception( "Currently expect 8 party members, got: " + partymemberData.Count );
            }

            foreach( string key in partymemberData.Keys ) {
                PartyMember pm = partymemberData[key];
                string output = System.IO.File.ReadAllText( "content/dat/statfiles/" + pm.statfile );

                string[] lines = output.Split( '\n' );

                List<LevelUpData> data = new List<LevelUpData>();

                foreach( string line in lines ) {
                    string l = line.Trim();
                    string[] stats = _.explode( l, ' ' );

                    if( line.StartsWith( "//" ) ) {
                        continue;
                    }

                    if( stats.Length == 14 ) {
                        data.Add( new LevelUpData(stats) );
                    }
                }

                if( data.Count != 50 ) {
                    throw new Exception( "There must be 50 entries in your levelup datafile, holmes." );
                }

                partyLevelUpData.Add( pm.name.ToLower(), data.ToArray() );
            }
        }
    }
}
