using System;
using System.Windows.Forms;

namespace Chummer
{
    internal class ButtonWithToolTip : Button
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

        public ButtonWithToolTip(ToolTip objToolTip = null) : base()
        {
            _tt = objToolTip;
            if (_tt == null)
                _tt = new ToolTip();
            _tt.AutoPopDelay = 1500;
            _tt.InitialDelay = 400;
            _tt.UseAnimation = true;
            _tt.UseFading = true;
            _tt.Active = true;
            this.MouseEnter += this.Label_MouseEnter;
            this.MouseLeave += this.Label_MouseLeave;
        }

        private void Label_MouseEnter(object sender, EventArgs ea)
        {
            if (!string.IsNullOrEmpty(this.TooltipText))
            {
                _tt.Show(this.TooltipText, this.Parent);
            }
        }
        private void Label_MouseLeave(object sender, EventArgs ea)
        {
            _tt.Hide(this);
        }
    }
}
