using System;
using InnoVault;
using InnoVault.TileProcessors;
using InnoVault.UIHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PWing.Common.Configs;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace PWing.Content.AzafureMiners
{
	public class PWingAzMinerUISlot : UIHandle
	{
		public int itemIndex = 0;
		public int type = 0;
		public Vector2 OffsetPos = default;

		private const int SlotSize = 44;
		private float hoverProgress = 0f;
		private bool hoverRightHeld;
		private int lastHoverSlot = -1;

		public override LayersModeEnum LayersMode => (LayersModeEnum)0;

		public Item Item
		{
			get
			{
				if (type == 0)
				{
					return PWingAzMinerUI.AzMinerTP.filters[itemIndex];
				}
				return PWingAzMinerUI.AzMinerTP.items[itemIndex];
			}
			set
			{
				if (type == 0)
				{
					PWingAzMinerUI.AzMinerTP.filters[itemIndex] = value;
				}
				else
				{
					PWingAzMinerUI.AzMinerTP.items[itemIndex] = value;
				}
			}
		}

		public override void Update()
		{
			base.DrawPosition = ((UIHandle)PWingAzMinerUI.Instance).DrawPosition + OffsetPos;
			base.UIHitBox = new Rectangle((int)(base.DrawPosition.X - 22f), (int)(base.DrawPosition.Y - 22f), 44, 44);
			base.hoverInMainPage = base.UIHitBox.Contains(MouseHitBox);
			if (!base.hoverInMainPage)
			{
				hoverProgress = Math.Max(0f, hoverProgress - 0.1f);
				hoverRightHeld = false;
				return;
			}
			hoverProgress = Math.Min(1f, hoverProgress + 0.12f);
			Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseItem.IsAir && !Item.IsAir)
			{
				Main.HoverItem = Item.Clone();
			}
			if (Item.IsAir && type == 0 && Main.mouseItem.IsAir)
			{
				Main.instance.MouseText(PWing.Instance.GetLocalization("SlotInfo2", null).Value, 0, 0, -1, -1, -1, -1, 0);
			}
			if (!Item.IsAir && Utils.PressingShift(Main.keyState))
			{
				TryMergeStacks();
			}
			if (!Main.mouseItem.IsAir || !Item.IsAir)
			{
				HandleRightClick();
				HandleLeftClick();
				((TileProcessor)PWingAzMinerUI.AzMinerTP).SendData();
			}
		}

		private void HandleRightClick()
		{
			int currentSlot = itemIndex;
			if (currentSlot != lastHoverSlot)
			{
				hoverRightHeld = false;
				lastHoverSlot = currentSlot;
			}
			if (Main.mouseItem.IsAir || (!Item.IsAir && (Item.type != Main.mouseItem.type || Item.stack >= Item.maxStack || Main.mouseItem.stack <= 0)))
			{
				return;
			}
			if ((int)((UIHandle)this).keyRightPressState == 3 && base.hoverInMainPage)
			{
				if (!hoverRightHeld)
				{
					AddOneFromMouse();
					hoverRightHeld = true;
				}
			}
			else if ((int)((UIHandle)this).keyRightPressState == 1)
			{
				AddOneFromMouse();
				hoverRightHeld = true;
			}
		}

		private void HandleLeftClick()
		{
			if ((int)((UIHandle)this).keyLeftPressState == 1)
			{
				PWingAzMinerUI.Instance.dontDragTime = 2;
				KeyboardState state = Keyboard.GetState();
				if (state.IsKeyDown(Keys.LeftShift))
				{
					SoundEngine.PlaySound(SoundID.Grab, (Vector2?)null);
					Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(null), Item, Item.stack);
					ClearSlot();
				}
				else if (Main.mouseItem.type == Item.type)
				{
					MergeWithMouse();
				}
				else if (type == 0 && !IsOre(Main.mouseItem) && Main.mouseItem.type != 0)
				{
					SoundEngine.PlaySound(SoundID.MenuTick, (Vector2?)null);
					Main.NewText(PWing.Instance.GetLocalization("AzMinerOreWarning", null).Value, Color.Red);
				}
				else
				{
					SwapWithMouse();
				}
			}
		}

		private void AddOneFromMouse()
		{
			SoundEngine.PlaySound(SoundID.Grab, (Vector2?)null);
			if (Item.IsAir)
			{
				Item = Main.mouseItem.Clone();
				Item.stack = 1;
			}
			else
			{
				Item.stack++;
			}
			Main.mouseItem.stack--;
			if (Main.mouseItem.stack <= 0)
			{
				Main.mouseItem.TurnToAir(false);
			}
		}

		private void MergeWithMouse()
		{
			SoundEngine.PlaySound(SoundID.Grab, (Vector2?)null);
			Item targetSlot = GetSlotRef();
			int total = targetSlot.stack + Main.mouseItem.stack;
			if (total > targetSlot.maxStack)
			{
				int overflow = total - targetSlot.maxStack;
				targetSlot.stack = targetSlot.maxStack;
				Main.mouseItem.stack = overflow;
			}
			else
			{
				targetSlot.stack = total;
				Main.mouseItem.TurnToAir(false);
			}
		}

		private void SwapWithMouse()
		{
			SoundEngine.PlaySound(SoundID.Grab, (Vector2?)null);
			Item targetSlot = GetSlotRef();
			Item temp = targetSlot.Clone();
			targetSlot.SetDefaults(Main.mouseItem.type);
			targetSlot.stack = Main.mouseItem.stack;
			Main.mouseItem = temp;
		}

		private void TryMergeStacks()
		{
			bool merged = false;
			foreach (PWingAzMinerUISlot slot in PWingAzMinerUI.Items)
			{
				if (slot.itemIndex != itemIndex && Item.type == slot.Item.type && Item.stack < Item.maxStack)
				{
					int total = Item.stack + slot.Item.stack;
					if (total > Item.maxStack)
					{
						Item.stack = Item.maxStack;
						slot.Item.stack = total - Item.maxStack;
					}
					else
					{
						Item.stack = total;
						slot.Item.TurnToAir(false);
					}
					merged = true;
				}
			}
			if (merged)
			{
				SoundEngine.PlaySound(SoundID.Grab, (Vector2?)null);
			}
		}

		private void ClearSlot()
		{
			if (type == 0)
			{
				PWingAzMinerUI.AzMinerTP.filters[itemIndex].TurnToAir(false);
			}
			else
			{
				PWingAzMinerUI.AzMinerTP.items[itemIndex].TurnToAir(false);
			}
		}

		private Item GetSlotRef()
		{
			return (type == 0) ? PWingAzMinerUI.AzMinerTP.filters[itemIndex] : PWingAzMinerUI.AzMinerTP.items[itemIndex];
		}

		private bool IsOre(Item item)
		{
			if (item == null || item.type == 0)
			{
				return false;
			}
			
			if (PWingAzMinerTP.ItemIsOre.TryGetValue(item.type, out bool isOre) && isOre)
			{
				return true;
			}
			
			if (PWingConfig.Instance != null && PWingConfig.Instance.azafureMinerWhitelistEnabled)
			{
				foreach (ItemDefinition itemDef in PWingConfig.Instance.azafureMinerWhitelistItems)
				{
					if (!itemDef.IsUnloaded && itemDef.Type == item.type)
					{
						return true;
					}
				}
			}
			
			return false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			float alpha = PWingAzMinerUI.uiAlpha;
			if (alpha < 0.01f)
			{
				return;
			}
			Texture2D pixel = VaultAsset.placeholder2.Value;
			Vector2 center = base.DrawPosition;
			float scale = 1f + hoverProgress * 0.08f;
			int scaledSize = (int)(44f * scale);
			Rectangle slotRect = new Rectangle((int)(center.X - scaledSize / 2f), (int)(center.Y - scaledSize / 2f), scaledSize, scaledSize);
			Color bgColor = (type == 0) ? new Color(50, 30, 28) : new Color(35, 22, 20);
			bgColor *= alpha * 0.9f;
			spriteBatch.Draw(pixel, slotRect, new Rectangle(0, 0, 1, 1), bgColor);
			Color borderColor = (type == 0) ? new Color(200, 120, 80) : new Color(160, 90, 60);
			borderColor *= alpha * (0.6f + hoverProgress * 0.4f);
			spriteBatch.Draw(pixel, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 2), new Rectangle(0, 0, 1, 1), borderColor);
			spriteBatch.Draw(pixel, new Rectangle(slotRect.X, slotRect.Bottom - 2, slotRect.Width, 2), new Rectangle(0, 0, 1, 1), borderColor * 0.8f);
			spriteBatch.Draw(pixel, new Rectangle(slotRect.X, slotRect.Y, 2, slotRect.Height), new Rectangle(0, 0, 1, 1), borderColor * 0.9f);
			spriteBatch.Draw(pixel, new Rectangle(slotRect.Right - 2, slotRect.Y, 2, slotRect.Height), new Rectangle(0, 0, 1, 1), borderColor * 0.9f);
			if (hoverProgress > 0.01f)
			{
				Color glowColor = new Color(255, 180, 120) * (alpha * hoverProgress * 0.2f);
				Rectangle glowRect = slotRect;
				glowRect.Inflate(2, 2);
				spriteBatch.Draw(pixel, glowRect, new Rectangle(0, 0, 1, 1), glowColor);
			}
			if (!Item.IsAir)
			{
				float itemScale = scale * (0.9f + hoverProgress * 0.1f);
				ItemSlot.DrawItemIcon(Item, 1, spriteBatch, center, itemScale, 128f, Color.White * alpha);
				if (Item.stack > 1)
				{
					DynamicSpriteFont font = FontAssets.ItemStack.Value;
					string stackText = Item.stack.ToString();
					Vector2 stackPos = center + new Vector2(4f, 6f) * scale;
					DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, font, stackText, stackPos + new Vector2(1f, 1f), Color.Black * alpha * 0.8f, 0f, Vector2.Zero, 0.75f * scale, SpriteEffects.None, 0f);
					DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, font, stackText, stackPos, Color.White * alpha, 0f, Vector2.Zero, 0.75f * scale, SpriteEffects.None, 0f);
				}
			}
			else if (type == 0)
			{
				Color hintColor = new Color(150, 100, 80) * (alpha * 0.4f);
				DynamicSpriteFont font2 = FontAssets.MouseText.Value;
				string hint = "?";
				Vector2 hintSize = font2.MeasureString(hint);
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, font2, hint, center - hintSize / 2f, hintColor);
			}
		}
	}
}
