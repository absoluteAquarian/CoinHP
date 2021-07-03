using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;

namespace CoinHP.API.MSIL{
	public static partial class Vanilla{
		internal static void UICharacterListItem_DrawSelf(ILContext il){
			FieldInfo UICharacterListItem__data = typeof(UICharacterListItem).GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo PlayerFileData_get_Player = typeof(PlayerFileData).GetMethod("get_Player", BindingFlags.Instance | BindingFlags.Public);

			ILCursor c = new ILCursor(il){
				Index = 0
			};

			//Inject the code at the very beginning of the method
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldfld, UICharacterListItem__data);
			c.Emit(OpCodes.Call, PlayerFileData_get_Player);
			c.EmitDelegate<Action<Player>>(player => {
				CoinPlayer mp = player.GetModPlayer<CoinPlayer>();
				
				mp.coins = mp.GetCoinCount();

				//Just updating "statLifeMax2" is good enough
				player.statLifeMax2 = CoinPlayer.ConvertCoinTotalToHealth(mp.coins);
			});
		}
	}
}
