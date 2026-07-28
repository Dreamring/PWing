using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using PWing.Common;
using PWing;

namespace PWing.Content.FakePlayer
{
    public static class PWingFakePlayerDrawing
    {
        public static Color DrawColor(PWingFakePlayer fakePlayer)
        {
            if (fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Tesseract)
            {
                return ColorHelper.TesseractColor(MathHelper.TwoPi * (fakePlayer.OverrideUseSlot % 10) / 10f, 0.4f);
            }
            return Color.White;
        }

        public static Texture2D WingTexture(int type, bool outLine)
        {
            if (outLine)
            {
                if (type == PWingFakePlayerTypeID.Hydro)
                {
                    return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/HydroServantWingsOutline").Value;
                }
                return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantWingsOutline").Value;
            }
            if (type == PWingFakePlayerTypeID.Hydro)
            {
                return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/HydroServantWings").Value;
            }
            return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantWings").Value;
        }

        public static Texture2D FakePlayerTexture(int type, bool outLine, int tesseractType)
        {
            if (outLine)
            {
                if (type == PWingFakePlayerTypeID.Tesseract)
                {
                    if (tesseractType == 2 || tesseractType == 3)
                    {
                        return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/Tesseract/TesseractServantSheetWhite").Value;
                    }
                    return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/Tesseract/TesseractServantSheetWhiteMale").Value;
                }
                if (type == PWingFakePlayerTypeID.Hydro)
                {
                    return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/HydroServantSheetOutline").Value;
                }
                return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantSheetGreen").Value;
            }
            if (type == PWingFakePlayerTypeID.Tesseract)
            {
                return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/Tesseract/TesseractServantSheet" + (tesseractType + 1)).Value;
            }
            if (type == PWingFakePlayerTypeID.Hydro)
            {
                return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/HydroServantSheet").Value;
            }
            return PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantSheet").Value;
        }

        public static void DrawFrontArm(PWingFakePlayer fakePlayer, SpriteBatch spriteBatch)
        {
            Texture2D texture = FakePlayerTexture(fakePlayer.FakePlayerType, false, fakePlayer.OverrideUseSlot % 10);
            SpriteEffects spriteEffects = fakePlayer.playerEffect;
            Vector2 vector = new Vector2((int)fakePlayer.Position.X, (int)fakePlayer.Position.Y) - Main.screenPosition + new Vector2(PWingFakePlayer.Width / 2, PWingFakePlayer.Height / 2 - 3);
            if (fakePlayer.compFrontArmFrame.X / fakePlayer.compFrontArmFrame.Width >= 7)
            {
                vector += new Vector2((!fakePlayer.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1), (!fakePlayer.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1));
            }
            Vector2 origin = fakePlayer.bodyVect;
            Vector2 position = vector + GetCompositeOffset_FrontArm(fakePlayer);
            Color color = DrawColor(fakePlayer);
            Rectangle frame = fakePlayer.compFrontArmFrame;
            float rotation = fakePlayer.compositeFrontArmRotation;
            spriteBatch.Draw(texture, position, frame, color, rotation, origin + GetCompositeOffset_FrontArm(fakePlayer), 1f, spriteEffects, 0);
        }

