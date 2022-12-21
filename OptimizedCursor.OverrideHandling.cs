using System;
using log4net.Repository.Hierarchy;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor;

public partial class OptimizedCursor : Mod
{
    public void LoadOverrideHandlingInjections()
    {
        IL.Terraria.GameContent.Tile_Entities.TEDisplayDoll.OverrideItemSlotHover += StsfldCursorOverrideUpdate;
        IL.Terraria.GameContent.Tile_Entities.TEHatRack.OverrideItemSlotHover += StsfldCursorOverrideUpdate;
        //IL.Terraria.Main.DrawInterface_41_InterfaceLogic4 += StsfldCursorOverrideUpdate();
        IL.Terraria.UI.ItemSlot.GetGamepadInstructions_ItemArray_int_int += StsfldCursorOverrideUpdate;
        IL.Terraria.UI.ItemSlot.OverrideHover_ItemArray_int_int += StsfldCursorOverrideUpdate;

        // What we do here is check if the method did not change the cursorOverride
        // If it did not, we update the cursor
        // This is to make sure we reset the cursor when we stop holding down Ctrl or Alt
        // This needs to be done everywhere we change the cursorOverride
        On.Terraria.GameContent.Tile_Entities.TEDisplayDoll.OverrideItemSlotHover +=
            (orig, self, inv, context, slot) =>
            {
                // Call the original method
                var value = orig(self, inv, context, slot);
                if (Main.cursorOverride == -1)
                {
                    UpdateCursor();
                }

                return value;
            };

        On.Terraria.GameContent.Tile_Entities.TEHatRack.OverrideItemSlotHover += (orig, self, inv, context, slot) =>
        {
            var value = orig(self, inv, context, slot);
            if (Main.cursorOverride == -1)
            {
                UpdateCursor();
            }

            return value;
        };

        On.Terraria.UI.ItemSlot.GetGamepadInstructions_ItemArray_int_int += (orig, inv, context, slot) =>
        {
            var value = orig(inv, context, slot);
            if (Main.cursorOverride == -1)
            {
                UpdateCursor();
            }

            return value;
        };

        On.Terraria.UI.ItemSlot.OverrideHover_ItemArray_int_int += (orig, inv, context, slot) =>
        {
            orig(inv, context, slot);
            if (Main.cursorOverride == -1)
            {
                UpdateCursor();
            }
        };
    }

    private void StsfldCursorOverrideUpdate(ILContext il)
    {
        // Create a new ILCursor to navigate the IL code
        var c = new ILCursor(il);

        // Find all instances of the Main.cursorOverride field being set in the method's IL code
        for (var i = 1;
             // Move the cursor to the instruction after each occurrence of Main.cursorOverride being set
             c.TryGotoNext(MoveType.After,
                 // Look for a stsfld instruction that sets Main.cursorOverride
                 instruction => instruction.MatchStsfld(typeof(Main).GetField(nameof(Main.cursorOverride))));
             i++)
        {
            // Inject a call to the UpdateCursor method at this point in the IL code
            c.EmitDelegate(UpdateCursor);

            // Move the cursor past the injected call
            c.Index++;

            // Log a debug message indicating that the UpdateCursor method has been injected
            Logger.Debug("Emitted cursorOverride update event for " + il.Method.Name + " (" + i + ")");
        }

        /*
        // This code appears to have been attempted but not completed, and is now commented out
        c.Index = il.Instrs.Count;
        Console.WriteLine("Emitting default check before " + c.Next);
        c.EmitDelegate(() =>
        {
            if (Main.cursorOverride == -1)
            {
                OnCursorOverrideUpdate();
            }
        });
        */
    }
}