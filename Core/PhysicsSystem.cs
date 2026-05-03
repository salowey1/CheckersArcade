#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using CheckersArcade.GameLogic;

namespace CheckersArcade.Core;

// Сбитая шашка -> физическое тело
public class PhysicsBody
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Mass = 1f;
    public float Radius;
    public Color Tint;
    public bool IsActive = true;
}

// Ударная волна
public class Shockwave
{
    public Vector2 Center;
    public float Radius;
    public float MaxRadius;
    public float Speed;
    public Color Tint;
    public bool IsActive;
    public HashSet<Point> AffectedCells = new();

    public Shockwave(Vector2 center, Color color, float cellSize, int cellRange = 2)
    {
        Center = center;
        Radius = 0;
        MaxRadius = cellSize * cellRange; // Ровно 2 клетки
        Speed = 950f;                    // Скорость фронта волны
        Tint = color;
        IsActive = true;
    }

    public void Update(float dt, Board board, List<PhysicsBody> spawned)
    {
        if (!IsActive) return;
        float prevRadius = Radius;
        Radius += Speed * dt;
        if (Radius >= MaxRadius) { IsActive = false; return; }

        // Проверяем клетки в расширяющемся кольце волны
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Point cell = new Point(x, y);
                if (AffectedCells.Contains(cell)) continue;

                var piece = board.GetPiece(x, y);
                if (piece == null) continue;

                Vector2 cellCenter = new Vector2(
                    Board.OffsetX + x * Board.CellSize + Board.CellSize / 2f,
                    Board.OffsetY + y * Board.CellSize + Board.CellSize / 2f);

                float dist = Vector2.Distance(Center, cellCenter);
                // Шашка попала под фронт волны
                if (dist <= Radius && dist >= prevRadius - 3f)
                {
                    AffectedCells.Add(cell);
                    board.RemovePiece(x, y); // Убираем с сетки

                    Vector2 dir = cellCenter - Center;
                    if (dir.LengthSquared() > 0.01f) dir.Normalize();

                    // Сила импульса зависит от расстояния до центра (как в керлинге)
                    float force = 850f * (1f - (dist / MaxRadius));
                    spawned.Add(new PhysicsBody
                    {
                        Position = cellCenter,
                        Velocity = dir * force + new Vector2(Random.Shared.Next(-40, 40), Random.Shared.Next(-40, 40)),
                        Radius = Board.CellSize / 2f - 5f,
                        Tint = piece.IsRed ? Color.Crimson : Color.Navy,
                        IsActive = true
                    });
                }
            }
        }
    }

    public void Draw(SpriteBatch sb, Texture2D pixel)
    {
        if (!IsActive) return;
        float alpha = 0.6f * (1f - (Radius / MaxRadius));
        Color drawColor = Tint * alpha;
        // Рисуем как расширяющийся диск-волну
        sb.Draw(pixel, Center, null, drawColor, 0f, Vector2.Zero, new Vector2(Radius * 2f), SpriteEffects.None, 0f);
    }
}

public class PhysicsSystem
{
    private readonly List<Shockwave> _waves = new();
    private readonly List<PhysicsBody> _bodies = new();
    private readonly Texture2D _pixel;
    private readonly Rectangle _bounds;

    // ⛸️ Настройки "керлинга"
    private const float FRICTION = 0.982f;      // Плавное скольжение
    private const float RESTITUTION = 0.85f;     // Упругость столкновений

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

        // 1. Обновляем волны -> спавним сбитые шашки
        for (int i = _waves.Count - 1; i >= 0; i--)
        {
            var w = _waves[i];
            w.Update(dt, board, newBodies);
            if (!w.IsActive) _waves.RemoveAt(i);
        }
        _bodies.AddRange(newBodies);

        // 2. Физика тел (скольжение + границы)
        for (int i = _bodies.Count - 1; i >= 0; i--)
        {
            var b = _bodies[i];
            if (!b.IsActive) continue;

            b.Position += b.Velocity * dt;
            b.Velocity *= FRICTION; // Трение "льда"

            // Остановка при микро-скорости
            if (b.Velocity.LengthSquared() < 0.5f) b.Velocity = Vector2.Zero;

            // Отскок от стен экрана
            float r = b.Radius;
            if (b.Position.X < _bounds.Left + r) { b.Position.X = _bounds.Left + r; b.Velocity.X = Math.Abs(b.Velocity.X) * RESTITUTION; }
            if (b.Position.X > _bounds.Right - r) { b.Position.X = _bounds.Right - r; b.Velocity.X = -Math.Abs(b.Velocity.X) * RESTITUTION; }
            if (b.Position.Y < _bounds.Top + r) { b.Position.Y = _bounds.Top + r; b.Velocity.Y = Math.Abs(b.Velocity.Y) * RESTITUTION; }
            if (b.Position.Y > _bounds.Bottom - r) { b.Position.Y = _bounds.Bottom - r; b.Velocity.Y = -Math.Abs(b.Velocity.Y) * RESTITUTION; }

            if (b.Position.Y > _bounds.Bottom + 150) b.IsActive = false; // Улетел за экран
        }

        // 3. Столкновения тел друг с другом (передача импульса)
        for (int i = 0; i < _bodies.Count; i++)
        {
            if (!_bodies[i].IsActive) continue;
            for (int j = i + 1; j < _bodies.Count; j++)
            {
                if (!_bodies[j].IsActive) continue;
                ResolveCollision(_bodies[i], _bodies[j]);
            }
        }

        _bodies.RemoveAll(b => !b.IsActive);
    }

    private void ResolveCollision(PhysicsBody a, PhysicsBody b)
    {
        Vector2 diff = a.Position - b.Position;
        float dist = diff.Length();
        float minDist = a.Radius + b.Radius;

        if (dist < minDist && dist > 0.01f)
        {
            Vector2 normal = diff / dist;
            float overlap = minDist - dist;

            // Разделяем пересечение
            a.Position += normal * overlap * 0.5f;
            b.Position -= normal * overlap * 0.5f;

            // Импульс по закону сохранения количества движения
            float relVel = Vector2.Dot(a.Velocity - b.Velocity, normal);
            if (relVel > 0) return; // Уже разлетаются

            float impulse = -(1 + RESTITUTION) * relVel / (1f / a.Mass + 1f / b.Mass);
            a.Velocity += impulse * normal / a.Mass;
            b.Velocity -= impulse * normal / b.Mass;
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
            sb.Draw(_pixel, b.Position, null, b.Tint, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}