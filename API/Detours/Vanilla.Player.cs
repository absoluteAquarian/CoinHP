using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CoinHP.API.Detours{
	public static partial class Vanilla{
#pragma warning disable IDE0060
		internal static void Player_UpdateLifeRegen(On.Terraria.Player.orig_UpdateLifeRegen orig, Player self){
			int pre = self.statLife;

			InnerUpdateLifeRegen(self);

			int post = self.statLife;

			CoinPlayer mp = self.GetModPlayer<CoinPlayer>();

			//Guaranteed to be a positive integer since positive health regen is disabled
			mp.HurtPlayer(pre - post);
		}

		internal static void InnerUpdateLifeRegen(Player self){
			//Only let negative regen happen here
			if(self.poisoned){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 4;
			}

			if(self.venom){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 12;
			}

			if(self.onFire){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 8;
			}

			if(self.onFrostBurn){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 12;
			}

			if(self.onFire2){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 12;
			}

			if(self.burned){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 60;
				self.moveSpeed *= 0.5f;
			}

			if(self.suffocating){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 40;
			}

			if(self.electrified){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 8;
				if(self.controlLeft || self.controlRight)
					self.lifeRegen -= 32;
			}

			if(self.tongued && Main.expertMode){
				if(self.lifeRegen > 0)
					self.lifeRegen = 0;

				self.lifeRegenTime = 0;
				self.lifeRegen -= 100;
			}

			PlayerHooks.UpdateBadLifeRegen(self);

			//Apply the DoT debuffs
			self.lifeRegenCount += self.lifeRegen;

			if(self.burned || self.suffocating || (self.tongued && Main.expertMode)){
				while(self.lifeRegenCount <= -600){
					self.lifeRegenCount += 600;

					self.statLife -= 5;

					CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegen, 5, dramatic: false, dot: true);

					if(self.statLife <= 0 && self.whoAmI == Main.myPlayer){
						if(self.suffocating)
							self.KillMe(PlayerDeathReason.ByOther(7), 10.0, 0);
						else
							self.KillMe(PlayerDeathReason.ByOther(8), 10.0, 0);
					}
				}

				return;
			}

			while(self.lifeRegenCount <= -120){
				if(self.lifeRegenCount <= -480){
					self.lifeRegenCount += 480;
					self.statLife -= 4;
					CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegen, 4, dramatic: false, dot: true);
				}else if(self.lifeRegenCount <= -360){
					self.lifeRegenCount += 360;
					self.statLife -= 3;
					CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegen, 3, dramatic: false, dot: true);
				}else if(self.lifeRegenCount <= -240){
					self.lifeRegenCount += 240;
					self.statLife -= 2;
					CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegen, 2, dramatic: false, dot: true);
				}else{
					self.lifeRegenCount += 120;
					self.statLife--;
					CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegen, 1, dramatic: false, dot: true);
				}

				if(self.statLife <= 0 && self.whoAmI == Main.myPlayer){
					if(self.poisoned || self.venom)
						self.KillMe(PlayerDeathReason.ByOther(9), 10.0, 0);
					else if(self.electrified)
						self.KillMe(PlayerDeathReason.ByOther(10), 10.0, 0);
					else
						self.KillMe(PlayerDeathReason.ByOther(8), 10.0, 0);
				}
			}
		}

		internal static void Player_QuickHeal(On.Terraria.Player.orig_QuickHeal orig, Player self){
			//Ignore the "self.statLife == self.statLifeMax2" check
			if(self.noItems || self.potionDelay > 0)
				return;

			Item item = self.QuickHeal_GetItemToUse();
			if(item == null)
				return;

			Main.PlaySound(item.UseSound, self.position);
			if(item.potion){
				if(item.type == 227){
					self.potionDelay = self.restorationDelayTime;
					self.AddBuff(21, self.potionDelay);
				}else{
					self.potionDelay = self.potionDelayTime;
					self.AddBuff(21, self.potionDelay);
				}
			}

			ItemLoader.UseItem(item, self);
			int healLife = self.GetHealLife(item, true);
			int healMana = self.GetHealMana(item, true);
			//Ignore code that affects "statLife" directly
			self.statMana += healMana;

			if(self.statMana > self.statManaMax2)
				self.statMana = self.statManaMax2;

			if(healLife > 0 && Main.myPlayer == self.whoAmI)
				self.HealEffect(healLife, true);

			if(healMana > 0){
				self.AddBuff(94, Player.manaSickTime);
				if(Main.myPlayer == self.whoAmI)
					self.ManaEffect(healMana);
			}

			if(ItemLoader.ConsumeItem(item, self))
				item.stack--;

			if(item.stack <= 0)
				item.TurnToAir();

			Recipe.FindRecipes();
		}

		internal static void Player_QuickMana(On.Terraria.Player.orig_QuickMana orig, Player self){
			if(self.noItems || self.statMana == self.statManaMax2)
				return;

			int num = 0;
			while(true){
				if(num < 58){
					if(self.inventory[num].stack > 0 && self.inventory[num].type > 0 && self.inventory[num].healMana > 0 && (self.potionDelay == 0 || !self.inventory[num].potion) && ItemLoader.CanUseItem(self.inventory[num], self))
						break;

					num++;
					continue;
				}

				return;
			}

			Main.PlaySound(self.inventory[num].UseSound, self.position);
			if(self.inventory[num].potion){
				if(self.inventory[num].type == 227){
					self.potionDelay = self.restorationDelayTime;
					self.AddBuff(21, self.potionDelay);
				}else{
					self.potionDelay = self.potionDelayTime;
					self.AddBuff(21, self.potionDelay);
				}
			}

			ItemLoader.UseItem(self.inventory[num], self);
			int healLife = self.GetHealLife(self.inventory[num], true);
			int healMana = self.GetHealMana(self.inventory[num], true);
			//Ignore code that affects "statLife" directly
			//self.statLife += healLife;
			self.statMana += healMana;

			if(self.statMana > self.statManaMax2)
				self.statMana = self.statManaMax2;

			if(healLife > 0 && Main.myPlayer == self.whoAmI)
				self.HealEffect(healLife, true);

			if(healMana > 0){
				self.AddBuff(94, Player.manaSickTime);
				if(Main.myPlayer == self.whoAmI)
					self.ManaEffect(healMana);
			}

			if(ItemLoader.ConsumeItem(self.inventory[num], self))
				self.inventory[num].stack--;

			if(self.inventory[num].stack <= 0)
				self.inventory[num].TurnToAir();

			Recipe.FindRecipes();
		}
	}
}
