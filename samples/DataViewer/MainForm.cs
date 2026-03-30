using Parsec.Shaiya.Data;

namespace Sample.DataViewer;

public sealed partial class MainForm : Form
{
    private Data? _data;

    public MainForm()
    {
        InitializeComponent();
    }

    // ──────────────────────────────────────────────
    // Toolbar actions
    // ──────────────────────────────────────────────

    private void btnOpen_Click(object sender, EventArgs e)
    {
        using var sahDialog = new OpenFileDialog
        {
            Title = "Select data.sah file",
            Filter = "SAH Index File (*.sah)|*.sah|All Files (*.*)|*.*",
            FileName = "data.sah"
        };

        if (sahDialog.ShowDialog() != DialogResult.OK)
            return;

        string sahPath = sahDialog.FileName;
        string safPath = Path.ChangeExtension(sahPath, ".saf");

        if (!File.Exists(safPath))
        {
            using var safDialog = new OpenFileDialog
            {
                Title = "Select data.saf file",
                Filter = "SAF Data File (*.saf)|*.saf|All Files (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(sahPath),
                FileName = "data.saf"
            };

            if (safDialog.ShowDialog() != DialogResult.OK)
                return;

            safPath = safDialog.FileName;
        }

        LoadData(sahPath, safPath);
    }

    private void btnExtractAll_Click(object sender, EventArgs e)
    {
        if (_data == null)
        {
            MessageBox.Show("No data loaded. Please open data.sah/saf first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select extraction output folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            Cursor = Cursors.WaitCursor;
            _data.ExtractAll(dialog.SelectedPath);
            Cursor = Cursors.Default;
            MessageBox.Show($"All files extracted to:\n{dialog.SelectedPath}", "Extraction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;
            MessageBox.Show($"Extraction failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnExtractSelected_Click(object sender, EventArgs e)
    {
        if (_data == null)
        {
            MessageBox.Show("No data loaded. Please open data.sah/saf first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (treeView.SelectedNode == null && listView.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select a folder in the tree or files in the list to extract.", "Nothing Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select extraction output folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            Cursor = Cursors.WaitCursor;
            int count = 0;

            // Extract selected files from the list view
            if (listView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    if (item.Tag is SFile sFile)
                    {
                        _data.Extract(sFile, dialog.SelectedPath);
                        count++;
                    }
                }
            }
            else if (treeView.SelectedNode?.Tag is SDirectory dir)
            {
                // Extract the selected directory
                _data.Extract(dir, dialog.SelectedPath);
                count = -1; // unknown number of files
            }

            Cursor = Cursors.Default;

            string msg = count >= 0
                ? $"{count} file(s) extracted to:\n{dialog.SelectedPath}"
                : $"Directory extracted to:\n{dialog.SelectedPath}";
            MessageBox.Show(msg, "Extraction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;
            MessageBox.Show($"Extraction failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ──────────────────────────────────────────────
    // Tree/List interaction
    // ──────────────────────────────────────────────

    private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        listView.Items.Clear();

        if (e.Node?.Tag is not SDirectory dir)
            return;

        foreach (var file in dir.Files)
        {
            var item = new ListViewItem(file.Name) { Tag = file };
            item.SubItems.Add(FormatBytes(file.Length));
            item.SubItems.Add(file.Offset.ToString());
            listView.Items.Add(item);
        }

        statusLabel.Text = $"{dir.Files.Count} file(s) in '{(string.IsNullOrEmpty(dir.Name) ? "(root)" : dir.Name)}'";
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private void LoadData(string sahPath, string safPath)
    {
        try
        {
            Cursor = Cursors.WaitCursor;

            _data?.Dispose();
            _data = new Data(sahPath, safPath);

            treeView.Nodes.Clear();
            listView.Items.Clear();

            var rootNode = BuildTreeNode(_data.RootDirectory);
            treeView.Nodes.Add(rootNode);
            rootNode.Expand();

            statusLabel.Text = $"Loaded {_data.FileCount} file(s) from {Path.GetFileName(sahPath)}";
            Text = $"Parsec Data Viewer — {Path.GetFileName(sahPath)}";

            btnExtractAll.Enabled = true;
            btnExtractSelected.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load data:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private static TreeNode BuildTreeNode(SDirectory directory)
    {
        string label = string.IsNullOrEmpty(directory.Name) ? "(root)" : directory.Name;
        var node = new TreeNode(label) { Tag = directory };

        foreach (var sub in directory.Directories)
            node.Nodes.Add(BuildTreeNode(sub));

        return node;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }
}
