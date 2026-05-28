#nullable disable

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CheckersArcade.Models;

namespace CheckersArcade.Views;

public class GameView
{
    private readonly CheckersBoardModel _board;
    private readonly Texture2D _whitePixel;
    private readonly SpriteFont _font;

    public GameView(GraphicsDevice graphicsDevice, SpriteFont font, CheckersBoardModel board)
    {
        _font = font;
        _board = board;

        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawBoard(spriteBatch);
        DrawHud(spriteBatch);
    }

    private void DrawBoard(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_whitePixel, BoardLayout.Bounds, Color.DimGray);

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                DrawCell(spriteBatch, x, y);
                DrawValidMoveMarker(spriteBatch, x, y);
            }
        }

        DrawPieces(spriteBatch);
    }

    private void DrawCell(SpriteBatch spriteBatch, int x, int y)
    {
        bool isDark = BoardLayout.IsDarkCell(x, y);
        Color cellColor = isDark ? new Color(80, 60, 40) : new Color(200, 180, 150);

        spriteBatch.Draw(
            _whitePixel,
            new Rectangle(
                BoardLayout.OffsetX + x * BoardLayout.CellSize,
                BoardLayout.OffsetY + y * BoardLayout.CellSize,
                BoardLayout.CellSize,
                BoardLayout.CellSize),
            cellColor);
    }

    private void DrawValidMoveMarker(SpriteBatch spriteBatch, int x, int y)
    {
        if (_board.IsBusy || !_board.ValidMoves.Contains(new Point(x, y)))
        {
            return;
        }

        DrawFilledCircle(spriteBatch, BoardLayout.CellToWorldCenter(x, y), 10f, Color.LimeGreen * 0.6f);
    }

    private void DrawPieces(SpriteBatch spriteBatch)
    {
        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                CheckerPiece piece = _board.GetPiece(x, y);
                if (piece == null)
                {
                    continue;
                }

                Color pieceColor = piece.Side == PieceSide.Red ? Color.Crimson : Color.Navy;
                if (!_board.IsBusy && _board.SelectedCell == new Point(x, y))
                {
                    pieceColor = Color.White;
                }

                Vector2 center = _board.GetPieceVisualCenter(x, y);
                DrawFilledCircle(spriteBatch, center, BoardLayout.PieceRadius, pieceColor);
                DrawCircleOutline(spriteBatch, center, BoardLayout.PieceRadius, Color.Black * 0.55f, 3f);

                if (piece.IsKing)
                {
                    DrawKingMark(spriteBatch, center);
                }
            }
        }
    }

    private void DrawHud(SpriteBatch spriteBatch)
    {
        if (_font == null)
        {
            return;
        }

        string turnText = _board.CurrentTurn == PieceSide.Red ? "Turn: RED" : "Turn: BLUE";
        Color textColor = Color.White;

        if (_board.IsSlidingActive)
        {
            turnText = "Pieces are sliding";
            textColor = Color.LightSkyBlue;
        }
        else if (_board.IsChainCaptureActive)
        {
            turnText = "Chain capture: continue with the same piece";
            textColor = Color.Yellow;
        }

        spriteBatch.DrawString(_font, turnText, new Vector2(20, 20), textColor);
        spriteBatch.DrawString(_font, "ESC - Menu", new Vector2(20, 45), Color.Gray);
    }

    private void DrawKingMark(SpriteBatch spriteBatch, Vector2 center)
    {
        DrawFilledCircle(spriteBatch, center, 13f, Color.Gold);
        DrawFilledCircle(spriteBatch, center, 7f, Color.Black * 0.75f);
    }

    private void DrawFilledCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        if (!IsFinite(center) || radius <= 0f)
        {
            return;
        }

        int r = Math.Max(1, (int)MathF.Ceiling(radius));
        float radiusSquared = radius * radius;

        for (int dy = -r; dy <= r; dy++)
        {
            float dx = MathF.Sqrt(Math.Max(0f, radiusSquared - dy * dy));
            spriteBatch.Draw(
                _whitePixel,
                new Rectangle((int)(center.X - dx), (int)(center.Y + dy), Math.Max(1, (int)(dx * 2f)), 1),
                color);
        }
    }

    private void DrawCircleOutline(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float thickness)
    {
        if (!IsFinite(center) || radius <= 0f || thickness <= 0f)
        {
            return;
        }

        const int segments = 32;
        float step = MathHelper.TwoPi / segments;

        for (int i = 0; i < segments; i++)
        {
            float a1 = i * step;
            float a2 = (i + 1) * step;
            Vector2 p1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;
            Vector2 p2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;
            DrawLine(spriteBatch, p1, p2, thickness, color);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 a, Vector2 b, float thickness, Color color)
    {
        if (!IsFinite(a) || !IsFinite(b) || thickness <= 0f)
        {
            return;
        }

        float length = Vector2.Distance(a, b);
        if (length <= 0.5f)
        {
            return;
        }

        float angle = MathF.Atan2(b.Y - a.Y, b.X - a.X);
        spriteBatch.Draw(_whitePixel, a, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    private static bool IsFinite(Vector2 value) =>
        !(float.IsNaN(value.X) || float.IsNaN(value.Y) ||
          float.IsInfinity(value.X) || float.IsInfinity(value.Y));
}
