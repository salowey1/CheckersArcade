#nullable disable

using System;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public static class BoardLayout
{
    public const int MinBoardSize = 4;
    public const int MaxBoardSize = 16;

    public static int BoardSize { get; private set; } = 8;
    public static int CellSize { get; private set; } = 80;
    public static int OffsetX { get; private set; } = 200;
    public static int OffsetY { get; private set; } = 40;
    public static float PieceRadius => Math.Max(8f, CellSize / 2f - 6f);

    public static Rectangle Bounds => new(OffsetX, OffsetY, BoardSize * CellSize, BoardSize * CellSize);

    public static void Configure(int boardSize, int screenWidth, int screenHeight)
    {
        BoardSize = ClampBoardSize(boardSize);

        int safeWidth = Math.Max(360, screenWidth);
        int safeHeight = Math.Max(360, screenHeight);
        int maxBoardPixels = Math.Max(160, Math.Min(safeWidth - 80, safeHeight - 100));
        CellSize = Math.Max(32, Math.Min(80, maxBoardPixels / BoardSize));

        int boardPixels = BoardSize * CellSize;
        OffsetX = Math.Max(20, (safeWidth - boardPixels) / 2);
        OffsetY = Math.Max(70, (safeHeight - boardPixels) / 2 + 10);
    }

    public static int ClampBoardSize(int boardSize)
    {
        if (boardSize < MinBoardSize)
        {
            return MinBoardSize;
        }

        if (boardSize > MaxBoardSize)
        {
            return MaxBoardSize;
        }

        return boardSize % 2 == 0 ? boardSize : boardSize - 1;
    }

    public static bool IsInside(int x, int y) => x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;

    public static bool IsInside(Point cell) => IsInside(cell.X, cell.Y);

    public static bool IsDarkCell(int x, int y) => IsInside(x, y) && (x + y) % 2 != 0;

    public static bool IsDarkCell(Point cell) => IsDarkCell(cell.X, cell.Y);

    public static Vector2 CellToWorldCenter(int x, int y) =>
        new(OffsetX + x * CellSize + CellSize / 2f, OffsetY + y * CellSize + CellSize / 2f);

    public static Vector2 CellToWorldCenter(Point cell) => CellToWorldCenter(cell.X, cell.Y);

    public static bool TryScreenToCell(int screenX, int screenY, out Point cell)
    {
        cell = Point.Zero;

        if (screenX < OffsetX || screenY < OffsetY ||
            screenX >= OffsetX + BoardSize * CellSize ||
            screenY >= OffsetY + BoardSize * CellSize)
        {
            return false;
        }

        int x = (screenX - OffsetX) / CellSize;
        int y = (screenY - OffsetY) / CellSize;
        cell = new Point(x, y);
        return IsInside(cell);
    }
}
