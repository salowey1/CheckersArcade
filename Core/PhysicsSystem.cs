#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using CheckersArcade.GameLogic;

namespace CheckersArcade.Core;

public class PhysicsBody
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius;
    public Color Tint;
    public float Alpha = 1f;
    public bool IsActive = true;
    public float Lifetime = 0f;
    public const float MAX_LIFETIME = 4.5f;
}

public class Shockwave
{
    public Vector2 Center;
    public float Radius;
    public float MaxRadius;
    public float Speed;
    public Color Tint;
    public bool IsActive;
    public HashSet<Point> HitCells = new();

    public Shockwave(Vector2 center, Color color, float cellSize, int range = 2)
    {
        Center = center;
        Radius = 0;
        MaxRadius = cellSize * range;
        Speed = 1000f;
        Tint = color;
        IsActive = true;
    }

    public void Update(float dt, Board board, List<PhysicsBody> spawned)
    {
        if (!IsActive) return;
        Radius += Speed * dt;
        if (Radius >= MaxRadius) { IsActive = false; return; }

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Point cell = new Point(x, y);
                if (HitCells.Contains(cell)) continue;

                var piece = board.GetPiece(x, y);
                if (piece == null) continue;

                Vector2 cellCenter = new Vector2(
                    Board.OffsetX + x * Board.CellSize + Board.CellSize / 2f,
                    Board.OffsetY + y * Board.CellSize + Board.CellSize / 2f);

                float dist = Vector2.Distance(Center, cellCenter);
                if (dist <= Radius)
                {
                    HitCells.Add(cell);
                    board.RemovePiece(x, y);

                    Vector2 dir = cellCenter - Center;
                    if (dir.LengthSquared() < 0.001f) dir = new Vector2(0, -1);
                    dir.Normalize();

                    float force = 950f * (1.0f - (dist / MaxRadius));
                    Vector2 vel = dir * force + new Vector2(Random.Shared.Next(-180, 180), Random.Shared.Next(-220, -40));

                    spawned.Add(new PhysicsBody
                    {
                        Position = cellCenter,
                        Velocity = vel,
                        Radius = Board.CellSize / 2f - 5f,
                        Tint = piece.IsRed ? Color.Crimson : Color.Navy,
                        IsActive = true,
                        Alpha = 1f
                    });
                }
            }
        }
    }

    public void Draw(SpriteBatch sb, Texture2D pixel)
    {
        if (!IsActive) return;
        float alpha = 0.55f * (1f - (Radius / MaxRadius));
        sb.Draw(pixel, Center, null, Tint * alpha, 0f, Vector2.Zero, new Vector2(Radius * 2f), SpriteEffects.None, 0f);
    }
}

public class PhysicsSystem
{
    private readonly List<Shockwave> _waves = new();
    private readonly List<PhysicsBody> _bodies = new();
    private readonly Texture2D _pixel;
    private readonly Rectangle _bounds;

    private const float FRICTION = 0.982f;
    private const float RESTITUTION = 0.85f;
    private const float MIN_SEP = 0.8f;

    public PhysicsSystem(GraphicsDevice gd, int width, int height)
    {
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _bounds = new Rectangle(0, 0, width, height);
    }

    public void Clear() { _waves.Clear(); _bodies.Clear(); }
    public void TriggerWave(Vector2 pos, Color color) => _waves.Add(new Shockwave(pos, color, Board.CellSize, 2));

    public void Update(float dt, Board board)
    {
        dt = Math.Min(dt, 0.05f);
        var newBodies = new List<PhysicsBody>();

        for (int i = _waves.Count - 1; i >= 0; i--)
        {
            _waves[i].Update(dt, board, newBodies);
            if (!_waves[i].IsActive) _waves.RemoveAt(i);
        }
        _bodies.AddRange(newBodies);

        for (int i = _bodies.Count - 1; i >= 0; i--)
        {
            var b = _bodies[i];
            if (!b.IsActive) continue;

            b.Lifetime += dt;
            b.Position += b.Velocity * dt;
            b.Velocity *= FRICTION;

            if (b.Lifetime > PhysicsBody.MAX_LIFETIME || b.Velocity.LengthSquared() < 0.3f)
            {
                b.Alpha -= dt * 2.5f;
                if (b.Alpha <= 0f) b.IsActive = false;
            }

            float r = b.Radius;
            if (b.Position.X < _bounds.Left + r) { b.Position.X = _bounds.Left + r; b.Velocity.X = Math.Abs(b.Velocity.X) * RESTITUTION; }
            if (b.Position.X > _bounds.Right - r) { b.Position.X = _bounds.Right - r; b.Velocity.X = -Math.Abs(b.Velocity.X) * RESTITUTION; }
            if (b.Position.Y < _bounds.Top + r) { b.Position.Y = _bounds.Top + r; b.Velocity.Y = Math.Abs(b.Velocity.Y) * RESTITUTION; }
            if (b.Position.Y > _bounds.Bottom - r) { b.Position.Y = _bounds.Bottom - r; b.Velocity.Y = -Math.Abs(b.Velocity.Y) * RESTITUTION; }
        }

        for (int i = 0; i < _bodies.Count; i++)
            if (_bodies[i].IsActive)
                for (int j = i + 1; j < _bodies.Count; j++)
                    if (_bodies[j].IsActive) ResolveCollision(_bodies[i], _bodies[j]);

        _bodies.RemoveAll(b => !b.IsActive);
    }

    private void ResolveCollision(PhysicsBody a, PhysicsBody b)
    {
        Vector2 diff = a.Position - b.Position;
        float dist = diff.Length();
        float minDist = a.Radius + b.Radius + MIN_SEP;

        if (dist < minDist && dist > 0.001f)
        {
            Vector2 normal = diff / dist;
            float overlap = minDist - dist;
            a.Position += normal * overlap * 0.5f;
            b.Position -= normal * overlap * 0.5f;

            float relVel = Vector2.Dot(a.Velocity - b.Velocity, normal);
            if (relVel > 0) return;

            float impulse = -(1 + RESTITUTION) * relVel * 0.5f;
            a.Velocity += impulse * normal;
            b.Velocity -= impulse * normal;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        foreach (var w in _waves) w.Draw(sb, _pixel);
        foreach (var b in _bodies)
        {
            if (!b.IsActive) continue;
            Vector2 origin = new Vector2(b.Radius, b.Radius);
            Vector2 scale = new Vector2(b.Radius * 2f, b.Radius * 2f);
            sb.Draw(_pixel, b.Position, null, b.Tint * b.Alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}