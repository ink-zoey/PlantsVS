using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PlantsVS.Content.Projectiles
{
	public class CommonPea : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.SentryShot[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 9;
			Projectile.height = 9;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 600;
            Projectile.scale = 2;
            DrawOffsetX = 4;
			DrawOriginOffsetY = 4;
		}

		public override void AI() {
		}

		public override void OnKill(int timeLeft)
        {
        }
	}
}