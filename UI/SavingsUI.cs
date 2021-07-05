using CoinHP.API.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace CoinHP.UI{
	public class SavingsUI : UIState{
		public static bool Visible;

		public float oldPosX = 800, oldPosY = 400;

		private UIDragablePanel panel;
		private UIItemSlot[] slots;

		public override void OnInitialize(){
			panel = new UIDragablePanel();
			var backTexture = Main.inventoryBack9Texture;
			const int buffer = 20;

			panel.Width.Set((backTexture.Width + buffer) * 4 + buffer, 0f);
			panel.Height.Set(backTexture.Height + 2 * buffer + 40, 0f);
			panel.Top.Set(oldPosY, 0f);
			panel.Left.Set(oldPosX, 0f);
			Append(panel);
			
			UIText caption = new UIText("Emergency Savings", textScale: 1f){
				HAlign = 0.5f
			};
			caption.Top.Set(10, 0f);
			panel.Append(caption);

			int x = 15;
			slots = new UIItemSlot[4];
			for(int i = 0; i < 4; i++){
				slots[i] = new UIItemSlot();
				slots[i].Left.Set(x - 8, 0f);
				slots[i].Top.Set(40 + buffer - 8, 0f);
				panel.Append(slots[i]);

				x += backTexture.Width + buffer;
			}

			slots[0].ValidItemFunc = item => HandleItemSlot(0, item);
			slots[1].ValidItemFunc = item => HandleItemSlot(1, item);
			slots[2].ValidItemFunc = item => HandleItemSlot(2, item);
			slots[3].ValidItemFunc = item => HandleItemSlot(3, item);
		}

		public void InitializeSlots(CoinPlayer player){
			for(int i = 0; i < 4; i++)
				slots[i].SetItem(player.savings[i], player.savings[i].stack);
		}

		private bool HandleItemSlot(int slot, Item incoming){
			bool goodItem = incoming.IsAir || incoming.type == ItemID.CopperCoin || incoming.type == ItemID.SilverCoin || incoming.type == ItemID.GoldCoin || incoming.type == ItemID.PlatinumCoin;

			if(goodItem && Main.mouseLeft && Main.mouseLeftRelease){
				//Separating it so i can see it in VS
				var player = Main.LocalPlayer.GetModPlayer<CoinPlayer>();
				player.savings[slot] = incoming;
			}

			return goodItem;
		}

		public override void Update(GameTime gameTime){
			base.Update(gameTime);

			oldPosX = panel.Left.Pixels;
			oldPosY = panel.Top.Pixels;

			Main.playerInventory = true;

			if(Visible && PlayerInput.Triggers.JustPressed.Inventory){
				Visible = false;

				Main.playerInventory = false;

				//Oink
				Main.PlaySound(SoundID.Item59);
			}
		}
	}
}
