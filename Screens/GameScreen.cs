#nullable disable

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Controllers;
using CheckersArcade.Models;
using CheckersArcade.Views;

namespace CheckersArcade.Screens;

public class GameScreen
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CheckersBoardModel _boardModel;
    private readonly CheckersGameController _controller;
    private readonly GameView _view;

    public GameScreen(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _graphicsDevice = graphicsDevice;
        BoardLayout.Configure(BoardLayout.BoardSize, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
        _boardModel = new CheckersBoardModel();
        _controller = new CheckersGameController(_boardModel);
        _view = new GameView(graphicsDevice, font, _boardModel);
        _boardModel.PieceCaptured += _view.PlayCaptureEffect;
    }

    public void HandleInput(MouseState mouse, bool isLeftClick)
    {
        _controller.HandleMouse(mouse, isLeftClick);
    }

    public void Update(float deltaSeconds)
    {
        _controller.Update(deltaSeconds);
        _view.Update(deltaSeconds);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _view.Draw(spriteBatch);
    }

    public void Reset(int boardSize)
    {
        BoardLayout.Configure(boardSize, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        _boardModel.Reset();
        _view.ClearEffects();
    }

    public void Resize(int screenWidth, int screenHeight)
    {
        BoardLayout.Configure(BoardLayout.BoardSize, screenWidth, screenHeight);
        _boardModel.ResetVisualPositions();
        _view.ClearEffects();
    }
}
