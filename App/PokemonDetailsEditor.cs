using System.Reflection;
using ZaTool.Engine;

namespace ZaTool.App;

internal sealed class PokemonDetailsEditor : UserControl
{
    private sealed record Binding(PropertyInfo Property, Control Control, Func<object?> Read, Action<object?> Write, CheckBox? Override);
    private readonly List<Binding> bindings = [];
    private readonly PokemonPatch baseline;
    private readonly bool adding;
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly ComboBox species = Combo(true), form = Combo(), gender = Combo();
    private readonly MoveSetEditor plus = new();
    private readonly Label evTotal = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly List<NumericUpDown> evControls = [];
    private bool filling;

    internal PokemonDetailsEditor(PokemonPatch original, PokemonPatch initial, PokemonDetails? details = null)
    {
        SuspendLayout(); Dock = DockStyle.Fill; baseline = original; adding = details is null;
        Controls.Add(tabs);
        var identity = Page("Identity");
        Choice(identity, "Species", "Species", Catalog.Species, species, required: true);
        Choice(identity, "Form", "Form", [], form, required: true);
        TextField(identity, "Nickname", "Nickname", 12);
        Toggle(identity, "IsNicknamed", "Use a nickname");
        Toggle(identity, "Favorite", "Favorite");
        Choice(identity, "Gender", "Gender", [], gender);
        Number(identity, "Level", "Level", 1, 100, true);
        Choice(identity, "Nature", "Original nature", Catalog.Natures.Select(x => new Choice(x.Id, L.Nature(x.Id))).ToArray(), required: true);
        Choice(identity, "StatNature", "Effective nature (mint)", Catalog.Natures.Select(x => new Choice(x.Id, L.Nature(x.Id))).ToArray());
        Note(identity, "An effective nature different from the original requires an available Mint. Neutral natures have no Mint effect.");
        Choice(identity, "Shiny", "Shiny style", [new(0, L.T("Non-shiny")), new(1, L.T("Star shiny")), new(2, L.T("Square shiny"))], required: true);
        Choice(identity, "Ball", "Poké Ball", Catalog.Balls, required: true);
        Choice(identity, "HeldItem", "Held item", new[] { new Choice(0, L.T("None")) }.Concat(Catalog.HeldItems).DistinctBy(x => x.Id));
        Note(identity, "Only persistent forms supported by the engine are offered. Changing identity does not guarantee legality.");

        var attributes = Page("Attributes");
        Stats(attributes, "IVs", "IVs", 31); Stats(attributes, "EVs", "EVs", 252); Wide(attributes, evTotal);
        Flags(attributes, "HyperTrained", "Hyper Training");
        Note(attributes, "Hyper Training is separate from IVs and requires level 50. EVs are limited to 252 per stat and 510 total.");
        Number(attributes, "Friendship", "Friendship", 0, 255);
        Markings(attributes);

        var movePage = Page("Moves");
        MoveArray(movePage, "Moves", "Move");
        Bind(movePage, "PlusMoves", "Registered + moves", plus, () => plus.Values, x => plus.SetValues(x as int[] ?? []));
        Note(movePage, "Registered + moves are stored flags, not a bypass for learnability. The engine validates selections.");
        if (details is not null)
        {
            ReadOnly(movePage, "Stored PP", string.Join(" / ", details.Values.PP ?? []));
            ReadOnly(movePage, "Stored PP Ups", string.Join(" / ", details.Values.PPUps ?? []));
            ReadOnly(movePage, "Stored relearn moves", string.Join(" / ", (details.Values.RelearnMoves ?? []).Select(id => Catalog.Moves.FirstOrDefault(x => x.Id == id)?.Name ?? id.ToString())));
        }
        Note(movePage, "Z-A does not use PP or PP Ups in battle. Stored PP, PP Ups and relearn moves are preserved rather than edited here.");

        var origin = Page("Origin");
        TextField(origin, "OriginalTrainerName", "Original trainer name", 12);
        Choice(origin, "OriginalTrainerGender", "Original trainer gender", [new(0, L.T("Male")), new(1, L.T("Female"))]);
        Number(origin, "TID16", "Trainer ID (16-bit)", 0, 65535); Number(origin, "SID16", "Secret ID (16-bit)", 0, 65535);
        Choice(origin, "Language", "Pokémon language", Catalog.Languages);
        Number(origin, "MetLevel", "Encounter level", 1, 100); Choice(origin, "MetLocation", "Encounter location", Catalog.MetLocations, search: true);
        var date = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = !adding && original.MetDate is null, MinDate = new DateTime(2000, 1, 1), MaxDate = new DateTime(2255, 12, 31) };
        Bind(origin, "MetDate", "Encounter date", date, () => date.Checked ? DateOnly.FromDateTime(date.Value) : null, x => { date.Checked = adding || x is DateOnly; if (x is DateOnly value) date.Value = value.ToDateTime(TimeOnly.MinValue); });
        Number(origin, "ObedienceLevel", "Obedience level", 0, 100);
        Note(origin, "Origin and trainer values affect identity and encounter legality. Backups do not guarantee online compatibility.");

