#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using CheckersArcade.Models;

namespace CheckersArcade.UI;

public class MenuScreen
{
    private enum MenuPage
    {
        Main,
        Rules
    }

    private readonly List<UiButton> _mainButtons = new();
    private readonly List<UiButton> _rulesButtons = new();
    private Rectangle _panelBounds;
    private Vector2 _center;
    private Texture2D _whitePixel;
    private SpriteFont _font;
    private int _buttonWidth;
    private MenuPage _page = MenuPage.Main;

    public int SelectedBoardSize { get; private set; } = 8;

    public event Action<int> OnPlayClicked;
    public event Action OnExitClicked;

    public MenuScreen(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _font = font;
        _whitePixel = new Texture2D(graphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });

        Resize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
    }

    public void Resize(int screenWidth, int screenHeight)
    {
        int safeWidth = Math.Max(420, screenWidth);
        int safeHeight = Math.Max(520, screenHeight);
        int panelWidth = Math.Min(560, safeWidth - 40);
        int panelHeight = Math.Min(660, safeHeight - 60);
        _panelBounds = new Rectangle(
            (safeWidth - panelWidth) / 2,
            (safeHeight - panelHeight) / 2,
            panelWidth,
            panelHeight);

        _center = new Vector2(_panelBounds.Center.X, _panelBounds.Center.Y);
        _buttonWidth = Math.Min(320, _panelBounds.Width - 90);
        RebuildButtons();
    }

    private void RebuildButtons()
    {
        _mainButtons.Clear();
        _rulesButtons.Clear();

        _mainButtons.Add(new UiButton("-", 54, 44, new Vector2(_center.X - 150, _panelBounds.Y + 205), () => ChangeBoardSize(-2)));
        _mainButtons.Add(new UiButton("+", 54, 44, new Vector2(_center.X + 150, _panelBounds.Y + 205), () => ChangeBoardSize(2)));
        _mainButtons.Add(new UiButton("ИГРАТЬ", _buttonWidth, 52, new Vector2(_center.X, _panelBounds.Y + 320), () => OnPlayClicked?.Invoke(SelectedBoardSize)));
        _mainButtons.Add(new UiButton("ПРАВИЛА", _buttonWidth, 52, new Vector2(_center.X, _panelBounds.Y + 385), () => _page = MenuPage.Rules));
        _mainButtons.Add(new UiButton("ВЫХОД", _buttonWidth, 52, new Vector2(_center.X, _panelBounds.Y + 450), () => OnExitClicked?.Invoke()));

        _rulesButtons.Add(new UiButton("НАЗАД", _buttonWidth, 52, new Vector2(_center.X, _panelBounds.Bottom - 45), () => _page = MenuPage.Main));
    }

    public void Update(MouseState mouse, bool isLeftClick)
    {
        List<UiButton> buttons = GetActiveButtons();
        foreach (UiButton button in buttons)
        {
            button.Update(mouse);
        }

        if (isLeftClick)
        {
            foreach (UiButton button in buttons)
            {
                if (button.Contains(mouse.X, mouse.Y))
                {
                    button.Click();
                    break;
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawPanel(spriteBatch);

        if (_page == MenuPage.Rules)
        {
            DrawRules(spriteBatch);
        }
        else
        {
            DrawMainMenu(spriteBatch);
        }

        foreach (UiButton button in GetActiveButtons())
        {
            button.Draw(spriteBatch, _whitePixel, _font);
        }
    }

    private void DrawMainMenu(SpriteBatch spriteBatch)
    {
        if (_font != null)
        {
            DrawCenteredText(spriteBatch, "ШАШКИ", _panelBounds.Y + 48, Color.Gold);
            DrawCenteredText(spriteBatch, "Выберите размер поля", _panelBounds.Y + 128, Color.LightGray);

            string sizeText = $"ПОЛЕ: {SelectedBoardSize} x {SelectedBoardSize}";
            DrawCenteredText(spriteBatch, sizeText, _panelBounds.Y + 192, Color.White);

            string rangeText = $"от {BoardLayout.MinBoardSize} x {BoardLayout.MinBoardSize} до {BoardLayout.MaxBoardSize} x {BoardLayout.MaxBoardSize}";
            DrawCenteredText(spriteBatch, rangeText, _panelBounds.Y + 240, Color.Gray);
        }
    }

    private void DrawRules(SpriteBatch spriteBatch)
    {
        if (_font == null)
        {
            return;
        }

        DrawCenteredText(spriteBatch, "ПРАВИЛА", _panelBounds.Y + 42, Color.Gold);

        string[] rules =
        {
            "1. Ходить можно по диагонали на темные клетки.",
            "2. Взятие не обязательно: можно выбрать обычный ход.",
            "3. При взятии шашка соперника удаляется.",
            "4. Ближайшие шашки скользят, сталкиваются и занимают свободные темные клетки.",
            "5. Двойной клик по своей шашке взрывает ее вместо хода.",
            "6. После завершения хода очередь переходит другому игроку.",
            "7. Победа: у соперника нет шашек или доступных ходов."
        };

        const float rulesScale = 0.72f;
        float y = _panelBounds.Y + 105;
        int textWidth = _panelBounds.Width - 72;

        foreach (string rule in rules)
        {
            y = DrawWrappedText(spriteBatch, rule, new Vector2(_panelBounds.X + 36, y), textWidth, Color.White, rulesScale);
            y += 8;
        }

        int dividerY = _panelBounds.Bottom - 92;
        spriteBatch.Draw(_whitePixel, new Rectangle(_panelBounds.X + 35, dividerY, _panelBounds.Width - 70, 1), new Color(55, 55, 80));
    }

    private void DrawPanel(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_whitePixel, _panelBounds, new Color(16, 16, 26));
        spriteBatch.Draw(_whitePixel, new Rectangle(_panelBounds.X, _panelBounds.Y, _panelBounds.Width, 2), new Color(55, 55, 80));
        spriteBatch.Draw(_whitePixel, new Rectangle(_panelBounds.X, _panelBounds.Bottom - 2, _panelBounds.Width, 2), new Color(55, 55, 80));
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float y, Color color)
    {
        Vector2 size = _font.MeasureString(text);
        spriteBatch.DrawString(_font, text, new Vector2(_center.X - size.X / 2f, y), color);
    }

    private float DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, int maxWidth, Color color)
    {
        return DrawWrappedText(spriteBatch, text, position, maxWidth, color, 1f);
    }

    private float DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, int maxWidth, Color color, float scale)
    {
        string[] words = text.Split(' ');
        string line = "";
        float y = position.Y;

        foreach (string word in words)
        {
            string testLine = line.Length == 0 ? word : line + " " + word;
            if (_font.MeasureString(testLine).X * scale > maxWidth && line.Length > 0)
            {
                DrawText(spriteBatch, line, new Vector2(position.X, y), color, scale);
                y += (_font.LineSpacing + 4) * scale;
                line = word;
            }
            else
            {
                line = testLine;
            }
        }

        if (line.Length > 0)
        {
            DrawText(spriteBatch, line, new Vector2(position.X, y), color, scale);
            y += (_font.LineSpacing + 4) * scale;
        }

        return y;
    }

    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
    {
        spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private List<UiButton> GetActiveButtons()
    {
        return _page == MenuPage.Rules ? _rulesButtons : _mainButtons;
    }

    private void ChangeBoardSize(int delta)
    {
        SelectedBoardSize = BoardLayout.ClampBoardSize(SelectedBoardSize + delta);
    }
}
