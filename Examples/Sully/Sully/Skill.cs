using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sully {
    
    class SkillType {
        public static Dictionary<string, SkillType> masterSkillTypes;
        public static void initSkillTypes() {
            masterSkillTypes = new Dictionary<string, SkillType>();

            string output = System.IO.File.ReadAllText( "content/dat/Skilltypes.json" );

            ArrayList ar = fastJSON.JSON.Instance.Parse( output ) as ArrayList;

            foreach( ArrayList line in ar ) {
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
        public ArrayList haltingStatuses;

        public SkillType( ArrayList entry ) {
            name = entry[0] as string;
            icon = Int32.Parse( entry[1] as string );
            isHidden = (entry[2] as string).ToLower() == "hidden";
            haltingStatuses = entry[3] as ArrayList;
            description = entry[4] as string;
        }
    }

    class Skill {
    }
}

/*

        public static Dictionary<string, Klass> masterKlassList;

        public static void initClasses() {
            masterKlassList = new Dictionary<string, Klass>();

            string output = System.IO.File.ReadAllText( "content/dat/Class.json" );

            Dictionary<string, object> dict = fastJSON.JSON.Instance.Parse( output ) as Dictionary<string, object>;
            
            foreach( string key in dict.Keys ) {
                
                Klass k = new Klass( 
                    key, 
                    dict[key] as Dictionary<string, object>
                );

                masterKlassList.Add( key.ToLower(), k );
            }
        }

        public static Klass getKlass( string s ) {
            Klass k = masterKlassList[s.ToLower()];

            if( k == null ) {
                throw new Exception( "Attempted to get an invalid klass named '"+s+"'.  Jerk." );
            }

            return k;
        }

        public string name { get {  return _name; } }  
        public string description { get {  return _description; } }
        public string[] skills { get { return _skills; } }

        private string _name, _description;
        private string[] _skills;

        public Klass( string name, Dictionary<string, object> dict )  {
            this._name = name;
            this._description = dict["description"] as string;
            ArrayList ar = dict["skills"] as ArrayList;
            this._skills = (string[])ar.ToArray( typeof(string) );
        }
 */ 