        var advanced = Page("Advanced");
        Toggle(advanced, "IsAlpha", "Alpha"); Note(advanced, "Alpha has encounter and size constraints. A changed flag alone does not create a valid Alpha encounter.");
        Number(advanced, "HeightScalar", "Height scalar", 0, 255); Number(advanced, "WeightScalar", "Weight scalar", 0, 255); Number(advanced, "Scale", "Scale", 0, 255);
        var ribbons = new[] { new Choice(-1, L.T("None")) }.Concat((details?.ReadOnly.Ribbons ?? []).Select(id => Catalog.Ribbons.FirstOrDefault(x => x.Id == id) ?? new Choice(id, "#" + id))).ToArray();
        Choice(advanced, "AffixedRibbon", "Affixed title", ribbons);
        Note(advanced, "Only already-owned ribbons can be affixed. This editor does not grant ribbons, eggs, abilities or raw internal identifiers.");
        if (details is not null)
        {
            var value = details.ReadOnly;
            ReadOnly(advanced, "PID / EC", $"{value.PID:X8} / {value.EncryptionConstant:X8}");
            ReadOnly(advanced, "HOME tracker", value.Tracker.ToString("X16"));
            ReadOnly(advanced, "Origin game", value.OriginGame.ToString());
            ReadOnly(advanced, "Ability / slot", $"{value.Ability} / {value.AbilitySlot}");
            ReadOnly(advanced, "Displayed trainer IDs", $"{value.DisplayTID} / {value.DisplaySID}");
            ReadOnly(advanced, "Handler", $"{value.CurrentHandler} · {value.HandlingTrainerName}");
            ReadOnly(advanced, "Egg data", $"{value.IsEgg} · {value.EggLocation} · {value.EggDate}");
            ReadOnly(advanced, "Owned ribbons", string.Join(", ", value.Ribbons.Select(id => Catalog.Ribbons.FirstOrDefault(x => x.Id == id)?.Name ?? "#" + id)));
            if (value.Unsupported.Count > 0) ReadOnly(advanced, "Preserved fields", string.Join(Environment.NewLine, value.Unsupported.Select(L.T)));
            Wide(advanced, new Label { Text = L.T("Original PKHeX report (English; before this draft):"), AutoSize = true });
            Wide(advanced, new TextBox { Text = details.LegalityReport, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Height = 120, Dock = DockStyle.Top });
            if (details.Warnings.Count > 0) ReadOnly(advanced, "Warnings:", string.Join(Environment.NewLine, details.Warnings.Select(L.Backend)));
        }

