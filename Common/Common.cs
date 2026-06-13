using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace PlantsVS.Common
{
    public static class PlantsVSCommon
    {

		public static Vector2 PlantPlacePosition { 
            get => Main.MouseWorld.ToTileCoordinates().ToVector2() * 16f - new Vector2(6);
        } 

        public static Vector2 PlantPlacePositionWithoutScreen { 
            get => Main.MouseWorld.ToTileCoordinates().ToVector2() * 16f;
        } 

    }
}
