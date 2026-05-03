#nullable disable
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CheckersArcade.GameLogic;

public class Board
{
    private readonly Piece[,] _grid = new Piece[8, 8];
    public const int CellSize = 80;
    public const int OffsetX = 200;
    public const int OffsetY = 40;

    public bool IsRedTurn { get; private set; } = true;
    public int SelectedX { get; set; } = -1;
    public int SelectedY { get; set; } = -1;

    private readonly List<Vector2> _validMoves = new();
    public IReadOnlyList<Vector2> ValidMoves => _validMoves;

    public event Action<Vector2, bool> OnPieceCaptured;

    public Board() => Init();

    public void Init()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if ((x + y) % 2 != 0)
                {
                    if (y < 3) _grid[x, y] = new Piece { IsRed = false };
                    else if (y > 4) _grid[x, y] = new Piece { IsRed = true };
                }
        IsRedTurn = true;
        SelectedX = SelectedY = -1;
        _validMoves.Clear();
    }

    public void HandleClick(int gx, int gy)
    {
        var piece = _grid[gx, gy];

        if (SelectedX == -1 && piece != null && piece.IsRed == IsRedTurn)
        {
            SelectedX = gx; SelectedY = gy;
            CalcValidMoves(gx, gy);
            return;
        }

        if (SelectedX != -1 && piece == null && _validMoves.Contains(new Vector2(gx, gy)))
        {
            ExecuteMove(SelectedX, SelectedY, gx, gy);
            SelectedX = SelectedY = -1;
            _validMoves.Clear();
            IsRedTurn = !IsRedTurn;
            return;
        }

        if (SelectedX != -1 && piece != null && piece.IsRed == IsRedTurn)
        {
            SelectedX = gx; SelectedY = gy;
            CalcValidMoves(gx, gy);
        }
        else
        {
            SelectedX = SelectedY = -1;
            _validMoves.Clear();
        }
    }

    private void CalcValidMoves(int x, int y)
    {
        _validMoves.Clear();
        int dir = IsRedTurn ? -1 : 1;
        CheckMove(x + 1, y + dir);
        CheckMove(x - 1, y + dir);
        CheckCapture(x + 2, y + dir * 2, x + 1, y + dir);
        CheckCapture(x - 2, y + dir * 2, x - 1, y + dir);
    }

    private void CheckMove(int tx, int ty)
    {
        if (tx >= 0 && tx < 8 && ty >= 0 && ty < 8 && _grid[tx, ty] == null)
            _validMoves.Add(new Vector2(tx, ty));
    }

    private void CheckCapture(int tx, int ty, int mx, int my)
    {
        if (tx >= 0 && tx < 8 && ty >= 0 && ty < 8 && _grid[tx, ty] == null)
        {
            var mid = _grid[mx, my];
            if (mid != null && mid.IsRed != IsRedTurn)
                _validMoves.Add(new Vector2(tx, ty));
        }
    }

    private void ExecuteMove(int fx, int fy, int tx, int ty)
    {
        bool isCapture = Math.Abs(tx - fx) == 2;
        _grid[tx, ty] = _grid[fx, fy];
        _grid[fx, fy] = null;

        if (isCapture)
        {
            int mx = (fx + tx) / 2;
            int my = (fy + ty) / 2;
            var captured = _grid[mx, my];
            _grid[mx, my] = null;

            Vector2 worldPos = new Vector2(
                OffsetX + mx * CellSize + CellSize / 2f,
                OffsetY + my * CellSize + CellSize / 2f);

            OnPieceCaptured?.Invoke(worldPos, captured.IsRed);
        }
    }

    public Piece GetPiece(int x, int y) => _grid[x, y];
}