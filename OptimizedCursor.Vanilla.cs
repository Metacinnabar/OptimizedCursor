using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace OptimizedCursor;

// didn't know what to name this file
public partial class OptimizedCursor : Mod
{
    public void LoadVanillaInjects()
    {
        RemoveCursorDrawing();
        IL.Terraria.Main.DoUpdate += ShowOriginalCursor;
    }

    public void RemoveCursorDrawing()
    {
        // Render the method completely useless by returning at the start of it.
        IL.Terraria.Main.DrawCursor += il =>
        {
            var c = new ILCursor(il);
            c.Emit(OpCodes.Ret);
        };

        // DrawThickCursor is always called inside of DrawCursor as the first argument,
        // except for one reference inside of Utils.DrawCursorSingle.
        // This means, if DrawCursor and DrawCursorSingle is not in use, the return value here means nothing.
        IL.Terraria.Main.DrawThickCursor += il =>
        {
            var c = new ILCursor(il);

            /* return new Vector(2f)
            // ldc.r4    2
            // newobj    instance void [FNA]Microsoft.Xna.Framework.Vector2::.ctor(float32)
            // ret
            
            var vectorConstructor = typeof(Vector2).GetConstructor(new[] { typeof(float) });
            c.Emit(OpCodes.Ldc_R4, 2f); // bonus
            c.Emit(OpCodes.Newobj, vectorConstructor);
            */

            // return Vector2.Zero
            c.Emit(OpCodes.Ldsfld,
                typeof(Vector2).GetField("zeroVector", BindingFlags.NonPublic | BindingFlags.Static));
            c.Emit(OpCodes.Ret);
        };

        // Only called when taking screen captures from the in-game system.
        // Render the method completely useless by returning at the start of it.
        IL.Terraria.Utils.DrawCursorSingle += il =>
        {
            var c = new ILCursor(il);

            /* debug injection
            // SpriteBatch sb, Color color, float rot = float.NaN, float scale = 1f, Vector2 manualPosition = default(Vector2), int cursorSlot = 0, int specialMode = 0
            
            // push cursorSlot onto the stack
            c.Emit(OpCodes.Ldarg, 5);

            // emit code with the cursorSlot variable.
            c.EmitDelegate<Action<int>>((cursorSlot) =>
            {
                Main.NewText("cursorSlot: " + cursorSlot);
            });*/

            c.Emit(OpCodes.Ret);
        };

        // Render the method completely useless by returning at the start of it.
        IL.Terraria.Main.DrawInterface_36_Cursor += il =>
        {
            var c = new ILCursor(il);
            c.Emit(OpCodes.Ret);
        };
    }

    private void ShowOriginalCursor(ILContext il)
    {
        // Create a new ILCursor to navigate the IL code
        var c = new ILCursor(il);

        // Find up to 3 instances of the Game.IsMouseVisible setter being called in the method's IL code
        for (var i = 0; i <= 2; i++)
        {
            // Move the cursor to the instruction before each occurrence of Game.IsMouseVisible being set
            if (!c.TryGotoNext(MoveType.Before, instruction =>
                    // Look for a call instruction that calls Game.IsMouseVisible's setter
                    instruction.MatchCall(typeof(Game).GetMethod("set_IsMouseVisible"))))
                continue;

            // Move the cursor back to the instruction before the call to Game.IsMouseVisible's setter
            c.Index--;
            // Move the cursor back to the instruction before the value being passed to Game.IsMouseVisible's setter
            c.Index--;
            // Move the cursor back to the instruction before the 'this' argument being passed to Game.IsMouseVisible's setter

            // Remove the instruction loading the 'this' argument
            c.Remove();
            // Remove the instruction loading the value being passed to Game.IsMouseVisible's setter
            c.Remove();
            // Remove the call to Game.IsMouseVisible's setter
            c.Remove();
        }
    }
}