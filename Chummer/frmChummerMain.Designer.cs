using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class frmChummerMain
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
            if (disposing)
            {
                components?.Dispose();
                _frmRoller?.Dispose();
                _frmUpdate?.Dispose();
                _mascotChummy?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChummerMain));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuNewCritter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.openToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.printToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuFilePrintMultiple = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.printSetupToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuClearUnpinnedItems = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMURSep = new System.Windows.Forms.ToolStripSeparator();
            this.mnuStickyMRU0 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU1 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU2 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU3 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU4 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU5 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU6 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU7 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU8 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuStickyMRU9 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU0 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU1 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU2 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU3 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU4 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU5 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU6 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU7 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU8 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuMRU9 = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuFileMRUSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsDiceRoller = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOptions = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuCharacterOptions = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuToolsUpdate = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuRestart = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuToolsTranslator = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuHeroLabImporter = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.windowsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newWindowToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.closeAllToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChummerWiki = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuChummerDiscord = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuHelpRevisionHistory = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.mnuHelpDumpshock = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.aboutToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbNewCharacter = new Chummer.DpiFriendlyToolStripButton(this.components);
            this.tsbOpen = new Chummer.DpiFriendlyToolStripButton(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tabForms = new System.Windows.Forms.TabControl();
            this.mnuProcessFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSave = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsSaveAs = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsClose = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.tsPrint = new Chummer.DpiFriendlyToolStripMenuItem(this.components);
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.mnuProcessFile.SuspendLayout();
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
            this.menuStrip.Size = new System.Drawing.Size(1264, 24);
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
            this.fileMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Tag = "Menu_Main_File";
            this.fileMenu.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = global::Chummer.Properties.Resources.user_add;
            this.newToolStripMenuItem.ImageDpi120 = null;
            this.newToolStripMenuItem.ImageDpi144 = null;
            this.newToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.user_add1;
            this.newToolStripMenuItem.ImageDpi288 = null;
            this.newToolStripMenuItem.ImageDpi384 = null;
            this.newToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.user_add;
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.newToolStripMenuItem.Tag = "Menu_Main_NewCharacter";
            this.newToolStripMenuItem.Text = "&New Character";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // mnuNewCritter
            // 
            this.mnuNewCritter.Image = global::Chummer.Properties.Resources.ladybird_add;
            this.mnuNewCritter.ImageDpi120 = null;
            this.mnuNewCritter.ImageDpi144 = null;
            this.mnuNewCritter.ImageDpi192 = global::Chummer.Properties.Resources.ladybird_add1;
            this.mnuNewCritter.ImageDpi288 = null;
            this.mnuNewCritter.ImageDpi384 = null;
            this.mnuNewCritter.ImageDpi96 = global::Chummer.Properties.Resources.ladybird_add;
            this.mnuNewCritter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuNewCritter.Name = "mnuNewCritter";
            this.mnuNewCritter.Size = new System.Drawing.Size(195, 22);
            this.mnuNewCritter.Tag = "Menu_Main_NewCritter";
            this.mnuNewCritter.Text = "New &Critter";
            this.mnuNewCritter.Click += new System.EventHandler(this.mnuNewCritter_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::Chummer.Properties.Resources.folder_page;
            this.openToolStripMenuItem.ImageDpi120 = null;
            this.openToolStripMenuItem.ImageDpi144 = null;
            this.openToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.folder_page1;
            this.openToolStripMenuItem.ImageDpi288 = null;
            this.openToolStripMenuItem.ImageDpi384 = null;
            this.openToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.folder_page;
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
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
            this.printToolStripMenuItem.Image = global::Chummer.Properties.Resources.printer;
            this.printToolStripMenuItem.ImageDpi120 = null;
            this.printToolStripMenuItem.ImageDpi144 = null;
            this.printToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.printer1;
            this.printToolStripMenuItem.ImageDpi288 = null;
            this.printToolStripMenuItem.ImageDpi384 = null;
            this.printToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.printer;
            this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printToolStripMenuItem.Text = "&Print";
            this.printToolStripMenuItem.Visible = false;
            // 
            // mnuFilePrintMultiple
            // 
            this.mnuFilePrintMultiple.Image = global::Chummer.Properties.Resources.printer_add;
            this.mnuFilePrintMultiple.ImageDpi120 = null;
            this.mnuFilePrintMultiple.ImageDpi144 = null;
            this.mnuFilePrintMultiple.ImageDpi192 = global::Chummer.Properties.Resources.printer_add1;
            this.mnuFilePrintMultiple.ImageDpi288 = null;
            this.mnuFilePrintMultiple.ImageDpi384 = null;
            this.mnuFilePrintMultiple.ImageDpi96 = global::Chummer.Properties.Resources.printer_add;
            this.mnuFilePrintMultiple.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuFilePrintMultiple.Name = "mnuFilePrintMultiple";
            this.mnuFilePrintMultiple.Size = new System.Drawing.Size(195, 22);
            this.mnuFilePrintMultiple.Tag = "Menu_Main_PrintMultiple";
            this.mnuFilePrintMultiple.Text = "Print &Multiple";
            this.mnuFilePrintMultiple.Click += new System.EventHandler(this.mnuFilePrintMultiple_Click);
            // 
            // printSetupToolStripMenuItem
            // 
            this.printSetupToolStripMenuItem.Image = global::Chummer.Properties.Resources.printer_empty;
            this.printSetupToolStripMenuItem.ImageDpi120 = null;
            this.printSetupToolStripMenuItem.ImageDpi144 = null;
            this.printSetupToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.printer_empty1;
            this.printSetupToolStripMenuItem.ImageDpi288 = null;
            this.printSetupToolStripMenuItem.ImageDpi384 = null;
            this.printSetupToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.printer_empty;
            this.printSetupToolStripMenuItem.Name = "printSetupToolStripMenuItem";
            this.printSetupToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printSetupToolStripMenuItem.Text = "Print Setup";
            this.printSetupToolStripMenuItem.Visible = false;
            // 
            // mnuClearUnpinnedItems
            // 
            this.mnuClearUnpinnedItems.Image = global::Chummer.Properties.Resources.delete;
            this.mnuClearUnpinnedItems.ImageDpi120 = null;
            this.mnuClearUnpinnedItems.ImageDpi144 = null;
            this.mnuClearUnpinnedItems.ImageDpi192 = global::Chummer.Properties.Resources.delete1;
            this.mnuClearUnpinnedItems.ImageDpi288 = null;
            this.mnuClearUnpinnedItems.ImageDpi384 = null;
            this.mnuClearUnpinnedItems.ImageDpi96 = global::Chummer.Properties.Resources.delete;
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
            this.mnuStickyMRU0.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU0.ImageDpi120 = null;
            this.mnuStickyMRU0.ImageDpi144 = null;
            this.mnuStickyMRU0.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU0.ImageDpi288 = null;
            this.mnuStickyMRU0.ImageDpi384 = null;
            this.mnuStickyMRU0.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU0.Name = "mnuStickyMRU0";
            this.mnuStickyMRU0.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU0.Text = "[StickyMRU0]";
            this.mnuStickyMRU0.Visible = false;
            this.mnuStickyMRU0.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU1
            // 
            this.mnuStickyMRU1.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU1.ImageDpi120 = null;
            this.mnuStickyMRU1.ImageDpi144 = null;
            this.mnuStickyMRU1.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU1.ImageDpi288 = null;
            this.mnuStickyMRU1.ImageDpi384 = null;
            this.mnuStickyMRU1.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU1.Name = "mnuStickyMRU1";
            this.mnuStickyMRU1.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU1.Text = "[StickyMRU1]";
            this.mnuStickyMRU1.Visible = false;
            this.mnuStickyMRU1.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU2
            // 
            this.mnuStickyMRU2.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU2.ImageDpi120 = null;
            this.mnuStickyMRU2.ImageDpi144 = null;
            this.mnuStickyMRU2.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU2.ImageDpi288 = null;
            this.mnuStickyMRU2.ImageDpi384 = null;
            this.mnuStickyMRU2.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU2.Name = "mnuStickyMRU2";
            this.mnuStickyMRU2.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU2.Text = "[StickyMRU2]";
            this.mnuStickyMRU2.Visible = false;
            this.mnuStickyMRU2.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU3
            // 
            this.mnuStickyMRU3.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU3.ImageDpi120 = null;
            this.mnuStickyMRU3.ImageDpi144 = null;
            this.mnuStickyMRU3.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU3.ImageDpi288 = null;
            this.mnuStickyMRU3.ImageDpi384 = null;
            this.mnuStickyMRU3.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU3.Name = "mnuStickyMRU3";
            this.mnuStickyMRU3.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU3.Text = "[StickyMRU3]";
            this.mnuStickyMRU3.Visible = false;
            this.mnuStickyMRU3.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU4
            // 
            this.mnuStickyMRU4.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU4.ImageDpi120 = null;
            this.mnuStickyMRU4.ImageDpi144 = null;
            this.mnuStickyMRU4.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU4.ImageDpi288 = null;
            this.mnuStickyMRU4.ImageDpi384 = null;
            this.mnuStickyMRU4.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU4.Name = "mnuStickyMRU4";
            this.mnuStickyMRU4.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU4.Text = "[StickyMRU4]";
            this.mnuStickyMRU4.Visible = false;
            this.mnuStickyMRU4.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU5
            // 
            this.mnuStickyMRU5.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU5.ImageDpi120 = null;
            this.mnuStickyMRU5.ImageDpi144 = null;
            this.mnuStickyMRU5.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU5.ImageDpi288 = null;
            this.mnuStickyMRU5.ImageDpi384 = null;
            this.mnuStickyMRU5.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU5.Name = "mnuStickyMRU5";
            this.mnuStickyMRU5.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU5.Text = "[StickyMRU5]";
            this.mnuStickyMRU5.Visible = false;
            this.mnuStickyMRU5.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU6
            // 
            this.mnuStickyMRU6.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU6.ImageDpi120 = null;
            this.mnuStickyMRU6.ImageDpi144 = null;
            this.mnuStickyMRU6.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU6.ImageDpi288 = null;
            this.mnuStickyMRU6.ImageDpi384 = null;
            this.mnuStickyMRU6.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU6.Name = "mnuStickyMRU6";
            this.mnuStickyMRU6.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU6.Text = "[StickyMRU6]";
            this.mnuStickyMRU6.Visible = false;
            this.mnuStickyMRU6.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU7
            // 
            this.mnuStickyMRU7.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU7.ImageDpi120 = null;
            this.mnuStickyMRU7.ImageDpi144 = null;
            this.mnuStickyMRU7.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU7.ImageDpi288 = null;
            this.mnuStickyMRU7.ImageDpi384 = null;
            this.mnuStickyMRU7.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU7.Name = "mnuStickyMRU7";
            this.mnuStickyMRU7.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU7.Text = "[StickyMRU7]";
            this.mnuStickyMRU7.Visible = false;
            this.mnuStickyMRU7.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU8
            // 
            this.mnuStickyMRU8.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU8.ImageDpi120 = null;
            this.mnuStickyMRU8.ImageDpi144 = null;
            this.mnuStickyMRU8.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU8.ImageDpi288 = null;
            this.mnuStickyMRU8.ImageDpi384 = null;
            this.mnuStickyMRU8.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU8.Name = "mnuStickyMRU8";
            this.mnuStickyMRU8.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU8.Text = "[StickyMRU8]";
            this.mnuStickyMRU8.Visible = false;
            this.mnuStickyMRU8.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU9
            // 
            this.mnuStickyMRU9.Image = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU9.ImageDpi120 = null;
            this.mnuStickyMRU9.ImageDpi144 = null;
            this.mnuStickyMRU9.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange1;
            this.mnuStickyMRU9.ImageDpi288 = null;
            this.mnuStickyMRU9.ImageDpi384 = null;
            this.mnuStickyMRU9.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange;
            this.mnuStickyMRU9.Name = "mnuStickyMRU9";
            this.mnuStickyMRU9.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU9.Text = "[StickyMRU9]";
            this.mnuStickyMRU9.Visible = false;
            this.mnuStickyMRU9.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuMRU0
            // 
            this.mnuMRU0.Image = null;
            this.mnuMRU0.ImageDpi120 = null;
            this.mnuMRU0.ImageDpi144 = null;
            this.mnuMRU0.ImageDpi192 = null;
            this.mnuMRU0.ImageDpi288 = null;
            this.mnuMRU0.ImageDpi384 = null;
            this.mnuMRU0.ImageDpi96 = null;
            this.mnuMRU0.Name = "mnuMRU0";
            this.mnuMRU0.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU0.Text = "[MRU0]";
            this.mnuMRU0.Visible = false;
            this.mnuMRU0.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU1
            // 
            this.mnuMRU1.Image = null;
            this.mnuMRU1.ImageDpi120 = null;
            this.mnuMRU1.ImageDpi144 = null;
            this.mnuMRU1.ImageDpi192 = null;
            this.mnuMRU1.ImageDpi288 = null;
            this.mnuMRU1.ImageDpi384 = null;
            this.mnuMRU1.ImageDpi96 = null;
            this.mnuMRU1.Name = "mnuMRU1";
            this.mnuMRU1.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU1.Text = "[MRU1]";
            this.mnuMRU1.Visible = false;
            this.mnuMRU1.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU2
            // 
            this.mnuMRU2.Image = null;
            this.mnuMRU2.ImageDpi120 = null;
            this.mnuMRU2.ImageDpi144 = null;
            this.mnuMRU2.ImageDpi192 = null;
            this.mnuMRU2.ImageDpi288 = null;
            this.mnuMRU2.ImageDpi384 = null;
            this.mnuMRU2.ImageDpi96 = null;
            this.mnuMRU2.Name = "mnuMRU2";
            this.mnuMRU2.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU2.Text = "[MRU2]";
            this.mnuMRU2.Visible = false;
            this.mnuMRU2.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU3
            // 
            this.mnuMRU3.Image = null;
            this.mnuMRU3.ImageDpi120 = null;
            this.mnuMRU3.ImageDpi144 = null;
            this.mnuMRU3.ImageDpi192 = null;
            this.mnuMRU3.ImageDpi288 = null;
            this.mnuMRU3.ImageDpi384 = null;
            this.mnuMRU3.ImageDpi96 = null;
            this.mnuMRU3.Name = "mnuMRU3";
            this.mnuMRU3.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU3.Text = "[MRU3]";
            this.mnuMRU3.Visible = false;
            this.mnuMRU3.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU4
            // 
            this.mnuMRU4.Image = null;
            this.mnuMRU4.ImageDpi120 = null;
            this.mnuMRU4.ImageDpi144 = null;
            this.mnuMRU4.ImageDpi192 = null;
            this.mnuMRU4.ImageDpi288 = null;
            this.mnuMRU4.ImageDpi384 = null;
            this.mnuMRU4.ImageDpi96 = null;
            this.mnuMRU4.Name = "mnuMRU4";
            this.mnuMRU4.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU4.Text = "[MRU4]";
            this.mnuMRU4.Visible = false;
            this.mnuMRU4.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU5
            // 
            this.mnuMRU5.Image = null;
            this.mnuMRU5.ImageDpi120 = null;
            this.mnuMRU5.ImageDpi144 = null;
            this.mnuMRU5.ImageDpi192 = null;
            this.mnuMRU5.ImageDpi288 = null;
            this.mnuMRU5.ImageDpi384 = null;
            this.mnuMRU5.ImageDpi96 = null;
            this.mnuMRU5.Name = "mnuMRU5";
            this.mnuMRU5.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU5.Text = "[MRU5]";
            this.mnuMRU5.Visible = false;
            this.mnuMRU5.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU6
            // 
            this.mnuMRU6.Image = null;
            this.mnuMRU6.ImageDpi120 = null;
            this.mnuMRU6.ImageDpi144 = null;
            this.mnuMRU6.ImageDpi192 = null;
            this.mnuMRU6.ImageDpi288 = null;
            this.mnuMRU6.ImageDpi384 = null;
            this.mnuMRU6.ImageDpi96 = null;
            this.mnuMRU6.Name = "mnuMRU6";
            this.mnuMRU6.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU6.Text = "[MRU6]";
            this.mnuMRU6.Visible = false;
            this.mnuMRU6.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU7
            // 
            this.mnuMRU7.Image = null;
            this.mnuMRU7.ImageDpi120 = null;
            this.mnuMRU7.ImageDpi144 = null;
            this.mnuMRU7.ImageDpi192 = null;
            this.mnuMRU7.ImageDpi288 = null;
            this.mnuMRU7.ImageDpi384 = null;
            this.mnuMRU7.ImageDpi96 = null;
            this.mnuMRU7.Name = "mnuMRU7";
            this.mnuMRU7.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU7.Text = "[MRU7]";
            this.mnuMRU7.Visible = false;
            this.mnuMRU7.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU8
            // 
            this.mnuMRU8.Image = null;
            this.mnuMRU8.ImageDpi120 = null;
            this.mnuMRU8.ImageDpi144 = null;
            this.mnuMRU8.ImageDpi192 = null;
            this.mnuMRU8.ImageDpi288 = null;
            this.mnuMRU8.ImageDpi384 = null;
            this.mnuMRU8.ImageDpi96 = null;
            this.mnuMRU8.Name = "mnuMRU8";
            this.mnuMRU8.Size = new System.Drawing.Size(195, 22);
            this.mnuMRU8.Text = "[MRU8]";
            this.mnuMRU8.Visible = false;
            this.mnuMRU8.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuMRU8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuMRU_MouseDown);
            // 
            // mnuMRU9
            // 
            this.mnuMRU9.Image = null;
            this.mnuMRU9.ImageDpi120 = null;
            this.mnuMRU9.ImageDpi144 = null;
            this.mnuMRU9.ImageDpi192 = null;
            this.mnuMRU9.ImageDpi288 = null;
            this.mnuMRU9.ImageDpi384 = null;
            this.mnuMRU9.ImageDpi96 = null;
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
            this.exitToolStripMenuItem.Image = global::Chummer.Properties.Resources.door_out;
            this.exitToolStripMenuItem.ImageDpi120 = null;
            this.exitToolStripMenuItem.ImageDpi144 = null;
            this.exitToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.door_out1;
            this.exitToolStripMenuItem.ImageDpi288 = null;
            this.exitToolStripMenuItem.ImageDpi384 = null;
            this.exitToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.door_out;
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
            this.mnuOptions,
            this.mnuCharacterOptions,
            this.mnuToolsUpdate,
            this.mnuRestart,
            this.toolStripSeparator6,
            this.mnuToolsTranslator,
            this.mnuHeroLabImporter});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(46, 20);
            this.toolsMenu.Tag = "Menu_Main_Tools";
            this.toolsMenu.Text = "&Tools";
            // 
            // mnuToolsDiceRoller
            // 
            this.mnuToolsDiceRoller.Image = global::Chummer.Properties.Resources.die;
            this.mnuToolsDiceRoller.ImageDpi120 = null;
            this.mnuToolsDiceRoller.ImageDpi144 = null;
            this.mnuToolsDiceRoller.ImageDpi192 = global::Chummer.Properties.Resources.die1;
            this.mnuToolsDiceRoller.ImageDpi288 = null;
            this.mnuToolsDiceRoller.ImageDpi384 = null;
            this.mnuToolsDiceRoller.ImageDpi96 = global::Chummer.Properties.Resources.die;
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
            // mnuOptions
            // 
            this.mnuOptions.Image = global::Chummer.Properties.Resources.cog_edit;
            this.mnuOptions.ImageDpi120 = null;
            this.mnuOptions.ImageDpi144 = null;
            this.mnuOptions.ImageDpi192 = global::Chummer.Properties.Resources.cog_edit1;
            this.mnuOptions.ImageDpi288 = null;
            this.mnuOptions.ImageDpi384 = null;
            this.mnuOptions.ImageDpi96 = global::Chummer.Properties.Resources.cog_edit;
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(171, 22);
            this.mnuOptions.Tag = "Menu_Main_Options";
            this.mnuOptions.Text = "&Global Options";
            this.mnuOptions.Click += new System.EventHandler(this.mnuOptions_Click);
            // 
            // mnuCharacterOptions
            // 
            this.mnuCharacterOptions.Image = global::Chummer.Properties.Resources.group_gear;
            this.mnuCharacterOptions.ImageDpi120 = null;
            this.mnuCharacterOptions.ImageDpi144 = null;
            this.mnuCharacterOptions.ImageDpi192 = global::Chummer.Properties.Resources.group_gear1;
            this.mnuCharacterOptions.ImageDpi288 = null;
            this.mnuCharacterOptions.ImageDpi384 = null;
            this.mnuCharacterOptions.ImageDpi96 = global::Chummer.Properties.Resources.group_gear;
            this.mnuCharacterOptions.Name = "mnuCharacterOptions";
            this.mnuCharacterOptions.Size = new System.Drawing.Size(171, 22);
            this.mnuCharacterOptions.Tag = "Menu_Main_Character_Options";
            this.mnuCharacterOptions.Text = "&Character Options";
            this.mnuCharacterOptions.Click += new System.EventHandler(this.mnuCharacterOptions_Click);
            // 
            // mnuToolsUpdate
            // 
            this.mnuToolsUpdate.Image = global::Chummer.Properties.Resources.database_refresh;
            this.mnuToolsUpdate.ImageDpi120 = null;
            this.mnuToolsUpdate.ImageDpi144 = null;
            this.mnuToolsUpdate.ImageDpi192 = global::Chummer.Properties.Resources.database_refresh1;
            this.mnuToolsUpdate.ImageDpi288 = null;
            this.mnuToolsUpdate.ImageDpi384 = null;
            this.mnuToolsUpdate.ImageDpi96 = global::Chummer.Properties.Resources.database_refresh;
            this.mnuToolsUpdate.Name = "mnuToolsUpdate";
            this.mnuToolsUpdate.Size = new System.Drawing.Size(171, 22);
            this.mnuToolsUpdate.Tag = "Menu_Main_Update";
            this.mnuToolsUpdate.Text = "Check for &Updates";
            this.mnuToolsUpdate.Click += new System.EventHandler(this.mnuToolsUpdate_Click);
            // 
            // mnuRestart
            // 
            this.mnuRestart.Image = global::Chummer.Properties.Resources.arrow_redo;
            this.mnuRestart.ImageDpi120 = null;
            this.mnuRestart.ImageDpi144 = null;
            this.mnuRestart.ImageDpi192 = global::Chummer.Properties.Resources.arrow_redo1;
            this.mnuRestart.ImageDpi288 = null;
            this.mnuRestart.ImageDpi384 = null;
            this.mnuRestart.ImageDpi96 = global::Chummer.Properties.Resources.arrow_redo;
            this.mnuRestart.Name = "mnuRestart";
            this.mnuRestart.Size = new System.Drawing.Size(171, 22);
            this.mnuRestart.Tag = "Menu_Main_Restart";
            this.mnuRestart.Text = "&Restart Chummer";
            this.mnuRestart.Click += new System.EventHandler(this.mnuRestart_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(168, 6);
            // 
            // mnuToolsTranslator
            // 
            this.mnuToolsTranslator.Image = global::Chummer.Properties.Resources.locate;
            this.mnuToolsTranslator.ImageDpi120 = null;
            this.mnuToolsTranslator.ImageDpi144 = null;
            this.mnuToolsTranslator.ImageDpi192 = global::Chummer.Properties.Resources.locate1;
            this.mnuToolsTranslator.ImageDpi288 = global::Chummer.Properties.Resources.Locate_48;
            this.mnuToolsTranslator.ImageDpi384 = null;
            this.mnuToolsTranslator.ImageDpi96 = global::Chummer.Properties.Resources.locate;
            this.mnuToolsTranslator.Name = "mnuToolsTranslator";
            this.mnuToolsTranslator.Size = new System.Drawing.Size(171, 22);
            this.mnuToolsTranslator.Tag = "Menu_Main_Translator";
            this.mnuToolsTranslator.Text = "&Translator";
            this.mnuToolsTranslator.Click += new System.EventHandler(this.mnuToolsTranslator_Click);
            // 
            // mnuHeroLabImporter
            // 
            this.mnuHeroLabImporter.Image = global::Chummer.Properties.Resources.HeroLab_16;
            this.mnuHeroLabImporter.ImageDpi120 = null;
            this.mnuHeroLabImporter.ImageDpi144 = null;
            this.mnuHeroLabImporter.ImageDpi192 = global::Chummer.Properties.Resources.HeroLab_32;
            this.mnuHeroLabImporter.ImageDpi288 = global::Chummer.Properties.Resources.HeroLab_48;
            this.mnuHeroLabImporter.ImageDpi384 = null;
            this.mnuHeroLabImporter.ImageDpi96 = global::Chummer.Properties.Resources.HeroLab_16;
            this.mnuHeroLabImporter.Name = "mnuHeroLabImporter";
            this.mnuHeroLabImporter.Size = new System.Drawing.Size(171, 22);
            this.mnuHeroLabImporter.Tag = "Menu_Main_HeroLabImporter";
            this.mnuHeroLabImporter.Text = "&Hero Lab Importer";
            this.mnuHeroLabImporter.Click += new System.EventHandler(this.mnuHeroLabImporter_Click);
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
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Image = global::Chummer.Properties.Resources.application_add;
            this.newWindowToolStripMenuItem.ImageDpi120 = null;
            this.newWindowToolStripMenuItem.ImageDpi144 = null;
            this.newWindowToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.application_add1;
            this.newWindowToolStripMenuItem.ImageDpi288 = null;
            this.newWindowToolStripMenuItem.ImageDpi384 = null;
            this.newWindowToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.application_add;
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.newWindowToolStripMenuItem.Tag = "Menu_Main_NewWindow";
            this.newWindowToolStripMenuItem.Text = "&New Window";
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Image = global::Chummer.Properties.Resources.application_delete;
            this.closeAllToolStripMenuItem.ImageDpi120 = null;
            this.closeAllToolStripMenuItem.ImageDpi144 = null;
            this.closeAllToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.application_delete1;
            this.closeAllToolStripMenuItem.ImageDpi288 = null;
            this.closeAllToolStripMenuItem.ImageDpi384 = null;
            this.closeAllToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.application_delete;
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.closeAllToolStripMenuItem.Tag = "Menu_Main_CloseAll";
            this.closeAllToolStripMenuItem.Text = "C&lose All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.CloseAllToolStripMenuItem_Click);
            // 
            // helpMenu
            // 
            this.helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuChummerWiki,
            this.mnuChummerDiscord,
            this.toolStripSeparator8,
            this.mnuHelpRevisionHistory,
            this.mnuHelpDumpshock,
            this.aboutToolStripMenuItem});
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(44, 20);
            this.helpMenu.Tag = "Menu_Main_Help";
            this.helpMenu.Text = "&Help";
            // 
            // mnuChummerWiki
            // 
            this.mnuChummerWiki.Image = global::Chummer.Properties.Resources.world_go;
            this.mnuChummerWiki.ImageDpi120 = null;
            this.mnuChummerWiki.ImageDpi144 = null;
            this.mnuChummerWiki.ImageDpi192 = global::Chummer.Properties.Resources.world_go1;
            this.mnuChummerWiki.ImageDpi288 = null;
            this.mnuChummerWiki.ImageDpi384 = null;
            this.mnuChummerWiki.ImageDpi96 = global::Chummer.Properties.Resources.world_go;
            this.mnuChummerWiki.Name = "mnuChummerWiki";
            this.mnuChummerWiki.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.mnuChummerWiki.Size = new System.Drawing.Size(217, 22);
            this.mnuChummerWiki.Tag = "Menu_Main_ChummerWiki";
            this.mnuChummerWiki.Text = "Chummer &Wiki";
            this.mnuChummerWiki.Click += new System.EventHandler(this.mnuChummerWiki_Click);
            // 
            // mnuChummerDiscord
            // 
            this.mnuChummerDiscord.Image = global::Chummer.Properties.Resources.discord_16px;
            this.mnuChummerDiscord.ImageDpi120 = null;
            this.mnuChummerDiscord.ImageDpi144 = null;
            this.mnuChummerDiscord.ImageDpi192 = global::Chummer.Properties.Resources.discord_32px;
            this.mnuChummerDiscord.ImageDpi288 = null;
            this.mnuChummerDiscord.ImageDpi384 = null;
            this.mnuChummerDiscord.ImageDpi96 = global::Chummer.Properties.Resources.discord_16px;
            this.mnuChummerDiscord.Name = "mnuChummerDiscord";
            this.mnuChummerDiscord.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.mnuChummerDiscord.Size = new System.Drawing.Size(217, 22);
            this.mnuChummerDiscord.Tag = "Menu_Main_ChummerDiscord";
            this.mnuChummerDiscord.Text = "Chummer &Discord";
            this.mnuChummerDiscord.Click += new System.EventHandler(this.mnuChummerDiscord_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(214, 6);
            // 
            // mnuHelpRevisionHistory
            // 
            this.mnuHelpRevisionHistory.Image = global::Chummer.Properties.Resources.report;
            this.mnuHelpRevisionHistory.ImageDpi120 = null;
            this.mnuHelpRevisionHistory.ImageDpi144 = null;
            this.mnuHelpRevisionHistory.ImageDpi192 = global::Chummer.Properties.Resources.report1;
            this.mnuHelpRevisionHistory.ImageDpi288 = null;
            this.mnuHelpRevisionHistory.ImageDpi384 = null;
            this.mnuHelpRevisionHistory.ImageDpi96 = global::Chummer.Properties.Resources.report;
            this.mnuHelpRevisionHistory.Name = "mnuHelpRevisionHistory";
            this.mnuHelpRevisionHistory.Size = new System.Drawing.Size(217, 22);
            this.mnuHelpRevisionHistory.Tag = "Menu_Main_RevisionHistory";
            this.mnuHelpRevisionHistory.Text = "&Revision History";
            this.mnuHelpRevisionHistory.Click += new System.EventHandler(this.mnuHelpRevisionHistory_Click);
            // 
            // mnuHelpDumpshock
            // 
            this.mnuHelpDumpshock.Image = global::Chummer.Properties.Resources.bug;
            this.mnuHelpDumpshock.ImageDpi120 = null;
            this.mnuHelpDumpshock.ImageDpi144 = null;
            this.mnuHelpDumpshock.ImageDpi192 = null;
            this.mnuHelpDumpshock.ImageDpi288 = null;
            this.mnuHelpDumpshock.ImageDpi384 = null;
            this.mnuHelpDumpshock.ImageDpi96 = global::Chummer.Properties.Resources.bug1;
            this.mnuHelpDumpshock.Name = "mnuHelpDumpshock";
            this.mnuHelpDumpshock.Size = new System.Drawing.Size(217, 22);
            this.mnuHelpDumpshock.Tag = "Menu_Main_IssueTracker";
            this.mnuHelpDumpshock.Text = "D&umpshock Thread";
            this.mnuHelpDumpshock.Click += new System.EventHandler(this.mnuHelpDumpshock_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::Chummer.Properties.Resources.information;
            this.aboutToolStripMenuItem.ImageDpi120 = null;
            this.aboutToolStripMenuItem.ImageDpi144 = null;
            this.aboutToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.information1;
            this.aboutToolStripMenuItem.ImageDpi288 = null;
            this.aboutToolStripMenuItem.ImageDpi384 = null;
            this.aboutToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.information;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.aboutToolStripMenuItem.Tag = "Menu_Main_About";
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewCharacter,
            this.tsbOpen});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1264, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "ToolStrip";
            this.toolStrip.ItemAdded += new System.Windows.Forms.ToolStripItemEventHandler(this.toolStrip_ItemAdded);
            this.toolStrip.ItemRemoved += new System.Windows.Forms.ToolStripItemEventHandler(this.toolStrip_ItemRemoved);
            // 
            // tsbNewCharacter
            // 
            this.tsbNewCharacter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNewCharacter.Image = global::Chummer.Properties.Resources.user_add;
            this.tsbNewCharacter.ImageDpi120 = null;
            this.tsbNewCharacter.ImageDpi144 = null;
            this.tsbNewCharacter.ImageDpi192 = global::Chummer.Properties.Resources.user_add1;
            this.tsbNewCharacter.ImageDpi288 = null;
            this.tsbNewCharacter.ImageDpi384 = null;
            this.tsbNewCharacter.ImageDpi96 = global::Chummer.Properties.Resources.user_add;
            this.tsbNewCharacter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewCharacter.Name = "tsbNewCharacter";
            this.tsbNewCharacter.Size = new System.Drawing.Size(23, 22);
            this.tsbNewCharacter.Tag = "Menu_Main_NewCharacter";
            this.tsbNewCharacter.Text = "New";
            this.tsbNewCharacter.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // tsbOpen
            // 
            this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpen.Image = global::Chummer.Properties.Resources.folder_page;
            this.tsbOpen.ImageDpi120 = null;
            this.tsbOpen.ImageDpi144 = null;
            this.tsbOpen.ImageDpi192 = global::Chummer.Properties.Resources.folder_page1;
            this.tsbOpen.ImageDpi288 = null;
            this.tsbOpen.ImageDpi384 = null;
            this.tsbOpen.ImageDpi96 = global::Chummer.Properties.Resources.folder_page;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(23, 22);
            this.tsbOpen.Tag = "Menu_Main_Open";
            this.tsbOpen.Text = "Open";
            this.tsbOpen.Click += new System.EventHandler(this.OpenFile);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.toolStripSeparator1.MergeIndex = 5;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.toolStripSeparator2.MergeIndex = 7;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(111, 6);
            this.toolStripSeparator2.Visible = false;
            // 
            // tabForms
            // 
            this.tabForms.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabForms.Location = new System.Drawing.Point(0, 49);
            this.tabForms.Name = "tabForms";
            this.tabForms.SelectedIndex = 0;
            this.tabForms.Size = new System.Drawing.Size(1264, 22);
            this.tabForms.TabIndex = 3;
            this.tabForms.Visible = false;
            this.tabForms.SelectedIndexChanged += new System.EventHandler(this.tabForms_SelectedIndexChanged);
            this.tabForms.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tabForms_MouseClick);
            // 
            // mnuProcessFile
            // 
            this.mnuProcessFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSave,
            this.tsSaveAs,
            this.toolStripSeparator1,
            this.tsClose,
            this.toolStripSeparator2,
            this.tsPrint});
            this.mnuProcessFile.Name = "mnuProcessFile";
            this.mnuProcessFile.Size = new System.Drawing.Size(115, 104);
            this.mnuProcessFile.Tag = "Menu_Main_File";
            this.mnuProcessFile.Text = "&File";
            // 
            // tsSave
            // 
            this.tsSave.Image = global::Chummer.Properties.Resources.disk;
            this.tsSave.ImageDpi120 = null;
            this.tsSave.ImageDpi144 = null;
            this.tsSave.ImageDpi192 = global::Chummer.Properties.Resources.disk1;
            this.tsSave.ImageDpi288 = null;
            this.tsSave.ImageDpi384 = null;
            this.tsSave.ImageDpi96 = global::Chummer.Properties.Resources.disk;
            this.tsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSave.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tsSave.MergeIndex = 3;
            this.tsSave.Name = "tsSave";
            this.tsSave.Size = new System.Drawing.Size(114, 22);
            this.tsSave.Tag = "Menu_FileSave";
            this.tsSave.Text = "&Save";
            this.tsSave.Click += new System.EventHandler(this.tsSave_Click);
            // 
            // tsSaveAs
            // 
            this.tsSaveAs.Image = global::Chummer.Properties.Resources.disk_multiple;
            this.tsSaveAs.ImageDpi120 = null;
            this.tsSaveAs.ImageDpi144 = null;
            this.tsSaveAs.ImageDpi192 = global::Chummer.Properties.Resources.disk_multiple1;
            this.tsSaveAs.ImageDpi288 = null;
            this.tsSaveAs.ImageDpi384 = null;
            this.tsSaveAs.ImageDpi96 = global::Chummer.Properties.Resources.disk_multiple;
            this.tsSaveAs.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tsSaveAs.MergeIndex = 4;
            this.tsSaveAs.Name = "tsSaveAs";
            this.tsSaveAs.Size = new System.Drawing.Size(114, 22);
            this.tsSaveAs.Tag = "Menu_FileSaveAs";
            this.tsSaveAs.Text = "Save &As";
            this.tsSaveAs.Click += new System.EventHandler(this.tsSaveAs_Click);
            // 
            // tsClose
            // 
            this.tsClose.Image = global::Chummer.Properties.Resources.cancel;
            this.tsClose.ImageDpi120 = null;
            this.tsClose.ImageDpi144 = null;
            this.tsClose.ImageDpi192 = global::Chummer.Properties.Resources.cancel1;
            this.tsClose.ImageDpi288 = null;
            this.tsClose.ImageDpi384 = null;
            this.tsClose.ImageDpi96 = global::Chummer.Properties.Resources.cancel;
            this.tsClose.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tsClose.MergeIndex = 6;
            this.tsClose.Name = "tsClose";
            this.tsClose.Size = new System.Drawing.Size(114, 22);
            this.tsClose.Tag = "Menu_FileClose";
            this.tsClose.Text = "&Close";
            this.tsClose.Click += new System.EventHandler(this.tsClose_Click);
            // 
            // tsPrint
            // 
            this.tsPrint.Image = global::Chummer.Properties.Resources.printer;
            this.tsPrint.ImageDpi120 = null;
            this.tsPrint.ImageDpi144 = null;
            this.tsPrint.ImageDpi192 = global::Chummer.Properties.Resources.printer1;
            this.tsPrint.ImageDpi288 = null;
            this.tsPrint.ImageDpi384 = null;
            this.tsPrint.ImageDpi96 = global::Chummer.Properties.Resources.printer;
            this.tsPrint.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tsPrint.MergeIndex = 8;
            this.tsPrint.Name = "tsPrint";
            this.tsPrint.Size = new System.Drawing.Size(114, 22);
            this.tsPrint.Tag = "Menu_FilePrint";
            this.tsPrint.Text = "&Print";
            this.tsPrint.Click += new System.EventHandler(this.tsPrint_Click);
            // 
            // frmChummerMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tabForms);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "frmChummerMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chummer5";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmChummerMain_Closing);
            this.Load += new System.EventHandler(this.frmChummerMain_Load);
            this.MdiChildActivate += new System.EventHandler(this.frmChummerMain_MdiChildActivate);
            this.DpiChanged += new System.Windows.Forms.DpiChangedEventHandler(this.frmChummerMain_DpiChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmChummerMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmChummerMain_DragEnter);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.mnuProcessFile.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem windowsMenu;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripSeparator mnuFileMRUSeparator;
        private System.Windows.Forms.TabControl tabForms;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ContextMenuStrip mnuProcessFile;
        private DpiFriendlyToolStripButton tsbNewCharacter;
        private DpiFriendlyToolStripButton tsbOpen;
        private DpiFriendlyToolStripMenuItem mnuChummerWiki;
        private DpiFriendlyToolStripMenuItem mnuChummerDiscord;
        private DpiFriendlyToolStripMenuItem mnuHelpRevisionHistory;
        private DpiFriendlyToolStripMenuItem mnuHelpDumpshock;
        private DpiFriendlyToolStripMenuItem aboutToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem newWindowToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem closeAllToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem mnuToolsDiceRoller;
        private DpiFriendlyToolStripMenuItem mnuOptions;
        private DpiFriendlyToolStripMenuItem mnuCharacterOptions;
        private DpiFriendlyToolStripMenuItem mnuToolsUpdate;
        private DpiFriendlyToolStripMenuItem mnuRestart;
        private DpiFriendlyToolStripMenuItem mnuToolsTranslator;
        private DpiFriendlyToolStripMenuItem mnuHeroLabImporter;
        private DpiFriendlyToolStripMenuItem newToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem mnuNewCritter;
        private DpiFriendlyToolStripMenuItem openToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem printToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem mnuFilePrintMultiple;
        private DpiFriendlyToolStripMenuItem printSetupToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem mnuClearUnpinnedItems;
        private DpiFriendlyToolStripMenuItem exitToolStripMenuItem;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU0;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU1;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU2;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU3;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU4;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU5;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU6;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU7;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU8;
        private DpiFriendlyToolStripMenuItem mnuStickyMRU9;
        private DpiFriendlyToolStripMenuItem mnuMRU0;
        private DpiFriendlyToolStripMenuItem mnuMRU1;
        private DpiFriendlyToolStripMenuItem mnuMRU2;
        private DpiFriendlyToolStripMenuItem mnuMRU3;
        private DpiFriendlyToolStripMenuItem mnuMRU4;
        private DpiFriendlyToolStripMenuItem mnuMRU5;
        private DpiFriendlyToolStripMenuItem mnuMRU6;
        private DpiFriendlyToolStripMenuItem mnuMRU7;
        private DpiFriendlyToolStripMenuItem mnuMRU8;
        private DpiFriendlyToolStripMenuItem mnuMRU9;
        private DpiFriendlyToolStripMenuItem tsSave;
        private DpiFriendlyToolStripMenuItem tsSaveAs;
        private DpiFriendlyToolStripMenuItem tsClose;
        private DpiFriendlyToolStripMenuItem tsPrint;
    }
}



