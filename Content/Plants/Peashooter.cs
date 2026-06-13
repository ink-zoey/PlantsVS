using PlantsVS.Content.Plants.Base;
using Terraria;
using Terraria.ModLoader;

namespace PlantsVS.Content.Plants
{

	public class Peashooter : BasePeashooter
	{
        public override int SunCost => 25;
		public override void SetDefaults() {
            base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 22;
		}
    }


	public class PeashooterItem: BaseSummonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 2;
			Item.shoot = ModContent.ProjectileType<Peashooter>();
		}
	}

}