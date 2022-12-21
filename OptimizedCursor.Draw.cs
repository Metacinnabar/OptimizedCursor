using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor;

public partial class OptimizedCursor : Mod
{
    public unsafe void SetCursor(Texture2D texture)
    {
        // Declare width and height variables and assign them the width and height of the texture
        var width = texture.Width;
        var height = texture.Height;

        // Initialize a Color array with the size of the width times the height of the texture
        var pixelData = new Color[width * height];

        // Get the data of the texture and assign it to the pixelData array
        texture.GetData(pixelData);

        // Pin the pixelData array in memory so that the address of the first element can be taken
        fixed (Color* p = &pixelData[0])
        {
            // Create an IntPtr using the address of the first element of the pixelData array
            var pixelDataPtr = new IntPtr(p);

            // Create a new surface using the pixelDataPtr, width, height, and the specified format
            var surfacePtr = SDL.SDL_CreateRGBSurfaceFrom(
                pixelDataPtr,
                width,
                height,
                8 * 4, // length of bits in a byte, times 4 bytes (r, g, b, a)
                4 * width,
                0x00FF0000,
                0x0000FF00,
                0x000000FF,
                0xFF000000
            );

            // Create a new cursor from the surfacePtr
            var cursor = SDL.SDL_CreateColorCursor(surfacePtr, 0, 0);

            // Set the cursor
            SDL.SDL_SetCursor(cursor);
            if (SDL.SDL_GetError() != string.Empty)
            {
                Logger.ErrorFormat("SDL_SetCursor threw an error: {0}", SDL.SDL_GetError());
            }
        }
    }

    public unsafe void SetCursor(Texture2D cursorTexture, Texture2D outlineTexture)
    {
        // Declare width and height variables and add 10 to each to allow for a border around the cursor
        var width = outlineTexture.Width + 10;
        var height = outlineTexture.Height + 10;

        // Initialize a new render target with the specified width and height
        var renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height);
        // Initialize a new sprite batch
        var spriteBatch = new SpriteBatch(Main.graphics.GraphicsDevice);

        // Set the graphics device's render target to the renderTarget
        Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
        // Clear the render target with a transparent color
        Main.graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));

        // Begin a new sprite batch
        spriteBatch.Begin();
        // Call the DrawCursor method to draw the cursor and outline textures to the sprite batch
        DrawCursor(cursorTexture, outlineTexture, spriteBatch);
        // End the sprite batch
        spriteBatch.End();

        // Reset the graphics device's render target to null
        Main.graphics.GraphicsDevice.SetRenderTarget(null);

        // Initialize a Color array with the size of the width times the height of the render target
        var pixelData = new Color[renderTarget.Width * renderTarget.Height];
        // Get the data from the render target and assign it to the Color array
        renderTarget.GetData(pixelData);

        // Pin the pixelData array in memory so that the address of the first element can be taken
        fixed (Color* p = &pixelData[0])
        {
            // Create an IntPtr using the address of the first element of the pixelData array
            var pixelDataPtr = new IntPtr(p);

            // Create a new surface using the pixelDataPtr, width, height, and the specified format
            var surfacePtr = SDL.SDL_CreateRGBSurfaceFrom(
                pixelDataPtr,
                width,
                height,
                8 * 4, // length of bits in a byte, times 4 bytes (r, g, b, a)
                4 * width,
                0x00FF0000,
                0x0000FF00,
                0x000000FF,
                0xFF000000
            );

            // Create a new cursor from the surfacePtr
            var cursor = SDL.SDL_CreateColorCursor(surfacePtr, 0, 0);

            // Set the operating system cursor to the specified cursor
            SDL.SDL_SetCursor(cursor);
            // Check if SDL_SetCursor threw an error
            if (SDL.SDL_GetError() != string.Empty)
            {
                // Log the error message using the specified format string and the error message from SDL_GetError
                Logger.ErrorFormat("SDL_SetCursor threw an error: {0}", SDL.SDL_GetError());
            }
        }
    }

    // combination of DrawThickCursor (outline) and DrawCursor from vanilla.
    public static void DrawCursor(Texture2D cursorTexture, Texture2D outlineTexture, SpriteBatch spriteBatch)
    {
        // Loop through the values 0 to 3
        for (var i = 0; i < 4; i++)
        {
            // Declare offset vector and assign it a value based on the current iteration of the loop
            var offset = i switch
            {
                0 => new Vector2(0f, 1f),
                1 => new Vector2(1f, 0f),
                2 => new Vector2(0f, -1f),
                3 => new Vector2(-1f, 0f),
                _ => Vector2.Zero
            };

            // Multiply the offset vector by 1f
            offset *= 1f;
            // Add a vector with a value of 2f to the offset vector
            offset += Vector2.One * 2f;

            // Draw the outline texture to the sprite batch using the specified position, color, rotation, scale, and effects
            spriteBatch.Draw(outlineTexture,
                //new Vector2(Main.mouseX, Main.mouseY) + offset,
                offset,
                null,
                new Color(Main.MouseBorderColor.B, Main.MouseBorderColor.G, Main.MouseBorderColor.R),
                //Main.MouseBorderColor,
                0f,
                new Vector2(2f),
                Main.cursorScale * 1.1f,
                SpriteEffects.None,
                0f);
        }

        // Declares a new 2D vector called bonus with a value of 2f
        var bonus = new Vector2(2f);

        // Declares a new Color variable called color with a value of Main.cursorColor with the red, green, and blue values swapped
        // Apparently low level shit has these swapped, I never actually figured out why these need to be swapped
        // but they do, if you don't want a green or blue cursor constantly.
        var color = new Color(Main.cursorColor.B, Main.cursorColor.G, Main.cursorColor.R);

        // If Main.gameMenu is false and Main.LocalPlayer has a rainbow cursor, then set color to a new color using Main.hslToRgb
        if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
        {
            color = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f);
        }

        // Declares a new Color variable called colorOffset with a value of a new color with red, green, and blue values equal to 20% of the corresponding values in color and an alpha value of 50% of the alpha value in color
        var colorOffset = new Color((int)(color.G * 0.2f), (int)(color.B * 0.2f), (int)(color.R * 0.2f),
            (int)(color.A * 0.5f));

        // Draws cursorTexture using colorOffset as the color, with the position offset by bonus plus one, a rotation of 0, a scale of Main.cursorScale times 1.1f, and no sprite effects
        spriteBatch.Draw(
            cursorTexture,
            Vector2.Zero + bonus + Vector2.One,
            null,
            colorOffset,
            0f,
            default,
            Main.cursorScale * 1.1f,
            SpriteEffects.None,
            0f);

        // Draws cursorTexture using color as the color, with the position offset by bonus, a rotation of 0, a scale of Main.cursorScale, and no sprite effects
        spriteBatch.Draw(
            cursorTexture,
            Vector2.Zero + bonus,
            null,
            color,
            0f,
            default,
            Main.cursorScale,
            SpriteEffects.None,
            0f);
    }
}