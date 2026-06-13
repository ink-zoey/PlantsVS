using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PlantsVS.Content.Plants.Base
{
	abstract public class BasePeashooter : BasePlant
	{
        public float BlinkTimer;
        public ref float ShootTimer => ref Projectile.ai[0];
        public int SquishTimer = 0;
        const int SquishDuration = 20;


        private void StemDraw()
        {
            float StemRotation = (float)(Math.Sin(GlobalTimer * GeneralAnimSpeed) / 10f);
            DrawData StemDrawdata = new()
            {
                texture = RequestTexture,
                position = BasePos,
                sourceRect = new(0, 30, 30, 30),
                color = Color.White,
                rotation = StemRotation,
                scale = new(2),
                origin = new(17,17),
            };
            Main.EntitySpriteDraw(StemDrawdata);
        }

        private void LeafDraw()
        {
            DrawData LeafDrawdata = new()
            {
                texture = RequestTexture,
                position = BasePos,
                sourceRect = new(0, 60, 30, 30),
                color = Color.White,
                rotation = 0,
                scale = new(2),
                origin = new(17,17),
            };
            Main.EntitySpriteDraw(LeafDrawdata);
        }

        private void HeadDraw()
        {
            Vector2 HeadPos = BasePos;
            Vector2 HeadScale = new (2);
            Vector2 HeadOrigin = new(14,16);
            HeadPos.Y -= 29;
            HeadPos.Y += (float)Math.Abs(Math.Sin(GlobalTimer * GeneralAnimSpeed / 2) * 3);
            HeadPos.X += (float)(Math.Sin(GlobalTimer * GeneralAnimSpeed) * 3);
            HeadPos.X -= 1;
            float HeadRotation = (float)(Math.Cos(GlobalTimer * GeneralAnimSpeed / 2) / 15f);
            SpriteEffects HeadEffect = SpriteEffects.None;

            // Special head rotation if plant has target
            if (ProjTarget != null) {
                if ( ProjTarget.Center.X < Projectile.Center.X){
                    HeadEffect = SpriteEffects.FlipHorizontally;
                    HeadPos.X -= 9;
                    HeadOrigin.X = 17;
                }
                else
                {
                    HeadRotation -= 90 + 45;
                }
            
                HeadRotation += (float)Math.Atan2(Projectile.Center.Y - ProjTarget.Center.Y, Projectile.Center.X - ProjTarget.Center.X);
            }

            // Define if plant should blink
            Rectangle HeadSourceRect = new(0, 0, 30, 30);
            if (BlinkTimer < 10)
            {
                HeadSourceRect.Y = 90;
                HeadScale.Y = MathHelper.Lerp(2, 1.9f, 0f + BlinkTimer / 10f);
            }

            // Squish after shooting!
            if(SquishTimer > 0)
            {
                HeadScale.Y -= 0.5f * SquishTimer / SquishDuration;
                HeadScale.X += 0.2f * SquishTimer / SquishDuration;
            }
            

            DrawData HeadDrawdata = new()
            {
                texture = RequestTexture,
                position = HeadPos,
                sourceRect = HeadSourceRect,
                color = Color.White,
                rotation = HeadRotation,
                scale = HeadScale,
                origin = HeadOrigin,
                effect = HeadEffect
            };
            Main.EntitySpriteDraw(HeadDrawdata);
        }


        protected void ResetBlinking() => BlinkTimer = Main.rand.Next(100, 150);

        public override bool PreDraw(ref Color lightColor)
        {
            StemDraw();
            LeafDraw();
            HeadDraw();
            
            return false;
        }

        public override void AI()
        {
            // Some constants only used on AI
            const int TargetingRange = 50 * 16;
            const int ShootFrequency = 60;
            const float FireVelocity = 6f;

            // Spawn behaviour
            if (JustSpawned) {
				JustSpawned = false;
                ResetBlinking();
            }

            // Blinking behaviour
            if (BlinkTimer < 0) { ResetBlinking(); }
            BlinkTimer -= 1;

            // Gravity moment
            Projectile.velocity.X = 0f;
			Projectile.velocity.Y += 0.2f;
			if (Projectile.velocity.Y > 16f) { Projectile.velocity.Y = 16f; }

            // Targeting logic
            float closestTargetDistance = TargetingRange;
			NPC targetNPC = null;

			if (Projectile.OwnerMinionAttackTargetNPC != null) {
				TryTargeting(Projectile.OwnerMinionAttackTargetNPC, ref closestTargetDistance, ref targetNPC);
			}

			if (targetNPC == null) {
				foreach (var npc in Main.ActiveNPCs) {
					TryTargeting(npc, ref closestTargetDistance, ref targetNPC);
				}
			}

            ProjTarget = targetNPC;

            // Squish after shooting behaviour
            if (SquishTimer > 0){ SquishTimer--; }

            // Shooting behaviour
            if (targetNPC != null) {
				if (ShootTimer <= 0) {
					ShootTimer = ShootFrequency;

					SoundEngine.PlaySound(SoundID.Item102 with { Volume = 0.4f }, Projectile.Center);

					if (Main.myPlayer == Projectile.owner) {
						Vector2 shootDirection = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
						Vector2 shootVelocity = shootDirection * FireVelocity;

						int type = ModContent.ProjectileType<CommonPea>();;
                        Vector2 shootPos = new(Projectile.Center.X, Projectile.Center.Y + 2);
                        shootPos += shootDirection * 35;

						Projectile.NewProjectile(Projectile.GetSource_FromThis(), shootPos, shootVelocity, type, Projectile.damage, 3, Projectile.owner);
                        SquishTimer = SquishDuration;
                        BlinkTimer = 10;
					}
				}
			}

			ShootTimer--;

            base.AI();
        }
    }
}