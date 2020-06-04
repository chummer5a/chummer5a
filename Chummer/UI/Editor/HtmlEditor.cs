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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;

namespace Chummer.UI.Editor
{
    public enum FontSize
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        NA
    }

    public enum SelectionType
    {
        Text,
        Control,
        None
    }

    public enum ReadyState
    {
        Uninitialized,
        Loading,
        Loaded,
        Interactive,
        Complete
    }

    public partial class HtmlEditor : UserControl
    {
        private bool _blnSkipUpdate;
        private readonly IHTMLDocument2 _domDocument;

        public HtmlEditor()
        {
            InitializeComponent();
            webContent.DocumentText = @"<html><body></body></html>";
            _domDocument = webContent.Document?.DomDocument as IHTMLDocument2;
            if (_domDocument == null)
                throw new ArgumentNullException(nameof(webContent.Document));
            _domDocument.designMode = "On";
            AutoCompleteStringCollection lstFontsAutoComplete = new AutoCompleteStringCollection();
            foreach (FontFamily objFontFamily in FontFamily.Families)
            {
                cboFont.Items.Add(objFontFamily.Name);
                lstFontsAutoComplete.Add(objFontFamily.Name);
            }
            cboFont.AutoCompleteCustomSource = lstFontsAutoComplete;
        }

        private void UpdateButtons(object sender, EventArgs e)
        {
            if (ReadyState != ReadyState.Complete)
                return;
            _blnSkipUpdate = true;
            if (!cboFont.Focused)
            {
                string strFontName = FontName.Name;
                if (!string.IsNullOrEmpty(strFontName) && strFontName != cboFont.Text)
                {
                    cboFont.Text = strFontName;
                }
            }
            if (!cboFontSize.Focused)
            {
                int intFontSize;
                switch (FontSize)
                {
                    case FontSize.One:
                        intFontSize = 1;
                        break;
                    case FontSize.Two:
                        intFontSize = 2;
                        break;
                    case FontSize.Three:
                        intFontSize = 3;
                        break;
                    case FontSize.Four:
                        intFontSize = 4;
                        break;
                    case FontSize.Five:
                        intFontSize = 5;
                        break;
                    case FontSize.Six:
                        intFontSize = 6;
                        break;
                    case FontSize.Seven:
                        intFontSize = 7;
                        break;
                    case FontSize.NA:
                        intFontSize = 0;
                        break;
                    default:
                        intFontSize = 7;
                        break;
                }
                string strFontSize = Convert.ToString(intFontSize);
                if (strFontSize != cboFontSize.Text)
                {
                    cboFontSize.Text = strFontSize;
                }
            }
            tsbBold.Checked = IsBold;
            tsbItalic.Checked = IsItalic;
            tsbUnderline.Checked = IsUnderline;
            tsbHyperlink.Enabled = CanInsertLink;
            tsbAlignLeft.Checked = IsJustifyLeft;
            tsbAlignCenter.Checked = IsJustifyCenter;
            tsbAlignRight.Checked = IsJustifyRight;
            tsbAlignJustify.Checked = IsJustifyFull;
            tsbOrderedList.Checked = IsOrderedList;
            tsbUnorderedList.Checked = IsUnorderedList;
            foreach (HTMLImg imgLoop in _domDocument.images.Cast<HTMLImg>().Where(x => x != null))
            {
                if (imgLoop.height != imgLoop.style.pixelHeight && imgLoop.style.pixelHeight != 0)
                    imgLoop.height = imgLoop.style.pixelHeight;
                if (imgLoop.width != imgLoop.style.pixelWidth && imgLoop.style.pixelWidth != 0)
                    imgLoop.width = imgLoop.style.pixelWidth;
            }
            _blnSkipUpdate = false;
        }

        private string ReplaceFileSystemImages(string strHtml)
        {
            MatchCollection lstImagesToHandle = Regex.Matches(strHtml,
                @"<img[^>]*?src\s*=\s*([""']?[^'"">]+?['""])[^>]*?>",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            foreach (string strImagePath in lstImagesToHandle.Cast<Match>().Select(x => x.Groups[1].Value))
            {
                string strImagePathTrimmed = strImagePath.Trim('\"');
                if (!string.IsNullOrEmpty(strImagePathTrimmed) && File.Exists(strImagePathTrimmed))
                {
                    string strExtension = Path.GetExtension(strImagePathTrimmed);
                    if (!string.IsNullOrEmpty(strExtension))
                    {
                        strHtml = strHtml.Replace(strImagePath,
                            string.Format(GlobalOptions.InvariantCultureInfo, "'data:image/{0};base64,{1}'",
                                strExtension,
                                Convert.ToBase64String(File.ReadAllBytes(strImagePathTrimmed))));
                    }
                    else
                        strHtml = strHtml.Replace(strImagePath, string.Empty);
                }
                else
                    strHtml = strHtml.Replace(strImagePath, string.Empty);
            }
            return strHtml;
        }

        private ReadyState ReadyState
        {
            get
            {
                switch (_domDocument.readyState.ToUpperInvariant())
                {
                    case "LOADING":
                        return ReadyState.Loading;
                    case "LOADED":
                        return ReadyState.Loaded;
                    case "INTERACTIVE":
                        return ReadyState.Interactive;
                    case "COMPLETE":
                        return ReadyState.Complete;
                    default:
                        return ReadyState.Uninitialized;
                }
            }
        }

        private SelectionType SelectionType
        {
            get
            {
                switch (_domDocument.selection.type.ToUpperInvariant())
                {
                    case "TEXT":
                        return SelectionType.Text;
                    case "CONTROL":
                        return SelectionType.Control;
                    default:
                        return SelectionType.None;
                }
            }
        }

        [Browsable(false)]
        private FontSize FontSize
        {
            get
            {
                if (ReadyState != ReadyState.Complete)
                    return FontSize.NA;
                switch (_domDocument.queryCommandValue("FontSize").ToString())
                {
                    case "1":
                        return FontSize.One;
                    case "2":
                        return FontSize.Two;
                    case "3":
                        return FontSize.Three;
                    case "4":
                        return FontSize.Four;
                    case "5":
                        return FontSize.Five;
                    case "6":
                        return FontSize.Six;
                    case "7":
                        return FontSize.Seven;
                    default:
                        return FontSize.NA;
                }
            }
            set
            {
                int intSize;
                switch (value)
                {
                    case FontSize.One:
                        intSize = 1;
                        break;
                    case FontSize.Two:
                        intSize = 2;
                        break;
                    case FontSize.Three:
                        intSize = 3;
                        break;
                    case FontSize.Four:
                        intSize = 4;
                        break;
                    case FontSize.Five:
                        intSize = 5;
                        break;
                    case FontSize.Six:
                        intSize = 6;
                        break;
                    case FontSize.Seven:
                        intSize = 7;
                        break;
                    default:
                        intSize = 7;
                        break;
                }
                Document.ExecCommand("FontSize", false, intSize.ToString(GlobalOptions.InvariantCultureInfo));
            }
        }

        [Browsable(false)]
        private FontFamily FontName
        {
            get
            {
                if (ReadyState != ReadyState.Complete)
                    return null;
                return _domDocument.queryCommandValue("FontName") is string strName
                    ? new FontFamily(strName)
                    : null;
            }
            set
            {
                string strName = value?.Name;
                if (!string.IsNullOrEmpty(strName))
                    Document.ExecCommand("FontName", false, strName);
            }
        }

        private HtmlDocument Document => webContent.Document;

        public string DocumentTitle => webContent.DocumentTitle;

        [Browsable(false)]
        public string DocumentText
        {
            get => webContent.DocumentText;
            set => Document?.Write(value ?? "<html><body></body></html>");
        }

        [Browsable(false)]
        public string BodyText
        {
            get
            {
                string strReturn = Document?.Body?.InnerText ?? string.Empty;
                if (!string.IsNullOrEmpty(strReturn))
                {
                    strReturn = ReplaceFileSystemImages(strReturn);
                }
                return strReturn;
            }
            set
            {
                Document?.OpenNew(false);
                if (Document?.Body != null)
                    Document.Body.InnerText = System.Net.WebUtility.HtmlEncode(value);
            }
        }

        [Browsable(false)]
        public string Html
        {
            get
            {
                string strReturn = Document?.Body?.InnerHtml ?? string.Empty;
                if (!string.IsNullOrEmpty(strReturn))
                {
                    strReturn = ReplaceFileSystemImages(strReturn);
                }
                return strReturn;
            }
            set
            {
                Document?.OpenNew(false);
                IHTMLDocument2 domNewDocument = Document?.DomDocument as IHTMLDocument2;
                try
                {
                    if (domNewDocument != null)
                    {
                        if (value == null)
                            domNewDocument.clear();
                        else
                            domNewDocument.write(value);
                    }
                }
                finally
                {
                    domNewDocument?.close();
                }
            }
        }

        [Browsable(true)]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (base.BackColor != value)
                {
                    base.BackColor = value;
                    if (ReadyState == ReadyState.Complete)
                    {
                        SetBackgroundColor(value);
                    }
                }
            }
        }

        private void SetBackgroundColor(Color objColor)
        {
            if (Document?.Body != null)
                Document.Body.Style = "background-color: " + objColor.Name;
        }

        [Browsable(false)]
        private Color EditorForeColor
        {
            get => ReadyState == ReadyState.Complete
                ? ConvertToColor(_domDocument.queryCommandValue("ForeColor").ToString())
                : SystemColors.WindowText;
            set => Document.ExecCommand("ForeColor", false,
                string.Format(GlobalOptions.InvariantCultureInfo, "#{0:X2}{1:X2}{2:X2}",
                    value.R, value.G, value.B));
        }

        [Browsable(false)]
        private Color EditorBackColor
        {
            get => ReadyState == ReadyState.Complete
                ? ConvertToColor(_domDocument.queryCommandValue("BackColor").ToString())
                : SystemColors.Window;
            set => Document.ExecCommand("BackColor", false,
                string.Format(GlobalOptions.InvariantCultureInfo, "#{0:X2}{1:X2}{2:X2}",
                    value.R, value.G, value.B));
        }

        private void SelectForeColor()
        {
            Color objColor = EditorForeColor;
            if (ShowColorDialog(ref objColor))
                EditorForeColor = objColor;
        }

        private void SelectBackColor()
        {
            Color objColor = EditorBackColor;
            if (ShowColorDialog(ref objColor))
                EditorBackColor = objColor;
        }

        private bool ShowColorDialog(ref Color objColor)
        {
            using (ColorDialog dlgChooseColor = new ColorDialog
            {
                SolidColorOnly = true,
                AllowFullOpen = false,
                AnyColor = false,
                FullOpen = false,
                CustomColors = null,
                Color = objColor
            })
            {
                if (dlgChooseColor.ShowDialog(this) == DialogResult.OK)
                {
                    objColor = dlgChooseColor.Color;
                    return true;
                }
            }
            return false;
        }

        private static Color ConvertToColor(string strColor)
        {
            int intR, intG, intB;
            if (strColor.StartsWith("#")) // HEX organized as RGB
            {
                int intColor = Convert.ToInt32(strColor.Substring(1), 16);
                intR = (intColor >> 16) & 255;
                intG = (intColor >> 8) & 255;
                intB = intColor & 255;
            }
            else // DECIMAL organized as BGR
            {
                int intColor = Convert.ToInt32(strColor);
                intR = intColor & 255;
                intG = (intColor >> 8) & 255;
                intB = (intColor >> 16) & 255;
            }
            return Color.FromArgb(intR, intG, intB);
        }

        public void InsertParagraph()
        {
            _domDocument.execCommand("InsertParagraph");
        }

        public void InsertBreak()
        {
            _domDocument.execCommand("InsertHorizontalRule");
        }

        public void SelectAll()
        {
            _domDocument.execCommand("SelectAll");
        }

        private bool CanUndo => _domDocument.queryCommandEnabled("Undo");

        private void Undo()
        {
            _domDocument.execCommand("Undo");
        }

        private bool CanRedo => _domDocument.queryCommandEnabled("Redo");

        private void Redo()
        {
            _domDocument.execCommand("Redo");
        }

        private bool CanCut => _domDocument.queryCommandEnabled("Cut");

        private void Cut()
        {
            _domDocument.execCommand("Cut");
        }

        private bool CanPaste => _domDocument.queryCommandEnabled("Paste");

        private void Paste()
        {
            _domDocument.execCommand("Paste");
        }

        private bool CanCopy => _domDocument.queryCommandEnabled("Copy");

        private void Copy()
        {
            _domDocument.execCommand("Copy");
        }

        private bool IsOrderedList => _domDocument.queryCommandState("InsertOrderedList");

        private void OrderedList()
        {
            _domDocument.execCommand("InsertOrderedList");
        }

        private bool IsUnorderedList => _domDocument.queryCommandState("InsertUnorderedList");

        private void UnorderedList()
        {
            _domDocument.execCommand("InsertUnorderedList");
        }

        private bool IsJustifyLeft => _domDocument.queryCommandState("JustifyLeft");

        private void JustifyLeft()
        {
            _domDocument.execCommand("JustifyLeft");
        }

        private bool IsJustifyRight => _domDocument.queryCommandState("JustifyRight");

        private void JustifyRight()
        {
            _domDocument.execCommand("JustifyRight");
        }

        private bool IsJustifyCenter => _domDocument.queryCommandState("JustifyCenter");

        private void JustifyCenter()
        {
            _domDocument.execCommand("JustifyCenter");
        }

        private bool IsJustifyFull => _domDocument.queryCommandState("JustifyFull");

        private void JustifyFull()
        {
            _domDocument.execCommand("JustifyFull");
        }

        private bool IsBold => _domDocument.queryCommandState("Bold");

        private void Bold()
        {
            _domDocument.execCommand("Bold");
        }

        private bool IsItalic => _domDocument.queryCommandState("Italic");

        private void Italic()
        {
            _domDocument.execCommand("Italic");
        }

        private bool IsUnderline => _domDocument.queryCommandState("Underline");

        private void Underline()
        {
            _domDocument.execCommand("Underline");
        }

        public bool CanDelete => _domDocument.queryCommandEnabled("Delete");

        public void Delete()
        {
            _domDocument.execCommand("Delete");
        }

        private void InsertImage()
        {
            _domDocument.execCommand("InsertImage", true);
        }

        private void Indent()
        {
            _domDocument.execCommand("Indent");
        }

        private void Outdent()
        {
            _domDocument.execCommand("Outdent");
        }

        private bool CanInsertLink
        {
            get
            {
                if (SelectionType != SelectionType.Text)
                    return false;
                if (_domDocument.selection.createRange() is IHTMLTxtRange txtRange
                    && !string.IsNullOrEmpty(txtRange.htmlText))
                {
                    MatchCollection lstMatches = Regex.Matches(txtRange.htmlText,
                        "<a href=\"[^\"]+\">[^<]+</a>",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    return lstMatches.Count <= 1;
                }
                return true;
            }
        }

        private void CreateLink(string url)
        {
            _domDocument.execCommand("CreateLink", false, url);
        }

        private void SelectLink()
        {
            string strUrl = string.Empty;
            switch (SelectionType)
            {
                case SelectionType.Control:
                    {
                        if (_domDocument.selection.createRange() is IHTMLControlRange objRange
                            && objRange.length > 0)
                        {
                            IHTMLElement eleSelected = objRange.item(0);
                            if (eleSelected != null
                                && string.Equals(eleSelected.tagName, "img", StringComparison.OrdinalIgnoreCase))
                            {
                                eleSelected = eleSelected.parentElement;
                                if (eleSelected != null
                                    && string.Equals(eleSelected.tagName, "a", StringComparison.OrdinalIgnoreCase))
                                {
                                    strUrl = eleSelected.getAttribute("href") as string;
                                }
                            }
                        }
                    }
                    break;
                case SelectionType.Text:
                    {
                        if (_domDocument.selection.createRange() is IHTMLTxtRange txtRange
                            && !string.IsNullOrEmpty(txtRange.htmlText))
                        {
                            Match match = Regex.Match(txtRange.htmlText,
                                "^\\s*<a href=\"([^\"]+)\">[^<]+</a>\\s*$",
                                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                            if (match.Success)
                                strUrl = match.Groups[1].Value;
                        }
                    }
                    break;
            }
            using (frmSelectText frmLink = new frmSelectText
            {
                Description = "Enter a URL",
                DefaultString = strUrl
            })
            {
                frmLink.ShowDialog(ParentForm);
                if (frmLink.DialogResult == DialogResult.Cancel)
                    return;
                if (!Uri.TryCreate(frmLink.SelectedValue, UriKind.Absolute, out Uri _))
                {
                    MessageBox.Show(this.ParentForm, "Invalid URL");
                    return;
                }
                CreateLink(frmLink.SelectedValue);
            }
        }

        #region Control Methods

        private void tsbBold_Click(object sender, EventArgs e)
        {
            Bold();
        }

        private void tsbItalic_Click(object sender, EventArgs e)
        {
            Italic();
        }

        private void tsbUnderline_Click(object sender, EventArgs e)
        {
            Underline();
        }

        private void tsbUnorderedList_Click(object sender, EventArgs e)
        {
            UnorderedList();
        }

        private void tsbOrderedList_Click(object sender, EventArgs e)
        {
            OrderedList();
        }

        private void webContent_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _domDocument.body?.setAttribute("contentEditable", "true");
            Document.AttachEventHandler("onkeyup", UpdateButtons);
            Document.AttachEventHandler("onmouseup", UpdateButtons);
        }

        private void webContent_GotFocus(object sender, EventArgs e)
        {
            Document?.Body?.Focus();
        }

        private void webContent_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            SetBackgroundColor(BackColor);
        }

        private void tsbIncreaseIndent_Click(object sender, EventArgs e)
        {
            Indent();
        }

        private void tsbDecreaseIndent_Click(object sender, EventArgs e)
        {
            Outdent();
        }

        private void tsbImage_Click(object sender, EventArgs e)
        {
            InsertImage();
        }

        private void tsbAlignLeft_Click(object sender, EventArgs e)
        {
            JustifyLeft();
        }

        private void tsbAlignCenter_Click(object sender, EventArgs e)
        {
            JustifyCenter();
        }

        private void tsbAlignRight_Click(object sender, EventArgs e)
        {
            JustifyRight();
        }

        private void tsbAlignJustify_Click(object sender, EventArgs e)
        {
            JustifyFull();
        }

        private void tsbForeColor_Click(object sender, EventArgs e)
        {
            SelectForeColor();
        }

        private void tsbBackColor_Click(object sender, EventArgs e)
        {
            SelectBackColor();
        }

        private void cboFont_Leave(object sender, EventArgs e)
        {
            if (_blnSkipUpdate)
                return;
            FontFamily objNewFont;
            try
            {
                objNewFont = new FontFamily(cboFont.Text);
            }
            catch (Exception)
            {
                _blnSkipUpdate = true;
                cboFont.Text = FontName.GetName(0);
                _blnSkipUpdate = false;
                return;
            }
            FontName = objNewFont;
        }

        private void cboFontSize_Leave(object sender, EventArgs e)
        {
            if (_blnSkipUpdate)
                return;
            switch (cboFontSize.Text.Trim())
            {
                case "1":
                    FontSize = FontSize.One;
                    break;
                case "2":
                    FontSize = FontSize.Two;
                    break;
                case "3":
                    FontSize = FontSize.Three;
                    break;
                case "4":
                    FontSize = FontSize.Four;
                    break;
                case "5":
                    FontSize = FontSize.Five;
                    break;
                case "6":
                    FontSize = FontSize.Six;
                    break;
                case "7":
                    FontSize = FontSize.Seven;
                    break;
                default:
                    FontSize = FontSize.Seven;
                    break;
            }
        }

        private void HtmlEditor_Enter(object sender, EventArgs e)
        {
            tsControls.Visible = true;
        }

        private void HtmlEditor_Leave(object sender, EventArgs e)
        {
            tsControls.Visible = false;
        }

        private void tsbHyperlink_Click(object sender, EventArgs e)
        {
            SelectLink();
        }

        private void cboFontSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
                if (e.KeyChar <= '7' && e.KeyChar > '0')
                    cboFontSize.Text = e.KeyChar.ToString();
            }
            else if (!char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}
