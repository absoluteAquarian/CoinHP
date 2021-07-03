using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace CoinHP{
	public class CoinUI : UIState{
		protected override void DrawSelf(SpriteBatch spriteBatch){
			string text = Language.GetTextValue("LegacyInterface.0") + " " + Main.LocalPlayer.statLife + " / " + Main.LocalPlayer.GetModPlayer<CoinPlayer>().GetStartingHealth();
			Vector2 size = Main.fontMouseText.MeasureString(text);

			const float XAnchor = 630;

			if(!Main.LocalPlayer.ghost){
				//Draw "Life: X"
				Utils.DrawBorderStringFourWay(Main.spriteBatch,
					Main.fontMouseText,
					text,
					XAnchor - size.X * 0.5f + CoinInterface.UI_ScreenAnchorX,
					18f,
					Color.White,
					Color.Black,
					default,
					1f);

				CoinPlayer mp = Main.LocalPlayer.GetModPlayer<CoinPlayer>();
				if(mp.extraLives > 0){
					text = "Extra Lives: " + mp.extraLives;
					size = Main.fontMouseText.MeasureString(text);
					
					//Draw "Extra Lives: X"
					Utils.DrawBorderStringFourWay(Main.spriteBatch,
						Main.fontMouseText,
						text,
						XAnchor - size.X * 0.5f + CoinInterface.UI_ScreenAnchorX,
						48f,
						Color.White,
						Color.Black,
						default,
						1f);
				}
			}
		}
	}
}
