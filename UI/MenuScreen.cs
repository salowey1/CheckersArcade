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
    private SpriteFont? _font;
    private GameMode _selectedMode = GameMode.Classic;

    public event Action<GameMode>? OnPlayClicked;
    public event Action? OnExitClicked;

    public MenuScreen(GraphicsDevice gd, SpriteFont? font)
    {
        _font = font;
        _whitePixel = new Texture2D(gd, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _center = new Vector2(gd.Viewport.Width / 2f, 250);

        _buttons.Add(new UiButton("ÊËÀÑÑÈÊÀ", 250, 50, _center + new Vector2(0, -100), () => _selectedMode = GameMode.Classic, true));
        _buttons.Add(new UiButton("ÔÈÇÈÊÀ", 250, 50, _center + new Vector2(0, -30), () => _selectedMode = GameMode.Physics, true));
        _buttons.Add(new UiButton("ÈÃÐÀÒÜ", 250, 50, _center + new Vector2(0, 60), () => OnPlayClicked?.Invoke(_selectedMode)));
        _buttons.Add(new UiButton("ÂÛÕÎÄ", 250, 50, _center + new Vector2(0, 130), () => OnExitClicked?.Invoke()));
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

        if (_font != null)
        {
            Vector2 titleSize = _font.MeasureString("ÀÐÊÀÄÍÛÅ ØÀØÊÈ");
            sb.DrawString(_font, "ÀÐÊÀÄÍÛÅ ØÀØÊÈ", new Vector2(_center.X - titleSize.X / 2, 160), Color.Gold);
            sb.DrawString(_font, "Âûáåðè ðåæèì:", new Vector2(_center.X - 80, 120), Color.Gray);
        }

        foreach (var btn in _buttons)
        {
            bool isActiveMode = (btn.Text == "ÊËÀÑÑÈÊÀ" && _selectedMode == GameMode.Classic) ||
                                (btn.Text == "ÔÈÇÈÊÀ" && _selectedMode == GameMode.Physics);
            btn.Draw(sb, _whitePixel, _font, isActiveMode);
        }
    }
}

// Âñïîìîãàòåëüíûé êëàññ êíîïêè (äîáàâëåí ïàðàìåòð isModeSelector äëÿ ïîäñâåòêè)
public class UiButton
{
    public Rectangle Bounds;
    public string Text { get; }
    public Action? OnClick { get; }
    private bool _isHover;
    private static readonly Color HoverCol = new(60, 60, 80);
    private static readonly Color NormalCol = new(40, 40, 60);
    private static readonly Color ActiveCol = new(80, 70, 120); // Ïîäñâåòêà âûáðàííîãî ðåæèìà

    public UiButton(string text, int w, int h, Vector2 pos, Action onClick)
    {
        Text = text;
        Bounds = new Rectangle((int)pos.X - w / 2, (int)pos.Y - h / 2, w, h);
        OnClick = onClick;
    }

    public void Update(MouseState ms) => _isHover = Bounds.Contains(ms.X, ms.Y);
    public bool Contains(int x, int y) => Bounds.Contains(x, y);

    public void Draw(SpriteBatch sb, Texture2D px, SpriteFont? font, bool isActive = false)
    {
        Color bg = isActive ? ActiveCol : (_isHover ? HoverCol : NormalCol);
        sb.Draw(px, Bounds, bg);
        if (font != null)
        {
            Vector2 size = font.MeasureString(Text);
            Vector2 txtPos = new(Bounds.X + Bounds.Width / 2f - size.X / 2f, Bounds.Y + Bounds.Height / 2f - size.Y / 2f);
            Color txtCol = isActive ? Color.Gold : (_isHover ? Color.White : new Color(200, 200, 200));
            sb.DrawString(font, Text, txtPos, txtCol);
        }
    }
}