namespace CoinHP.API{
	public static class EditsLoader{
		public static readonly bool LogILEdits = false;

		public static void Load(){
			ILHelper.InitMonoModDumps();

			On.Terraria.Player.UpdateLifeRegen += Detours.Vanilla.Player_UpdateLifeRegen;
			On.Terraria.Player.QuickHeal += Detours.Vanilla.Player_QuickHeal;
			On.Terraria.Player.QuickMana += Detours.Vanilla.Player_QuickMana;

			On.Terraria.Chest.SetupShop += Detours.Vanilla.Chest_SetupShop;

			IL.Terraria.GameContent.UI.Elements.UICharacterListItem.DrawSelf += MSIL.Vanilla.UICharacterListItem_DrawSelf;
			IL.Terraria.Player.ItemCheck += MSIL.Vanilla.Player_ItemCheck;

			ILHelper.DeInitMonoModDumps();
		}

		public static void Unload(){
			//used in CoreMod.Load() in a try-catch block
			On.Terraria.Player.UpdateLifeRegen -= Detours.Vanilla.Player_UpdateLifeRegen;
			On.Terraria.Player.QuickHeal -= Detours.Vanilla.Player_QuickHeal;
			On.Terraria.Player.QuickMana -= Detours.Vanilla.Player_QuickMana;

			On.Terraria.Chest.SetupShop -= Detours.Vanilla.Chest_SetupShop;

			IL.Terraria.GameContent.UI.Elements.UICharacterListItem.DrawSelf -= MSIL.Vanilla.UICharacterListItem_DrawSelf;
			IL.Terraria.Player.ItemCheck -= MSIL.Vanilla.Player_ItemCheck;

			ILHelper.DeInitMonoModDumps();
		}
	}
}
