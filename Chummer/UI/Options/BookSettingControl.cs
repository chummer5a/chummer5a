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

        public BookSettingControl()
        {
            InitializeComponent();
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

            Dictionary<string, OptionEntryProxy> otherProxies =
                _book.Children.OfType<OptionEntryProxy>().ToDictionary(x => x.TargetProperty.Name);

            
            chkEnabled.DataBindings.Add(nameof(CheckBox.Checked), enabledProxy,
                nameof(OptionDictionaryEntryProxy<string, bool>.Value), false, DataSourceUpdateMode.OnPropertyChanged);
            
            

            chkEnabled.Text = LanguageManager.Instance.GetString("Label_Enabled");
            lblName.Text = otherProxies["Name"].Value as string;
            lblOffset.Text = LanguageManager.Instance.GetString("Label_Options_PDFOffset");
            lblPath.Text = LanguageManager.Instance.GetString("Label_Options_PDFLocation");
            btnTest.Text = LanguageManager.Instance.GetString("Button_Options_PDFTest");

        }

        private void btnChangePath_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
