using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CoinHP{
	public class CoinPlayer : ModPlayer{
		public long coins;

		private int respawnDeathDelay;

		private bool extraLifeUsed;

		public int copperLost;
		public int silverLost;
		public int goldLost;
		public int platinumLost;

		public int extraLives;
		public int lifeCrystals;
		public int lifeFruit;

		private bool spawnCoinsOnRespawn;

		public override TagCompound Save()
			=> new TagCompound(){
				["lives"] = extraLives,
				["crystals"] = lifeCrystals,
				["fruit"] = lifeFruit
			};

		public override void Load(TagCompound tag){
			extraLives = tag.GetInt("lives");

			//Player inventory has already been loaded at this point.  Count the coins in the player's inventory
			coins = GetCoinCount();

			player.statLifeMax2 = player.statLife = ConvertCoinTotalToHealth(coins);

			lifeCrystals = tag.GetInt("crystals");
			lifeFruit = tag.GetInt("fruit");
		}

		public void UpdateHealth(int newHealth){
			int diff = newHealth - player.statLife;

			if(diff == 0)
				return;

			if(diff > 0){
				//Give coins
				SpawnCoinsFromHealthOffset(diff);
			}else{
				//Remove coins
				DissectHealthToCoinCounts(-diff, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

				player.BuyItem((int)CombineCounts(diffCopper, diffSilver, diffGold, diffPlatinum));
			}

			DissectHealthToCoinCounts(newHealth, out int copper, out int silver, out int gold, out int platinum);

			coins = CombineCounts(copper, silver, gold, platinum);

			player.statLifeMax2 = player.statLife = newHealth;
		}

		//+2 base health per Life Crystal
		//+0.5 base health per Life Fruit
		public int GetStartingHealth() => 50 + lifeCrystals * 2 + lifeFruit;

		public override void PreUpdate(){
			copperLost = 0;
			silverLost = 0;
			goldLost = 0;
			platinumLost = 0;

			//Force "statLifeMax" to behave
			player.statLifeMax = GetStartingHealth();

			if(spawnCoinsOnRespawn){
				spawnCoinsOnRespawn = false;

				int health = GetStartingHealth();

				DissectHealthToCoinCounts(health, out int copper, out int silver, out int gold, out int platinum);

				player.statLifeMax2 = player.statLife = health;

				coins = CombineCounts(copper, silver, gold, platinum);

				SpawnCoinsFromHealth(health);
			}
		}

		public override void PostUpdate(){
			coins = GetCoinCount();

			player.statLife = ConvertCoinTotalToHealth(coins);

			int maxDelay = player.difficulty == 2 ? 600 : (player.difficulty == 1 ? 300 : 120);

			if(coins <= 0 && !player.dead && !extraLifeUsed){
				if(respawnDeathDelay == maxDelay)
					Main.NewText($"WARNING: You will die in {maxDelay / 60} seconds if you do not get any coins in your inventory!", Color.Red);
				else if(respawnDeathDelay <= 0)
					player.KillMe(PlayerDeathReason.ByCustomReason(CoinUtils.NoCoinsMessage(player)), 420, 0);
				else{
					player.immuneNoBlink = false;
					player.immune = true;
					player.immuneTime = 2;
				}

				respawnDeathDelay--;
			}else
				respawnDeathDelay = maxDelay;

			extraLifeUsed = false;

			//Limit each gore to 30
			if(copperLost > 30)
				copperLost = 30;
			if(silverLost > 30)
				silverLost = 30;
			if(goldLost > 30)
				goldLost = 30;
			if(platinumLost > 30)
				platinumLost = 30;

			//Spawn some coin effects if the player has lost some coins
			if((copperLost | silverLost | goldLost | platinumLost) != 0){
				for(int c = 0; c < copperLost; c++)
					SpawnGore("Copper");
				for(int s = 0; s < silverLost; s++)
					SpawnGore("Silver");
				for(int g = 0; g < goldLost; g++)
					SpawnGore("Gold");
				for(int p = 0; p < platinumLost; p++)
					SpawnGore("Platinum");
			}

			copperLost = 0;
			silverLost = 0;
			goldLost = 0;
			platinumLost = 0;
		}

		public static void ItemCheck_LifeCrystals(Player player, Item item){
			CoinPlayer mp = player.GetModPlayer<CoinPlayer>();

			if(item.type == ItemID.LifeCrystal && player.itemAnimation > 0 && mp.lifeCrystals < 15 && player.itemTime == 0){
				player.itemTime = PlayerHooks.TotalUseTime(item.useTime, player, item);
				player.statLifeMax += 2;
				player.statLifeMax2 += 2;
				player.statLife += 2;

				mp.lifeCrystals++;

				if(Main.myPlayer == player.whoAmI){
					player.HealEffect(2);

					mp.SpawnCoinsFromHealthOffset(2);
				}

				AchievementsHelper.HandleSpecialEvent(player, 0);
			}
		}

		public static void ItemCheck_LifeFruit(Player player, Item item){
			CoinPlayer mp = player.GetModPlayer<CoinPlayer>();

			if(item.type == ItemID.LifeFruit && player.itemAnimation > 0 && mp.lifeFruit < 20 && player.itemTime == 0){
				player.itemTime = PlayerHooks.TotalUseTime(item.useTime, player, item);
				player.statLifeMax++;
				player.statLifeMax2++;
				player.statLife++;

				mp.lifeFruit++;

				if(Main.myPlayer == player.whoAmI){
					player.HealEffect(1);

					mp.SpawnCoinsFromHealthOffset(1);
				}

				AchievementsHelper.HandleSpecialEvent(player, 2);
			}
		}

		public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText){
			health = 0;
			// TODO: add an item or NPC to clear debuffs with since the Nurse is being repurposed
			removeDebuffs = false;

			if(CoinWorld.nurseGiftTimer > 0){
				chatText = "Come back in a few days to get another \"Extra life\".";
				return false;
			}

			return true;
		}

		public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price){
			//Price needs to be at least 1 copper to actually let the nurse "heal" the player
			price = Item.buyPrice(gold: 1, silver: 50);
		}

		public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price){
			extraLives++;

			CoinWorld.nurseGiftTimer = CoinWorld.NurseGiftDelay;

			if(CoinWorld.nurseGiftTimer > 0)
				Main.npcChatText = "Come back in a few days to get another \"Extra Life\".";
		}

		private void SpawnGore(string name)
			=> Gore.NewGore(player.Center + new Vector2(0, 12),
				new Vector2(Main.rand.NextFloat(-1.7f, 1.7f), Main.rand.NextFloat(-4, -1.5f)),
				mod.GetGoreSlot($"Gores/Coin_{name}"));

		public override void UpdateBadLifeRegen(){
			//No natural regen
			if(player.lifeRegen > 0)
				player.lifeRegen = 0;
		}

		public override void NaturalLifeRegen(ref float regen){
			//No regen
			regen = 0f;
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit){
			//More immune time
			player.immuneTime *= 2;

			HurtPlayer((int)damage);
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			//Try to take away an extra life if the player has one
			if(extraLives > 0){
				extraLives--;

				extraLifeUsed = true;

				player.immune = true;
				player.immuneTime = 150;
				player.immuneNoBlink = false;

				Rectangle r = new Rectangle((int)player.position.X, (int)player.position.Y - 3 * 16, 0, 0);
				CombatText.NewText(r, CombatText.LifeRegenNegative, "-1 EXTRA LIFE", dramatic: true);

				//Give the player some coins so that they don't die immediately
				SpawnCoinsFromHealth(50);

				return false;
			}

			//Remove any coins in the player's inventory and banks
			int copper, silver, gold, platinum;
			copper = silver = gold = platinum = int.MaxValue;
			ModifyCoinCounts(ref copper, ref silver, ref gold, ref platinum);

			coins = 0;

			return false;
		}

		public void HurtPlayer(int damage){
			if(damage <= 0)
				return;

			DissectHealthToCoinCounts(player.statLife + damage, out int curCopper, out int curSilver, out int curGold, out int curPlatinum);
			DissectHealthToCoinCounts(player.statLife, out int newCopper, out int newSilver, out int newGold, out int newPlatinum);

			long curCoins = CombineCounts(curCopper, curSilver, curGold, curPlatinum);
			long newCoins = CombineCounts(newCopper, newSilver, newGold, newPlatinum);

			long diff = curCoins - newCoins;

			SplitCoins(diff, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

			copperLost = diffCopper;
			silverLost = diffSilver;
			goldLost = diffGold;
			platinumLost = diffPlatinum;

			player.BuyItem((int)diff);

			coins = GetCoinCount();

			player.statLife = ConvertCoinTotalToHealth(coins);

			player.statLifeMax2 = Math.Max(1, player.statLife);
		}

		public override void OnRespawn(Player player){
			respawnDeathDelay = 120;
			spawnCoinsOnRespawn = true;
		}

		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath){
			//Mediumcore would just use the normal respawn code for giving the player coins
			if(!mediumcoreDeath){
				DissectHealthToCoinCounts(50, out int copper, out int silver, out int gold, out int platinum);

				Item item;
				if(copper > 0){
					item = new Item();
					item.SetDefaults(ItemID.CopperCoin);
					item.stack = copper;

					items.Add(item);
				}
				if(silver > 0){
					item = new Item();
					item.SetDefaults(ItemID.SilverCoin);
					item.stack = silver;

					items.Add(item);
				}
				if(gold > 0){
					item = new Item();
					item.SetDefaults(ItemID.GoldCoin);
					item.stack = gold;

					items.Add(item);
				}
				if(platinum > 0){
					item = new Item();
					item.SetDefaults(ItemID.PlatinumCoin);
					item.stack = platinum;

					items.Add(item);
				}
			}
		}

		public void SpawnCoinsFromHealth(int health){
			DissectHealthToCoinCounts(health, out int copper, out int silver, out int gold, out int platinum);

			if(copper > 0)
				player.QuickSpawnItem(ItemID.CopperCoin, copper);
			if(silver > 0)
				player.QuickSpawnItem(ItemID.SilverCoin, silver);
			if(gold > 0)
				player.QuickSpawnItem(ItemID.GoldCoin, gold);
			if(platinum > 0)
				player.QuickSpawnItem(ItemID.PlatinumCoin, platinum);
		}

		public void SpawnCoinsFromHealthOffset(int offset){
			if(offset <= 0)
				return;

			DissectHealthToCoinCounts(player.statLife, out int copper, out int silver, out int gold, out int platinum);
			DissectHealthToCoinCounts(player.statLife + offset, out int newCopper, out int newSilver, out int newGold, out int newPlatinum);

			long total = CombineCounts(copper, silver, gold, platinum);
			long newTotal = CombineCounts(newCopper, newSilver, newGold, newPlatinum);

			SplitCoins(newTotal - total, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

			if(diffCopper > 0)
				player.QuickSpawnItem(ItemID.CopperCoin, diffCopper);
			if(diffSilver > 0)
				player.QuickSpawnItem(ItemID.SilverCoin, diffSilver);
			if(diffGold > 0)
				player.QuickSpawnItem(ItemID.GoldCoin, diffGold);
			if(diffPlatinum > 0)
				player.QuickSpawnItem(ItemID.PlatinumCoin, diffPlatinum);
		}

		public long GetCoinCount()
			=> CountCoinsInInventory(player.inventory)
				+ CountCoinsInInventory(player.bank.item)
				+ CountCoinsInInventory(player.bank2.item)
				+ CountCoinsInInventory(player.bank3.item);

		public void ModifyCoinCounts(ref int copper, ref int silver, ref int gold, ref int platinum){
			ModifyCoinCountInInventory(player.inventory, ref copper, ref silver, ref gold, ref platinum);
			ModifyCoinCountInInventory(player.bank.item, ref copper, ref silver, ref gold, ref platinum);
			ModifyCoinCountInInventory(player.bank2.item, ref copper, ref silver, ref gold, ref platinum);
			ModifyCoinCountInInventory(player.bank3.item, ref copper, ref silver, ref gold, ref platinum);
		}

		private long CountCoinsInInventory(Item[] inventory){
			long coins = 0;

			if(inventory is null)
				return coins;

			for(int i = 0; i < inventory.Length; i++){
				Item item = inventory[i];

				switch(item.type){
					case ItemID.CopperCoin:
						coins += item.stack;
						break;
					case ItemID.SilverCoin:
						coins += item.stack * 100;
						break;
					case ItemID.GoldCoin:
						coins += item.stack * 10000;
						break;
					case ItemID.PlatinumCoin:
						coins += item.stack * 1000000;
						break;
				}
			}

			return coins;
		}

		private void ModifyCoinCountInInventory(Item[] inventory, ref int copperLoss, ref int silverLoss, ref int goldLoss, ref int platinumLoss){
			if(inventory is null)
				return;

			for(int i = 0; i < inventory.Length; i++){
				Item item = inventory[i];

				switch(item.type){
					case ItemID.CopperCoin:
						HandleLoss(ref copperLoss, item);
						break;
					case ItemID.SilverCoin:
						HandleLoss(ref silverLoss, item);
						break;
					case ItemID.GoldCoin:
						HandleLoss(ref goldLoss, item);
						break;
					case ItemID.PlatinumCoin:
						HandleLoss(ref platinumLoss, item);
						break;
				}
			}
		}

		private void HandleLoss(ref int change, Item item, bool isActuallyLoss = true){
			if(change <= 0)
				return;

			if(isActuallyLoss){
				if(change >= item.stack){
					change -= item.stack;
					item.TurnToAir();
				}else{
					item.stack -= change;
					change = 0;
				}
			}else{
				if(item.stack + change >= item.maxStack){
					change -= item.maxStack - item.stack;
					item.stack = item.maxStack;
				}else{
					item.stack += change;
					change = 0;
				}
			}
		}

		public const float AlgorithmFactor = 6.1f;
	
		public static void DissectHealthToCoinCounts(int health, out int copper, out int silver, out int gold, out int platinum){
			if(health <= 0){
				copper = silver = gold = platinum = 0;
				return;
			}

			float d = health / AlgorithmFactor;
			long realDamage = Math.Max(1, (long)Math.Ceiling(d * d * d));

			SplitCoins(realDamage, out copper, out silver, out gold, out platinum);
		}
	
		public static int ConvertCoinsToHealth(int copper = 0, int silver = 0, int gold = 0, int platinum = 0){
			//Reverse of DissectDamageToCoinLoss
			double real = platinum;
			real *= 100;
			real += gold;
			real *= 100;
			real += silver;
			real *= 100;
			real += copper;
		
			real = CoinUtils.Cbrt(real);
			real *= AlgorithmFactor;
		
			return (int)real;
		}

		public static int ConvertCoinTotalToHealth(long total){
			var stacks = Utils.CoinsSplit(total);
			return ConvertCoinsToHealth(stacks[0], stacks[1], stacks[2], stacks[3]);
		}

		public static long CombineCounts(int copper, int silver, int gold, int platinum)
			=> copper + silver * 100 + gold * 10000 + platinum * 1000000;

		public static void SplitCoins(long coins, out int copper, out int silver, out int gold, out int platinum){
			copper = (int)(coins % 100);
			coins /= 100;
			silver = (int)(coins % 100);
			coins /= 100;
			gold = (int)(coins % 100);
			coins /= 100;
			platinum = (int)coins;
		}
	}
}
