using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Common;
using PlantsVS.Content.PVSystem;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace PlantsVS.Content.Plants.Base
{
	public abstract class BaseSummonItem : ModItem
	{
        public int SunCost;

		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
		}

		public override void SetDefaults() {
			Item.DamageType = ModContent.GetInstance<PlantClass>();
			Item.sentry = true;
			Item.scale = 2;
			Item.width = 25;
			Item.height = 25;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item83;
		}

		// A little modified version of Player.FindSentryRestingSpot
		public static bool FindSpotToPlacePlant(int checkProj, out int worldX, out int worldY)
		{
			const int VerticalFloorCheck = 3;

			Vector2 CheckPosition = PlantsVSCommon.PlantPlacePosition;
			int posX = (int)(CheckPosition.X / 16f);
			int posY = (int)(CheckPosition.Y / 16f);

			worldX = (int) PlantsVSCommon.PlantPlacePosition.X;
            worldY = (int) PlantsVSCommon.PlantPlacePosition.Y;

			// Check if there's valid floor to place plant
 			for (int i = 0; i < 3; i++){
				if (Main.tile[posX + i, posY + VerticalFloorCheck] == null || !WorldGen.SolidTile2(posX + i, posY + VerticalFloorCheck)){
					Rectangle WarnRect = new((int)CheckPosition.X, (int)CheckPosition.Y, 1, 1);
					CombatText.NewText(WarnRect, Color.Red, "Invalid floor to place plant!", false, true);
					return false;
				}
			}

			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            // Check if you have enough sun remaning to spawn the plant
            BasePlant PlantToSummon = (BasePlant) ContentSamples.ProjectilesByType[type].ModProjectile;

            if(player.GetModPlayer<PlantPlayer>().CurrentSun < PlantToSummon.SunCost)
            {
                Main.NewText("Not enough sun!");
                return false;
            }

			Projectile ProjectileRef = ContentSamples.ProjectilesByType[type];

            if (!FindSpotToPlacePlant(type, out int worldX, out int worldY)) { 
				return false; 
			}

		    player.GetModPlayer<PlantPlayer>().CurrentSun -= PlantToSummon.SunCost;

			position = new Vector2(worldX + PlantToSummon.Projectile.width / 3, worldY + PlantToSummon.Projectile.height / 3);

			Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, Main.myPlayer);

			for (int i = 0; i < 5; i++) 
			{ 
				int dust = Dust.NewDust(position, -ProjectileRef.width, ProjectileRef.height / 2, DustID.Dirt);
				Main.dust[dust].velocity.Y -= 1.2f;
			}

			return false;
		}

		bool PlayerHasProjectile(int playerIndex, int type)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.type == type && proj.owner == playerIndex)
					return true;
			}
			return false;
		}

        private sealed class PlacementVisual : ModSystem
        {
            private static readonly Asset<Texture2D> placementArea =
                ModContent.Request<Texture2D>("PlantsVS/Content/Plants/Base/PlantPlaceCheck");

            public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
            {
                int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Tile Grid Option"));
                if (mouseTextIndex == -1)
                {
                    return;
                }

                layers.Insert(
                    mouseTextIndex,
                    new LegacyGameInterfaceLayer(
                        $"{nameof(PlantsVS)}: {nameof(PlacementVisual)}",
                        () =>
                        {
                            if (Main.LocalPlayer.HeldItem.ModItem is BaseSummonItem)
                            {
                                DrawPlacementVisual(Main.spriteBatch);
                            }
                            return true;
                        },
                        InterfaceScaleType.None
                    )
                );

                return;

                static void DrawPlacementVisual(SpriteBatch sb)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    {
                        var drawPos = PlantsVSCommon.PlantPlacePositionWithoutScreen;
                        drawPos -= Main.screenPosition;
                        drawPos -= new Vector2(16);

                        var blink = (float)Math.Abs(Math.Sin(Main.timeForVisualEffects / 50));
                        var drawColor = Color.White * blink;

                        var source = new Rectangle(
                            0,
                            (int)(24 * (Math.Round(Main.timeForVisualEffects / 15) % 4)),
                            24,
                            24
                        );

                        sb.Draw(
                            placementArea.Value,
                            drawPos,
                            source,
                            drawColor,
                            0,
                            Vector2.Zero,
                            2f,
                            SpriteEffects.None,
                            0f
                        );
                    }
                    sb.End();
                    sb.Begin(); // Doesn't matter what's used here
                }
            }
        }
    }
}