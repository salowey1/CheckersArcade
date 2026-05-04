#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CheckersArcade.UI;

public class UiButton
{
    public Rectangle Bounds;
    public string Text { get; }
    public Action? OnClick { get; }
    private bool _isHover;
    private static readonly Color HoverCol = new(60, 60, 80);
    private static readonly Color NormalCol = new(40, 40, 60);
    private static readonly Color ActiveCol = new(80, 70, 120);

    public UiButton(string text, int w, int h, Vector2 pos, Action onClick)
    {
        Text = text;
        Bounds = new Rectangle((int)pos.X - w / 2, (int)pos.Y - h / 2, w, h);
        OnClick = onClick;
    }

    public void Update(MouseState ms) => _isHover = Bounds.Contains(ms.X, ms.Y);
    public bool Contains(int x, int y) => Bounds.Contains(x, y);

    public void Draw(SpriteBatch sb, Texture2D px, SpriteFont font, bool isActive = false)
    {
        Color bg = isActive ? ActiveCol : (_isHover ? HoverCol : NormalCol);
        sb.Draw(px, Bounds, bg);

        Vector2 size = font.MeasureString(Text);
        Vector2 txtPos = new(Bounds.X + Bounds.Width / 2f - size.X / 2f, Bounds.Y + Bounds.Height / 2f - size.Y / 2f);
        Color txtCol = isActive ? Color.Gold : (_isHover ? Color.White : new Color(200, 200, 200));
        sb.DrawString(font, Text, txtPos, txtCol);
    }
}