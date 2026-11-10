using ZaTool.Engine;

namespace ZaTool.App;

internal sealed class PokemonDetailsDialog : Form
{
    private readonly PokemonDetailsEditor editor;
    internal PokemonPatch? Result { get; private set; }
    internal PokemonDetailsDialog(PokemonDetails snapshot, PokemonPatch initial)
    {
        SuspendLayout(); AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96); Font = new Font("Segoe UI", 10); Text = L.T("Edit details"); ClientSize = new Size(900, 720); MinimumSize = new Size(760, 580); StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(16) }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); Controls.Add(root);
        root.Controls.Add(new Label { Text = L.T("Apply adds changes to the draft only. Save changes in the main window writes the save with backup."), AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 3, 3, 12) }, 0, 0);
        editor = new PokemonDetailsEditor(snapshot.Values, initial, snapshot); root.Controls.Add(editor, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 6, 0, 0) }; var apply = new DarkButton { Text = L.T("Apply to draft"), Size = new Size(180, 36) }; var cancel = new DarkButton { Text = L.T("Cancel"), Size = new Size(120, 36), DialogResult = DialogResult.Cancel }; actions.Controls.Add(apply); actions.Controls.Add(cancel); root.Controls.Add(actions, 0, 2); CancelButton = cancel;
        apply.Click += (_, _) => { try { Result = editor.ReadPatch(); DialogResult = DialogResult.OK; Close(); } catch (Exception ex) { MessageBox.Show(this, L.Backend(ex.Message), L.T("Edit details"), MessageBoxButtons.OK, MessageBoxIcon.Information); } };
        DarkTheme.Apply(this); apply.BackColor = DarkTheme.Accent; apply.ForeColor = DarkTheme.Canvas; ResumeLayout(true);
    }
    protected override void OnShown(EventArgs e) { DarkTheme.Window(this); var work = Screen.FromControl(this).WorkingArea; Size = new Size(Math.Min(Width, work.Width), Math.Min(Height, work.Height)); base.OnShown(e); }
}
