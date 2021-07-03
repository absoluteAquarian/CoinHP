using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CoinHP.Gores{
	public class CoinGore : ModGore{
		public override void OnSpawn(Gore gore){
			gore.rotation = gore.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override bool Update(Gore gore){
			gore.alpha += 5;

			if(gore.alpha > 255){
				gore.alpha = 255;
				gore.active = false;

				return false;
			}

			return true;
		}
	}
}
