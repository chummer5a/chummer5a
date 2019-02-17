using System;
using System.Windows.Forms;

namespace Chummer.helpers
{
    internal class Label : System.Windows.Forms.Label
    {
        private readonly ToolTip _tt = new ToolTip();

        public string TooltipText { get; set; }

        public Label()
        {
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
            if (string.IsNullOrEmpty(this.TooltipText)) return;
            _tt.SetToolTip(this, this.TooltipText);
            _tt.Show(this.TooltipText, this.Parent);
        }
        private void Label_MouseLeave(object sender, EventArgs ea)
        {
            _tt.Hide(this);
        }
    }
}