using Microsoft.Xna.Framework;
using System;

namespace CheckersArcade.Core;

public class ParticlePool
{
    private readonly Particle[] _pool;

    public ParticlePool(int capacity)
    {
        _pool = new Particle[capacity];
        for (int i = 0; i < capacity; i++) _pool[i] = new Particle();
    }

    public Particle? Get(Vector2 pos, Vector2 vel, Color tint, float size, float life)
    {
        foreach (var p in _pool)
            if (!p.IsActive)
            {
                p.Position = pos; p.Velocity = vel; p.Tint = tint;
                p.Size = size; p.MaxLife = p.Life = life;
                p.Rotation = 0f;
                p.RotationSpeed = (float)(Random.Shared.NextDouble() * Math.PI * 2 - Math.PI);
                p.IsActive = true;
                return p;
            }
        return null;
    }

    public void Update(float dt)
    {
        foreach (var p in _pool)
            if (p.IsActive)
            {
                p.Life -= dt;
                if (p.Life <= 0) p.Reset();
            }
    }

    public void DrawActive(Action<Particle> drawAction)
    {
        foreach (var p in _pool)
            if (p.IsActive) drawAction(p);
    }
}