/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
        private RichTextBox _rtbTemp = new RichTextBox();

        #region Windows Generated

        private RichTextBox _rtb1;
        private ImageList _imgList1;
        private ContextMenu _cmColors;
        private MenuItem _miBlack;
        private MenuItem _miBlue;
        private MenuItem _miRed;
        private MenuItem _miGreen;
        private OpenFileDialog _ofd1;
        private SaveFileDialog _sfd1;
        private ContextMenu _cmFonts;
        private MenuItem _miArial;
        private MenuItem _miGaramond;
        private MenuItem _miTahoma;
        private MenuItem _miTimesNewRoman;
        private MenuItem _miVerdana;
        private MenuItem _miCourierNew;
        private MenuItem _miMicrosoftSansSerif;
        private ContextMenu _cmFontSizes;
        private MenuItem _mi8;
        private MenuItem _mi9;
        private MenuItem _mi10;
        private MenuItem _mi11;
        private MenuItem _mi12;
        private MenuItem _mi14;
        private MenuItem _mi16;
        private MenuItem _mi18;
        private MenuItem _mi20;
        private MenuItem _mi22;
        private MenuItem _mi24;
        private MenuItem _mi26;
        private MenuItem _mi28;
        private MenuItem _mi36;
        private MenuItem _mi48;
        private MenuItem _mi72;
        private ToolBarButton _tbbStamp;
        private ToolBarButton _tbbPaste;
        private ToolBarButton _tbbCopy;
        private ToolBarButton _tbbCut;
        private ToolBarButton _tbbSeparator4;
        private ToolBarButton _tbbRedo;
        private ToolBarButton _tbbUndo;
        private ToolBarButton _tbbSeparator2;
        private ToolBarButton _tbbRight;
        private ToolBarButton _tbbCenter;
        private ToolBarButton _tbbLeft;
        private ToolBarButton _tbbSeparator1;
        private ToolBarButton _tbbStrikeout;
        private ToolBarButton _tbbUnderline;
        private ToolBarButton _tbbItalic;
        private ToolBarButton _tbbBold;
        private ToolBarButton _tbbSeparator5;
        private ToolBarButton _tbbColor;
        private ToolBarButton _tbbFontSize;
        private ToolBarButton _tbbFont;
        private ToolBarButton _tbbSeparator3;
        private ToolBarButton _tbbSave;
        private ToolBar _tb1;
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
            this._cmFonts = new System.Windows.Forms.ContextMenu();
            this._miArial = new System.Windows.Forms.MenuItem();
            this._miCourierNew = new System.Windows.Forms.MenuItem();
            this._miGaramond = new System.Windows.Forms.MenuItem();
            this._miMicrosoftSansSerif = new System.Windows.Forms.MenuItem();
            this._miTahoma = new System.Windows.Forms.MenuItem();
            this._miTimesNewRoman = new System.Windows.Forms.MenuItem();
            this._miVerdana = new System.Windows.Forms.MenuItem();
            this._cmFontSizes = new System.Windows.Forms.ContextMenu();
            this._mi8 = new System.Windows.Forms.MenuItem();
            this._mi9 = new System.Windows.Forms.MenuItem();
            this._mi10 = new System.Windows.Forms.MenuItem();
            this._mi11 = new System.Windows.Forms.MenuItem();
            this._mi12 = new System.Windows.Forms.MenuItem();
            this._mi14 = new System.Windows.Forms.MenuItem();
            this._mi16 = new System.Windows.Forms.MenuItem();
            this._mi18 = new System.Windows.Forms.MenuItem();
            this._mi20 = new System.Windows.Forms.MenuItem();
            this._mi22 = new System.Windows.Forms.MenuItem();
            this._mi24 = new System.Windows.Forms.MenuItem();
            this._mi26 = new System.Windows.Forms.MenuItem();
            this._mi28 = new System.Windows.Forms.MenuItem();
            this._mi36 = new System.Windows.Forms.MenuItem();
            this._mi48 = new System.Windows.Forms.MenuItem();
            this._mi72 = new System.Windows.Forms.MenuItem();
            this._cmColors = new System.Windows.Forms.ContextMenu();
            this._miBlack = new System.Windows.Forms.MenuItem();
            this._miBlue = new System.Windows.Forms.MenuItem();
            this._miRed = new System.Windows.Forms.MenuItem();
            this._miGreen = new System.Windows.Forms.MenuItem();
            this._imgList1 = new System.Windows.Forms.ImageList(this.components);
            this._rtb1 = new System.Windows.Forms.RichTextBox();
            this._ofd1 = new System.Windows.Forms.OpenFileDialog();
            this._sfd1 = new System.Windows.Forms.SaveFileDialog();
            this._tbbStamp = new System.Windows.Forms.ToolBarButton();
            this._tbbPaste = new System.Windows.Forms.ToolBarButton();
            this._tbbCopy = new System.Windows.Forms.ToolBarButton();
            this._tbbCut = new System.Windows.Forms.ToolBarButton();
            this._tbbSeparator4 = new System.Windows.Forms.ToolBarButton();
            this._tbbRedo = new System.Windows.Forms.ToolBarButton();
            this._tbbUndo = new System.Windows.Forms.ToolBarButton();
            this._tbbSeparator2 = new System.Windows.Forms.ToolBarButton();
            this._tbbRight = new System.Windows.Forms.ToolBarButton();
            this._tbbCenter = new System.Windows.Forms.ToolBarButton();
            this._tbbLeft = new System.Windows.Forms.ToolBarButton();
            this._tbbSeparator1 = new System.Windows.Forms.ToolBarButton();
            this._tbbStrikeout = new System.Windows.Forms.ToolBarButton();
            this._tbbUnderline = new System.Windows.Forms.ToolBarButton();
            this._tbbItalic = new System.Windows.Forms.ToolBarButton();
            this._tbbBold = new System.Windows.Forms.ToolBarButton();
            this._tbbSeparator5 = new System.Windows.Forms.ToolBarButton();
            this._tbbColor = new System.Windows.Forms.ToolBarButton();
            this._tbbFontSize = new System.Windows.Forms.ToolBarButton();
            this._tbbFont = new System.Windows.Forms.ToolBarButton();
            this._tbbSeparator3 = new System.Windows.Forms.ToolBarButton();
            this._tbbSave = new System.Windows.Forms.ToolBarButton();
            this._tb1 = new System.Windows.Forms.ToolBar();
            this.SuspendLayout();
            //
            // cmFonts
            //
            this._cmFonts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miArial,
            this._miCourierNew,
            this._miGaramond,
            this._miMicrosoftSansSerif,
            this._miTahoma,
            this._miTimesNewRoman,
            this._miVerdana});
            //
            // miArial
            //
            this._miArial.Index = 0;
            this._miArial.Text = "Arial";
            this._miArial.Click += new System.EventHandler(this.Font_Click);
            //
            // miCourierNew
            //
            this._miCourierNew.Index = 1;
            this._miCourierNew.Text = "Courier New";
            this._miCourierNew.Click += new System.EventHandler(this.Font_Click);
            //
            // miGaramond
            //
            this._miGaramond.Index = 2;
            this._miGaramond.Text = "Garamond";
            this._miGaramond.Click += new System.EventHandler(this.Font_Click);
            //
            // miMicrosoftSansSerif
            //
            this._miMicrosoftSansSerif.Index = 3;
            this._miMicrosoftSansSerif.Text = "Microsoft Sans Serif";
            this._miMicrosoftSansSerif.Click += new System.EventHandler(this.Font_Click);
            //
            // miTahoma
            //
            this._miTahoma.Index = 4;
            this._miTahoma.Text = "Tahoma";
            this._miTahoma.Click += new System.EventHandler(this.Font_Click);
            //
            // miTimesNewRoman
            //
            this._miTimesNewRoman.Index = 5;
            this._miTimesNewRoman.Text = "Times New Roman";
            this._miTimesNewRoman.Click += new System.EventHandler(this.Font_Click);
            //
            // miVerdana
            //
            this._miVerdana.Index = 6;
            this._miVerdana.Text = "Verdana";
            this._miVerdana.Click += new System.EventHandler(this.Font_Click);
            //
            // cmFontSizes
            //
            this._cmFontSizes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mi8,
            this._mi9,
            this._mi10,
            this._mi11,
            this._mi12,
            this._mi14,
            this._mi16,
            this._mi18,
            this._mi20,
            this._mi22,
            this._mi24,
            this._mi26,
            this._mi28,
            this._mi36,
            this._mi48,
            this._mi72});
            //
            // mi8
            //
            this._mi8.Index = 0;
            this._mi8.Text = "8";
            this._mi8.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi9
            //
            this._mi9.Index = 1;
            this._mi9.Text = "9";
            this._mi9.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi10
            //
            this._mi10.Index = 2;
            this._mi10.Text = "10";
            this._mi10.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi11
            //
            this._mi11.Index = 3;
            this._mi11.Text = "11";
            this._mi11.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi12
            //
            this._mi12.Index = 4;
            this._mi12.Text = "12";
            this._mi12.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi14
            //
            this._mi14.Index = 5;
            this._mi14.Text = "14";
            this._mi14.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi16
            //
            this._mi16.Index = 6;
            this._mi16.Text = "16";
            this._mi16.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi18
            //
            this._mi18.Index = 7;
            this._mi18.Text = "18";
            this._mi18.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi20
            //
            this._mi20.Index = 8;
            this._mi20.Text = "20";
            this._mi20.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi22
            //
            this._mi22.Index = 9;
            this._mi22.Text = "22";
            this._mi22.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi24
            //
            this._mi24.Index = 10;
            this._mi24.Text = "24";
            this._mi24.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi26
            //
            this._mi26.Index = 11;
            this._mi26.Text = "26";
            this._mi26.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi28
            //
            this._mi28.Index = 12;
            this._mi28.Text = "28";
            this._mi28.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi36
            //
            this._mi36.Index = 13;
            this._mi36.Text = "36";
            this._mi36.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi48
            //
            this._mi48.Index = 14;
            this._mi48.Text = "48";
            this._mi48.Click += new System.EventHandler(this.FontSize_Click);
            //
            // mi72
            //
            this._mi72.Index = 15;
            this._mi72.Text = "72";
            this._mi72.Click += new System.EventHandler(this.FontSize_Click);
            //
            // cmColors
            //
            this._cmColors.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miBlack,
            this._miBlue,
            this._miRed,
            this._miGreen});
            //
            // miBlack
            //
            this._miBlack.Index = 0;
            this._miBlack.Text = "Black";
            this._miBlack.Click += new System.EventHandler(this.Color_Click);
            //
            // miBlue
            //
            this._miBlue.Index = 1;
            this._miBlue.Text = "Blue";
            this._miBlue.Click += new System.EventHandler(this.Color_Click);
            //
            // miRed
            //
            this._miRed.Index = 2;
            this._miRed.Text = "Red";
            this._miRed.Click += new System.EventHandler(this.Color_Click);
            //
            // miGreen
            //
            this._miGreen.Index = 3;
            this._miGreen.Text = "Green";
            this._miGreen.Click += new System.EventHandler(this.Color_Click);
            //
            // imgList1
            //
            this._imgList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList1.ImageStream")));
            this._imgList1.TransparentColor = System.Drawing.Color.Transparent;
            this._imgList1.Images.SetKeyName(0, "");
            this._imgList1.Images.SetKeyName(1, "");
            this._imgList1.Images.SetKeyName(2, "");
            this._imgList1.Images.SetKeyName(3, "");
            this._imgList1.Images.SetKeyName(4, "");
            this._imgList1.Images.SetKeyName(5, "");
            this._imgList1.Images.SetKeyName(6, "");
            this._imgList1.Images.SetKeyName(7, "");
            this._imgList1.Images.SetKeyName(8, "");
            this._imgList1.Images.SetKeyName(9, "");
            this._imgList1.Images.SetKeyName(10, "");
            this._imgList1.Images.SetKeyName(11, "");
            this._imgList1.Images.SetKeyName(12, "");
            this._imgList1.Images.SetKeyName(13, "");
            this._imgList1.Images.SetKeyName(14, "");
            this._imgList1.Images.SetKeyName(15, "");
            this._imgList1.Images.SetKeyName(16, "");
            this._imgList1.Images.SetKeyName(17, "");
            this._imgList1.Images.SetKeyName(18, "");
            this._imgList1.Images.SetKeyName(19, "");
            this._imgList1.Images.SetKeyName(20, "");
            //
            // rtb1
            //
            this._rtb1.AutoWordSelection = true;
            this._rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rtb1.Location = new System.Drawing.Point(0, 26);
            this._rtb1.Name = "_rtb1";
            this._rtb1.Size = new System.Drawing.Size(504, 198);
            this._rtb1.TabIndex = 1;
            this._rtb1.Text = "";
            this._rtb1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtb1_LinkClicked);
            this._rtb1.SelectionChanged += new System.EventHandler(this.rtb1_SelectionChanged);
            this._rtb1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtb1_KeyDown);
            this._rtb1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.rtb1_KeyPress);
            //
            // ofd1
            //
            this._ofd1.DefaultExt = "rtf";
            this._ofd1.Filter = "Rich Text Files|*.rtf|Plain Text File|*.txt";
            this._ofd1.Title = "Open File";
            //
            // sfd1
            //
            this._sfd1.DefaultExt = "rtf";
            this._sfd1.Filter = "Rich Text File|*.rtf|Plain Text File|*.txt";
            this._sfd1.Title = "Save As";
            //
            // tbbStamp
            //
            this._tbbStamp.ImageIndex = 8;
            this._tbbStamp.Name = "_tbbStamp";
            this._tbbStamp.Tag = "edit stamp";
            //
            // tbbPaste
            //
            this._tbbPaste.ImageIndex = 19;
            this._tbbPaste.Name = "_tbbPaste";
            this._tbbPaste.Tag = "paste";
            //
            // tbbCopy
            //
            this._tbbCopy.ImageIndex = 18;
            this._tbbCopy.Name = "_tbbCopy";
            this._tbbCopy.Tag = "copy";
            //
            // tbbCut
            //
            this._tbbCut.ImageIndex = 17;
            this._tbbCut.Name = "_tbbCut";
            this._tbbCut.Tag = "cut";
            //
            // tbbSeparator4
            //
            this._tbbSeparator4.Name = "_tbbSeparator4";
            this._tbbSeparator4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            //
            // tbbRedo
            //
            this._tbbRedo.ImageIndex = 13;
            this._tbbRedo.Name = "_tbbRedo";
            this._tbbRedo.Tag = "redo";
            //
            // tbbUndo
            //
            this._tbbUndo.ImageIndex = 12;
            this._tbbUndo.Name = "_tbbUndo";
            this._tbbUndo.Tag = "undo";
            //
            // tbbSeparator2
            //
            this._tbbSeparator2.Name = "_tbbSeparator2";
            this._tbbSeparator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            //
            // tbbRight
            //
            this._tbbRight.ImageIndex = 6;
            this._tbbRight.Name = "_tbbRight";
            this._tbbRight.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbRight.Tag = "right";
            //
            // tbbCenter
            //
            this._tbbCenter.ImageIndex = 5;
            this._tbbCenter.Name = "_tbbCenter";
            this._tbbCenter.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbCenter.Tag = "center";
            //
            // tbbLeft
            //
            this._tbbLeft.ImageIndex = 4;
            this._tbbLeft.Name = "_tbbLeft";
            this._tbbLeft.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbLeft.Tag = "left";
            //
            // tbbSeparator1
            //
            this._tbbSeparator1.Name = "_tbbSeparator1";
            this._tbbSeparator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            //
            // tbbStrikeout
            //
            this._tbbStrikeout.ImageIndex = 3;
            this._tbbStrikeout.Name = "_tbbStrikeout";
            this._tbbStrikeout.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbStrikeout.Tag = "strikeout";
            //
            // tbbUnderline
            //
            this._tbbUnderline.ImageIndex = 2;
            this._tbbUnderline.Name = "_tbbUnderline";
            this._tbbUnderline.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbUnderline.Tag = "underline";
            //
            // tbbItalic
            //
            this._tbbItalic.ImageIndex = 1;
            this._tbbItalic.Name = "_tbbItalic";
            this._tbbItalic.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbItalic.Tag = "italic";
            //
            // tbbBold
            //
            this._tbbBold.ImageIndex = 0;
            this._tbbBold.Name = "_tbbBold";
            this._tbbBold.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this._tbbBold.Tag = "bold";
            //
            // tbbSeparator5
            //
            this._tbbSeparator5.Name = "_tbbSeparator5";
            this._tbbSeparator5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            //
            // tbbColor
            //
            this._tbbColor.DropDownMenu = this._cmColors;
            this._tbbColor.ImageIndex = 7;
            this._tbbColor.Name = "_tbbColor";
            this._tbbColor.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this._tbbColor.Tag = "color";
            //
            // tbbFontSize
            //
            this._tbbFontSize.DropDownMenu = this._cmFontSizes;
            this._tbbFontSize.ImageIndex = 15;
            this._tbbFontSize.Name = "_tbbFontSize";
            this._tbbFontSize.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this._tbbFontSize.Tag = "font size";
            //
            // tbbFont
            //
            this._tbbFont.DropDownMenu = this._cmFonts;
            this._tbbFont.ImageIndex = 14;
            this._tbbFont.Name = "_tbbFont";
            this._tbbFont.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this._tbbFont.Tag = "font";
            //
            // tbbSeparator3
            //
            this._tbbSeparator3.Name = "_tbbSeparator3";
            this._tbbSeparator3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            //
            // tbbSave
            //
            this._tbbSave.ImageIndex = 11;
            this._tbbSave.Name = "_tbbSave";
            this._tbbSave.Tag = "save";
            //
            // tb1
            //
            this._tb1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this._tb1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this._tbbSave,
            this._tbbSeparator3,
            this._tbbFont,
            this._tbbFontSize,
            this._tbbColor,
            this._tbbSeparator5,
            this._tbbBold,
            this._tbbItalic,
            this._tbbUnderline,
            this._tbbStrikeout,
            this._tbbSeparator1,
            this._tbbLeft,
            this._tbbCenter,
            this._tbbRight,
            this._tbbSeparator2,
            this._tbbUndo,
            this._tbbRedo,
            this._tbbSeparator4,
            this._tbbCut,
            this._tbbCopy,
            this._tbbPaste,
            this._tbbStamp});
            this._tb1.ButtonSize = new System.Drawing.Size(16, 16);
            this._tb1.Divider = false;
            this._tb1.DropDownArrows = true;
            this._tb1.ImageList = this._imgList1;
            this._tb1.Location = new System.Drawing.Point(0, 0);
            this._tb1.Name = "_tb1";
            this._tb1.ShowToolTips = true;
            this._tb1.Size = new System.Drawing.Size(504, 26);
            this._tb1.TabIndex = 0;
            this._tb1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tb1_ButtonClick);
            //
            // RichTextBoxExtended
            //
            this.Controls.Add(this._rtb1);
            this.Controls.Add(this._tb1);
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
                        if (_rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
                        stamp.Append("Edited by ");
                        //use the CurrentPrincipal name if one exsist else use windows logon username
                        if (Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null || Thread.CurrentPrincipal.Identity.Name.Length <= 0)
                            stamp.Append(Environment.UserName);
                        else
                            stamp.Append(Thread.CurrentPrincipal.Identity.Name);
                        stamp.Append(" on " + DateTime.Now.ToLongDateString() + "\r\n");

                        _rtb1.SelectionLength = 0; //unselect everything basicly
                        _rtb1.SelectionStart = _rtb1.Text.Length; //start new selection at the end of the text
                        _rtb1.SelectionColor = StampColor; //make the selection blue
                        _rtb1.SelectionFont = new Font(_rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
                        _rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
                        _rtb1.Focus(); //set focus back on the richtextbox
                    }
                    break; //end edited by stamp
                case StampActions.DateTime:
                    {
                        StringBuilder stamp = new StringBuilder(""); //holds our stamp text
                        if (_rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
                        stamp.Append(DateTime.Now.ToLongDateString() + "\r\n");
                        _rtb1.SelectionLength = 0; //unselect everything basicly
                        _rtb1.SelectionStart = _rtb1.Text.Length; //start new selection at the end of the text
                        _rtb1.SelectionColor = StampColor; //make the selection blue
                        _rtb1.SelectionFont = new Font(_rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
                        _rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
                        _rtb1.Focus(); //set focus back on the richtextbox
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
                    _rtb1.SelectionAlignment = HorizontalAlignment.Left;
                    _tbbCenter.Pushed = false;
                    _tbbRight.Pushed = false;
                    break;

                case "center":
                    //change horizontal alignment to center
                    _tbbLeft.Pushed = false;
                    _rtb1.SelectionAlignment = HorizontalAlignment.Center;
                    _tbbRight.Pushed = false;
                    break;

                case "right":
                    //change horizontal alignment to right
                    _tbbLeft.Pushed = false;
                    _tbbCenter.Pushed = false;
                    _rtb1.SelectionAlignment = HorizontalAlignment.Right;
                    break;

                case "edit stamp":
                    OnStamp(new EventArgs()); //send stamp event
                    break;

                case "color":
                    _rtb1.SelectionColor = Color.Black;
                    break;

                case "undo":
                    _rtb1.Undo();
                    break;

                case "redo":
                    _rtb1.Redo();
                    break;

                case "save":
                    if (_sfd1.ShowDialog() == DialogResult.OK && _sfd1.FileName.Length > 0)
                        if (Path.GetExtension(_sfd1.FileName).ToLower().Equals(".rtf"))
                            _rtb1.SaveFile(_sfd1.FileName);
                        else
                            _rtb1.SaveFile(_sfd1.FileName, RichTextBoxStreamType.PlainText);
                    break;

                case "cut":
                    {
                        if (_rtb1.SelectedText.Length <= 0) break;
                        _rtb1.Cut();
                        break;
                    }
                case "copy":
                    {
                        if (_rtb1.SelectedText.Length <= 0) break;
                        _rtb1.Copy();
                        break;
                    }
                case "paste":
                    {
                        try
                        {
                            _rtb1.Paste();
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

            //Set all the style buttons using the gathered style
            _tbbBold.Pushed = fnt.Bold; //bold button
            _tbbItalic.Pushed = fnt.Italic; //italic button
            _tbbUnderline.Pushed = fnt.Underline; //underline button
            _tbbStrikeout.Pushed = fnt.Strikeout; //strikeout button
            _tbbLeft.Pushed = (_rtb1.SelectionAlignment == HorizontalAlignment.Left); //justify left
            _tbbCenter.Pushed = (_rtb1.SelectionAlignment == HorizontalAlignment.Center); //justify center
            _tbbRight.Pushed = (_rtb1.SelectionAlignment == HorizontalAlignment.Right); //justify right

            //Check the correct color
            foreach (MenuItem mi in _cmColors.MenuItems)
                mi.Checked = (_rtb1.SelectionColor == Color.FromName(mi.Text));

            //Check the correct font
            foreach (MenuItem mi in _cmFonts.MenuItems)
                mi.Checked = (fnt.FontFamily.Name == mi.Text);

            //Check the correct font size
            foreach (MenuItem mi in _cmFontSizes.MenuItems)
                mi.Checked = ((int)fnt.SizeInPoints == float.Parse(mi.Text));
        }

        #endregion Update Toolbar

        #region Update Toolbar Seperators

        private void UpdateToolbarSeperators()
        {
            //Save & Open
            _tbbSeparator3.Visible = _tbbSave.Visible;

            //Font & Font Size
            if (!_tbbFont.Visible && !_tbbFontSize.Visible && !_tbbColor.Visible)
                _tbbSeparator5.Visible = false;
            else
                _tbbSeparator5.Visible = true;

            //Bold, Italic, Underline, & Strikeout
            if (!_tbbBold.Visible && !_tbbItalic.Visible && !_tbbUnderline.Visible && !_tbbStrikeout.Visible)
                _tbbSeparator1.Visible = false;
            else
                _tbbSeparator1.Visible = true;

            //Left, Center, & Right
            if (!_tbbLeft.Visible && !_tbbCenter.Visible && !_tbbRight.Visible)
                _tbbSeparator2.Visible = false;
            else
                _tbbSeparator2.Visible = true;

            //Undo & Redo
            if (!_tbbUndo.Visible && !_tbbRedo.Visible)
                _tbbSeparator4.Visible = false;
            else
                _tbbSeparator4.Visible = true;
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
            SelChanged?.Invoke(this, e);
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
            get { return _tb1; }
        }

        /// <summary>
        ///     The RichTextBox that is contained with-in the RichTextBoxExtened control
        /// </summary>
        [Description("The internal richtextbox control"),
        Category("Internal Controls"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RichTextBox RichTextBox
        {
            get { return _rtb1; }
        }

        /// <summary>
        ///     Show the save button or not
        /// </summary>
        [Description("Show the save button or not"),
        Category("Appearance")]
        public bool ShowSave
        {
            get { return _tbbSave.Visible; }
            set { _tbbSave.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the stamp button or not
        /// </summary>
        [Description("Show the stamp button or not"),
         Category("Appearance")]
        public bool ShowStamp
        {
            get { return _tbbStamp.Visible; }
            set { _tbbStamp.Visible = value; }
        }

        /// <summary>
        ///     Show the color button or not
        /// </summary>
        [Description("Show the color button or not"),
        Category("Appearance")]
        public bool ShowColors
        {
            get { return _tbbColor.Visible; }
            set { _tbbColor.Visible = value; }
        }

        /// <summary>
        ///     Show the undo button or not
        /// </summary>
        [Description("Show the undo button or not"),
        Category("Appearance")]
        public bool ShowUndo
        {
            get { return _tbbUndo.Visible; }
            set { _tbbUndo.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the redo button or not
        /// </summary>
        [Description("Show the redo button or not"),
        Category("Appearance")]
        public bool ShowRedo
        {
            get { return _tbbRedo.Visible; }
            set { _tbbRedo.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the bold button or not
        /// </summary>
        [Description("Show the bold button or not"),
        Category("Appearance")]
        public bool ShowBold
        {
            get { return _tbbBold.Visible; }
            set { _tbbBold.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the italic button or not
        /// </summary>
        [Description("Show the italic button or not"),
        Category("Appearance")]
        public bool ShowItalic
        {
            get { return _tbbItalic.Visible; }
            set { _tbbItalic.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the underline button or not
        /// </summary>
        [Description("Show the underline button or not"),
        Category("Appearance")]
        public bool ShowUnderline
        {
            get { return _tbbUnderline.Visible; }
            set { _tbbUnderline.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the strikeout button or not
        /// </summary>
        [Description("Show the strikeout button or not"),
        Category("Appearance")]
        public bool ShowStrikeout
        {
            get { return _tbbStrikeout.Visible; }
            set { _tbbStrikeout.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the left justify button or not
        /// </summary>
        [Description("Show the left justify button or not"),
        Category("Appearance")]
        public bool ShowLeftJustify
        {
            get { return _tbbLeft.Visible; }
            set { _tbbLeft.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the right justify button or not
        /// </summary>
        [Description("Show the right justify button or not"),
        Category("Appearance")]
        public bool ShowRightJustify
        {
            get { return _tbbRight.Visible; }
            set { _tbbRight.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Show the center justify button or not
        /// </summary>
        [Description("Show the center justify button or not"),
        Category("Appearance")]
        public bool ShowCenterJustify
        {
            get { return _tbbCenter.Visible; }
            set { _tbbCenter.Visible = value; UpdateToolbarSeperators(); }
        }

        /// <summary>
        ///     Determines how the stamp button will respond
        /// </summary>
        private StampActions _mStampAction = StampActions.EditedBy;

        [Description("Determines how the stamp button will respond"),
        Category("Behavior")]
        public StampActions StampAction
        {
            get { return _mStampAction; }
            set { _mStampAction = value; }
        }

        /// <summary>
        ///     Color of the stamp text
        /// </summary>
        private Color _mStampColor = Color.Blue;

        [Description("Color of the stamp text"),
        Category("Appearance")]
        public Color StampColor
        {
            get { return _mStampColor; }
            set { _mStampColor = value; }
        }

        /// <summary>
        ///     Show the font button or not
        /// </summary>
        [Description("Show the font button or not"),
        Category("Appearance")]
        public bool ShowFont
        {
            get { return _tbbFont.Visible; }
            set { _tbbFont.Visible = value; }
        }

        /// <summary>
        ///     Show the font size button or not
        /// </summary>
        [Description("Show the font size button or not"),
        Category("Appearance")]
        public bool ShowFontSize
        {
            get { return _tbbFontSize.Visible; }
            set { _tbbFontSize.Visible = value; }
        }

        /// <summary>
        ///     Show the cut button or not
        /// </summary>
        [Description("Show the cut button or not"),
        Category("Appearance")]
        public bool ShowCut
        {
            get { return _tbbCut.Visible; }
            set { _tbbCut.Visible = value; }
        }

        /// <summary>
        ///     Show the copy button or not
        /// </summary>
        [Description("Show the copy button or not"),
        Category("Appearance")]
        public bool ShowCopy
        {
            get { return _tbbCopy.Visible; }
            set { _tbbCopy.Visible = value; }
        }

        /// <summary>
        ///     Show the paste button or not
        /// </summary>
        [Description("Show the paste button or not"),
        Category("Appearance")]
        public bool ShowPaste
        {
            get { return _tbbPaste.Visible; }
            set { _tbbPaste.Visible = value; }
        }

        /// <summary>
        ///     Detect URLs with-in the richtextbox
        /// </summary>
        [Description("Detect URLs with-in the richtextbox"),
        Category("Behavior")]
        public bool DetectUrLs
        {
            get { return _rtb1.DetectUrls; }
            set { _rtb1.DetectUrls = value; }
        }

        /// <summary>
        /// Determines if the TAB key moves to the next control or enters a TAB character in the richtextbox
        /// </summary>
        [Description("Determines if the TAB key moves to the next control or enters a TAB character in the richtextbox"),
        Category("Behavior")]
        public bool AcceptsTab
        {
            get { return _rtb1.AcceptsTab; }
            set { _rtb1.AcceptsTab = value; }
        }

        /// <summary>
        /// Determines if auto word selection is enabled
        /// </summary>
        [Description("Determines if auto word selection is enabled"),
        Category("Behavior")]
        public bool AutoWordSelection
        {
            get { return _rtb1.AutoWordSelection; }
            set { _rtb1.AutoWordSelection = value; }
        }

        /// <summary>
        /// Determines if this control can be edited
        /// </summary>
        [Description("Determines if this control can be edited"),
        Category("Behavior")]
        public bool ReadOnly
        {
            get { return _rtb1.ReadOnly; }
            set
            {
                _tb1.Visible = !value;
                _rtb1.ReadOnly = value;
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
                    _tbbSave.Text = "Save";
                    _tbbBold.Text = "Bold";
                    _tbbFont.Text = "Font";
                    _tbbFontSize.Text = "Font Size";
                    _tbbColor.Text = "Font Color";
                    _tbbItalic.Text = "Italic";
                    _tbbStrikeout.Text = "Strikeout";
                    _tbbUnderline.Text = "Underline";
                    _tbbLeft.Text = "Left";
                    _tbbCenter.Text = "Center";
                    _tbbRight.Text = "Right";
                    _tbbUndo.Text = "Undo";
                    _tbbRedo.Text = "Redo";
                    _tbbCut.Text = "Cut";
                    _tbbCopy.Text = "Copy";
                    _tbbPaste.Text = "Paste";
                    _tbbStamp.Text = "Stamp";
                }
                else
                {
                    _tbbSave.Text = "";
                    _tbbBold.Text = "";
                    _tbbFont.Text = "";
                    _tbbFontSize.Text = "";
                    _tbbColor.Text = "";
                    _tbbItalic.Text = "";
                    _tbbStrikeout.Text = "";
                    _tbbUnderline.Text = "";
                    _tbbLeft.Text = "";
                    _tbbCenter.Text = "";
                    _tbbRight.Text = "";
                    _tbbUndo.Text = "";
                    _tbbRedo.Text = "";
                    _tbbCut.Text = "";
                    _tbbCopy.Text = "";
                    _tbbPaste.Text = "";
                    _tbbStamp.Text = "";
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

            int rtb1Start = _rtb1.SelectionStart;
            int len = _rtb1.SelectionLength;
            int rtbTempStart = 0;

            // If len <= 1 and there is a selection font, amend and return
            if (len <= 1 && _rtb1.SelectionFont != null)
            {
                _rtb1.SelectionFont =
                    new Font(fontFamily, _rtb1.SelectionFont.Size, _rtb1.SelectionFont.Style);
                return;
            }

            // Step through the selected text one char at a time
            _rtbTemp.Rtf = _rtb1.SelectedRtf;
            for (int i = 0; i < len; ++i)
            {
                _rtbTemp.Select(rtbTempStart + i, 1);
                _rtbTemp.SelectionFont = new Font(fontFamily, _rtbTemp.SelectionFont.Size, _rtbTemp.SelectionFont.Style);
            }

            // Replace & reselect
            _rtbTemp.Select(rtbTempStart, len);
            _rtb1.SelectedRtf = _rtbTemp.SelectedRtf;
            _rtb1.Select(rtb1Start, len);
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

            int rtb1Start = _rtb1.SelectionStart;
            int len = _rtb1.SelectionLength;
            int rtbTempStart = 0;

            //if len <= 1 and there is a selection font then just handle and return
            if (len <= 1 && _rtb1.SelectionFont != null)
            {
                //add or remove style
                if (add)
                    _rtb1.SelectionFont = new Font(_rtb1.SelectionFont, _rtb1.SelectionFont.Style | style);
                else
                    _rtb1.SelectionFont = new Font(_rtb1.SelectionFont, _rtb1.SelectionFont.Style & ~style);

                return;
            }

            // Step through the selected text one char at a time
            _rtbTemp.Rtf = _rtb1.SelectedRtf;
            for (int i = 0; i < len; ++i)
            {
                _rtbTemp.Select(rtbTempStart + i, 1);

                //add or remove style
                if (add)
                    _rtbTemp.SelectionFont = new Font(_rtbTemp.SelectionFont, _rtbTemp.SelectionFont.Style | style);
                else
                    _rtbTemp.SelectionFont = new Font(_rtbTemp.SelectionFont, _rtbTemp.SelectionFont.Style & ~style);
            }

            // Replace & reselect
            _rtbTemp.Select(rtbTempStart, len);
            _rtb1.SelectedRtf = _rtbTemp.SelectedRtf;
            _rtb1.Select(rtb1Start, len);
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

            int rtb1Start = _rtb1.SelectionStart;
            int len = _rtb1.SelectionLength;
            int rtbTempStart = 0;

            // If len <= 1 and there is a selection font, amend and return
            if (len <= 1 && _rtb1.SelectionFont != null)
            {
                _rtb1.SelectionFont =
                    new Font(_rtb1.SelectionFont.FontFamily, fontSize, _rtb1.SelectionFont.Style);
                return;
            }

            // Step through the selected text one char at a time
            _rtbTemp.Rtf = _rtb1.SelectedRtf;
            for (int i = 0; i < len; ++i)
            {
                _rtbTemp.Select(rtbTempStart + i, 1);
                _rtbTemp.SelectionFont = new Font(_rtbTemp.SelectionFont.FontFamily, fontSize, _rtbTemp.SelectionFont.Style);
            }

            // Replace & reselect
            _rtbTemp.Select(rtbTempStart, len);
            _rtb1.SelectedRtf = _rtbTemp.SelectedRtf;
            _rtb1.Select(rtb1Start, len);
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

            int rtb1Start = _rtb1.SelectionStart;
            int len = _rtb1.SelectionLength;
            int rtbTempStart = 0;

            //if len <= 1 and there is a selection font then just handle and return
            if (len <= 1 && _rtb1.SelectionFont != null)
            {
                _rtb1.SelectionColor = newColor;
                return;
            }

            // Step through the selected text one char at a time
            _rtbTemp.Rtf = _rtb1.SelectedRtf;
            for (int i = 0; i < len; ++i)
            {
                _rtbTemp.Select(rtbTempStart + i, 1);

                //change color
                _rtbTemp.SelectionColor = newColor;
            }

            // Replace & reselect
            _rtbTemp.Select(rtbTempStart, len);
            _rtb1.SelectedRtf = _rtbTemp.SelectedRtf;
            _rtb1.Select(rtb1Start, len);
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

            int len = _rtb1.SelectionLength;
            const int rtbTempStart = 0;

            if (len <= 1)
            {
                // Return the selection or default font
                if (_rtb1.SelectionFont != null)
                    return _rtb1.SelectionFont;
                return _rtb1.Font;
            }

            // Step through the selected text one char at a time
            // after setting defaults from first char
            _rtbTemp.Rtf = _rtb1.SelectedRtf;

            //Turn everything on so we can turn it off one by one
            FontStyle replystyle =
                FontStyle.Bold | FontStyle.Italic | FontStyle.Strikeout | FontStyle.Underline;

            // Set reply font, size and style to that of first char in selection.
            _rtbTemp.Select(rtbTempStart, 1);
            string replyfont = _rtbTemp.SelectionFont.Name;
            float replyfontsize = _rtbTemp.SelectionFont.Size;
            replystyle = replystyle & _rtbTemp.SelectionFont.Style;

            // Search the rest of the selection
            for (int i = 1; i < len; ++i)
            {
                _rtbTemp.Select(rtbTempStart + i, 1);

                // Check reply for different style
                replystyle = replystyle & _rtbTemp.SelectionFont.Style;

                // Check font
                if (replyfont != _rtbTemp.SelectionFont.FontFamily.Name)
                    replyfont = "";

                // Check font size
                if (replyfontsize != _rtbTemp.SelectionFont.Size)
                    replyfontsize = (float)0.0;
            }

            // Now set font and size if more than one font or font size was selected
            if (replyfont == "")
                replyfont = _rtbTemp.Font.FontFamily.Name;

            if (replyfontsize == 0.0)
                replyfontsize = _rtbTemp.Font.Size;

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
                        tbb = _tbbBold;
                        break;

                    case Keys.I:
                        tbb = _tbbItalic;
                        break;

                    case Keys.S:
                        tbb = _tbbStamp;
                        break;

                    case Keys.U:
                        tbb = _tbbUnderline;
                        break;

                    case Keys.OemMinus:
                        tbb = _tbbStrikeout;
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
                _rtb1.SelectedText = "\t";
        }

        private void rtb1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 9)
                e.Handled = true; // Stops Ctrl+I from inserting a tab (char HT) into the richtextbox
        }

        #endregion Keyboard Handler
    } //end class
} //end namespace