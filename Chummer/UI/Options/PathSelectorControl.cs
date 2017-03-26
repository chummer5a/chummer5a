using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
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
        private FolderBrowserDialog browserDialog;
        private OptionEntryProxy _pathEntry;
        private IsPathAttribute metaAttribute;

        public PathSelectorControl()
        {
            InitializeComponent();
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            if (_pathEntry == null) return;

            DialogResult result = ShowDialog();

            if (result != DialogResult.OK) return;

            string read = GetResult();

            _pathEntry.Value = read;
            UpdateDisplayString(read);
        }

        private string GetResult()
        {
            if (dialog != null) return dialog.FileName;

            return browserDialog.SelectedPath;
        }

        private DialogResult ShowDialog()
        {
            if (dialog != null) return dialog.ShowDialog();

            return browserDialog.ShowDialog();
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


                metaAttribute = _pathEntry.TargetProperty.GetCustomAttribute<IsPathAttribute>();
                
                UpdateDisplayString(_pathEntry.Value?.ToString());
                
                if (metaAttribute.Folder)
                {
                    browserDialog = new FolderBrowserDialog();
                }
                else
                {
                    dialog = new OpenFileDialog();
                    if (!string.IsNullOrWhiteSpace(metaAttribute.Filter))
                    {
                        dialog.Filter = metaAttribute.Filter;
                    }
                }
                

            }
        }
    }
}
