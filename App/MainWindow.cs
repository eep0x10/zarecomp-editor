using System.Diagnostics;
using System.Text.Json;
using ZaTool.Engine;

namespace ZaTool.App;

public sealed class MainWindow : Form
{
    private readonly SaveService service = new();
    private readonly PokemonWorkspaceControl workspace = new();
    private readonly ComboBox languages = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
    private readonly ComboBox candidates = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly Button browse = new DarkButton(), find = new DarkButton(), add = new DarkButton(), save = new DarkButton(), discard = new DarkButton(), backup = new DarkButton() { Enabled = false };
    private readonly Label summaryLabel = new() { AutoSize = true, Dock = DockStyle.Fill }, hint = new() { AutoSize = true, Dock = DockStyle.Fill }, backupHint = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly TextBox path = new() { ReadOnly = true, Dock = DockStyle.Fill }, status = new() { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TableLayoutPanel content = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20) };
    private readonly string settingsPath = Path.Combine(AppContext.BaseDirectory, "RecompPokemon.settings.json");
    private SaveSummary? summary;
    private string? backupDirectory;
    private bool busy, fillingLanguage;
    private int previousDraftCount;
    private string currentStatus = "Choose a save to view your team and boxes.";

    public MainWindow(string? initialPath)
    {
        SuspendLayout(); AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96); Font = new Font("Segoe UI", 10);
        Text = "Recomp Pokémon"; ClientSize = new Size(1160, 780); MinimumSize = new Size(1000, 650); StartPosition = FormStartPosition.CenterScreen; AllowDrop = true;
        PortableSettings? preferences = null; try { if (File.Exists(settingsPath)) preferences = JsonSerializer.Deserialize<PortableSettings>(File.ReadAllText(settingsPath)); } catch { }
        if (preferences?.Language is "pt-BR" or "en" or "es") L.Language = preferences.Language;
        Controls.Add(content); content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        browse.Size = new Size(125, 32); find.Size = new Size(125, 32); add.Size = new Size(175, 32); save.Size = new Size(190, 32); discard.Size = new Size(180, 32); backup.Size = new Size(145, 32);
        foreach (var button in new[] { browse, find, add, save, discard, backup }) { button.AutoSize = false; button.Margin = new Padding(3); button.Font = Font; button.TextAlign = ContentAlignment.MiddleCenter; }
        var header = new Panel { Dock = DockStyle.Fill };
        var headerActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 3, 0, 0) };
        headerActions.Controls.Add(languages); headerActions.Controls.Add(add); headerActions.Controls.Add(find); headerActions.Controls.Add(browse); header.Controls.Add(headerActions);
        header.Controls.Add(new Label { Text = "RECOMP / POKÉMON", Font = new Font("Segoe UI", 15, FontStyle.Bold), Dock = DockStyle.Left, Width = 260, TextAlign = ContentAlignment.MiddleLeft }); content.Controls.Add(header, 0, 0);
        var saveArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 }; saveArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); saveArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 23)); saveArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); saveArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); saveArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); saveArea.Controls.Add(path, 0, 0); saveArea.Controls.Add(candidates, 1, 0); saveArea.Controls.Add(summaryLabel, 0, 1); saveArea.SetColumnSpan(summaryLabel, 2); saveArea.Controls.Add(hint, 0, 2); saveArea.SetColumnSpan(hint, 2); content.Controls.Add(saveArea, 0, 1); content.Controls.Add(workspace, 0, 2);
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 }; footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; actions.Controls.Add(save); actions.Controls.Add(discard); actions.Controls.Add(backup); footer.Controls.Add(actions, 0, 0); footer.Controls.Add(backupHint, 0, 1); footer.Controls.Add(status, 0, 2); content.Controls.Add(footer, 0, 3);
        languages.Items.AddRange(["Português (BR)", "English", "Español"]); fillingLanguage = true; languages.SelectedIndex = L.Language == "en" ? 1 : L.Language == "es" ? 2 : 0; fillingLanguage = false;
        languages.SelectedIndexChanged += (_, _) => { if (fillingLanguage) return; L.Language = languages.SelectedIndex == 1 ? "en" : languages.SelectedIndex == 2 ? "es" : "pt-BR"; ApplyLanguage(); Remember(); };
        browse.Click += async (_, _) => { using var dialog = new OpenFileDialog { Title = L.T("Choose save"), Filter = "main|main|*.*|*.*", CheckFileExists = true }; if (summary is not null) dialog.InitialDirectory = Path.GetDirectoryName(summary.Path); if (dialog.ShowDialog(this) == DialogResult.OK) await LoadSave(dialog.FileName); };
        find.Click += async (_, _) => await Discover(); candidates.SelectionChangeCommitted += async (_, _) => { if (candidates.SelectedItem is Candidate value) await LoadSave(value.Value.Path); };
        workspace.BoxRequested += async index => await LoadBox(index); workspace.DraftChanged += () => { UpdateActions(); if (!busy) { if (workspace.DraftCount > 0) Message("Changes are drafts. Click Save changes to write them."); else if (previousDraftCount > 0) Message("No pending changes."); } previousDraftCount = workspace.DraftCount; };
        save.Click += async (_, _) => await SaveChanges(); discard.Click += (_, _) => workspace.Discard(); backup.Click += (_, _) => { try { if (backupDirectory is not null) Process.Start(new ProcessStartInfo(backupDirectory) { UseShellExecute = true }); } catch (Exception ex) { Error(ex); } };
        add.Click += async (_, _) => await ShowAddDialog();
        DragEnter += (_, e) => e.Effect = !busy && e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None; DragDrop += async (_, e) => { if (!busy && e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) await LoadSave(files[0]); };
        FormClosing += (_, e) => { if (busy) { e.Cancel = true; Message("Wait for the current operation to finish."); } else if (!ConfirmDiscard()) e.Cancel = true; };
        Shown += async (_, _) => { await Discover(); string? selected = initialPath ?? (File.Exists(preferences?.LastSavePath) ? preferences.LastSavePath : null); if (selected is not null) await LoadSave(selected); };
        DarkTheme.Apply(this); content.BackColor = DarkTheme.Canvas; ApplyLanguage(); ResumeLayout(true);
    }
    protected override void OnShown(EventArgs e) { DarkTheme.Window(this); var work = Screen.FromControl(this).WorkingArea; float scale = DeviceDpi / 96f; Size = new Size(Math.Min((int)(1160 * scale), work.Width), Math.Min((int)(800 * scale), work.Height)); Location = new Point(work.Left + (work.Width - Width) / 2, work.Top + (work.Height - Height) / 2); base.OnShown(e); }
    private void ApplyLanguage() { browse.Text = L.T("Choose save"); find.Text = L.T("Find saves"); add.Text = "+ " + L.T("Add Pokémon"); save.Text = L.T("Save changes"); discard.Text = L.T("Discard drafts"); backup.Text = L.T("Open backup"); languages.AccessibleName = L.T("Language"); hint.Text = L.T("Select the main file in PORT/save/save_0/main or save/…/main. Drag and drop is supported."); backupHint.Text = L.T("Close the game before saving. Automatic backup on every write."); workspace.ApplyLanguage(); RenderSummary(); Message(currentStatus); }
    private async Task Discover() { if (busy) return; SetBusy(true); Message("Finding saves…"); try { var values = await Task.Run(() => service.DiscoverSaves()); candidates.Items.Clear(); foreach (var value in values) candidates.Items.Add(new Candidate(value)); MatchCandidate(); Message(values.Count == 0 ? "No saves found. Choose the main file manually." : "Select a discovered save to load it."); } catch (Exception ex) { Error(ex); } finally { SetBusy(false); } }
    private async Task LoadSave(string file)
    {
        if (busy || !ConfirmDiscard()) return; SetBusy(true); Message("Loading save…");
        try { var loaded = await Task.Run(() => service.Inspect(file)); var team = await Task.Run(() => service.InspectTeam(file)); var box = loaded.BoxCount > 0 ? await Task.Run(() => service.InspectBox(file, 0)) : []; summary = loaded; workspace.Reset(team, loaded.BoxCount); if (loaded.BoxCount > 0) workspace.ShowBox(0, box); path.Text = loaded.Path; RenderSummary(); MatchCandidate(); Message("Save loaded. Changes remain drafts until you save."); Remember(); }
        catch (Exception ex) { Error(ex); } finally { SetBusy(false); }
    }
    private async Task LoadBox(int index) { if (busy || summary is null) return; SetBusy(true); try { workspace.ShowBox(index, await Task.Run(() => service.InspectBox(summary.Path, index))); } catch (Exception ex) { Error(ex); } finally { SetBusy(false); } }
    private async Task SaveChanges()
    {
        if (busy || summary is null || workspace.DraftCount == 0) return; SetBusy(true); Message("Creating backup and saving…");
        try
        {
            int previousBox = workspace.CurrentBoxIndex;
            var result = await Task.Run(() => service.SaveEdits(summary.Path, workspace.TeamEdits, workspace.BoxEdits, summary.Hash)); backupDirectory = result.BackupDirectory;
            string outcome = "Changes saved." + Environment.NewLine + "Backup: " + backupDirectory + (result.Warnings.Count > 0 ? Environment.NewLine + "Warnings:" + Environment.NewLine + string.Join(Environment.NewLine, result.Warnings) : "");
            try { summary = await Task.Run(() => service.Inspect(result.SavePath)); workspace.Reset(result.Team, summary.BoxCount, true); if (summary.BoxCount > 0) workspace.ShowBox(Math.Min(previousBox, summary.BoxCount - 1), await Task.Run(() => service.InspectBox(summary.Path, Math.Min(previousBox, summary.BoxCount - 1)))); RenderSummary(); }
            catch (Exception ex) { summary = null; workspace.Reset([], 0); outcome += Environment.NewLine + "Saved, but could not reload. Reopen the save." + Environment.NewLine + ex.Message; }
            Message(outcome);
        }
        catch (Exception ex) { Error(ex); } finally { SetBusy(false); }
    }
    private void SetBusy(bool value) { busy = value; workspace.Enabled = browse.Enabled = find.Enabled = candidates.Enabled = languages.Enabled = !value; UseWaitCursor = value; UpdateActions(); }
    private void UpdateActions() { save.Enabled = discard.Enabled = !busy && workspace.DraftCount > 0; add.Enabled = !busy && summary is not null; backup.Enabled = !busy && backupDirectory is not null; }
    private void RenderSummary() { summaryLabel.Text = summary is null ? L.T("Choose a save to view your team and boxes.") : $"{L.T("Trainer")}: {summary.Trainer}    ·    {L.T("Team")}: {summary.PartyCount}/6    ·    {L.T("Boxes")}: {summary.BoxCount}    ·    {L.T("Free slots")}: {summary.EmptyBoxSlots}"; }
    private void MatchCandidate() { candidates.SelectedIndex = -1; for (int i = 0; summary is not null && i < candidates.Items.Count; i++) if (candidates.Items[i] is Candidate value && string.Equals(Path.GetFullPath(value.Value.Path), summary.Path, StringComparison.OrdinalIgnoreCase)) { candidates.SelectedIndex = i; break; } }
    private bool ConfirmDiscard() => workspace.DraftCount == 0 || MessageBox.Show(this, L.T("Discard unsaved drafts?"), L.T("Unsaved changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    private void Message(string text) { currentStatus = text; status.Text = L.Status(text); status.SelectionStart = 0; status.ScrollToCaret(); }
    private void Error(Exception ex) => Message("Operation failed: " + ex.Message);
    private void Remember() { try { File.WriteAllText(settingsPath, JsonSerializer.Serialize(new PortableSettings(summary?.Path, L.Language))); } catch { Message(currentStatus + Environment.NewLine + "Preferences could not be saved beside the executable."); } }
    private sealed record PortableSettings(string? LastSavePath, string Language = "pt-BR");
    private sealed record Candidate(SaveCandidate Value) { public override string ToString() => $"{Value.Label} · {Value.LastModified:g}"; }
    private Task ShowAddDialog()
    {
        if (busy || summary is null) return Task.CompletedTask;
        if (workspace.DraftCount > 0) { Message("Save or discard pending drafts before adding a Pokémon."); return Task.CompletedTask; }
        using var dialog = new AddPokemonDialog(summary.BoxCount, async options =>
        {
            SetBusy(true);
            try
            {
                var loaded = summary;
                var result = await Task.Run(() => service.Add(loaded.Path, options, loaded.Hash)); backupDirectory = result.BackupDirectory;
                string outcome = "Pokémon added." + Environment.NewLine + result.Location + Environment.NewLine + "Backup: " + backupDirectory + (!result.Legal ? Environment.NewLine + "External PKHeX report (English):" + Environment.NewLine + result.LegalityReport : "");
                try { summary = await Task.Run(() => service.Inspect(result.SavePath)); var team = await Task.Run(() => service.InspectTeam(result.SavePath)); workspace.Reset(team, summary.BoxCount); if (summary.BoxCount > 0) workspace.ShowBox(0, await Task.Run(() => service.InspectBox(summary.Path, 0))); RenderSummary(); }
                catch (Exception ex) { summary = null; workspace.Reset([], 0); outcome += Environment.NewLine + "Saved, but could not reload. Reopen the save." + Environment.NewLine + ex.Message; }
                Message(outcome); return true;
            }
            finally { SetBusy(false); }
        });
        dialog.ShowDialog(this); return Task.CompletedTask;
    }
}
