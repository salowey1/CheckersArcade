#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using CheckersArcade; // ⬅️ Чтобы видеть enum GameMode

namespace CheckersArcade.UI;

public class MenuScreen
{
    private readonly List<UiButton> _buttons = new();
    private readonly Vector2 _center;
    private Texture2D _whitePixel;
    private SpriteFont _font;
    private GameMode _selectedMode = GameMode.Classic;

    public event Action<GameMode>? OnPlayClicked;
    public event Action? OnExitClicked;

    public MenuScreen(GraphicsDevice gd, SpriteFont font)
    {
        _font = font;
        _whitePixel = new Texture2D(gd, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _center = new Vector2(gd.Viewport.Width / 2f, 250);

        _buttons.Add(new UiButton("КЛАССИКА", 250, 50, _center + new Vector2(0, -100), () => _selectedMode = GameMode.Classic));
        _buttons.Add(new UiButton("ФИЗИКА", 250, 50, _center + new Vector2(0, -30), () => _selectedMode = GameMode.Physics));
        _buttons.Add(new UiButton("ИГРАТЬ", 250, 50, _center + new Vector2(0, 60), () => OnPlayClicked?.Invoke(_selectedMode)));
        _buttons.Add(new UiButton("ВЫХОД", 250, 50, _center + new Vector2(0, 130), () => OnExitClicked?.Invoke()));
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
        sb.Draw(_whitePixel, new Rectangle((int)_center.X - 160, 120, 320, 280), new Color(20, 20, 30));
        sb.Draw(_whitePixel, new Rectangle((int)_center.X - 150, 140, 300, 4), Color.Gold);
        sb.Draw(_whitePixel, new Rectangle((int)_center.X - 150, 150, 300, 4), Color.Gold);

        Vector2 titleSize = _font.MeasureString("АРКАДНЫЕ ШАШКИ");
        sb.DrawString(_font, "АРКАДНЫЕ ШАШКИ", new Vector2(_center.X - titleSize.X / 2, 160), Color.Gold);
        sb.DrawString(_font, "Выбери режим:", new Vector2(_center.X - 80, 120), Color.Gray);

        foreach (var btn in _buttons)
        {
            bool isActiveMode = (btn.Text == "КЛАССИКА" && _selectedMode == GameMode.Classic) ||
                                (btn.Text == "ФИЗИКА" && _selectedMode == GameMode.Physics);
            btn.Draw(sb, _whitePixel, _font, isActiveMode);
        }
    }
}