using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options
{
    public class BookControl : UserControl
    {
        //Behavour state
        internal const int SCALE = 2;
        private int _layoutedForWidth = 0;
        private string _selectedBook = null;

        //Control references
        private Panel _bookPanel;
        private Panel _detailPanel;
        private BookSettingControl _activeBookControl = null;
        private OptionRender _optionRender;
        private readonly Dictionary<string, PictureBox> _pictures = new Dictionary<string, PictureBox>();
        private readonly Dictionary<string, BookSettingControl> _bookControls = new Dictionary<string, BookSettingControl>();

        //All options that options manage with some groups
        private readonly OptionCollectionCache _options;

        //Values used for display layout
        private const int IMAGE_HEIGHT = 360;
        private int IMAGE_BORDER => Program.BookImageManager.GlowBorder;
        private const int IMAGE_WIDTH = 278;
        private readonly int _checkboxHeight;
        private readonly int _checkboxWidth;
        
        //.ctor & Initialization
        public BookControl(OptionCollectionCache options)
        {
            _options = options;
            using (Image checkbox = Properties.Resources.checkbox_checked)
            {
                _checkboxHeight = checkbox.Height;
                _checkboxWidth = checkbox.Width;
            }

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _bookPanel = new Panel();
            _detailPanel = new Panel()
            {
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            Controls.Add(_detailPanel);

            Controls.Add(_bookPanel);

            _bookPanel.Location = new Point(0, 0);
            //TODO: make height lower if we want stuff visible.

            _bookPanel.AutoScroll = true;


            _optionRender = new OptionRender()
            {
                Factories = _options.ControlFactories
            };

            System.Diagnostics.Stopwatch sw = Stopwatch.StartNew();
            foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
            {
                PictureBox bookBox = new PictureBox { Tag = book.Code };
                _bookPanel.Controls.Add(bookBox);
                bookBox.SizeMode = PictureBoxSizeMode.AutoSize;
                bookBox.Image = Program.BookImageManager.GetImage(book.Code, _options.BookEnabled[book.Code].Value, false, SCALE);

                bookBox.MouseEnter += Picture_MouseEnter;
                bookBox.MouseLeave += Picture_MouseLeave;
                bookBox.Click += Picture_Click;
                bookBox.DoubleClick += Picture_DoubleClick;

                _pictures.Add(book.Code, bookBox);
            }
            Console.WriteLine("Created books in {0} ms", sw.ElapsedMilliseconds);

            Resize += OnResize;
            _bookPanel.MouseEnter += BookPanel_OnMouseEnter;

            foreach (OptionDictionaryEntryProxy<string, bool> thing in _options.BookEnabled.Values)
            {
                thing.ValueChanged += () =>
                {
                    _pictures[thing.Key].Image = Program.BookImageManager.GetImage(thing.Key, _options.BookEnabled[thing.Key].Value, false, SCALE);
                };
            }
        }

        ////API reacting on other things than events
        //public int Scale
        //{
        //    get { return _scale; }
        //    set
        //    {
        //        _scale = value;
        //        ResetAllImages();
        //        PerformBookLayout();
        //    }
        //}

        public void Save()
        {

        }

        //Behavour state change
        private void ToggleSelectBook(string code)
        {
            if (_selectedBook == code)
            {
                _selectedBook = null;
                ResizeForHiddenDetailPanel();
            }
            else
            {
                _selectedBook = code;
                SetDetailPanelToBook(code);
                ResizeForShowDetailPanel();
            }
        }

        private void ToggleBookEnabled(PictureBox box)
        {

            string bookCode = box.Tag as string;
            _options.BookEnabled[bookCode].Value = !_options.BookEnabled[bookCode].Value;

            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, true, SCALE);
        }

        //Display update
        private void ResetAllImages()
        {
            foreach (PictureBox picture in _bookPanel.Controls)
            {
                string tag = (string) picture.Tag;
                picture.Image = Program.BookImageManager.GetImage(tag, _options.BookEnabled[tag].Value, false, SCALE);
            }
        }

        private void ResizeForShowDetailPanel()
        {
            if (_activeBookControl != null)
            {
                _optionRender.Location = new Point(0, _activeBookControl.Right);
                _optionRender.Width = Width - _activeBookControl.Right;
                _optionRender.Height = _activeBookControl.Height;

                _detailPanel.Location = new Point(0, Height - _activeBookControl.Height);
                _detailPanel.Size = new Size(Width, _activeBookControl.Height);

            }

            _bookPanel.Size = new Size(this.Width - SystemInformation.VerticalScrollBarWidth,
                this.Height - _detailPanel.Height);

            _detailPanel.Visible = true;
        }

        private void ResizeForHiddenDetailPanel()
        {
            _bookPanel.Size = new Size(this.Width - SystemInformation.VerticalScrollBarWidth, this.Height);
            _detailPanel.Visible = false;
        }

        private void PerformBookLayout()
        {
            int xw = (IMAGE_WIDTH + IMAGE_BORDER ) / SCALE;
            int yw = (IMAGE_HEIGHT + IMAGE_BORDER) / SCALE;
            
            int booksPerRow = Math.Max((_bookPanel.Width - (IMAGE_BORDER / SCALE)) / xw, 1);

            if (booksPerRow != Math.Max((_layoutedForWidth - (IMAGE_BORDER / SCALE)) / xw, 1))
            {
                for (int i = 0; i < _bookPanel.Controls.Count; i++)
                {
                    int column = i % booksPerRow;
                    int row = i / booksPerRow;

                    _bookPanel.Controls[i].Location = new Point(column * xw, row * yw);
                    _bookPanel.Controls[i].Size = new Size(xw + (IMAGE_BORDER / SCALE), yw + (IMAGE_BORDER / SCALE));
                }
            }

            _layoutedForWidth = _bookPanel.Width;
        }

        private void SetDetailPanelToBook(string code)
        {
            if (_activeBookControl != null) _activeBookControl.Visible = false;
            BookSettingControl control;
            if (_bookControls.TryGetValue(code, out control))
            {
                _activeBookControl = control;
                _activeBookControl.Visible = true;
            }
            else
            {
                _activeBookControl = new BookSettingControl()
                {
                    Location = new Point(0, 0),
                    Book = _options.Books[code]
                };
                _bookControls[code] = _activeBookControl;
                _detailPanel.Controls.Add(_activeBookControl);
            }

            _optionRender.SetContents(
                _options.NotBookOptions
                    .Where(x => x.Tags.Any(y => y == "OPTIONALRULE+" + code))
                    .Select<OptionItem, OptionRenderItem>(x => x)
                    .ToList()
            );
        }

        //Control event handlers
        private void Picture_Click(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if (box == null)
            {
                Utils.BreakIfDebug();
                return;
            }

            MouseEventArgs me = e as MouseEventArgs;
            if (me != null && IsInCheckboxArear(me, box))
            {
                ToggleBookEnabled(box);
            }
            else
            {
                ToggleSelectBook(box.Tag as string);
            }
        }

        private void Picture_DoubleClick(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if (box == null)
            {
                Utils.BreakIfDebug();
                return;
            }

            ToggleBookEnabled(box);
        }
        
        private void Picture_MouseLeave(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, false, SCALE);
        }

        private void Picture_MouseEnter(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            _bookPanel.Controls.SetChildIndex(box, 0);
            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, true, SCALE);
        }

        private void BookPanel_OnMouseEnter(object sender, EventArgs eventArgs)
        {
            _bookPanel.Focus();
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (_selectedBook == null)
            {
                ResizeForHiddenDetailPanel();
            }
            else
            {
                ResizeForShowDetailPanel();
            }

            if (_bookPanel.Width != _layoutedForWidth)
            {
                PerformBookLayout();
            }

            //_bookPanel.Size = new Size(this.Width - SystemInformation.VerticalScrollBarWidth, this.Height);
            //_optionRender.Size = new Size(this.Width - _activeBookControl.Width,
            //    this.Height - (IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight));
        }

        //Event handler helper methods
        private bool IsInCheckboxArear(MouseEventArgs me, PictureBox pictuture)
        {
            bool xinside = (IMAGE_BORDER + _checkboxWidth) / SCALE > me.X && me.X > IMAGE_BORDER / SCALE;
            bool yinside = pictuture.Image.Height - (IMAGE_BORDER / SCALE) > me.Y && me.Y > pictuture.Image.Height - ((IMAGE_BORDER + _checkboxHeight) / SCALE);
            return xinside && yinside;
        }


        

    }
}
