using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Options;

namespace Chummer.UI.Options
{
    public partial class PathSelectorControl : UserControl
    {
        private FileDialog dialog;
        private OptionEntryProxy _pathEntry;
        private IsPathAttribute metaAttribute;

        public PathSelectorControl()
        {
            InitializeComponent();
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            if (_pathEntry == null) return;

            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;

            _pathEntry.Value = dialog.FileName;
            UpdateDisplayString(dialog.FileName);
        }

        private void UpdateDisplayString(string value)
        {
            txtDisplay.Text = value;
        }

        public OptionEntryProxy PathEntry
        {
            get { return _pathEntry; }
            set
            {
                _pathEntry = value;
                UpdateDisplayString(_pathEntry.Value.ToString());
                dialog = new OpenFileDialog();
                metaAttribute = _pathEntry.TargetProperty.GetCustomAttribute<IsPathAttribute>();

                if (!string.IsNullOrWhiteSpace(metaAttribute.Filter))
                {
                    dialog.Filter = metaAttribute.Filter;
                }

            }
        }
    }
}
