using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Content.PVSystem;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace PlantsVS.Content.Plants.Base
{
	abstract public class BasePlant : ModProjectile
	{

        // Drawing related variable and functions
        public Texture2D RequestTexture { get => TextureAssets.Projectile[Type].Value; }
        public float GeneralAnimSpeed = 3;
        public static ref float GlobalTimer => ref Main.GlobalTimeWrappedHourly;

        public Vector2 BasePos { 
            get => Projectile.position - Main.screenPosition + 
            new Vector2(Projectile.width / 2, Projectile.height);
        }

        // Ai related variable and functions
        public NPC? ProjTarget = null;

        protected bool JustSpawned {
			get => Projectile.localAI[0] == 0;
			set => Projectile.localAI[0] = value ? 0 : 1;
		}

        protected void TryTargeting(NPC npc, ref float closestTargetDistance, ref NPC targetNPC) {
			if (npc.CanBeChasedBy(this)) {
				float distanceToTargetNPC = Vector2.Distance(Projectile.Center, npc.Center);
				if (distanceToTargetNPC < closestTargetDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
					closestTargetDistance = distanceToTargetNPC;
					targetNPC = npc;
				}
			}
		}

        // Interaction with the cursor and stuff
        public override void PostDraw(Color lightColor)
        {
            if (Projectile.Hitbox.Contains(Main.MouseWorld.ToPoint())) {
                Main.LocalPlayer.cursorItemIconEnabled = true;
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<PeashooterItem>();

                if (Main.mouseRight && Main.mouseRightRelease && Player.BlockInteractionWithProjectiles == 0)
                {
                    Main.LocalPlayer.GetModPlayer<PlantPlayer>().CurrentSun += SunCost;
                    Projectile.Kill();
                }
            }
        }

        // Boilerplate stuff
        public virtual int SunCost => 0;
        public override void SetDefaults() {
			Projectile.DamageType = ModContent.GetInstance<PlantClass>();
			Projectile.timeLeft = Projectile.SentryLifeTime;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
            Projectile.scale = 2f;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) 
        {
            fallThrough = false; 
            return true;
        }
		public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public static implicit operator BasePlant(Projectile v)
        {
            throw new NotImplementedException();
        }
    }
}