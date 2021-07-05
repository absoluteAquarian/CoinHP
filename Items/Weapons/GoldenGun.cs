using CoinHP.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Weapons{
	public class GoldenGun : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Golden Gun");
			Tooltip.SetDefault("'A cursed weapon that punishes those who are greedy'");
		}

		public override void SetDefaults(){
			item.useTurn = false;
			item.autoReuse = false;
			item.useTime = item.useAnimation = 38;
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

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			//Take away health from the player
			CoinPlayer mp = player.GetModPlayer<CoinPlayer>();

			long oldCoins = mp.coins;

			mp.UpdateHealth(player.statLife - 1);

			long newCoins = mp.coins;

			//Every 1 silver, 35 coppers is 2 damage
			int diff = (int)(oldCoins - newCoins);

			damage = Math.Max(1, diff / 135 * 2);

			if(damage < 20)
				knockBack = 1.6f;
			else if(damage < 60)
				knockBack = 3.3f;
			else if(damage < 150)
				knockBack = 6.4f;
			else if(damage < 300)
				knockBack = 9.2f;
			else
				knockBack = 11.5f;

			return true;
		}
	}
}
