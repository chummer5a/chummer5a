using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Chummer.Backend;

namespace Chummer.UI.Options
{
    public class BookControl : UserControl
    {
        private readonly HashSet<string> _enabledBooks;
        private const int IMAGE_HEIGHT = 351;
        private const int IMAGE_BORDER = 20;  //THIS IS ALSO USED IN BookImageManager.cs as a seperate value. Would need change both places or strange stuff might happen
        private const int IMAGE_WIDTH = 248;


        public BookControl(HashSet<string> enabledBooks)
        {
            _enabledBooks = new HashSet<string>(enabledBooks);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _bookPanel = new Panel();
            this.Controls.Add(_bookPanel);

            _bookPanel.Location = new Point(0, 0);
            _bookPanel.Size = new Size(this.Width, IMAGE_HEIGHT + IMAGE_BORDER * 2 + SystemInformation.HorizontalScrollBarHeight);
            _bookPanel.AutoScroll = true;
            _bookPanel.AutoScrollMinSize = new Size(GlobalOptions.Instance.SourcebookInfo.Count * (IMAGE_BORDER+IMAGE_WIDTH) + IMAGE_BORDER, IMAGE_HEIGHT + IMAGE_BORDER * 2);

            foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
            {
                PictureBox bookBox = new PictureBox{ Tag = book.Code};
                _bookPanel.Controls.Add(bookBox);
                bookBox.SizeMode = PictureBoxSizeMode.AutoSize;
                bookBox.Size = new Size(IMAGE_WIDTH + IMAGE_BORDER*2, IMAGE_HEIGHT + IMAGE_BORDER * 2);
                bookBox.Location = new Point(((_bookPanel.Controls.Count -1)* (IMAGE_BORDER *2 + IMAGE_WIDTH)), 0);
                bookBox.Image = Program.BookImageManager.GetImage(book.Code, _enabledBooks.Contains(book.Code), false);

                bookBox.MouseEnter += Picture_MouseEnter;
                bookBox.MouseLeave += Picture_MouseLeave;
                bookBox.Click += Picture_Click;
                bookBox.DoubleClick += Picture_DoubleClick;
            }

            this.Resize += OnResize;
            _bookPanel.MouseWheel += BookPanel_OnMouseWheel;
            _bookPanel.MouseEnter += BookPanel_OnMouseEnter;

        }

        private void Picture_Click(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            _enabledBooks.Toggle(bookCode);

            box.Image = Program.BookImageManager.GetImage(bookCode, _enabledBooks.Contains(bookCode), true);

        }

        private void Picture_DoubleClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Picture_MouseLeave(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            box.Image = Program.BookImageManager.GetImage(bookCode, _enabledBooks.Contains(bookCode), false);
        }

        private void Picture_MouseEnter(object sender, EventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if(box == null){Utils.BreakIfDebug(); return;}

            string bookCode = box.Tag as string;

            box.Image = Program.BookImageManager.GetImage(bookCode, _enabledBooks.Contains(bookCode), true);
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
        }

        public void Save()
        {

        }

        private Panel _bookPanel;
    }
}