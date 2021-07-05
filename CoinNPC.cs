using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP{
	public class CoinNPC : GlobalNPC{
		public override void SetDefaults(NPC npc){
			//NPCs need to drop more money
			if(!npc.boss)
				npc.value *= 5.8f;
		}

		public override void GetChat(NPC npc, ref string chat){
			if(npc.type == NPCID.Nurse){
				if(CoinWorld.nurseGiftTimer <= 0)
					chat = !Main.bloodMoon
						? $"Hello, {Main.LocalPlayer.name}, your \"Extra Life\" item has been prepared!"
						: "Take your item and get lost.";
				else if(Main.rand.NextFloat() < 0.35f)
					chat = !Main.bloodMoon
						? $"Hello, {Main.LocalPlayer.name}, I still need some time to prepare the next \"Extra Life\" item you wanted."
						: "What do you want? I don't have your life freebie item yet.";
			}
		}

		public override bool PreNPCLoot(NPC npc){
			for(int i = 0; i < Main.maxPlayers; i++){
				Player player = Main.player[i];

				if(player.dead || !player.active)
					continue;

				if(npc.playerInteraction[i] && player.GetModPlayer<CoinPlayer>().wallet){
					//Drop more coins
					npc.value *= 1.25f;
					return true;
				}
			}

			return true;
		}
	}
}
