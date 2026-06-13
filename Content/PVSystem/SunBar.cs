using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlantsVS.Content.Plants.Base;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace  PlantsVS.Content.PVSystem
{
    class SunBarUI : UIState
    {
        public float VerticalAnimationPos;
        const float VerticalAnimationLerp = 0.04f;

        protected override void DrawSelf(SpriteBatch spriteBatch) {

            if (Main.LocalPlayer.HeldItem.ModItem is not BaseSummonItem)
            {
                if(Math.Round(VerticalAnimationPos) != 110)
                {
                    VerticalAnimationPos = MathHelper.Lerp(VerticalAnimationPos, 110, VerticalAnimationLerp);
                }
            }
            else if (Math.Round(VerticalAnimationPos) != 0)
            {
               VerticalAnimationPos = MathHelper.Lerp(VerticalAnimationPos, 0, VerticalAnimationLerp);
            }

            if (Math.Round(VerticalAnimationPos) == 110)
            {
                return;
            }

            RasterizerState OverflowHiddenRasterizerState = new(){
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            
            spriteBatch.End();
			spriteBatch.Begin(UseImmediateMode ? SpriteSortMode.Immediate : SpriteSortMode.Deferred, BlendState.AlphaBlend, (OverrideSamplerState != null) ? OverrideSamplerState : SamplerState.PointClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);

            Vector2 SunBarPos = new(Main.screenWidth - 640, 35 - VerticalAnimationPos);

            Vector2 BarPos = SunBarPos;

            Texture2D FullBarTexture = ModContent.Request<Texture2D>("PlantsVS/Content/PVSystem/SunBarFull").Value;

            Texture2D FrontIconTexture = ModContent.Request<Texture2D>("PlantsVS/Content/PVSystem/SunBarIconFront").Value;
            Texture2D BackIconTexture = ModContent.Request<Texture2D>("PlantsVS/Content/PVSystem/SunBarIconBack").Value;

            Vector2 IconOrigin = new(BackIconTexture.Width / 2, BackIconTexture.Height / 2);
            Vector2 IconPos = SunBarPos + new Vector2(-22.5f, 15);

            Vector2 TextPos = SunBarPos + new Vector2(-22f, 15);

            float BackIconRotation = (float)(Main.timeForVisualEffects / 500);
            float FrontIconScale = (float)(2 + Math.Sin(Main.timeForVisualEffects / 100) / 6);

            // Draw the empty part of the bar
			spriteBatch.Draw(ModContent.Request<Texture2D>("PlantsVS/Content/PVSystem/SunBarEmpty").Value, BarPos, null, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0f);
            // Draw the full part of the bar

            PlantPlayer modPlayer = Main.LocalPlayer.GetModPlayer<PlantPlayer>();
            Rectangle FullBarClip = FullBarTexture.Bounds;
            int PercantageClip = (int)(FullBarTexture.Width / (float)modPlayer.GetMaxSun() * modPlayer.CurrentSun);
            FullBarClip.Width = PercantageClip;

			spriteBatch.Draw(FullBarTexture, BarPos, FullBarClip, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0f);

            // Draw the back of the sun
            spriteBatch.Draw(BackIconTexture, IconPos, null, Color.White, BackIconRotation, IconOrigin, 2, SpriteEffects.None, 0f);
            // Draw the front of the sun
            spriteBatch.Draw(FrontIconTexture, IconPos, null, Color.White, 0, IconOrigin, FrontIconScale, SpriteEffects.None, 0f);

            // Draw text
            spriteBatch.End();
            spriteBatch.Begin(UseImmediateMode ? SpriteSortMode.Immediate : SpriteSortMode.Deferred, BlendState.AlphaBlend, (OverrideSamplerState != null) ? OverrideSamplerState : SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);

            DynamicSpriteFont font = FontAssets.DeathText.Value;
            string TextToDisplay = "Sun: " + modPlayer.CurrentSun + "/" + modPlayer.GetMaxSun();

            DrawStringWithOutline(spriteBatch, font, TextToDisplay, TextPos, Color.White, Color.Black, 0.45f);
		}

        private static void DrawStringWithOutline(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color textColor, Color outlineColor, float scale = 1f) {
            float thickness = 2f;

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) continue;
                    sb.DrawString(font, text, position + new Vector2(x, y) * thickness, outlineColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            sb.DrawString(font, text, position, textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
    }


	[Autoload(Side = ModSide.Client)]
	internal class SunBarUISystem : ModSystem
	{
		private UserInterface SunBarUserInterface;

		internal SunBarUI SunBar;

		public override void Load() {
			SunBar = new();
			SunBarUserInterface = new();
			SunBarUserInterface.SetState(SunBar);
		}

		public override void UpdateUI(GameTime gameTime) {
			SunBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"PlantsVS: Sun Bar",
					delegate {
						SunBarUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
    }
}