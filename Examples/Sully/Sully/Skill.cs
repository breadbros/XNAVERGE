using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAVERGE;

namespace Sully {
    
    public class SkillType {
        public static Dictionary<string, SkillType> masterSkillTypes;
        public static void initSkillTypes() {
            masterSkillTypes = new Dictionary<string, SkillType>();

            List<Object> list = (List<Object>)Utility.parse_JSON( @"content\dat\Skilltypes.json" );

            foreach( List<Object> line in list ) {
                SkillType st = new SkillType(line);
                masterSkillTypes.Add( st.name.ToLower(), st );
            }
        }

        public static SkillType get( string key ) {
            SkillType st = masterSkillTypes[key.ToLower()];

            if( st == null ) {
                throw new Exception( "Attempted to get an invalid SkillType named '" + key + "'.  Why?" );
            }

            return st;
        }

        public string name, description;
        public int icon;
        public bool isHidden;
        public String[] haltingStatuses;

        public SkillType( List<Object> entry ) {
            name = entry[0] as string;

            Int64? o = entry[1] as Int64?;
            icon = (int)o.Value;

            isHidden = (entry[2] as string).ToLower() == "hidden";

            List<object> obj = entry[3] as List<object>;
            List<string> foo = new List<string>();
            foreach( string s in obj ) {
                foo.Add( s );
            }
            haltingStatuses = foo.ToArray();
            
            description = entry[4] as string;
        }
    }

    public class Status {
        public static Dictionary<string, Status> masterStatus;

        public static void initStatuses() {
            masterStatus = new Dictionary<string, Status>();

            Dictionary<string, object> dict = (Dictionary<string, object>)Utility.parse_JSON( @"content\dat\Status.json" );

            foreach( string key in dict.Keys ) {

                Dictionary<string, object> myLine = (Dictionary<string, object>)dict[key];
                Status s = new Status( key, myLine );
                masterStatus.Add( s.name.ToLower(), s );
            }

            int i = 0;
        }

        public static Status get( string key ) {
            Status s = masterStatus[key.ToLower()];

            if( s == null ) {
                throw new Exception( "Attempted to get an invalid Status named '" + key + "'.  Status seeker." );
            }

            return s;
        }

        public string name { get; private set; }
        public int icon { get; private set; }
        public int duration { get; private set; }
        public string render_func { get; private set; }
        public string effect_func { get; private set; }
        public string description { get; private set; }
        public string[] remove_events { get; private set; }
        public string CANCELS { get; private set; }

        public Status( string name, Dictionary<string, object> entry ) {
            this.name = name;
            Int64? i;

            i = entry["icon"] as Int64?;
            icon = (int)i.Value;
            i = entry["duration"] as Int64?;
            duration = (int)i.Value;

            render_func = entry["render_func"] as string;
            effect_func = entry["effect_func"] as string;

            if( entry.ContainsKey( "CANCELS" ) ) {
                CANCELS = entry["CANCELS"] as string;
            }

            description = entry["description"] as string;

            List<object> obj = entry["remove"] as List<object>;
            List<string> foo = new List<string>();
            foreach( string s in obj ) {
                foo.Add( s );
            }
            remove_events = foo.ToArray();

        }
    }

    public class Skill {
        public static Dictionary<string, Skill> masterSkills;
        public static void initSkills() {
            masterSkills = new Dictionary<string, Skill>();

            List<Object> list = (List<Object>)Utility.parse_JSON( @"content\dat\Skills.json" );

            foreach( object line in list ) {
                Dictionary<string, object> myLine = (Dictionary<string, object>)line;
                Skill s = new Skill( myLine );
                masterSkills.Add( s.name.ToLower(), s );
            }

            int i = 0;
        }

        public static Skill get( string key ) {
            Skill s = masterSkills[key.ToLower()];

            if( s == null ) {
                throw new Exception( "Attempted to get an invalid Skill named '" + key + "'.  Why u no get real skill?" );
            }

            return s;
        }


        public string name { get; private set; }
        public string description { get; private set; }
        public int icon { get; private set; }
        public SkillType parentSkill { get; private set; }
        public bool use_battle { get; private set; }
        public bool use_menu { get; private set; }
        public int base_price { get; private set; }
        public int mp_cost { get; private set; }
        public string func_targetting { get; private set; }
        public string func_effect { get; private set; }
        public int charge_time { get; private set; }
        public int delay_time { get; private set; }
        public bool is_nullable { get; private set; }
        public bool is_reflectable { get; private set; }

        public Skill( Dictionary<string, object> entry ) {
            name = entry["name"] as string;
            description = entry["description"] as string;

            string s = entry["skilltype"] as string;
            if( s != "" ) {
                parentSkill = SkillType.get(s);
            } else {
                parentSkill = null;
            }

            Int64? i;

            i = entry["use_battle"] as Int64?;
            use_battle = i.Value != 0;
            i = entry["use_menu"] as Int64?;
            use_menu = i.Value != 0;
            i = entry["is_nullable"] as Int64?;
            is_nullable = i.Value != 0;
            i = entry["is_reflectable"] as Int64?;
            is_reflectable = i.Value != 0;

            i = entry["mp_cost"] as Int64?;
            mp_cost = (int)i.Value;
            i = entry["price"] as Int64?;
            base_price = (int)i.Value;
            i = entry["charge_time"] as Int64?;
            charge_time = (int)i.Value;
            i = entry["delay_time"] as Int64?;
            delay_time = (int)i.Value;

            func_targetting = entry["func_targetting"] as string;
            func_effect = entry["func_effect"] as string;
        }
    }
}