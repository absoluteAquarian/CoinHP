using System.Collections.Generic;
using Terraria;

namespace CoinHP.API.Detours{
	public static partial class Vanilla{
		internal static long Utils_CoinsCount(On.Terraria.Utils.orig_CoinsCount orig, out bool overFlowing, Item[] inv, int[] ignoreSlots){
			List<int> list = new List<int>(ignoreSlots);
			long num = 0L;
			for(int i = 0; i < inv.Length; i++){
				long prev = num;

				unchecked{
					if(!list.Contains(i)){
						switch(inv[i].type){
							case 71:
								num += inv[i].stack;
								break;
							case 72:
								num += inv[i].stack * 100;
								break;
							case 73:
								num += inv[i].stack * 10000;
								break;
							case 74:
								num += inv[i].stack * 1000000;
								break;
						}
					}
				}

				if(num < prev){
					overFlowing = true;
					return prev;
				}
			}

			overFlowing = false;
			return num;
		}

		internal static long Utils_CoinsCombineStacks(On.Terraria.Utils.orig_CoinsCombineStacks orig, out bool overFlowing, long[] coinCounts){
			long num = 0L;
			foreach(long num2 in coinCounts){
				long prev = num;

				unchecked{
					num += num2;
				}

				if(num < prev){
					overFlowing = true;
					return prev;
				}
			}

			overFlowing = false;
			return num;
		}
	}
}
