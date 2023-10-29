/*
 * File: MouseCursor.cs
 * Author: ryancheung
 * Modified: true
 * Source: https://github.com/FNA-NET/FNA/blob/79084855a0a1bbaef50fba6764dae70fc6cfc726/src/Input/MouseCursor.cs
 * License: Ms-PL, found at "OptimizedCursor\licenses\MonoGame\LICENSE.txt"

 * LICENSE DISCLAIMER:
 * THE SOFTWARE IS PROVIDED "AS IS," WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace SDLCursorDebug;

public class MouseCursor : IDisposable
{
    public IntPtr Handle { get; set; }

    private bool _disposed;

    private MouseCursor(IntPtr handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
            
        if (Handle != IntPtr.Zero)
            SDL.SDL_FreeCursor(Handle);
            
        Handle = IntPtr.Zero;
            
        _disposed = true;
        GC.SuppressFinalize(this);
    }
        
    public static MouseCursor FromTexture2D(Texture2D texture, int originX, int originY)
    {
        if (texture.Format != SurfaceFormat.Color)
            throw new ArgumentException("Only Color textures are accepted for mouse cursors", nameof(texture));

        var surface = IntPtr.Zero;
        IntPtr handle;
        try
        {
            var bytes = new byte[texture.Width * texture.Height * 4];
            texture.GetData(bytes);

            var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            surface = SDL.SDL_CreateRGBSurfaceFrom(gcHandle.AddrOfPinnedObject(),
                texture.Width,
                texture.Height,
                32,
                texture.Width * 4,
                0x000000ff,
                0x0000FF00,
                0x00FF0000,
                0xFF000000);
                
            gcHandle.Free();
            if (surface == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create surface for mouse cursor: " + SDL.SDL_GetError());

            handle = SDL.SDL_CreateColorCursor(surface, originX, originY);
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to set surface for mouse cursor: " + SDL.SDL_GetError());

            Console.WriteLine("post: " + surface + ", " + handle);
        }
        finally
        {
            if (surface != IntPtr.Zero)
                SDL.SDL_FreeSurface(surface);
        }

        return new MouseCursor(handle);
    }
}