using Microsoft.Xna.Framework;

namespace CheckersArcade.Core;

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Life;
    public float MaxLife;
    public float Size;
    public float Rotation;
    public float RotationSpeed;
    public Color Tint;
    public bool IsActive;

    public void Reset() => IsActive = false;
}