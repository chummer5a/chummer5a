using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Chummer
{
    public partial class ColorableCheckBox : CheckBox
    {
        // CheckBox doesn't let you control its colors when it is disabled, so this extended class exists as a hacky version of implementing that
        // It does so by never actually disabling the CheckBox control, instead just controlling the AutoCheck property and coloring the foreground accordingly
        public ColorableCheckBox() : this(null)
        {
        }

        public ColorableCheckBox(IContainer container)
        {
            container?.Add(this);

            InitializeComponent();
            FlatStyle = FlatStyle.Flat; // Flat checkboxes' borders obey ForeColor
            FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
            FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
            BackColorChanged += OnBackColorChanged;
        }

        private void OnBackColorChanged(object sender, EventArgs e)
        {
            if (!Enabled)
            {
                FlatAppearance.MouseDownBackColor = BackColor;
                FlatAppearance.MouseOverBackColor = BackColor;
            }
        }

        private bool _blnRealEnabled = true;

        public new bool Enabled
        {
            get => _blnRealEnabled;
            set
            {
                if (_blnRealEnabled == value)
                    return;
                AutoCheck = _blnRealEnabled = value;
                if (value)
                {
                    ForeColor = ColorManager.ControlText;
                    FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
                    FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
                }
                else
                {
                    ForeColor = ColorManager.GrayText;
                    FlatAppearance.MouseDownBackColor = BackColor;
                    FlatAppearance.MouseOverBackColor = BackColor;
                }
            }
        }
    }
}
