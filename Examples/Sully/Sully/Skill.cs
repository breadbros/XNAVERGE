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

    public class Skill {

    }
}