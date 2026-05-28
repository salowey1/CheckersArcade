#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public class CheckersBoardModel
{
    private readonly CheckerPiece[,] _grid = new CheckerPiece[BoardLayout.BoardSize, BoardLayout.BoardSize];
    private readonly List<Point> _validMoves = new();
    private readonly CheckersRules _rules = new();
    private readonly SlidingPhysicsModel _sliding = new();

    private bool _chainCaptureActive;
    private CheckerPiece _pendingCapturePiece;
    private bool _pendingCaptureResolution;

    public PieceSide CurrentTurn { get; private set; } = PieceSide.Red;
    public Point? SelectedCell { get; private set; }
    public IReadOnlyList<Point> ValidMoves => _validMoves;

    public bool IsRedTurn => CurrentTurn == PieceSide.Red;
    public bool IsChainCaptureActive => _chainCaptureActive;
    public bool IsSlidingActive => _sliding.IsActive;
    public bool IsAnimating => CurrentAnimation?.IsActive == true;
    public bool IsBusy => IsAnimating || IsSlidingActive;
    public PieceAnimation CurrentAnimation { get; private set; }

    public CheckersBoardModel() => Reset();

    public void Reset()
    {
        Array.Clear(_grid, 0, _grid.Length);

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                if (!BoardLayout.IsDarkCell(x, y))
                {
                    continue;
                }

                if (y < 3)
                {
                    PlaceNewPiece(x, y, PieceSide.Blue);
                }
                else if (y > 4)
                {
                    PlaceNewPiece(x, y, PieceSide.Red);
                }
            }
        }

        CurrentTurn = PieceSide.Red;
        ClearSelection();
        _chainCaptureActive = false;
        _sliding.Reset();
        _pendingCapturePiece = null;
        _pendingCaptureResolution = false;
        CurrentAnimation = null;
    }

    public void Update(float deltaSeconds)
    {
        float dt = ClampDelta(deltaSeconds);

        if (CurrentAnimation?.IsActive == true)
        {
            CurrentAnimation.Update(dt);
            if (!CurrentAnimation.IsActive)
            {
                CurrentAnimation = null;
            }
        }

        if (!IsAnimating && _sliding.Update(dt, GetPieces(), out List<SlidingSnap> snapAssignments))
        {
            ApplySlidingSnap(snapAssignments);
        }

        if (!IsBusy && _pendingCaptureResolution)
        {
            ResolvePendingCapture();
        }
    }

    public CheckerPiece GetPiece(int x, int y) => BoardLayout.IsInside(x, y) ? _grid[x, y] : null;

    public CheckerPiece GetPiece(Point cell) => GetPiece(cell.X, cell.Y);

    public Vector2 GetPieceVisualCenter(int x, int y)
    {
        CheckerPiece piece = GetPiece(x, y);
        return piece?.VisualPosition ?? BoardLayout.CellToWorldCenter(x, y);
    }

    public bool CanSelect(Point cell)
    {
        CheckerPiece piece = GetPiece(cell);
        return !IsBusy && piece != null && piece.Side == CurrentTurn;
    }

    public bool SelectPiece(Point cell)
    {
        if (!CanSelect(cell))
        {
            return false;
        }

        List<Point> moves = _rules.GetSelectableMoves(_grid, cell, CurrentTurn);

        if (moves.Count == 0)
        {
            ClearSelection();
            return false;
        }

        SelectedCell = cell;
        _validMoves.Clear();
        _validMoves.AddRange(moves);
        return true;
    }

    public bool TryMoveSelectedPiece(Point target)
    {
        if (IsBusy || SelectedCell == null || !BoardLayout.IsInside(target) || !_validMoves.Contains(target))
        {
            return false;
        }

        Point from = SelectedCell.Value;
        ExecuteMove(from.X, from.Y, target.X, target.Y);
        return true;
    }

    public void ClearSelection()
    {
        SelectedCell = null;
        _validMoves.Clear();
    }

    private void PlaceNewPiece(int x, int y, PieceSide side)
    {
        _grid[x, y] = new CheckerPiece
        {
            Side = side,
            IsKing = false,
            GridX = x,
            GridY = y,
            VisualPosition = BoardLayout.CellToWorldCenter(x, y),
            Velocity = Vector2.Zero,
            IsSliding = false
        };
    }

    private void ExecuteMove(int fromX, int fromY, int toX, int toY)
    {
        if (!BoardLayout.IsInside(fromX, fromY) ||
            !BoardLayout.IsInside(toX, toY) ||
            !BoardLayout.IsDarkCell(toX, toY))
        {
            ClearSelection();
            return;
        }

        CheckerPiece piece = _grid[fromX, fromY];
        if (piece == null)
        {
            ClearSelection();
            return;
        }

        if (_grid[toX, toY] != null)
        {
            ClearSelection();
            return;
        }

        Point? capturedCell = _rules.FindCapturedCell(_grid, fromX, fromY, toX, toY, piece);
        bool isCapture = capturedCell.HasValue;
        CheckerPiece captured = null;

        if (isCapture)
        {
            Point cell = capturedCell.Value;
            captured = GetPiece(cell);
            if (captured == null || captured.Side == piece.Side)
            {
                ClearSelection();
                return;
            }
        }

        _grid[toX, toY] = piece;
        _grid[fromX, fromY] = null;
        piece.GridX = toX;
        piece.GridY = toY;

        CurrentAnimation = new PieceAnimation
        {
            Piece = piece,
            StartPosition = piece.VisualPosition,
            EndPosition = BoardLayout.CellToWorldCenter(toX, toY),
            Duration = 0.22f
        };

        ClearSelection();

        if (isCapture)
        {
            Point cell = capturedCell.Value;
            _grid[cell.X, cell.Y] = null;

            Vector2 epicenter = captured.VisualPosition;
            StartSliding(epicenter, piece);

            _rules.PromoteIfNeeded(piece);
            _pendingCapturePiece = piece;
            _pendingCaptureResolution = true;
            return;
        }

        _rules.PromoteIfNeeded(piece);
        EndTurn();
    }

    private void ResolvePendingCapture()
    {
        _pendingCaptureResolution = false;

        CheckerPiece piece = _pendingCapturePiece;
        _pendingCapturePiece = null;

        if (piece != null && GetPiece(piece.GridX, piece.GridY) == piece)
        {
            List<Point> captures = _rules.GetCaptures(_grid, piece.GridX, piece.GridY);
            if (captures.Count > 0)
            {
                SelectedCell = new Point(piece.GridX, piece.GridY);
                _validMoves.Clear();
                _validMoves.AddRange(captures);
                _chainCaptureActive = true;
                return;
            }
        }

        EndTurn();
    }

    private void EndTurn()
    {
        _chainCaptureActive = false;
        ClearSelection();
        CurrentTurn = CurrentTurn == PieceSide.Red ? PieceSide.Blue : PieceSide.Red;
    }

    private void StartSliding(Vector2 epicenter, CheckerPiece eater)
    {
        _sliding.Start(epicenter, eater, GetPieces());
    }

    private void ApplySlidingSnap(List<SlidingSnap> assignments)
    {
        if (assignments == null)
        {
            return;
        }

        Array.Clear(_grid, 0, _grid.Length);
        bool[,] used = new bool[BoardLayout.BoardSize, BoardLayout.BoardSize];

        foreach (SlidingSnap assignment in assignments)
        {
            CheckerPiece piece = assignment.Piece;
            if (piece == null)
            {
                continue;
            }

            Point cell = assignment.Cell;
            if (!BoardLayout.IsInside(cell) || !BoardLayout.IsDarkCell(cell) || used[cell.X, cell.Y])
            {
                if (!TryFindNearestFreeDarkCell(piece.VisualPosition, used, out cell))
                {
                    continue;
                }
            }

            used[cell.X, cell.Y] = true;
            _grid[cell.X, cell.Y] = piece;
            SnapPieceToCell(piece, cell);
        }
    }

    private static void SnapPieceToCell(CheckerPiece piece, Point cell)
    {
        piece.GridX = cell.X;
        piece.GridY = cell.Y;
        piece.VisualPosition = BoardLayout.CellToWorldCenter(cell);
        piece.Velocity = Vector2.Zero;
        piece.IsSliding = false;
    }

    private static bool TryFindNearestFreeDarkCell(Vector2 position, bool[,] used, out Point best)
    {
        best = Point.Zero;
        float bestDistance = float.MaxValue;

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                if (!BoardLayout.IsDarkCell(x, y) || used[x, y])
                {
                    continue;
                }

                float distance = Vector2.DistanceSquared(position, BoardLayout.CellToWorldCenter(x, y));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = new Point(x, y);
                }
            }
        }

        return bestDistance < float.MaxValue;
    }

    private List<CheckerPiece> GetPieces()
    {
        List<CheckerPiece> pieces = new();
        HashSet<CheckerPiece> seen = new(ReferenceEqualityComparer.Instance);

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                CheckerPiece piece = _grid[x, y];
                if (piece != null && seen.Add(piece))
                {
                    pieces.Add(piece);
                }
            }
        }

        return pieces;
    }

    private static float ClampDelta(float deltaSeconds)
    {
        if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f)
        {
            return 0f;
        }

        return Math.Min(deltaSeconds, 0.05f);
    }
}