        species.SelectedIndexChanged += (_, _) => { if (!filling) UpdateForms(); };
        form.SelectedIndexChanged += (_, _) => { if (!filling) UpdateGenders(); };
        filling = true;
        foreach (var binding in bindings) binding.Write(binding.Property.GetValue(initial));
        UpdateForms(initial.Form); UpdateGenders(initial.Gender);
        foreach (var binding in bindings)
            if (binding.Override is { } check) { check.Checked = binding.Property.GetValue(initial) is not null && !PokemonPatchTools.Same(binding.Property.GetValue(initial), binding.Property.GetValue(original)); binding.Control.Enabled = check.Checked; }
        filling = false; UpdateEvs(); DarkTheme.Apply(this); ResumeLayout(true);
    }

    internal PokemonPatch ReadPatch()
    {
        var patch = new PokemonPatch();
        foreach (var binding in bindings)
        {
            if (binding.Override is { Checked: false }) continue;
            object? value = binding.Read();
            if (binding.Property.PropertyType == typeof(ShinyStyle?) && value is int id) value = (ShinyStyle)id;
            if (!PokemonPatchTools.Same(value, binding.Property.GetValue(baseline))) binding.Property.SetValue(patch, value);
        }
        if (patch.EVs?.Sum() > 510) throw new InvalidOperationException(L.T("Total EVs cannot exceed 510."));
        if (patch.HyperTrained?.Any(x => x) == true && (patch.Level ?? baseline.Level ?? 1) < 50) throw new InvalidOperationException(L.T("Hyper Training requires level 50."));
        if (patch.StatNature is not null && patch.Nature is null && !adding) patch = patch with { Nature = baseline.Nature };
        return patch;
    }
    private void UpdateForms(int? preferred = null)
    {
        int id = Selected(species); int old = preferred ?? (form.SelectedItem as Choice)?.Id ?? 0;
        form.DataSource = Catalog.Forms(id).Select(x => x with { Name = L.T(x.Name) }).ToArray(); SetChoice(form, old, preferred.HasValue); UpdateGenders();
    }
    private void UpdateGenders(int? preferred = null)
    {
        if (species.SelectedItem is not Choice chosen || form.SelectedItem is not Choice shape) return;
        int old = preferred ?? (gender.SelectedItem as Choice)?.Id ?? 0;
        gender.DataSource = Catalog.Genders(chosen.Id, shape.Id).Select(x => x with { Name = L.T(x.Id == 0 ? "Male" : x.Id == 1 ? "Female" : "Genderless") }).ToArray(); SetChoice(gender, old, preferred.HasValue);
        plus.SetChoices(Catalog.PlusMoves(chosen.Id, shape.Id));
    }
    private void Choice(TableLayoutPanel grid, string property, string caption, IEnumerable<Choice> choices, ComboBox? control = null, bool required = false, bool search = false)
    {
        var combo = control ?? Combo(search); combo.DataSource = choices.ToArray();
        Bind(grid, property, caption, combo, () => Selected(combo), x => { if (x is not null) SetChoice(combo, Convert.ToInt32(x), true); }, required);
    }
    private void TextField(TableLayoutPanel grid, string property, string caption, int max) { var text = new TextBox { Dock = DockStyle.Fill, MaxLength = max }; Bind(grid, property, caption, text, () => text.Text, x => text.Text = x as string ?? ""); }
    private void Number(TableLayoutPanel grid, string property, string caption, int min, int max, bool required = false) { var number = new NumericUpDown { Dock = DockStyle.Fill, Minimum = min, Maximum = max }; Bind(grid, property, caption, number, () => (int)number.Value, x => { int value = x is null ? min : Convert.ToInt32(x); number.Minimum = Math.Min(min, value); number.Maximum = Math.Max(max, value); number.Value = value; }, required); }
    private void Toggle(TableLayoutPanel grid, string property, string caption) { var check = new CheckBox { AutoSize = true }; Bind(grid, property, caption, check, () => check.Checked, x => check.Checked = x is true); }
    private void Stats(TableLayoutPanel grid, string property, string caption, int max)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6, RowCount = 2 }; var controls = new List<NumericUpDown>(); string[] names = ["HP", "Attack", "Defense", "Speed", "Sp. Atk", "Sp. Def"];
        for (int i = 0; i < 6; i++) { panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6)); var number = new NumericUpDown { Minimum = 0, Maximum = max, Dock = DockStyle.Fill, AccessibleName = caption + " " + L.T(names[i]) }; controls.Add(number); panel.Controls.Add(new Label { Text = L.T(names[i]), AutoSize = true }, i, 0); panel.Controls.Add(number, i, 1); if (property == "EVs") { evControls.Add(number); number.ValueChanged += (_, _) => UpdateEvs(); } }
        Bind(grid, property, caption, panel, () => controls.Select(x => (int)x.Value).ToArray(), x => { var values = x as int[]; for (int i = 0; i < 6; i++) { int value = values is not null ? values[i] : max == 31 ? 31 : 0; controls[i].Minimum = Math.Min(0, value); controls[i].Maximum = Math.Max(max, value); controls[i].Value = value; } });
    }
    private void Flags(TableLayoutPanel grid, string property, string caption)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true }; string[] names = ["HP", "Attack", "Defense", "Speed", "Sp. Atk", "Sp. Def"]; var flags = names.Select(x => new CheckBox { Text = L.T(x), AutoSize = true }).ToArray(); panel.Controls.AddRange(flags);
        Bind(grid, property, caption, panel, () => flags.Select(x => x.Checked).ToArray(), x => { var values = x as bool[]; for (int i = 0; i < flags.Length; i++) flags[i].Checked = values is not null && values[i]; });
    }
    private void Markings(TableLayoutPanel grid)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 }; var values = Enumerable.Range(0, 6).Select(_ => Combo()).ToArray();
        for (int i = 0; i < 3; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
        for (int i = 0; i < values.Length; i++) { values[i].DataSource = new[] { new Choice(0, L.T("Unmarked")), new Choice(1, L.T("Blue")), new Choice(2, L.T("Red")) }; values[i].AccessibleName = L.T("Marking") + " " + (i + 1); panel.Controls.Add(values[i], i % 3, i / 3); }
        Bind(grid, "Markings", "Organization markings", panel, () => values.Select(Selected).ToArray(), x => { var existing = x as int[]; for (int i = 0; i < values.Length; i++) SetChoice(values[i], existing?[i] ?? 0, true); });
    }
    private void MoveArray(TableLayoutPanel grid, string property, string caption)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 }; panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); var controls = Enumerable.Range(0, 4).Select(_ => Combo(true)).ToArray();
        for (int i = 0; i < controls.Length; i++) { controls[i].DataSource = Catalog.Moves.Select(x => x.Id == 0 ? x with { Name = L.T("(Empty)") } : x).ToArray(); controls[i].AccessibleName = L.T(caption) + " " + (i + 1); panel.Controls.Add(controls[i]); }
        Bind(grid, property, "Moves", panel, () => controls.Select(Selected).ToArray(), x => { var values = x as int[]; for (int i = 0; i < controls.Length; i++) SetChoice(controls[i], values?[i] ?? 0, true); });
    }
    private void UpdateEvs() { int total = evControls.Sum(x => (int)x.Value); evTotal.Text = $"EV: {total} / 510"; evTotal.ForeColor = total > 510 ? Color.Salmon : DarkTheme.Muted; }
    private void Bind(TableLayoutPanel grid, string property, string caption, Control control, Func<object?> read, Action<object?> write, bool required = false)
    {
        var info = typeof(PokemonPatch).GetProperty(property) ?? throw new InvalidOperationException(property);
        CheckBox? check = adding && !required ? new CheckBox { Text = L.T(caption), AutoSize = true, Margin = new Padding(3, 7, 3, 5) } : null;
        int row = grid.RowCount++; grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(check ?? (Control)new Label { Text = L.T(caption), AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 8, 6, 6) }, 0, row);
        control.Margin = new Padding(3, 5, 3, 5); control.AccessibleName = L.T(caption); grid.Controls.Add(control, 1, row);
        if (check is not null) check.CheckedChanged += (_, _) => control.Enabled = check.Checked;
        bindings.Add(new(info, control, read, write, check));
    }
    private TableLayoutPanel Page(string name) { var page = new TabPage(L.T(name)) { Padding = new Padding(12) }; tabs.TabPages.Add(page); var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true }; page.Controls.Add(scroll); var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 }; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); scroll.Controls.Add(grid); if (adding) Note(grid, "Checked optional fields override automatic values; unchecked fields remain automatic."); return grid; }
    private static void Note(TableLayoutPanel grid, string text) => Wide(grid, new Label { Text = L.T(text), AutoSize = true, Dock = DockStyle.Fill, ForeColor = DarkTheme.Muted });
    private static void ReadOnly(TableLayoutPanel grid, string caption, string text) { int row = grid.RowCount++; grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); grid.Controls.Add(new Label { Text = L.T(caption), AutoSize = true, Dock = DockStyle.Fill }, 0, row); grid.Controls.Add(new TextBox { Text = text, ReadOnly = true, Dock = DockStyle.Fill, Multiline = text.Contains('\n'), Height = text.Contains('\n') ? 70 : 26 }, 1, row); }
    private static void Wide(TableLayoutPanel grid, Control control) { int row = grid.RowCount++; grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); control.Margin = new Padding(3, 8, 3, 5); grid.Controls.Add(control, 0, row); grid.SetColumnSpan(control, 2); }
    internal static ComboBox Combo(bool search = false) => new() { BindingContext = new BindingContext(), Dock = DockStyle.Fill, DropDownStyle = search ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList, AutoCompleteMode = search ? AutoCompleteMode.SuggestAppend : AutoCompleteMode.None, AutoCompleteSource = search ? AutoCompleteSource.ListItems : AutoCompleteSource.None, IntegralHeight = false, DropDownHeight = 250 };
    internal static int Selected(ComboBox combo)
    {
        if (combo.SelectedItem is Choice selected && (combo.DropDownStyle == ComboBoxStyle.DropDownList || string.IsNullOrWhiteSpace(combo.Text) || selected.Name.Equals(combo.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))) return selected.Id;
        var choices = combo.DataSource as Choice[] ?? combo.Items.Cast<Choice>().ToArray();
        return choices.FirstOrDefault(x => x.Name.Equals(combo.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))?.Id ?? throw new InvalidOperationException(L.T("Select a valid value: ") + combo.AccessibleName);
    }
    private static void SetChoice(ComboBox combo, int id, bool preserve = false)
    {
        combo.BindingContext ??= new BindingContext();
        if (combo.DataSource is Choice[] values)
        {
            int index = Array.FindIndex(values, x => x.Id == id);
            if (index >= 0) { combo.SelectedIndex = index; return; }
            if (preserve) { combo.DataSource = values.Append(new Choice(id, L.T("Stored value") + " #" + id)).ToArray(); combo.SelectedIndex = values.Length; return; }
        }
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }
}

