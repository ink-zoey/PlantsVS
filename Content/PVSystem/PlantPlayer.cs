using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Content.Plants.Base;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace  PlantsVS.Content.PVSystem
{
	public class PlantPlayer : ModPlayer
	{
		public int CurrentSun;
		public const int DefaultMaxSun = 100;

        public override void Initialize()
        {
            CurrentSun = DefaultMaxSun;
        }

		public int GetMaxSun()
		{
			return DefaultMaxSun;
		}
	}
}