using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP{
	public class ItemChecks : GlobalItem{
		public override bool UseItem(Item item, Player player){
			if(item.healLife > 0){
				//Give the player more coins based on the health healed
				int heal = item.healLife / 10;

				player.GetModPlayer<CoinPlayer>().UpdateHealth(player.statLife + heal);
			}
			return false;
		}

		public override bool OnPickup(Item item, Player player){
			if(item.type == ItemID.Heart || item.type == ItemID.CandyApple || item.type == ItemID.CandyCane){
				player.GetModPlayer<CoinPlayer>().UpdateHealth(player.statLife + 3);
				return false;
			}

			return true;
		}
	}
}
