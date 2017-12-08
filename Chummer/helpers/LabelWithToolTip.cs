using System;
using System.Windows.Forms;

namespace Chummer
{
    internal class LabelWithToolTip : Label
    {
        private readonly ToolTip _tt;

        private string _strToolTipText = string.Empty;
        public string TooltipText
        {
            get
            {
                return _strToolTipText;
            }
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

        public LabelWithToolTip() : this(null) { }

        public LabelWithToolTip(ToolTip objToolTip) : base()
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
    }
}
