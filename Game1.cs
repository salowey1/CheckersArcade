using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CheckersArcade
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _whitePixel;
        private ExplosionSystem _explosions;

        private GameState _currentState = GameState.Menu;
        private MouseState _currentMouse, _prevMouse;

        // --- МЕНЮ ---
        private readonly List<UiButton> _menuButtons = new();
        private const int MENU_BUTTON_WIDTH = 250;
        private const int MENU_BUTTON_HEIGHT = 50;
        private const int MENU_SPACING = 70;
        private Vector2 _menuCenter;

        // --- ИГРА ---
        private Piece[,] _board = new Piece[8, 8];
        private int _selX = -1, _selY = -1;
        private bool _isRedTurn = true;
        private const int CELL = 80;
        private const int BOARD_OFF_X = 200;
        private const int BOARD_OFF_Y = 40;
        private readonly List<Vector2> _validMoves = new();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166666);
        }

        protected override void Initialize()
        {
            _explosions = new ExplosionSystem(GraphicsDevice, 600, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            try { _font = Content.Load<SpriteFont>("Fonts/Arial"); }
            catch { _font = null; }

            _menuCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, 200);
            _menuButtons.Clear();
            _menuButtons.Add(new UiButton("ИГРАТЬ", MENU_BUTTON_WIDTH, MENU_BUTTON_HEIGHT, _menuCenter, StartGame));
            _menuButtons.Add(new UiButton("ВЫХОД", MENU_BUTTON_WIDTH, MENU_BUTTON_HEIGHT, _menuCenter + new Vector2(0, MENU_SPACING), Exit));
        }

        private void StartGame()
        {
            _currentState = GameState.Playing;
            InitBoard();
            _explosions.Clear();
        }

        private void InitBoard()
        {
            _board = new Piece[8, 8];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    if ((x + y) % 2 != 0)
                    {
                        if (y < 3) _board[x, y] = new Piece { IsRed = false };
                        else if (y > 4) _board[x, y] = new Piece { IsRed = true };
                    }
            _isRedTurn = true;
            _selX = _selY = -1;
            _validMoves.Clear();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            _prevMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentState == GameState.Menu) UpdateMenu();
            else if (_currentState == GameState.Playing)
            {
                UpdateGame(dt);
                _explosions.Update(dt);
            }

            base.Update(gameTime);
        }

        private void UpdateMenu()
        {
            foreach (var btn in _menuButtons) btn.Update(_currentMouse);
            bool leftClick = _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            if (leftClick)
                foreach (var btn in _menuButtons)
                    if (btn.Contains(_currentMouse.X, _currentMouse.Y)) btn.OnClick?.Invoke();
        }

        private void UpdateGame(float dt)
        {
            bool leftClick = _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            if (!leftClick) return;

            int mx = _currentMouse.X;
            int my = _currentMouse.Y;
            int gx = (mx - BOARD_OFF_X) / CELL;
            int gy = (my - BOARD_OFF_Y) / CELL;

            if (gx >= 0 && gx < 8 && gy >= 0 && gy < 8)
                HandleBoardClick(gx, gy);
        }

        private void HandleBoardClick(int gx, int gy)
        {
            var piece = _board[gx, gy];

            if (_selX == -1 && piece != null && piece.IsRed == _isRedTurn)
            {
                _selX = gx; _selY = gy;
                CalcValidMoves(gx, gy);
                return;
            }

            if (_selX != -1 && piece == null && _validMoves.Contains(new Vector2(gx, gy)))
            {
                ExecuteMove(_selX, _selY, gx, gy);
                _selX = _selY = -1;
                _validMoves.Clear();
                _isRedTurn = !_isRedTurn;
                return;
            }

            if (_selX != -1 && piece != null && piece.IsRed == _isRedTurn)
            {
                _selX = gx; _selY = gy;
                CalcValidMoves(gx, gy);
            }
            else
            {
                _selX = _selY = -1;
                _validMoves.Clear();
            }
        }

        private void CalcValidMoves(int x, int y)
        {
            _validMoves.Clear();
            int dir = _isRedTurn ? -1 : 1;
            CheckMove(x + 1, y + dir);
            CheckMove(x - 1, y + dir);
            CheckCapture(x + 2, y + dir * 2, x + 1, y + dir);
            CheckCapture(x - 2, y + dir * 2, x - 1, y + dir);
        }

        private void CheckMove(int tx, int ty)
        {
            if (tx >= 0 && tx < 8 && ty >= 0 && ty < 8 && _board[tx, ty] == null)
                _validMoves.Add(new Vector2(tx, ty));
        }

        private void CheckCapture(int tx, int ty, int mx, int my)
        {
            if (tx >= 0 && tx < 8 && ty >= 0 && ty < 8 && _board[tx, ty] == null)
            {
                var mid = _board[mx, my];
                if (mid != null && mid.IsRed != _isRedTurn)
                    _validMoves.Add(new Vector2(tx, ty));
            }
        }

        private void ExecuteMove(int fx, int fy, int tx, int ty)
        {
            bool isCapture = Math.Abs(tx - fx) == 2;
            _board[tx, ty] = _board[fx, fy];
            _board[fx, fy] = null;

            if (isCapture)
            {
                int mx = (fx + tx) / 2;
                int my = (fy + ty) / 2;
                var captured = _board[mx, my];
                Vector2 worldPos = new(BOARD_OFF_X + mx * CELL + CELL / 2f, BOARD_OFF_Y + my * CELL + CELL / 2f);
                _explosions.SpawnExplosion(worldPos, captured.IsRed ? Color.Crimson : Color.DarkBlue, 20);
                _board[mx, my] = null;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            if (_currentState == GameState.Menu) DrawMenu();
            else if (_currentState == GameState.Playing)
            {
                DrawGame();
                _explosions.Draw(_spriteBatch);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            DrawRect(new Rectangle((int)_menuCenter.X - 150, 80, 300, 60), new Color(30, 30, 40));
            DrawText("АРКАДНЫЕ ШАШКИ", new Vector2(_menuCenter.X - 130, 95), Color.Gold);
            foreach (var btn in _menuButtons) btn.Draw(_spriteBatch, _whitePixel, _font);
        }

        private void DrawGame()
        {
            DrawRect(new Rectangle(BOARD_OFF_X, BOARD_OFF_Y, 8 * CELL, 8 * CELL), Color.DimGray);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    bool isDark = (x + y) % 2 != 0;
                    Color cellCol = isDark ? new Color(80, 60, 40) : new Color(200, 180, 150);
                    DrawRect(new Rectangle(BOARD_OFF_X + x * CELL, BOARD_OFF_Y + y * CELL, CELL, CELL), cellCol);

                    if (_validMoves.Contains(new Vector2(x, y)))
                        DrawCircle(BOARD_OFF_X + x * CELL + CELL / 2, BOARD_OFF_Y + y * CELL + CELL / 2, 10, Color.LimeGreen * 0.6f);

                    var p = _board[x, y];
                    if (p != null)
                    {
                        Color pCol = p.IsRed ? Color.Crimson : Color.Navy;
                        if (x == _selX && y == _selY) pCol = Color.White;
                        DrawCircle(BOARD_OFF_X + x * CELL + CELL / 2, BOARD_OFF_Y + y * CELL + CELL / 2, CELL / 2 - 10, pCol);
                    }
                }

            DrawText(_isRedTurn ? "Ход: КРАСНЫЕ" : "Ход: СИНИЕ", new Vector2(20, 20), Color.White);
            DrawText("ESC - Меню", new Vector2(20, 45), Color.Gray);
        }

        private void DrawRect(Rectangle rect, Color color) => _spriteBatch.Draw(_whitePixel, rect, color);
        private void DrawText(string text, Vector2 pos, Color color)
        {
            if (_font != null) _spriteBatch.DrawString(_font, text, pos, color);
        }

        private void DrawCircle(float x, float y, float radius, Color color)
        {
            int segments = 16;
            float step = MathHelper.Pi * 2 / segments;
            for (int i = 0; i < segments; i++)
            {
                float a1 = i * step, a2 = (i + 1) * step;
                Vector2 p1 = new((float)Math.Cos(a1), (float)Math.Sin(a1)) * radius;
                Vector2 p2 = new((float)Math.Cos(a2), (float)Math.Sin(a2)) * radius;
                DrawLine(x + p1.X, y + p1.Y, x + p2.X, y + p2.Y, 4, color);
            }
            DrawRect(new Rectangle((int)(x - radius + 2), (int)(y - radius + 2), (int)(radius * 2 - 4), (int)(radius * 2 - 4)), color);
        }

        private void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color color)
        {
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);
            float length = Vector2.Distance(new(x1, y1), new(x2, y2));
            _spriteBatch.Draw(_whitePixel, new(x1, y1), null, color, angle, Vector2.Zero, new(length, thickness), SpriteEffects.None, 0);
        }

        private enum GameState { Menu, Playing }
        private class Piece { public bool IsRed; }

        private class UiButton
        {
            public Rectangle Bounds;
            public string Text;
            public Action? OnClick;
            private bool _isHover;
            private static readonly Color HoverCol = new(60, 60, 80);
            private static readonly Color NormalCol = new(40, 40, 60);

            public UiButton(string text, int w, int h, Vector2 pos, Action onClick)
            {
                Text = text;
                Bounds = new Rectangle((int)pos.X - w / 2, (int)pos.Y - h / 2, w, h);
                OnClick = onClick;
            }

            public void Update(MouseState ms) => _isHover = Bounds.Contains(ms.X, ms.Y);
            public bool Contains(int x, int y) => Bounds.Contains(x, y);

            public void Draw(SpriteBatch sb, Texture2D px, SpriteFont? font)
            {
                sb.Draw(px, Bounds, _isHover ? HoverCol : NormalCol);
                if (font != null)
                {
                    Vector2 size = font.MeasureString(Text);
                    Vector2 txtPos = new(Bounds.X + Bounds.Width / 2f - size.X / 2f, Bounds.Y + Bounds.Height / 2f - size.Y / 2f);
                    sb.DrawString(font, Text, txtPos, _isHover ? Color.Gold : Color.White);
                }
            }
        }
    }

    // ==========================================
    // СИСТЕМА ЧАСТИЦ (ВЗРЫВЫ + ФИЗИКА)
    // ==========================================
    public class Particle
    {
        public Vector2 Position, Velocity;
        public float Life, MaxLife, Size, Rotation, RotationSpeed;
        public Color Tint;
        public bool IsActive;
        public void Reset() => IsActive = false;
    }

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

    public class ExplosionSystem
    {
        private readonly ParticlePool _pool;
        private readonly Texture2D _pixel;
        private readonly Rectangle _bounds;
        private const float GRAVITY = 900f;
        private const float BOUNCE = 0.55f;
        private const float FRICTION = 0.92f;

        public ExplosionSystem(GraphicsDevice gd, int max, int w, int h)
        {
            _pool = new ParticlePool(max);
            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _bounds = new Rectangle(0, 0, w, h);
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
}