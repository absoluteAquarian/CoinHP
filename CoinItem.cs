using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP{
	public class CoinItem : GlobalItem{
		public override void GrabRange(Item item, Player player, ref int grabRange){
			//Weak "Gold Ring" effect, but only when the Gold Ring (or its upgrades) aren't equipped
			if(!player.goldRing && (item.type == ItemID.CopperCoin || item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin))
				grabRange += 28;
		}
	}
}
