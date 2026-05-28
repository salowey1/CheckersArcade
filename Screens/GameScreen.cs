#nullable disable

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Controllers;
using CheckersArcade.Models;
using CheckersArcade.Views;

namespace CheckersArcade.Screens;

public class GameScreen
{
    private readonly CheckersBoardModel _boardModel;
    private readonly CheckersGameController _controller;
    private readonly GameView _view;

    public GameScreen(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _boardModel = new CheckersBoardModel();
        _controller = new CheckersGameController(_boardModel);
        _view = new GameView(graphicsDevice, font, _boardModel);
    }

    public void Update(MouseState mouse, bool isLeftClick)
    {
        _controller.HandleMouse(mouse, isLeftClick);
    }

    public void UpdateEffects(float deltaSeconds)
    {
        _controller.Update(deltaSeconds);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _view.Draw(spriteBatch);
    }

    public void Reset()
    {
        _boardModel.Reset();
    }
}
