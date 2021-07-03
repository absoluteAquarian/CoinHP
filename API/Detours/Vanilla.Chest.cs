using Terraria;
using Terraria.ID;

namespace CoinHP.API.Detours{
	public static partial class Vanilla{
		internal static void Chest_SetupShop(On.Terraria.Chest.orig_SetupShop orig, Chest self, int type){
			orig(self, type);

			//Increase the value of all shop items due to coins being easier to obtain
			foreach(var item in self.item)
				if(item.shopSpecialCurrency == CustomCurrencyID.None)
					item.value = (int)(item.value * 2.3f);
		}
	}
}
