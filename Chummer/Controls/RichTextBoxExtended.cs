using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
	#region StampActions

	public enum StampActions
	{
		EditedBy = 1,
		DateTime = 2,
		Custom = 4
	}

	#endregion StampActions

	/// <summary>
	/// An extended RichTextBox that contains a toolbar.
	/// </summary>
	public class RichTextBoxExtended : UserControl
	{
		//Used for looping
		private RichTextBox rtbTemp = new RichTextBox();

		#region Windows Generated

		private RichTextBox rtb1;
		private ImageList imgList1;
		private ContextMenu cmColors;
		private MenuItem miBlack;
		private MenuItem miBlue;
		private MenuItem miRed;
		private MenuItem miGreen;
		private OpenFileDialog ofd1;
		private SaveFileDialog sfd1;
		private ContextMenu cmFonts;
		private MenuItem miArial;
		private MenuItem miGaramond;
		private MenuItem miTahoma;
		private MenuItem miTimesNewRoman;
		private MenuItem miVerdana;
		private MenuItem miCourierNew;
		private MenuItem miMicrosoftSansSerif;
		private ContextMenu cmFontSizes;
		private MenuItem mi8;
		private MenuItem mi9;
		private MenuItem mi10;
		private MenuItem mi11;
		private MenuItem mi12;
		private MenuItem mi14;
		private MenuItem mi16;
		private MenuItem mi18;
		private MenuItem mi20;
		private MenuItem mi22;
		private MenuItem mi24;
		private MenuItem mi26;
		private MenuItem mi28;
		private MenuItem mi36;
		private MenuItem mi48;
		private MenuItem mi72;
		private ToolBarButton tbbStamp;
		private ToolBarButton tbbPaste;
		private ToolBarButton tbbCopy;
		private ToolBarButton tbbCut;
		private ToolBarButton tbbSeparator4;
		private ToolBarButton tbbRedo;
		private ToolBarButton tbbUndo;
		private ToolBarButton tbbSeparator2;
		private ToolBarButton tbbRight;
		private ToolBarButton tbbCenter;
		private ToolBarButton tbbLeft;
		private ToolBarButton tbbSeparator1;
		private ToolBarButton tbbStrikeout;
		private ToolBarButton tbbUnderline;
		private ToolBarButton tbbItalic;
		private ToolBarButton tbbBold;
		private ToolBarButton tbbSeparator5;
		private ToolBarButton tbbColor;
		private ToolBarButton tbbFontSize;
		private ToolBarButton tbbFont;
		private ToolBarButton tbbSeparator3;
		private ToolBarButton tbbSave;
		private ToolBar tb1;
		private IContainer components;

		public RichTextBoxExtended()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//Update the graphics on the toolbar
			UpdateToolbar();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion Windows Generated

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RichTextBoxExtended));
			this.cmFonts = new System.Windows.Forms.ContextMenu();
			this.miArial = new System.Windows.Forms.MenuItem();
			this.miCourierNew = new System.Windows.Forms.MenuItem();
			this.miGaramond = new System.Windows.Forms.MenuItem();
			this.miMicrosoftSansSerif = new System.Windows.Forms.MenuItem();
			this.miTahoma = new System.Windows.Forms.MenuItem();
			this.miTimesNewRoman = new System.Windows.Forms.MenuItem();
			this.miVerdana = new System.Windows.Forms.MenuItem();
			this.cmFontSizes = new System.Windows.Forms.ContextMenu();
			this.mi8 = new System.Windows.Forms.MenuItem();
			this.mi9 = new System.Windows.Forms.MenuItem();
			this.mi10 = new System.Windows.Forms.MenuItem();
			this.mi11 = new System.Windows.Forms.MenuItem();
			this.mi12 = new System.Windows.Forms.MenuItem();
			this.mi14 = new System.Windows.Forms.MenuItem();
			this.mi16 = new System.Windows.Forms.MenuItem();
			this.mi18 = new System.Windows.Forms.MenuItem();
			this.mi20 = new System.Windows.Forms.MenuItem();
			this.mi22 = new System.Windows.Forms.MenuItem();
			this.mi24 = new System.Windows.Forms.MenuItem();
			this.mi26 = new System.Windows.Forms.MenuItem();
			this.mi28 = new System.Windows.Forms.MenuItem();
			this.mi36 = new System.Windows.Forms.MenuItem();
			this.mi48 = new System.Windows.Forms.MenuItem();
			this.mi72 = new System.Windows.Forms.MenuItem();
			this.cmColors = new System.Windows.Forms.ContextMenu();
			this.miBlack = new System.Windows.Forms.MenuItem();
			this.miBlue = new System.Windows.Forms.MenuItem();
			this.miRed = new System.Windows.Forms.MenuItem();
			this.miGreen = new System.Windows.Forms.MenuItem();
			this.imgList1 = new System.Windows.Forms.ImageList(this.components);
			this.rtb1 = new System.Windows.Forms.RichTextBox();
			this.ofd1 = new System.Windows.Forms.OpenFileDialog();
			this.sfd1 = new System.Windows.Forms.SaveFileDialog();
			this.tbbStamp = new System.Windows.Forms.ToolBarButton();
			this.tbbPaste = new System.Windows.Forms.ToolBarButton();
			this.tbbCopy = new System.Windows.Forms.ToolBarButton();
			this.tbbCut = new System.Windows.Forms.ToolBarButton();
			this.tbbSeparator4 = new System.Windows.Forms.ToolBarButton();
			this.tbbRedo = new System.Windows.Forms.ToolBarButton();
			this.tbbUndo = new System.Windows.Forms.ToolBarButton();
			this.tbbSeparator2 = new System.Windows.Forms.ToolBarButton();
			this.tbbRight = new System.Windows.Forms.ToolBarButton();
			this.tbbCenter = new System.Windows.Forms.ToolBarButton();
			this.tbbLeft = new System.Windows.Forms.ToolBarButton();
			this.tbbSeparator1 = new System.Windows.Forms.ToolBarButton();
			this.tbbStrikeout = new System.Windows.Forms.ToolBarButton();
			this.tbbUnderline = new System.Windows.Forms.ToolBarButton();
			this.tbbItalic = new System.Windows.Forms.ToolBarButton();
			this.tbbBold = new System.Windows.Forms.ToolBarButton();
			this.tbbSeparator5 = new System.Windows.Forms.ToolBarButton();
			this.tbbColor = new System.Windows.Forms.ToolBarButton();
			this.tbbFontSize = new System.Windows.Forms.ToolBarButton();
			this.tbbFont = new System.Windows.Forms.ToolBarButton();
			this.tbbSeparator3 = new System.Windows.Forms.ToolBarButton();
			this.tbbSave = new System.Windows.Forms.ToolBarButton();
			this.tb1 = new System.Windows.Forms.ToolBar();
			this.SuspendLayout();
			//
			// cmFonts
			//
			this.cmFonts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.miArial,
			this.miCourierNew,
			this.miGaramond,
			this.miMicrosoftSansSerif,
			this.miTahoma,
			this.miTimesNewRoman,
			this.miVerdana});
			//
			// miArial
			//
			this.miArial.Index = 0;
			this.miArial.Text = "Arial";
			this.miArial.Click += new System.EventHandler(this.Font_Click);
			//
			// miCourierNew
			//
			this.miCourierNew.Index = 1;
			this.miCourierNew.Text = "Courier New";
			this.miCourierNew.Click += new System.EventHandler(this.Font_Click);
			//
			// miGaramond
			//
			this.miGaramond.Index = 2;
			this.miGaramond.Text = "Garamond";
			this.miGaramond.Click += new System.EventHandler(this.Font_Click);
			//
			// miMicrosoftSansSerif
			//
			this.miMicrosoftSansSerif.Index = 3;
			this.miMicrosoftSansSerif.Text = "Microsoft Sans Serif";
			this.miMicrosoftSansSerif.Click += new System.EventHandler(this.Font_Click);
			//
			// miTahoma
			//
			this.miTahoma.Index = 4;
			this.miTahoma.Text = "Tahoma";
			this.miTahoma.Click += new System.EventHandler(this.Font_Click);
			//
			// miTimesNewRoman
			//
			this.miTimesNewRoman.Index = 5;
			this.miTimesNewRoman.Text = "Times New Roman";
			this.miTimesNewRoman.Click += new System.EventHandler(this.Font_Click);
			//
			// miVerdana
			//
			this.miVerdana.Index = 6;
			this.miVerdana.Text = "Verdana";
			this.miVerdana.Click += new System.EventHandler(this.Font_Click);
			//
			// cmFontSizes
			//
			this.cmFontSizes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.mi8,
			this.mi9,
			this.mi10,
			this.mi11,
			this.mi12,
			this.mi14,
			this.mi16,
			this.mi18,
			this.mi20,
			this.mi22,
			this.mi24,
			this.mi26,
			this.mi28,
			this.mi36,
			this.mi48,
			this.mi72});
			//
			// mi8
			//
			this.mi8.Index = 0;
			this.mi8.Text = "8";
			this.mi8.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi9
			//
			this.mi9.Index = 1;
			this.mi9.Text = "9";
			this.mi9.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi10
			//
			this.mi10.Index = 2;
			this.mi10.Text = "10";
			this.mi10.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi11
			//
			this.mi11.Index = 3;
			this.mi11.Text = "11";
			this.mi11.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi12
			//
			this.mi12.Index = 4;
			this.mi12.Text = "12";
			this.mi12.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi14
			//
			this.mi14.Index = 5;
			this.mi14.Text = "14";
			this.mi14.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi16
			//
			this.mi16.Index = 6;
			this.mi16.Text = "16";
			this.mi16.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi18
			//
			this.mi18.Index = 7;
			this.mi18.Text = "18";
			this.mi18.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi20
			//
			this.mi20.Index = 8;
			this.mi20.Text = "20";
			this.mi20.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi22
			//
			this.mi22.Index = 9;
			this.mi22.Text = "22";
			this.mi22.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi24
			//
			this.mi24.Index = 10;
			this.mi24.Text = "24";
			this.mi24.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi26
			//
			this.mi26.Index = 11;
			this.mi26.Text = "26";
			this.mi26.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi28
			//
			this.mi28.Index = 12;
			this.mi28.Text = "28";
			this.mi28.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi36
			//
			this.mi36.Index = 13;
			this.mi36.Text = "36";
			this.mi36.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi48
			//
			this.mi48.Index = 14;
			this.mi48.Text = "48";
			this.mi48.Click += new System.EventHandler(this.FontSize_Click);
			//
			// mi72
			//
			this.mi72.Index = 15;
			this.mi72.Text = "72";
			this.mi72.Click += new System.EventHandler(this.FontSize_Click);
			//
			// cmColors
			//
			this.cmColors.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.miBlack,
			this.miBlue,
			this.miRed,
			this.miGreen});
			//
			// miBlack
			//
			this.miBlack.Index = 0;
			this.miBlack.Text = "Black";
			this.miBlack.Click += new System.EventHandler(this.Color_Click);
			//
			// miBlue
			//
			this.miBlue.Index = 1;
			this.miBlue.Text = "Blue";
			this.miBlue.Click += new System.EventHandler(this.Color_Click);
			//
			// miRed
			//
			this.miRed.Index = 2;
			this.miRed.Text = "Red";
			this.miRed.Click += new System.EventHandler(this.Color_Click);
			//
			// miGreen
			//
			this.miGreen.Index = 3;
			this.miGreen.Text = "Green";
			this.miGreen.Click += new System.EventHandler(this.Color_Click);
			//
			// imgList1
			//
			this.imgList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList1.ImageStream")));
			this.imgList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imgList1.Images.SetKeyName(0, "");
			this.imgList1.Images.SetKeyName(1, "");
			this.imgList1.Images.SetKeyName(2, "");
			this.imgList1.Images.SetKeyName(3, "");
			this.imgList1.Images.SetKeyName(4, "");
			this.imgList1.Images.SetKeyName(5, "");
			this.imgList1.Images.SetKeyName(6, "");
			this.imgList1.Images.SetKeyName(7, "");
			this.imgList1.Images.SetKeyName(8, "");
			this.imgList1.Images.SetKeyName(9, "");
			this.imgList1.Images.SetKeyName(10, "");
			this.imgList1.Images.SetKeyName(11, "");
			this.imgList1.Images.SetKeyName(12, "");
			this.imgList1.Images.SetKeyName(13, "");
			this.imgList1.Images.SetKeyName(14, "");
			this.imgList1.Images.SetKeyName(15, "");
			this.imgList1.Images.SetKeyName(16, "");
			this.imgList1.Images.SetKeyName(17, "");
			this.imgList1.Images.SetKeyName(18, "");
			this.imgList1.Images.SetKeyName(19, "");
			this.imgList1.Images.SetKeyName(20, "");
			//
			// rtb1
			//
			this.rtb1.AutoWordSelection = true;
			this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtb1.Location = new System.Drawing.Point(0, 26);
			this.rtb1.Name = "rtb1";
			this.rtb1.Size = new System.Drawing.Size(504, 198);
			this.rtb1.TabIndex = 1;
			this.rtb1.Text = "";
			this.rtb1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtb1_LinkClicked);
			this.rtb1.SelectionChanged += new System.EventHandler(this.rtb1_SelectionChanged);
			this.rtb1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtb1_KeyDown);
			this.rtb1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.rtb1_KeyPress);
			//
			// ofd1
			//
			this.ofd1.DefaultExt = "rtf";
			this.ofd1.Filter = "Rich Text Files|*.rtf|Plain Text File|*.txt";
			this.ofd1.Title = "Open File";
			//
			// sfd1
			//
			this.sfd1.DefaultExt = "rtf";
			this.sfd1.Filter = "Rich Text File|*.rtf|Plain Text File|*.txt";
			this.sfd1.Title = "Save As";
			//
			// tbbStamp
			//
			this.tbbStamp.ImageIndex = 8;
			this.tbbStamp.Name = "tbbStamp";
			this.tbbStamp.Tag = "edit stamp";
			//
			// tbbPaste
			//
			this.tbbPaste.ImageIndex = 19;
			this.tbbPaste.Name = "tbbPaste";
			this.tbbPaste.Tag = "paste";
			//
			// tbbCopy
			//
			this.tbbCopy.ImageIndex = 18;
			this.tbbCopy.Name = "tbbCopy";
			this.tbbCopy.Tag = "copy";
			//
			// tbbCut
			//
			this.tbbCut.ImageIndex = 17;
			this.tbbCut.Name = "tbbCut";
			this.tbbCut.Tag = "cut";
			//
			// tbbSeparator4
			//
			this.tbbSeparator4.Name = "tbbSeparator4";
			this.tbbSeparator4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			//
			// tbbRedo
			//
			this.tbbRedo.ImageIndex = 13;
			this.tbbRedo.Name = "tbbRedo";
			this.tbbRedo.Tag = "redo";
			//
			// tbbUndo
			//
			this.tbbUndo.ImageIndex = 12;
			this.tbbUndo.Name = "tbbUndo";
			this.tbbUndo.Tag = "undo";
			//
			// tbbSeparator2
			//
			this.tbbSeparator2.Name = "tbbSeparator2";
			this.tbbSeparator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			//
			// tbbRight
			//
			this.tbbRight.ImageIndex = 6;
			this.tbbRight.Name = "tbbRight";
			this.tbbRight.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbRight.Tag = "right";
			//
			// tbbCenter
			//
			this.tbbCenter.ImageIndex = 5;
			this.tbbCenter.Name = "tbbCenter";
			this.tbbCenter.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbCenter.Tag = "center";
			//
			// tbbLeft
			//
			this.tbbLeft.ImageIndex = 4;
			this.tbbLeft.Name = "tbbLeft";
			this.tbbLeft.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbLeft.Tag = "left";
			//
			// tbbSeparator1
			//
			this.tbbSeparator1.Name = "tbbSeparator1";
			this.tbbSeparator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			//
			// tbbStrikeout
			//
			this.tbbStrikeout.ImageIndex = 3;
			this.tbbStrikeout.Name = "tbbStrikeout";
			this.tbbStrikeout.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbStrikeout.Tag = "strikeout";
			//
			// tbbUnderline
			//
			this.tbbUnderline.ImageIndex = 2;
			this.tbbUnderline.Name = "tbbUnderline";
			this.tbbUnderline.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbUnderline.Tag = "underline";
			//
			// tbbItalic
			//
			this.tbbItalic.ImageIndex = 1;
			this.tbbItalic.Name = "tbbItalic";
			this.tbbItalic.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbItalic.Tag = "italic";
			//
			// tbbBold
			//
			this.tbbBold.ImageIndex = 0;
			this.tbbBold.Name = "tbbBold";
			this.tbbBold.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this.tbbBold.Tag = "bold";
			//
			// tbbSeparator5
			//
			this.tbbSeparator5.Name = "tbbSeparator5";
			this.tbbSeparator5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			//
			// tbbColor
			//
			this.tbbColor.DropDownMenu = this.cmColors;
			this.tbbColor.ImageIndex = 7;
			this.tbbColor.Name = "tbbColor";
			this.tbbColor.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbColor.Tag = "color";
			//
			// tbbFontSize
			//
			this.tbbFontSize.DropDownMenu = this.cmFontSizes;
			this.tbbFontSize.ImageIndex = 15;
			this.tbbFontSize.Name = "tbbFontSize";
			this.tbbFontSize.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbFontSize.Tag = "font size";
			//
			// tbbFont
			//
			this.tbbFont.DropDownMenu = this.cmFonts;
			this.tbbFont.ImageIndex = 14;
			this.tbbFont.Name = "tbbFont";
			this.tbbFont.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbFont.Tag = "font";
			//
			// tbbSeparator3
			//
			this.tbbSeparator3.Name = "tbbSeparator3";
			this.tbbSeparator3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			//
			// tbbSave
			//
			this.tbbSave.ImageIndex = 11;
			this.tbbSave.Name = "tbbSave";
			this.tbbSave.Tag = "save";
			//
			// tb1
			//
			this.tb1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tb1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
			this.tbbSave,
			this.tbbSeparator3,
			this.tbbFont,
			this.tbbFontSize,
			this.tbbColor,
			this.tbbSeparator5,
			this.tbbBold,
			this.tbbItalic,
			this.tbbUnderline,
			this.tbbStrikeout,
			this.tbbSeparator1,
			this.tbbLeft,
			this.tbbCenter,
			this.tbbRight,
			this.tbbSeparator2,
			this.tbbUndo,
			this.tbbRedo,
			this.tbbSeparator4,
			this.tbbCut,
			this.tbbCopy,
			this.tbbPaste,
			this.tbbStamp});
			this.tb1.ButtonSize = new System.Drawing.Size(16, 16);
			this.tb1.Divider = false;
			this.tb1.DropDownArrows = true;
			this.tb1.ImageList = this.imgList1;
			this.tb1.Location = new System.Drawing.Point(0, 0);
			this.tb1.Name = "tb1";
			this.tb1.ShowToolTips = true;
			this.tb1.Size = new System.Drawing.Size(504, 26);
			this.tb1.TabIndex = 0;
			this.tb1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tb1_ButtonClick);
			//
			// RichTextBoxExtended
			//
			this.Controls.Add(this.rtb1);
			this.Controls.Add(this.tb1);
			this.Name = "RichTextBoxExtended";
			this.Size = new System.Drawing.Size(504, 224);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion Component Designer generated code

		#region Selection Change event

		[Description("Occurs when the selection is changed"),
		Category("Behavior")]
		// Raised in tb1 SelectionChanged event so that user can do useful things
		public event EventHandler SelChanged;

		#endregion Selection Change event

		#region Stamp Event Stuff

		[Description("Occurs when the stamp button is clicked"),
		 Category("Behavior")]
		public event EventHandler Stamp;

		/// <summary>
		/// OnStamp event
		/// </summary>
		protected virtual void OnStamp(EventArgs e)
		{
			Stamp?.Invoke(this, e);

			switch (StampAction)
			{
				case StampActions.EditedBy:
					{
						StringBuilder stamp = new StringBuilder(""); //holds our stamp text
						if (rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
						stamp.Append("Edited by ");
						//use the CurrentPrincipal name if one exsist else use windows logon username
						if (Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null || Thread.CurrentPrincipal.Identity.Name.Length <= 0)
							stamp.Append(Environment.UserName);
						else
							stamp.Append(Thread.CurrentPrincipal.Identity.Name);
						stamp.Append(" on " + DateTime.Now.ToLongDateString() + "\r\n");

						rtb1.SelectionLength = 0; //unselect everything basicly
						rtb1.SelectionStart = rtb1.Text.Length; //start new selection at the end of the text
						rtb1.SelectionColor = StampColor; //make the selection blue
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
						rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
						rtb1.Focus(); //set focus back on the richtextbox
					}
					break; //end edited by stamp
				case StampActions.DateTime:
					{
						StringBuilder stamp = new StringBuilder(""); //holds our stamp text
						if (rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
						stamp.Append(DateTime.Now.ToLongDateString() + "\r\n");
						rtb1.SelectionLength = 0; //unselect everything basicly
						rtb1.SelectionStart = rtb1.Text.Length; //start new selection at the end of the text
						rtb1.SelectionColor = StampColor; //make the selection blue
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
						rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
						rtb1.Focus(); //set focus back on the richtextbox
					}
					break;
			} //end select
		}

		#endregion Stamp Event Stuff

		#region Toolbar button click

		/// <summary>
		///     Handler for the toolbar button click event
		/// </summary>
		private void tb1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			// true if style to be added
			// false to remove style
			bool add = e.Button.Pushed;

			//Switch based on the tag of the button pressed
			switch (e.Button.Tag.ToString().ToLower())
			{
				case "bold":
					ChangeFontStyle(FontStyle.Bold, add);
					break;

				case "italic":
					ChangeFontStyle(FontStyle.Italic, add);
					break;

				case "underline":
					ChangeFontStyle(FontStyle.Underline, add);
					break;

				case "strikeout":
					ChangeFontStyle(FontStyle.Strikeout, add);
					break;

				case "left":
					//change horizontal alignment to left
					rtb1.SelectionAlignment = HorizontalAlignment.Left;
					tbbCenter.Pushed = false;
					tbbRight.Pushed = false;
					break;

				case "center":
					//change horizontal alignment to center
					tbbLeft.Pushed = false;
					rtb1.SelectionAlignment = HorizontalAlignment.Center;
					tbbRight.Pushed = false;
					break;

				case "right":
					//change horizontal alignment to right
					tbbLeft.Pushed = false;
					tbbCenter.Pushed = false;
					rtb1.SelectionAlignment = HorizontalAlignment.Right;
					break;

				case "edit stamp":
					OnStamp(new EventArgs()); //send stamp event
					break;

				case "color":
					rtb1.SelectionColor = Color.Black;
					break;

				case "undo":
					rtb1.Undo();
					break;

				case "redo":
					rtb1.Redo();
					break;

				case "save":
					if (sfd1.ShowDialog() == DialogResult.OK && sfd1.FileName.Length > 0)
						if (Path.GetExtension(sfd1.FileName).ToLower().Equals(".rtf"))
							rtb1.SaveFile(sfd1.FileName);
						else
							rtb1.SaveFile(sfd1.FileName, RichTextBoxStreamType.PlainText);
					break;

				case "cut":
					{
						if (rtb1.SelectedText.Length <= 0) break;
						rtb1.Cut();
						break;
					}
				case "copy":
					{
						if (rtb1.SelectedText.Length <= 0) break;
						rtb1.Copy();
						break;
					}
				case "paste":
					{
						try
						{
							rtb1.Paste();
						}
						catch
						{
							MessageBox.Show("Paste Failed");
						}
						break;
					}
			} //end select
		}

		#endregion Toolbar button click

		#region Update Toolbar

		/// <summary>
		///     Update the toolbar button statuses
		/// </summary>
		public void UpdateToolbar()
		{
			// Get the font, fontsize and style to apply to the toolbar buttons
			Font fnt = GetFontDetails();
			// Set font style buttons to the styles applying to the entire selection
			FontStyle style = fnt.Style;

			//Set all the style buttons using the gathered style
			tbbBold.Pushed = fnt.Bold; //bold button
			tbbItalic.Pushed = fnt.Italic; //italic button
			tbbUnderline.Pushed = fnt.Underline; //underline button
			tbbStrikeout.Pushed = fnt.Strikeout; //strikeout button
			tbbLeft.Pushed = (rtb1.SelectionAlignment == HorizontalAlignment.Left); //justify left
			tbbCenter.Pushed = (rtb1.SelectionAlignment == HorizontalAlignment.Center); //justify center
			tbbRight.Pushed = (rtb1.SelectionAlignment == HorizontalAlignment.Right); //justify right

			//Check the correct color
			foreach (MenuItem mi in cmColors.MenuItems)
				mi.Checked = (rtb1.SelectionColor == Color.FromName(mi.Text));

			//Check the correct font
			foreach (MenuItem mi in cmFonts.MenuItems)
				mi.Checked = (fnt.FontFamily.Name == mi.Text);

			//Check the correct font size
			foreach (MenuItem mi in cmFontSizes.MenuItems)
				mi.Checked = ((int)fnt.SizeInPoints == float.Parse(mi.Text));
		}

		#endregion Update Toolbar

		#region Update Toolbar Seperators

		private void UpdateToolbarSeperators()
		{
			//Save & Open
			tbbSeparator3.Visible = tbbSave.Visible;

			//Font & Font Size
			if (!tbbFont.Visible && !tbbFontSize.Visible && !tbbColor.Visible)
				tbbSeparator5.Visible = false;
			else
				tbbSeparator5.Visible = true;

			//Bold, Italic, Underline, & Strikeout
			if (!tbbBold.Visible && !tbbItalic.Visible && !tbbUnderline.Visible && !tbbStrikeout.Visible)
				tbbSeparator1.Visible = false;
			else
				tbbSeparator1.Visible = true;

			//Left, Center, & Right
			if (!tbbLeft.Visible && !tbbCenter.Visible && !tbbRight.Visible)
				tbbSeparator2.Visible = false;
			else
				tbbSeparator2.Visible = true;

			//Undo & Redo
			if (!tbbUndo.Visible && !tbbRedo.Visible)
				tbbSeparator4.Visible = false;
			else
				tbbSeparator4.Visible = true;
		}

		#endregion Update Toolbar Seperators

		#region RichTextBox Selection Change

		/// <summary>
		///		Change the toolbar buttons when new text is selected
		///		and raise event SelChanged
		/// </summary>
		private void rtb1_SelectionChanged(object sender, EventArgs e)
		{
			//Update the toolbar buttons
			UpdateToolbar();

			//Send the SelChangedEvent
			if (SelChanged != null)
				SelChanged(this, e);
		}

		#endregion RichTextBox Selection Change

		#region Color Click

		/// <summary>
		///     Change the richtextbox color
		/// </summary>
		private void Color_Click(object sender, EventArgs e)
		{
			//set the richtextbox color based on the name of the menu item
			ChangeFontColor(Color.FromName(((MenuItem)sender).Text));
		}

		#endregion Color Click

		#region Font Click

		/// <summary>
		///     Change the richtextbox font
		/// </summary>
		private void Font_Click(object sender, EventArgs e)
		{
			// Set the font for the entire selection
			ChangeFont(((MenuItem)sender).Text);
		}

		#endregion Font Click

		#region Font Size Click

		/// <summary>
		///     Change the richtextbox font size
		/// </summary>
		private void FontSize_Click(object sender, EventArgs e)
		{
			//set the richtextbox font size based on the name of the menu item
			ChangeFontSize(float.Parse(((MenuItem)sender).Text));
		}

		#endregion Font Size Click

		#region Link Clicked

		/// <summary>
		/// Starts the default browser if a link is clicked
		/// </summary>
		private void rtb1_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		#endregion Link Clicked

		#region Public Properties

		/// <summary>
		///     The toolbar that is contained with-in the RichTextBoxExtened control
		/// </summary>
		[Description("The internal toolbar control"),
		Category("Internal Controls"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ToolBar Toolbar
		{
			get { return tb1; }
		}

		/// <summary>
		///     The RichTextBox that is contained with-in the RichTextBoxExtened control
		/// </summary>
		[Description("The internal richtextbox control"),
		Category("Internal Controls"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RichTextBox RichTextBox
		{
			get { return rtb1; }
		}

		/// <summary>
		///     Show the save button or not
		/// </summary>
		[Description("Show the save button or not"),
		Category("Appearance")]
		public bool ShowSave
		{
			get { return tbbSave.Visible; }
			set { tbbSave.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the stamp button or not
		/// </summary>
		[Description("Show the stamp button or not"),
		 Category("Appearance")]
		public bool ShowStamp
		{
			get { return tbbStamp.Visible; }
			set { tbbStamp.Visible = value; }
		}

		/// <summary>
		///     Show the color button or not
		/// </summary>
		[Description("Show the color button or not"),
		Category("Appearance")]
		public bool ShowColors
		{
			get { return tbbColor.Visible; }
			set { tbbColor.Visible = value; }
		}

		/// <summary>
		///     Show the undo button or not
		/// </summary>
		[Description("Show the undo button or not"),
		Category("Appearance")]
		public bool ShowUndo
		{
			get { return tbbUndo.Visible; }
			set { tbbUndo.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the redo button or not
		/// </summary>
		[Description("Show the redo button or not"),
		Category("Appearance")]
		public bool ShowRedo
		{
			get { return tbbRedo.Visible; }
			set { tbbRedo.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the bold button or not
		/// </summary>
		[Description("Show the bold button or not"),
		Category("Appearance")]
		public bool ShowBold
		{
			get { return tbbBold.Visible; }
			set { tbbBold.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the italic button or not
		/// </summary>
		[Description("Show the italic button or not"),
		Category("Appearance")]
		public bool ShowItalic
		{
			get { return tbbItalic.Visible; }
			set { tbbItalic.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the underline button or not
		/// </summary>
		[Description("Show the underline button or not"),
		Category("Appearance")]
		public bool ShowUnderline
		{
			get { return tbbUnderline.Visible; }
			set { tbbUnderline.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the strikeout button or not
		/// </summary>
		[Description("Show the strikeout button or not"),
		Category("Appearance")]
		public bool ShowStrikeout
		{
			get { return tbbStrikeout.Visible; }
			set { tbbStrikeout.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the left justify button or not
		/// </summary>
		[Description("Show the left justify button or not"),
		Category("Appearance")]
		public bool ShowLeftJustify
		{
			get { return tbbLeft.Visible; }
			set { tbbLeft.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the right justify button or not
		/// </summary>
		[Description("Show the right justify button or not"),
		Category("Appearance")]
		public bool ShowRightJustify
		{
			get { return tbbRight.Visible; }
			set { tbbRight.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the center justify button or not
		/// </summary>
		[Description("Show the center justify button or not"),
		Category("Appearance")]
		public bool ShowCenterJustify
		{
			get { return tbbCenter.Visible; }
			set { tbbCenter.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Determines how the stamp button will respond
		/// </summary>
		private StampActions m_StampAction = StampActions.EditedBy;

		[Description("Determines how the stamp button will respond"),
		Category("Behavior")]
		public StampActions StampAction
		{
			get { return m_StampAction; }
			set { m_StampAction = value; }
		}

		/// <summary>
		///     Color of the stamp text
		/// </summary>
		private Color m_StampColor = Color.Blue;

		[Description("Color of the stamp text"),
		Category("Appearance")]
		public Color StampColor
		{
			get { return m_StampColor; }
			set { m_StampColor = value; }
		}

		/// <summary>
		///     Show the font button or not
		/// </summary>
		[Description("Show the font button or not"),
		Category("Appearance")]
		public bool ShowFont
		{
			get { return tbbFont.Visible; }
			set { tbbFont.Visible = value; }
		}

		/// <summary>
		///     Show the font size button or not
		/// </summary>
		[Description("Show the font size button or not"),
		Category("Appearance")]
		public bool ShowFontSize
		{
			get { return tbbFontSize.Visible; }
			set { tbbFontSize.Visible = value; }
		}

		/// <summary>
		///     Show the cut button or not
		/// </summary>
		[Description("Show the cut button or not"),
		Category("Appearance")]
		public bool ShowCut
		{
			get { return tbbCut.Visible; }
			set { tbbCut.Visible = value; }
		}

		/// <summary>
		///     Show the copy button or not
		/// </summary>
		[Description("Show the copy button or not"),
		Category("Appearance")]
		public bool ShowCopy
		{
			get { return tbbCopy.Visible; }
			set { tbbCopy.Visible = value; }
		}

		/// <summary>
		///     Show the paste button or not
		/// </summary>
		[Description("Show the paste button or not"),
		Category("Appearance")]
		public bool ShowPaste
		{
			get { return tbbPaste.Visible; }
			set { tbbPaste.Visible = value; }
		}

		/// <summary>
		///     Detect URLs with-in the richtextbox
		/// </summary>
		[Description("Detect URLs with-in the richtextbox"),
		Category("Behavior")]
		public bool DetectURLs
		{
			get { return rtb1.DetectUrls; }
			set { rtb1.DetectUrls = value; }
		}

		/// <summary>
		/// Determines if the TAB key moves to the next control or enters a TAB character in the richtextbox
		/// </summary>
		[Description("Determines if the TAB key moves to the next control or enters a TAB character in the richtextbox"),
		Category("Behavior")]
		public bool AcceptsTab
		{
			get { return rtb1.AcceptsTab; }
			set { rtb1.AcceptsTab = value; }
		}

		/// <summary>
		/// Determines if auto word selection is enabled
		/// </summary>
		[Description("Determines if auto word selection is enabled"),
		Category("Behavior")]
		public bool AutoWordSelection
		{
			get { return rtb1.AutoWordSelection; }
			set { rtb1.AutoWordSelection = value; }
		}

		/// <summary>
		/// Determines if this control can be edited
		/// </summary>
		[Description("Determines if this control can be edited"),
		Category("Behavior")]
		public bool ReadOnly
		{
			get { return rtb1.ReadOnly; }
			set
			{
				tb1.Visible = !value;
				rtb1.ReadOnly = value;
			}
		}

		private bool _showToolBarText;

		/// <summary>
		/// Determines if the buttons on the toolbar will show there text or not
		/// </summary>
		[Description("Determines if the buttons on the toolbar will show there text or not"),
		Category("Behavior")]
		public bool ShowToolBarText
		{
			get { return _showToolBarText; }
			set
			{
				_showToolBarText = value;

				if (_showToolBarText)
				{
					tbbSave.Text = "Save";
					tbbBold.Text = "Bold";
					tbbFont.Text = "Font";
					tbbFontSize.Text = "Font Size";
					tbbColor.Text = "Font Color";
					tbbItalic.Text = "Italic";
					tbbStrikeout.Text = "Strikeout";
					tbbUnderline.Text = "Underline";
					tbbLeft.Text = "Left";
					tbbCenter.Text = "Center";
					tbbRight.Text = "Right";
					tbbUndo.Text = "Undo";
					tbbRedo.Text = "Redo";
					tbbCut.Text = "Cut";
					tbbCopy.Text = "Copy";
					tbbPaste.Text = "Paste";
					tbbStamp.Text = "Stamp";
				}
				else
				{
					tbbSave.Text = "";
					tbbBold.Text = "";
					tbbFont.Text = "";
					tbbFontSize.Text = "";
					tbbColor.Text = "";
					tbbItalic.Text = "";
					tbbStrikeout.Text = "";
					tbbUnderline.Text = "";
					tbbLeft.Text = "";
					tbbCenter.Text = "";
					tbbRight.Text = "";
					tbbUndo.Text = "";
					tbbRedo.Text = "";
					tbbCut.Text = "";
					tbbCopy.Text = "";
					tbbPaste.Text = "";
					tbbStamp.Text = "";
				}

				Invalidate();
				Update();
			}
		}

		#endregion Public Properties

		#region Change font

		/// <summary>
		///     Change the richtextbox font for the current selection
		/// </summary>
		public void ChangeFont(string fontFamily)
		{
			//This method should handle cases that occur when multiple fonts/styles are selected
			// Parameters:-
			// fontFamily - the font to be applied, eg "Courier New"

			// Reason: The reason this method and the others exist is because
			// setting these items via the selection font doen't work because
			// a null selection font is returned for a selection with more
			// than one font!

			int rtb1start = rtb1.SelectionStart;
			int len = rtb1.SelectionLength;
			int rtbTempStart = 0;

			// If len <= 1 and there is a selection font, amend and return
			if (len <= 1 && rtb1.SelectionFont != null)
			{
				rtb1.SelectionFont =
					new Font(fontFamily, rtb1.SelectionFont.Size, rtb1.SelectionFont.Style);
				return;
			}

			// Step through the selected text one char at a time
			rtbTemp.Rtf = rtb1.SelectedRtf;
			for (int i = 0; i < len; ++i)
			{
				rtbTemp.Select(rtbTempStart + i, 1);
				rtbTemp.SelectionFont = new Font(fontFamily, rtbTemp.SelectionFont.Size, rtbTemp.SelectionFont.Style);
			}

			// Replace & reselect
			rtbTemp.Select(rtbTempStart, len);
			rtb1.SelectedRtf = rtbTemp.SelectedRtf;
			rtb1.Select(rtb1start, len);
		}

		#endregion Change font

		#region Change font style

		/// <summary>
		///     Change the richtextbox style for the current selection
		/// </summary>
		public void ChangeFontStyle(FontStyle style, bool add)
		{
			//This method should handle cases that occur when multiple fonts/styles are selected
			// Parameters:-
			//	style - eg FontStyle.Bold
			//	add - IF true then add else remove

			// throw error if style isn't: bold, italic, strikeout or underline
			if (style != FontStyle.Bold
				&& style != FontStyle.Italic
				&& style != FontStyle.Strikeout
				&& style != FontStyle.Underline)
				throw new InvalidProgramException("Invalid style parameter to ChangeFontStyle");

			int rtb1start = rtb1.SelectionStart;
			int len = rtb1.SelectionLength;
			int rtbTempStart = 0;

			//if len <= 1 and there is a selection font then just handle and return
			if (len <= 1 && rtb1.SelectionFont != null)
			{
				//add or remove style
				if (add)
					rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style | style);
				else
					rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style & ~style);

				return;
			}

			// Step through the selected text one char at a time
			rtbTemp.Rtf = rtb1.SelectedRtf;
			for (int i = 0; i < len; ++i)
			{
				rtbTemp.Select(rtbTempStart + i, 1);

				//add or remove style
				if (add)
					rtbTemp.SelectionFont = new Font(rtbTemp.SelectionFont, rtbTemp.SelectionFont.Style | style);
				else
					rtbTemp.SelectionFont = new Font(rtbTemp.SelectionFont, rtbTemp.SelectionFont.Style & ~style);
			}

			// Replace & reselect
			rtbTemp.Select(rtbTempStart, len);
			rtb1.SelectedRtf = rtbTemp.SelectedRtf;
			rtb1.Select(rtb1start, len);
		}

		#endregion Change font style

		#region Change font size

		/// <summary>
		///     Change the richtextbox font size for the current selection
		/// </summary>
		public void ChangeFontSize(float fontSize)
		{
			//This method should handle cases that occur when multiple fonts/styles are selected
			// Parameters:-
			// fontSize - the fontsize to be applied, eg 33.5

			if (fontSize <= 0.0)
				throw new InvalidProgramException("Invalid font size parameter to ChangeFontSize");

			int rtb1start = rtb1.SelectionStart;
			int len = rtb1.SelectionLength;
			int rtbTempStart = 0;

			// If len <= 1 and there is a selection font, amend and return
			if (len <= 1 && rtb1.SelectionFont != null)
			{
				rtb1.SelectionFont =
					new Font(rtb1.SelectionFont.FontFamily, fontSize, rtb1.SelectionFont.Style);
				return;
			}

			// Step through the selected text one char at a time
			rtbTemp.Rtf = rtb1.SelectedRtf;
			for (int i = 0; i < len; ++i)
			{
				rtbTemp.Select(rtbTempStart + i, 1);
				rtbTemp.SelectionFont = new Font(rtbTemp.SelectionFont.FontFamily, fontSize, rtbTemp.SelectionFont.Style);
			}

			// Replace & reselect
			rtbTemp.Select(rtbTempStart, len);
			rtb1.SelectedRtf = rtbTemp.SelectedRtf;
			rtb1.Select(rtb1start, len);
		}

		#endregion Change font size

		#region Change font color

		/// <summary>
		///     Change the richtextbox font color for the current selection
		/// </summary>
		public void ChangeFontColor(Color newColor)
		{
			//This method should handle cases that occur when multiple fonts/styles are selected
			// Parameters:-
			//	newColor - eg Color.Red

			int rtb1start = rtb1.SelectionStart;
			int len = rtb1.SelectionLength;
			int rtbTempStart = 0;

			//if len <= 1 and there is a selection font then just handle and return
			if (len <= 1 && rtb1.SelectionFont != null)
			{
				rtb1.SelectionColor = newColor;
				return;
			}

			// Step through the selected text one char at a time
			rtbTemp.Rtf = rtb1.SelectedRtf;
			for (int i = 0; i < len; ++i)
			{
				rtbTemp.Select(rtbTempStart + i, 1);

				//change color
				rtbTemp.SelectionColor = newColor;
			}

			// Replace & reselect
			rtbTemp.Select(rtbTempStart, len);
			rtb1.SelectedRtf = rtbTemp.SelectedRtf;
			rtb1.Select(rtb1start, len);
		}

		#endregion Change font color

		#region Get Font Details

		/// <summary>
		///     Returns a Font with:
		///     1) The font applying to the entire selection, if none is the default font.
		///     2) The font size applying to the entire selection, if none is the size of the default font.
		///     3) A style containing the attributes that are common to the entire selection, default regular.
		/// </summary>
		///
		public Font GetFontDetails()
		{
			//This method should handle cases that occur when multiple fonts/styles are selected

			int rtb1start = rtb1.SelectionStart;
			int len = rtb1.SelectionLength;
			int rtbTempStart = 0;

			if (len <= 1)
			{
				// Return the selection or default font
				if (rtb1.SelectionFont != null)
					return rtb1.SelectionFont;
				return rtb1.Font;
			}

			// Step through the selected text one char at a time
			// after setting defaults from first char
			rtbTemp.Rtf = rtb1.SelectedRtf;

			//Turn everything on so we can turn it off one by one
			FontStyle replystyle =
				FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout | FontStyle.Underline;

			// Set reply font, size and style to that of first char in selection.
			rtbTemp.Select(rtbTempStart, 1);
			string replyfont = rtbTemp.SelectionFont.Name;
			float replyfontsize = rtbTemp.SelectionFont.Size;
			replystyle = replystyle & rtbTemp.SelectionFont.Style;

			// Search the rest of the selection
			for (int i = 1; i < len; ++i)
			{
				rtbTemp.Select(rtbTempStart + i, 1);

				// Check reply for different style
				replystyle = replystyle & rtbTemp.SelectionFont.Style;

				// Check font
				if (replyfont != rtbTemp.SelectionFont.FontFamily.Name)
					replyfont = "";

				// Check font size
				if (replyfontsize != rtbTemp.SelectionFont.Size)
					replyfontsize = (float)0.0;
			}

			// Now set font and size if more than one font or font size was selected
			if (replyfont == "")
				replyfont = rtbTemp.Font.FontFamily.Name;

			if (replyfontsize == 0.0)
				replyfontsize = rtbTemp.Font.Size;

			// generate reply font
			Font reply
				= new Font(replyfont, replyfontsize, replystyle);

			return reply;
		}

		#endregion Get Font Details

		#region Keyboard Handler

		private void rtb1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control)
			{
				ToolBarButton tbb = null;

				switch (e.KeyCode)
				{
					case Keys.B:
						tbb = tbbBold;
						break;

					case Keys.I:
						tbb = tbbItalic;
						break;

					case Keys.S:
						tbb = tbbStamp;
						break;

					case Keys.U:
						tbb = tbbUnderline;
						break;

					case Keys.OemMinus:
						tbb = tbbStrikeout;
						break;
				}

				if (tbb != null)
				{
					if (e.KeyCode != Keys.S) tbb.Pushed = !tbb.Pushed;
					tb1_ButtonClick(null, new ToolBarButtonClickEventArgs(tbb));
				}
			}

			//Insert a tab if the tab key was pressed.
			/* NOTE: This was needed because in rtb1_KeyPress I tell the richtextbox not
			 * to handle tab events.  I do that because CTRL+I inserts a tab for some
			 * strange reason.  What was MicroSoft thinking?
			 * Richard Parsons 02/08/2007
			 */
			if (e.KeyCode == Keys.Tab)
				rtb1.SelectedText = "\t";
		}

		private void rtb1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 9)
				e.Handled = true; // Stops Ctrl+I from inserting a tab (char HT) into the richtextbox
		}

		#endregion Keyboard Handler
	} //end class
} //end namespace