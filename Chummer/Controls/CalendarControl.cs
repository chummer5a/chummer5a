using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsCalendar;

namespace Chummer
{
    public partial class CalendarControl : UserControl
    {
        private List<CalendarItem> _calItems = new List<CalendarItem>();
        public CalendarControl()
        {
            InitializeComponent();
            calendar1.DataBindings.Add("ViewStart", this, nameof(StartDate), false, DataSourceUpdateMode.OnPropertyChanged);
            calendar1.DataBindings.Add("ViewEnd", this, nameof(EndDate), false, DataSourceUpdateMode.OnPropertyChanged);
            monthView1.DataBindings.Add("ViewStart", this, nameof(StartDate), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public void AddRange(List<CalendarItem> calendarObjects)
        {
            foreach (CalendarItem c in calendarObjects)
            {
                c.Calendar = calendar1;
            }
            _calItems.AddRange(calendarObjects);
            StartDate = _calItems.Select(o => o.EndDate).Max();
            calendar1.SetViewRange(StartDate,EndDate);
        }
        private void calendar1_LoadItems(object sender, CalendarLoadEventArgs e)
        {
            this.PlaceItems();
        }

        private void PlaceItems()
        {
            foreach (CalendarItem calendarItem in _calItems)
            {
                if (calendar1.ViewIntersects(calendarItem))
                    calendar1.Items.Add(calendarItem);
            }
        }

        private void calendar1_ItemCreated(object sender, CalendarItemCancelEventArgs e)
        {
            _calItems.Add(e.Item);
        }

        private void calendar1_ItemMouseHover(object sender, CalendarItemEventArgs e)
        {
            Text = e.Item.Text;
        }

        private void calendar1_ItemClick(object sender, CalendarItemEventArgs e)
        {
        }

        private void hourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.SixtyMinutes;
        }

        private void minutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.ThirtyMinutes;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.FifteenMinutes;
        }

        private void minutesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.SixMinutes;
        }

        private void minutesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.TenMinutes;
        }

        private void minutesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.calendar1.TimeScale = CalendarTimeScale.FiveMinutes;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //this.contextItem = this.calendar1.ItemAt(this.contextMenuStrip1.Bounds.Location);
        }

        private void redTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Red);
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void yellowTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Gold);
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void greenTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Green);
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void blueTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.DarkBlue);
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void editItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.calendar1.ActivateEditMode();
        }

        private void DemoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*List<ItemInfo> itemInfoList = new List<ItemInfo>();
            foreach (CalendarItem calendarItem in this._items)
                itemInfoList.Add(new ItemInfo(calendarItem.StartDate, calendarItem.EndDate, calendarItem.Text, calendarItem.BackgroundColor));
            XmlSerializer xmlSerializer = new XmlSerializer(itemInfoList.GetType());
            if (this.ItemsFile.Exists)
                this.ItemsFile.Delete();
            using (Stream stream = (Stream)this.ItemsFile.OpenWrite())
            {
                xmlSerializer.Serialize(stream, (object)itemInfoList);
                stream.Close();
            }*/
        }

        private void otherColorTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() != DialogResult.OK)
                    return;
                foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
                {
                    selectedItem.ApplyColor(colorDialog.Color);
                    this.calendar1.Invalidate(selectedItem);
                }
            }
        }

        private void calendar1_ItemDoubleClick(object sender, CalendarItemEventArgs e)
        {
            int num = (int)MessageBox.Show("Double click: " + e.Item.Text);
        }

        private void calendar1_ItemDeleted(object sender, CalendarItemEventArgs e)
        {
            this.calendar1.Items.Remove(e.Item);
        }

        private void calendar1_DayHeaderClick(object sender, CalendarDayEventArgs e)
        {
            this.calendar1.SetViewRange(e.CalendarDay.Date, e.CalendarDay.Date);
        }

        private void diagonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.ForwardDiagonal;
                selectedItem.PatternColor = Color.Red;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.Vertical;
                selectedItem.PatternColor = Color.Red;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.Horizontal;
                selectedItem.PatternColor = Color.Red;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void hatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.DiagonalCross;
                selectedItem.PatternColor = Color.Red;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.DiagonalCross;
                selectedItem.PatternColor = Color.Empty;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void monthView1_SelectionChanged(object sender, EventArgs e)
        {
            this.calendar1.SetViewRange(this.monthView1.SelectionStart, this.monthView1.SelectionEnd);
        }

        private void northToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.North;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void eastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.East;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void southToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.South;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void westToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.West;
                this.calendar1.Invalidate(selectedItem);
            }
        }

        private void selectImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "*.gif|*.gif|*.png|*.png|*.jpg|*.jpg";
                if (openFileDialog.ShowDialog((IWin32Window)this) != DialogResult.OK)
                    return;
                Image image = Image.FromFile(openFileDialog.FileName);
                foreach (CalendarItem selectedItem in this.calendar1.GetSelectedItems())
                {
                    selectedItem.Image = image;
                    this.calendar1.Invalidate(selectedItem);
                }
            }
        }

        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);

        public DateTime EndDate => StartDate.AddMonths(1);

        public bool InCharacter { get; set; } = true;
    }
}
