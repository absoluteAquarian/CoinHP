using CoinHP.Buffs;
using CoinHP.Projectiles;
using CoinHP.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.GameInput;
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
		private bool needCheckSavings;

		public bool wallet;
		public bool goldPig;
		public bool goldPigVisual;

		//If the player left the world before they could die
		private bool chicken;

		public Item[] savings = new Item[4];

		public override TagCompound Save(){
			chicken = player.statLifeMax2 <= 0 || player.statLife <= 0;

			return new TagCompound(){
				["lives"] = extraLives,
				["crystals"] = lifeCrystals,
				["fruit"] = lifeFruit,
				["savings"] = SaveSavings(),
				["chicken"] = chicken
			};
		}

		private TagCompound SaveSavings()
			=> new TagCompound(){
				["slot0"] = SaveSavingsSlot(0),
				["slot1"] = SaveSavingsSlot(1),
				["slot2"] = SaveSavingsSlot(2),
				["slot3"] = SaveSavingsSlot(3)
			};

		private TagCompound SaveSavingsSlot(int slot)
			=> new TagCompound(){
				["id"] = savings[slot].type,
				["stack"] = savings[slot].stack
			};

		public override void Initialize(){
			savings = new Item[4];

			for(int i = 0; i < 4; i++)
				savings[i] = new Item();
		}

		public override void Load(TagCompound tag){
			extraLives = tag.GetInt("lives");

			//Player inventory has already been loaded at this point.  Count the coins in the player's inventory
			coins = GetCoinCount();

			player.statLifeMax2 = player.statLife = ConvertCoinTotalToHealth(coins);

			lifeCrystals = tag.GetInt("crystals");
			lifeFruit = tag.GetInt("fruit");

			savings = LoadSavings(tag.GetCompound("savings")) ?? savings;

			chicken = tag.GetBool("chicken");

			waitingForWorldEnter = true;
		}

		internal bool playerWillDieImmediately;
		internal bool waitingForWorldEnter;

		public override void OnEnterWorld(Player player){
			CoreMod.Instance.savingsUI.InitializeSlots(player.GetModPlayer<CoinPlayer>());

			//Uh oh, looks like this player was created before using this mod.  Give them some starting coins
			//Only give them coins if they didn't try to bypass the "kill when no coins" system though
			if(!chicken && (player.statLifeMax2 <= 0 || player.statLife <= 0))
				playerWillDieImmediately = true;

			waitingForWorldEnter = false;
		}

		private Item[] LoadSavings(TagCompound tag){
			if(tag is null)
				return null;

			return new Item[]{
				LoadSavingsSlot(tag.GetCompound("slot0")),
				LoadSavingsSlot(tag.GetCompound("slot1")),
				LoadSavingsSlot(tag.GetCompound("slot2")),
				LoadSavingsSlot(tag.GetCompound("slot3"))
			};
		}

		private Item LoadSavingsSlot(TagCompound tag){
			Item item = new Item(){
				type = tag.GetInt("id"),
				stack = tag.GetInt("stack")
			};
			return item;
		}

		public override void ResetEffects(){
			wallet = false;
			goldPig = false;
			goldPigVisual = false;
		}

		public override bool ConsumeAmmo(Item weapon, Item ammo){
			if(weapon.type == ItemID.CoinGun){
				switch(ammo.type){
					case ItemID.CopperCoin:
						return Main.rand.NextFloat() >= 0.1f;
					case ItemID.SilverCoin:
						return Main.rand.NextFloat() >= 0.15f;
					case ItemID.GoldCoin:
						return Main.rand.NextFloat() >= 0.25f;
					case ItemID.PlatinumCoin:
						return Main.rand.NextFloat() >= 0.5f;
				}
			}

			return true;
		}

		public void UpdateHealth(int newHealth){
			int copper, silver, gold, platinum;
			if(newHealth <= AlgorithmFactor){
				//Coins -> Health conversion has become too coarse.  Just kill the player
				coins = 0;

				player.statLife = 0;
				player.statLifeMax2 = 1;

				copper = silver = gold = platinum = int.MaxValue;

				ModifyCoinCounts(ref copper, ref silver, ref gold, ref platinum);

				return;
			}

			int diff = newHealth - player.statLife;

			if(diff == 0)
				return;
			
			if(diff > 0){
				//Give coins
				SpawnCoinsFromHealthOffset(diff);
			}else{
				//Remove coins
				DissectHealthOffsetToCoinCounts(diff, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

				player.BuyItem((int)CombineCounts(diffCopper, diffSilver, diffGold, diffPlatinum));
			}

			DissectHealthToCoinCounts(newHealth, out copper, out silver, out gold, out platinum);

			coins = CombineCounts(copper, silver, gold, platinum);

			player.statLifeMax2 = player.statLife = newHealth;
		}

		//+2 base health per Life Crystal
		//+1 base health per Life Fruit
		public const int BaseHealth = 50;
		public int GetStartingHealth() => BaseHealth + lifeCrystals * 2 + lifeFruit;

		public override void PreUpdate(){
			copperLost = 0;
			silverLost = 0;
			goldLost = 0;
			platinumLost = 0;

			//Force "statLifeMax" to behave
			player.statLifeMax = GetStartingHealth();

			if(spawnCoinsOnRespawn || playerWillDieImmediately){
				spawnCoinsOnRespawn = false;

				int health = GetStartingHealth();

				DissectHealthToCoinCounts(health, out int copper, out int silver, out int gold, out int platinum);

				player.statLifeMax2 = player.statLife = health;

				coins = CombineCounts(copper, silver, gold, platinum);

				SpawnCoinsFromHealth(health);

				//Get the coins from the piggy bank
				needCheckSavings = true;
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet){
			if(Main.mouseRight && Main.mouseRightRelease){
				for(int i = 0; i < Main.maxProjectiles; i++){
					Projectile projectile = Main.projectile[i];

					if(!projectile.active || projectile.owner != player.whoAmI || !(projectile.modProjectile is SavingsPig))
						continue;

					//Give some wiggle room when clicking the pig
					Rectangle rect = projectile.Hitbox;
					rect.Inflate(8, 8);

					if(!rect.Contains(Main.MouseWorld.ToPoint()))
						continue;

					//Clicking the pig toggles the UI
					SavingsUI.Visible = !SavingsUI.Visible;

					//Oink
					Main.PlaySound(SoundID.Item59);
					break;
				}
			}
		}

		public override void PostUpdateEquips(){
			var type = ModContent.ProjectileType<SavingsPig>();

			if(goldPig && player.HasBuff(ModContent.BuffType<SavingsBuff>())){
				if(goldPigVisual && player.ownedProjectileCounts[type] == 0){
					//Oink
					Main.PlaySound(SoundID.Item59);

					Projectile.NewProjectile(player.Center, Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 4f, type, 0, 0, Owner: player.whoAmI);
				}
			}else if(player.ownedProjectileCounts[type] > 0){
				//Oink
				if(!goldPig)
					Main.PlaySound(SoundID.Item59);

				goldPig = false;

				for(int i = 0; i < Main.maxProjectiles; i++){
					Projectile projectile = Main.projectile[i];

					if(!projectile.active || projectile.type != type || projectile.owner != player.whoAmI)
						continue;

					projectile.Kill();
				}
			}

			if(needCheckSavings && goldPig){
				//Get the saved coins
				for(int i = 0; i < 4; i++){
					if(!savings[i].IsAir){
						player.QuickSpawnItem(savings[i].type, savings[i].stack);

						savings[i].TurnToAir();
					}
				}

				CoreMod.Instance.savingsUI.InitializeSlots(this);
			}

			needCheckSavings = false;
		}

		private bool forceDeath;

		public override void PostUpdate(){
			coins = GetCoinCount();

			player.statLife = ConvertCoinTotalToHealth(coins);

			int maxDelay = player.difficulty == 2 ? 600 : (player.difficulty == 1 ? 300 : 120);

			if(coins <= 0 && !player.dead && !extraLifeUsed){
				if(respawnDeathDelay == maxDelay)
					Main.NewText($"WARNING: You will die in {maxDelay / 60} seconds if you do not get any coins in your inventory!", Color.Red);
				else if(respawnDeathDelay <= 0){
					forceDeath = true;

					player.KillMe(PlayerDeathReason.ByCustomReason(CoinUtils.NoCoinsMessage(player)), 420, 0);

					forceDeath = false;
				}else{
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

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			if(wallet)
				damage = (int)(damage * 1.25f);

			return true;
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit){
			//More immune time
			player.immuneTime *= 2;

			HurtPlayer((int)damage);
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			//Game might try to kill the player before they're given their respawn coins
			if(waitingForWorldEnter || playerWillDieImmediately)
				return false;

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
				SpawnCoinsFromHealth(BaseHealth);

				return false;
			}

			//Remove any coins in the player's inventory and banks
			int copper, silver, gold, platinum;
			copper = silver = gold = platinum = int.MaxValue;
			ModifyCoinCounts(ref copper, ref silver, ref gold, ref platinum);

			coins = 0;

			return forceDeath;
		}

		public void HurtPlayer(int damage){
			if(damage <= 0)
				return;

			player.statLife += damage;

			DissectHealthOffsetToCoinCounts(-damage, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

			player.statLife -= damage;

			copperLost = diffCopper;
			silverLost = diffSilver;
			goldLost = diffGold;
			platinumLost = diffPlatinum;

			long count = CombineCounts(diffCopper, diffSilver, diffGold, diffPlatinum);
			do{
				player.BuyItem((int)(count % int.MaxValue));
				count %= int.MaxValue;
			}while(count > int.MaxValue);

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
				DissectHealthToCoinCounts(BaseHealth, out int copper, out int silver, out int gold, out int platinum);

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

			DissectHealthOffsetToCoinCounts(offset, out int diffCopper, out int diffSilver, out int diffGold, out int diffPlatinum);

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
			if(health <= AlgorithmFactor){
				copper = silver = gold = platinum = 0;
				return;
			}

			float d = health / AlgorithmFactor;
			long realDamage = (long)(d * d * d);

			SplitCoins(realDamage, out copper, out silver, out gold, out platinum);
		}

		public void DissectHealthOffsetToCoinCounts(int offset, out int copper, out int silver, out int gold, out int platinum){
			DissectHealthToCoinCounts(player.statLife, out int curCopper, out int curSilver, out int curGold, out int curPlatinum);
			DissectHealthToCoinCounts(player.statLife + offset, out int offCopper, out int offSilver, out int offGold, out int offPlatinum);

			long curCoins = CombineCounts(curCopper, curSilver, curGold, curPlatinum);
			long offCoins = CombineCounts(offCopper, offSilver, offGold, offPlatinum);

			SplitCoins(Math.Abs(curCoins - offCoins), out copper, out silver, out gold, out platinum);
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
