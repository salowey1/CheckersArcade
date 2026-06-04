#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public readonly struct SlidingSnap
{
    public SlidingSnap(CheckerPiece piece, Point cell)
    {
        Piece = piece;
        Cell = cell;
    }

    public CheckerPiece Piece { get; }
    public Point Cell { get; }
}

public class SlidingPhysicsModel
{
    private const float PushForce = 900f;
    private const float Friction = 0.96f;
    private const float StopSpeed = 15f;
    private const float SettleTime = 0.2f;

    private CheckerPiece _fixedPiece;
    private float _settleTimer;

    public bool IsActive { get; private set; }

    public void Reset()
    {
        IsActive = false;
        _fixedPiece = null;
        _settleTimer = 0f;
    }

    public void Start(Vector2 center, CheckerPiece fixedPiece, List<CheckerPiece> pieces)
    {
        Reset();

        IsActive = true;
        _fixedPiece = fixedPiece;
        _fixedPiece.Velocity = Vector2.Zero;
        _fixedPiece.IsSliding = false;

        foreach (CheckerPiece piece in pieces)
        {
            if (piece == _fixedPiece)
            {
                continue;
            }

            Vector2 offset = piece.VisualPosition - center;
            float distance = offset.Length();
            float pushRadius = BoardLayout.CellSize * 2.5f;
            if (distance <= 1f || distance > pushRadius)
            {
                continue;
            }

            Vector2 direction = offset / distance;
            float strength = 1f - distance / pushRadius;
            piece.Velocity = direction * (PushForce * strength + 120f);
            piece.IsSliding = true;
        }

        if (!HasMovingPiece(pieces))
        {
            Reset();
        }
    }

    public bool Update(float deltaSeconds, List<CheckerPiece> pieces, out List<SlidingSnap> snaps)
    {
        snaps = null;

        if (!IsActive)
        {
            return false;
        }

        float friction = MathF.Pow(Friction, deltaSeconds * 60f);
        bool hasMovingPiece = false;

        foreach (CheckerPiece piece in pieces)
        {
            if (piece == _fixedPiece)
            {
                piece.Velocity = Vector2.Zero;
                piece.IsSliding = false;
                continue;
            }

            piece.VisualPosition += piece.Velocity * deltaSeconds;
            piece.Velocity *= friction;
            KeepInsideBoard(piece);

            if (piece.Velocity.LengthSquared() > StopSpeed * StopSpeed)
            {
                hasMovingPiece = true;
                piece.IsSliding = true;
            }
            else
            {
                piece.Velocity = Vector2.Zero;
                piece.IsSliding = false;
            }
        }

        if (hasMovingPiece)
        {
            _settleTimer = 0f;
            return false;
        }

        _settleTimer += deltaSeconds;
        if (_settleTimer < SettleTime)
        {
            return false;
        }

        snaps = CreateSnapAssignments(pieces);
        Reset();
        return true;
    }

    private List<SlidingSnap> CreateSnapAssignments(List<CheckerPiece> pieces)
    {
        List<SlidingSnap> result = new();
        bool[,] used = new bool[BoardLayout.BoardSize, BoardLayout.BoardSize];

        Point fixedCell = new(_fixedPiece.GridX, _fixedPiece.GridY);
        if (!CanUseCell(fixedCell, used))
        {
            fixedCell = FindNearestFreeDarkCell(_fixedPiece.VisualPosition, used);
        }

        used[fixedCell.X, fixedCell.Y] = true;
        result.Add(new SlidingSnap(_fixedPiece, fixedCell));

        foreach (CheckerPiece piece in pieces)
        {
            if (piece == _fixedPiece)
            {
                continue;
            }

            Point cell = FindNearestFreeDarkCell(piece.VisualPosition, used);
            used[cell.X, cell.Y] = true;
            result.Add(new SlidingSnap(piece, cell));
        }

        return result;
    }

    private static Point FindNearestFreeDarkCell(Vector2 position, bool[,] used)
    {
        Point best = new(0, 1);
        float bestDistance = float.MaxValue;

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                Point cell = new(x, y);
                if (!CanUseCell(cell, used))
                {
                    continue;
                }

                float distance = Vector2.DistanceSquared(position, BoardLayout.CellToWorldCenter(cell));
                if (distance < bestDistance)
                {
                    best = cell;
                    bestDistance = distance;
                }
            }
        }

        return best;
    }

    private static bool CanUseCell(Point cell, bool[,] used) =>
        BoardLayout.IsInside(cell) && BoardLayout.IsDarkCell(cell) && !used[cell.X, cell.Y];

    private static void KeepInsideBoard(CheckerPiece piece)
    {
        float left = BoardLayout.OffsetX + BoardLayout.PieceRadius;
        float right = BoardLayout.OffsetX + BoardLayout.BoardSize * BoardLayout.CellSize - BoardLayout.PieceRadius;
        float top = BoardLayout.OffsetY + BoardLayout.PieceRadius;
        float bottom = BoardLayout.OffsetY + BoardLayout.BoardSize * BoardLayout.CellSize - BoardLayout.PieceRadius;

        piece.VisualPosition = new Vector2(
            MathHelper.Clamp(piece.VisualPosition.X, left, right),
            MathHelper.Clamp(piece.VisualPosition.Y, top, bottom));
    }

    private static bool HasMovingPiece(List<CheckerPiece> pieces)
    {
        foreach (CheckerPiece piece in pieces)
        {
            if (piece.IsSliding)
            {
                return true;
            }
        }

        return false;
    }

}
