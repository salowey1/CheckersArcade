#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public class CheckersBoardModel
{
    private CheckerPiece[,] _grid;
    private readonly List<Point> _validMoves = new();
    private readonly CheckersRules _rules = new();
    private readonly SlidingPhysicsModel _sliding = new();

    private bool _shouldEndTurnAfterCapture;

    public PieceSide CurrentTurn { get; private set; } = PieceSide.Red;
    public Point? SelectedCell { get; private set; }
    public IReadOnlyList<Point> ValidMoves => _validMoves;

    public bool IsGameOver { get; private set; }
    public PieceSide? Winner { get; private set; }
    public bool IsSlidingActive => _sliding.IsActive;
    public bool IsAnimating => CurrentAnimation?.IsActive == true;
    public bool IsBusy => IsAnimating || IsSlidingActive;
    public PieceAnimation CurrentAnimation { get; private set; }

    public event Action<Vector2> PieceCaptured;

    public CheckersBoardModel() => Reset();

    public void Reset()
    {
        _grid = new CheckerPiece[BoardLayout.BoardSize, BoardLayout.BoardSize];
        int setupRows = Math.Max(1, BoardLayout.BoardSize / 2 - 1);

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                if (!BoardLayout.IsDarkCell(x, y))
                {
                    continue;
                }

                if (y < setupRows)
                {
                    PlaceNewPiece(x, y, PieceSide.Blue);
                }
                else if (y >= BoardLayout.BoardSize - setupRows)
                {
                    PlaceNewPiece(x, y, PieceSide.Red);
                }
            }
        }

        CurrentTurn = PieceSide.Red;
        ClearSelection();
        _sliding.Reset();
        _shouldEndTurnAfterCapture = false;
        CurrentAnimation = null;
        IsGameOver = false;
        Winner = null;
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

        if (!IsBusy && _shouldEndTurnAfterCapture)
        {
            FinishCaptureMove();
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
        return !IsGameOver && !IsBusy && piece != null && piece.Side == CurrentTurn;
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
        if (IsGameOver || IsBusy || SelectedCell == null || !BoardLayout.IsInside(target) || !_validMoves.Contains(target))
        {
            return false;
        }

        Point from = SelectedCell.Value;
        ExecuteMove(from, target);
        return true;
    }

    public void ClearSelection()
    {
        SelectedCell = null;
        _validMoves.Clear();
    }

    public void ResetVisualPositions()
    {
        CurrentAnimation = null;
        _sliding.Reset();

        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                CheckerPiece piece = _grid[x, y];
                if (piece == null)
                {
                    continue;
                }

                piece.VisualPosition = BoardLayout.CellToWorldCenter(x, y);
                piece.Velocity = Vector2.Zero;
                piece.IsSliding = false;
            }
        }
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

    private void ExecuteMove(Point from, Point to)
    {
        if (!CanMoveToCell(from, to, out CheckerPiece piece))
        {
            ClearSelection();
            return;
        }

        Point? capturedCell = _rules.FindCapturedCell(_grid, from.X, from.Y, to.X, to.Y, piece);
        CheckerPiece captured = null;

        if (capturedCell.HasValue && !TryGetCapturedPiece(capturedCell.Value, piece, out captured))
        {
            ClearSelection();
            return;
        }

        MovePiece(piece, from, to);
        StartMoveAnimation(piece, to);
        ClearSelection();

        if (capturedCell.HasValue)
        {
            CapturePiece(capturedCell.Value, captured, piece);
            return;
        }

        CompleteNormalMove(piece);
    }

    private bool CanMoveToCell(Point from, Point to, out CheckerPiece piece)
    {
        piece = null;

        if (!BoardLayout.IsInside(from) || !BoardLayout.IsInside(to) || !BoardLayout.IsDarkCell(to))
        {
            return false;
        }

        piece = _grid[from.X, from.Y];
        return piece != null && _grid[to.X, to.Y] == null;
    }

    private bool TryGetCapturedPiece(Point capturedCell, CheckerPiece movingPiece, out CheckerPiece captured)
    {
        captured = GetPiece(capturedCell);
        return captured != null && captured.Side != movingPiece.Side;
    }

    private void MovePiece(CheckerPiece piece, Point from, Point to)
    {
        _grid[to.X, to.Y] = piece;
        _grid[from.X, from.Y] = null;
        piece.GridX = to.X;
        piece.GridY = to.Y;
    }

    private void StartMoveAnimation(CheckerPiece piece, Point target)
    {
        CurrentAnimation = new PieceAnimation
        {
            Piece = piece,
            StartPosition = piece.VisualPosition,
            EndPosition = BoardLayout.CellToWorldCenter(target),
            Duration = 0.22f
        };
    }

    private void CapturePiece(Point capturedCell, CheckerPiece capturedPiece, CheckerPiece movingPiece)
    {
        _grid[capturedCell.X, capturedCell.Y] = null;

        Vector2 epicenter = capturedPiece.VisualPosition;
        PieceCaptured?.Invoke(epicenter);
        StartSliding(epicenter, movingPiece);

        _rules.PromoteIfNeeded(movingPiece);
        _shouldEndTurnAfterCapture = true;
    }

    private void CompleteNormalMove(CheckerPiece piece)
    {
        _rules.PromoteIfNeeded(piece);
        EndTurn();
    }

    private void FinishCaptureMove()
    {
        _shouldEndTurnAfterCapture = false;

        EndTurn();
    }

    private void EndTurn()
    {
        ClearSelection();
        CurrentTurn = CurrentTurn == PieceSide.Red ? PieceSide.Blue : PieceSide.Red;
        UpdateGameOverState();
    }

    private void UpdateGameOverState()
    {
        if (!HasAnyPiece(PieceSide.Red))
        {
            FinishGame(PieceSide.Blue);
            return;
        }

        if (!HasAnyPiece(PieceSide.Blue))
        {
            FinishGame(PieceSide.Red);
            return;
        }

        if (!_rules.HasAnyMove(_grid, CurrentTurn))
        {
            FinishGame(GetOppositeSide(CurrentTurn));
        }
    }

    private void FinishGame(PieceSide winner)
    {
        IsGameOver = true;
        Winner = winner;
        _shouldEndTurnAfterCapture = false;
        ClearSelection();
    }

    private bool HasAnyPiece(PieceSide side)
    {
        for (int y = 0; y < BoardLayout.BoardSize; y++)
        {
            for (int x = 0; x < BoardLayout.BoardSize; x++)
            {
                CheckerPiece piece = _grid[x, y];
                if (piece != null && piece.Side == side)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static PieceSide GetOppositeSide(PieceSide side) =>
        side == PieceSide.Red ? PieceSide.Blue : PieceSide.Red;

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
