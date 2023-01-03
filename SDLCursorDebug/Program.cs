// See https://aka.ms/new-console-template for more information

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace SDLCursorDebug;

public class Program : Game
{
    public readonly GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch = null!;
    
    public static Texture2D WhiteTexture { get; set; } = null!;

    private RenderTarget2D _cursorRenderTarget = null!;
    private IntPtr _lastCursor;
    private const int DebugSquareSize = 10;

    public Program()
    {
        Graphics = new GraphicsDeviceManager(this);
    }

    public static void Main(string[] args)
    {
        // needed to load SDL2.dll?
        Bootstrap.Initialize_FNA();
        
        using var game = new Program();
        game.Run();
    }
    
    protected override void Initialize()
    {
        IsMouseVisible = true;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        WhiteTexture = new Texture2D(GraphicsDevice, width: 1, height: 1);
        WhiteTexture.SetData(new[] { Color.White });
        
        _cursorRenderTarget = new RenderTarget2D(Graphics.GraphicsDevice, DebugSquareSize, DebugSquareSize);
        base.LoadContent();
    }
    
    protected override void UnloadContent()
    {
        SpriteBatch.Dispose();
        WhiteTexture.Dispose();
        _cursorRenderTarget.Dispose();
        base.UnloadContent();
    }
    
    protected override void Draw(GameTime gameTime)
    {
        // Load render targets before drawing anything.
        LoadCursorRenderTarget();
        
        // begin drawing
        GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch.Begin();
        
        // draw to test that the draw code is correct
        DrawRedSquare();

        // initialize the renderTarget
        var pixelData = new Color[_cursorRenderTarget.Width * _cursorRenderTarget.Height];
        _cursorRenderTarget.GetData(pixelData);
        
        // draw to test that the renderTarget is correct
        SpriteBatch.Draw(_cursorRenderTarget, new Vector2(20, 0), Color.White);
        
        var customCursor = MouseCursor.FromTexture2D(_cursorRenderTarget, 0, 0);

        Console.WriteLine("pre set cursor: " + customCursor.Handle);
        
        _lastCursor = SDL.SDL_GetCursor();
        SDL.SDL_SetCursor(customCursor.Handle);
        
        Console.WriteLine("post set cursor: " + customCursor.Handle);
        
        // free memory
        if (_lastCursor != IntPtr.Zero)
        {
            SDL.SDL_FreeCursor(_lastCursor);
            _lastCursor = IntPtr.Zero;
        }
        
        SpriteBatch.End();
        base.Draw(gameTime);
    }

    private void LoadCursorRenderTarget()
    {
        Graphics.GraphicsDevice.SetRenderTarget(_cursorRenderTarget);
        Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
        SpriteBatch.Begin();
        
        DrawRedSquare();

        SpriteBatch.End();
        Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    private void DrawRedSquare()
    {
        // 10x10 red square for debugging purposes
        SpriteBatch.Draw(WhiteTexture, new Rectangle(0, 0, DebugSquareSize, DebugSquareSize), Color.Red);
    }
}