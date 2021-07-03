using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace CoinHP{
	public static class CoinInterface{
		public static CoinUI ui;
		public static UserInterface userInterface;

		//Stuff from Main that's not public
		public static int UI_ScreenAnchorX;

		public static void DrawCoinsIndicator(SpriteBatch spriteBatch){
			userInterface?.Draw(spriteBatch, new GameTime());
		}
	}
}
