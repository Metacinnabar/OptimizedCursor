using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor;

public partial class OptimizedCursor : Mod
{
    public void LoadSmartHandlingInjections()
    {
        IL.Terraria.Player.Update += StsfldSmartCursorWanted;
    }
    
    private void StsfldSmartCursorWanted(ILContext il)
    {
        // Create a new ILCursor to navigate the IL code
        var c = new ILCursor(il);

        // Find the first occurrence of the Main.SmartCursorWanted field being set in the method's IL code
        if (c.TryGotoNext(MoveType.After,
                // Look for a stsfld instruction that sets Main.SmartCursorWanted
                instruction => instruction.MatchStsfld(typeof(Main).GetField(nameof(Main.SmartCursorWanted)))))
        {
            // Inject a call to the UpdateCursor method at this point in the IL code
            c.EmitDelegate(UpdateCursor);
        }
    }
}