        public static Vector2 GetCompositeOffset_BackArm(ref PlayerDrawSet drawInfo)
        {
            return new Vector2(6 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 2 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1)));
        }

        public static void DrawBackArm(PWingFakePlayer fakePlayer, ref PlayerDrawSet drawInfo, bool outline)
        {
            Texture2D texture = FakePlayerTexture(fakePlayer.FakePlayerType, false, fakePlayer.OverrideUseSlot % 10);
            Texture2D textureGreen = FakePlayerTexture(fakePlayer.FakePlayerType, true, fakePlayer.OverrideUseSlot % 10);
            Vector2 vector = new Vector2((int)drawInfo.Position.X, (int)drawInfo.Position.Y) - Main.screenPosition + new Vector2(PWingFakePlayer.Width / 2, PWingFakePlayer.Height / 2 - 3);
            Vector2 vector3 = vector;
            Vector2 compositeOffset_BackArm = GetCompositeOffset_BackArm(ref drawInfo);
            vector3 += compositeOffset_BackArm;
            float rotation = drawInfo.compositeBackArmRotation;
            Color saveColor = drawInfo.armGlowColor;
            drawInfo.armGlowColor = DrawColor(fakePlayer);
            Color color = DrawColor(fakePlayer);
            if (outline)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 circular = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(90 * k));
                    PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.BackArm, new DrawData(textureGreen, vector3 + circular, drawInfo.compBackArmFrame, color, rotation, drawInfo.bodyVect + compositeOffset_BackArm, 1f, drawInfo.playerEffect, 0));
                }
            }
            else
            {
                PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.BackArm, new DrawData(texture, vector3, drawInfo.compBackArmFrame, color, rotation, drawInfo.bodyVect + compositeOffset_BackArm, 1f, drawInfo.playerEffect, 0));
            }
            drawInfo.armGlowColor = saveColor;
        }

        public static void DrawTail(PWingFakePlayer fakePlayer, ref PlayerDrawSet drawInfo, bool outLine = false)
        {
            if (fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Subspace || fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Tesseract)
            {
                Texture2D texture = fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Tesseract ? PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/TesseractServantTail").Value : PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantTail").Value;
                Texture2D textureOutline = fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Tesseract ? PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/TesseractServantTailOutline").Value : PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantTailOutline").Value;
                Vector2 center = drawInfo.Position + new Vector2(PWingFakePlayer.Width / 2, PWingFakePlayer.Height / 2 + 2);
                Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
                Vector2 velo = new Vector2(0, 4f);
                float scale = 1f;
                List<Vector2> positions = new List<Vector2>();
                List<float> rotations = new List<float>();
                for (int i = 0; i < 9; i++)
                {
                    Vector2 toOldPosition = fakePlayer.SecondPosition - drawInfo.Position;
                    toOldPosition.SafeNormalize(Vector2.Zero);
                    velo += toOldPosition * 0.333f;
                    velo = velo.SafeNormalize(Vector2.Zero) * scale * 4;
                    center += velo;
                    Vector2 drawPos = center - Main.screenPosition + new Vector2(0, -16 + PWingFakePlayer.Height / 2);
                    positions.Add(drawPos);
                    rotations.Add(velo.ToRotation() - MathHelper.ToRadians(90));
                    scale -= 0.0725f;
                }
                if (outLine)
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            Vector2 circular = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(90 * k));
                            drawInfo.DrawDataCache.Add(new DrawData(textureOutline, positions[i] + circular, new Rectangle(0, 0, texture.Width, texture.Height), DrawColor(fakePlayer), rotations[i], origin, (1 - i * 0.08f), fakePlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0));
                        }
                    }
                }
                else
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        drawInfo.DrawDataCache.Add(new DrawData(texture, positions[i], new Rectangle(0, 0, texture.Width, texture.Height), DrawColor(fakePlayer), rotations[i], origin, 1 - i * 0.08f, fakePlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0));
                    }
                    if (fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Subspace)
                        {
                            Texture2D textureInner = PWing.Instance.Assets.Request<Texture2D>("Content/FakePlayer/SubspaceServantTailScales").Value;
                        for (int i = 8; i >= 0; i--)
                        {
                            drawInfo.DrawDataCache.Add(new DrawData(textureInner, positions[i], new Rectangle(0, 0, texture.Width, texture.Height), DrawColor(fakePlayer), rotations[i], origin, 1 - i * 0.08f, fakePlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0));
                        }
                    }
                }
            }
        }

        public static Vector2 GetCompositeOffset_FrontArm(ref PlayerDrawSet drawInfo)
        {
            return new Vector2(-5 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 0f);
        }

        public static Vector2 GetCompositeOffset_FrontArm(PWingFakePlayer fakePlayer)
        {
            return new Vector2(-5 * ((!fakePlayer.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 0f);
        }

        public static void DrawFrontArm(PWingFakePlayer fakePlayer, ref PlayerDrawSet drawInfo, bool outline)
        {
            Texture2D texture = FakePlayerTexture(fakePlayer.FakePlayerType, false, fakePlayer.OverrideUseSlot % 10);
            Texture2D textureGreen = FakePlayerTexture(fakePlayer.FakePlayerType, true, fakePlayer.OverrideUseSlot % 10);
            SpriteEffects spriteEffects = drawInfo.playerEffect;
            Vector2 vector = new Vector2((int)drawInfo.Position.X, (int)drawInfo.Position.Y) - Main.screenPosition + new Vector2(PWingFakePlayer.Width / 2, PWingFakePlayer.Height / 2 - 3);
            if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7)
            {
                vector += new Vector2((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1), (!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1));
            }
            Vector2 origin = drawInfo.bodyVect;
            Vector2 position = vector + GetCompositeOffset_FrontArm(ref drawInfo);
            Color color = DrawColor(fakePlayer);
            Rectangle frame = drawInfo.compFrontArmFrame;
            float rotation = drawInfo.compositeFrontArmRotation;
            if (outline)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 circular = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(90 * k));
                    DrawData drawData2 = new DrawData(textureGreen, position + circular, frame, color, rotation, origin + GetCompositeOffset_FrontArm(ref drawInfo), 1f, spriteEffects, 0);
                    drawInfo.DrawDataCache.Add(drawData2);
                }
            }
            else
            {
                DrawData drawData = new DrawData(texture, position, frame, color, rotation, origin + GetCompositeOffset_FrontArm(ref drawInfo), 1f, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }

        public static void DrawBody(PWingFakePlayer fakePlayer, ref PlayerDrawSet drawInfo, bool outline)
        {
            Texture2D texture = FakePlayerTexture(fakePlayer.FakePlayerType, false, fakePlayer.OverrideUseSlot % 10);
            Texture2D textureGreen = FakePlayerTexture(fakePlayer.FakePlayerType, true, fakePlayer.OverrideUseSlot % 10);
            Player drawPlayer = drawInfo.drawPlayer;
            float drawX = (int)drawInfo.Position.X + PWingFakePlayer.Width / 2;
            float drawY = (int)drawInfo.Position.Y + PWingFakePlayer.Height - drawPlayer.bodyFrame.Height / 2 + 4f;
            Vector2 origin = drawInfo.bodyVect;
            Vector2 position = new Vector2(drawX, drawY) - Main.screenPosition;
            Color color = DrawColor(fakePlayer);
            Rectangle frame = new Rectangle(0, 0, 40, 56);
            SpriteEffects spriteEffects = drawInfo.playerEffect;
            if (outline)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 circular = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(90 * k));
                    DrawData drawData2 = new DrawData(textureGreen, position + circular, frame, color, 0f, origin, 1f, spriteEffects, 0);
                    drawInfo.DrawDataCache.Add(drawData2);
                }
            }
            else
            {
                DrawData drawData = new DrawData(texture, position, frame, color, 0f, origin, 1f, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }

        public static void DrawWings(PWingFakePlayer fakePlayer, ref PlayerDrawSet drawInfo, int Frame, bool outline)
        {
            if (fakePlayer.FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                return;
            int Direction = drawInfo.drawPlayer.direction;
            Texture2D texture = WingTexture(fakePlayer.FakePlayerType, false);
            Texture2D textureOutline = WingTexture(fakePlayer.FakePlayerType, true);
            Vector2 drawPos = new Vector2((int)drawInfo.Position.X, (int)drawInfo.Position.Y) + new Vector2(PWingFakePlayer.Width / 2, PWingFakePlayer.Height / 2) - Main.screenPosition + new Vector2(-8 * Direction, -5);
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 6 / 2);
            if (fakePlayer.FakePlayerType == 1)
            {
                drawPos += new Vector2(1 * Direction, 3);
            }
            if (Frame < 0)
            {
                Frame = 0;
            }
            if (Frame > 5)
            {
                Frame = 5;
            }
            if (outline)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 circular = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(90 * k));
                    DrawData drawData2 = new DrawData(textureOutline, drawPos + circular, new Rectangle(0, Frame * texture.Height / 6, texture.Width, texture.Height / 6), DrawColor(fakePlayer), 0, origin, 1f, Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                    drawInfo.DrawDataCache.Add(drawData2);
                }
            }
            else
            {
                DrawData drawData = new DrawData(texture, drawPos, new Rectangle(0, Frame * texture.Height / 6, texture.Width, texture.Height / 6), DrawColor(fakePlayer), 0, origin, 1f, Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }

        public static void SetupCompositeDrawing(ref PlayerDrawSet drawInfo, PWingFakePlayer fakePlayer, Player player)
        {
            drawInfo.drawPlayer = player;
            drawInfo.drawPlayer.compositeBackArm = fakePlayer.compositeBackArm;
            drawInfo.drawPlayer.compositeFrontArm = fakePlayer.compositeFrontArm;
            drawInfo.drawPlayer.bodyFrame = fakePlayer.bodyFrame;
            drawInfo.drawPlayer.Male = true;
            drawInfo.drawPlayer.selectedItem = fakePlayer.UseItemSlot(player);
            drawInfo.drawPlayer.lastVisualizedSelectedItem = player.inventory[drawInfo.drawPlayer.selectedItem];
            drawInfo.drawPlayer.heldProj = fakePlayer.HeldProj;
            drawInfo.Position = fakePlayer.Position;
            drawInfo.BoringSetup(player, new List<DrawData>(), new List<int>(), new List<int>(), Vector2.Zero, 0f, 0f, Vector2.Zero);
            fakePlayer.heldProjOverHand = drawInfo.heldProjOverHand;
            fakePlayer.compFrontArmFrame = drawInfo.compFrontArmFrame;
            fakePlayer.compBackArmFrame = drawInfo.compBackArmFrame;
            fakePlayer.compositeFrontArmRotation = drawInfo.compositeFrontArmRotation;
            fakePlayer.compositeBackArmRotation = drawInfo.compositeBackArmRotation;
            fakePlayer.weaponDrawOrder = (int)drawInfo.weaponDrawOrder;
            if (drawInfo.weaponOverFrontArm || Terraria.Item.claw[drawInfo.heldItem.type])
            {
                fakePlayer.weaponDrawOrder = 2;
            }
            fakePlayer.bodyVect = drawInfo.bodyVect;
        }

        private static SpriteDrawBuffer spriteBuffer;

        public static void DrawFromCache(ref PlayerDrawSet drawinfo)
        {
            List<DrawData> drawDataCache = drawinfo.DrawDataCache;
            if (spriteBuffer == null)
            {
                spriteBuffer = new SpriteDrawBuffer(Main.graphics.GraphicsDevice, 200);
            }
            else
            {
                spriteBuffer.CheckGraphicsDevice(Main.graphics.GraphicsDevice);
            }
            foreach (DrawData item in drawDataCache)
            {
                if (item.texture != null)
                {
                    item.Draw(spriteBuffer);
                }
            }
            spriteBuffer.UploadAndBind();
            DrawData cdd;
            int num = 0;
            for (int i = 0; i <= drawDataCache.Count; i++)
            {
                if (i != drawDataCache.Count)
                {
                    cdd = drawDataCache[i];
                    if (!cdd.sourceRect.HasValue)
                    {
                        cdd.sourceRect = cdd.texture.Frame();
                    }
                    PlayerDrawHelper.SetShaderForData(drawinfo.drawPlayer, drawinfo.cHead, ref cdd);
                    if (cdd.texture != null)
                    {
                        spriteBuffer.DrawSingle(num++);
                    }
                }
            }
            spriteBuffer.Unbind();
        }

        public static List<PWingFakePlayer> GetFakePlayers(Player player)
        {
            List<PWingFakePlayer> ret = new List<PWingFakePlayer>();
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active)
                {
                    if (proj.ModProjectile is PWingFakePlayerPossessingProjectile fppp && proj.owner == player.whoAmI)
                    {
                        ret.Add(fppp.FakePlayer);
                    }
                }
            }
            return ret;
        }

        public static void DrawFakePlayers(int fakePlayerType, int DrawState)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    DrawMyFakePlayers(player, fakePlayerType, DrawState);
                }
            }
        }

        public static void DrawMyFakePlayers(Player player, int drawType, int drawState)
        {
            if (player == null)
                return;
            List<PWingFakePlayer> fakePlayers = GetFakePlayers(player);
            if (fakePlayers == null || fakePlayers.Count <= 0)
                return;
            for (int i = 0; i < fakePlayers.Count; i++)
            {
                PWingFakePlayer fakePlayer = fakePlayers[i];
                if (fakePlayer == null)
                    continue;
                PlayerDrawSet drawInfo = new PlayerDrawSet();
                if (fakePlayer.FakePlayerType == drawType)
                {
                    bool canIDraw = fakePlayer.PrepareDrawing(ref drawInfo, player, drawState);
                    if (canIDraw)
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                        DrawFromCache(ref drawInfo);
                        fakePlayer.DrawFrontHandAndHeldProj(Main.spriteBatch, player, drawState);
                    }
                }
            }
        }
    }
}
