using System.Runtime.InteropServices;

namespace ZaTool.App;

internal static class DarkTheme
{
    public static readonly Color Canvas = Color.FromArgb(19, 24, 34), Surface = Color.FromArgb(29, 36, 48), Input = Color.FromArgb(40, 49, 64), Text = Color.FromArgb(234, 239, 247), Muted = Color.FromArgb(162, 176, 194), Accent = Color.FromArgb(75, 211, 177);
    public static void Apply(Control root)
    {
        root.BackColor = root is TextBox or ComboBox or NumericUpDown ? Input : root is Button ? Input : Surface;
        root.ForeColor = Text;
        if (root is Button button) { button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderColor = Color.FromArgb(65, 79, 96); button.Cursor = Cursors.Hand; }
        if (root is ComboBox combo && combo.DrawMode == DrawMode.Normal)
        {
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.DrawItem += (_, e) =>
            {
                bool active = (e.State & DrawItemState.Selected) != 0;
                using var brush = new SolidBrush(active ? Color.FromArgb(43, 83, 85) : Input); e.Graphics.FillRectangle(brush, e.Bounds);
                string text = e.Index >= 0 ? combo.GetItemText(combo.Items[e.Index]) ?? "" : combo.Text;
                TextRenderer.DrawText(e.Graphics, text, combo.Font, e.Bounds, combo.Enabled ? Text : Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
            };
        }
        foreach (Control child in root.Controls) Apply(child);
    }
    public static void Window(Form form)
    {
        try { int dark = 1; DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int)); int backdrop = 3; DwmSetWindowAttribute(form.Handle, 38, ref backdrop, sizeof(int)); } catch { /* Dark opaque surfaces remain available on unsupported Windows. */ }
    }
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int length);
}
