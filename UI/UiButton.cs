#nullable disable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CheckersArcade.UI;

public class UiButton
{
    private static readonly Color HoverColor = new(60, 60, 80);
    private static readonly Color NormalColor = new(40, 40, 60);

    private readonly Action _onClick;
    private bool _isHover;

    public Rectangle Bounds { get; }
    public string Text { get; }

    public UiButton(string text, int width, int height, Vector2 center, Action onClick)
    {
        Text = text;
        Bounds = new Rectangle((int)center.X - width / 2, (int)center.Y - height / 2, width, height);
        _onClick = onClick;
    }

    public void Update(MouseState mouse) => _isHover = Bounds.Contains(mouse.X, mouse.Y);

    public bool Contains(int x, int y) => Bounds.Contains(x, y);

    public void Click()
    {
        _onClick?.Invoke();
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        spriteBatch.Draw(pixel, Bounds, _isHover ? HoverColor : NormalColor);

        if (font != null)
        {
            Vector2 size = font.MeasureString(Text);
            Vector2 textPosition = new(
                Bounds.X + Bounds.Width / 2f - size.X / 2f,
                Bounds.Y + Bounds.Height / 2f - size.Y / 2f);

            spriteBatch.DrawString(font, Text, textPosition, _isHover ? Color.Gold : Color.White);
        }
    }
}
