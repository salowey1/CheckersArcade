#nullable disable

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Screens;
using CheckersArcade.UI;

namespace CheckersArcade;

public class Game1 : Game
{
    private const int MinWindowWidth = 720;
    private const int MinWindowHeight = 620;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private bool _isApplyingWindowSize;

    private ScreenState _state = ScreenState.Menu;
    private MenuScreen _menu;
    private GameScreen _game;

    private MouseState _currentMouse;
    private MouseState _previousMouse;
    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1024,
            PreferredBackBufferHeight = 768
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        try
        {
            _font = Content.Load<SpriteFont>("Fonts/Arial");
        }
        catch
        {
            _font = null;
        }

        _menu = new MenuScreen(GraphicsDevice, _font);
        _menu.OnPlayClicked += boardSize =>
        {
            _state = ScreenState.Playing;
            _game?.Reset(boardSize);
        };
        _menu.OnExitClicked += Exit;

        _game = new GameScreen(GraphicsDevice, _font);
    }

    protected override void Update(GameTime gameTime)
    {
        ReadInput();
        HandleEscape();

        bool leftClick = IsLeftClickPressed();
        float deltaSeconds = GetDeltaSeconds(gameTime);

        if (_state == ScreenState.Menu)
        {
            _menu?.Update(_currentMouse, leftClick);
        }
        else
        {
            _game?.HandleInput(_currentMouse, leftClick);
            _game?.Update(deltaSeconds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        if (_state == ScreenState.Menu)
        {
            _menu?.Draw(_spriteBatch);
        }
        else
        {
            _game?.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private bool IsKeyPressed(Keys key) =>
        _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private void ReadInput()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }

    private void HandleEscape()
    {
        if (!IsKeyPressed(Keys.Escape))
        {
            return;
        }

        if (_state == ScreenState.Playing)
        {
            _state = ScreenState.Menu;
            return;
        }

        Exit();
    }

    private bool IsLeftClickPressed() =>
        _currentMouse.LeftButton == ButtonState.Pressed &&
        _previousMouse.LeftButton == ButtonState.Released;

    private static float GetDeltaSeconds(GameTime gameTime) =>
        (float)gameTime.ElapsedGameTime.TotalSeconds;

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        if (_isApplyingWindowSize)
        {
            return;
        }

        int width = Math.Max(MinWindowWidth, Window.ClientBounds.Width);
        int height = Math.Max(MinWindowHeight, Window.ClientBounds.Height);

        if (width != Window.ClientBounds.Width || height != Window.ClientBounds.Height)
        {
            _isApplyingWindowSize = true;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
            _isApplyingWindowSize = false;
        }

        _menu?.Resize(width, height);
        _game?.Resize(width, height);
    }

    private enum ScreenState
    {
        Menu,
        Playing
    }
}
