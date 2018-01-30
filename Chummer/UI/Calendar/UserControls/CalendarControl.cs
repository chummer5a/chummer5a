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
        private Character _characterObject;

        public CalendarControl()
        {
            InitializeComponent();
        }

        private void DoBindings()
        {
            calendar1.DataBindings.Add("ViewStart", this, nameof(StartDate), false,
                DataSourceUpdateMode.OnPropertyChanged);
            calendar1.DataBindings.Add("ViewEnd", this, nameof(EndDate), false,
                DataSourceUpdateMode.OnPropertyChanged);
            //calendar1.DataBindings.Add("Items", CharacterObject, nameof(CharacterObject.Calendar), false,
            //    DataSourceUpdateMode.OnPropertyChanged);
            monthView1.DataBindings.Add("ViewStart", this, nameof(StartDate), false,
                DataSourceUpdateMode.OnPropertyChanged);
        }
        public void AddRange(BindingList<CalendarItem> calendarObjects)
        {
            foreach (CalendarItem c in calendarObjects)
            {
                c.Calendar = calendar1;
            }
            Items.AddRange(calendarObjects);
            StartDate = Items.Select(o => o.EndDate).Max();
            calendar1.SetViewRange(StartDate,EndDate);
        }
        private void calendar1_LoadItems(object sender, CalendarLoadEventArgs e)
        {
            PlaceItems();
        }

        private void PlaceItems()
        {
            foreach (CalendarItem calendarItem in Items)
            {
                if (calendar1.ViewIntersects(calendarItem))
                    calendar1.Items.Add(calendarItem);
                if (monthView1.ViewIntersects(calendarItem))
                    monthView1.Items.Add(calendarItem);
            }
        }

        private void calendar1_ItemCreated(object sender, CalendarItemCancelEventArgs e)
        {
            CharacterObject.Calendar.Add(e.Item);
            Items.Add(e.Item);
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
            calendar1.TimeScale = CalendarTimeScale.SixtyMinutes;
        }

        private void minutesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            calendar1.TimeScale = CalendarTimeScale.ThirtyMinutes;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            calendar1.TimeScale = CalendarTimeScale.FifteenMinutes;
        }

        private void minutesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            calendar1.TimeScale = CalendarTimeScale.SixMinutes;
        }

        private void minutesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            calendar1.TimeScale = CalendarTimeScale.TenMinutes;
        }

        private void minutesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            calendar1.TimeScale = CalendarTimeScale.FiveMinutes;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //this.contextItem = this.calendar1.ItemAt(this.contextMenuStrip1.Bounds.Location);
        }

        private void redTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Red);
                calendar1.Invalidate(selectedItem);
            }
        }

        private void yellowTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Gold);
                calendar1.Invalidate(selectedItem);
            }
        }

        private void greenTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.Green);
                calendar1.Invalidate(selectedItem);
            }
        }

        private void blueTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ApplyColor(Color.DarkBlue);
                calendar1.Invalidate(selectedItem);
            }
        }

        private void editItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            calendar1.ActivateEditMode();
        }

        private void otherColorTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() != DialogResult.OK)
                    return;
                foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
                {
                    selectedItem.ApplyColor(colorDialog.Color);
                    calendar1.Invalidate(selectedItem);
                }
            }
        }

        private void calendar1_ItemDoubleClick(object sender, CalendarItemEventArgs e)
        {
            int num = (int)MessageBox.Show("Double click: " + e.Item.Text);
        }

        private void calendar1_ItemDeleted(object sender, CalendarItemEventArgs e)
        {
            calendar1.Items.Remove(e.Item);
            CharacterObject.Calendar.Remove(e.Item);
        }

        private void calendar1_DayHeaderClick(object sender, CalendarDayEventArgs e)
        {
            calendar1.SetViewRange(e.CalendarDay.Date, e.CalendarDay.Date);
        }

        private void diagonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.ForwardDiagonal;
                selectedItem.PatternColor = Color.Red;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.Vertical;
                selectedItem.PatternColor = Color.Red;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.Horizontal;
                selectedItem.PatternColor = Color.Red;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void hatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.DiagonalCross;
                selectedItem.PatternColor = Color.Red;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.Pattern = HatchStyle.DiagonalCross;
                selectedItem.PatternColor = Color.Empty;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void monthView1_SelectionChanged(object sender, EventArgs e)
        {
            calendar1.SetViewRange(monthView1.SelectionStart, monthView1.SelectionStart.AddDays(calendar1.MaximumViewDays - 1));
        }

        private void northToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.North;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void eastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.East;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void southToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.South;
                calendar1.Invalidate(selectedItem);
            }
        }

        private void westToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
            {
                selectedItem.ImageAlign = CalendarItemImageAlign.West;
                calendar1.Invalidate(selectedItem);
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
                foreach (CalendarItem selectedItem in calendar1.GetSelectedItems())
                {
                    selectedItem.Image = image;
                    calendar1.Invalidate(selectedItem);
                }
            }
        }

        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);

        public DateTime EndDate => StartDate.AddMonths(1);

        public bool InCharacter { get; set; } = true;

        public Character CharacterObject
        {
            get => _characterObject;
            set
            {
                _characterObject = value; 
                DoBindings();
            }
        }

        public List<CalendarItem> Items { get; set; } = new List<CalendarItem>();
    }
}
