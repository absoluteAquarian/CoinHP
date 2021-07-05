using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.Items.Weapons{
	public class CoinMagnet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Coin Magnet");
			Tooltip.SetDefault("Steal the coins back from the monsters that stole them from you!" +
				"\nThis item can also pick up coins in the world.");

			Item.staff[item.type] = true;
		}

		public override void SetDefaults(){
			item.width = 44;
			item.height = 44;
			item.scale = 0.8f;
			item.magic = true;
			item.autoReuse = true;
			item.mana = 4;
			item.useTime = 8;
			item.useAnimation = 8;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.rare = ItemRarityID.Green;
			item.damage = 5;
			item.knockBack = 1.1f;
			item.value = Item.buyPrice(silver: 80, copper: 45);
			item.noMelee = true;
			//These two are needed for Shoot to do anything.  Oh well
			item.shoot = 10;
			item.shootSpeed = 1f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 20);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
			recipe.AddRecipeGroup(CoreMod.RecipeGroup_T4Bars, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}

		public override Vector2? HoldoutOrigin()
			=> new Vector2(9, 17);

		private int uses;

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			if(Main.myPlayer != player.whoAmI)
				return false;

			//Ignore literally everything regarding Shoot and just hurt the baddies near the mouse
			const float distance = 3 * 16;
			Rectangle mouseRect = new Rectangle((int)(Main.MouseWorld.X - distance), (int)(Main.MouseWorld.Y - distance), (int)(distance * 2), (int)(distance * 2));
			for(int i = 0; i < Main.maxNPCs; i++){
				NPC npc = Main.npc[i];

				if(!npc.active || npc.lifeMax <= 5 || npc.friendly || npc.immortal || npc.dontTakeDamage || !npc.Hitbox.Intersects(mouseRect))
					continue;

				bool crit = Main.rand.Next(1, 101) <= player.magicCrit;

				//Hurt the NPC
				if(uses % 3 == 0)
					npc.StrikeNPC(damage, knockBack, player.Center.X > npc.Center.X ? -1 : (player.Center.X < npc.Center.X ? 1 : 0), crit);

				uses++;

				if(Main.netMode == NetmodeID.MultiplayerClient)
					npc.PlayerInteraction(player.whoAmI);

				if(npc.extraValue < 1)
					continue;

				//Succ the coins
				int succed = Math.Min(2500, (int)npc.extraValue);

				npc.extraValue -= succed;

				if(succed % 100 != 0)
					player.QuickSpawnItem(ItemID.CopperCoin, succed % 100);
				if(succed / 100 != 0)
					player.QuickSpawnItem(ItemID.SilverCoin, succed / 100);

				if(Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.SyncExtraValue, ignoreClient: player.whoAmI, number: npc.whoAmI, number2: succed, number3: player.Center.X, number4: player.Center.Y);
			}

			//Pick up coins near the mouse
			for(int i = 0; i < Main.maxItems; i++){
				Item dropped = Main.item[i];

				if(!dropped.active || dropped.type < ItemID.CopperCoin || dropped.type > ItemID.PlatinumCoin || dropped.DistanceSQ(Main.MouseWorld) > distance * distance)
					continue;

				//Warp the coin to the player's center
				dropped.Center = player.Center;
			}

			return false;
		}
	}
}
