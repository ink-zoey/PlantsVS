using Microsoft.Xna.Framework;
using Terraria;

namespace PlantsVS.Common
{
    public static class PlantsVSCommon
    {
		public static Vector2 PlantPlacePosition
        { 
            get => new Vector2(Player.tileTargetX, Player.tileTargetY) * 16f - new Vector2(6);
        }

        public static Vector2 PlantPlacePositionWithoutScreen
        { 
            get => new Vector2(Player.tileTargetX, Player.tileTargetY) * 16f;
        }
    }
}
