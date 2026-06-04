#nullable disable

using System;
using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public class PieceAnimation
{
    public CheckerPiece Piece { get; set; }
    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }
    public float Duration { get; set; } = 0.22f;
    public float Timer { get; private set; }

    public bool IsActive => Timer < Duration;

    public void Update(float deltaSeconds)
    {
        Timer += deltaSeconds;
        Piece.VisualPosition = IsActive ? CurrentPosition : EndPosition;
    }

    public Vector2 CurrentPosition
    {
        get
        {
            float t = Math.Min(Timer / Duration, 1f);
            float smooth = t * t * (3f - 2f * t);
            return Vector2.Lerp(StartPosition, EndPosition, smooth);
        }
    }
}
