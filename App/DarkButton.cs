namespace ZaTool.App;

internal sealed class DarkButton : Button
{
    protected override void OnPaint(PaintEventArgs e)
    {
        Color background = Enabled && ClientRectangle.Contains(PointToClient(Cursor.Position)) ? ControlPaint.Light(BackColor, .12f) : BackColor;
        using var fill = new SolidBrush(background); e.Graphics.FillRectangle(fill, ClientRectangle);
        using var border = new Pen(Focused ? DarkTheme.Accent : Color.FromArgb(65, 79, 96)); e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        bool star = Text is "★" or "☆";
        TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(star ? 1 : 5, 2, Math.Max(0, Width - (star ? 2 : 10)), Math.Max(0, Height - 4)), Enabled ? ForeColor : DarkTheme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -4, -4), ForeColor, BackColor);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Invalidate(); }
}
