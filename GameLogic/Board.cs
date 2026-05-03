#nullable disable
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CheckersArcade.GameLogic;

// Класс анимации передвижения
public class PieceAnimation
{
    public Vector2 StartPos;
    public Vector2 EndPos;
    public float Duration = 0.22f; // Длительность в секундах
    public float Timer = 0f;
    public bool IsActive => Timer < Duration;

    public float SmoothProgress
    {
        get
        {
            float t = Math.Min(Timer / Duration, 1f);
            return t * t * (3f - 2f * t); // Smoothstep: разгон и плавное торможение
        }
    }
    public Vector2 CurrentPos => Vector2.Lerp(StartPos, EndPos, SmoothProgress);
}

public class Board
{
    private readonly Piece[,] _grid = new Piece[8, 8];
    public const int CellSize = 80;
    public const int OffsetX = 200;
    public const int OffsetY = 40;

    public bool IsRedTurn { get; private set; } = true;
    public int SelectedX { get; set; } = -1;
    public int SelectedY { get; set; } = -1;
    public bool IsChainCaptureActive => _mustContinueCapture;

    public PieceAnimation? CurrentAnimation { get; private set; }
    private bool _mustContinueCapture = false;
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
        _mustContinueCapture = false;
        CurrentAnimation = null;
    }

    public void UpdateAnimations(float dt)
    {
        if (CurrentAnimation?.IsActive == true)
        {
            CurrentAnimation.Timer += dt;
            if (!CurrentAnimation.IsActive)
                CurrentAnimation = null;
        }
    }

    // Возвращает визуальный центр клетки (учитывает анимацию)
    public Vector2 GetVisualPieceCenter(int x, int y)
    {
        if (CurrentAnimation?.IsActive == true && CurrentAnimation.EndPos != Vector2.Zero)
        {
            // Если это целевая клетка текущей анимации
            float targetX = OffsetX + x * CellSize + CellSize / 2f;
            float targetY = OffsetY + y * CellSize + CellSize / 2f;
            if (Math.Abs(targetX - CurrentAnimation.EndPos.X) < 1f &&
                Math.Abs(targetY - CurrentAnimation.EndPos.Y) < 1f)
            {
                return CurrentAnimation.CurrentPos;
            }
        }
        return new Vector2(OffsetX + x * CellSize + CellSize / 2f, OffsetY + y * CellSize + CellSize / 2f);
    }

    public void HandleClick(int gx, int gy)
    {
        var piece = _grid[gx, gy];

        if (_mustContinueCapture)
        {
            if (_validMoves.Contains(new Vector2(gx, gy)))
            {
                ExecuteMove(SelectedX, SelectedY, gx, gy);
                return;
            }
            return;
        }

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
            return;
        }

        if (SelectedX != -1 && piece != null && piece.IsRed == IsRedTurn)
        {
            SelectedX = gx; SelectedY = gy;
            CalcValidMoves(gx, gy);
        }
        else if (SelectedX != -1)
        {
            SelectedX = SelectedY = -1;
            _validMoves.Clear();
        }
    }

    private void CalcValidMoves(int x, int y, bool onlyCaptures = false)
    {
        _validMoves.Clear();
        int dir = IsRedTurn ? -1 : 1;
        if (!onlyCaptures)
        {
            CheckMove(x + 1, y + dir);
            CheckMove(x - 1, y + dir);
        }
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

        // Логически двигаем мгновенно (сохраняем отзывчивость)
        _grid[tx, ty] = _grid[fx, fy];
        _grid[fx, fy] = null;

        // Запускаем визуальную анимацию
        CurrentAnimation = new PieceAnimation
        {
            StartPos = new Vector2(OffsetX + fx * CellSize + CellSize / 2f, OffsetY + fy * CellSize + CellSize / 2f),
            EndPos = new Vector2(OffsetX + tx * CellSize + CellSize / 2f, OffsetY + ty * CellSize + CellSize / 2f),
            Duration = 0.22f,
            Timer = 0f
        };

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

            CalcValidMoves(tx, ty, onlyCaptures: true);
            if (_validMoves.Count > 0)
            {
                SelectedX = tx;
                SelectedY = ty;
                _mustContinueCapture = true;
                return;
            }
        }

        _mustContinueCapture = false;
        SelectedX = SelectedY = -1;
        _validMoves.Clear();
        IsRedTurn = !IsRedTurn;
    }

    public Piece GetPiece(int x, int y) => _grid[x, y];
    public void RemovePiece(int x, int y) => _grid[x, y] = null;
}