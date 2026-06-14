using System.Collections.Generic;
using CalamityMod;
using InnoVault;
using InnoVault.TileProcessors;
using InnoVault.UIHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace PWing.Content.AzafureMiners
{
	public class AzafureMinerTile : ModTile
	{
		public override void Load()
		{
			AzMinerUI.Items = new List<AzMinerUISlot>();
			AzMinerUI.Filters = new List<AzMinerUISlot>();
			for (int i = 0; i < AzMinerTP.FiltersCount; i++)
			{
				AzMinerUI.Filters.Add(new AzMinerUISlot
				{
					OffsetPos = new Vector2(-168f, -100f + 240f / (float)AzMinerTP.FiltersCount * (float)i),
					itemIndex = i
				});
			}
			int z = 0;
			Vector2 pos = new Vector2(-110f, -100f);
			for (int j = 0; j < AzMinerTP.ItemsCount; j++)
			{
				AzMinerUI.Items.Add(new AzMinerUISlot
				{
					OffsetPos = new Vector2(pos.X, pos.Y),
					type = 1,
					itemIndex = j
				});
				z++;
				pos.X += 50f;
				if (z >= 7)
				{
					z = 0;
					pos.Y += 50f;
					pos.X = -110f;
				}
			}
		}

		public override void Unload()
		{
			AzMinerUI.Filters = null;
			AzMinerUI.Items = null;
		}

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileWaterDeath[Type] = false;
			AddMapEntry(new Color(190, 72, 81), VaultUtils.GetLocalizedItemName<AzafureMiner>());
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.AnchorBottom = new AnchorData((AnchorType)11, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.CoordinateHeights = new int[3] { 16, 16, 16 };
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			VaultUtils.SetMouseOverByTile<AzafureMiner>(Main.LocalPlayer);
		}

		public override bool RightClick(int i, int j)
		{
			AzMinerTP tempTP = null;
			if (TileProcessorLoader.AutoPositionGetTP<AzMinerTP>(i, j, out tempTP))
			{
				bool sameMachine = AzMinerUI.AzMinerTP == tempTP;
				AzMinerUI.AzMinerTP = tempTP;
				if (sameMachine)
				{
					((UIHandle)AzMinerUI.Instance).Active = !((UIHandle)AzMinerUI.Instance).Active;
				}
				else
				{
					((UIHandle)AzMinerUI.Instance).Active = true;
				}
				Main.playerInventory = true;
				SoundStyle menuOpen = SoundID.MenuOpen;
				menuOpen.Pitch = 0.3f;
				SoundEngine.PlaySound(menuOpen, (Vector2?)null);
				return true;
			}
			return false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Point16 point = default(Point16);
			if (!VaultUtils.SafeGetTopLeft(i, j, out point))
			{
				return false;
			}
			AzMinerTP tempTP = null;
			if (!TileProcessorLoader.ByPositionGetTP<AzMinerTP>(point, out tempTP))
			{
				return false;
			}
			Tile t = Main.tile[i, j];
			int frameXPos = t.TileFrameX;
			int frameYPos = t.TileFrameY;
			Texture2D tex = TextureAssets.Tile[Type].Value;
			Vector2 offset = (Main.drawToScreen ? Vector2.Zero : (new Vector2(Main.offScreenRange) + tempTP.OffsetPos));
			Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + offset;
			Color drawColor = Lighting.GetColor(i, j);
			if (!tempTP.IsWork)
			{
				drawColor = new Color(drawColor.R / 2, drawColor.G / 2, drawColor.B / 2, byte.MaxValue);
			}
			bool slc = false;
			bool inArea = false;
			tempTP.SmartCursorHover = slc;
			if (inArea)
			{
				Color outlineColor = (slc ? Color.Yellow : Color.Gray);
				if (!t.IsHalfBlock && t.Slope == 0)
				{
					spriteBatch.Draw(tex, drawOffset, new Rectangle(frameXPos, frameYPos, 16, 16), outlineColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.01f);
				}
				else if (t.IsHalfBlock)
				{
					spriteBatch.Draw(tex, drawOffset + Vector2.UnitY * 8f, new Rectangle(frameXPos, frameYPos, 16, 16), outlineColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.01f);
				}
			}
			if (!t.IsHalfBlock && t.Slope == 0)
			{
				spriteBatch.Draw(tex, drawOffset, new Rectangle(frameXPos, frameYPos, 16, 16), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			else if (t.IsHalfBlock)
			{
				spriteBatch.Draw(tex, drawOffset + Vector2.UnitY * 8f, new Rectangle(frameXPos, frameYPos, 16, 16), drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}
