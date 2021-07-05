using CoinHP.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Projectiles{
	public class SavingsPig : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Golden Piggy Bank");

			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.FlyingPiggyBank);
			aiType = ProjectileID.FlyingPiggyBank;
		}

		public override void AI(){
			Player owner = Main.player[projectile.owner];
			if(owner.dead || !owner.active || !owner.GetModPlayer<CoinPlayer>().goldPig || !owner.GetModPlayer<CoinPlayer>().goldPigVisual){
				//Oink
				Main.PlaySound(SoundID.Item59);

				projectile.Kill();
				return;
			}

			//Prevent this piggy bank from being on top of the normal one
			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile other = Main.projectile[i];

				if(i == projectile.whoAmI || !other.active || !(other.type == ProjectileID.FlyingPiggyBank || other.modProjectile is SavingsPig) || other.owner != projectile.owner || !projectile.Hitbox.Intersects(other.Hitbox))
					continue;

				Vector2 dir = (other.Center - projectile.Center).SafeNormalize(Vector2.UnitY);

				projectile.velocity += -dir * 0.8f;
				other.velocity += dir * 0.8f;
			}
		}
	}
}
