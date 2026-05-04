#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Screens;
using CheckersArcade.UI;
using System;

namespace CheckersArcade;

public enum GameMode { Classic, Physics } // ⬅️ Новый enum
public enum GameState { Menu, Playing }

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private SpriteFont? _font;
    private GameState _state = GameState.Menu;
    private MenuScreen _menu = null!;
    private GameScreen _game = null!;
    private MouseState _currentMouse, _prevMouse;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromTicks(166666);
    }

    protected override void Initialize() => base.Initialize();

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        try { _font = Content.Load<SpriteFont>("Fonts/Arial"); }
        catch { _font = null; }

        _menu = new MenuScreen(GraphicsDevice, _font);
        _menu.OnPlayClicked += (mode) =>
        {
            _state = GameState.Playing;
            _game.Start(mode);
        };
        _menu.OnExitClicked += Exit;

        _game = new GameScreen(GraphicsDevice, _font);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            if (_state == GameState.Playing) _state = GameState.Menu;
            else Exit();
        }

        _prevMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
        bool leftClick = _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_state)
        {
            case GameState.Menu:
                _menu.Update(_currentMouse, leftClick);
                break;
            case GameState.Playing:
                _game.Update(_currentMouse, leftClick);
                _game.UpdateEffects(dt);
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        switch (_state)
        {
            case GameState.Menu:
                _menu.Draw(_spriteBatch);
                break;
            case GameState.Playing:
                _game.Draw(_spriteBatch);
                break;
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}