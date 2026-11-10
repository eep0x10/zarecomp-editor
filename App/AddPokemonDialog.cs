using ZaTool.Engine;

namespace ZaTool.App;

internal sealed class AddPokemonDialog : Form
{
    private readonly ComboBox species = ChoiceBox(true), form = ChoiceBox(), nature = ChoiceBox(), gender = ChoiceBox(), ball = ChoiceBox(), destination = ChoiceBox(), box = ChoiceBox();
    private readonly NumericUpDown level = Number(1, 100, 10);
    private readonly CheckBox shiny = new() { AutoSize = true, Text = "★ " + L.T("Shiny") }, manualMoves = new() { AutoSize = true, Text = L.T("Choose moves manually") }, custom = new() { AutoSize = true, Text = L.T("Customize IVs / EVs") };
    private readonly TextBox nickname = new() { Dock = DockStyle.Fill, MaxLength = 12, PlaceholderText = L.T("Use species name") };
    private readonly ComboBox[] moves = Enumerable.Range(0, 4).Select(_ => ChoiceBox(true)).ToArray();
    private readonly NumericUpDown[] ivs = Enumerable.Range(0, 6).Select(_ => Number(0, 31, 31)).ToArray(), evs = Enumerable.Range(0, 6).Select(_ => Number(0, 252, 0)).ToArray();
    private readonly Label total = new() { AutoSize = true }, feedback = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly Button submit = new DarkButton() { Text = L.T("Add Pokémon"), AutoSize = false, Size = new Size(180, 38) }, cancel = new DarkButton() { Text = L.T("Cancel"), AutoSize = false, Size = new Size(120, 38), DialogResult = DialogResult.Cancel };
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly Func<AddOptions, Task<bool>> save;
    private bool busy;

