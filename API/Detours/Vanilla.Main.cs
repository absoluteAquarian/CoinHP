using Terraria;

namespace CoinHP.API.Detours{
	public static partial class Vanilla{
#pragma warning disable IDE0060
		internal static void Main_GUIBarsMouseOverLife(Main self){
			//Literally do nothing
			//"orig" isn't needed since this detour does nothing
		}

		internal static void Main_DrawInterface_Resource_Life(){
			//Draw the special UI thing instead of the vanilla one
			CoinInterface.ui.Draw(Main.spriteBatch);
		}
	}
}
