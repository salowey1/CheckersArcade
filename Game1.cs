#nullable disable
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Screens;
using CheckersArcade.UI;

namespace CheckersArcade;

public enum GameState { Menu, Playing }

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Texture2D _whitePixel;
    private GameState _state = GameState.Menu;
    private MenuScreen _menu;
    private GameScreen _game;
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

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        try { _font = Content.Load<SpriteFont>("Fonts/Arial"); }
        catch { _font = null; }

        _menu = new MenuScreen(GraphicsDevice, _font);
        _menu.OnPlayClicked += () => { _state = GameState.Playing; _game?.Reset(); };
        _menu.OnExitClicked += Exit;

        _game = new GameScreen(GraphicsDevice, _font);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            if (_state == GameState.Playing) _state = GameState.Menu;
            else Exit();
        }

        _prevMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
        bool leftClick = _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_state == GameState.Menu) _menu?.Update(_currentMouse, leftClick);
        else if (_state == GameState.Playing)
        {
            _game?.Update(_currentMouse, leftClick);
            _game?.UpdateEffects(dt);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        if (_state == GameState.Menu) _menu?.Draw(_spriteBatch);
        else if (_state == GameState.Playing) _game?.Draw(_spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}