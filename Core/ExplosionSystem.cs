using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CheckersArcade.Core;

public class ExplosionSystem
{
    private readonly ParticlePool _pool;
    private readonly Texture2D _pixel;
    private readonly Rectangle _bounds;
    private const float GRAVITY = 900f;
    private const float BOUNCE = 0.55f;
    private const float FRICTION = 0.92f;

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
            float spd = (float)(Random.Shared.NextDouble() * 350 + 150);
            Vector2 vel = new((float)Math.Cos(ang) * spd, (float)Math.Sin(ang) * spd - 250);
            float sz = (float)(Random.Shared.NextDouble() * 10 + 4);
            float life = (float)(Random.Shared.NextDouble() * 1.5f + 0.6f);
            Color t = Color.Lerp(baseColor, Color.White, (float)Random.Shared.NextDouble() * 0.6f);
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

            if (p.Position.X < _bounds.X) { p.Position.X = _bounds.X; p.Velocity.X *= -BOUNCE; }
            if (p.Position.X > _bounds.Right) { p.Position.X = _bounds.Right; p.Velocity.X *= -BOUNCE; }
            if (p.Position.Y > _bounds.Bottom - p.Size)
            {
                p.Position.Y = _bounds.Bottom - p.Size;
                p.Velocity.Y *= -BOUNCE;
                p.Velocity.X *= FRICTION;
            }
            if (p.Position.Y < _bounds.Y) { p.Position.Y = _bounds.Y; p.Velocity.Y *= -BOUNCE; }
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