internal sealed class MoveSetEditor : UserControl
{
    private readonly ComboBox input = PokemonDetailsEditor.Combo(true);
    private readonly ListBox list = new() { Dock = DockStyle.Fill };
    internal int[] Values => list.Items.Cast<Choice>().Select(x => x.Id).ToArray();
    internal MoveSetEditor()
    {
        Dock = DockStyle.Top; Height = 160; var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 }; grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); Controls.Add(grid);
        var add = new DarkButton { Text = "+", Dock = DockStyle.Fill }; var remove = new DarkButton { Text = L.T("Remove"), Dock = DockStyle.Fill }; grid.Controls.Add(input, 0, 0); grid.Controls.Add(add, 1, 0); grid.Controls.Add(remove, 2, 0); grid.Controls.Add(list, 0, 1); grid.SetColumnSpan(list, 3);
        add.Click += (_, _) => { try { int id = PokemonDetailsEditor.Selected(input); if (!Values.Contains(id)) list.Items.Add(Catalog.Moves.First(x => x.Id == id)); } catch (Exception ex) { MessageBox.Show(this, ex.Message, L.T("Registered + moves")); } }; remove.Click += (_, _) => { if (list.SelectedIndex >= 0) list.Items.RemoveAt(list.SelectedIndex); };
    }
    internal void SetValues(int[] values) { list.Items.Clear(); foreach (int id in values.Distinct()) list.Items.Add(Catalog.Moves.FirstOrDefault(x => x.Id == id) ?? new Choice(id, "#" + id)); }
    internal void SetChoices(IReadOnlyList<Choice> choices) { input.DataSource = choices.ToArray(); input.Enabled = choices.Count > 0; }
}
