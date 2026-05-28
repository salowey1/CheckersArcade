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
    public Action OnClick { get; }
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

    public void Draw(SpriteBatch sb, Texture2D px, SpriteFont font)
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
