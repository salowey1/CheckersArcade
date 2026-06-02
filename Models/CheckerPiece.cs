#nullable disable

using Microsoft.Xna.Framework;

namespace CheckersArcade.Models;

public class CheckerPiece
{
    public PieceSide Side { get; set; }
    public bool IsKing { get; set; }

    public int GridX { get; set; }
    public int GridY { get; set; }

    public Vector2 VisualPosition { get; set; }
    public Vector2 Velocity { get; set; }
    public bool IsSliding { get; set; }
}
