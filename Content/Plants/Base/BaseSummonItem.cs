using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Common;
using PlantsVS.Content.PVSystem;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PlantsVS.Content.Plants.Base
{

	abstract public class BaseSummonItem : ModItem
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

        public override void HoldItem(Player player)
        {
			if (Main.netMode != NetmodeID.Server && !PlayerHasProjectile(player.whoAmI, ModContent.ProjectileType<PlantPlaceCheck>()))
			{
				Main.NewText("New PlaceCheckDisplay created!");
            	Projectile.NewProjectile(player.GetSource_FromThis(), PlantsVSCommon.PlantPlacePosition, Vector2.Zero, ModContent.ProjectileType<PlantPlaceCheck>(), 0, 0, Main.myPlayer);
			}
        }

    }

    public class PlantPlaceCheck : ModProjectile {
		public override void SetDefaults()
        {
			Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.scale = 2;
        }

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.Center = player.Center;

			if (player.HeldItem.ModItem is not BaseSummonItem)
			{
				Main.NewText("Old PlaceCheckDisplay destroyed!");
				Projectile.Kill();
			}
    	}

		public Texture2D RequestTexture { get => TextureAssets.Projectile[Type].Value; }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.gamePaused) { 
				return false; 
			} 

			// Get position to draw selection
			Vector2 DrawPos = PlantsVSCommon.PlantPlacePositionWithoutScreen - Main.screenPosition - new Vector2(16);

			// Get color to draw selection
			float Blink = (float)Math.Abs(Math.Sin(Main.timeForVisualEffects / 50));
			Color DrawColor = new(Blink, Blink, Blink, Blink);

			Rectangle TextureBounds = new(0, (int) (24 * (Math.Round(Main.timeForVisualEffects / 15) % 4)), 24, 24);

			Main.EntitySpriteDraw(RequestTexture, DrawPos, TextureBounds, DrawColor,  0, Vector2.Zero, 2, SpriteEffects.None, 0f);

			return false;
        }
	}
}