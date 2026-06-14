using System;
using System.Collections.Generic;
using InnoVault;
using InnoVault.TileProcessors;
using InnoVault.UIHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace PWing.Content.AzafureMiners
{
	public class AzMinerUI : UIHandle
	{
		private class TechParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Life;
			public float MaxLife;
			public float Size;
			public float Rotation;

			public TechParticle(Vector2 pos)
			{
				Position = pos;
				float angle = Utils.NextFloat(Main.rand, (float)Math.PI * 2f);
				float speed = Utils.NextFloat(Main.rand, 0.2f, 0.8f);
				Velocity = Utils.ToRotationVector2(angle) * speed;
				Life = 0f;
				MaxLife = Utils.NextFloat(Main.rand, 40f, 80f);
				Size = Utils.NextFloat(Main.rand, 1f, 2.5f);
				Rotation = Utils.NextFloat(Main.rand, (float)Math.PI * 2f);
			}

			public bool Update()
			{
				Life += 1f;
				Position += Velocity;
				Velocity *= 0.98f;
				Rotation += 0.02f;
				return Life >= MaxLife;
			}

			public void Draw(SpriteBatch sb, float alpha)
			{
				float t = Life / MaxLife;
				float fade = (float)Math.Sin(t * Math.PI) * alpha;
				Texture2D pixel = VaultAsset.placeholder2.Value;
				Color color = new Color(255, 140, 100) * (0.5f * fade);
				sb.Draw(pixel, Position, new Rectangle(0, 0, 1, 1), color, Rotation, new Vector2(0.5f), new Vector2(Size * 2f, Size * 0.4f), SpriteEffects.None, 0f);
			}
		}

		private class EmberParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Life;
			public float MaxLife;
			public float Size;

			public EmberParticle(Vector2 pos)
			{
				Position = pos;
				Velocity = new Vector2(Utils.NextFloat(Main.rand, -0.5f, 0.5f), Utils.NextFloat(Main.rand, -1.5f, -0.8f));
				Life = 0f;
				MaxLife = Utils.NextFloat(Main.rand, 30f, 60f);
				Size = Utils.NextFloat(Main.rand, 1.5f, 3f);
			}

			public bool Update()
			{
				Life += 1f;
				Position += Velocity;
				Velocity.X += Utils.NextFloat(Main.rand, -0.05f, 0.05f);
				Velocity.Y *= 0.99f;
				return Life >= MaxLife;
			}

			public void Draw(SpriteBatch sb, float alpha)
			{
				float t = Life / MaxLife;
				float fade = (float)Math.Sin(t * Math.PI) * alpha;
				Texture2D pixel = VaultAsset.placeholder2.Value;
				Color color = Color.Lerp(new Color(255, 200, 100), new Color(255, 80, 40), t) * fade;
				sb.Draw(pixel, Position, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f), Size * (1f - t * 0.5f), SpriteEffects.None, 0f);
			}
		}

		private const float PanelWidth = 420f;
		private const float PanelHeight = 320f;
		private const float TitleBarHeight = 36f;

		private float scanLineTimer = 0f;
		private float heatPulseTimer = 0f;
		private float dataStreamTimer = 0f;
		private float sparkTimer = 0f;
		private float borderGlowTimer = 0f;

		private readonly List<TechParticle> techParticles = new List<TechParticle>();
		private int particleSpawnTimer = 0;

		private readonly List<EmberParticle> embers = new List<EmberParticle>();
		private int emberSpawnTimer = 0;

		internal static float uiAlpha = 0f;
		private static bool IsActive;

		private bool isDragging = false;
		private Vector2 dragOffset = new Vector2(800f, 440f);
		public int dontDragTime;
		private float hoverProgress = 0f;

		private Rectangle panelRect;
		private Rectangle titleBarRect;

		public static bool InitDragPos = true;

		public static AzMinerUI Instance => UIHandleLoader.GetUIHandleOfType<AzMinerUI>();
		public static AzMinerTP AzMinerTP { get; set; } = null;
		public static List<AzMinerUISlot> Filters { get; set; }
		public static List<AzMinerUISlot> Items { get; set; }

		public override bool Active
		{
			get
			{
				if (AzMinerTP == null || !((TileProcessor)AzMinerTP).Active)
				{
					IsActive = false;
				}
				return IsActive || uiAlpha > 0.01f;
			}
			set
			{
				IsActive = value;
			}
		}

		public override void Update()
		{
			if (InitDragPos)
			{
				InitDragPos = false;
				DrawPosition = new Vector2(800f, 440f);
			}
			if (dontDragTime > 0)
			{
				dontDragTime--;
			}
			float targetAlpha = (IsActive ? 1f : 0f);
			uiAlpha = MathHelper.Lerp(uiAlpha, targetAlpha, 0.4f);
			if (uiAlpha < 0.1f && !IsActive)
			{
				return;
			}
			UpdateAnimationTimers();
			UpdateParticles();
			if (AzMinerTP == null)
			{
				return;
			}
			HandleDragging();
			DrawPosition.X = MathHelper.Clamp(DrawPosition.X, 220f, Main.screenWidth - 210f - 10f);
			DrawPosition.Y = MathHelper.Clamp(DrawPosition.Y, 170f, Main.screenHeight - 160f - 10f);
			Vector2 topLeft = DrawPosition - new Vector2(210f, 160f);
			panelRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, 440, 320);
			titleBarRect = new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 36);
			UIHitBox = panelRect;
			hoverInMainPage = panelRect.Contains(MouseHitBox);
			if (hoverInMainPage)
			{
				hoverProgress = Math.Min(1f, hoverProgress + 0.08f);
			}
			else
			{
				hoverProgress = Math.Max(0f, hoverProgress - 0.06f);
			}
			if (!Main.playerInventory || Utils.Distance(((TileProcessor)AzMinerTP).CenterInWorld, Main.LocalPlayer.Center) > 320f)
			{
				IsActive = false;
				return;
			}
			if (Filters != null)
			{
				foreach (AzMinerUISlot s in Filters)
				{
					((UIHandle)s).Update();
				}
			}
			if (Items != null)
			{
				foreach (AzMinerUISlot s2 in Items)
				{
					((UIHandle)s2).Update();
				}
			}
			if (hoverInMainPage)
			{
				UIHandle.player.mouseInterface = true;
			}
		}

		private void UpdateAnimationTimers()
		{
			scanLineTimer += 0.04f;
			heatPulseTimer += 0.025f;
			dataStreamTimer += 0.05f;
			sparkTimer += 0.08f;
			borderGlowTimer += 0.035f;
			if (scanLineTimer > Math.PI * 2f) scanLineTimer -= (float)Math.PI * 2f;
			if (heatPulseTimer > Math.PI * 2f) heatPulseTimer -= (float)Math.PI * 2f;
			if (dataStreamTimer > Math.PI * 2f) dataStreamTimer -= (float)Math.PI * 2f;
			if (sparkTimer > Math.PI * 2f) sparkTimer -= (float)Math.PI * 2f;
			if (borderGlowTimer > Math.PI * 2f) borderGlowTimer -= (float)Math.PI * 2f;
		}

		private void UpdateParticles()
		{
			if (uiAlpha < 0.3f)
			{
				return;
			}
			Vector2 panelCenter = DrawPosition;
			particleSpawnTimer++;
			if (IsActive && particleSpawnTimer >= 15 && techParticles.Count < 12)
			{
				particleSpawnTimer = 0;
				float xPos = Utils.NextFloat(Main.rand, panelCenter.X - 210f + 30f, panelCenter.X + 210f - 30f);
				float yPos = Utils.NextFloat(Main.rand, panelCenter.Y - 160f + 50f, panelCenter.Y + 160f - 30f);
				techParticles.Add(new TechParticle(new Vector2(xPos, yPos)));
			}
			for (int i = techParticles.Count - 1; i >= 0; i--)
			{
				if (techParticles[i].Update())
				{
					techParticles.RemoveAt(i);
				}
			}
			if (AzMinerTP != null && AzMinerTP.IsWork)
			{
				emberSpawnTimer++;
				if (emberSpawnTimer >= 6 && embers.Count < 25)
				{
					emberSpawnTimer = 0;
					float xPos2 = Utils.NextFloat(Main.rand, panelCenter.X - 210f + 40f, panelCenter.X + 210f - 40f);
					Vector2 startPos = new Vector2(xPos2, panelCenter.Y + 160f - 20f);
					embers.Add(new EmberParticle(startPos));
				}
			}
			for (int i2 = embers.Count - 1; i2 >= 0; i2--)
			{
				if (embers[i2].Update())
				{
					embers.RemoveAt(i2);
				}
			}
		}

		private void HandleDragging()
		{
			bool hoveringAnySlot = false;
			if (Filters != null)
			{
				foreach (AzMinerUISlot slot in Filters)
				{
					Rectangle slotRect = new Rectangle((int)(DrawPosition.X + slot.OffsetPos.X - 22f), (int)(DrawPosition.Y + slot.OffsetPos.Y - 22f), 44, 44);
					if (slotRect.Contains(MouseHitBox))
					{
						hoveringAnySlot = true;
						break;
					}
				}
			}
			if (!hoveringAnySlot && Items != null)
			{
				foreach (AzMinerUISlot slot2 in Items)
				{
					Rectangle slotRect2 = new Rectangle((int)(DrawPosition.X + slot2.OffsetPos.X - 22f), (int)(DrawPosition.Y + slot2.OffsetPos.Y - 22f), 44, 44);
					if (slotRect2.Contains(MouseHitBox))
					{
						hoveringAnySlot = true;
						break;
					}
				}
			}
			if (panelRect.Contains(MouseHitBox) && !hoveringAnySlot && dontDragTime <= 0 && Main.mouseLeft)
			{
				isDragging = true;
				dragOffset = DrawPosition - MousePosition;
			}
			if (isDragging)
			{
				UIHandle.player.mouseInterface = true;
				DrawPosition = MousePosition + dragOffset;
				if (!Main.mouseLeft)
				{
					isDragging = false;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (uiAlpha < 0.01f || AzMinerTP == null)
			{
				return;
			}
			DrawMainPanel(spriteBatch);
			DrawParticles(spriteBatch);
			DrawTitleBar(spriteBatch);
			DrawStatusIndicator(spriteBatch);
			if (Filters != null)
			{
				foreach (AzMinerUISlot slot in Filters)
				{
					((UIHandle)slot).Draw(spriteBatch);
				}
			}
			if (Items != null)
			{
				foreach (AzMinerUISlot slot2 in Items)
				{
					((UIHandle)slot2).Draw(spriteBatch);
				}
			}
			DrawScanLines(spriteBatch);
		}

		private void DrawMainPanel(SpriteBatch sb)
		{
			Texture2D pixel = VaultAsset.placeholder2.Value;
			float alpha = uiAlpha;
			Rectangle shadowRect = panelRect;
			shadowRect.Offset(6, 8);
			sb.Draw(pixel, shadowRect, new Rectangle(0, 0, 1, 1), Color.Black * (alpha * 0.5f));
			Color bgColor = new Color(22, 14, 12) * (alpha * 0.95f);
			sb.Draw(pixel, panelRect, new Rectangle(0, 0, 1, 1), bgColor);
			Rectangle innerRect = panelRect;
			innerRect.Inflate(-4, -4);
			Color innerBg = new Color(35, 20, 18) * (alpha * 0.85f);
			sb.Draw(pixel, innerRect, new Rectangle(0, 0, 1, 1), innerBg);
			float pulse = (float)Math.Sin(borderGlowTimer) * 0.3f + 0.7f;
			Color borderColor = new Color(180, 80, 60) * (alpha * pulse);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 3), new Rectangle(0, 0, 1, 1), borderColor);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 3, panelRect.Width, 3), new Rectangle(0, 0, 1, 1), borderColor * 0.8f);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 3, panelRect.Height), new Rectangle(0, 0, 1, 1), borderColor * 0.9f);
			sb.Draw(pixel, new Rectangle(panelRect.Right - 3, panelRect.Y, 3, panelRect.Height), new Rectangle(0, 0, 1, 1), borderColor * 0.9f);
			Rectangle innerBorder = panelRect;
			innerBorder.Inflate(-6, -6);
			Color innerGlow = new Color(255, 120, 80) * (alpha * 0.15f * pulse);
			sb.Draw(pixel, new Rectangle(innerBorder.X, innerBorder.Y, innerBorder.Width, 2), new Rectangle(0, 0, 1, 1), innerGlow);
			sb.Draw(pixel, new Rectangle(innerBorder.X, innerBorder.Bottom - 2, innerBorder.Width, 2), new Rectangle(0, 0, 1, 1), innerGlow * 0.7f);
			sb.Draw(pixel, new Rectangle(innerBorder.X, innerBorder.Y, 2, innerBorder.Height), new Rectangle(0, 0, 1, 1), innerGlow * 0.8f);
			sb.Draw(pixel, new Rectangle(innerBorder.Right - 2, innerBorder.Y, 2, innerBorder.Height), new Rectangle(0, 0, 1, 1), innerGlow * 0.8f);
			DrawCornerDecoration(sb, new Vector2(panelRect.X, panelRect.Y), alpha, 0);
			DrawCornerDecoration(sb, new Vector2(panelRect.Right, panelRect.Y), alpha, 1);
			DrawCornerDecoration(sb, new Vector2(panelRect.X, panelRect.Bottom), alpha, 2);
			DrawCornerDecoration(sb, new Vector2(panelRect.Right, panelRect.Bottom), alpha, 3);
		}

		private void DrawCornerDecoration(SpriteBatch sb, Vector2 pos, float alpha, int corner)
		{
			Texture2D pixel = VaultAsset.placeholder2.Value;
			Color decorColor = new Color(200, 100, 70) * (alpha * 0.6f);
			int size = 12;
			int thickness = 2;
			switch (corner)
			{
				case 0:
					sb.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, size, thickness), new Rectangle(0, 0, 1, 1), decorColor);
					sb.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, thickness, size), new Rectangle(0, 0, 1, 1), decorColor);
					break;
				case 1:
					sb.Draw(pixel, new Rectangle((int)pos.X - size, (int)pos.Y, size, thickness), new Rectangle(0, 0, 1, 1), decorColor);
					sb.Draw(pixel, new Rectangle((int)pos.X - thickness, (int)pos.Y, thickness, size), new Rectangle(0, 0, 1, 1), decorColor);
					break;
				case 2:
					sb.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y - thickness, size, thickness), new Rectangle(0, 0, 1, 1), decorColor);
					sb.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y - size, thickness, size), new Rectangle(0, 0, 1, 1), decorColor);
					break;
				case 3:
					sb.Draw(pixel, new Rectangle((int)pos.X - size, (int)pos.Y - thickness, size, thickness), new Rectangle(0, 0, 1, 1), decorColor);
					sb.Draw(pixel, new Rectangle((int)pos.X - thickness, (int)pos.Y - size, thickness, size), new Rectangle(0, 0, 1, 1), decorColor);
					break;
			}
		}

		private void DrawTitleBar(SpriteBatch sb)
		{
			Texture2D pixel = VaultAsset.placeholder2.Value;
			float alpha = uiAlpha;
			Color titleBg = new Color(45, 25, 22) * (alpha * 0.9f);
			sb.Draw(pixel, titleBarRect, new Rectangle(0, 0, 1, 1), titleBg);
			float pulse = (float)Math.Sin(heatPulseTimer * 2f) * 0.3f + 0.7f;
			Color separatorColor = new Color(200, 90, 60) * (alpha * pulse * 0.8f);
			sb.Draw(pixel, new Rectangle(titleBarRect.X + 8, titleBarRect.Bottom - 2, titleBarRect.Width - 16, 2), new Rectangle(0, 0, 1, 1), separatorColor);
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			string title = VaultUtils.GetLocalizedItemName<AzafureMiner>().Value;
			Vector2 titleSize = font.MeasureString(title);
			Vector2 titlePos = new Vector2(titleBarRect.X + (titleBarRect.Width - titleSize.X) / 2f, titleBarRect.Y + (titleBarRect.Height - titleSize.Y) / 2f);
			Color glowColor = new Color(255, 150, 100) * (alpha * 0.5f);
			for (int i = 0; i < 4; i++)
			{
				float angle = (float)Math.PI * 2f * i / 4f;
				Vector2 offset = Utils.ToRotationVector2(angle) * 2f;
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, title, titlePos + offset, glowColor);
			}
			DynamicSpriteFontExtensionMethods.DrawString(sb, font, title, titlePos, new Color(255, 220, 200) * alpha);
		}

		private void DrawStatusIndicator(SpriteBatch sb)
		{
			Texture2D pixel = VaultAsset.placeholder2.Value;
			float alpha = uiAlpha;
			Vector2 indicatorPos = new Vector2(panelRect.Right - 25, titleBarRect.Y + titleBarRect.Height / 2);
			Color bgColor = new Color(20, 12, 10) * alpha;
			Rectangle bgRect = new Rectangle((int)indicatorPos.X - 8, (int)indicatorPos.Y - 8, 16, 16);
			sb.Draw(pixel, bgRect, new Rectangle(0, 0, 1, 1), bgColor);
			bool isWorking = AzMinerTP?.IsWork ?? false;
			float pulse = (float)Math.Sin(sparkTimer * (isWorking ? 3f : 1f)) * 0.3f + 0.7f;
			Color lightColor = isWorking ? new Color(100, 255, 100) : new Color(255, 180, 80);
			lightColor *= alpha * pulse;
			Rectangle lightRect = new Rectangle((int)indicatorPos.X - 5, (int)indicatorPos.Y - 5, 10, 10);
			sb.Draw(pixel, lightRect, new Rectangle(0, 0, 1, 1), lightColor);
			if (isWorking)
			{
				Color glowCol = new Color(100, 255, 100) * (alpha * 0.3f * pulse);
				Rectangle glowRect = new Rectangle((int)indicatorPos.X - 8, (int)indicatorPos.Y - 8, 16, 16);
				sb.Draw(pixel, glowRect, new Rectangle(0, 0, 1, 1), glowCol);
			}
		}

		private void DrawScanLines(SpriteBatch sb)
		{
			Texture2D pixel = VaultAsset.placeholder2.Value;
			float alpha = uiAlpha * 0.08f;
			float scanY = (float)panelRect.Y + (float)Math.Sin(scanLineTimer) * 0.5f * panelRect.Height + panelRect.Height * 0.5f;
			Color scanColor = new Color(255, 150, 100) * alpha;
			for (int i = 0; i < 3; i++)
			{
				float y = scanY + i * 3;
				if (y > panelRect.Y && y < panelRect.Bottom)
				{
					sb.Draw(pixel, new Rectangle(panelRect.X + 10, (int)y, panelRect.Width - 20, 1), new Rectangle(0, 0, 1, 1), scanColor * (1f - i * 0.3f));
				}
			}
		}

		private void DrawParticles(SpriteBatch sb)
		{
			float alpha = uiAlpha;
			foreach (TechParticle particle in techParticles)
			{
				particle.Draw(sb, alpha);
			}
			foreach (EmberParticle ember in embers)
			{
				ember.Draw(sb, alpha);
			}
		}
	}
}
