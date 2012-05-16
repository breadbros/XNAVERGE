﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization;

using XNAVERGE;

namespace Sully {
  
    public enum Stat {
        HP, MP, STR, END, MAG, MGR, HIT, DOD, STK, FER, REA, CTR, ATK, DEF
    };

    public partial class _ {
        public static Stat getStat( string s ) {
            switch( s ) {
                case "HP": return Stat.HP;
                case "MP": return Stat.MP;
                case "STR": return Stat.STR;
                case "END": return Stat.END;
                case "MAG": return Stat.MAG;
                case "MGR": return Stat.MGR;
                case "HIT": return Stat.HIT;
                case "DOD": return Stat.DOD;
                case "STK": return Stat.STK;
                case "FER": return Stat.FER;
                case "REA": return Stat.REA;
                case "CTR": return Stat.CTR;
                case "ATK": return Stat.ATK;
                case "DEF": return Stat.DEF;
                default: 
                    throw new Exception( "Unknown stat '" + s + "'" );
            }
        }
    }    
    
    public class PartyMember {
        public const int MAX_LEVEL = 50;
        public const int NUM_STATS = 14; // We need this since Enum.GetValues won't work on Xbox.

        private Dictionary<Stat, int> basestats;

        private Dictionary<string, EquipmentSlot> equipment_slots;
        public Dictionary<string, EquipmentSlot> equipment { get { return equipment_slots; } }

        public static readonly string[] equipment_slot_order = new string[] { "r. hand", "l. hand", "body", "acc. 1", "acc. 2" };

        public Entity ent;
        public string name, normal_chr, overworld_chr, battle_spr, statfile, description;

        public Klass klass;
        public int level { get; private set; }
        public int cur_xp { get; private set; }
        public int cur_hp { get; private set; }
        public int cur_mp { get; private set; }

        public void initEquipmentSlots() {
            equipment_slots = new Dictionary<string, EquipmentSlot>();
            foreach (string slotname in EquipmentSlot.names) {
                equipment_slots[slotname] = new EquipmentSlot(EquipmentSlot.typeFromName(slotname));
            }
        }

        public void _loadState(int xp, int hp, int mp) {
            basestats.Clear();
            basestats.Add(Stat.ATK, 0);
            basestats.Add(Stat.DEF, 0);
            level = 0;
            cur_xp = xp;
            cur_hp = hp;
            cur_mp = mp;
            _handleLevelUp();
        }

        public PartyMember( Entity e ) {
            basestats = new Dictionary<Stat, int>();
            ent = e;
            initEquipmentSlots();
        }

        public PartyMember() {
            basestats = new Dictionary<Stat, int>();
            basestats.Add( Stat.ATK, 0 );
            basestats.Add( Stat.DEF, 0 );
            initEquipmentSlots();
        }

        public int getStat( Stat s ) {
            int mod = 0;

            foreach( string key in equipment_slots.Keys ) {
                EquipmentSlot es = equipment_slots[key];
                mod += es.getStatMod( s );
            }

            return Math.Max(this.basestats[s] + mod, 1);
        }

        public int getPretendStat( Stat s, string slotKeyName, Item newItem ) {
            int mod = 0;

            foreach( string key in equipment_slots.Keys ) {

                if( key != slotKeyName ) {
                    EquipmentSlot e = equipment_slots[key];
                    mod += e.getStatMod( s );
                }
            }

            int value = 0;
            newItem.equip_stats.TryGetValue( s, out value );

            mod += value;

            return Math.Max( this.basestats[s] + mod, 1 );
        }

        public string getXpUntilNextLevel() {
            if( level >= PartyMember.MAX_LEVEL ) return "---";

            LevelUpData lud = PartyData.partyLevelUpData[name.ToLower()][this.level];

            return "" + ( lud.xp - this.cur_xp ); 
        }

        /// scans for a discrepancy between your current level and your current xp.
        private void _handleLevelUp() {
            int newlevel = 0;
            LevelUpData[] lud = PartyData.partyLevelUpData[name.ToLower()];

            foreach( LevelUpData l in lud ) {
                if( l.xp <= this.cur_xp ) {
                    newlevel = l.level;
                }
            }

            /// congrats, you've levelled up!
            while( this.level < newlevel ) {
                LevelUpData nextLevel = lud[this.level];

                if( nextLevel.level != this.level + 1 ) {
                    throw new Exception( "Expected nextLevel.level to be " + ( this.level + 1 ) + ", got " + nextLevel.level );
                }

                foreach( Stat s in nextLevel.stat_increases.Keys ) {

                    if( this.basestats.ContainsKey( s ) ) {
                        this.basestats[s] += nextLevel.stat_increases[s];
                    } else {
                        this.basestats.Add( s, nextLevel.stat_increases[s] );
                    }
                }

                this.cur_hp = this.basestats[Stat.HP];
                this.cur_mp = this.basestats[Stat.MP];

                this.level++;
            }

            // should probably return all of the LevelUpDatas for all new levels?  Deal with this when you can actually earn experience in-engine.
        }


