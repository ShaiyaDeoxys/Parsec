namespace Sample.DataViewer;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private ToolStrip toolStrip;
    private ToolStripButton btnOpen;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton btnExtractAll;
    private ToolStripButton btnExtractSelected;
    private SplitContainer splitContainer;
    private TreeView treeView;
    private ListView listView;
    private ColumnHeader colName;
    private ColumnHeader colSize;
    private ColumnHeader colOffset;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        _data?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ── ToolStrip ──────────────────────────────────
        toolStrip = new ToolStrip();
        btnOpen = new ToolStripButton("Open data.sah…") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        toolStripSeparator1 = new ToolStripSeparator();
        btnExtractAll = new ToolStripButton("Extract All") { DisplayStyle = ToolStripItemDisplayStyle.Text, Enabled = false };
        btnExtractSelected = new ToolStripButton("Extract Selected") { DisplayStyle = ToolStripItemDisplayStyle.Text, Enabled = false };

        toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, toolStripSeparator1, btnExtractAll, btnExtractSelected });

        // ── TreeView ───────────────────────────────────
        treeView = new TreeView
        {
            Dock = DockStyle.Fill,
            HideSelection = false
        };

        // ── ListView ───────────────────────────────────
        listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            HideSelection = false,
            MultiSelect = true
        };
        colName = new ColumnHeader { Text = "Name", Width = 280 };
        colSize = new ColumnHeader { Text = "Size", Width = 90 };
        colOffset = new ColumnHeader { Text = "Offset", Width = 110 };
        listView.Columns.AddRange(new[] { colName, colSize, colOffset });

        // ── SplitContainer ─────────────────────────────
        splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 260
        };
        splitContainer.Panel1.Controls.Add(treeView);
        splitContainer.Panel2.Controls.Add(listView);

        // ── StatusStrip ────────────────────────────────
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        statusStrip.Items.Add(statusLabel);

        // ── Form ───────────────────────────────────────
        Text = "Parsec Data Viewer";
        ClientSize = new System.Drawing.Size(900, 600);
        MinimumSize = new System.Drawing.Size(640, 480);

        Controls.Add(splitContainer);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);

        // ── Events ─────────────────────────────────────
        btnOpen.Click += btnOpen_Click;
        btnExtractAll.Click += btnExtractAll_Click;
        btnExtractSelected.Click += btnExtractSelected_Click;
        treeView.AfterSelect += treeView_AfterSelect;
    }
}
