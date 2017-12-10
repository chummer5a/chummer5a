using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsCalendar;

namespace Chummer
{
    partial class CalendarControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            CalendarHighlightRange calendarHighlightRange1 = new CalendarHighlightRange();
            CalendarHighlightRange calendarHighlightRange2 = new CalendarHighlightRange();
            CalendarHighlightRange calendarHighlightRange3 = new CalendarHighlightRange();
            CalendarHighlightRange calendarHighlightRange4 = new CalendarHighlightRange();
            CalendarHighlightRange calendarHighlightRange5 = new CalendarHighlightRange();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.redTagToolStripMenuItem = new ToolStripMenuItem();
            this.yellowTagToolStripMenuItem = new ToolStripMenuItem();
            this.greenTagToolStripMenuItem = new ToolStripMenuItem();
            this.blueTagToolStripMenuItem = new ToolStripMenuItem();
            this.otherColorTagToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripSeparator();
            this.patternToolStripMenuItem = new ToolStripMenuItem();
            this.diagonalToolStripMenuItem = new ToolStripMenuItem();
            this.verticalToolStripMenuItem = new ToolStripMenuItem();
            this.horizontalToolStripMenuItem = new ToolStripMenuItem();
            this.hatchToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem3 = new ToolStripSeparator();
            this.noneToolStripMenuItem = new ToolStripMenuItem();
            this.timescaleToolStripMenuItem = new ToolStripMenuItem();
            this.hourToolStripMenuItem = new ToolStripMenuItem();
            this.minutesToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem4 = new ToolStripMenuItem();
            this.minutesToolStripMenuItem1 = new ToolStripMenuItem();
            this.minutesToolStripMenuItem2 = new ToolStripMenuItem();
            this.minutesToolStripMenuItem3 = new ToolStripMenuItem();
            this.toolStripMenuItem2 = new ToolStripSeparator();
            this.editItemToolStripMenuItem = new ToolStripMenuItem();
            this.splitter1 = new Splitter();
            this.calendar1 = new Calendar();
            this.monthView1 = new MonthView();
            this.toolStripMenuItem5 = new ToolStripSeparator();
            this.selectImageToolStripMenuItem = new ToolStripMenuItem();
            this.imageAlignmentToolStripMenuItem = new ToolStripMenuItem();
            this.northToolStripMenuItem = new ToolStripMenuItem();
            this.eastToolStripMenuItem = new ToolStripMenuItem();
            this.southToolStripMenuItem = new ToolStripMenuItem();
            this.westToolStripMenuItem = new ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[13]
            {
                (ToolStripItem) this.redTagToolStripMenuItem,
                (ToolStripItem) this.yellowTagToolStripMenuItem,
                (ToolStripItem) this.greenTagToolStripMenuItem,
                (ToolStripItem) this.blueTagToolStripMenuItem,
                (ToolStripItem) this.otherColorTagToolStripMenuItem,
                (ToolStripItem) this.toolStripMenuItem1,
                (ToolStripItem) this.patternToolStripMenuItem,
                (ToolStripItem) this.timescaleToolStripMenuItem,
                (ToolStripItem) this.toolStripMenuItem2,
                (ToolStripItem) this.selectImageToolStripMenuItem,
                (ToolStripItem) this.imageAlignmentToolStripMenuItem,
                (ToolStripItem) this.toolStripMenuItem5,
                (ToolStripItem) this.editItemToolStripMenuItem
            });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(167, 264);
            this.contextMenuStrip1.Opening += new CancelEventHandler(this.contextMenuStrip1_Opening);
            this.redTagToolStripMenuItem.Name = "redTagToolStripMenuItem";
            this.redTagToolStripMenuItem.Size = new Size(166, 22);
            this.redTagToolStripMenuItem.Text = "Red tag";
            this.redTagToolStripMenuItem.Click += new EventHandler(this.redTagToolStripMenuItem_Click);
            this.yellowTagToolStripMenuItem.Name = "yellowTagToolStripMenuItem";
            this.yellowTagToolStripMenuItem.Size = new Size(166, 22);
            this.yellowTagToolStripMenuItem.Text = "Yellow tag";
            this.yellowTagToolStripMenuItem.Click += new EventHandler(this.yellowTagToolStripMenuItem_Click);
            this.greenTagToolStripMenuItem.Name = "greenTagToolStripMenuItem";
            this.greenTagToolStripMenuItem.Size = new Size(166, 22);
            this.greenTagToolStripMenuItem.Text = "Green tag";
            this.greenTagToolStripMenuItem.Click += new EventHandler(this.greenTagToolStripMenuItem_Click);
            this.blueTagToolStripMenuItem.Name = "blueTagToolStripMenuItem";
            this.blueTagToolStripMenuItem.Size = new Size(166, 22);
            this.blueTagToolStripMenuItem.Text = "Blue tag";
            this.blueTagToolStripMenuItem.Click += new EventHandler(this.blueTagToolStripMenuItem_Click);
            this.otherColorTagToolStripMenuItem.Name = "otherColorTagToolStripMenuItem";
            this.otherColorTagToolStripMenuItem.Size = new Size(166, 22);
            this.otherColorTagToolStripMenuItem.Text = "Other color tag...";
            this.otherColorTagToolStripMenuItem.Click += new EventHandler(this.otherColorTagToolStripMenuItem_Click);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(163, 6);
            this.patternToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[6]
            {
        (ToolStripItem) this.diagonalToolStripMenuItem,
        (ToolStripItem) this.verticalToolStripMenuItem,
        (ToolStripItem) this.horizontalToolStripMenuItem,
        (ToolStripItem) this.hatchToolStripMenuItem,
        (ToolStripItem) this.toolStripMenuItem3,
        (ToolStripItem) this.noneToolStripMenuItem
            });
            this.patternToolStripMenuItem.Name = "patternToolStripMenuItem";
            this.patternToolStripMenuItem.Size = new Size(166, 22);
            this.patternToolStripMenuItem.Text = "Pattern";
            this.diagonalToolStripMenuItem.Name = "diagonalToolStripMenuItem";
            this.diagonalToolStripMenuItem.Size = new Size(129, 22);
            this.diagonalToolStripMenuItem.Text = "Diagonal";
            this.diagonalToolStripMenuItem.Click += new EventHandler(this.diagonalToolStripMenuItem_Click);
            this.verticalToolStripMenuItem.Name = "verticalToolStripMenuItem";
            this.verticalToolStripMenuItem.Size = new Size(129, 22);
            this.verticalToolStripMenuItem.Text = "Vertical";
            this.verticalToolStripMenuItem.Click += new EventHandler(this.verticalToolStripMenuItem_Click);
            this.horizontalToolStripMenuItem.Name = "horizontalToolStripMenuItem";
            this.horizontalToolStripMenuItem.Size = new Size(129, 22);
            this.horizontalToolStripMenuItem.Text = "Horizontal";
            this.horizontalToolStripMenuItem.Click += new EventHandler(this.horizontalToolStripMenuItem_Click);
            this.hatchToolStripMenuItem.Name = "hatchToolStripMenuItem";
            this.hatchToolStripMenuItem.Size = new Size(129, 22);
            this.hatchToolStripMenuItem.Text = "Cross";
            this.hatchToolStripMenuItem.Click += new EventHandler(this.hatchToolStripMenuItem_Click);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new Size(126, 6);
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new Size(129, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new EventHandler(this.noneToolStripMenuItem_Click);
            this.timescaleToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[6]
            {
        (ToolStripItem) this.hourToolStripMenuItem,
        (ToolStripItem) this.minutesToolStripMenuItem,
        (ToolStripItem) this.toolStripMenuItem4,
        (ToolStripItem) this.minutesToolStripMenuItem1,
        (ToolStripItem) this.minutesToolStripMenuItem2,
        (ToolStripItem) this.minutesToolStripMenuItem3
            });
            this.timescaleToolStripMenuItem.Name = "timescaleToolStripMenuItem";
            this.timescaleToolStripMenuItem.Size = new Size(166, 22);
            this.timescaleToolStripMenuItem.Text = "Timescale";
            this.hourToolStripMenuItem.Name = "hourToolStripMenuItem";
            this.hourToolStripMenuItem.Size = new Size(132, 22);
            this.hourToolStripMenuItem.Text = "1 hour";
            this.hourToolStripMenuItem.Click += new EventHandler(this.hourToolStripMenuItem_Click);
            this.minutesToolStripMenuItem.Name = "minutesToolStripMenuItem";
            this.minutesToolStripMenuItem.Size = new Size(132, 22);
            this.minutesToolStripMenuItem.Text = "30 minutes";
            this.minutesToolStripMenuItem.Click += new EventHandler(this.minutesToolStripMenuItem_Click);
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new Size(132, 22);
            this.toolStripMenuItem4.Text = "15 minutes";
            this.toolStripMenuItem4.Click += new EventHandler(this.toolStripMenuItem4_Click);
            this.minutesToolStripMenuItem1.Name = "minutesToolStripMenuItem1";
            this.minutesToolStripMenuItem1.Size = new Size(132, 22);
            this.minutesToolStripMenuItem1.Text = "10 minutes";
            this.minutesToolStripMenuItem1.Click += new EventHandler(this.minutesToolStripMenuItem1_Click);
            this.minutesToolStripMenuItem2.Name = "minutesToolStripMenuItem2";
            this.minutesToolStripMenuItem2.Size = new Size(132, 22);
            this.minutesToolStripMenuItem2.Text = "6 minutes";
            this.minutesToolStripMenuItem2.Click += new EventHandler(this.minutesToolStripMenuItem2_Click);
            this.minutesToolStripMenuItem3.Name = "minutesToolStripMenuItem3";
            this.minutesToolStripMenuItem3.Size = new Size(132, 22);
            this.minutesToolStripMenuItem3.Text = "5 minutes";
            this.minutesToolStripMenuItem3.Click += new EventHandler(this.minutesToolStripMenuItem3_Click);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new Size(163, 6);
            this.editItemToolStripMenuItem.Name = "editItemToolStripMenuItem";
            this.editItemToolStripMenuItem.Size = new Size(166, 22);
            this.editItemToolStripMenuItem.Text = "Edit item's text";
            this.editItemToolStripMenuItem.Click += new EventHandler(this.editItemToolStripMenuItem_Click);
            this.splitter1.Location = new Point(208, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new Size(5, 342);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            this.calendar1.ContextMenuStrip = this.contextMenuStrip1;
            this.calendar1.Dock = DockStyle.Fill;
            this.calendar1.Font = new Font("Segoe UI", 9f);
            calendarHighlightRange1.DayOfWeek = DayOfWeek.Monday;
            calendarHighlightRange1.EndTime = TimeSpan.Parse("17:00:00");
            calendarHighlightRange1.StartTime = TimeSpan.Parse("08:00:00");
            calendarHighlightRange2.DayOfWeek = DayOfWeek.Tuesday;
            calendarHighlightRange2.EndTime = TimeSpan.Parse("17:00:00");
            calendarHighlightRange2.StartTime = TimeSpan.Parse("08:00:00");
            calendarHighlightRange3.DayOfWeek = DayOfWeek.Wednesday;
            calendarHighlightRange3.EndTime = TimeSpan.Parse("17:00:00");
            calendarHighlightRange3.StartTime = TimeSpan.Parse("08:00:00");
            calendarHighlightRange4.DayOfWeek = DayOfWeek.Thursday;
            calendarHighlightRange4.EndTime = TimeSpan.Parse("17:00:00");
            calendarHighlightRange4.StartTime = TimeSpan.Parse("08:00:00");
            calendarHighlightRange5.DayOfWeek = DayOfWeek.Friday;
            calendarHighlightRange5.EndTime = TimeSpan.Parse("17:00:00");
            calendarHighlightRange5.StartTime = TimeSpan.Parse("08:00:00");
            this.calendar1.HighlightRanges = new CalendarHighlightRange[5]
            {
        calendarHighlightRange1,
        calendarHighlightRange2,
        calendarHighlightRange3,
        calendarHighlightRange4,
        calendarHighlightRange5
            };
            this.calendar1.Location = new Point(213, 0);
            this.calendar1.Name = "calendar1";
            this.calendar1.Size = new Size(458, 342);
            this.calendar1.TabIndex = 2;
            this.calendar1.Text = "calendar1";
            this.calendar1.ItemDeleted += new Calendar.CalendarItemEventHandler(this.calendar1_ItemDeleted);
            this.calendar1.ItemClick += new Calendar.CalendarItemEventHandler(this.calendar1_ItemClick);
            this.calendar1.DayHeaderClick += new Calendar.CalendarDayEventHandler(this.calendar1_DayHeaderClick);
            this.calendar1.ItemCreated += new Calendar.CalendarItemCancelEventHandler(this.calendar1_ItemCreated);
            this.calendar1.ItemDoubleClick += new Calendar.CalendarItemEventHandler(this.calendar1_ItemDoubleClick);
            this.calendar1.LoadItems += new Calendar.CalendarLoadEventHandler(this.calendar1_LoadItems);
            this.calendar1.ItemMouseHover += new Calendar.CalendarItemEventHandler(this.calendar1_ItemMouseHover);
            this.monthView1.ArrowsColor = SystemColors.Window;
            this.monthView1.ArrowsSelectedColor = Color.Gold;
            this.monthView1.DayBackgroundColor = Color.Empty;
            this.monthView1.DayGrayedText = SystemColors.GrayText;
            this.monthView1.DaySelectedBackgroundColor = SystemColors.Highlight;
            this.monthView1.DaySelectedColor = SystemColors.WindowText;
            this.monthView1.DaySelectedTextColor = SystemColors.HighlightText;
            this.monthView1.Dock = DockStyle.Left;
            this.monthView1.ItemPadding = new Padding(2);
            this.monthView1.Location = new Point(0, 0);
            this.monthView1.MaxSelectionCount = 35;
            this.monthView1.MonthTitleColor = SystemColors.ActiveCaption;
            this.monthView1.MonthTitleColorInactive = SystemColors.InactiveCaption;
            this.monthView1.MonthTitleTextColor = SystemColors.ActiveCaptionText;
            this.monthView1.MonthTitleTextColorInactive = SystemColors.InactiveCaptionText;
            this.monthView1.Name = "monthView1";
            this.monthView1.Size = new Size(208, 342);
            this.monthView1.TabIndex = 3;
            this.monthView1.Text = "monthView1";
            this.monthView1.TodayBorderColor = Color.Maroon;
            this.monthView1.SelectionChanged += new EventHandler(this.monthView1_SelectionChanged);
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new Size(163, 6);
            this.selectImageToolStripMenuItem.Name = "selectImageToolStripMenuItem";
            this.selectImageToolStripMenuItem.Size = new Size(166, 22);
            this.selectImageToolStripMenuItem.Text = "Select Image...";
            this.selectImageToolStripMenuItem.Click += new EventHandler(this.selectImageToolStripMenuItem_Click);
            this.imageAlignmentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[4]
            {
        (ToolStripItem) this.northToolStripMenuItem,
        (ToolStripItem) this.eastToolStripMenuItem,
        (ToolStripItem) this.southToolStripMenuItem,
        (ToolStripItem) this.westToolStripMenuItem
            });
            this.imageAlignmentToolStripMenuItem.Name = "imageAlignmentToolStripMenuItem";
            this.imageAlignmentToolStripMenuItem.Size = new Size(166, 22);
            this.imageAlignmentToolStripMenuItem.Text = "Image Alignment";
            this.northToolStripMenuItem.Name = "northToolStripMenuItem";
            this.northToolStripMenuItem.Size = new Size(152, 22);
            this.northToolStripMenuItem.Text = "North";
            this.northToolStripMenuItem.Click += new EventHandler(this.northToolStripMenuItem_Click);
            this.eastToolStripMenuItem.Name = "eastToolStripMenuItem";
            this.eastToolStripMenuItem.Size = new Size(152, 22);
            this.eastToolStripMenuItem.Text = "East";
            this.eastToolStripMenuItem.Click += new EventHandler(this.eastToolStripMenuItem_Click);
            this.southToolStripMenuItem.Name = "southToolStripMenuItem";
            this.southToolStripMenuItem.Size = new Size(152, 22);
            this.southToolStripMenuItem.Text = "South";
            this.southToolStripMenuItem.Click += new EventHandler(this.southToolStripMenuItem_Click);
            this.westToolStripMenuItem.Name = "westToolStripMenuItem";
            this.westToolStripMenuItem.Size = new Size(152, 22);
            this.westToolStripMenuItem.Text = "West";
            this.westToolStripMenuItem.Click += new EventHandler(this.westToolStripMenuItem_Click);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(671, 342);
            this.Controls.Add((Control)this.calendar1);
            this.Controls.Add((Control)this.splitter1);
            this.Controls.Add((Control)this.monthView1);
            this.Name = "calControl";
            this.Text = "Calendar Demo";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private Calendar calendar1;
        private MonthView monthView1;
        private Splitter splitter1;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem selectImageToolStripMenuItem;
        private ToolStripMenuItem imageAlignmentToolStripMenuItem;
        private ToolStripMenuItem northToolStripMenuItem;
        private ToolStripMenuItem eastToolStripMenuItem;
        private ToolStripMenuItem southToolStripMenuItem;
        private ToolStripMenuItem westToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem redTagToolStripMenuItem;
        private ToolStripMenuItem yellowTagToolStripMenuItem;
        private ToolStripMenuItem greenTagToolStripMenuItem;
        private ToolStripMenuItem blueTagToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem timescaleToolStripMenuItem;
        private ToolStripMenuItem hourToolStripMenuItem;
        private ToolStripMenuItem minutesToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem editItemToolStripMenuItem;
        private ToolStripMenuItem minutesToolStripMenuItem1;
        private ToolStripMenuItem minutesToolStripMenuItem2;
        private ToolStripMenuItem minutesToolStripMenuItem3;
        private ToolStripMenuItem otherColorTagToolStripMenuItem;
        private ToolStripMenuItem patternToolStripMenuItem;
        private ToolStripMenuItem diagonalToolStripMenuItem;
        private ToolStripMenuItem verticalToolStripMenuItem;
        private ToolStripMenuItem horizontalToolStripMenuItem;
        private ToolStripMenuItem hatchToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem noneToolStripMenuItem;
    }
}
