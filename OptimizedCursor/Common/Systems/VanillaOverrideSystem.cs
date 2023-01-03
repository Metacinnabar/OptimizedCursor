using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor.Common.Systems;

public class VanillaOverrideSystem : ModSystem
{
    public override void Load()
    {
        // allow for showing the SDL cursor
        IL.Terraria.Main.DoUpdate += MainOnDoUpdate;
        // remove vanilla cursor drawing
        IL.Terraria.Main.DrawCursor += ReturnAtStart;
        IL.Terraria.Main.DrawThickCursor += MainOnDrawThickCursor;
        IL.Terraria.Main.DrawInterface_36_Cursor += ReturnAtStart;
        // allow for our renderTarget to process cursor overrides
        IL.Terraria.Main.DrawInterface_41_InterfaceLogic4 += MainOnDrawInterface_41_InterfaceLogic4;
        // fix vanilla bug of having smart cursor in main menu
        IL.Terraria.WorldGen.SaveAndQuitCallBack += WorldGenOnSaveAndQuitCallBack;
    }

    public override void Unload()
    {
        // unsubscribe from all the IL
        IL.Terraria.Main.DoUpdate -= MainOnDoUpdate;
        IL.Terraria.Main.DrawCursor -= ReturnAtStart;
        IL.Terraria.Main.DrawThickCursor -= MainOnDrawThickCursor;
        IL.Terraria.Main.DrawInterface_36_Cursor -= ReturnAtStart;
        IL.Terraria.Main.DrawInterface_41_InterfaceLogic4 -= MainOnDrawInterface_41_InterfaceLogic4;
        IL.Terraria.WorldGen.SaveAndQuitCallBack -= WorldGenOnSaveAndQuitCallBack;
    }

    private static void ReturnAtStart(ILContext il)
    {
        var c = new ILCursor(il);
        c.Emit(OpCodes.Ret);
    }

    private static void MainOnDoUpdate(ILContext il)
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

    private static void MainOnDrawThickCursor(ILContext il)
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
    }

    private static void MainOnDrawInterface_41_InterfaceLogic4(ILContext il)
    {
        // remove the Main.cursorOverride = -1; at the bottom of the method
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before, i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.cursorOverride)))))
            return;

        // next pos is stsfld    int32 Terraria.Main::cursorOverride
        c.Index--;
        // next pos is ldc.i4.m1

        // remove ldc.i4.m1, next pos is stsfld    int32 Terraria.Main::cursorOverride
        c.Remove();
        // remove stsfld    int32 Terraria.Main::cursorOverride, next pos is ret
        c.Remove();
    }

    private static void WorldGenOnSaveAndQuitCallBack(ILContext il)
    {
        // reset smartCursor when exiting back to main menu
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(1),
                i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.gameMenu)))))
            return;

        // next pos is call      void Terraria.Audio.SoundEngine::StopTrackedSounds()
        c.EmitDelegate(() => { Main.SmartCursorWanted = false; });
    }
}