using ZaTool.Engine;

namespace ZaTool.App;

internal sealed class PokemonWorkspaceControl : UserControl
{
    private readonly TableLayoutPanel teamGrid = Grid(2, 3), boxGrid = Grid(6, 5);
    private readonly Label teamHeading = Label("", 11, true), boxHeading = Label("", 11, true), title = Label("", 18, true), info = Label("", 10), reason = Label("", 10), pending = Label("", 10);
    private readonly ComboBox boxSelector = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button optimize = new DarkButton() { AutoSize = false, Height = 36, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9) };
    private readonly Button editDetails = new DarkButton() { Height = 34, Dock = DockStyle.Top };
    private readonly NumericUpDown level = new() { Minimum = 1, Maximum = 100, Dock = DockStyle.Fill };
    private readonly ComboBox nature = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox shiny = new() { AutoSize = true, Text = "★ Shiny" };
    private readonly Label levelCaption = Label("", 10, true), natureCaption = Label("", 10, true), recommendationCaption = Label("", 10, true);
    private readonly PictureBox preview = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, MinimumSize = new Size(0, 140) };
    private readonly Dictionary<(bool Party, int Slot), TeamPokemon> originals = [];
    private readonly Dictionary<(bool Party, int Slot), TeamEdit> drafts = [];
    private readonly Dictionary<int, string> recommendations = [];
    private readonly Dictionary<(bool Party, int Slot), SlotView> views = [];
    private readonly PokemonSpriteCache sprites = new();
    private readonly Dictionary<PictureBox, int> imageVersions = [];
    private CancellationTokenSource cancellation = new();
    private (bool Party, int Slot)? selected;
    private int boxIndex;
    private bool filling;
    private IReadOnlyList<TeamPokemon> team = [], currentBox = [];
    public event Action<int>? BoxRequested;
    public event Action? DraftChanged;
    public event Action<bool, int, TeamPokemon, TeamEdit?>? EditDetailsRequested;
    public int DraftCount => drafts.Count;
    internal int CurrentBoxIndex => boxIndex;
    public IReadOnlyList<TeamEdit> TeamEdits => Export(true);
    public IReadOnlyList<TeamEdit> BoxEdits => Export(false);
    private IReadOnlyList<TeamEdit> Export(bool party) => drafts.Where(x => x.Key.Party == party).Select(x => x.Value with { Level = x.Value.Level == originals[x.Key].Level ? null : x.Value.Level, Nature = x.Value.Nature == originals[x.Key].Nature ? null : x.Value.Nature, Shiny = x.Value.Shiny == originals[x.Key].Shiny ? null : x.Value.Shiny }).ToArray();

    public PokemonWorkspaceControl()
    {
        Dock = DockStyle.Fill;
        var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 12, 0, 12) };
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23)); columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48)); columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29)); Controls.Add(columns);
        var left = Section(); var middle = Section(); var right = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16, 0, 0, 0) };
        columns.Controls.Add(left, 0, 0); columns.Controls.Add(middle, 1, 0); columns.Controls.Add(right, 2, 0);
        left.Controls.Add(teamHeading, 0, 0); left.Controls.Add(teamGrid, 0, 1); left.Controls.Add(optimize, 0, 2);
        var boxHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Margin = new Padding(8, 0, 8, 8) };
        boxHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); boxHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); boxHeader.Controls.Add(boxHeading, 0, 0); boxHeader.Controls.Add(boxSelector, 1, 0);
        middle.Controls.Add(boxHeader, 0, 0); middle.Controls.Add(boxGrid, 0, 1); middle.Controls.Add(pending, 0, 2);
        var details = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 }; details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); right.Controls.Add(details);
        foreach (Control control in new Control[] { title, preview, info, levelCaption, level, natureCaption, nature, shiny, editDetails, recommendationCaption, reason }) { control.Margin = new Padding(4, 3, 4, 3); details.RowStyles.Add(new RowStyle(control == preview ? SizeType.Absolute : SizeType.AutoSize, control == preview ? 102 : 0)); details.Controls.Add(control); }
        editDetails.Click += (_, _) => { if (selected is { } key && originals.TryGetValue(key, out var pokemon)) EditDetailsRequested?.Invoke(key.Party, key.Slot, pokemon, drafts.GetValueOrDefault(key)); };
        preview.MinimumSize = new Size(0, 96); preview.Height = 96;
        boxSelector.SelectedIndexChanged += (_, _) => { if (!filling && boxSelector.SelectedIndex >= 0) BoxRequested?.Invoke(boxSelector.SelectedIndex); };
        level.ValueChanged += (_, _) => EditSelected(); level.TextChanged += (_, _) => EditSelected(); nature.SelectedIndexChanged += (_, _) => EditSelected(); shiny.CheckedChanged += (_, _) => EditSelected();
        optimize.Click += (_, _) =>
        {
            foreach (var original in team) { var key = (true, original.Slot); var pokemon = DisplayPokemon(key); var suggestion = NatureAdvisor.Suggest(pokemon, L.Language); recommendations[pokemon.Slot] = suggestion.Reason; var before = Effective(key); Store(key, before with { Nature = suggestion.Nature, Details = before.Details is { } patch && suggestion.Nature != before.Nature ? patch with { Nature = null, StatNature = null } : before.Details }); }
            RefreshCards(); FillDetails(); Changed();
        };
        DarkTheme.Apply(this); ApplyLanguage();
    }
    public void Reset(IReadOnlyList<TeamPokemon> newTeam, int boxes, bool preserveSelection = false)
    {
        drafts.Clear(); recommendations.Clear(); originals.Clear(); if (!preserveSelection) selected = null; team = newTeam; currentBox = [];
        foreach (var pokemon in team) originals[(true, pokemon.Slot)] = pokemon;
        filling = true; boxSelector.Items.Clear(); for (int i = 0; i < boxes; i++) boxSelector.Items.Add($"{L.T("Box")} {i + 1}"); boxSelector.SelectedIndex = boxes > 0 ? 0 : -1; filling = false;
        boxIndex = 0; Render(); Changed();
    }
    public void ShowBox(int index, IReadOnlyList<TeamPokemon> contents)
    {
        boxIndex = index; currentBox = contents;
        foreach (var pokemon in contents) originals[(false, pokemon.Slot)] = pokemon;
        if (selected is { Party: false } key && key.Slot / 30 != index) selected = contents.Count > 0 ? (false, contents[0].Slot) : null;
        filling = true; boxSelector.SelectedIndex = index; filling = false; Render();
    }
    public void Discard() { drafts.Clear(); recommendations.Clear(); Render(); Changed(); }
    public void ApplyLanguage()
    {
        teamHeading.Text = L.T("Your team"); boxHeading.Text = L.T("Boxes"); optimize.Text = L.T("Optimize team"); levelCaption.Text = L.T("Level"); natureCaption.Text = L.T("Nature"); recommendationCaption.Text = L.T("Recommendation"); shiny.Text = "★ " + L.T("Shiny");
        level.AccessibleName = L.T("Level"); nature.AccessibleName = L.T("Nature"); boxSelector.AccessibleName = L.T("Box"); editDetails.Text = L.T("Edit details");
        filling = true; int old = boxSelector.SelectedIndex; for (int i = 0; i < boxSelector.Items.Count; i++) boxSelector.Items[i] = $"{L.T("Box")} {i + 1}"; boxSelector.SelectedIndex = old;
        nature.DataSource = Catalog.Natures.Select(x => new Choice(x.Id, L.Nature(x.Id))).ToArray(); filling = false;
        foreach (int slot in recommendations.Keys.ToArray()) if (originals.ContainsKey((true, slot))) recommendations[slot] = NatureAdvisor.Suggest(DisplayPokemon((true, slot)), L.Language).Reason;
        Render(); Changed();
    }
    private void Render()
    {
        cancellation.Cancel(); cancellation.Dispose(); cancellation = new(); imageVersions.Clear();
        foreach (Control control in teamGrid.Controls.Cast<Control>().Concat(boxGrid.Controls.Cast<Control>()).ToArray()) control.Dispose(); teamGrid.Controls.Clear(); boxGrid.Controls.Clear(); views.Clear();
        for (int i = 0; i < 6; i++) AddSlot(teamGrid, true, i, team.FirstOrDefault(x => x.Slot == i), i % 2, i / 2);
        for (int i = 0; i < 30; i++) { int global = boxIndex * 30 + i; AddSlot(boxGrid, false, global, currentBox.FirstOrDefault(x => x.Slot == global), i % 6, i / 6); }
        if (selected is null) { if (team.Count > 0) selected = (true, team[0].Slot); else if (currentBox.Count > 0) selected = (false, currentBox[0].Slot); }
        RefreshCards(); FillDetails();
    }
    private void AddSlot(TableLayoutPanel grid, bool inParty, int slot, TeamPokemon? pokemon, int col, int row)
    {
        var display = pokemon is null ? null : DisplayPokemon((inParty, slot));
        var view = new SlotView(display, inParty, () => Select((inParty, slot)), () => ToggleShiny((inParty, slot)));
        grid.Controls.Add(view, col, row);
        if (pokemon is not null) { views[(inParty, slot)] = view; LoadImage(view.Image, display!, Effective((inParty, slot)).Shiny ?? pokemon.Shiny); }
    }
    private void Select((bool Party, int Slot) key) { selected = key; RefreshCards(); FillDetails(); }
    private TeamEdit Effective((bool Party, int Slot) key) => drafts.TryGetValue(key, out var value) ? value : new TeamEdit(key.Slot, originals[key].Level, originals[key].Nature, originals[key].Shiny);
    private TeamPokemon DisplayPokemon((bool Party, int Slot) key)
    {
        var original = originals[key]; var edit = Effective(key); var patch = edit.Details;
        int species = patch?.Species ?? original.Species, form = patch?.Form ?? original.Form; int[] moves = patch?.Moves ?? original.Moves;
        return original with { Species = species, Form = form, Name = species != original.Species ? Catalog.Species.FirstOrDefault(x => x.Id == species)?.Name ?? original.Name : original.Name, Level = edit.Level ?? original.Level, Nature = edit.Nature ?? original.Nature, Shiny = edit.Shiny ?? original.Shiny, Moves = moves, MoveCategories = moves.Select(NatureAdvisor.GetMoveCategory).ToArray(), BaseStats = Catalog.Forms(species).Any(x => x.Id == form) ? Catalog.BaseStats(species, form) : original.BaseStats };
    }
    private void Store((bool Party, int Slot) key, TeamEdit edit) { var p = originals[key]; if (edit.Level == p.Level && edit.Nature == p.Nature && edit.Shiny == p.Shiny && PokemonPatchTools.Empty(edit.Details)) drafts.Remove(key); else drafts[key] = edit; }
    internal void ApplyDetails(bool party, int slot, PokemonDetails snapshot, PokemonPatch patch)
    {
        var key = (party, slot); if (!originals.TryGetValue(key, out var original)) return;
        var values = PokemonPatchTools.Merge(snapshot.Values, patch);
        Store(key, new TeamEdit(slot, values.Level ?? original.Level, values.StatNature ?? original.Nature, values.Shiny is ShinyStyle.Star or ShinyStyle.Square) { Details = PokemonPatchTools.Empty(patch) ? null : patch });
        Render(); Changed();
    }
    private void EditSelected()
    {
        if (filling || selected is not { } key || !originals.ContainsKey(key) || nature.SelectedItem is not Choice choice) return;
        bool previousShiny = Effective(key).Shiny ?? false;
        int enteredLevel = int.TryParse(level.Text, out int entered) && entered is >= 1 and <= 100 ? entered : (int)level.Value;
        var before = Effective(key); var patch = before.Details;
        if (patch is not null) { if (enteredLevel != before.Level) patch = patch with { Level = null }; if (choice.Id != before.Nature) patch = patch with { Nature = null, StatNature = null }; if (shiny.Checked != before.Shiny) patch = patch with { Shiny = null }; }
        Store(key, new TeamEdit(key.Slot, enteredLevel, choice.Id, shiny.Checked) { Details = patch }); RefreshCards();
        if (previousShiny != shiny.Checked) { LoadImage(preview, DisplayPokemon(key), shiny.Checked); if (views.TryGetValue(key, out var view)) LoadImage(view.Image, DisplayPokemon(key), shiny.Checked); }
        Changed();
    }
    private void ToggleShiny((bool Party, int Slot) key) { var edit = Effective(key); Store(key, edit with { Shiny = !(edit.Shiny ?? false), Details = edit.Details is { } patch ? patch with { Shiny = null } : null }); Select(key); if (views.TryGetValue(key, out var view)) LoadImage(view.Image, DisplayPokemon(key), Effective(key).Shiny ?? false); Changed(); }
    private void RefreshCards() { foreach (var pair in views) pair.Value.Update(Effective(pair.Key), selected == pair.Key, drafts.ContainsKey(pair.Key)); }
    private void FillDetails()
    {
        filling = true;
        bool valid = selected is { } key && originals.ContainsKey(key); level.Enabled = nature.Enabled = shiny.Enabled = editDetails.Enabled = valid;
        if (!valid) { title.Text = L.T("Select a Pokémon"); info.Text = L.T("Choose a Pokémon to edit. Clicking an empty slot never creates one."); reason.Text = L.T("Optimize previews heuristic nature recommendations based on stats and moves. There is no universally best nature."); preview.Image?.Dispose(); preview.Image = null; }
        else
        {
            var id = selected!.Value; var pokemon = DisplayPokemon(id); var edit = Effective(id);
            title.Text = pokemon.Name; info.Text = $"#{pokemon.Species:0000}  ·  {(id.Party ? L.T("Team") : L.T("Box") + " " + (pokemon.Slot / 30 + 1))}\n{L.T("Form")}: {L.T(Catalog.Forms(pokemon.Species).FirstOrDefault(x => x.Id == pokemon.Form)?.Name ?? "Default")}";
            level.Value = Math.Clamp(edit.Level ?? pokemon.Level, 1, 100); shiny.Checked = edit.Shiny ?? pokemon.Shiny; nature.SelectedItem = nature.Items.Cast<Choice>().FirstOrDefault(x => x.Id == edit.Nature);
            reason.Text = id.Party && recommendations.TryGetValue(pokemon.Slot, out var explanation) ? explanation : L.T("Optimize previews heuristic nature recommendations based on stats and moves. There is no universally best nature.");
            LoadImage(preview, pokemon, shiny.Checked);
        }
        filling = false;
    }
    private void Changed() { pending.Text = drafts.Count == 0 ? L.T("No pending changes.") : $"{L.T("Pending")}: {drafts.Count}"; DraftChanged?.Invoke(); }
    private async void LoadImage(PictureBox target, TeamPokemon pokemon, bool isShiny)
    {
        int version = imageVersions.TryGetValue(target, out int old) ? old + 1 : 1; imageVersions[target] = version;
        var previousImage = target.Image; target.Image = null; previousImage?.Dispose();
        var token = cancellation.Token; var image = await sprites.LoadAsync(pokemon.Species, pokemon.Form, Catalog.Forms(pokemon.Species).FirstOrDefault(x => x.Id == pokemon.Form)?.Name ?? "", isShiny, token);
        if (token.IsCancellationRequested || IsDisposed || target.IsDisposed || !imageVersions.TryGetValue(target, out int current) || current != version) { image?.Dispose(); return; }
        var previous = target.Image; target.Image = image; target.AccessibleName = image is null ? L.T("Artwork unavailable") : pokemon.Name; previous?.Dispose();
    }
    protected override void Dispose(bool disposing) { if (disposing) { cancellation.Cancel(); cancellation.Dispose(); preview.Image?.Dispose(); } base.Dispose(disposing); }
    private static Label Label(string text, float size, bool bold = false) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };
    private static TableLayoutPanel Grid(int columns, int rows) { var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows }; for (int i = 0; i < columns; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns)); for (int i = 0; i < rows; i++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows)); return grid; }
    private static TableLayoutPanel Section() { var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(5) }; table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); table.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); return table; }
    private sealed class SlotView : Panel
    {
        public PictureBox Image { get; } = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
        private readonly Label caption = new() { Dock = DockStyle.Bottom, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5f) };
        private readonly Button star = new DarkButton() { Size = new Size(26, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12), Margin = Padding.Empty };
        private readonly TeamPokemon? pokemon;
        private readonly bool partySlot;
        private readonly Panel layout;
        public SlotView(TeamPokemon? value, bool inParty, Action select, Action toggle)
        {
            pokemon = value; partySlot = inParty; Dock = DockStyle.Fill; Margin = new Padding(3); Padding = new Padding(2); BackColor = DarkTheme.Input; TabStop = value is not null;
            layout = new Panel { Dock = DockStyle.Fill, BackColor = DarkTheme.Surface }; Controls.Add(layout);
            caption.Height = inParty ? 34 : 20; layout.Controls.Add(Image); layout.Controls.Add(caption); layout.Controls.Add(star); star.BringToFront();
            layout.SizeChanged += (_, _) => PositionStar();
            DarkTheme.Apply(layout); star.FlatAppearance.BorderSize = 0;
            if (value is null) { caption.Text = "—"; star.Visible = false; AccessibleName = L.T("Empty slot"); return; }
            AccessibleName = value.Name; star.AccessibleName = L.T("Shiny") + " " + value.Name; star.Click += (_, _) => toggle();
            foreach (Control control in new Control[] { this, layout, Image, caption }) { control.Cursor = Cursors.Hand; control.Click += (_, _) => select(); }
            KeyDown += (_, e) => { if (e.KeyCode is Keys.Enter or Keys.Space) { select(); e.Handled = true; } };
        }
        public void Update(TeamEdit edit, bool selected, bool changed) { if (pokemon is null) return; BackColor = selected ? DarkTheme.Accent : DarkTheme.Input; caption.Text = partySlot ? $"{pokemon.Name}\n{L.T("Lv.")} {edit.Level}{(changed ? " •" : "")}" : pokemon.Name + (changed ? " •" : ""); star.Text = edit.Shiny == true ? "★" : "☆"; star.ForeColor = edit.Shiny == true ? Color.Gold : Color.FromArgb(213, 224, 237); AccessibleDescription = $"{pokemon.Name}, {L.T("Level")} {edit.Level}, {(edit.Shiny == true ? L.T("Shiny") : L.T("Normal"))}"; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyDpiMetrics(); }
        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); ApplyDpiMetrics(); }
        private int Pixels(int logical) => (int)Math.Round(logical * DeviceDpi / 96f);
        private void ApplyDpiMetrics()
        {
            // Cards are created after the form has scaled, so their fixed dimensions
            // must be converted explicitly instead of relying on the initial form pass.
            SuspendLayout();
            Margin = new Padding(Pixels(3)); Padding = new Padding(Pixels(2));
            star.Size = new Size(Pixels(26), Pixels(26));
            caption.Height = Pixels(partySlot ? 34 : 20);
            PositionStar(); ResumeLayout(true);
        }
        private void PositionStar() => star.Location = new Point(Math.Max(0, layout.ClientSize.Width - star.Width - Pixels(1)), Pixels(1));
        protected override void Dispose(bool disposing) { if (disposing) Image.Image?.Dispose(); base.Dispose(disposing); }
    }
}
