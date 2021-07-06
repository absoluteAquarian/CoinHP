using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace CoinHP.API.Detours{
	public static partial class TML{
		internal static void PlayerIO_LoadModData(Action<Player, IList<TagCompound>> orig, Player player, IList<TagCompound> list){
			orig(player, list);

			//Ensure that this flag is true, no matter what
			player.GetModPlayer<CoinPlayer>().waitingForWorldEnter = true;

			//Also, check if the player has CoinPlayer data.  If they do, and the "hotfix" value has been set, reset statLifeMax back to 100
			if(player.GetModPlayer<CoinPlayer>().checkHotfix)
				player.statLifeMax = 100;
		}
	}
}
