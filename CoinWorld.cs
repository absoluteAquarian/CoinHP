using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CoinHP{
	public class CoinWorld : ModWorld{
		public static int nurseGiftTimer;

		public const int FullDay = 54000;
		public const int FullNight = 32400;
		public const int NurseGiftDelay = (FullDay + FullNight) * 3;

		public override void Load(TagCompound tag){
			//Tag doesn't exist?  default to the default value since this is a new world
			if(tag.ContainsKey("giftTimer"))
				nurseGiftTimer = tag.GetInt("giftTimer");
			else
				nurseGiftTimer = NurseGiftDelay;
		}

		public override TagCompound Save()
			=> new TagCompound(){
				["giftTimer"] = nurseGiftTimer
			};

		public override void PostUpdate(){
			nurseGiftTimer--;
		}
	}
}