    public AddPokemonDialog(int boxes, Func<AddOptions, Task<bool>> action)
    {
        save = action; SuspendLayout(); AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96); Font = new Font("Segoe UI", 10); Text = L.T("Add Pokémon"); ClientSize = new Size(780, 700); MinimumSize = new Size(700, 580); StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(18) }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Controls.Add(root); root.Controls.Add(tabs, 0, 0); root.Controls.Add(feedback, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft }; actions.Controls.Add(submit); actions.Controls.Add(cancel); root.Controls.Add(actions, 0, 2);
        var basics = Page(L.T("Add Pokémon")); var details = Page(L.T("Optional details"));
        AddField(basics, "Species", species); AddField(basics, "Form", form); AddField(basics, "Level", level); AddField(basics, "Nature", nature); AddField(basics, "Shiny", shiny); AddField(basics, "Destination", destination); AddField(basics, "Preferred box", box);
        AddField(details, "Nickname", nickname); AddField(details, "Gender", gender); AddField(details, "Poké Ball", ball); Wide(details, manualMoves);
        for (int i = 0; i < 4; i++) AddField(details, L.T("Move") + " " + (i + 1), moves[i]);
        Wide(details, custom);
        var stats = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 7, RowCount = 3 }; stats.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        string[] names = ["HP", "Attack", "Defense", "Speed", "Sp. Atk", "Sp. Def"];
        stats.Controls.Add(new Label { Text = "IV", AutoSize = true }, 0, 1); stats.Controls.Add(new Label { Text = "EV", AutoSize = true }, 0, 2);
        for (int i = 0; i < 6; i++) { stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6)); stats.Controls.Add(new Label { Text = L.T(names[i]), AutoSize = true, Dock = DockStyle.Fill }, i + 1, 0); ivs[i].AccessibleName = "IV " + L.T(names[i]); evs[i].AccessibleName = "EV " + L.T(names[i]); stats.Controls.Add(ivs[i], i + 1, 1); stats.Controls.Add(evs[i], i + 1, 2); }
        Wide(details, stats); Wide(details, total); Wide(details, new Label { Text = L.T("IVs 0–31; EVs 0–252 each, 510 total. Automatic moves follow species and level."), AutoSize = true, Dock = DockStyle.Fill });
        species.DataSource = Catalog.Species.ToArray(); nature.DataSource = Catalog.Natures.Select(x => new Choice(x.Id, L.Nature(x.Id))).ToArray(); ball.DataSource = Catalog.Balls.ToArray();
        SelectId(species, 4); SelectId(nature, 15); SelectId(ball, 4);
        destination.Items.AddRange([L.T("Automatic: team, then boxes"), L.T("Team"), L.T("Boxes")]); destination.SelectedIndex = 0; box.Items.Add(L.T("First available")); for (int i = 1; i <= boxes; i++) box.Items.Add($"{L.T("Box")} {i}"); box.SelectedIndex = 0;
        foreach (var combo in moves) { combo.DataSource = new[] { new Choice(0, L.T("(Empty)")) }.Concat(Catalog.Moves.Where(x => x.Id != 0)).ToArray(); combo.Enabled = false; }
        foreach (var number in ivs.Concat(evs)) number.Enabled = false;
        species.SelectedIndexChanged += (_, _) => UpdateForm(); form.SelectedIndexChanged += (_, _) => UpdateGender(); destination.SelectedIndexChanged += (_, _) => box.Enabled = destination.SelectedIndex != 1;
        manualMoves.CheckedChanged += (_, _) => { foreach (var combo in moves) combo.Enabled = manualMoves.Checked; }; custom.CheckedChanged += (_, _) => { foreach (var number in ivs.Concat(evs)) number.Enabled = custom.Checked; };
        foreach (var number in evs) number.ValueChanged += (_, _) => UpdateTotal();
        submit.Click += async (_, _) => await Submit(); FormClosing += (_, e) => { if (busy) e.Cancel = true; }; CancelButton = cancel;
        UpdateForm(); UpdateTotal(); DarkTheme.Apply(this); submit.BackColor = DarkTheme.Accent; submit.ForeColor = DarkTheme.Canvas; ResumeLayout(true);
    }
    protected override void OnShown(EventArgs e) { DarkTheme.Window(this); var work = Screen.FromControl(this).WorkingArea; if (Height > work.Height) Height = work.Height; base.OnShown(e); }
    private async Task Submit()
    {
        if (busy) return;
        try
        {
            var selectedSpecies = Resolve(species, "Species"); if ((species.SelectedItem as Choice)?.Id != selectedSpecies.Id) species.SelectedItem = selectedSpecies;
            if (custom.Checked && evs.Sum(x => x.Value) > 510) throw new InvalidOperationException(L.T("Total EVs cannot exceed 510."));
            var options = new AddOptions { Species = selectedSpecies.Id, Form = Resolve(form, "Form").Id, Nature = Resolve(nature, "Nature").Id, Gender = Resolve(gender, "Gender").Id, Ball = Resolve(ball, "Poké Ball").Id, Nickname = nickname.Text.Trim(), Level = (int)level.Value, Shiny = shiny.Checked, Destination = (Destination)destination.SelectedIndex, PreferredBox = box.SelectedIndex - 1, Moves = manualMoves.Checked ? moves.Select(x => Resolve(x, "Move").Id).ToArray() : null, IVs = custom.Checked ? ivs.Select(x => (int)x.Value).ToArray() : null, EVs = custom.Checked ? evs.Select(x => (int)x.Value).ToArray() : null };
            busy = true; tabs.Enabled = submit.Enabled = cancel.Enabled = false; UseWaitCursor = true; feedback.Text = L.T("Adding Pokémon with backup…");
            if (await save(options)) { DialogResult = DialogResult.OK; busy = false; Close(); } else feedback.Text = L.T("Operation failed: ") + L.T("Save or discard pending drafts before adding a Pokémon.");
        }
        catch (Exception ex) { feedback.Text = L.T("Operation failed: ") + L.Backend(ex.Message); }
        finally { busy = false; tabs.Enabled = submit.Enabled = cancel.Enabled = true; UseWaitCursor = false; }
    }
    private void UpdateForm() { if (species.SelectedItem is Choice selected) { form.DataSource = Catalog.Forms(selected.Id).Select(x => x with { Name = L.T(x.Name) }).ToArray(); UpdateGender(); } }
    private void UpdateGender() { if (species.SelectedItem is Choice selected && form.SelectedItem is Choice shape) gender.DataSource = Catalog.Genders(selected.Id, shape.Id).Select(x => x with { Name = L.T(x.Name) }).ToArray(); }
    private void UpdateTotal() { int value = evs.Sum(x => (int)x.Value); total.Text = $"EV: {value} / 510"; total.ForeColor = value > 510 ? Color.Salmon : DarkTheme.Muted; }
    private TableLayoutPanel Page(string title) { var page = new TabPage(title) { Padding = new Padding(12) }; tabs.TabPages.Add(page); var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true }; page.Controls.Add(scroll); var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 }; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); scroll.Controls.Add(grid); return grid; }
    private static void AddField(TableLayoutPanel grid, string text, Control control) { int row = grid.RowCount++; grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); grid.Controls.Add(new Label { Text = L.T(text), AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 10, 3, 6) }, 0, row); control.Margin = new Padding(3, 6, 3, 6); control.AccessibleName = L.T(text); grid.Controls.Add(control, 1, row); }
    private static void Wide(TableLayoutPanel grid, Control control) { int row = grid.RowCount++; grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); control.Margin = new Padding(3, 8, 3, 5); grid.Controls.Add(control, 0, row); grid.SetColumnSpan(control, 2); }
    private static NumericUpDown Number(int min, int max, int value) => new() { Minimum = min, Maximum = max, Value = value, Dock = DockStyle.Fill };
    private static ComboBox ChoiceBox(bool search = false) => new() { Dock = DockStyle.Fill, DropDownStyle = search ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList, AutoCompleteMode = search ? AutoCompleteMode.SuggestAppend : AutoCompleteMode.None, AutoCompleteSource = search ? AutoCompleteSource.ListItems : AutoCompleteSource.None, IntegralHeight = false, DropDownHeight = 250 };
    private static Choice Resolve(ComboBox combo, string name) => combo.Items.Cast<Choice>().FirstOrDefault(x => x.Name.Equals(combo.Text.Trim(), StringComparison.CurrentCultureIgnoreCase)) ?? throw new InvalidOperationException(L.T("Select a valid value: ") + L.T(name));
    private static void SelectId(ComboBox combo, int id) { for (int i = 0; i < combo.Items.Count; i++) if (combo.Items[i] is Choice value && value.Id == id) { combo.SelectedIndex = i; break; } }
}
