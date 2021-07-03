using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;

namespace CoinHP.API.MSIL{
	public static partial class Vanilla{
		internal static void Player_ItemCheck(ILContext il){
			FieldInfo Item_type = typeof(Item).GetField("type", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il){
				Index = 0
			};

			ILHelper.CompleteLog(c, beforeEdit: true);

			//Usage: doing custom item check code for the Life Crystal and Life Fruit, then skipping over the existing code
			ILLabel fruitCheck = null;
			ILLabel afterFruit = null;

			if(!c.TryGotoNext(MoveType.After,
				i => i.MatchLdloc(2),
				i => i.MatchLdfld(Item_type),
				i => i.MatchLdcI4(ItemID.LifeCrystal),
				i => i.MatchBneUn(out fruitCheck)))
				goto bad_il;
			
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldloc_2);
			c.EmitDelegate<Action<Player, Item>>((player, item) => CoinPlayer.ItemCheck_LifeCrystals(player, item));
			c.Emit(OpCodes.Br, fruitCheck);

			if(!c.TryGotoNext(MoveType.After,
				i => i.MatchLdloc(2),
				i => i.MatchLdfld(Item_type),
				i => i.MatchLdcI4(ItemID.LifeFruit),
				i => i.MatchBneUn(out afterFruit)))
				goto bad_il;
			
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldloc_2);
			c.EmitDelegate<Action<Player, Item>>((player, item) => CoinPlayer.ItemCheck_LifeFruit(player, item));
			c.Emit(OpCodes.Br, afterFruit);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c, beforeEdit: false);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Could not fully patch Terraria.Player.ItemCheck()");
		}
	}
}
