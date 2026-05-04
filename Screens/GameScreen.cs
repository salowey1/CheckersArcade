#nullable disable
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
    private readonly SpriteFont _font;

    public GameScreen(GraphicsDevice gd, SpriteFont font)
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
            int gx = (mouse.X - Board.OffsetX) / Board.CellSize;
            int gy = (mouse.Y - Board.OffsetY) / Board.CellSize;
            if (gx >= 0 && gx < 8 && gy >= 0 && gy < 8) _board.HandleClick(gx, gy);
        }
    }

    public void UpdateEffects(float dt)
    {
        _board.UpdateAnimations(dt);
        _physics.Update(dt, _board);
    }

    public void Draw(SpriteBatch sb)
    {
        DrawBoard(sb);
        _physics.Draw(sb);
        DrawHud(sb);
    }

    public void Reset()
    {
        _board.Init();
        _physics.Clear();
    }

    private void DrawBoard(SpriteBatch sb)
    {
        sb.Draw(_whitePixel, new Rectangle(Board.OffsetX, Board.OffsetY, 8 * Board.CellSize, 8 * Board.CellSize), Color.DimGray);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool isDark = (x + y) % 2 != 0;
                Color cellCol = isDark ? new Color(80, 60, 40) : new Color(200, 180, 150);
                sb.Draw(_whitePixel, new Rectangle(Board.OffsetX + x * Board.CellSize, Board.OffsetY + y * Board.CellSize, Board.CellSize, Board.CellSize), cellCol);

                if (_board.ValidMoves.Contains(new Vector2(x, y)))
                    DrawCircle(sb, Board.OffsetX + x * Board.CellSize + Board.CellSize / 2f, Board.OffsetY + y * Board.CellSize + Board.CellSize / 2f, 10f, Color.LimeGreen * 0.6f);

                var p = _board.GetPiece(x, y);
                if (p != null)
                {
                    Color pCol = p.IsRed ? Color.Crimson : Color.Navy;
                    if (x == _board.SelectedX && y == _board.SelectedY) pCol = Color.White;
                    Vector2 center = _board.GetVisualPieceCenter(x, y);
                    DrawCircle(sb, center.X, center.Y, Board.CellSize / 2f - 10f, pCol);
                }
            }
        }
    }

    private void DrawHud(SpriteBatch sb)
    {
        if (_font == null) return;
        string turnText = _board.IsRedTurn ? "Ход: КРАСНЫЕ" : "Ход: СИНИЕ";
        if (_board.IsChainCaptureActive) turnText = "⚡ ЦЕПНОЕ ВЗЯТИЕ!";
        sb.DrawString(_font, turnText, new Vector2(20, 20), _board.IsChainCaptureActive ? Color.Yellow : Color.White);
        sb.DrawString(_font, "ESC - Меню", new Vector2(20, 45), Color.Gray);
    }

    private void DrawCircle(SpriteBatch sb, float x, float y, float radius, Color color)
    {
        int segments = 16;
        float step = MathHelper.Pi * 2 / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * step, a2 = (i + 1) * step;
            Vector2 p1 = new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * radius;
            Vector2 p2 = new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * radius;
            DrawLine(sb, x + p1.X, y + p1.Y, x + p2.X, y + p2.Y, 4f, color);
        }
        sb.Draw(_whitePixel, new Rectangle((int)(x - radius + 2), (int)(y - radius + 2), (int)(radius * 2 - 4), (int)(radius * 2 - 4)), color);
    }

    private void DrawLine(SpriteBatch sb, float x1, float y1, float x2, float y2, float thickness, Color color)
    {
        float angle = (float)Math.Atan2(y2 - y1, x2 - x1);
        float length = Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));
        sb.Draw(_whitePixel, new Vector2(x1, y1), null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
}