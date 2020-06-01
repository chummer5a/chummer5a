using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

    public enum Headings
    {
        H1 = 1,
        H2,
        H3,
        H4,
        H5,
        H6
    }

    public partial class HtmlEditor : UserControl
    {
        private readonly IHTMLDocument2 _domDocument;

        public HtmlEditor()
        {
            InitializeComponent();
            webContent.DocumentText = @"<html><body></body></html>";
            _domDocument = webContent.Document?.DomDocument as IHTMLDocument2;
            if (_domDocument == null)
                throw new ArgumentNullException(nameof(webContent.Document));
            _domDocument.designMode = "On";
            if (Document.Body != null)
            {
                Document.Body.GotFocus += BodyOnGotFocus;
                Document.Body.LostFocus += BodyOnLostFocus;
            }
        }

        private void BodyOnLostFocus(object sender, HtmlElementEventArgs e)
        {
            tsControls.Visible = false;
        }

        private void BodyOnGotFocus(object sender, HtmlElementEventArgs e)
        {
            tsControls.Visible = true;
        }

        /// <summary>
        /// Get the ready state of the internal browser component.
        /// </summary>
        public ReadyState ReadyState
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

        public HtmlDocument Document => webContent.Document;

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
            get => webContent.Document?.Body?.InnerHtml ?? string.Empty;
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
            get => Document?.Body?.InnerHtml ?? string.Empty;
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

        /// <summary>
        /// Determine the status of the Undo command in the document editor.
        /// </summary>
        /// <returns>whether or not an undo operation is currently valid</returns>
        public bool CanUndo()
        {
            return _domDocument.queryCommandEnabled("Undo");
        }

        /// <summary>
        /// Determine the status of the Redo command in the document editor.
        /// </summary>
        /// <returns>whether or not a redo operation is currently valid</returns>
        public bool CanRedo()
        {
            return _domDocument.queryCommandEnabled("Redo");
        }

        /// <summary>
        /// Determine the status of the Cut command in the document editor.
        /// </summary>
        /// <returns>whether or not a cut operation is currently valid</returns>
        public bool CanCut()
        {
            return _domDocument.queryCommandEnabled("Cut");
        }

        /// <summary>
        /// Determine the status of the Copy command in the document editor.
        /// </summary>
        /// <returns>whether or not a copy operation is currently valid</returns>
        public bool CanCopy()
        {
            return _domDocument.queryCommandEnabled("Copy");
        }

        /// <summary>
        /// Determine the status of the Paste command in the document editor.
        /// </summary>
        /// <returns>whether or not a copy operation is currently valid</returns>
        public bool CanPaste()
        {
            return _domDocument.queryCommandEnabled("Paste");
        }

        /// <summary>
        /// Determine the status of the Delete command in the document editor.
        /// </summary>
        /// <returns>whether or not a copy operation is currently valid</returns>
        public bool CanDelete()
        {
            return _domDocument.queryCommandEnabled("Delete");
        }

        /// <summary>
        /// Determine whether the current block is left justified.
        /// </summary>
        /// <returns>true if left justified, otherwise false</returns>
        public bool IsJustifyLeft()
        {
            return _domDocument.queryCommandState("JustifyLeft");
        }

        /// <summary>
        /// Determine whether the current block is right justified.
        /// </summary>
        /// <returns>true if right justified, otherwise false</returns>
        public bool IsJustifyRight()
        {
            return _domDocument.queryCommandState("JustifyRight");
        }

        /// <summary>
        /// Determine whether the current block is center justified.
        /// </summary>
        /// <returns>true if center justified, false otherwise</returns>
        public bool IsJustifyCenter()
        {
            return _domDocument.queryCommandState("JustifyCenter");
        }

        /// <summary>
        /// Determine whether the current block is full justified.
        /// </summary>
        /// <returns>true if full justified, false otherwise</returns>
        public bool IsJustifyFull()
        {
            return _domDocument.queryCommandState("JustifyFull");
        }

        /// <summary>
        /// Determine whether the current selection is in Bold mode.
        /// </summary>
        /// <returns>whether or not the current selection is Bold</returns>
        public bool IsBold()
        {
            return _domDocument.queryCommandState("Bold");
        }

        /// <summary>
        /// Determine whether the current selection is in Italic mode.
        /// </summary>
        /// <returns>whether or not the current selection is Italicized</returns>
        public bool IsItalic()
        {
            return _domDocument.queryCommandState("Italic");
        }

        /// <summary>
        /// Determine whether the current selection is in Underline mode.
        /// </summary>
        /// <returns>whether or not the current selection is Underlined</returns>
        public bool IsUnderline()
        {
            return _domDocument.queryCommandState("Underline");
        }

        /// <summary>
        /// Determine whether the current paragraph is an ordered list.
        /// </summary>
        /// <returns>true if current paragraph is ordered, false otherwise</returns>
        public bool IsOrderedList()
        {
            return _domDocument.queryCommandState("InsertOrderedList");
        }

        /// <summary>
        /// Determine whether the current paragraph is an unordered list.
        /// </summary>
        /// <returns>true if current paragraph is ordered, false otherwise</returns>
        public bool IsUnorderedList()
        {
            return _domDocument.queryCommandState("InsertUnorderedList");
        }

        /// <summary>
        /// Insert a paragraph break
        /// </summary>
        public void InsertParagraph()
        {
            _domDocument.execCommand("InsertParagraph");
        }

        /// <summary>
        /// Insert a horizontal rule
        /// </summary>
        public void InsertBreak()
        {
            _domDocument.execCommand("InsertHorizontalRule");
        }

        /// <summary>
        /// Select all text in the document.
        /// </summary>
        public void SelectAll()
        {
            _domDocument.execCommand("SelectAll");
        }

        /// <summary>
        /// Undo the last operation
        /// </summary>
        public void Undo()
        {
            _domDocument.execCommand("Undo");
        }

        /// <summary>
        /// Redo based on the last Undo
        /// </summary>
        public void Redo()
        {
            _domDocument.execCommand("Redo");
        }

        /// <summary>
        /// Cut the current selection and place it in the clipboard.
        /// </summary>
        public void Cut()
        {
            _domDocument.execCommand("Cut");
        }

        /// <summary>
        /// Paste the contents of the clipboard into the current selection.
        /// </summary>
        public void Paste()
        {
            _domDocument.execCommand("Paste");
        }

        /// <summary>
        /// Copy the current selection into the clipboard.
        /// </summary>
        public void Copy()
        {
            _domDocument.execCommand("Copy");
        }

        /// <summary>
        /// Toggle the ordered list property for the current paragraph.
        /// </summary>
        public void OrderedList()
        {
            _domDocument.execCommand("InsertOrderedList");
        }

        /// <summary>
        /// Toggle the unordered list property for the current paragraph.
        /// </summary>
        public void UnorderedList()
        {
            _domDocument.execCommand("InsertUnorderedList");
        }

        /// <summary>
        /// Toggle the left justify property for the current block.
        /// </summary>
        public void JustifyLeft()
        {
            _domDocument.execCommand("JustifyLeft");
        }

        /// <summary>
        /// Toggle the right justify property for the current block.
        /// </summary>
        public void JustifyRight()
        {
            _domDocument.execCommand("JustifyRight");
        }

        /// <summary>
        /// Toggle the center justify property for the current block.
        /// </summary>
        public void JustifyCenter()
        {
            _domDocument.execCommand("JustifyCenter");
        }

        /// <summary>
        /// Toggle the full justify property for the current block.
        /// </summary>
        public void JustifyFull()
        {
            _domDocument.execCommand("JustifyFull");
        }

        /// <summary>
        /// Toggle bold formatting on the current selection.
        /// </summary>
        public void Bold()
        {
            _domDocument.execCommand("Bold");
        }

        /// <summary>
        /// Toggle italic formatting on the current selection.
        /// </summary>
        public void Italic()
        {
            _domDocument.execCommand("Italic");
        }

        /// <summary>
        /// Toggle underline formatting on the current selection.
        /// </summary>
        public void Underline()
        {
            _domDocument.execCommand("Underline");
        }

        /// <summary>
        /// Delete the current selection.
        /// </summary>
        public void Delete()
        {
            _domDocument.execCommand("Delete");
        }

        /// <summary>
        /// Insert an image.
        /// </summary>
        public void InsertImage()
        {
            _domDocument.execCommand("InsertImage", true);
        }

        /// <summary>
        /// Indent the current paragraph.
        /// </summary>
        public void Indent()
        {
            _domDocument.execCommand("Indent");
        }

        /// <summary>
        /// Outdent the current paragraph.
        /// </summary>
        public void Outdent()
        {
            _domDocument.execCommand("Outdent");
        }

        /// <summary>
        /// Insert a link at the current selection.
        /// </summary>
        /// <param name="url">The link url</param>
        public void InsertLink(string url)
        {
            _domDocument.execCommand("CreateLink", false, url);
        }

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
        }

        private void webContent_GotFocus(object sender, EventArgs e)
        {
            Document?.Body?.Focus();
        }
    }
}
