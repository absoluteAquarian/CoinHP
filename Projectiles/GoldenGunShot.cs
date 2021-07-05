using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CoinHP.Projectiles{
	public class GoldenGunShot : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Curse of Midas");
		}

		public override void SetDefaults(){
			projectile.width = 4;
			projectile.height = 4;
			projectile.tileCollide = true;
			projectile.aiStyle = -1;
			projectile.extraUpdates = 2;
			projectile.ranged = true;
			projectile.timeLeft = 600;
			projectile.friendly = true;
			projectile.alpha = 255;
		}

		public override void AI(){
			if(projectile.alpha > 0)
				projectile.alpha -= 15;

			if(projectile.alpha < 0)
				projectile.alpha = 0;

			for(int i = 0; i < 10; i++){
				float x2 = projectile.Center.X - projectile.velocity.X / 10f * i;
				float y2 = projectile.Center.Y - projectile.velocity.Y / 10f * i;

				Dust dust = Dust.NewDustDirect(new Vector2(x2, y2), 1, 1, 64);
				dust.alpha = projectile.alpha;
				dust.position.X = x2;
				dust.position.Y = y2;
				dust.velocity *= 0f;
				dust.fadeIn = 0.2f;
				dust.noGravity = true;
			}
		}
	}
}
