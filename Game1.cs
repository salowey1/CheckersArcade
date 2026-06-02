#nullable disable

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Screens;
using CheckersArcade.UI;

namespace CheckersArcade;

public enum GameState
{
    Menu,
    Playing
}

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private GameState _state = GameState.Menu;
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
            _state = GameState.Playing;
            _game?.Reset(boardSize);
        };
        _menu.OnExitClicked += Exit;

        _game = new GameScreen(GraphicsDevice, _font);
    }

    protected override void Update(GameTime gameTime)
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();

        if (IsKeyPressed(Keys.Escape))
        {
            if (_state == GameState.Playing)
            {
                _state = GameState.Menu;
            }
            else
            {
                Exit();
            }
        }

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();

        bool leftClick = _currentMouse.LeftButton == ButtonState.Pressed &&
                         _previousMouse.LeftButton == ButtonState.Released;

        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_state == GameState.Menu)
        {
            _menu?.Update(_currentMouse, leftClick);
        }
        else if (_state == GameState.Playing)
        {
            _game?.Update(_currentMouse, leftClick);
            _game?.UpdateEffects(deltaSeconds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        if (_state == GameState.Menu)
        {
            _menu?.Draw(_spriteBatch);
        }
        else if (_state == GameState.Playing)
        {
            _game?.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private bool IsKeyPressed(Keys key) =>
        _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
}
