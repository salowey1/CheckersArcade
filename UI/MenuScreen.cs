#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CheckersArcade.UI;

public class MenuScreen
{
    private readonly List<UiButton> _buttons = new();
    private readonly Vector2 _center;
    private Texture2D _whitePixel;
    private SpriteFont _font;

    public event Action? OnPlayClicked;
    public event Action? OnExitClicked;

    public MenuScreen(GraphicsDevice gd, SpriteFont font)
    {
        _font = font;
        _whitePixel = new Texture2D(gd, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _center = new Vector2(gd.Viewport.Width / 2f, 250);

        _buttons.Add(new UiButton("ИГРАТЬ", 250, 50, _center, () => OnPlayClicked?.Invoke()));
        _buttons.Add(new UiButton("ВЫХОД", 250, 50, _center + new Vector2(0, 70), () => OnExitClicked?.Invoke()));
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
        sb.Draw(_whitePixel, new Rectangle((int)_center.X - 160, 120, 320, 200), new Color(20, 20, 30));
        if (_font != null)
        {
            Vector2 titleSize = _font.MeasureString("АРКАДНЫЕ ШАШКИ");
            sb.DrawString(_font, "АРКАДНЫЕ ШАШКИ", new Vector2(_center.X - titleSize.X / 2, 140), Color.Gold);
        }
        foreach (var btn in _buttons) btn.Draw(sb, _whitePixel, _font);
    }
}