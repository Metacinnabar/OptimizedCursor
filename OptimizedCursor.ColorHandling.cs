using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor;

public partial class OptimizedCursor : Mod
{
    public void LoadColorHandlingInjections()
    {
        // Inject the StsfldMouseColorUpdate method into the Draw method of the IngameOptions class
        IL.Terraria.IngameOptions.Draw += StsfldMouseColorUpdate;
        // Inject the StsfldMouseColorUpdate method into the DrawMenu method of the Main class
        IL.Terraria.Main.DrawMenu += StsfldMouseColorUpdate;
    }

    private void StsfldMouseColorUpdate(ILContext il)
    {
        // Create a cursor for iterating through the instructions in the method
        var c = new ILCursor(il);

        // If the cursor finds a "stsfld" instruction that stores a value in the MouseBorderColor field of the Main class
        if (c.TryGotoNext(instruction =>
                instruction.MatchStsfld(typeof(Main).GetField(nameof(Main.MouseBorderColor)))))
        {
            // Emit a delegate that calls the UpdateCursor method to update the cursor based on the new value of the MouseBorderColor field
            c.EmitDelegate(UpdateCursor);
            // Increment the cursor to move to the next instruction
            c.Index++;
        }

        // Reset the cursor to the beginning of the method
        c.Goto(0);

        // If the cursor finds a "stsfld" instruction that stores a value in the mouseColor field of the Main class
        if (c.TryGotoNext(instruction =>
                instruction.MatchStsfld(typeof(Main).GetField(nameof(Main.mouseColor)))))
        {
            // Emit a delegate that calls the UpdateCursor method to update the cursor based on the new value of the mouseColor field
            c.EmitDelegate(UpdateCursor);
            // Increment the cursor to move to the next instruction
            c.Index++;
        }
    }
}