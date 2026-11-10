using ZaTool.Engine;

namespace ZaTool.App;

internal sealed class AddPokemonDialog : Form
{
    private readonly PokemonDetailsEditor editor;
    private readonly ComboBox destination = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 }, box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly Label feedback = new() { Dock = DockStyle.Fill, AutoSize = true };
    private readonly Button submit = new DarkButton { Text = L.T("Add Pokémon"), Size = new Size(180, 36) }, cancel = new DarkButton { Text = L.T("Cancel"), Size = new Size(120, 36), DialogResult = DialogResult.Cancel };
    private readonly Func<AddOptions, Task<bool>> save;
    private bool busy;
    internal AddPokemonDialog(int boxes, Func<AddOptions, Task<bool>> action)
    {
        save = action; SuspendLayout(); AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96); Font = new Font("Segoe UI", 10); Text = L.T("Add Pokémon"); ClientSize = new Size(900, 740); MinimumSize = new Size(760, 580); StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(16) }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); Controls.Add(root);
        var placement = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; placement.Controls.Add(new Label { Text = L.T("Destination"), AutoSize = true, Margin = new Padding(3, 8, 3, 3) }); placement.Controls.Add(destination); placement.Controls.Add(box); root.Controls.Add(placement, 0, 0);
        destination.Items.AddRange([L.T("Automatic: team, then boxes"), L.T("Team"), L.T("Boxes")]); destination.SelectedIndex = 0; box.Items.Add(L.T("First available")); for (int i = 1; i <= boxes; i++) box.Items.Add($"{L.T("Box")} {i}"); box.SelectedIndex = 0; destination.SelectedIndexChanged += (_, _) => box.Enabled = destination.SelectedIndex != 1;
        editor = new PokemonDetailsEditor(new PokemonPatch(), new PokemonPatch { Species = 4, Form = 0, Level = 10, Nature = 15, Shiny = ShinyStyle.NonShiny, Ball = 4 }); root.Controls.Add(editor, 0, 1); root.Controls.Add(feedback, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 6, 0, 0) }; actions.Controls.Add(submit); actions.Controls.Add(cancel); root.Controls.Add(actions, 0, 3); CancelButton = cancel;
        submit.Click += async (_, _) => await Submit(); FormClosing += (_, e) => { if (busy) e.Cancel = true; };
        DarkTheme.Apply(this); submit.BackColor = DarkTheme.Accent; submit.ForeColor = DarkTheme.Canvas; ResumeLayout(true);
    }
    protected override void OnShown(EventArgs e) { DarkTheme.Window(this); var work = Screen.FromControl(this).WorkingArea; Size = new Size(Math.Min(Width, work.Width), Math.Min(Height, work.Height)); base.OnShown(e); }
    private async Task Submit()
    {
        if (busy) return;
        try
        {
            var patch = editor.ReadPatch();
            var options = new AddOptions { Species = patch.Species ?? 4, Form = patch.Form ?? 0, Level = patch.Level ?? 10, Nature = patch.Nature ?? 15, Shiny = patch.Shiny is ShinyStyle.Star or ShinyStyle.Square, Ball = patch.Ball ?? 4, Destination = (Destination)destination.SelectedIndex, PreferredBox = box.SelectedIndex - 1, Details = patch };
            busy = true; editor.Enabled = submit.Enabled = cancel.Enabled = destination.Enabled = box.Enabled = false; UseWaitCursor = true; feedback.Text = L.T("Adding Pokémon with backup…");
            if (await save(options)) { busy = false; DialogResult = DialogResult.OK; Close(); }
        }
        catch (Exception ex) { feedback.Text = L.T("Operation failed: ") + L.Backend(ex.Message); }
        finally { busy = false; editor.Enabled = submit.Enabled = cancel.Enabled = destination.Enabled = true; box.Enabled = destination.SelectedIndex != 1; UseWaitCursor = false; }
    }
}
