using System;
using System.Collections.Generic;

using XNAVERGE;
using Microsoft.Xna.Framework;

// This is just a big pile of delegates, as well as any special classes/structs those delegates use to group their parameters.
namespace XNAVERGE {
    // The generic no-frills delegate, most often used for chains of event-scripting callbacks.
    public delegate void BasicDelegate();

    // for rendering events.
    public delegate void RenderDelegate( int px, int py );

    // ZoneDelegates are events called when zones are activated. Their arguments are the coordinates of the triggering zone,
    // and whether it was triggered via step or adjacent activation.
    public delegate void ZoneDelegate(int tx, int ty, bool adjacent);

    // This type of delegate is called when an entity is activated adjacently by the player ("talked to").
    public delegate void EntityActivationDelegate(Entity ent);
   
    /// This type of delegate is called at the begining of the game's update loop.  
    /// return true if you want normal map handling to occur after, false if you don't.
    public delegate bool GameControlDelegate();

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
    public Vector2 starting_point, attempted_path, actual_path;

    public bool collided;
    // What entity was collided with. If collided = false or the collision was with an obstacle, set to null.
    public Entity collided_entity;
    public bool obstructed_by_entity {
        get { return (collided_entity != null); }
    }
    public Vector2 collision_direction {
        get { return attempted_path - actual_path; }
    }    

    // Both these time variables are in speed-adjusted hundredths of ticks. That is, when speed is 100, they're
    // actually hundredths of ticks. Otherwise, 1 unit = 1/speed ticks.
    public int time; 
    public int time_shortfall; // if the movement was interrupted, this indicates how much of the intended movement time was "lost".
}