using CoinHP.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Weapons{
	public class GoldenGatligator : GoldenGun{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Golden Gatligator");
			Tooltip.SetDefault("'A cursed weapon that punishes those who are greedy'" +
				"\n67% chance to not consume coins");
		}

		public override void SetDefaults(){
			base.SetDefaults();

			item.autoReuse = true;
			item.useTime = item.useAnimation = 6;
			item.width = 54;
			item.height = 22;
			item.rare = ItemRarityID.LightPurple;
			item.shootSpeed = 22f / 3;  //Projectile has 3 updates per tick
			item.value = Item.buyPrice(gold: 10, silver: 25);

			item.damage = 52;
			item.knockBack = 4.2f;
			item.crit = 0;
		}

		public override bool ConsumeAmmo(Player player)
			=> Main.rand.NextFloat() > 0.67f;

		//Every 3 silvers, 45 coppers is 3 damage
		protected override int GetDamageFromCoinsSpent(int coinDifference)
			=> coinDifference / 345 * 3;

		protected override float GetKnockbackFromDamage(int damage){
			if(damage < 20)
				return 0.1f + 1.5f * (damage / 20f);
			else if(damage < 65)
				return 1.6f + 1.7f * ((damage - 20) / 45f);
			else if(damage < 150)
				return 3.3f + 3.1f * ((damage - 65) / 85f);
			else if(damage < 250)
				return 6.4f + 5.1f * ((damage - 150) / 100f);
			return 11.5f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.IllegalGunParts, 2);
			recipe.AddIngredient(ItemID.Gatligator);
			recipe.AddIngredient(ItemID.GoldBar, 24);
			recipe.AddIngredient(ItemID.HallowedBar, 30);
			recipe.AddIngredient(ItemID.SoulofSight, 5);
			recipe.AddIngredient(ItemID.SoulofMight, 5);
			recipe.AddIngredient(ItemID.SoulofFright, 5);
			recipe.AddIngredient(ItemID.GoldCoin, 45);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
