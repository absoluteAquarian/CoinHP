using CoinHP.API;
using CoinHP.Gores;
using CoinHP.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace CoinHP{
	public class CoreMod : Mod{
		public static CoreMod Instance => ModContent.GetInstance<CoreMod>();

		public const string RecipeGroup_EvilBars = "CoinHP: Evil Bars";

		public const string RecipeGroup_T4Bars = "CoinHP: Gold or Platinum Bars";

		internal SavingsUI savingsUI;
		private UserInterface savingsInterface;

		public override void Load(){
			if(!Main.dedServ){
				CoinInterface.ui = new CoinUI();
				CoinInterface.ui.Activate();

				CoinInterface.userInterface = new UserInterface();
				CoinInterface.userInterface.SetState(CoinInterface.ui);

				savingsUI = new SavingsUI();
				savingsUI.Activate();

				savingsInterface = new UserInterface();
				savingsInterface.SetState(savingsUI);
			}

			AddGore("CoinHP/Gores/Coin_Copper", new CoinGore());
			AddGore("CoinHP/Gores/Coin_Silver", new CoinGore());
			AddGore("CoinHP/Gores/Coin_Gold", new CoinGore());
			AddGore("CoinHP/Gores/Coin_Platinum", new CoinGore());

			try{
				API.EditsLoader.Load();
			}catch(Exception ex){
				API.EditsLoader.Unload();
				throw new Exception("An error occurred while patching the game in CoinHP", ex);
			}

			API.DirectDetours.Manager.Load();

			Main.OnPostDraw += gameTime => {
				if(Main.gameMenu || Main.netMode == NetmodeID.Server)
					return;

				//Reset the "was going to die" flags in the players
				for(int i = 0; i < Main.maxPlayers; i++){
					Player player = Main.player[i];

					if(player.active && (Main.netMode == NetmodeID.SinglePlayer || Netplay.Clients[i].State == 10))
						player.GetModPlayer<CoinPlayer>().playerWillDieImmediately = false;
				}
			};
		}

		public override void AddRecipeGroups(){
			RegisterRecipeGroup(RecipeGroup_EvilBars, ItemID.DemoniteBar, new int[]{ ItemID.DemoniteBar, ItemID.CrimtaneBar });

			RegisterRecipeGroup(RecipeGroup_T4Bars, ItemID.GoldBar, new int[]{ ItemID.GoldBar, ItemID.PlatinumBar });
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));

		public override void Unload(){
			API.DirectDetours.Manager.Unload();

			CoinInterface.ui = null;
			CoinInterface.userInterface = null;
			savingsUI = null;
			savingsInterface = null;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers){
			//We're overwriting the health bar drawing
			//Just to be safe, we should remove the vanilla one AND any ones between it and the next vanilla layer
			//This should prevent any weirdness from happening...
			int barsIndex = layers.FindIndex(gil => gil.Name == "Vanilla: Resource Bars");
			if(barsIndex == -1)
				return;

			//Insert the custom layer
			layers.Insert(barsIndex + 1, new LegacyGameInterfaceLayer("CoinHP: ResourceBars", () => {
					VanillaGUI.DrawResourcesNoHearts();
					return true;
				},
				InterfaceScaleType.UI));

			//Then remove the rest
			layers.RemoveAt(barsIndex);
			int layerIdx = layers.FindIndex(gil => gil.Name == "CoinHP: ResourceBars") + 1;

			while(layers[layerIdx].Name != "Vanilla: Interface Logic 3"){
				layers.RemoveAt(layerIdx);
				layerIdx++;
			}

			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

			if(mouseTextIndex != -1){
				layers.Insert(mouseTextIndex - 1, new LegacyGameInterfaceLayer("CoinHP: Savings UI", () => {
						if(SavingsUI.Visible)
							savingsInterface.Draw(Main.spriteBatch, new GameTime());

						return true;
					}, InterfaceScaleType.UI));
			}
		}

		public override void UpdateUI(GameTime gameTime){
			if(SavingsUI.Visible)
				savingsInterface?.Update(gameTime);
		}

		public override object Call(params object[] args){
			if(args.Length < 1)
				return null;

			if(!(args[0] is string method))
				throw new Exception("Invalid Mod.Call() requested");

			switch(method){
				case "Get Nurse Gift Timer":
					if(args.Length != 1)
						throw new Exception($"Too many arguments specified for call \"{method}\"");

					return CoinWorld.nurseGiftTimer;
				case "Set Nurse Gift Timer":
					if(args.Length < 2)
						throw new Exception($"Too few arguments specified for call \"{method}\"");
					if(args.Length > 2)
						throw new Exception($"Too many arguments specified for call \"{method}\"");

					if(!(args[1] is int timer))
						throw new Exception($"Mod.Call(\"{method}\", int) was requested with an invalid format");

					CoinWorld.nurseGiftTimer = timer;
					return true;
				case "Get Player Field":
					if(args.Length < 3)
						throw new Exception($"Too few arguments specified for call \"{method}\"");
					if(args.Length > 3)
						throw new Exception($"Too many arguments specified for call \"{method}\"");

					if(!(args[1] is int playerID) || !(args[2] is string playerField))
						throw new Exception($"Mod.Call(\"{method}\", int, string) was requested with an invalid format");

					if(Main.netMode == NetmodeID.SinglePlayer && playerID > 0)
						throw new Exception($"Mod.Call(\"{method}\", int, string) can only be used with player ID 0 in singleplayer instances. Requested: {playerID}");

					if(Main.netMode == NetmodeID.Server)
						throw new Exception($"Mod.Call(\"{method}\", int, string) cannot be used in server instances");

					CoinPlayer mp = Main.player[playerID].GetModPlayer<CoinPlayer>();

					switch(playerField){
						case nameof(CoinPlayer.coins):
							return mp.coins;
						case nameof(CoinPlayer.extraLives):
							return mp.extraLives;
						case nameof(CoinPlayer.lifeCrystals):
							return mp.lifeCrystals;
						case nameof(CoinPlayer.lifeFruit):
							return mp.lifeFruit;
						default:
							throw new Exception($"Mod.Call(\"{method}\", int, string) was provided an invalid player field name: \"{playerField}\"");
					}
				case "Set Player Field":
					if(args.Length < 4)
						throw new Exception($"Too few arguments specified for call \"{method}\"");
					if(args.Length > 4)
						throw new Exception($"Too many arguments specified for call \"{method}\"");

					if(!(args[1] is int playerID2) || !(args[2] is string playerField2))
						throw new Exception($"Mod.Call(\"{method}\", int, string, object) was requested with an invalid format");

					if(Main.netMode == NetmodeID.SinglePlayer && playerID2 > 0)
						throw new Exception($"Mod.Call(\"{method}\", int, string, object) can only be used with player ID 0 in singleplayer instances. Requested: {playerID2}");

					if(Main.netMode == NetmodeID.Server)
						throw new Exception($"Mod.Call(\"{method}\", int, string, object) cannot be used in server instances");

					CoinPlayer mp2 = Main.player[playerID2].GetModPlayer<CoinPlayer>();

					switch(playerField2){
						case nameof(CoinPlayer.coins):
							if(args[3] is long coinArg)
								mp2.coins = coinArg;
							if(args[3] is int coinArgInt)
								mp2.coins = coinArgInt;
							else
								throw new Exception($"Mod.Call(\"{method}\", int, \"{playerField2}\", object) was requested with an invalid format");

							break;
						case nameof(CoinPlayer.extraLives):
							if(!(args[3] is int livesArg))
								throw new Exception($"Mod.Call(\"{method}\", int, \"{playerField2}\", object) was requested with an invalid format");

							mp2.extraLives = livesArg;
							break;
						case nameof(CoinPlayer.lifeCrystals):
							if(!(args[3] is int crystalsArg))
								throw new Exception($"Mod.Call(\"{method}\", int, \"{playerField2}\", object) was requested with an invalid format");

							mp2.lifeCrystals = crystalsArg;
							break;
						case nameof(CoinPlayer.lifeFruit):
							if(!(args[3] is int fruitArg))
								throw new Exception($"Mod.Call(\"{method}\", int, \"{playerField2}\", object) was requested with an invalid format");

							mp2.lifeFruit = fruitArg;
							break;
						default:
							throw new Exception($"Mod.Call(\"{method}\", int, string, object) was provided an invalid player field name: \"{playerField2}\"");
					}

					return true;
				default:
					throw new Exception($"Unknown Mod.Call() requested: \"{method}\"");
			}
		}
	}
}