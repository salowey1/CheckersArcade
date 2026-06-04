#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CheckersArcade.Models;

namespace CheckersArcade.Views;

public class GameView
{
    private const int MaxCaptureEffects = 8;
    private const int CaptureEffectRayCount = 8;

    private readonly CheckersBoardModel _board;
    private readonly Texture2D _whitePixel;
    private readonly SpriteFont _font;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly List<CaptureEffect> _captureEffects = new();

    public GameView(GraphicsDevice graphicsDevice, SpriteFont font, CheckersBoardModel board)
    {
        _font = font;
        _board = board;
        _graphicsDevice = graphicsDevice;

        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawBoard(spriteBatch);
        DrawCaptureEffects(spriteBatch);
        DrawHud(spriteBatch);
    }

    public void Update(float deltaSeconds)
    {
        for (int i = _captureEffects.Count - 1; i >= 0; i--)
        {
            _captureEffects[i].Age += deltaSeconds;
            if (_captureEffects[i].Age >= CaptureEffect.Duration)
            {
                _captureEffects.RemoveAt(i);
            }
        }
    }

    public void PlayCaptureEffect(Vector2 center)
    {
        if (_captureEffects.Count >= MaxCaptureEffects)
        {
            _captureEffects.RemoveAt(0);
        }

        _captureEffects.Add(new CaptureEffect { Center = center });
    }

    public void ClearEffects()
    {
        _captureEffects.Clear();
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

    private void DrawCaptureEffects(SpriteBatch spriteBatch)
    {
        foreach (CaptureEffect effect in _captureEffects)
        {
            float progress = MathHelper.Clamp(effect.Age / CaptureEffect.Duration, 0f, 1f);
            float alpha = 1f - progress;
            float radius = BoardLayout.PieceRadius * (0.35f + progress * 1.55f);
            float rayLength = BoardLayout.PieceRadius * (0.55f + progress * 1.35f);
            Color ringColor = Color.OrangeRed * alpha;
            Color rayColor = Color.Gold * alpha;

            DrawCircleOutline(spriteBatch, effect.Center, radius, ringColor, Math.Max(1f, 4f * alpha));

            for (int i = 0; i < CaptureEffectRayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / CaptureEffectRayCount;
                Vector2 direction = new(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 start = effect.Center + direction * (BoardLayout.PieceRadius * 0.25f);
                Vector2 end = effect.Center + direction * rayLength;
                DrawLine(spriteBatch, start, end, Math.Max(1f, 3f * alpha), rayColor);
            }
        }
    }

    private void DrawHud(SpriteBatch spriteBatch)
    {
        if (_font == null)
        {
            return;
        }

        string turnText = _board.CurrentTurn == PieceSide.Red ? "Ход: красные" : "Ход: синие";
        Color textColor = Color.White;

        if (_board.IsGameOver)
        {
            turnText = _board.Winner == PieceSide.Red ? "Игра окончена: победили красные" : "Игра окончена: победили синие";
            textColor = Color.Gold;
        }
        else if (_board.IsSlidingActive)
        {
            turnText = "Шашки скользят";
            textColor = Color.LightSkyBlue;
        }
        float scale = GetHudScale(turnText);
        Vector2 firstLine = new(16f * scale, 16f * scale);
        Vector2 secondLine = new(firstLine.X, firstLine.Y + (_font.LineSpacing + 4f) * scale);

        DrawHudText(spriteBatch, turnText, firstLine, textColor, scale);
        DrawHudText(spriteBatch, "ESC - меню", secondLine, Color.Gray, scale);
    }

    private float GetHudScale(string text)
    {
        int width = Math.Max(1, _graphicsDevice.Viewport.Width);
        int height = Math.Max(1, _graphicsDevice.Viewport.Height);

        float scale = MathHelper.Clamp(Math.Min(width / 1024f, height / 768f), 0.7f, 1f);
        float maxTextWidth = Math.Max(120f, width - 32f);
        float measuredWidth = _font.MeasureString(text).X * scale;

        if (measuredWidth > maxTextWidth)
        {
            scale *= maxTextWidth / measuredWidth;
        }

        return MathHelper.Clamp(scale, 0.55f, 1f);
    }

    private void DrawHudText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
    {
        spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawKingMark(SpriteBatch spriteBatch, Vector2 center)
    {
        DrawFilledCircle(spriteBatch, center, 13f, Color.Gold);
        DrawFilledCircle(spriteBatch, center, 7f, Color.Black * 0.75f);
    }

    private void DrawFilledCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
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
        float length = Vector2.Distance(a, b);
        float angle = MathF.Atan2(b.Y - a.Y, b.X - a.X);
        spriteBatch.Draw(_whitePixel, a, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    private sealed class CaptureEffect
    {
        public const float Duration = 0.38f;

        public Vector2 Center;
        public float Age;
    }
}
