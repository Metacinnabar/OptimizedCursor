using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OptimizedCursor.Common.Systems;

public class CursorRenderSystem : ModSystem
{
    private RenderTarget2D _cursorRenderTarget = null!;
    private static MouseCursor _mouseCursor;
    private IntPtr _lastCursor;
    private bool _initialized;

    // estimation
    private const int CursorSize = 30;

    public override void Load()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Main.QueueMainThreadAction(() =>
        {
            _cursorRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, CursorSize, CursorSize);
            resetEvent.Set();
        });
        
        // wait until the code above is executed to proceed
        resetEvent.Wait();
        _initialized = true;

        On.Terraria.Main.Draw += Draw;
    }
    
    public override void Unload()
    {
        // I am not exactly sure what here is necessary
        On.Terraria.Main.Draw -= Draw;
        
        Main.QueueMainThreadAction(() =>
        {
            // dispose all resources
            _cursorRenderTarget.Dispose();
            // set all resources to null so they can be garbage collected
            _cursorRenderTarget = null!;
        });
        
        // free cursor memory
        if (_lastCursor == IntPtr.Zero) 
            return;
        
        SDL.SDL_FreeCursor(_lastCursor);
        _lastCursor = IntPtr.Zero;
    }
    
    public override void PostSetupContent()
    {
        // Make the SDL mouse cursor visible
        Main.QueueMainThreadAction(() =>
        {
            Main.instance.IsMouseVisible = true;
        });
    }

    private void LoadCursorRenderTarget()
    {
        Main.graphics.GraphicsDevice.SetRenderTarget(_cursorRenderTarget);
        Main.graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
        // taken from Main.DrawInterface_36_Cursor()
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor,
            DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

        if (Main.cursorOverride == -1)
            VanillaReimpl.DrawColoredCursor(
                VanillaReimpl.DrawColoredCursorBorder(Main.SmartCursorIsUsed), Main.SmartCursorIsUsed);
        else
        {
            VanillaReimpl.DrawCursorOverride();
            // reset cursorOverride for the next frame
            Main.cursorOverride = -1;
        }

        Main.spriteBatch.End();
        Main.graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private void Draw(On.Terraria.Main.orig_Draw orig, Main self, GameTime gameTime)
    {
        // Load render targets before drawing anything.
        LoadCursorRenderTarget();

        // begin drawing
        orig(self, gameTime);

        if (!_initialized)
            return;

        // initialize the renderTarget
        var pixelData = new Color[_cursorRenderTarget.Width * _cursorRenderTarget.Height];
        _cursorRenderTarget.GetData(pixelData);

        // set the cursor
        
        var cursor = MouseCursor.FromTexture2D(_cursorRenderTarget, 0, 0);
        SDL.SDL_SetCursor(cursor.Handle);
        // out with the old, in with the new
        _mouseCursor?.Dispose();
        _mouseCursor = cursor;

        // free memory
        if (_lastCursor != IntPtr.Zero)
        {
            SDL.SDL_FreeCursor(_lastCursor);
            _lastCursor = IntPtr.Zero;
        }
        
        _lastCursor = SDL.SDL_GetCursor();
    }
}