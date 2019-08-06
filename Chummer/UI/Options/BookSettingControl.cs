using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options
{
    public partial class BookSettingControl : UserControl
    {
        private OptionGroup _book;
        private Dictionary<string, OptionEntryProxy> _childFields;
        private PathSelectorControl _pathSelector;

        public BookSettingControl()
        {
            InitializeComponent();
            _pathSelector = new PathSelectorControl();
            _pathSelector.Location = new Point(83, 47);
            Controls.Add(_pathSelector);
        }

        public OptionGroup Book
        {
            get { return _book; }
            set
            {
                _book = value;
                DoBindings();
            }
        }

        private void DoBindings()
        {
            OptionDictionaryEntryProxy<string, bool> enabledProxy =
                _book.Children.OfType<OptionDictionaryEntryProxy<string, bool>>().First();

            _childFields =
                _book.Children.OfType<OptionEntryProxy>().ToDictionary(x => x.TargetProperty.Name);


            
            chkEnabled.DataBindings.Add(nameof(CheckBox.Checked), enabledProxy,
                nameof(OptionDictionaryEntryProxy<string, bool>.Value), false, DataSourceUpdateMode.OnPropertyChanged);

            _pathSelector.PathEntry = _childFields["Path"];

            chkEnabled.Text = LanguageManager.GetString("Label_Enabled");
            lblName.Text = _childFields["Name"].Value as string;
            lblOffset.Text = LanguageManager.GetString("Label_Options_PDFOffset");
            lblPath.Text = LanguageManager.GetString("Label_Options_PDFLocation");
            btnTest.Text = LanguageManager.GetString("Button_Options_PDFTest");

        }

        

        private void btnTest_Click(object sender, EventArgs e)
        {
            int page = 0;
            if (int.TryParse(_childFields["Offset"].Value?.ToString() ?? "0", out page))
            {
                page += 5;
                string val = _childFields["Code"].Value + " " + page;

                CommonFunctions.OpenPDF(val);

            }
        }
    }
}
