public class ExplosionSystem
{
    private readonly ParticlePool _pool;
    private readonly Texture2D _pixel;
    private readonly Rectangle _screenBounds;
    private readonly float _gravity = 800f;
    private readonly float _bounceDamping = 0.6f;

    public ExplosionSystem(GraphicsDevice device, int maxParticles, int width, int height)
    {
        _pool = new ParticlePool(maxParticles);
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _screenBounds = new Rectangle(0, 0, width, height);
    }

    public void SpawnExplosion(Vector2 worldPos, Color baseColor, int count = 15)
    {
        var rand = new Random();
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(rand.NextDouble() * Math.PI * 2);
            float speed = (float)(rand.NextDouble() * 300 + 150);
            Vector2 vel = new Vector2((float)Math.Cos(angle) * speed,
                                      (float)Math.Sin(angle) * speed - 200); // начальный импульс вверх

            float size = (float)(rand.NextDouble() * 8 + 4);
            float life = (float)(rand.NextDouble() * 1.2 + 0.5);
            Color tint = Color.Lerp(baseColor, Color.White, (float)rand.NextDouble() * 0.5f);

            _pool.Get(worldPos, vel, tint, size, life);
        }
    }

    public void Update(float dt)
    {
        _pool.Update(dt);
        foreach (var p in _pool.ActiveParticles)
        {
            // Гравитация
            p.Velocity.Y += _gravity * dt;

            // Перемещение
            p.Position += p.Velocity * dt;
            p.Rotation += p.RotationSpeed * dt;

            // Коллизия с границами экрана/доски
            if (p.Position.X < 0)
            { p.Position.X = 0; p.Velocity.X *= -_bounceDamping; }
            if (p.Position.X > _screenBounds.Width)
            { p.Position.X = _screenBounds.Width; p.Velocity.X *= -_bounceDamping; }
            if (p.Position.Y > _screenBounds.Height - p.Size)
            {
                p.Position.Y = _screenBounds.Height - p.Size;
                p.Velocity.Y *= -_bounceDamping;
                p.Velocity.X *= 0.9f; // трение
            }
            if (p.Position.Y < 0)
            { p.Position.Y = 0; p.Velocity.Y *= -_bounceDamping; }
        }
    }

    public void Draw(SpriteBatch sb)
    {
        foreach (var p in _pool.ActiveParticles)
        {
            float alpha = MathHelper.Clamp(p.Life / p.MaxLife, 0f, 1f);
            Color finalColor = p.Tint * alpha;
            Vector2 origin = new Vector2(0.5f);
            sb.Draw(_pixel, p.Position, null, finalColor, p.Rotation, origin, p.Size, SpriteEffects.None, 0f);
        }
    }
}