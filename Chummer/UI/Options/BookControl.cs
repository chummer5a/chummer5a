using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chummer.Backend;
using Chummer.Backend.Options;

namespace Chummer.UI.Options
{
    public class BookControl : UserControl
    {
        private readonly OptionCollectionCache _options;
        //private Dictionary<string, OptionGroup> _books;
        //private Dictionary<string, OptionDictionaryEntryProxy<string, bool>> _enabled;
        private Dictionary<string, PictureBox> _pictures = new Dictionary<string, PictureBox>();
        private Dictionary<string, BookSettingControl> _bookControls = new Dictionary<string, BookSettingControl>();
        private BookSettingControl _activeBookControl = null;
        private OptionRender _optionRender;
        private const int IMAGE_HEIGHT = 360;
        private const int IMAGE_BORDER = 20;  //THIS IS ALSO USED IN BookImageManager.cs as a seperate value. Would need change both places or strange stuff might happen
        private const int IMAGE_WIDTH = 278;

        private int CheckboxHeight;
        private int CheckboxWidth;
        private Panel _bookPanel;
        private int _scale = 1;

        public int Scale

        {
            get { return _scale; }
            set
            {
                _scale = value;
                ResetAllImages();
                PerformBookLayout();
            }
        }

        private void ResetAllImages()
        {
            foreach (PictureBox picture in _bookPanel.Controls)
            {
                string tag = (string) picture.Tag;
                picture.Image = Program.BookImageManager.GetImage(tag, _options.BookEnabled[tag].Value, false, _scale);
            }
        }

        public BookControl(OptionCollectionCache options)
        {
            _options = options;
            Image checkbox = Properties.Resources.checkbox_checked;
            CheckboxHeight = checkbox.Height;
            CheckboxWidth = checkbox.Width;

            InitializeComponents();

            foreach (OptionDictionaryEntryProxy<string,bool> thing in _options.BookEnabled.Values)
            {
                thing.ValueChanged += () =>
                {
                    _pictures[thing.Key].Image = Program.BookImageManager.GetImage(thing.Key, _options.BookEnabled[thing.Key].Value, false, _scale);
                };
            }
        }

        private void InitializeComponents()
        {
            _bookPanel = new Panel();
            this.Controls.Add(_bookPanel);

            _bookPanel.Location = new Point(0, 18);
            _bookPanel.Size = new Size(this.Width, IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight);
            _bookPanel.AutoScroll = true;


            foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
            {
                PictureBox bookBox = new PictureBox{ Tag = book.Code};
                _bookPanel.Controls.Add(bookBox);
                bookBox.SizeMode = PictureBoxSizeMode.AutoSize;
                bookBox.Image = Program.BookImageManager.GetImage(book.Code, _options.BookEnabled[book.Code].Value, false, _scale);

                bookBox.MouseEnter += Picture_MouseEnter;
                bookBox.MouseLeave += Picture_MouseLeave;
                bookBox.Click += Picture_Click;
                bookBox.DoubleClick += Picture_DoubleClick;

                _pictures.Add(book.Code,  bookBox);
            }

            PerformBookLayout();

            this.Resize += OnResize;
            _bookPanel.MouseWheel += BookPanel_OnMouseWheel;
            _bookPanel.MouseEnter += BookPanel_OnMouseEnter;
            _activeBookControl = new BookSettingControl()
            {
                Location = new Point(0,IMAGE_HEIGHT + IMAGE_BORDER * 2+ SystemInformation.HorizontalScrollBarHeight + 18), Book = _options.Books["SR5"]
            };
            Controls.Add(_activeBookControl);

            _optionRender = new OptionRender()
            {
                Location = new Point(
                    _activeBookControl.Width,
                    IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight + 18
                ),
                Factories = _options.ControlFactories
            };
            ;
            Controls.Add(_optionRender);

            _optionRender.SetContents(
                _options.NotBookOptions
                .Where(x => x.Tags.Any(y => y == "OPTIONALRULE+SR5"))
                .Select<OptionItem, OptionRenderItem>(x => x)
                .ToList()
            );

            CheckBox b = new CheckBox(){Text = "Small", Location = new Point(2,2)};
            b.CheckedChanged += (sender, args) => { Scale = b.Checked ? 2 : 1; };
            Controls.Add(b);
        }

        private void PerformBookLayout()
        {
            int xw = (IMAGE_WIDTH + IMAGE_BORDER * 2) / _scale;
            int yw = (IMAGE_HEIGHT + IMAGE_BORDER * 2) / _scale;
            //_bookPanel.AutoScrollMinSize = new Size(GlobalOptions.Instance.SourcebookInfo.Count * (IMAGE_BORDER+IMAGE_WIDTH) + IMAGE_BORDER, IMAGE_HEIGHT + IMAGE_BORDER * 2);

            for (int i = 0; i < _bookPanel.Controls.Count; i++)
            {
                _bookPanel.Controls[i].Location = new Point((i/_scale) *xw, (i%_scale)*yw);
                _bookPanel.Controls[i].Size = new Size(xw, yw);
            }
        }

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
                ToggleBook(box);
            }
            else
            {
                SelectBook(box.Tag as string);
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

            ToggleBook(box);
        }

        private void SelectBook(string code)
        {
            _activeBookControl.Visible = false;
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
                    Location = new Point(0,IMAGE_HEIGHT + IMAGE_BORDER * 2+ SystemInformation.HorizontalScrollBarHeight), Book = _options.Books[code]
                };
                _bookControls[code] = _activeBookControl;
                Controls.Add(_activeBookControl);
            }

            _optionRender.SetContents(
                _options.NotBookOptions
                    .Where(x => x.Tags.Any(y => y == "OPTIONALRULE+" + code))
                    .Select<OptionItem, OptionRenderItem>(x => x)
                    .ToList()
            );
        }

        private bool IsInCheckboxArear(MouseEventArgs me, PictureBox pictuture)
        {
            bool xinside = (IMAGE_BORDER + CheckboxHeight) / _scale > me.X && me.X > IMAGE_BORDER / _scale;
            bool yinside = pictuture.Image.Height - (IMAGE_BORDER / _scale)> me.Y && me.Y > pictuture.Image.Height - ((IMAGE_BORDER + CheckboxHeight) / _scale);
            return xinside && yinside;
        }

        private void ToggleBook(PictureBox box)
        {

            string bookCode = box.Tag as string;
            _options.BookEnabled[bookCode].Value = !_options.BookEnabled[bookCode].Value;

            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, true, _scale);
        }



        private void Picture_MouseLeave(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, false, _scale);
        }

        private void Picture_MouseEnter(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            _bookPanel.Controls.SetChildIndex(box, 0);
            box.Image = Program.BookImageManager.GetImage(bookCode, _options.BookEnabled[bookCode].Value, true, _scale);
        }

        private void BookPanel_OnMouseEnter(object sender, EventArgs eventArgs)
        {
            _bookPanel.Focus();
        }

        private void BookPanel_OnMouseWheel(object sender, MouseEventArgs mouseEventArgs)
        {
            _bookPanel.HorizontalScroll.Value = Math.Min(_bookPanel.HorizontalScroll.Maximum - _bookPanel.Width,
                Math.Max(_bookPanel.HorizontalScroll.Minimum, _bookPanel.HorizontalScroll.Value + (mouseEventArgs.Delta * -1)));
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            _bookPanel.Size = new Size(this.Width, IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight);
            _optionRender.Size = new Size(this.Width - _activeBookControl.Width,
                this.Height - (IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight));
        }

        public void Save()
        {

        }


    }
}