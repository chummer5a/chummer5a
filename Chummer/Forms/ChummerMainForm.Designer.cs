using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class ChummerMainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChummerMainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuNewCritter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.openToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuOpenForPrinting = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuOpenForExport = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStripSeparator3 = new Chummer.ColorableToolStripSeparator();
            this.toolStripSeparator4 = new Chummer.ColorableToolStripSeparator();
            this.printToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuFilePrintMultiple = new Chummer.DpiFriendlyToolStripMenuItem();
            this.printSetupToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuClearUnpinnedItems = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMURSep = new Chummer.ColorableToolStripSeparator();
            this.mnuStickyMRU0 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU1 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU2 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU3 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU4 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU5 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU6 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU7 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU8 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuStickyMRU9 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU0 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU1 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU2 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU3 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU4 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU5 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU6 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU7 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU8 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMRU9 = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuFileMRUSeparator = new Chummer.ColorableToolStripSeparator();
            this.exitToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsDiceRoller = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStripSeparator5 = new Chummer.ColorableToolStripSeparator();
            this.mnuGlobalSettings = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuCharacterSettings = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuToolsUpdate = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuRestart = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStripSeparator6 = new Chummer.ColorableToolStripSeparator();
            this.mnuToolsTranslator = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuXmlEditor = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuHeroLabImporter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuMasterIndex = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuCharacterRoster = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuDataExporter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStripSeparator7 = new Chummer.ColorableToolStripSeparator();
            this.mnuReportBug = new Chummer.DpiFriendlyToolStripMenuItem();
            this.windowsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newWindowToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.closeWindowToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.closeAllToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChummerWiki = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuChummerDiscord = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStripSeparator8 = new Chummer.ColorableToolStripSeparator();
            this.mnuHelpRevisionHistory = new Chummer.DpiFriendlyToolStripMenuItem();
            this.mnuHelpDumpshock = new Chummer.DpiFriendlyToolStripMenuItem();
            this.aboutToolStripMenuItem = new Chummer.DpiFriendlyToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbNewCharacter = new Chummer.DpiFriendlyToolStripButton();
            this.tsbOpen = new Chummer.DpiFriendlyToolStripButton();
            this.tsbOpenForPrinting = new Chummer.DpiFriendlyToolStripButton();
            this.tsbOpenForExport = new Chummer.DpiFriendlyToolStripButton();
            this.toolStripSeparator1 = new Chummer.ColorableToolStripSeparator();
            this.toolStripSeparator2 = new Chummer.ColorableToolStripSeparator();
            this.tabForms = new System.Windows.Forms.TabControl();
            this.mnuProcessFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsSave = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsSaveAs = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsClose = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsPrint = new Chummer.DpiFriendlyToolStripMenuItem();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
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
            this.mnuOpenForPrinting,
            this.mnuOpenForExport,
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
            this.newToolStripMenuItem.Image = global::Chummer.Properties.Resources.user_add_16;
            this.newToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.user_add_20;
            this.newToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.user_add_24;
            this.newToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.user_add_32;
            this.newToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.user_add_48;
            this.newToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.user_add_64;
            this.newToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.user_add_16;
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
            this.mnuNewCritter.Image = global::Chummer.Properties.Resources.ladybird_add_16;
            this.mnuNewCritter.ImageDpi120 = global::Chummer.Properties.Resources.ladybird_add_20;
            this.mnuNewCritter.ImageDpi144 = global::Chummer.Properties.Resources.ladybird_add_24;
            this.mnuNewCritter.ImageDpi192 = global::Chummer.Properties.Resources.ladybird_add_32;
            this.mnuNewCritter.ImageDpi288 = global::Chummer.Properties.Resources.ladybird_add_48;
            this.mnuNewCritter.ImageDpi384 = global::Chummer.Properties.Resources.ladybird_add_64;
            this.mnuNewCritter.ImageDpi96 = global::Chummer.Properties.Resources.ladybird_add_16;
            this.mnuNewCritter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuNewCritter.Name = "mnuNewCritter";
            this.mnuNewCritter.Size = new System.Drawing.Size(195, 22);
            this.mnuNewCritter.Tag = "Menu_Main_NewCritter";
            this.mnuNewCritter.Text = "New &Critter";
            this.mnuNewCritter.Click += new System.EventHandler(this.mnuNewCritter_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::Chummer.Properties.Resources.folder_page_16;
            this.openToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.folder_page_20;
            this.openToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.folder_page_24;
            this.openToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.folder_page_32;
            this.openToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.folder_page_48;
            this.openToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.folder_page_64;
            this.openToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.folder_page_16;
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.openToolStripMenuItem.Tag = "Menu_Main_Open";
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenFile);
            // 
            // mnuOpenForPrinting
            // 
            this.mnuOpenForPrinting.Image = global::Chummer.Properties.Resources.folder_print_16;
            this.mnuOpenForPrinting.ImageDpi120 = global::Chummer.Properties.Resources.folder_print_20;
            this.mnuOpenForPrinting.ImageDpi144 = global::Chummer.Properties.Resources.folder_print_24;
            this.mnuOpenForPrinting.ImageDpi192 = global::Chummer.Properties.Resources.folder_print_32;
            this.mnuOpenForPrinting.ImageDpi288 = global::Chummer.Properties.Resources.folder_print_48;
            this.mnuOpenForPrinting.ImageDpi384 = global::Chummer.Properties.Resources.folder_print_64;
            this.mnuOpenForPrinting.ImageDpi96 = global::Chummer.Properties.Resources.folder_print_16;
            this.mnuOpenForPrinting.Name = "mnuOpenForPrinting";
            this.mnuOpenForPrinting.Size = new System.Drawing.Size(195, 22);
            this.mnuOpenForPrinting.Tag = "Menu_Main_OpenForPrinting";
            this.mnuOpenForPrinting.Text = "Open for P&rinting";
            this.mnuOpenForPrinting.Click += new System.EventHandler(this.OpenFileForPrinting);
            // 
            // mnuOpenForExport
            // 
            this.mnuOpenForExport.Image = global::Chummer.Properties.Resources.folder_script_go_16;
            this.mnuOpenForExport.ImageDpi120 = global::Chummer.Properties.Resources.folder_script_go_20;
            this.mnuOpenForExport.ImageDpi144 = global::Chummer.Properties.Resources.folder_script_go_24;
            this.mnuOpenForExport.ImageDpi192 = global::Chummer.Properties.Resources.folder_script_go_32;
            this.mnuOpenForExport.ImageDpi288 = global::Chummer.Properties.Resources.folder_script_go_48;
            this.mnuOpenForExport.ImageDpi384 = global::Chummer.Properties.Resources.folder_script_go_64;
            this.mnuOpenForExport.ImageDpi96 = global::Chummer.Properties.Resources.folder_script_go_16;
            this.mnuOpenForExport.Name = "mnuOpenForExport";
            this.mnuOpenForExport.Size = new System.Drawing.Size(195, 22);
            this.mnuOpenForExport.Tag = "Menu_Main_OpenForExport";
            this.mnuOpenForExport.Text = "Open for E&xport";
            this.mnuOpenForExport.Click += new System.EventHandler(this.OpenFileForExport);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.DefaultColorScheme = true;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.DefaultColorScheme = true;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            this.toolStripSeparator4.Visible = false;
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Image = global::Chummer.Properties.Resources.printer_16;
            this.printToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.printer_20;
            this.printToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.printer_24;
            this.printToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.printer_32;
            this.printToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.printer_48;
            this.printToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.printer_64;
            this.printToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.printer_16;
            this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printToolStripMenuItem.Text = "&Print";
            this.printToolStripMenuItem.Visible = false;
            // 
            // mnuFilePrintMultiple
            // 
            this.mnuFilePrintMultiple.Image = global::Chummer.Properties.Resources.printer_add_16;
            this.mnuFilePrintMultiple.ImageDpi120 = global::Chummer.Properties.Resources.printer_add_20;
            this.mnuFilePrintMultiple.ImageDpi144 = global::Chummer.Properties.Resources.printer_add_24;
            this.mnuFilePrintMultiple.ImageDpi192 = global::Chummer.Properties.Resources.printer_add_32;
            this.mnuFilePrintMultiple.ImageDpi288 = global::Chummer.Properties.Resources.printer_add_48;
            this.mnuFilePrintMultiple.ImageDpi384 = global::Chummer.Properties.Resources.printer_add_64;
            this.mnuFilePrintMultiple.ImageDpi96 = global::Chummer.Properties.Resources.printer_add_16;
            this.mnuFilePrintMultiple.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuFilePrintMultiple.Name = "mnuFilePrintMultiple";
            this.mnuFilePrintMultiple.Size = new System.Drawing.Size(195, 22);
            this.mnuFilePrintMultiple.Tag = "Menu_Main_PrintMultiple";
            this.mnuFilePrintMultiple.Text = "Print &Multiple";
            this.mnuFilePrintMultiple.Click += new System.EventHandler(this.mnuFilePrintMultiple_Click);
            // 
            // printSetupToolStripMenuItem
            // 
            this.printSetupToolStripMenuItem.Image = global::Chummer.Properties.Resources.printer_empty_16;
            this.printSetupToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.printer_empty_20;
            this.printSetupToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.printer_empty_24;
            this.printSetupToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.printer_empty_32;
            this.printSetupToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.printer_empty_48;
            this.printSetupToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.printer_empty_64;
            this.printSetupToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.printer_empty_16;
            this.printSetupToolStripMenuItem.Name = "printSetupToolStripMenuItem";
            this.printSetupToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.printSetupToolStripMenuItem.Text = "Print Setup";
            this.printSetupToolStripMenuItem.Visible = false;
            // 
            // mnuClearUnpinnedItems
            // 
            this.mnuClearUnpinnedItems.Image = global::Chummer.Properties.Resources.delete_16;
            this.mnuClearUnpinnedItems.ImageDpi120 = global::Chummer.Properties.Resources.delete_20;
            this.mnuClearUnpinnedItems.ImageDpi144 = global::Chummer.Properties.Resources.delete_24;
            this.mnuClearUnpinnedItems.ImageDpi192 = global::Chummer.Properties.Resources.delete_32;
            this.mnuClearUnpinnedItems.ImageDpi288 = global::Chummer.Properties.Resources.delete_48;
            this.mnuClearUnpinnedItems.ImageDpi384 = global::Chummer.Properties.Resources.delete_64;
            this.mnuClearUnpinnedItems.ImageDpi96 = global::Chummer.Properties.Resources.delete_16;
            this.mnuClearUnpinnedItems.Name = "mnuClearUnpinnedItems";
            this.mnuClearUnpinnedItems.Size = new System.Drawing.Size(195, 22);
            this.mnuClearUnpinnedItems.Tag = "Menu_Main_ClearUnpinnedItems";
            this.mnuClearUnpinnedItems.Text = "Clear Unpinned Items";
            this.mnuClearUnpinnedItems.Click += new System.EventHandler(this.mnuClearUnpinnedItems_Click);
            // 
            // mnuMURSep
            // 
            this.mnuMURSep.DefaultColorScheme = true;
            this.mnuMURSep.Name = "mnuMURSep";
            this.mnuMURSep.Size = new System.Drawing.Size(192, 6);
            // 
            // mnuStickyMRU0
            // 
            this.mnuStickyMRU0.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU0.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU0.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU0.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU0.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU0.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU0.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU0.Name = "mnuStickyMRU0";
            this.mnuStickyMRU0.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU0.Text = "[StickyMRU0]";
            this.mnuStickyMRU0.Visible = false;
            this.mnuStickyMRU0.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU1
            // 
            this.mnuStickyMRU1.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU1.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU1.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU1.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU1.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU1.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU1.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU1.Name = "mnuStickyMRU1";
            this.mnuStickyMRU1.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU1.Text = "[StickyMRU1]";
            this.mnuStickyMRU1.Visible = false;
            this.mnuStickyMRU1.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU2
            // 
            this.mnuStickyMRU2.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU2.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU2.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU2.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU2.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU2.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU2.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU2.Name = "mnuStickyMRU2";
            this.mnuStickyMRU2.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU2.Text = "[StickyMRU2]";
            this.mnuStickyMRU2.Visible = false;
            this.mnuStickyMRU2.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU3
            // 
            this.mnuStickyMRU3.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU3.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU3.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU3.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU3.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU3.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU3.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU3.Name = "mnuStickyMRU3";
            this.mnuStickyMRU3.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU3.Text = "[StickyMRU3]";
            this.mnuStickyMRU3.Visible = false;
            this.mnuStickyMRU3.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU4
            // 
            this.mnuStickyMRU4.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU4.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU4.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU4.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU4.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU4.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU4.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU4.Name = "mnuStickyMRU4";
            this.mnuStickyMRU4.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU4.Text = "[StickyMRU4]";
            this.mnuStickyMRU4.Visible = false;
            this.mnuStickyMRU4.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU5
            // 
            this.mnuStickyMRU5.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU5.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU5.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU5.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU5.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU5.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU5.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU5.Name = "mnuStickyMRU5";
            this.mnuStickyMRU5.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU5.Text = "[StickyMRU5]";
            this.mnuStickyMRU5.Visible = false;
            this.mnuStickyMRU5.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU6
            // 
            this.mnuStickyMRU6.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU6.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU6.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU6.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU6.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU6.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU6.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU6.Name = "mnuStickyMRU6";
            this.mnuStickyMRU6.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU6.Text = "[StickyMRU6]";
            this.mnuStickyMRU6.Visible = false;
            this.mnuStickyMRU6.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU7
            // 
            this.mnuStickyMRU7.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU7.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU7.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU7.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU7.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU7.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU7.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU7.Name = "mnuStickyMRU7";
            this.mnuStickyMRU7.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU7.Text = "[StickyMRU7]";
            this.mnuStickyMRU7.Visible = false;
            this.mnuStickyMRU7.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU8
            // 
            this.mnuStickyMRU8.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU8.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU8.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU8.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU8.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU8.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU8.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU8.Name = "mnuStickyMRU8";
            this.mnuStickyMRU8.Size = new System.Drawing.Size(195, 22);
            this.mnuStickyMRU8.Text = "[StickyMRU8]";
            this.mnuStickyMRU8.Visible = false;
            this.mnuStickyMRU8.Click += new System.EventHandler(this.mnuMRU_Click);
            this.mnuStickyMRU8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mnuStickyMRU_MouseDown);
            // 
            // mnuStickyMRU9
            // 
            this.mnuStickyMRU9.Image = global::Chummer.Properties.Resources.asterisk_orange_16;
            this.mnuStickyMRU9.ImageDpi120 = global::Chummer.Properties.Resources.asterisk_orange_20;
            this.mnuStickyMRU9.ImageDpi144 = global::Chummer.Properties.Resources.asterisk_orange_24;
            this.mnuStickyMRU9.ImageDpi192 = global::Chummer.Properties.Resources.asterisk_orange_32;
            this.mnuStickyMRU9.ImageDpi288 = global::Chummer.Properties.Resources.asterisk_orange_48;
            this.mnuStickyMRU9.ImageDpi384 = global::Chummer.Properties.Resources.asterisk_orange_64;
            this.mnuStickyMRU9.ImageDpi96 = global::Chummer.Properties.Resources.asterisk_orange_16;
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
            this.mnuFileMRUSeparator.DefaultColorScheme = true;
            this.mnuFileMRUSeparator.Name = "mnuFileMRUSeparator";
            this.mnuFileMRUSeparator.Size = new System.Drawing.Size(192, 6);
            this.mnuFileMRUSeparator.Visible = false;
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::Chummer.Properties.Resources.door_out_16;
            this.exitToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.door_out_20;
            this.exitToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.door_out_24;
            this.exitToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.door_out_32;
            this.exitToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.door_out_48;
            this.exitToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.door_out_64;
            this.exitToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.door_out_16;
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
            this.mnuGlobalSettings,
            this.mnuCharacterSettings,
            this.mnuToolsUpdate,
            this.mnuRestart,
            this.toolStripSeparator6,
            this.mnuToolsTranslator,
            this.mnuXmlEditor,
            this.mnuHeroLabImporter,
            this.mnuMasterIndex,
            this.mnuCharacterRoster,
            this.mnuDataExporter,
            this.toolStripSeparator7,
            this.mnuReportBug});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(47, 20);
            this.toolsMenu.Tag = "Menu_Main_Tools";
            this.toolsMenu.Text = "&Tools";
            // 
            // mnuToolsDiceRoller
            // 
            this.mnuToolsDiceRoller.Image = global::Chummer.Properties.Resources.die_16;
            this.mnuToolsDiceRoller.ImageDpi120 = global::Chummer.Properties.Resources.die_20;
            this.mnuToolsDiceRoller.ImageDpi144 = global::Chummer.Properties.Resources.die_24;
            this.mnuToolsDiceRoller.ImageDpi192 = global::Chummer.Properties.Resources.die_32;
            this.mnuToolsDiceRoller.ImageDpi288 = global::Chummer.Properties.Resources.die_48;
            this.mnuToolsDiceRoller.ImageDpi384 = global::Chummer.Properties.Resources.die_64;
            this.mnuToolsDiceRoller.ImageDpi96 = global::Chummer.Properties.Resources.die_16;
            this.mnuToolsDiceRoller.Name = "mnuToolsDiceRoller";
            this.mnuToolsDiceRoller.Size = new System.Drawing.Size(202, 22);
            this.mnuToolsDiceRoller.Tag = "Menu_Main_DiceRoller";
            this.mnuToolsDiceRoller.Text = "&Dice Roller";
            this.mnuToolsDiceRoller.Click += new System.EventHandler(this.mnuToolsDiceRoller_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.DefaultColorScheme = true;
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(199, 6);
            // 
            // mnuGlobalSettings
            // 
            this.mnuGlobalSettings.Image = global::Chummer.Properties.Resources.cog_edit_16;
            this.mnuGlobalSettings.ImageDpi120 = global::Chummer.Properties.Resources.cog_edit_20;
            this.mnuGlobalSettings.ImageDpi144 = global::Chummer.Properties.Resources.cog_edit_24;
            this.mnuGlobalSettings.ImageDpi192 = global::Chummer.Properties.Resources.cog_edit_32;
            this.mnuGlobalSettings.ImageDpi288 = global::Chummer.Properties.Resources.cog_edit_48;
            this.mnuGlobalSettings.ImageDpi384 = global::Chummer.Properties.Resources.cog_edit_64;
            this.mnuGlobalSettings.ImageDpi96 = global::Chummer.Properties.Resources.cog_edit_16;
            this.mnuGlobalSettings.Name = "mnuGlobalSettings";
            this.mnuGlobalSettings.Size = new System.Drawing.Size(202, 22);
            this.mnuGlobalSettings.Tag = "Menu_Main_Options";
            this.mnuGlobalSettings.Text = "&Global Settings";
            this.mnuGlobalSettings.Click += new System.EventHandler(this.mnuGlobalSettings_Click);
            // 
            // mnuCharacterSettings
            // 
            this.mnuCharacterSettings.Image = global::Chummer.Properties.Resources.group_gear_16;
            this.mnuCharacterSettings.ImageDpi120 = global::Chummer.Properties.Resources.group_gear_20;
            this.mnuCharacterSettings.ImageDpi144 = global::Chummer.Properties.Resources.group_gear_24;
            this.mnuCharacterSettings.ImageDpi192 = global::Chummer.Properties.Resources.group_gear_32;
            this.mnuCharacterSettings.ImageDpi288 = global::Chummer.Properties.Resources.group_gear_48;
            this.mnuCharacterSettings.ImageDpi384 = global::Chummer.Properties.Resources.group_gear_64;
            this.mnuCharacterSettings.ImageDpi96 = global::Chummer.Properties.Resources.group_gear_16;
            this.mnuCharacterSettings.Name = "mnuCharacterSettings";
            this.mnuCharacterSettings.Size = new System.Drawing.Size(202, 22);
            this.mnuCharacterSettings.Tag = "Menu_Main_Character_Options";
            this.mnuCharacterSettings.Text = "&Character Settings";
            this.mnuCharacterSettings.Click += new System.EventHandler(this.mnuCharacterSettings_Click);
            // 
            // mnuToolsUpdate
            // 
            this.mnuToolsUpdate.Image = global::Chummer.Properties.Resources.database_refresh_16;
            this.mnuToolsUpdate.ImageDpi120 = global::Chummer.Properties.Resources.database_refresh_20;
            this.mnuToolsUpdate.ImageDpi144 = global::Chummer.Properties.Resources.database_refresh_24;
            this.mnuToolsUpdate.ImageDpi192 = global::Chummer.Properties.Resources.database_refresh_32;
            this.mnuToolsUpdate.ImageDpi288 = global::Chummer.Properties.Resources.database_refresh_48;
            this.mnuToolsUpdate.ImageDpi384 = global::Chummer.Properties.Resources.database_refresh_64;
            this.mnuToolsUpdate.ImageDpi96 = global::Chummer.Properties.Resources.database_refresh_16;
            this.mnuToolsUpdate.Name = "mnuToolsUpdate";
            this.mnuToolsUpdate.Size = new System.Drawing.Size(202, 22);
            this.mnuToolsUpdate.Tag = "Menu_Main_Update";
            this.mnuToolsUpdate.Text = "Check for &Updates";
            this.mnuToolsUpdate.Click += new System.EventHandler(this.mnuToolsUpdate_Click);
            // 
            // mnuRestart
            // 
            this.mnuRestart.Image = global::Chummer.Properties.Resources.arrow_redo_16;
            this.mnuRestart.ImageDpi120 = global::Chummer.Properties.Resources.arrow_redo_20;
            this.mnuRestart.ImageDpi144 = global::Chummer.Properties.Resources.arrow_redo_24;
            this.mnuRestart.ImageDpi192 = global::Chummer.Properties.Resources.arrow_redo_32;
            this.mnuRestart.ImageDpi288 = global::Chummer.Properties.Resources.arrow_redo_48;
            this.mnuRestart.ImageDpi384 = global::Chummer.Properties.Resources.arrow_redo_64;
            this.mnuRestart.ImageDpi96 = global::Chummer.Properties.Resources.arrow_redo_16;
            this.mnuRestart.Name = "mnuRestart";
            this.mnuRestart.Size = new System.Drawing.Size(202, 22);
            this.mnuRestart.Tag = "Menu_Main_Restart";
            this.mnuRestart.Text = "&Restart Chummer";
            this.mnuRestart.Click += new System.EventHandler(this.mnuRestart_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.DefaultColorScheme = true;
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(199, 6);
            // 
            // mnuToolsTranslator
            // 
            this.mnuToolsTranslator.Image = global::Chummer.Properties.Resources.locate_16;
            this.mnuToolsTranslator.ImageDpi120 = global::Chummer.Properties.Resources.locate_20;
            this.mnuToolsTranslator.ImageDpi144 = global::Chummer.Properties.Resources.locate_24;
            this.mnuToolsTranslator.ImageDpi192 = global::Chummer.Properties.Resources.locate_32;
            this.mnuToolsTranslator.ImageDpi288 = global::Chummer.Properties.Resources.locate_48;
            this.mnuToolsTranslator.ImageDpi384 = global::Chummer.Properties.Resources.locate_64;
            this.mnuToolsTranslator.ImageDpi96 = global::Chummer.Properties.Resources.locate_16;
            this.mnuToolsTranslator.Name = "mnuToolsTranslator";
            this.mnuToolsTranslator.Size = new System.Drawing.Size(202, 22);
            this.mnuToolsTranslator.Tag = "Menu_Main_Translator";
            this.mnuToolsTranslator.Text = "&Translator";
            this.mnuToolsTranslator.Click += new System.EventHandler(this.mnuToolsTranslator_Click);
            // 
            // mnuXmlEditor
            // 
            this.mnuXmlEditor.Image = global::Chummer.Properties.Resources.track_changes_edit_16;
            this.mnuXmlEditor.ImageDpi120 = global::Chummer.Properties.Resources.track_changes_edit_20;
            this.mnuXmlEditor.ImageDpi144 = global::Chummer.Properties.Resources.track_changes_edit_24;
            this.mnuXmlEditor.ImageDpi192 = global::Chummer.Properties.Resources.track_changes_edit_32;
            this.mnuXmlEditor.ImageDpi288 = global::Chummer.Properties.Resources.track_changes_edit_48;
            this.mnuXmlEditor.ImageDpi384 = global::Chummer.Properties.Resources.track_changes_edit_64;
            this.mnuXmlEditor.ImageDpi96 = global::Chummer.Properties.Resources.track_changes_edit_16;
            this.mnuXmlEditor.Name = "mnuXmlEditor";
            this.mnuXmlEditor.Size = new System.Drawing.Size(202, 22);
            this.mnuXmlEditor.Tag = "Menu_Main_XmlEditor";
            this.mnuXmlEditor.Text = "&XML Amendment Editor";
            this.mnuXmlEditor.Click += new System.EventHandler(this.mnuXmlEditor_Click);
            // 
            // mnuHeroLabImporter
            // 
            this.mnuHeroLabImporter.Image = global::Chummer.Properties.Resources.herolab_16;
            this.mnuHeroLabImporter.ImageDpi120 = global::Chummer.Properties.Resources.herolab_20;
            this.mnuHeroLabImporter.ImageDpi144 = global::Chummer.Properties.Resources.herolab_24;
            this.mnuHeroLabImporter.ImageDpi192 = global::Chummer.Properties.Resources.herolab_32;
            this.mnuHeroLabImporter.ImageDpi288 = global::Chummer.Properties.Resources.herolab_48;
            this.mnuHeroLabImporter.ImageDpi384 = global::Chummer.Properties.Resources.herolab_64;
            this.mnuHeroLabImporter.ImageDpi96 = global::Chummer.Properties.Resources.herolab_16;
            this.mnuHeroLabImporter.Name = "mnuHeroLabImporter";
            this.mnuHeroLabImporter.Size = new System.Drawing.Size(202, 22);
            this.mnuHeroLabImporter.Tag = "Menu_Main_HeroLabImporter";
            this.mnuHeroLabImporter.Text = "&Hero Lab Importer";
            this.mnuHeroLabImporter.Click += new System.EventHandler(this.mnuHeroLabImporter_Click);
            // 
            // mnuMasterIndex
            // 
            this.mnuMasterIndex.Image = global::Chummer.Properties.Resources.books_16;
            this.mnuMasterIndex.ImageDpi120 = global::Chummer.Properties.Resources.books_20;
            this.mnuMasterIndex.ImageDpi144 = global::Chummer.Properties.Resources.books_24;
            this.mnuMasterIndex.ImageDpi192 = global::Chummer.Properties.Resources.books_32;
            this.mnuMasterIndex.ImageDpi288 = global::Chummer.Properties.Resources.books_48;
            this.mnuMasterIndex.ImageDpi384 = global::Chummer.Properties.Resources.books_64;
            this.mnuMasterIndex.ImageDpi96 = global::Chummer.Properties.Resources.books_16;
            this.mnuMasterIndex.Name = "mnuMasterIndex";
            this.mnuMasterIndex.Size = new System.Drawing.Size(202, 22);
            this.mnuMasterIndex.Tag = "Menu_Main_MasterIndex";
            this.mnuMasterIndex.Text = "&Master Index";
            this.mnuMasterIndex.Click += new System.EventHandler(this.mnuMasterIndex_Click);
            // 
            // mnuCharacterRoster
            // 
            this.mnuCharacterRoster.Image = global::Chummer.Properties.Resources.group_16;
            this.mnuCharacterRoster.ImageDpi120 = global::Chummer.Properties.Resources.group_20;
            this.mnuCharacterRoster.ImageDpi144 = global::Chummer.Properties.Resources.group_24;
            this.mnuCharacterRoster.ImageDpi192 = global::Chummer.Properties.Resources.group_32;
            this.mnuCharacterRoster.ImageDpi288 = global::Chummer.Properties.Resources.group_48;
            this.mnuCharacterRoster.ImageDpi384 = global::Chummer.Properties.Resources.group_64;
            this.mnuCharacterRoster.ImageDpi96 = global::Chummer.Properties.Resources.group_16;
            this.mnuCharacterRoster.Name = "mnuCharacterRoster";
            this.mnuCharacterRoster.Size = new System.Drawing.Size(202, 22);
            this.mnuCharacterRoster.Tag = "Menu_Main_CharacterRoster";
            this.mnuCharacterRoster.Text = "Ch&aracter Roster";
            this.mnuCharacterRoster.Click += new System.EventHandler(this.mnuCharacterRoster_Click);
            // 
            // mnuDataExporter
            // 
            this.mnuDataExporter.Image = global::Chummer.Properties.Resources.database_go_16;
            this.mnuDataExporter.ImageDpi120 = global::Chummer.Properties.Resources.database_go_20;
            this.mnuDataExporter.ImageDpi144 = global::Chummer.Properties.Resources.database_go_24;
            this.mnuDataExporter.ImageDpi192 = global::Chummer.Properties.Resources.database_go_32;
            this.mnuDataExporter.ImageDpi288 = global::Chummer.Properties.Resources.database_go_48;
            this.mnuDataExporter.ImageDpi384 = global::Chummer.Properties.Resources.database_go_64;
            this.mnuDataExporter.ImageDpi96 = global::Chummer.Properties.Resources.database_go_16;
            this.mnuDataExporter.Name = "mnuDataExporter";
            this.mnuDataExporter.Size = new System.Drawing.Size(202, 22);
            this.mnuDataExporter.Text = "Data &Exporter";
            this.mnuDataExporter.Click += new System.EventHandler(this.mnuDataExporter_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.DefaultColorScheme = true;
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(199, 6);
            // 
            // mnuReportBug
            // 
            this.mnuReportBug.Image = global::Chummer.Properties.Resources.bug_16;
            this.mnuReportBug.ImageDpi120 = global::Chummer.Properties.Resources.bug_20;
            this.mnuReportBug.ImageDpi144 = global::Chummer.Properties.Resources.bug_24;
            this.mnuReportBug.ImageDpi192 = global::Chummer.Properties.Resources.bug_32;
            this.mnuReportBug.ImageDpi288 = global::Chummer.Properties.Resources.bug_48;
            this.mnuReportBug.ImageDpi384 = global::Chummer.Properties.Resources.bug_64;
            this.mnuReportBug.ImageDpi96 = global::Chummer.Properties.Resources.bug_16;
            this.mnuReportBug.Name = "mnuReportBug";
            this.mnuReportBug.Size = new System.Drawing.Size(202, 22);
            this.mnuReportBug.Tag = "Menu_Main_ReportBug";
            this.mnuReportBug.Text = "Report a &Bug";
            this.mnuReportBug.Click += new System.EventHandler(this.mnuReportBug_Click);
            // 
            // windowsMenu
            // 
            this.windowsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newWindowToolStripMenuItem,
            this.closeWindowToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.windowsMenu.Name = "windowsMenu";
            this.windowsMenu.Size = new System.Drawing.Size(68, 20);
            this.windowsMenu.Tag = "Menu_Main_Window";
            this.windowsMenu.Text = "&Windows";
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Image = global::Chummer.Properties.Resources.application_add_16;
            this.newWindowToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.application_add_20;
            this.newWindowToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.application_add_24;
            this.newWindowToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.application_add_32;
            this.newWindowToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.application_add_48;
            this.newWindowToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.application_add_64;
            this.newWindowToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.application_add_16;
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.newWindowToolStripMenuItem.Tag = "Menu_Main_NewWindow";
            this.newWindowToolStripMenuItem.Text = "&New Window";
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.ShowNewForm);
            // 
            // closeWindowToolStripMenuItem
            // 
            this.closeWindowToolStripMenuItem.Image = global::Chummer.Properties.Resources.application_delete_16;
            this.closeWindowToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.application_delete_20;
            this.closeWindowToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.application_delete_24;
            this.closeWindowToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.application_delete_32;
            this.closeWindowToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.application_delete_48;
            this.closeWindowToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.application_delete_64;
            this.closeWindowToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.application_delete_16;
            this.closeWindowToolStripMenuItem.Name = "closeWindowToolStripMenuItem";
            this.closeWindowToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.closeWindowToolStripMenuItem.Tag = "Menu_Main_CloseWindow";
            this.closeWindowToolStripMenuItem.Text = "&Close Window";
            this.closeWindowToolStripMenuItem.Click += new System.EventHandler(this.closeWindowToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Image = global::Chummer.Properties.Resources.application_cascade_delete_16;
            this.closeAllToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.application_cascade_delete_20;
            this.closeAllToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.application_cascade_delete_24;
            this.closeAllToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.application_cascade_delete_32;
            this.closeAllToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.application_cascade_delete_48;
            this.closeAllToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.application_cascade_delete_64;
            this.closeAllToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.application_cascade_delete_16;
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
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
            this.mnuChummerWiki.Image = global::Chummer.Properties.Resources.world_go_16;
            this.mnuChummerWiki.ImageDpi120 = global::Chummer.Properties.Resources.world_go_20;
            this.mnuChummerWiki.ImageDpi144 = global::Chummer.Properties.Resources.world_go_24;
            this.mnuChummerWiki.ImageDpi192 = global::Chummer.Properties.Resources.world_go_32;
            this.mnuChummerWiki.ImageDpi288 = global::Chummer.Properties.Resources.world_go_48;
            this.mnuChummerWiki.ImageDpi384 = global::Chummer.Properties.Resources.world_go_64;
            this.mnuChummerWiki.ImageDpi96 = global::Chummer.Properties.Resources.world_go_16;
            this.mnuChummerWiki.Name = "mnuChummerWiki";
            this.mnuChummerWiki.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.mnuChummerWiki.Size = new System.Drawing.Size(217, 22);
            this.mnuChummerWiki.Tag = "Menu_Main_ChummerWiki";
            this.mnuChummerWiki.Text = "Chummer &Wiki";
            this.mnuChummerWiki.Click += new System.EventHandler(this.mnuChummerWiki_Click);
            // 
            // mnuChummerDiscord
            // 
            this.mnuChummerDiscord.Image = global::Chummer.Properties.Resources.discord_16;
            this.mnuChummerDiscord.ImageDpi120 = global::Chummer.Properties.Resources.discord_20;
            this.mnuChummerDiscord.ImageDpi144 = global::Chummer.Properties.Resources.discord_24;
            this.mnuChummerDiscord.ImageDpi192 = global::Chummer.Properties.Resources.discord_32;
            this.mnuChummerDiscord.ImageDpi288 = global::Chummer.Properties.Resources.discord_48;
            this.mnuChummerDiscord.ImageDpi384 = global::Chummer.Properties.Resources.discord_64;
            this.mnuChummerDiscord.ImageDpi96 = global::Chummer.Properties.Resources.discord_16;
            this.mnuChummerDiscord.Name = "mnuChummerDiscord";
            this.mnuChummerDiscord.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.mnuChummerDiscord.Size = new System.Drawing.Size(217, 22);
            this.mnuChummerDiscord.Tag = "Menu_Main_ChummerDiscord";
            this.mnuChummerDiscord.Text = "Chummer &Discord";
            this.mnuChummerDiscord.Click += new System.EventHandler(this.mnuChummerDiscord_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.DefaultColorScheme = true;
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(214, 6);
            // 
            // mnuHelpRevisionHistory
            // 
            this.mnuHelpRevisionHistory.Image = global::Chummer.Properties.Resources.report_16;
            this.mnuHelpRevisionHistory.ImageDpi120 = global::Chummer.Properties.Resources.report_20;
            this.mnuHelpRevisionHistory.ImageDpi144 = global::Chummer.Properties.Resources.report_24;
            this.mnuHelpRevisionHistory.ImageDpi192 = global::Chummer.Properties.Resources.report_32;
            this.mnuHelpRevisionHistory.ImageDpi288 = global::Chummer.Properties.Resources.report_48;
            this.mnuHelpRevisionHistory.ImageDpi384 = global::Chummer.Properties.Resources.report_64;
            this.mnuHelpRevisionHistory.ImageDpi96 = global::Chummer.Properties.Resources.report_16;
            this.mnuHelpRevisionHistory.Name = "mnuHelpRevisionHistory";
            this.mnuHelpRevisionHistory.Size = new System.Drawing.Size(217, 22);
            this.mnuHelpRevisionHistory.Tag = "Menu_Main_RevisionHistory";
            this.mnuHelpRevisionHistory.Text = "&Revision History";
            this.mnuHelpRevisionHistory.Click += new System.EventHandler(this.mnuHelpRevisionHistory_Click);
            // 
            // mnuHelpDumpshock
            // 
            this.mnuHelpDumpshock.Image = global::Chummer.Properties.Resources.bug_fixing_16;
            this.mnuHelpDumpshock.ImageDpi120 = global::Chummer.Properties.Resources.bug_fixing_20;
            this.mnuHelpDumpshock.ImageDpi144 = global::Chummer.Properties.Resources.bug_fixing_24;
            this.mnuHelpDumpshock.ImageDpi192 = global::Chummer.Properties.Resources.bug_fixing_32;
            this.mnuHelpDumpshock.ImageDpi288 = global::Chummer.Properties.Resources.bug_fixing_48;
            this.mnuHelpDumpshock.ImageDpi384 = global::Chummer.Properties.Resources.bug_fixing_64;
            this.mnuHelpDumpshock.ImageDpi96 = global::Chummer.Properties.Resources.bug_fixing_16;
            this.mnuHelpDumpshock.Name = "mnuHelpDumpshock";
            this.mnuHelpDumpshock.Size = new System.Drawing.Size(217, 22);
            this.mnuHelpDumpshock.Tag = "Menu_Main_IssueTracker";
            this.mnuHelpDumpshock.Text = "Issue Tracker";
            this.mnuHelpDumpshock.Click += new System.EventHandler(this.mnuHelpDumpshock_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::Chummer.Properties.Resources.information_16;
            this.aboutToolStripMenuItem.ImageDpi120 = global::Chummer.Properties.Resources.information_20;
            this.aboutToolStripMenuItem.ImageDpi144 = global::Chummer.Properties.Resources.information_24;
            this.aboutToolStripMenuItem.ImageDpi192 = global::Chummer.Properties.Resources.information_32;
            this.aboutToolStripMenuItem.ImageDpi288 = global::Chummer.Properties.Resources.information_48;
            this.aboutToolStripMenuItem.ImageDpi384 = global::Chummer.Properties.Resources.information_64;
            this.aboutToolStripMenuItem.ImageDpi96 = global::Chummer.Properties.Resources.information_16;
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
            this.tsbOpen,
            this.tsbOpenForPrinting,
            this.tsbOpenForExport});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1264, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "ToolStrip";
            this.toolStrip.ItemAdded += new System.Windows.Forms.ToolStripItemEventHandler(this.RefreshToolStripDisplays);
            this.toolStrip.ItemRemoved += new System.Windows.Forms.ToolStripItemEventHandler(this.RefreshToolStripDisplays);
            // 
            // tsbNewCharacter
            // 
            this.tsbNewCharacter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNewCharacter.Image = global::Chummer.Properties.Resources.user_add_16;
            this.tsbNewCharacter.ImageDpi120 = global::Chummer.Properties.Resources.user_add_20;
            this.tsbNewCharacter.ImageDpi144 = global::Chummer.Properties.Resources.user_add_24;
            this.tsbNewCharacter.ImageDpi192 = global::Chummer.Properties.Resources.user_add_32;
            this.tsbNewCharacter.ImageDpi288 = global::Chummer.Properties.Resources.user_add_48;
            this.tsbNewCharacter.ImageDpi384 = global::Chummer.Properties.Resources.user_add_64;
            this.tsbNewCharacter.ImageDpi96 = global::Chummer.Properties.Resources.user_add_16;
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
            this.tsbOpen.Image = global::Chummer.Properties.Resources.folder_page_16;
            this.tsbOpen.ImageDpi120 = global::Chummer.Properties.Resources.folder_page_20;
            this.tsbOpen.ImageDpi144 = global::Chummer.Properties.Resources.folder_page_24;
            this.tsbOpen.ImageDpi192 = global::Chummer.Properties.Resources.folder_page_32;
            this.tsbOpen.ImageDpi288 = global::Chummer.Properties.Resources.folder_page_48;
            this.tsbOpen.ImageDpi384 = global::Chummer.Properties.Resources.folder_page_64;
            this.tsbOpen.ImageDpi96 = global::Chummer.Properties.Resources.folder_page_16;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(23, 22);
            this.tsbOpen.Tag = "Menu_Main_Open";
            this.tsbOpen.Text = "Open";
            this.tsbOpen.Click += new System.EventHandler(this.OpenFile);
            // 
            // tsbOpenForPrinting
            // 
            this.tsbOpenForPrinting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpenForPrinting.Image = global::Chummer.Properties.Resources.folder_print_16;
            this.tsbOpenForPrinting.ImageDpi120 = global::Chummer.Properties.Resources.folder_print_20;
            this.tsbOpenForPrinting.ImageDpi144 = global::Chummer.Properties.Resources.folder_print_24;
            this.tsbOpenForPrinting.ImageDpi192 = global::Chummer.Properties.Resources.folder_print_32;
            this.tsbOpenForPrinting.ImageDpi288 = global::Chummer.Properties.Resources.folder_print_48;
            this.tsbOpenForPrinting.ImageDpi384 = global::Chummer.Properties.Resources.folder_print_64;
            this.tsbOpenForPrinting.ImageDpi96 = global::Chummer.Properties.Resources.folder_print_16;
            this.tsbOpenForPrinting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenForPrinting.Name = "tsbOpenForPrinting";
            this.tsbOpenForPrinting.Size = new System.Drawing.Size(23, 22);
            this.tsbOpenForPrinting.Tag = "Menu_Main_OpenForPrinting";
            this.tsbOpenForPrinting.Text = "Open for P&rinting";
            this.tsbOpenForPrinting.Click += new System.EventHandler(this.OpenFileForPrinting);
            // 
            // tsbOpenForExport
            // 
            this.tsbOpenForExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpenForExport.Image = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsbOpenForExport.ImageDpi120 = global::Chummer.Properties.Resources.folder_script_go_20;
            this.tsbOpenForExport.ImageDpi144 = global::Chummer.Properties.Resources.folder_script_go_24;
            this.tsbOpenForExport.ImageDpi192 = global::Chummer.Properties.Resources.folder_script_go_32;
            this.tsbOpenForExport.ImageDpi288 = global::Chummer.Properties.Resources.folder_script_go_48;
            this.tsbOpenForExport.ImageDpi384 = global::Chummer.Properties.Resources.folder_script_go_64;
            this.tsbOpenForExport.ImageDpi96 = global::Chummer.Properties.Resources.folder_script_go_16;
            this.tsbOpenForExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenForExport.Name = "tsbOpenForExport";
            this.tsbOpenForExport.Size = new System.Drawing.Size(23, 22);
            this.tsbOpenForExport.Tag = "Menu_Main_OpenForExport";
            this.tsbOpenForExport.Text = "Open for E&xport";
            this.tsbOpenForExport.Click += new System.EventHandler(this.OpenFileForExport);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.DefaultColorScheme = true;
            this.toolStripSeparator1.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.toolStripSeparator1.MergeIndex = 5;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.DefaultColorScheme = true;
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
            this.tsSave.Image = global::Chummer.Properties.Resources.disk_16;
            this.tsSave.ImageDpi120 = global::Chummer.Properties.Resources.disk_20;
            this.tsSave.ImageDpi144 = global::Chummer.Properties.Resources.disk_24;
            this.tsSave.ImageDpi192 = global::Chummer.Properties.Resources.disk_32;
            this.tsSave.ImageDpi288 = global::Chummer.Properties.Resources.disk_48;
            this.tsSave.ImageDpi384 = global::Chummer.Properties.Resources.disk_64;
            this.tsSave.ImageDpi96 = global::Chummer.Properties.Resources.disk_16;
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
            this.tsSaveAs.Image = global::Chummer.Properties.Resources.disk_multiple_16;
            this.tsSaveAs.ImageDpi120 = global::Chummer.Properties.Resources.disk_multiple_20;
            this.tsSaveAs.ImageDpi144 = global::Chummer.Properties.Resources.disk_multiple_24;
            this.tsSaveAs.ImageDpi192 = global::Chummer.Properties.Resources.disk_multiple_32;
            this.tsSaveAs.ImageDpi288 = global::Chummer.Properties.Resources.disk_multiple_48;
            this.tsSaveAs.ImageDpi384 = global::Chummer.Properties.Resources.disk_multiple_64;
            this.tsSaveAs.ImageDpi96 = global::Chummer.Properties.Resources.disk_multiple_16;
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
            this.tsClose.Image = global::Chummer.Properties.Resources.cancel_16;
            this.tsClose.ImageDpi120 = global::Chummer.Properties.Resources.cancel_20;
            this.tsClose.ImageDpi144 = global::Chummer.Properties.Resources.cancel_24;
            this.tsClose.ImageDpi192 = global::Chummer.Properties.Resources.cancel_32;
            this.tsClose.ImageDpi288 = global::Chummer.Properties.Resources.cancel_48;
            this.tsClose.ImageDpi384 = global::Chummer.Properties.Resources.cancel_64;
            this.tsClose.ImageDpi96 = global::Chummer.Properties.Resources.cancel_16;
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
            this.tsPrint.Image = global::Chummer.Properties.Resources.printer_16;
            this.tsPrint.ImageDpi120 = global::Chummer.Properties.Resources.printer_20;
            this.tsPrint.ImageDpi144 = global::Chummer.Properties.Resources.printer_24;
            this.tsPrint.ImageDpi192 = global::Chummer.Properties.Resources.printer_32;
            this.tsPrint.ImageDpi288 = global::Chummer.Properties.Resources.printer_48;
            this.tsPrint.ImageDpi384 = global::Chummer.Properties.Resources.printer_64;
            this.tsPrint.ImageDpi96 = global::Chummer.Properties.Resources.printer_16;
            this.tsPrint.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.tsPrint.MergeIndex = 8;
            this.tsPrint.Name = "tsPrint";
            this.tsPrint.Size = new System.Drawing.Size(114, 22);
            this.tsPrint.Tag = "Menu_FilePrint";
            this.tsPrint.Text = "&Print";
            this.tsPrint.Click += new System.EventHandler(this.tsPrint_Click);
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.Multiselect = true;
            // 
            // ChummerMainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.tabForms);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "ChummerMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chummer5";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChummerMainForm_Closing);
            this.Load += new System.EventHandler(this.ChummerMainForm_Load);
            this.MdiChildActivate += new System.EventHandler(this.ChummerMainForm_MdiChildActivate);
            this.DpiChanged += new System.Windows.Forms.DpiChangedEventHandler(this.ChummerMainForm_DpiChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ChummerMainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ChummerMainForm_DragEnter);
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
        private Chummer.ColorableToolStripSeparator toolStripSeparator1;
        private Chummer.ColorableToolStripSeparator toolStripSeparator2;
        private Chummer.ColorableToolStripSeparator toolStripSeparator3;
        private Chummer.ColorableToolStripSeparator toolStripSeparator4;
        private Chummer.ColorableToolStripSeparator mnuMURSep;
        private Chummer.ColorableToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem windowsMenu;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private Chummer.ColorableToolStripSeparator mnuFileMRUSeparator;
        private System.Windows.Forms.TabControl tabForms;
        private Chummer.ColorableToolStripSeparator toolStripSeparator5;
        private Chummer.ColorableToolStripSeparator toolStripSeparator6;
        private Chummer.ColorableToolStripSeparator toolStripSeparator7;
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
        private DpiFriendlyToolStripMenuItem mnuGlobalSettings;
        private DpiFriendlyToolStripMenuItem mnuCharacterSettings;
        private DpiFriendlyToolStripMenuItem mnuToolsUpdate;
        private DpiFriendlyToolStripMenuItem mnuRestart;
        private DpiFriendlyToolStripMenuItem mnuToolsTranslator;
        private DpiFriendlyToolStripMenuItem mnuXmlEditor;
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
        private DpiFriendlyToolStripMenuItem mnuOpenForPrinting;
        private DpiFriendlyToolStripMenuItem mnuOpenForExport;
        private DpiFriendlyToolStripMenuItem mnuMasterIndex;
        private DpiFriendlyToolStripMenuItem mnuCharacterRoster;
        private OpenFileDialog dlgOpenFile;
        private DpiFriendlyToolStripMenuItem closeWindowToolStripMenuItem;
        private DpiFriendlyToolStripButton tsbOpenForPrinting;
        private DpiFriendlyToolStripButton tsbOpenForExport;
        private DpiFriendlyToolStripMenuItem mnuDataExporter;
        private DpiFriendlyToolStripMenuItem mnuReportBug;
    }
}



