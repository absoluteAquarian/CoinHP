using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;

namespace CoinHP{
	public static class CoinUtils{
		public static double Cbrt(double value){
			if(value == 0)
				return 0;
			if(value == 1)
				return 1;

			return Math.Pow(value, 1.0 / 3.0);
		}

		public static string NoCoinsMessage(Player player) => $"{player.name} ran out of coins.";
	}
}
