using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This is just a big pile of delegates, as well as any special classes/structs those delegates use to group their parameters.
namespace XNAVERGE {
    public delegate void BasicDelegate();

    public delegate void ZoneDelegate(int tx, int ty, bool adjacent);

    public delegate void EntityActivationDelegate(Entity ent);

    public delegate void MovescriptDelegate(Entity ent, bool aborted);
}
