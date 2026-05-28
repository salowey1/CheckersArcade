#nullable disable

using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public static class BoardLayout
{
    public const int BoardSize = 8;
    public const int CellSize = 80;
    public const int OffsetX = 200;
    public const int OffsetY = 40;
    public const float PieceRadius = CellSize / 2f - 10f;

    public static Rectangle Bounds => new(OffsetX, OffsetY, BoardSize * CellSize, BoardSize * CellSize);

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
