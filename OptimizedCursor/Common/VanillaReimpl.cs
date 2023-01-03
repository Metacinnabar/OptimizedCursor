using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI.Gamepad;

namespace OptimizedCursor.Common;

public class VanillaReimpl
{
    public static void DrawColoredCursor(Vector2 bonus, bool smart)
    {
        if (Main.gameMenu && Main.alreadyGrabbingSunOrMoon)
            return;

        if (Main.player[Main.myPlayer].dead || Main.player[Main.myPlayer].mouseInterface)
        {
            Main.ClearSmartInteract();
            Main.TileInteractionLX = Main.TileInteractionHX = Main.TileInteractionLY = Main.TileInteractionHY = -1;
        }

        var color = Main.cursorColor;
        
        if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
            color = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f);

        var cursorOverride = CursorOverrideID.DefaultCursor;
        
        if (!PlayerInput.SettingsForUI.ShowGamepadCursor)
        {
            if (smart)
                cursorOverride = CursorOverrideID.SmartCursor;
            
            Main.spriteBatch.Draw(TextureAssets.Cursors[cursorOverride].Value,
                bonus + Vector2.One,
                null,
                new Color((int)(color.R * 0.2f), (int)(color.G * 0.2f), (int)(color.B * 0.2f), (int)(color.A * 0.5f)),
                0f, 
                default, 
                Main.cursorScale * 1.1f, 
                SpriteEffects.None,
                0f);
            
            Main.spriteBatch.Draw(TextureAssets.Cursors[cursorOverride].Value,
                bonus, 
                null, 
                color, 
                0f, 
                default,
                Main.cursorScale, 
                SpriteEffects.None, 
                0f);
            
            return;
        }

