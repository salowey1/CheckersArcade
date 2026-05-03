using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CheckersArcade.Core;

public class ExplosionSystem
{
    private readonly ParticlePool _pool;
    private readonly Texture2D _pixel;
    private readonly Rectangle _bounds;

    // ⚙️ Аркадные параметры физики
    private const float GRAVITY = 650f;
    private const float BOUNCE = 0.75f;   // Упругий отскок
    private const float FRICTION = 0.92f; // Затухание по горизонтали

    public ExplosionSystem(GraphicsDevice gd, int maxParticles, int width, int height)
    {
        _pool = new ParticlePool(maxParticles);
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _bounds = new Rectangle(0, 0, width, height);
    }

    public void Clear() => _pool.Update(100f);

    public void SpawnExplosion(Vector2 pos, Color baseColor, int count = 15)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = (float)(Random.Shared.NextDouble() * Math.PI * 2);
            float spd = (float)(Random.Shared.NextDouble() * 450 + 200); // 💥 Мощный разлёт
            Vector2 vel = new Vector2((float)Math.Cos(ang) * spd, (float)Math.Sin(ang) * spd - 350);

            float sz = (float)(Random.Shared.NextDouble() * 12 + 5);
            float life = (float)(Random.Shared.NextDouble() * 1.8f + 0.8f);
            Color t = Color.Lerp(baseColor, Color.White, (float)Random.Shared.NextDouble() * 0.7f);

            _pool.Get(pos, vel, t, sz, life);
        }
    }

    public void Update(float dt)
    {
        _pool.Update(dt);
        _pool.DrawActive(p =>
        {
            p.Velocity.Y += GRAVITY * dt;
            p.Position += p.Velocity * dt;
            p.Rotation += p.RotationSpeed * dt;

            // 🛡️ Корректная коллизия с границами (защита от залипания)
            bool hit = false;

            if (p.Position.X < _bounds.Left)
            { p.Position.X = _bounds.Left; p.Velocity.X = Math.Abs(p.Velocity.X) * BOUNCE; hit = true; }
            if (p.Position.X > _bounds.Right - p.Size)
            { p.Position.X = _bounds.Right - p.Size; p.Velocity.X = -Math.Abs(p.Velocity.X) * BOUNCE; hit = true; }

            if (p.Position.Y < _bounds.Top)
            { p.Position.Y = _bounds.Top; p.Velocity.Y = Math.Abs(p.Velocity.Y) * BOUNCE; hit = true; }

            if (p.Position.Y > _bounds.Bottom - p.Size)
            {
                p.Position.Y = _bounds.Bottom - p.Size;
                p.Velocity.Y = -Math.Abs(p.Velocity.Y) * BOUNCE;
                p.Velocity.X *= FRICTION;
                hit = true;
            }

            // Если частица ударила пол и скорость упала -> "умирает" быстрее
            if (hit && Math.Abs(p.Velocity.Y) < 15f)
                p.Life -= dt * 2f;
        });
    }

    public void Draw(SpriteBatch sb)
    {
        _pool.DrawActive(p =>
        {
            float alpha = MathHelper.Clamp(p.Life / p.MaxLife, 0f, 1f);
            Color col = p.Tint * alpha;
            sb.Draw(_pixel, p.Position, null, col, p.Rotation, new Vector2(0.5f), p.Size, SpriteEffects.None, 0f);
        });
    }
}