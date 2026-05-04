#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CheckersArcade.Core;

public class ClassicParticle
{
	public Vector2 Position;
	public Vector2 Velocity;
	public float Life;
	public float MaxLife;
	public float Radius;
	public Color Tint;
	public bool IsActive = true;
}

public class ClassicExplosionSystem
{
	private readonly List<ClassicParticle> _particles = new();
	private readonly Texture2D _pixel;
	private readonly Rectangle _bounds;
	private const float GRAVITY = 300f;
	private const float FRICTION = 0.98f;

	public ClassicExplosionSystem(GraphicsDevice gd, int width, int height)
	{
		_pixel = new Texture2D(gd, 1, 1);
		_pixel.SetData(new[] { Color.White });
		_bounds = new Rectangle(0, 0, width, height);
	}

	public void Clear() => _particles.Clear();

	public void Spawn(Vector2 pos, Color baseColor, int count = 12)
	{
		for (int i = 0; i < count; i++)
		{
			float ang = (float)(Random.Shared.NextDouble() * Math.PI * 2);
			float spd = (float)(Random.Shared.NextDouble() * 350 + 150);
			_particles.Add(new ClassicParticle
			{
				Position = pos,
				Velocity = new Vector2((float)Math.Cos(ang) * spd, (float)Math.Sin(ang) * spd - 200),
				Radius = Random.Shared.Next(3, 7),
				Tint = Color.Lerp(baseColor, Color.White, (float)Random.Shared.NextDouble() * 0.5f),
				Life = Random.Shared.Next(800, 1400) / 1000f,
				MaxLife = Random.Shared.Next(800, 1400) / 1000f,
				IsActive = true
			});
		}
	}

	public void Update(float dt)
	{
		foreach (var p in _particles)
		{
			if (!p.IsActive) continue;
			p.Life -= dt;
			if (p.Life <= 0) { p.IsActive = false; continue; }

			p.Velocity.Y += GRAVITY * dt;
			p.Position += p.Velocity * dt;
			p.Velocity *= FRICTION;

			if (p.Position.Y > _bounds.Bottom - p.Radius)
			{
				p.Position.Y = _bounds.Bottom - p.Radius;
				p.Velocity.Y *= -0.5f;
				p.Velocity.X *= 0.8f;
			}
		}
		_particles.RemoveAll(p => !p.IsActive);
	}

	public void Draw(SpriteBatch sb)
	{
		foreach (var p in _particles)
		{
			if (!p.IsActive) continue;
			float alpha = p.Life / p.MaxLife;
			Color col = p.Tint * alpha;
			sb.Draw(_pixel, p.Position, null, col, 0f, Vector2.Zero, new Vector2(p.Radius * 2f), SpriteEffects.None, 0f);
		}
	}
}