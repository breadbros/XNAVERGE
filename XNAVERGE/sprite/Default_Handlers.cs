using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAVERGE {

    // This class holds default processing functions to be used for customizable XNAVERGE hooks.
    public static class Default_Handlers {
        internal static VERGEGame game;

        public static void Entity_Movescript_Handler(Entity ent) {
            if (VERGEGame.game.player == ent) { return; }

            ent.velocity = new Vector2(0.25f, 0.25f);
        }

    }

}
