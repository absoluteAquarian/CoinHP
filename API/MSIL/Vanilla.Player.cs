using CoinHP.Items.Weapons;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoinHP.API.MSIL{
	public static partial class Vanilla{
		internal static void Player_ItemCheck(ILContext il){
			FieldInfo Item_type = typeof(Item).GetField("type", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il){
				Index = 0
			};

			ILHelper.CompleteLog(c, beforeEdit: true);

			//Usage: allowing the Golden Gatligator to have a random spread
			ILLabel postGatligatorCheck = null;
			ILLabel newPostCheck = c.DefineLabel();
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(2),
				i => i.MatchLdfld(Item_type),
				i => i.MatchLdcI4(ItemID.Gatligator),
				i => i.MatchBneUn(out postGatligatorCheck)))
				goto bad_il;
			c.Index--;
			c.Instrs[c.Index].Operand = newPostCheck;

			if(!c.TryGotoNext(MoveType.Before, i => i.MatchLdloc(125),
				i => i.MatchLdloc(127),
				i => i.MatchMul(),
				i => i.MatchStloc(125),
				i => i.MatchLdloc(126),
				i => i.MatchLdloc(127),
				i => i.MatchMul(),
				i => i.MatchStloc(126)))
				goto bad_il;

			int index = c.Index;
			c.Emit(OpCodes.Ldloc, 2);
			c.EmitDelegate<Func<Item, bool>>(item => item.type == ModContent.ItemType<GoldenGatligator>());
			c.Emit(OpCodes.Brfalse, postGatligatorCheck);
			c.Emit(OpCodes.Ldloc, 125);
			c.Emit(OpCodes.Ldloc, 127);
			c.EmitDelegate<Func<float, float, float>>((mouseX, dirLength) => mouseX + Main.rand.Next(-30, 30) * 0.02f / dirLength);
			c.Emit(OpCodes.Stloc, 125);
			c.Emit(OpCodes.Ldloc, 126);
			c.Emit(OpCodes.Ldloc, 127);
			c.EmitDelegate<Func<float, float, float>>((mouseY, dirLength) => mouseY + Main.rand.Next(-30, 30) * 0.02f / dirLength);
			c.Emit(OpCodes.Stloc, 126);

			c.Index = index;
			c.MarkLabel(newPostCheck);

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
