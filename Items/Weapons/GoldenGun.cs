using CoinHP.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Weapons{
	public class GoldenGun : ModItem{
		public override bool CloneNewInstances => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Golden Gun");
			Tooltip.SetDefault("'A cursed weapon that punishes those who are greedy'");
		}

		public override void SetDefaults(){
			item.useTurn = false;
			item.autoReuse = false;
			item.useTime = item.useAnimation = 27;
			item.ranged = true;
			item.width = 42;
			item.height = 30;
			item.scale = 0.9f;
			item.shoot = ModContent.ProjectileType<GoldenGunShot>();
			item.UseSound = SoundID.Item41;
			item.rare = ItemRarityID.Orange;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.shootSpeed = 14f / 3;  //Projectile has 3 updates per tick
			item.value = Item.buyPrice(gold: 6, silver: 50);
			item.noMelee = true;

			item.damage = 23;
			item.knockBack = 3f;
			item.crit = 0;
		}

		public override void UpdateInventory(Player player){
			//Calculate what the damage/knockback would be, were the player to use the weapon
			CoinPlayer mp = player.GetModPlayer<CoinPlayer>();

			mp.coins = mp.GetCoinCount();

			byte prefix = item.prefix;
			item.SetDefaults(item.type);

			if(mp.coins <= 0){
				item.damage = 1;
				item.knockBack = 0.1f;
				return;
			}

			//Ensure that the player has the correct health values...
			player.statLife = CoinPlayer.ConvertCoinTotalToHealth(mp.coins);

			long oldCoins = mp.coins;

			CoinPlayer.DissectHealthToCoinCounts(player.statLife - 1, out int copper, out int silver, out int gold, out int platinum);
			long newCoins = CoinPlayer.CombineCounts(copper, silver, gold, platinum);

			GetStats(oldCoins, newCoins, ref item.damage, ref item.knockBack);

			//Make stat modifications carry over
			item.Prefix(prefix);
		}

		public override Vector2? HoldoutOffset()
			=> new Vector2(0, 2);

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.IllegalGunParts, 1);
			recipe.AddIngredient(ItemID.Handgun, 1);
			recipe.AddRecipeGroup(CoreMod.RecipeGroup_T4Bars, 20);
			recipe.AddIngredient(ItemID.GoldCoin, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

		private void GetStats(long oldCoins, long newCoins, ref int damage, ref float knockBack){
			int diff = (int)(oldCoins - newCoins);

			damage = Math.Max(1, GetDamageFromCoinsSpent(diff));

			knockBack = GetKnockbackFromDamage(damage);
		}

		//Every 2 silvers, 75 coppers is 1 damage
		protected virtual int GetDamageFromCoinsSpent(int coinDifference)
			=> coinDifference / 275;

		protected virtual float GetKnockbackFromDamage(int damage){
			if(damage < 5)
				return 0.1f + 1.5f * (damage / 5f);
			else if(damage < 20)
				return 1.6f + 1.7f * ((damage - 5) / 15f);
			else if(damage < 80)
				return 3.3f + 3.1f * ((damage - 20) / 60f);
			else if(damage < 150)
				return 6.4f + 5.1f * ((damage - 80) / 70f);
			return 11.5f;
		}

		public override bool CanUseItem(Player player)
			=> player.GetModPlayer<CoinPlayer>().coins > 0;

		private bool CanConsumeCoins(Player player){
			Item coin = new Item();
			coin.SetDefaults(ItemID.CopperCoin);

			bool dontConsume = false;
			if(player.ammoBox && Main.rand.Next(5) == 0)
				dontConsume = true;

			if(player.ammoPotion && Main.rand.Next(5) == 0)
				dontConsume = true;

			if(player.ammoCost80 && Main.rand.Next(5) == 0)
				dontConsume = true;

			if(player.ammoCost75 && Main.rand.Next(4) == 0)
				dontConsume = true;

			dontConsume |= !PlayerHooks.ConsumeAmmo(player, item, coin) | !ItemLoader.ConsumeAmmo(item, coin, player);

			return !dontConsume;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			//Take away health from the player
			CoinPlayer mp = player.GetModPlayer<CoinPlayer>();

			//Ensure that the player has the correct health values...
			player.statLife = CoinPlayer.ConvertCoinTotalToHealth(mp.coins);

			long oldCoins = mp.coins, newCoins;

			bool wasConsumed = false;
			if(CanConsumeCoins(player)){
				mp.UpdateHealth(player.statLife - 1);
				newCoins = mp.coins;
				wasConsumed = true;
			}else{
				CoinPlayer.DissectHealthToCoinCounts(player.statLife - 1, out int copper, out int silver, out int gold, out int platinum);
				newCoins = CoinPlayer.CombineCounts(copper, silver, gold, platinum);
			}

			GetStats(oldCoins, newCoins, ref damage, ref knockBack);

			if(!wasConsumed)
				damage = (int)(damage * 0.6f);
			
			return true;
		}
	}
}
