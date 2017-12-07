using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Chummer.helpers
{
    internal class ElasticComboBox : ComboBox
    {
        private readonly ToolTip _tt;

        private string _strToolTipText = string.Empty;
        public string TooltipText
        {
            get => _strToolTipText;
            set
            {
                if (_strToolTipText == value) return;
                _strToolTipText = value;
                if (!string.IsNullOrEmpty(value))
                    _tt.SetToolTip(this, value);
            }
        }

        public ElasticComboBox() : base()
        {
            MouseDown += ResizeDropdown;
            _tt = new ToolTip
            {
                AutoPopDelay = 1500,
                InitialDelay = 400,
                UseAnimation = true,
                UseFading = true,
                Active = true
            };
            
            MouseEnter += Label_MouseEnter;
            MouseLeave += Label_MouseLeave;
        }

        private void Label_MouseEnter(object sender, EventArgs ea)
        {
            TooltipText = Text;
            _tt.Show(TooltipText, Parent);
        }
        private void Label_MouseLeave(object sender, EventArgs ea)
        {
            _tt.Hide(this);
        }

        private void ResizeDropdown(object sender, MouseEventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            if (senderComboBox == null) return;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;

            width = (from ListItem l in ((ComboBox) sender).Items select (int) g.MeasureString(l.Name, font).Width).Concat(new[] {width}).Max();
            senderComboBox.DropDownWidth = width;
        }
    }
}