        public void setXP( int new_xp ) {
            if( cur_xp > new_xp ) {
                throw new Exception( "No backsies!  Tried to set a lower XP amount for " + this.name + " (XP was " + cur_xp + ", tried to set to " + new_xp +")" );
            }

            cur_xp = new_xp;

            _handleLevelUp();
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
                throw new Exception( "Sully Level up data expects 14 well-formed int particles passed in...", e );
            }

        }
    }

    public class Party {
        List<PartyMember> party;
        ContentManager content;

        public Party( ContentManager cm ) {
            party = new List<PartyMember>();
            content = cm;
        }

        public void AddPartyMember( string name, int level ) {

            string safename = name.ToLower();

            PartyMember pm = PartyData.partymemberData[safename];
            
            if( pm == null ) {
                throw new Exception( "The party member '"+safename+"' didnt exist.  WHAT ARE YOU PLAYING AT?" );
            }

            if( party.Contains( pm ) ) {
                throw new Exception( "'"+safename+"' is already in your party.  NO CLONES ALLOWED." );
            }

            if( level <= 0 || level > PartyMember.MAX_LEVEL ) {
                throw new Exception( "Invalid level '" + level + "' for sully.  This game only supports [1," + PartyMember.MAX_LEVEL + "]." );
            }

            if( pm.level > level ) {
                throw new Exception( "You can't go backwards in level for '" + name + "'!  You asked for level " + level + ", and they had " + pm.level );
            }

            if( pm.ent == null && _.sg.map != null) {
                pm.ent = _.sg.map.spawn_entity(-10, -10, pm.normal_chr);
                pm.ent.name = "Party Member: " + pm.name; 
            }

            if( pm.level < level ) {
                LevelUpData lud = PartyData.partyLevelUpData[safename][level - 1];

                if( lud.level != level ) {
                    throw new Exception( "What devilry is this?  lookup for level "+level+" said it was actually for level " + lud.level +"!  Something is foul in your datafile." );
                }

                pm.setXP(lud.xp);
            }

            party.Add( pm );
        }

        // This removes a character from the party. If delete_entity is true, it will also get rid of the entity.
        // Returns true if the character was actually in the party, false if not.
        public bool RemovePartyMember(string name, bool delete_entity) {            
            PartyMember target = null;
            foreach (PartyMember p in party) {
                if (String.Equals(name, p.name, StringComparison.CurrentCultureIgnoreCase)) {
                    target = p;
                    break;
                }
            }
            if (target != null) {
                party.Remove(target);
                if (delete_entity && target.ent != null) {
                    VERGEGame.game.map.delete_entity(target.ent);
                    target.ent = null;
                }
                return true;
            }
            return false;
        }

        // Remove everyone from the party, such as before loading a save. If delete_entities is true, any
        // entities associated with the PartyMembers will be removed also.
        public void ClearParty(bool delete_entities) {
            foreach (PartyMember p in party) {                
                if (delete_entities && p.ent != null) {
                    VERGEGame.game.map.delete_entity(p.ent);
                    p.ent = null;
                }
            }
            party.Clear();
        }

        public PartyMember[] getMembers() {
            return party.ToArray();
        }

        public Entity[] getEntities() {
            List<Entity> ents = new List<Entity>();

            foreach( PartyMember pm in party ) {
                ents.Add( pm.ent );
            }

            return ents.ToArray();
        }
    }

    
    static class PartyData {

        public static Dictionary<string, LevelUpData[]> partyLevelUpData;
        public static Dictionary<string, PartyMember> partymemberData;

        public static void InitializePartyData() {

            partymemberData = new Dictionary<string, PartyMember>();
            partyLevelUpData = new Dictionary<string, LevelUpData[]>();

            {
                string output = Utility.read_file_text( @"content\dat\cast.txt" );

                string[] lines = output.Split( '\n' );

                foreach( string line in lines ) {

                    string[] particles = line.Split( '\t' );
                    
                    if( particles.Length == 2 ) {
                        string description = particles[1];
                        string[] words = _.explode( particles[0], ' ' );

                        if( words.Length == 6 ) {
                            PartyMember pm = new PartyMember();
                            pm.name = words[0];
                            pm.klass = Klass.get( words[1] );
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
                string output = Utility.read_file_text( "content/dat/statfiles/" + pm.statfile );

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

                if( data.Count != PartyMember.MAX_LEVEL ) {
                    throw new Exception( "There must be " + PartyMember.MAX_LEVEL + " entries in your levelup datafile, holmes." );
                }

                partyLevelUpData.Add( pm.name.ToLower(), data.ToArray() );
            }
        }

    }

    public class Klass {
        public static Dictionary<string, Klass> masterKlassList;

        public static void initClasses() {
            masterKlassList = new Dictionary<string, Klass>();

            Dictionary<String, Object> dict = (Dictionary<String, Object>)Utility.parse_JSON( @"content\dat\Class.json" );

            foreach( string key in dict.Keys ) {
                
                Klass k = new Klass( 
                    key, 
                    dict[key] as Dictionary<string, object>
                );

                masterKlassList.Add( key.ToLower(), k );
            }
        }

        public static Klass get( string s ) {
            Klass k = masterKlassList[s.ToLower()];

            if( k == null ) {
                throw new Exception( "Attempted to get an invalid klass named '"+s+"'.  Jerk." );
            }

            return k;
        }

        public string name { get {  return _name; } }  
        public string description { get {  return _description; } }
        public SkillType[] skills { get { return _skills; } }

        private string _name, _description;
        private SkillType[] _skills;

        public Klass( string name, Dictionary<string, object> dict )  {
            this._name = name;
            this._description = dict["description"] as string;

            List<object> o = (List<object>)dict["skills"];

            List<SkillType> ar = new List<SkillType>();
            foreach( string skilltypename in o ) {
                ar.Add( SkillType.get( skilltypename ) );
            }

            this._skills = ar.ToArray();
        }
    }
}
