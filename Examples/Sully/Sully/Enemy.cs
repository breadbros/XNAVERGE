using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace Sully {

    public class DropSet {
        public int chance;
        public Item loot;
        public DropSet( int chance, string loot ) {
            this.chance = chance;
            this.loot = Item.get(loot);
        }
    }

    public class Enemy {
        public static Dictionary<string, Enemy> masterEnemies;

        public static void initEnemies() {
            masterEnemies = new Dictionary<string, Enemy>();

            Dictionary<string, object> dict = (Dictionary<string, object>)Utility.parse_JSON( @"content\dat\Enemies.json" );

            foreach( string key in dict.Keys ) {

                Enemy e = new Enemy( key, dict[key] as Dictionary<string, object> );
                masterEnemies.Add( e.name.ToLower(), e );
            }
        }

        public static Enemy get( string key ) {
            Enemy e = masterEnemies[key.ToLower()];

            if( e == null ) {
                throw new Exception( "Attempted to get an invalid Enemy named '" + key + "'.  Try being friends?" );
            }

            return e;
        }

        public string name { get; private set; }
        public int money { get; private set; }
        public int exp { get; private set; }
        
        public string spr_file { get; private set; }
        public int icon { get; private set; }
        public string ai_type { get; private set; }
        public Element ELEMENT { get; private set; }
        public Element ABSORB { get; private set; }
        public Element DOUBLE { get; private set; }
        public Element NEGATE { get; private set; }
        public Element HALVE { get; private set; }
        public DropSet DROP { get; private set; }

        public Dictionary<Stat, int> basestats { get; private set; }

        public Enemy( string key, Dictionary<string,object> line ) {
            name = key;

            spr_file = line["sprite"] as string;
            Int64? i;
            i = line["icon"] as Int64?;
            icon = (int)i.Value;
            i = line["money"] as Int64?;
            money = (int)i.Value;
            i = line["exp"] as Int64?;
            exp = (int)i.Value;

            Dictionary<Stat, int> basestats = new Dictionary<Stat, int>();
            Dictionary<string, object> stats = (Dictionary<string, object>)line["stats"];

            i = stats["HP"] as Int64?;
            basestats.Add( Stat.HP, (int)i.Value );
            i = stats["MP"] as Int64?;
            basestats.Add( Stat.MP, (int)i.Value );
            i = stats["STR"] as Int64?;
            basestats.Add( Stat.STR, (int)i.Value );
            i = stats["END"] as Int64?;
            basestats.Add( Stat.END, (int)i.Value );
            i = stats["MAG"] as Int64?;
            basestats.Add( Stat.MAG, (int)i.Value );
            i = stats["MGR"] as Int64?;
            basestats.Add( Stat.MGR, (int)i.Value );
            i = stats["HIT"] as Int64?;
            basestats.Add( Stat.HIT, (int)i.Value );
            i = stats["DOD"] as Int64?;
            basestats.Add( Stat.DOD, (int)i.Value );
            i = stats["STK"] as Int64?;
            basestats.Add( Stat.STK, (int)i.Value );
            i = stats["FER"] as Int64?;
            basestats.Add( Stat.FER, (int)i.Value );
            i = stats["REA"] as Int64?;
            basestats.Add( Stat.REA, (int)i.Value );
            i = stats["CTR"] as Int64?;
            basestats.Add( Stat.CTR, (int)i.Value ); 

            ai_type = line["ai"] as string;

            if( line.ContainsKey( "ELEMENT" ) ) {
                ELEMENT = Element.get( line["ELEMENT"] as string );
            }
            if( line.ContainsKey( "ABSORB" ) ) {
                ABSORB = Element.get( line["ABSORB"] as string );
            }
            if( line.ContainsKey( "DOUBLE" ) ) {
                DOUBLE = Element.get( line["DOUBLE"] as string );
            }
            if( line.ContainsKey( "NEGATE" ) ) {
                NEGATE = Element.get( line["NEGATE"] as string );
            }
            if( line.ContainsKey( "HALVE" ) ) {
                HALVE = Element.get( line["HALVE"] as string );
            }

            if( line.ContainsKey( "DROP" ) ) {
                List<object> dList = (List<object>)line["DROP"];
                string s = (string)dList[0];
                i = dList[1] as Int64?;
                int c = (int)i.Value;
               
                DROP = new DropSet( c, s );
            }
        }
    }
}