        /* gamepad shit I cannot be bothered to make sense of
        if ((Main.player[Main.myPlayer].dead && !Main.player[Main.myPlayer].ghost && !Main.gameMenu) ||
            PlayerInput.InvisibleGamepadInMenus)
            return;
        
        var t = Vector2.Zero;
        var t2 = Vector2.Zero;
        var flag2 = Main.SmartCursorIsUsed;
        if (flag2)
        {
            PlayerInput.smartSelectPointer.UpdateCenter(Main.ScreenSize.ToVector2() / 2f);
            t2 = PlayerInput.smartSelectPointer.GetPointerPosition();
            if (Vector2.Distance(t2, t) < 1f)
            {
                flag2 = false;
            }
            else
            {
                Utils.Swap(ref t, ref t2);
            }
        }

        float scale = 1f;
        if (flag2)
        {
            scale = 0.3f;
            color = Color.White * Main.GamepadCursorAlpha;
            int num5 = 17;
            int frameX = 0;
            Main.spriteBatch.Draw(TextureAssets.Cursors[num5].Value, t2 + bonus,
                TextureAssets.Cursors[num5].Frame(1, 1, frameX), color,
                1.5707964f * Main.GlobalTimeWrappedHourly,
                TextureAssets.Cursors[num5].Frame(1, 1, frameX).Size() / 2f, Main.cursorScale,
                SpriteEffects.None, 0f);
        }

        if (smart && !(UILinkPointNavigator.Available && !PlayerInput.InBuildingMode))
        {
            color = Color.White * Main.GamepadCursorAlpha * scale;
            int num6 = 13;
            int frameX2 = 0;
            Main.spriteBatch.Draw(TextureAssets.Cursors[num6].Value, t + bonus,
                TextureAssets.Cursors[num6].Frame(2, 1, frameX2), color, 0f,
                TextureAssets.Cursors[num6].Frame(2, 1, frameX2).Size() / 2f, Main.cursorScale,
                SpriteEffects.None, 0f);
            return;
        }

        color = Color.White;
        int num7 = 15;
        Main.spriteBatch.Draw(TextureAssets.Cursors[num7].Value,
            new Vector2(Main.mouseX, Main.mouseY) + bonus, null, color, 0f,
            TextureAssets.Cursors[num7].Value.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
        */
    }

    public static Vector2 DrawColoredCursorBorder(bool smart)
    {
        if (!Main.ThickMouse)
            return Vector2.Zero;

        if (Main.gameMenu && Main.alreadyGrabbingSunOrMoon)
            return Vector2.Zero;

        var showGamepadCursor = PlayerInput.SettingsForUI.ShowGamepadCursor;

        switch (showGamepadCursor)
        {
            case true when PlayerInput.InvisibleGamepadInMenus:
            case true when Main.player[Main.myPlayer].dead && !Main.player[Main.myPlayer].ghost && !Main.gameMenu:
                return Vector2.Zero;
        }

        int cursorOverride = smart ? CursorOverrideID.SmartCursorOutline : CursorOverrideID.DefaultCursorOutline;
        var mouseBorderColor = Main.MouseBorderColor;
        var origin = new Vector2(2f);
        Rectangle? sourceRectangle = null;
        var scale = Main.cursorScale * 1.1f;

        for (var i = 0; i < 4; i++)
        {
            var offset = i switch
            {
                0 => new Vector2(0f, 1f),
                1 => new Vector2(1f, 0f),
                2 => new Vector2(0f, -1f),
                3 => new Vector2(-1f, 0f),
                _ => Vector2.Zero
            };

            offset *= 1f;
            offset += Vector2.One * 2f;

            if (showGamepadCursor)
            {
                if (smart && !(UILinkPointNavigator.Available && !PlayerInput.InBuildingMode))
                {
                    cursorOverride = CursorOverrideID.GamepadSmartCursor;
                    offset = Vector2.One;
                    sourceRectangle = TextureAssets.Cursors[cursorOverride].Frame(2);
                    origin = TextureAssets.Cursors[cursorOverride].Frame(2).Size() / 2f;
                    mouseBorderColor *= Main.GamepadCursorAlpha;
                }
                else
                {
                    cursorOverride = CursorOverrideID.GamepadDefaultCursor;
                    offset = Vector2.One;
                    origin = TextureAssets.Cursors[cursorOverride].Value.Size() / 2f;
                }
            }

            Main.spriteBatch.Draw(TextureAssets.Cursors[cursorOverride].Value,
                offset, sourceRectangle, mouseBorderColor, 0f, origin,
                scale, SpriteEffects.None, 0f);
        }

        return new Vector2(2f);
    }

    public static void DrawCursorOverride()
    {
        var border = true;
        var color = Main.cursorColor;
        var scale = 1f;
        var offset = Vector2.Zero;

        switch (Main.cursorOverride)
        {
            case CursorOverrideID.Magnifiers:
                border = false;
                color = Color.White;
                scale = 0.7f;
                offset = new Vector2(0.1f);
                break;
            case CursorOverrideID.FavoriteStar:
            case CursorOverrideID.TrashCan:
            case CursorOverrideID.BackInventory:
            case CursorOverrideID.ChestToInventory:
            case CursorOverrideID.InventoryToChest:
            case CursorOverrideID.QuickSell:
                border = false;
                color = Color.White;
                break;
        }

        if (border)
        {
            var borderColor = new Color((int)(color.R * 0.2f), (int)(color.G * 0.2f), (int)(color.B * 0.2f),
                (int)(color.A * 0.5f));

            Main.spriteBatch.Draw(TextureAssets.Cursors[Main.cursorOverride].Value,
                Vector2.One,
                null,
                borderColor,
                0f,
                offset * TextureAssets.Cursors[Main.cursorOverride].Value.Size(),
                Main.cursorScale * 1.1f * scale,
                SpriteEffects.None,
                0f);
        }

        Main.spriteBatch.Draw(TextureAssets.Cursors[Main.cursorOverride].Value,
            Vector2.Zero,
            null,
            color,
            0f,
            offset * TextureAssets.Cursors[Main.cursorOverride].Value.Size(),
            Main.cursorScale * scale,
            SpriteEffects.None,
            0f);
    }
}