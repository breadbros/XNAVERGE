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

    class Item {
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
