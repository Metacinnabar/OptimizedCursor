// This mod completely changes how the game shows it's cursor, however it still
// includes support for cursor outlines, colors, smart cursor, and special cursors such as trash and favorite.
// Bug: where the big cursor still exists if toggled and leaving the world (this bug may also exist in vanilla Terraria)
// Note: this code has not been tested with a gamepad and does not provide support for the screen capture menu.
// Note: and has also not been tested with the rainbow cursor.

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OptimizedCursor;

public partial class OptimizedCursor : Mod
{
    public override void Load()
    {
        // Load various injections for different features of the mod
        LoadVanillaInjects();
        LoadColorHandlingInjections();
        LoadOverrideHandlingInjections();
        LoadSmartHandlingInjections();
    }

    public override void PostSetupContent()
    {
        // Make the mouse cursor visible
        Main.QueueMainThreadAction(() => { Main.instance.IsMouseVisible = true; });

        // Update the cursor to reflect any changes made by the mod
        UpdateCursor();
    }

    public void UpdateCursor()
    {
        // If the cursor override has not been set
        if (Main.cursorOverride == -1)
        {
            // If the smart cursor is wanted, set the cursor to the smart cursor and smart cursor outline textures
            if (Main.SmartCursorWanted)
            {
                Main.QueueMainThreadAction(() =>
                {
                    SetCursor(
                        TextureAssets.Cursors[CursorOverrideID.SmartCursor].Value,
                        TextureAssets.Cursors[CursorOverrideID.SmartCursorOutline].Value);
                });
            }
            // Otherwise, set the cursor to the default cursor and default cursor outline textures
            else
            {
                Main.QueueMainThreadAction(() =>
                {
                    SetCursor(
                        TextureAssets.Cursors[CursorOverrideID.DefaultCursor].Value,
                        TextureAssets.Cursors[CursorOverrideID.DefaultCursorOutline].Value);
                });
            }

            // Return early to avoid setting the cursor to a special cursor texture
            return;
        }

        // Set the cursor to the texture specified by the cursor override
        SetCursor(TextureAssets.Cursors[Main.cursorOverride].Value);
    }
}