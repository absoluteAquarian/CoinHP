using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CoinHP.API{
	public static class VanillaGUI{
		internal static void DrawResourcesNoHearts(){
			CoinInterface.UI_ScreenAnchorX = Main.screenWidth - 800;

			CoinInterface.DrawCoinsIndicator(Main.spriteBatch);

			DrawInterface_Resources_Mana();

			Main.spriteBatch.End();

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
			
			DrawInterface_Resources_Breath();
			
			Main.spriteBatch.End();

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
			
			DrawInterface_Resources_ClearBuffs();
			
			if(!Main.ingameOptionsWindow && !Main.playerInventory && !Main.inFancyUI)
				DrawInterface_Resources_Buffs();
		}

		private static void DrawInterface_Resources_ClearBuffs(){
			Main.buffString = "";
			Main.bannerMouseOver = false;
			if(!Main.recBigList)
				Main.recStart = 0;
		}

		private static void DrawInterface_Resources_Mana(){
			if(Main.LocalPlayer.ghost || Main.LocalPlayer.statManaMax2 <= 0)
				return;

			Vector2 vector = Main.fontMouseText.MeasureString(Language.GetTextValue("LegacyInterface.2"));
			int num = 50;
			if(vector.X >= 45f)
				num = (int)vector.X + 5;

			Main.spriteBatch.DrawString(Main.fontMouseText,
				Language.GetTextValue("LegacyInterface.2"),
				new Vector2(800 - num + CoinInterface.UI_ScreenAnchorX, 6f),
				new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor),
				0f,
				default,
				1f,
				SpriteEffects.None,
				0f);

			for(int i = 1; i < Main.LocalPlayer.statManaMax2 / 20 + 1; i++){
				int num2;
				bool flag = false;
				float num3 = 1f;
				if(Main.LocalPlayer.statMana >= i * 20){
					num2 = 255;
					if(Main.LocalPlayer.statMana == i * 20)
						flag = true;
				}else{
					float num4 = (Main.LocalPlayer.statMana - (i - 1) * 20) / (float)20;
					num2 = (int)(30f + 225f * num4);
					if(num2 < 30)
						num2 = 30;

					num3 = num4 / 4f + 0.75f;
					if(num3 < 0.75)
						num3 = 0.75f;

					if(num4 > 0f)
						flag = true;
				}

				if(flag)
					num3 += Main.cursorScale - 1f;

				int a = (int)((float)num2 * 0.9);
				Main.spriteBatch.Draw(Main.manaTexture,
					new Vector2(775 + CoinInterface.UI_ScreenAnchorX, 30 + Main.manaTexture.Height / 2 + (Main.manaTexture.Height - Main.manaTexture.Height * num3) / 2f + 28 * (i - 1)),
					new Rectangle(0, 0, Main.manaTexture.Width, Main.manaTexture.Height),
					new Color(num2, num2, num2, a),
					0f,
					new Vector2(Main.manaTexture.Width / 2, Main.manaTexture.Height / 2),
					num3,
					SpriteEffects.None,
					0f);
			}
		}

		private static void DrawInterface_Resources_Buffs(){
			Main.recBigList = false;
			int num = -1;
			int num2 = 11;

			for(int i = 0; i < Player.MaxBuffs; i++){
				if(Main.LocalPlayer.buffType[i] > 0){
					int b = Main.LocalPlayer.buffType[i];
					int x = 32 + i * 38;
					int num3 = 76;

					if(i >= num2){
						x = 32 + Math.Abs(i % 11) * 38;
						num3 += 50 * (i / 11);
					}

					num = DrawBuffIcon(num, i, b, x, num3);
				}else
					Main.buffAlpha[i] = 0.4f;
			}

			if(num < 0)
				return;

			int num4 = Main.LocalPlayer.buffType[num];
			if(num4 > 0){
				Main.buffString = Lang.GetBuffDescription(num4);
				if(num4 == 26 && Main.expertMode)
					Main.buffString = Language.GetTextValue("BuffDescription.WellFed_Expert");

				if(num4 == 147)
					Main.bannerMouseOver = true;

				if(num4 == 94){
					int num5 = (int)(Main.LocalPlayer.manaSickReduction * 100f) + 1;
					Main.buffString = Main.buffString + num5 + "%";
				}

				int rare = Main.meleeBuff[num4] ? -10 : 0;
				BuffLoader.ModifyBuffTip(num4, ref Main.buffString, ref rare);
				Main.instance.MouseTextHackZoom(Lang.GetBuffName(num4), rare);
			}
		}

		private static int DrawBuffIcon(int drawBuffText, int i, int b, int x, int y){
			if(b == 0)
				return drawBuffText;

			Color color = new Color(Main.buffAlpha[i], Main.buffAlpha[i], Main.buffAlpha[i], Main.buffAlpha[i]);
			
			Main.spriteBatch.Draw(Main.buffTexture[b], new Vector2(x, y), new Rectangle(0, 0, Main.buffTexture[b].Width, Main.buffTexture[b].Height), color, 0f, default, 1f, SpriteEffects.None, 0f);

			if(!Main.vanityPet[b] && !Main.lightPet[b] && !Main.buffNoTimeDisplay[b] && (!Main.LocalPlayer.honeyWet || b != 48) && (!Main.LocalPlayer.wet || !Main.expertMode || b != 46) && Main.LocalPlayer.buffTime[i] > 2){
				string text = Lang.LocalizedDuration(new TimeSpan(0, 0, Main.LocalPlayer.buffTime[i] / 60), abbreviated: true, showAllAvailableUnits: false);
				Main.spriteBatch.DrawString(Main.fontItemStack, text, new Vector2(x, y + Main.buffTexture[b].Height), color, 0f, default, 0.8f, SpriteEffects.None, 0f);
			}

			if(Main.mouseX < x + Main.buffTexture[b].Width && Main.mouseY < y + Main.buffTexture[b].Height && Main.mouseX > x && Main.mouseY > y){
				drawBuffText = i;
				Main.buffAlpha[i] += 0.1f;

				bool flag = Main.mouseRight && Main.mouseRightRelease;
				if(PlayerInput.UsingGamepad){
					flag = Main.mouseLeft && Main.mouseLeftRelease && Main.playerInventory;
					if(Main.playerInventory)
						Main.LocalPlayer.mouseInterface = true;
				}else
					Main.LocalPlayer.mouseInterface = true;

				if(flag)
					TryRemovingBuff(i, b);
			}else
				Main.buffAlpha[i] -= 0.05f;

			if(Main.buffAlpha[i] > 1f)
				Main.buffAlpha[i] = 1f;
			else if(Main.buffAlpha[i] < 0.4)
				Main.buffAlpha[i] = 0.4f;

			if(PlayerInput.UsingGamepad && !Main.playerInventory)
				drawBuffText = -1;

			return drawBuffText;
		}

		private static void TryRemovingBuff(int i, int b){
			bool flag = false;
			if(!Main.debuff[b] && b != 60 && b != 151){
				if(Main.LocalPlayer.mount.Active && Main.LocalPlayer.mount.CheckBuff(b)){
					Main.LocalPlayer.mount.Dismount(Main.LocalPlayer);
					flag = true;
				}

				if(Main.LocalPlayer.miscEquips[0].buffType == b && !Main.LocalPlayer.hideMisc[0])
					Main.LocalPlayer.hideMisc[0] = true;

				if(Main.LocalPlayer.miscEquips[1].buffType == b && !Main.LocalPlayer.hideMisc[1])
					Main.LocalPlayer.hideMisc[1] = true;

				Main.PlaySound(12);
				if(!flag)
					Main.LocalPlayer.DelBuff(i);
			}
		}

		private static void DrawInterface_Resources_Breath(){
			bool flag = false;
			if(Main.LocalPlayer.dead)
				return;

			if(Main.LocalPlayer.lavaTime < Main.LocalPlayer.lavaMax && Main.LocalPlayer.lavaWet)
				flag = true;
			else if(Main.LocalPlayer.lavaTime < Main.LocalPlayer.lavaMax && Main.LocalPlayer.breath == Main.LocalPlayer.breathMax)
				flag = true;

			Vector2 value = Main.LocalPlayer.Top + new Vector2(0f, Main.LocalPlayer.gfxOffY);
			if(Main.playerInventory && Main.screenHeight < 1000)
				value.Y += Main.LocalPlayer.height - 20;

			value = Vector2.Transform(value - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
			if(!Main.playerInventory || Main.screenHeight >= 1000)
				value.Y -= 100f;

			value /= Main.UIScale;
			if(Main.ingameOptionsWindow || Main.InGameUI.IsVisible){
				value = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + 236);
				if(Main.InGameUI.IsVisible)
					value.Y = Main.screenHeight - 64;
			}

			if(Main.LocalPlayer.breath < Main.LocalPlayer.breathMax && !Main.LocalPlayer.ghost && !flag){
				_ = Main.LocalPlayer.breathMax / 20;
				int num = 20;
				for(int i = 1; i < Main.LocalPlayer.breathMax / num + 1; i++){
					int num2;
					float num3 = 1f;
					if(Main.LocalPlayer.breath >= i * num){
						num2 = 255;
					}
					else{
						float num4 = (float)(Main.LocalPlayer.breath - (i - 1) * num) / num;
						num2 = (int)(30f + 225f * num4);
						if(num2 < 30)
							num2 = 30;

						num3 = num4 / 4f + 0.75f;
						if(num3 < 0.75)
							num3 = 0.75f;
					}

					int num5 = 0;
					int num6 = 0;
					if(i > 10){
						num5 -= 260;
						num6 += 26;
					}

					Main.spriteBatch.Draw(Main.bubbleTexture,
						value + new Vector2(26 * (i - 1) + num5 - 125f, 32f + (Main.bubbleTexture.Height - Main.bubbleTexture.Height * num3) / 2f + num6),
						new Rectangle(0, 0, Main.bubbleTexture.Width, Main.bubbleTexture.Height),
						new Color(num2, num2, num2, num2),
						0f,
						default,
						num3,
						SpriteEffects.None,
						0f);
				}
			}

			if(!(Main.LocalPlayer.lavaTime < Main.LocalPlayer.lavaMax && !Main.LocalPlayer.ghost && flag))
				return;

			int num7 = Main.LocalPlayer.lavaMax / 10;
			for(int j = 1; j < Main.LocalPlayer.lavaMax / num7 + 1; j++){
				int num8;
				float num9 = 1f;

				if(Main.LocalPlayer.lavaTime >= j * num7){
					num8 = 255;
				}else{
					float num10 = (Main.LocalPlayer.lavaTime - (j - 1) * num7) / (float)num7;
					num8 = (int)(30f + 225f * num10);
					if(num8 < 30)
						num8 = 30;

					num9 = num10 / 4f + 0.75f;
					if(num9 < 0.75)
						num9 = 0.75f;
				}

				int num11 = 0;
				int num12 = 0;
				if(j > 10){
					num11 -= 260;
					num12 += 26;
				}

				Main.spriteBatch.Draw(Main.flameTexture,
					value + new Vector2(26 * (j - 1) + num11 - 125f, 32f + (Main.flameTexture.Height - Main.flameTexture.Height * num9) / 2f + num12),
					new Rectangle(0, 0, Main.bubbleTexture.Width, Main.bubbleTexture.Height),
					new Color(num8, num8, num8, num8),
					0f,
					default,
					num9,
					SpriteEffects.None,
					0f);
			}
		}
	}
}
