using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Core;
using CheckersArcade.GameLogic;

namespace CheckersArcade.Screens;

public class GameScreen
{
    private readonly Board _board;
    private readonly PhysicsSystem _physics;
    private readonly Texture2D _whitePixel;
    private readonly SpriteFont? _font;

    public GameScreen(GraphicsDevice gd, SpriteFont? font)
    {
        _font = font;
        _board = new Board();
        _physics = new PhysicsSystem(gd, gd.Viewport.Width, gd.Viewport.Height);
        _whitePixel = new Texture2D(gd, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        _board.OnPieceCaptured += (pos, isRed) =>
            _physics.TriggerWave(pos, isRed ? Color.Crimson : Color.DarkBlue);
    }

    public void Update(MouseState mouse, bool isLeftClick)
    {
        if (isLeftClick)
        {
            int mx = mouse.X;
            int my = mouse.Y;
            int gx = (mx - Board.OffsetX) / Board.CellSize;
            int gy = (my - Board.OffsetY) / Board.CellSize;

            if (gx >= 0 && gx < 8 && gy >= 0 && gy < 8)
                _board.HandleClick(gx, gy);
        }
    }

    public void UpdatePhysics(float dt) => _physics.Update(dt, _board);

    public void Draw(SpriteBatch sb)
    {
        DrawBoard(sb);
        _physics.Draw(sb); // Рисуем волны и физические копии ПОВЕРХ доски
        DrawHud(sb);
    }

    private void DrawBoard(SpriteBatch sb) { /* ... (остается без изменений) ... */ }
    private void DrawHud(SpriteBatch sb) { /* ... (остается без изменений) ... */ }
    private void DrawCircle(SpriteBatch sb, float x, float y, float radius, Color color) { /* ... */ }
    private void DrawLine(SpriteBatch sb, float x1, float y1, float x2, float y2, float thickness, Color color) { /* ... */ }
    public void Reset() { _board.Init(); _physics.Clear(); }
}