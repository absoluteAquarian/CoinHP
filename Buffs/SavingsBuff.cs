using Terraria;
using Terraria.ModLoader;

namespace CoinHP.Buffs{
	public class SavingsBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Emergency Savings");
			Description.SetDefault("Start your next life with a bit of extra cash");

			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.buffTime[buffIndex] = 18000;
		}
	}
}
