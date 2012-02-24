using System;
using System.Collections.Generic;

using XNAVERGE;
using Microsoft.Xna.Framework;

// This is just a big pile of delegates, as well as any special classes/structs those delegates use to group their parameters.
namespace XNAVERGE {
    // The generic no-frills delegate, most often used for chains of event-scripting callbacks.
    public delegate void BasicDelegate();

    // ZoneDelegates are events called when zones are activated. Their arguments are the coordinates of the triggering zone,
    // and whether it was triggered via step or adjacent activation.
    public delegate void ZoneDelegate(int tx, int ty, bool adjacent);

    // This type of delegate is called when an entity is activated adjacently by the player ("talked to").
    public delegate void EntityActivationDelegate(Entity ent);
   

    public delegate int EntityMovementDelegate(Entity ent, ref EntityMovementData data); 

    // MovescriptDelegates can be associated with movestrings and are called when the movestring completes. Currently
    // the "aborted" value is always false, but eventually it will be possible to set movestrings to timeout, in which
    // case they will call the delegate with aborted true.
    // MovescriptEndingDelegates should not change entity position, alter other entities, or change the entity roster.
    // If you need to do that, I'll be adding an additional callback you can set up during the logic delegate, to 
    // be called after movement and collision are finished.
    public delegate void MovestringEndingDelegate(Entity ent, bool aborted);

    // The type of script that handles a scripted render layer's appearance. ScriptRenderLayers do little more than
    // call their RenderLayerDelegate when drawing. None of the set-up (spritebatch initialization, etc) is done for
    // you, so the delegate will need to include that and choose whether to obey the layer's blending properties, etc. 
    // The "clipping_region" argument is a screen-bounds rectangle, offset by parallax-adjusted camera coordinates.
    // For example, when the ScriptRenderLayer has zero parallax (the default), the rectangle just goes from 
    // (0, 0) to (screen.w-1, screen.h-1).
    public delegate void RenderLayerDelegate(ScriptRenderLayer layer, Rectangle clipping_region);
}

public struct EntityMovementData {
    public bool first_call;
    public bool collided; // TODO: add data indicating what was collided with?
    public Vector2 starting_point, attempted_path, actual_path;
    public Vector2 collision_direction {
        get { return attempted_path - actual_path; }
    }
    public int time; // in hundredths of ticks    
}