using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Accessories{
	public class Wallet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Gamer's Wallet");
			Tooltip.SetDefault("'It burns a hole in your pocket'");
		}

		public override void SetDefaults(){
			item.width = 24;
			item.height = 32;
			item.rare = ItemRarityID.Blue;
			item.accessory = true;
			item.value = Item.buyPrice(silver: 50, copper: 33);
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			player.GetModPlayer<CoinPlayer>().wallet = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(ItemID.SilverCoin, 80);
			recipe.AddRecipeGroup(CoreMod.RecipeGroup_EvilBars, 6);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
