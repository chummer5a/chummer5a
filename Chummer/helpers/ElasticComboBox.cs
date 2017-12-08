using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    internal class ElasticComboBox : ComboBox
    {
        private readonly ToolTip _tt;
        private readonly Graphics _objGraphics;

        private string _strToolTipText = string.Empty;
        public string TooltipText
        {
            get => _strToolTipText;
            set
            {
                if (_strToolTipText != value)
                {
                    _strToolTipText = value;
                    if (!string.IsNullOrEmpty(value))
                        _tt.SetToolTip(this, value);
                }
            }
        }

        public ElasticComboBox() : this(null) { }

        public ElasticComboBox(ToolTip objToolTip) : base()
        {
            _tt = objToolTip;
            if (_tt == null)
            {
                _tt = new ToolTip
                {
                    AutoPopDelay = 1500,
                    InitialDelay = 400,
                    UseAnimation = true,
                    UseFading = true,
                    Active = true
                };
            }
            
            MouseEnter += Label_MouseEnter;
            MouseLeave += Label_MouseLeave;

            _objGraphics = CreateGraphics();
        }

        private void Label_MouseEnter(object sender, EventArgs ea)
        {
            if (!string.IsNullOrEmpty(TooltipText))
            {
                _tt.Show(TooltipText, Parent);
            }
        }
        private void Label_MouseLeave(object sender, EventArgs ea)
        {
            _tt.Hide(this);
        }

        public new object DataSource
        {
            get { return base.DataSource; }
            set
            {
                if (base.DataSource != value)
                {
                    base.DataSource = value;
                    ResizeDropDown();
                }
            }
        }
        public new string DisplayMember
        {
            get { return base.DisplayMember; }
            set
            {
                if (base.DisplayMember != value)
                {
                    base.DisplayMember = value;
                    ResizeDropDown();
                }
            }
        }
        public new string ValueMember
        {
            get { return base.ValueMember; }
            set
            {
                if (base.ValueMember != value)
                {
                    base.ValueMember = value;
                    ResizeDropDown();
                }
            }
        }

        private void ResizeDropDown()
        {
            float fltMaxItemWidth = Width;
            foreach (var objItem in Items)
            {
                string strItemText = (objItem as ListItem)?.Name ?? GetItemText(objItem);
                float fltLoopItemWidth = _objGraphics.MeasureString(strItemText, Font).Width;
                if (fltLoopItemWidth > fltMaxItemWidth)
                    fltMaxItemWidth = fltLoopItemWidth;
            }
            DropDownWidth = Convert.ToInt32(Math.Ceiling(fltMaxItemWidth));
        }
    }
}
