#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public sealed class CheckersRules
{
    private static readonly Point[] ForwardRed = { new(-1, -1), new(1, -1) };
    private static readonly Point[] ForwardBlue = { new(-1, 1), new(1, 1) };
    private static readonly Point[] AllDiagonals =
    {
        new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)
    };

    public List<Point> GetSelectableMoves(CheckerPiece[,] grid, Point cell)
    {
        List<Point> moves = GetSimpleMoves(grid, cell.X, cell.Y);

        foreach (Point capture in GetCaptures(grid, cell.X, cell.Y))
        {
            if (!moves.Contains(capture))
            {
                moves.Add(capture);
            }
        }

        return moves;
    }

    public List<Point> GetSimpleMoves(CheckerPiece[,] grid, int x, int y)
    {
        List<Point> moves = new();
        CheckerPiece piece = GetPiece(grid, x, y);

        if (piece.IsKing)
        {
            foreach (Point dir in AllDiagonals)
            {
                int tx = x + dir.X;
                int ty = y + dir.Y;

                while (BoardLayout.IsInside(tx, ty) && grid[tx, ty] == null)
                {
                    moves.Add(new Point(tx, ty));
                    tx += dir.X;
                    ty += dir.Y;
                }
            }

            return moves;
        }

        Point[] dirs = piece.Side == PieceSide.Red ? ForwardRed : ForwardBlue;
        foreach (Point dir in dirs)
        {
            int tx = x + dir.X;
            int ty = y + dir.Y;
            if (BoardLayout.IsInside(tx, ty) && grid[tx, ty] == null)
            {
                moves.Add(new Point(tx, ty));
            }
        }

        return moves;
    }

    public List<Point> GetCaptures(CheckerPiece[,] grid, int x, int y)
    {
        List<Point> captures = new();
        CheckerPiece piece = GetPiece(grid, x, y);

        if (piece.IsKing)
        {
            AddKingCaptures(grid, x, y, piece, captures);
            return captures;
        }

        foreach (Point dir in AllDiagonals)
        {
            int midX = x + dir.X;
            int midY = y + dir.Y;
            int toX = x + dir.X * 2;
            int toY = y + dir.Y * 2;

            if (!BoardLayout.IsInside(midX, midY) ||
                !BoardLayout.IsInside(toX, toY) ||
                grid[toX, toY] != null)
            {
                continue;
            }

            CheckerPiece middle = GetPiece(grid, midX, midY);
            if (middle != null && middle.Side != piece.Side)
            {
                captures.Add(new Point(toX, toY));
            }
        }

        return captures;
    }

    public bool HasAnyMove(CheckerPiece[,] grid, PieceSide side)
    {
        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                CheckerPiece piece = grid[x, y];
                if (piece == null || piece.Side != side)
                {
                    continue;
                }

                if (GetCaptures(grid, x, y).Count > 0 || GetSimpleMoves(grid, x, y).Count > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Point? FindCapturedCell(CheckerPiece[,] grid, int fromX, int fromY, int toX, int toY, CheckerPiece movingPiece)
    {
        if (Math.Abs(toX - fromX) != Math.Abs(toY - fromY))
        {
            return null;
        }

        int dx = Math.Sign(toX - fromX);
        int dy = Math.Sign(toY - fromY);
        Point? found = null;
        int x = fromX + dx;
        int y = fromY + dy;

        while (x != toX && y != toY)
        {
            CheckerPiece current = GetPiece(grid, x, y);
            if (current != null)
            {
                if (current.Side == movingPiece.Side || found.HasValue)
                {
                    return null;
                }

                found = new Point(x, y);
            }

            x += dx;
            y += dy;
        }

        return found;
    }

    public void PromoteIfNeeded(CheckerPiece piece)
    {
        if (piece.IsKing)
        {
            return;
        }

        if ((piece.Side == PieceSide.Red && piece.GridY == 0) ||
            (piece.Side == PieceSide.Blue && piece.GridY == BoardLayout.BoardSize - 1))
        {
            piece.IsKing = true;
        }
    }

    private static void AddKingCaptures(CheckerPiece[,] grid, int x, int y, CheckerPiece piece, List<Point> captures)
    {
        foreach (Point dir in AllDiagonals)
        {
            bool enemySeen = false;
            int tx = x + dir.X;
            int ty = y + dir.Y;

            while (BoardLayout.IsInside(tx, ty))
            {
                CheckerPiece current = grid[tx, ty];

                if (current == null)
                {
                    if (enemySeen)
                    {
                        captures.Add(new Point(tx, ty));
                    }
                }
                else
                {
                    if (current.Side == piece.Side || enemySeen)
                    {
                        break;
                    }

                    enemySeen = true;
                }

                tx += dir.X;
                ty += dir.Y;
            }
        }
    }

    private static CheckerPiece GetPiece(CheckerPiece[,] grid, Point cell) => GetPiece(grid, cell.X, cell.Y);

    private static CheckerPiece GetPiece(CheckerPiece[,] grid, int x, int y) => grid[x, y];
}
