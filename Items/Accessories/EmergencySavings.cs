using CoinHP.Buffs;
using CoinHP.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Accessories{
	public class EmergencySavings : ModItem{
		public override string Texture => typeof(SavingsPig).FullName.Replace('.', '/');

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Emergency Savings");
			Tooltip.SetDefault("Spawns a flying, golden piggy bank while equipped" +
				"\nRight click the golden piggy bank to pull up a menu where you can store coins" +
				"\nThese coins do not count towards your health and will be given to you when you respawn");

			Main.RegisterItemAnimation(item.type, new EmergencySavingsAnimation());
		}

		public override void SetDefaults(){
			item.width = 32;
			item.height = 32;
			item.scale = 0.8f;
			item.rare = ItemRarityID.Orange;
			item.value = Item.buyPrice(gold: 5, silver: 45);
			item.accessory = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.PiggyBank, 1);
			recipe.AddIngredient(ItemID.MoneyTrough, 1);
			recipe.AddRecipeGroup(CoreMod.RecipeGroup_T4Bars, 16);
			recipe.AddIngredient(ItemID.Cloud, 25);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			player.AddBuff(ModContent.BuffType<SavingsBuff>(), 60);
			player.GetModPlayer<CoinPlayer>().goldPig = true;
			player.GetModPlayer<CoinPlayer>().goldPigVisual = !hideVisual;
		}
	}

	internal class EmergencySavingsAnimation : DrawAnimationVertical{
		public EmergencySavingsAnimation() : base(ticksperframe: 4, frameCount: 8){ }

		public override Rectangle GetFrame(Texture2D texture){
			int src;

			switch(Frame){
				case 0:
					src = 0;
					break;
				case 1:
				case 7:
					src = 1;
					break;
				case 2:
				case 6:
					src = 2;
					break;
				case 3:
				case 5:
					src = 3;
					break;
				case 4:
					src = 4;
					break;
				default:
					src = 0;
					break;
			}

			return texture.Frame(1, 5, 0, src);
		}
	}
}
