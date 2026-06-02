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
    private readonly List<UiButton> _buttons = new();
    private readonly Vector2 _center;
    private Texture2D _whitePixel;
    private SpriteFont _font;

    public int SelectedBoardSize { get; private set; } = 8;

    public event Action<int> OnPlayClicked;
    public event Action OnExitClicked;

    public MenuScreen(GraphicsDevice gd, SpriteFont font)
    {
        _font = font;
        _whitePixel = new Texture2D(gd, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _center = new Vector2(gd.Viewport.Width / 2f, 220);

        _buttons.Add(new UiButton("-", 54, 44, _center + new Vector2(-105, 70), () => ChangeBoardSize(-2)));
        _buttons.Add(new UiButton("+", 54, 44, _center + new Vector2(105, 70), () => ChangeBoardSize(2)));
        _buttons.Add(new UiButton("ИГРАТЬ", 250, 50, _center + new Vector2(0, 135), () => OnPlayClicked?.Invoke(SelectedBoardSize)));
        _buttons.Add(new UiButton("ВЫХОД", 250, 50, _center + new Vector2(0, 205), () => OnExitClicked?.Invoke()));
    }

    public void Update(MouseState mouse, bool isLeftClick)
    {
        foreach (var btn in _buttons) btn.Update(mouse);
        if (isLeftClick)
            foreach (var btn in _buttons)
                if (btn.Contains(mouse.X, mouse.Y)) btn.OnClick?.Invoke();
    }

    public void Draw(SpriteBatch sb)
    {
        sb.Draw(_whitePixel, new Rectangle((int)_center.X - 190, 110, 380, 365), new Color(20, 20, 30));
        if (_font != null)
        {
            Vector2 titleSize = _font.MeasureString("ШАШКИ");
            sb.DrawString(_font, "ШАШКИ", new Vector2(_center.X - titleSize.X / 2, 140), Color.Gold);

            string sizeText = $"ПОЛЕ: {SelectedBoardSize} x {SelectedBoardSize}";
            Vector2 size = _font.MeasureString(sizeText);
            sb.DrawString(_font, sizeText, new Vector2(_center.X - size.X / 2, _center.Y + 58), Color.White);

            string rangeText = $"{BoardLayout.MinBoardSize} x {BoardLayout.MinBoardSize} ... {BoardLayout.MaxBoardSize} x {BoardLayout.MaxBoardSize}";
            Vector2 range = _font.MeasureString(rangeText);
            sb.DrawString(_font, rangeText, new Vector2(_center.X - range.X / 2, _center.Y + 92), Color.Gray);
        }
        foreach (var btn in _buttons) btn.Draw(sb, _whitePixel, _font);
    }

    private void ChangeBoardSize(int delta)
    {
        SelectedBoardSize = BoardLayout.ClampBoardSize(SelectedBoardSize + delta);
    }
}
