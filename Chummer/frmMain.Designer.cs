namespace Chummer
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DoubleBuffered = true;
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewCritter = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFilePrintMultiple = new System.Windows.Forms.ToolStripMenuItem();
            this.printSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClearUnpinnedItems = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMURSep = new System.Windows.Forms.ToolStripSeparator();
            this.mnuStickyMRU0 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU5 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU6 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU7 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU8 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStickyMRU9 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU0 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU5 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU6 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU7 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU8 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMRU9 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileMRUSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsDiceRoller = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRestart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsOmae = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuHelpRevisionHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpDumpshock = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPrint = new System.Windows.Forms.ToolStripButton();
            this.printPreviewToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolTip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.tabForms = new System.Windows.Forms.TabControl();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.toolsMenu,
            this.windowsMenu,
            this.helpMenu});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.MdiWindowListItem = this.windowsMenu;
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1175, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "MenuStrip";
            this.menuStrip.ItemAdded += new System.Windows.Forms.ToolStripItemEventHandler(this.menuStrip_ItemAdded);
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.mnuNewCritter,
            this.openToolStripMenuItem,
            this.toolStripSeparator3,
            this.toolStripSeparator4,
            this.printToolStripMenuItem,
            this.mnuFilePrintMultiple,
            this.printSetupToolStripMenuItem,
            this.mnuClearUnpinnedItems,
            this.mnuMURSep,
            this.mnuStickyMRU0,
            this.mnuStickyMRU1,
            this.mnuStickyMRU2,
            this.mnuStickyMRU3,
            this.mnuStickyMRU4,
            this.mnuStickyMRU5,
            this.mnuStickyMRU6,
            this.mnuStickyMRU7,
            this.mnuStickyMRU8,
            this.mnuStickyMRU9,
            this.mnuMRU0,
            this.mnuMRU1,
            this.mnuMRU2,
            this.mnuMRU3,
            this.mnuMRU4,
            this.mnuMRU5,
            this.mnuMRU6,
            this.mnuMRU7,
            this.mnuMRU8,
            this.mnuMRU9,
            this.mnuFileMRUSeparator,
            this.exitToolStripMenuItem});
            this.fileMenu.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Tag = "Menu_Main_File";
            this.fileMenu.Text = "&File";
            this.fileMenu.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.newToolStripMenuItem.Tag = "Menu_Main_NewCharater";
            this.newToolStripMenuItem.Text = "&New Character";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // mnuNewCritter
            // 
            this.mnuNewCritter.Image = ((System.Drawing.Image)(resources.GetObject("mnuNewCritter.Image")));
            this.mnuNewCritter.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuNewCritter.Name = "mnuNewCritter";
            this.mnuNewCritter.Size = new System.Drawing.Size(195, 22);
            this.mnuNewCritter.Tag = "Menu_Main_NewCritter";
            this.mnuNewCritter.Text = "New &Critter";
            this.mnuNewCritter.Click += new System.EventHandler(this.mnuNewCritter_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.openToolStripMenuItem.Tag = "Menu_Main_Open";
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenFile);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            this.toolStripSeparator4.Visible = false;
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("printToolStripMenuItem.Image")));
            this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printToolStripMenuItem.Text = "&Print";
            this.printToolStripMenuItem.Visible = false;
            // 
            // mnuFilePrintMultiple
            // 
            this.mnuFilePrintMultiple.Image = global::Chummer.Properties.Resources.printer;
            this.mnuFilePrintMultiple.ImageTransparentColor = System.Drawing.Color.Black;
            this.mnuFilePrintMultiple.Name = "mnuFilePrintMultiple";
            this.mnuFilePrintMultiple.Size = new System.Drawing.Size(195, 22);
            this.mnuFilePrintMultiple.Tag = "Menu_Main_PrintMultiple";
            this.mnuFilePrintMultiple.Text = "Print &Multiple";
            this.mnuFilePrintMultiple.Click += new System.EventHandler(this.mnuFilePrintMultiple_Click);
            // 
            // printSetupToolStripMenuItem
            // 
            this.printSetupToolStripMenuItem.Name = "printSetupToolStripMenuItem";
            this.printSetupToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printSetupToolStripMenuItem.Text = "Print Setup";
            this.printSetupToolStripMenuItem.Visible = false;
            // 
            // mnuClearUnpinnedItems
            // 
            this.mnuClearUnpinnedItems.Image = global::Chummer.Properties.Resources.delete;
            this.mnuClearUnpinnedItems.Name = "mnuClearUnpinnedItems";
            this.mnuClearUnpinnedItems.Size = new System.Drawing.Size(195, 22);
            this.mnuClearUnpinnedItems.Tag = "Menu_Main_ClearUnpinnedItems";
            this.mnuClearUnpinnedItems.Text = "Clear Unpinned Items";
            this.mnuClearUnpinnedItems.Click += new System.EventHandler(this.mnuClearUnpinnedItems_Click);
            // 
            // mnuMURSep
            // 
            this.mnuMURSep.Name = "mnuMURSep";
            this.mnuMURSep.Size = new System.Drawing.Size(192, 6);
            // 
            // mnuStickyMRU0
            // 
            this.mnuStickyMRU0.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU0.Image")));
            this.mnuStickyMRU0.Name = "mnuStickyMRU0";
            this.mnuStickyMRU0.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU0.Text = "[StickyMRU0]";
            this.mnuStickyMRU0.Visible = false;
            this.mnuStickyMRU0.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU1
            // 
            this.mnuStickyMRU1.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU1.Image")));
            this.mnuStickyMRU1.Name = "mnuStickyMRU1";
            this.mnuStickyMRU1.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU1.Text = "[StickyMRU1]";
            this.mnuStickyMRU1.Visible = false;
            this.mnuStickyMRU1.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU2
            // 
            this.mnuStickyMRU2.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU2.Image")));
            this.mnuStickyMRU2.Name = "mnuStickyMRU2";
            this.mnuStickyMRU2.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU2.Text = "[StickyMRU2]";
            this.mnuStickyMRU2.Visible = false;
            this.mnuStickyMRU2.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU3
            // 
            this.mnuStickyMRU3.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU3.Image")));
            this.mnuStickyMRU3.Name = "mnuStickyMRU3";
            this.mnuStickyMRU3.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU3.Text = "[StickyMRU3]";
            this.mnuStickyMRU3.Visible = false;
            this.mnuStickyMRU3.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU4
            // 
            this.mnuStickyMRU4.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU4.Image")));
            this.mnuStickyMRU4.Name = "mnuStickyMRU4";
            this.mnuStickyMRU4.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU4.Text = "[StickyMRU4]";
            this.mnuStickyMRU4.Visible = false;
            this.mnuStickyMRU4.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU5
            // 
            this.mnuStickyMRU5.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU5.Image")));
            this.mnuStickyMRU5.Name = "mnuStickyMRU5";
            this.mnuStickyMRU5.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU5.Text = "[StickyMRU5]";
            this.mnuStickyMRU5.Visible = false;
            this.mnuStickyMRU5.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU6
            // 
            this.mnuStickyMRU6.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU6.Image")));
            this.mnuStickyMRU6.Name = "mnuStickyMRU6";
            this.mnuStickyMRU6.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU6.Text = "[StickyMRU6]";
            this.mnuStickyMRU6.Visible = false;
            this.mnuStickyMRU6.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU7
            // 
            this.mnuStickyMRU7.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU7.Image")));
            this.mnuStickyMRU7.Name = "mnuStickyMRU7";
            this.mnuStickyMRU7.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU7.Text = "[StickyMRU7]";
            this.mnuStickyMRU7.Visible = false;
            this.mnuStickyMRU7.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU8
            // 
            this.mnuStickyMRU8.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU8.Image")));
            this.mnuStickyMRU8.Name = "mnuStickyMRU8";
            this.mnuStickyMRU8.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU8.Text = "[StickyMRU8]";
            this.mnuStickyMRU8.Visible = false;
            this.mnuStickyMRU8.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU9
            // 
            this.mnuStickyMRU9.Image = ((System.Drawing.Image)(resources.GetObject("mnuStickyMRU9.Image")));
            this.mnuStickyMRU9.Name = "mnuStickyMRU9";
            this.mnuStickyMRU9.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU9.Text = "[StickyMRU9]";
            this.mnuStickyMRU9.Visible = false;
            this.mnuStickyMRU9.Click += new System.EventHandler(this.mnuStickyMRU_Click);
            this.mnuStickyMRU9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuMRU0
            // 
            this.mnuMRU0.Name = "mnuMRU0";
            this.mnuMRU0.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU0.Text = "[MRU0]";
            this.mnuMRU0.Visible = false;
            this.mnuMRU0.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU1
            // 
            this.mnuMRU1.Name = "mnuMRU1";
            this.mnuMRU1.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU1.Text = "[MRU1]";
            this.mnuMRU1.Visible = false;
            this.mnuMRU1.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU2
            // 
            this.mnuMRU2.Name = "mnuMRU2";
            this.mnuMRU2.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU2.Text = "[MRU2]";
            this.mnuMRU2.Visible = false;
            this.mnuMRU2.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU3
            // 
            this.mnuMRU3.Name = "mnuMRU3";
            this.mnuMRU3.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU3.Text = "[MRU3]";
            this.mnuMRU3.Visible = false;
            this.mnuMRU3.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU4
            // 
            this.mnuMRU4.Name = "mnuMRU4";
            this.mnuMRU4.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU4.Text = "[MRU4]";
            this.mnuMRU4.Visible = false;
            this.mnuMRU4.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU5
            // 
            this.mnuMRU5.Name = "mnuMRU5";
            this.mnuMRU5.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU5.Text = "[MRU5]";
            this.mnuMRU5.Visible = false;
            this.mnuMRU5.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU6
            // 
            this.mnuMRU6.Name = "mnuMRU6";
            this.mnuMRU6.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU6.Text = "[MRU6]";
            this.mnuMRU6.Visible = false;
            this.mnuMRU6.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU7
            // 
            this.mnuMRU7.Name = "mnuMRU7";
            this.mnuMRU7.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU7.Text = "[MRU7]";
            this.mnuMRU7.Visible = false;
            this.mnuMRU7.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU8
            // 
            this.mnuMRU8.Name = "mnuMRU8";
            this.mnuMRU8.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU8.Text = "[MRU8]";
            this.mnuMRU8.Visible = false;
            this.mnuMRU8.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU9
            // 
            this.mnuMRU9.Name = "mnuMRU9";
            this.mnuMRU9.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU9.Text = "[MRU9]";
            this.mnuMRU9.Visible = false;
            this.mnuMRU9.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuFileMRUSeparator
            // 
            this.mnuFileMRUSeparator.Name = "mnuFileMRUSeparator";
            this.mnuFileMRUSeparator.Size = new System.Drawing.Size(192, 6);
            this.mnuFileMRUSeparator.Visible = false;
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.exitToolStripMenuItem.Tag = "Menu_Main_Exit";
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolsStripMenuItem_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuToolsDiceRoller,
            this.toolStripSeparator5,
            this.optionsToolStripMenuItem,
            this.mnuToolsUpdate,
            this.mnuRestart,
            this.mnuToolsOmae});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(47, 20);
            this.toolsMenu.Tag = "Menu_Main_Tools";
            this.toolsMenu.Text = "&Tools";
            this.toolsMenu.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
            // 
            // mnuToolsDiceRoller
            // 
            this.mnuToolsDiceRoller.Image = global::Chummer.Properties.Resources.die;
            this.mnuToolsDiceRoller.Name = "mnuToolsDiceRoller";
            this.mnuToolsDiceRoller.Size = new System.Drawing.Size(171, 22);
            this.mnuToolsDiceRoller.Tag = "Menu_Main_DiceRoller";
            this.mnuToolsDiceRoller.Text = "&Dice Roller";
            this.mnuToolsDiceRoller.Click += new System.EventHandler(this.mnuToolsDiceRoller_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(168, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Image = global::Chummer.Properties.Resources.cog_edit;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.optionsToolStripMenuItem.Tag = "Menu_Main_Options";
            this.optionsToolStripMenuItem.Text = "&Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // mnuToolsUpdate
            // 
            this.mnuToolsUpdate.Image = global::Chummer.Properties.Resources.server_lightning;
            this.mnuToolsUpdate.Name = "mnuToolsUpdate";
            this.mnuToolsUpdate.Size = new System.Drawing.Size(171, 22);
            this.mnuToolsUpdate.Tag = "Menu_Main_Update";
            this.mnuToolsUpdate.Text = "Check for Updates";
            this.mnuToolsUpdate.Click += new System.EventHandler(this.mnuToolsUpdate_Click);
            // 
            // mnuRestart
            // 
            this.mnuRestart.Image = global::Chummer.Properties.Resources.arrow_redo;
            this.mnuRestart.Name = "mnuRestart";
            this.mnuRestart.Size = new System.Drawing.Size(171, 22);
            this.mnuRestart.Tag = "Button_Update_RestartChummer";
            this.mnuRestart.Text = "Restart Chummer";
            this.mnuRestart.Click += new System.EventHandler(this.mnuRestart_Click);
            // 
            // mnuToolsOmae
            // 
            this.mnuToolsOmae.Name = "mnuToolsOmae";
            this.mnuToolsOmae.Size = new System.Drawing.Size(171, 22);
            this.mnuToolsOmae.Tag = "Menu_Main_Omae";
            this.mnuToolsOmae.Text = "Omae";
            this.mnuToolsOmae.Click += new System.EventHandler(this.mnuToolsOmae_Click);
            // 
            // windowsMenu
            // 
            this.windowsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newWindowToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.windowsMenu.Name = "windowsMenu";
            this.windowsMenu.Size = new System.Drawing.Size(68, 20);
            this.windowsMenu.Tag = "Menu_Main_Window";
            this.windowsMenu.Text = "&Windows";
            this.windowsMenu.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.newWindowToolStripMenuItem.Tag = "Menu_Main_NewWindow";
            this.newWindowToolStripMenuItem.Text = "&New Window";
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.closeAllToolStripMenuItem.Tag = "Menu_Main_CloseAll";
            this.closeAllToolStripMenuItem.Text = "C&lose All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.CloseAllToolStripMenuItem_Click);
            // 
            // helpMenu
            // 
            this.helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.toolStripSeparator8,
            this.mnuHelpRevisionHistory,
            this.mnuHelpDumpshock,
            this.aboutToolStripMenuItem});
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(44, 20);
            this.helpMenu.Tag = "Menu_Main_Help";
            this.helpMenu.Text = "&Help";
            this.helpMenu.DropDownOpening += new System.EventHandler(this.Menu_DropDownOpening);
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Image = global::Chummer.Properties.Resources.world_go;
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.contentsToolStripMenuItem.Tag = "Menu_Main_ChummerWiki";
            this.contentsToolStripMenuItem.Text = "Chummer Wiki";
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.contentsToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(197, 6);
            // 
            // mnuHelpRevisionHistory
            // 
            this.mnuHelpRevisionHistory.Name = "mnuHelpRevisionHistory";
            this.mnuHelpRevisionHistory.Size = new System.Drawing.Size(200, 22);
            this.mnuHelpRevisionHistory.Tag = "Menu_Main_RevisionHistory";
            this.mnuHelpRevisionHistory.Text = "&Revision History";
            this.mnuHelpRevisionHistory.Click += new System.EventHandler(this.mnuHelpRevisionHistory_Click);
            // 
            // mnuHelpDumpshock
            // 
            this.mnuHelpDumpshock.Name = "mnuHelpDumpshock";
            this.mnuHelpDumpshock.Size = new System.Drawing.Size(200, 22);
            this.mnuHelpDumpshock.Tag = "Menu_Main_IssueTracker";
            this.mnuHelpDumpshock.Text = "&Dumpshock Thread";
            this.mnuHelpDumpshock.Click += new System.EventHandler(this.mnuHelpDumpshock_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.aboutToolStripMenuItem.Tag = "Menu_Main_About";
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripButton,
            this.openToolStripButton,
            this.tsbSave,
            this.toolStripSeparator1,
            this.tsbPrint,
            this.printPreviewToolStripButton,
            this.toolStripSeparator2,
            this.helpToolStripButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1175, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "ToolStrip";
            this.toolStrip.ItemAdded += new System.Windows.Forms.ToolStripItemEventHandler(this.toolStrip_ItemAdded);
            this.toolStrip.ItemRemoved += new System.Windows.Forms.ToolStripItemEventHandler(this.toolStrip_ItemRemoved);
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newToolStripButton.Image = global::Chummer.Properties.Resources.page;
            this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newToolStripButton.Tag = "Menu_Main_NewCharater";
            this.newToolStripButton.Text = "New";
            this.newToolStripButton.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = global::Chummer.Properties.Resources.folder_page;
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Tag = "Menu_Main_Open";
            this.openToolStripButton.Text = "Open";
            this.openToolStripButton.Click += new System.EventHandler(this.OpenFile);
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSave.Image = global::Chummer.Properties.Resources.disk;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(23, 22);
            this.tsbSave.Text = "Save";
            this.tsbSave.Visible = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator1.Visible = false;
            // 
            // tsbPrint
            // 
            this.tsbPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPrint.Image = global::Chummer.Properties.Resources.printer;
            this.tsbPrint.ImageTransparentColor = System.Drawing.Color.Black;
            this.tsbPrint.Name = "tsbPrint";
            this.tsbPrint.Size = new System.Drawing.Size(23, 22);
            this.tsbPrint.Text = "Print";
            this.tsbPrint.Visible = false;
            // 
            // printPreviewToolStripButton
            // 
            this.printPreviewToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printPreviewToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("printPreviewToolStripButton.Image")));
            this.printPreviewToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.printPreviewToolStripButton.Name = "printPreviewToolStripButton";
            this.printPreviewToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.printPreviewToolStripButton.Text = "Print Preview";
            this.printPreviewToolStripButton.Visible = false;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator2.Visible = false;
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("helpToolStripButton.Image")));
            this.helpToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.helpToolStripButton.Text = "Help";
            this.helpToolStripButton.Visible = false;
            // 
            // tabForms
            // 
            this.tabForms.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabForms.Location = new System.Drawing.Point(0, 49);
            this.tabForms.Name = "tabForms";
            this.tabForms.SelectedIndex = 0;
            this.tabForms.Size = new System.Drawing.Size(1175, 22);
            this.tabForms.TabIndex = 3;
            this.tabForms.Visible = false;
            this.tabForms.SelectedIndexChanged += new System.EventHandler(this.tabForms_SelectedIndexChanged);
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 713);
            this.Controls.Add(this.tabForms);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chummer5";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_Closing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.MdiChildActivate += new System.EventHandler(this.frmMain_MdiChildActivate);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMain_DragEnter);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator mnuMURSep;
        private System.Windows.Forms.ToolStripMenuItem printSetupToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrintMultiple;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsMenu;
        private System.Windows.Forms.ToolStripMenuItem newWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton newToolStripButton;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton tsbPrint;
        private System.Windows.Forms.ToolStripButton printPreviewToolStripButton;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsUpdate;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpRevisionHistory;
        private System.Windows.Forms.ToolStripMenuItem mnuNewCritter;
        private System.Windows.Forms.ToolStripSeparator mnuFileMRUSeparator;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU0;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU1;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU3;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU2;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU4;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU5;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU6;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU9;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU7;
        private System.Windows.Forms.ToolStripMenuItem mnuMRU8;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU0;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU1;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU2;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU3;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU4;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU5;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU6;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU7;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU8;
        private System.Windows.Forms.ToolStripMenuItem mnuStickyMRU9;
        private System.Windows.Forms.TabControl tabForms;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsDiceRoller;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsOmae;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpDumpshock;
        private System.Windows.Forms.ToolStripMenuItem mnuClearUnpinnedItems;
        private System.Windows.Forms.ToolStripMenuItem mnuRestart;
    }
}



