using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SovietReigns
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _cardTex;

        // Состояние игры
        private Dictionary<string, int> _stats = new()
        {
            ["ideology"] = 50,
            ["economy"] = 50,
            ["military"] = 50,
            ["trust"] = 50
        };

        private List<Card> _deck = new();
        private int _cardIndex = 0;
        private Card _currentCard;
        private bool _gameOver = false;
        private string _gameOverReason = "";

        // Ввод
        private Vector2 _cardPos = new Vector2(400, 300);
        private bool _isDragging = false;
        private Vector2 _dragStart;
        private Vector2 _dragOffset;
        private MouseState _prevMouse;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Советский Генсек";
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            InitDeck();
            _currentCard = _deck[0];
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");
            _cardTex = Content.Load<Texture2D>("card");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (_gameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                    RestartGame();
                base.Update(gameTime);
                return;
            }

            var mouse = Mouse.GetState();

            // Начало перетаскивания
            if (mouse.LeftButton == ButtonState.Pressed && !_isDragging)
            {
                _isDragging = true;
                _dragStart = new Vector2(mouse.X, mouse.Y);
                _dragOffset = Vector2.Zero;
            }

            // Движение
            if (_isDragging && mouse.LeftButton == ButtonState.Pressed)
            {
                _dragOffset = new Vector2(mouse.X, mouse.Y) - _dragStart;
            }

            // Отпускание / свайп
            if (_isDragging && mouse.LeftButton == ButtonState.Released)
            {
                if (Math.Abs(_dragOffset.X) > 80)
                {
                    ApplyChoice(_dragOffset.X > 0);
                }
                _isDragging = false;
                _dragOffset = Vector2.Zero;
            }

            _prevMouse = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            // Фон статов
            DrawStatBars();

            if (!_gameOver)
            {
                // Карта
                var drawPos = _cardPos + new Vector2(
                    _isDragging ? _dragOffset.X * 0.5f : 0,
                    _isDragging ? Math.Abs(_dragOffset.X) * 0.1f : 0
                );
                _spriteBatch.Draw(_cardTex, new Rectangle((int)drawPos.X - 150, (int)drawPos.Y - 200, 300, 400), Color.White);

                // Текст карты
                DrawCenteredText(_currentCard.Text, new Vector2(400, 220), 200);

                // Подсказки выбора
                DrawCenteredText("⬅ " + _currentCard.LeftChoice, new Vector2(250, 450), 120, Color.Gray);
                DrawCenteredText(_currentCard.RightChoice + " ➡", new Vector2(550, 450), 120, Color.Gray);
            }
            else
            {
                DrawCenteredText(_gameOverReason, new Vector2(400, 250), 300, Color.OrangeRed);
                DrawCenteredText("Нажмите R для перезапуска", new Vector2(400, 400), 200, Color.White);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        // Логика 
        private void InitDeck()
        {
            _deck = new List<Card>
            {
                new Card
                {
                    Text = "Учёные предлагают запустить программу «ИИ для плановой экономики». Партия сомневается.",
                    LeftChoice = "Запретить как буржуазную ересь",
                    RightChoice = "Выделить ресурсы и контролировать",
                    LeftEffects = new Dictionary<string, int> { ["ideology"] = 15, ["economy"] = -10, ["trust"] = -5 },
                    RightEffects = new Dictionary<string, int> { ["economy"] = 10, ["ideology"] = -5, ["military"] = 5 }
                },
                new Card
                {
                    Text = "На заводе в Челябинске рабочие требуют повышения норм выдачи мяса.",
                    LeftChoice = "Отказать. Дисциплина важнее.",
                    RightChoice = "Добавить пайки из резервов",
                    LeftEffects = new Dictionary<string, int> { ["trust"] = -15, ["economy"] = 5, ["ideology"] = 10 },
                    RightEffects = new Dictionary<string, int> { ["trust"] = 10, ["economy"] = -10, ["military"] = -5 }
                },
                new Card
                {
                    Text = "Генштаб предлагает разместить ракеты на границе. НАТО нервничает.",
                    LeftChoice = "Развернуть. Сила решает.",
                    RightChoice = "Отложить. Дипломатия дешевле.",
                    LeftEffects = new Dictionary<string, int> { ["military"] = 20, ["trust"] = -10, ["economy"] = -15 },
                    RightEffects = new Dictionary<string, int> { ["economy"] = 10, ["trust"] = 5, ["military"] = -5 }
                }
            };
            _cardIndex = 0;
        }

        private void ApplyChoice(bool isRight)
        {
            var effects = isRight ? _currentCard.RightEffects : _currentCard.LeftEffects;
            foreach (var kv in effects)
                _stats[kv.Key] = Math.Clamp(_stats[kv.Key] + kv.Value, 0, 100);

            CheckGameOver();
            if (!_gameOver)
            {
                _cardIndex = (_cardIndex + 1) % _deck.Count;
                _currentCard = _deck[_cardIndex];
            }
        }

        private void CheckGameOver()
        {
            foreach (var kv in _stats)
            {
                if (kv.Value <= 0)
                {
                    _gameOver = true;
                    _gameOverReason = kv.Key switch
                    {
                        "ideology" => "Партия потеряла идеологический контроль. Вас сместили на Пленуме.",
                        "economy" => "Коллапс снабжения. Дефицит парализовал страну.",
                        "military" => "Армия потеряла боеспособность. Границы открыты.",
                        "trust" => "Народ вышел на улицы. Революция неизбежна.",
                        _ => "Игра окончена."
                    };
                    break;
                }
                if (kv.Value >= 100)
                {
                    _gameOver = true;
                    _gameOverReason = kv.Key switch
                    {
                        "ideology" => "Культ личности достиг абсолюта. Вы стали мифом, а не правителем.",
                        "economy" => "Гиперплановая экономика задушила инициативу. Стагнация.",
                        "military" => "Милитаризация поглотила всё. Страна стала казармой.",
                        "trust" => "Слепая вера в вождя привела к фанатизму и изоляции.",
                        _ => "Игра окончена."
                    };
                    break;
                }
            }
        }

        private void RestartGame()
        {
            foreach (var k in _stats.Keys.ToList()) _stats[k] = 50;
            _gameOver = false;
            _gameOverReason = "";
            InitDeck();
            _currentCard = _deck[0];
        }

        // Отрисовка
        private void DrawStatBars()
        {
            int x = 50, y = 50, w = 700, h = 20, gap = 30;
            string[] order = { "ideology", "economy", "military", "trust" };
            Color[] colors = { Color.Red, Color.Gold, Color.Gray, Color.Green };
            string[] labels = { "Идеология", "Экономика", "Армия", "Доверие" };

            for (int i = 0; i < 4; i++)
            {
                var val = _stats[order[i]];
                _spriteBatch.Draw(_cardTex, new Rectangle(x, y + i * gap, w, h), Color.DarkGray);
                _spriteBatch.Draw(_cardTex, new Rectangle(x, y + i * gap, w * val / 100, h), colors[i]);
                _spriteBatch.DrawString(_font, $"{labels[i]}: {val}", new Vector2(x, y + i * gap - 18), Color.White);
            }
        }

        private void DrawCenteredText(string text, Vector2 pos, int maxWidth, Color? color = null)
        {
            var col = color ?? Color.White;
            var size = _font.MeasureString(text);
            _spriteBatch.DrawString(_font, text, pos - size / 2f, col);
        }
    }
}