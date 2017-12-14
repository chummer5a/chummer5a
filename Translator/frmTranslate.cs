using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
// ReSharper disable LocalizableElement

namespace Translator
{
    public partial class frmTranslate : Form
    {
        private bool _blnLoading;
        private readonly XmlDocument _objDataDoc = new XmlDocument();
        private readonly XmlDocument _objTranslationDoc = new XmlDocument();
        private readonly string _strCode = string.Empty;
        private readonly string _strLanguage = string.Empty;
        private readonly string _strPath = string.Empty;

        [Obsolete("This constructor is for use by form designers only.", true)]
        public frmTranslate()
        {
            InitializeComponent();
        }

        public frmTranslate(string strLanguage)
        {
            _strPath = Application.StartupPath;
            _strLanguage = strLanguage;
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            _strCode = Language.Substring(Language.IndexOf('(') + 1, 5);

            InitializeComponent();

            _workerSectionLoader.WorkerReportsProgress = false;
            _workerSectionLoader.WorkerSupportsCancellation = true;
            _workerSectionLoader.DoWork += LoadSections;
            _workerSectionLoader.RunWorkerCompleted += FinishLoadingSections;
        }

        #region Control Events
        private void chkOnlyTranslation_CheckedChanged(object sender, EventArgs e)
        {
            // ReSharper disable once LocalizableElement
            if (cboFile.Text == "Strings")
            {
                LoadStrings();
                return;
            }
            LoadSection();
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (dgvSection.Visible)
            {
                int rowCount = dgvSection.RowCount;
                int columnCount = dgvSection.ColumnCount;
                int rowIndex = dgvSection.SelectedCells[0].RowIndex;
                int columnIndex = dgvSection.SelectedCells[0].ColumnIndex;
                for (int i = rowIndex; i <= rowCount; i++)
                    for (int j = 0; j <= columnCount; j++)
                        if (((i > rowIndex) || (j > columnIndex)) &&
                            (dgvSection.Rows[i].Cells[j].Value.ToString()
                                 .IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1))
                        {
                            dgvSection.ClearSelection();
                            dgvSection.Rows[i].Cells[j].Selected = true;
                            dgvSection.FirstDisplayedScrollingRowIndex = i;
                            dgvSection.Select();
                            Cursor = Cursors.Default;
                            return;
                        }
                for (int k = 0; k < rowIndex; k++)
                    for (int l = 0; l <= columnCount; l++)
                        if (
                            dgvSection.Rows[k].Cells[l].Value.ToString()
                                .IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            dgvSection.ClearSelection();
                            dgvSection.Rows[k].Cells[l].Selected = true;
                            dgvSection.FirstDisplayedScrollingRowIndex = k;
                            dgvSection.Select();
                            Cursor = Cursors.Default;
                            return;
                        }
                Cursor = Cursors.Default;
                MessageBox.Show("Search text was not found.");
                return;
            }
            int num = dgvTranslate.RowCount;
            int columnCount1 = dgvTranslate.ColumnCount;
            int rowIndex1 = dgvTranslate.SelectedCells[0].RowIndex;
            int columnIndex1 = dgvTranslate.SelectedCells[0].ColumnIndex;
            for (int m = rowIndex1; m < num; m++)
                for (int n = 0; n < columnCount1; n++)
                    if (((m > rowIndex1) || (n > columnIndex1)) &&
                        (dgvTranslate.Rows[m].Cells[n].Value.ToString()
                             .IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1))
                    {
                        dgvTranslate.ClearSelection();
                        dgvTranslate.Rows[m].Cells[n].Selected = true;
                        dgvTranslate.FirstDisplayedScrollingRowIndex = m;
                        dgvTranslate.Select();
                        Cursor = Cursors.Default;
                        return;
                    }
            for (int o = 0; o < rowIndex1; o++)
                for (int p = 0; p < columnCount1; p++)
                    if (
                        dgvTranslate.Rows[o].Cells[p].Value.ToString()
                            .IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        dgvTranslate.ClearSelection();
                        dgvTranslate.Rows[o].Cells[p].Selected = true;
                        dgvTranslate.FirstDisplayedScrollingRowIndex = o;
                        dgvTranslate.Select();
                        Cursor = Cursors.Default;
                        return;
                    }
            Cursor = Cursors.Default;
            MessageBox.Show("Search text was not found.");
        }

        private void cboFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _blnLoading = true;
            if (cboFile.SelectedIndex == 0)
                LoadStrings();
            else if (cboFile.SelectedIndex > 0)
            {
                cboSection.Enabled = false;
                if (_workerSectionLoader.IsBusy)
                    _workerSectionLoader.CancelAsync();
                _workerSectionLoader.RunWorkerAsync();
            }
            _blnLoading = false;
        }

        private void cboSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSection();
        }

        private void dgvSection_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((e.ColumnIndex == 4) && (e.RowIndex != -1))
                dgvSection.EndEdit();
        }

        private void dgvSection_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_blnLoading || (e.RowIndex < 0))
                return;
            DataGridViewRow item = dgvSection.Rows[e.RowIndex];
            bool flag = Convert.ToBoolean(item.Cells["translated"].Value);
            TranslatedIndicator(item);
            string strTranslated = item.Cells["text"].Value.ToString();
            string strEnglish = item.Cells["english"].Value.ToString();
            string strPage = item.Cells["page"].Value.ToString();
            XmlDocument xmlDocument = _objDataDoc;
            XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]/" + cboSection.Text + "//name[text()=\"" + strEnglish + "\"]/..");
            if (xmlNodeLocal == null)
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                xmlNodeLocal = xmlDocument1.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]/" + cboSection.Text + "/*[text()=\"" + strEnglish + "\"]");
                if (xmlNodeLocal?.Attributes != null)
                {
                    xmlNodeLocal.Attributes["translate"].InnerText = strTranslated;
                    XmlAttribute objAttrib;
                    if (!flag)
                    {
                        objAttrib = xmlNodeLocal.Attributes["translated"];
                        if (objAttrib != null)
                            xmlNodeLocal.Attributes.Remove(objAttrib);
                    }
                    else
                    {
                        objAttrib = xmlNodeLocal.Attributes["translated"];
                        if (objAttrib == null)
                        {
                            objAttrib = _objDataDoc.CreateAttribute("translated");
                            objAttrib.InnerText = bool.TrueString;
                            xmlNodeLocal.Attributes.Append(objAttrib);
                        }
                        else
                            objAttrib.InnerText = bool.TrueString;
                    }
                }
            }
            else
            {
                XmlElement element = xmlNodeLocal["translate"];
                if (element != null) element.InnerText = strTranslated;
                XmlElement xmlElement = xmlNodeLocal["page"];
                if (xmlElement != null) xmlElement.InnerText = strPage;

                XmlAttribute itemOf;
                if (!flag)
                {
                    itemOf = xmlNodeLocal.Attributes?["translated"];
                    if (itemOf != null)
                        xmlNodeLocal.Attributes.Remove(itemOf);
                }
                else
                {
                    itemOf = xmlNodeLocal.Attributes?["translated"];
                    if (itemOf == null)
                    {
                        itemOf = _objDataDoc.CreateAttribute("translated");
                        itemOf.InnerText = System.Boolean.TrueString;
                        xmlNodeLocal.Attributes.Append(itemOf);
                    }
                    else
                    {
                        itemOf.InnerText = System.Boolean.TrueString;
                    }
                }
            }
            Save(_objDataDoc);
        }

        private static void TranslatedIndicator(DataGridViewRow item)
        {
            if (Convert.ToBoolean(item.Cells["translated"].Value))
            {
                item.DefaultCellStyle.BackColor = Color.Empty;
                return;
            }
            item.DefaultCellStyle.BackColor = item.Cells["english"].Value.ToString() == item.Cells["text"].Value.ToString() ? Color.Wheat : Color.Empty;
        }

        private void dgvSection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                btnSearch.PerformClick();
                return;
            }
            if ((e.KeyCode == Keys.F) && (e.Modifiers == Keys.Control))
                txtSearch.Focus();
        }

        private void dgvSection_Sorted(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSection.Rows)
            {
                TranslatedIndicator(row);
            }
        }

        private void dgvTranslate_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_blnLoading || (e.RowIndex < 0))
                return;
            DataGridViewRow item = dgvTranslate.Rows[e.RowIndex];
            TranslatedIndicator(item);
            string strText = item.Cells["text"].Value.ToString();
            string strKey = item.Cells["key"].Value.ToString();
            XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode("/chummer/strings/string[key = \"" + strKey + "\"]");
            if (xmlNodeLocal != null)
            {
                XmlElement xmlElement = xmlNodeLocal["text"];
                if (xmlElement != null)
                    xmlElement.InnerText = strText;
            }
            else
            {
                XmlNode newNode = _objTranslationDoc.CreateNode(XmlNodeType.Element, "string", null);

                XmlElement elem = _objTranslationDoc.CreateElement("key");
                XmlText xmlString = _objTranslationDoc.CreateTextNode(strKey);
                newNode.AppendChild(elem);
                newNode.LastChild.AppendChild(xmlString);

                elem = _objTranslationDoc.CreateElement("text");
                xmlString = _objTranslationDoc.CreateTextNode(strText);
                newNode.AppendChild(elem);
                newNode.LastChild.AppendChild(xmlString);

                XmlNode root = _objTranslationDoc.SelectSingleNode("/chummer/strings/.");
                root?.AppendChild(newNode);
            }
            Save(_objTranslationDoc, false);
        }

        private void dgvTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                btnSearch.PerformClick();
                return;
            }
            if ((e.KeyCode == Keys.F) && (e.Modifiers == Keys.Control))
                txtSearch.Focus();
        }

        private void dgvTranslate_Sorted(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvTranslate.Rows)
            {
                TranslatedIndicator(row);
            }
        }

        private void frmTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                btnSearch.PerformClick();
                return;
            }
            if ((e.KeyCode == Keys.F) && (e.Modifiers == Keys.Control))
                txtSearch.Focus();
        }

        private void frmTranslate_Load(object sender, EventArgs e)
        {
            Text = "Translating " + Language;
            _objTranslationDoc.Load(Path.Combine(ApplicationPath, "lang", Code + ".xml"));
            _objDataDoc.Load(Path.Combine(ApplicationPath, "lang", Code + "_data.xml"));
            cboFile.Items.Add("Strings");
            List<string> strs = (from XmlNode xmlNodeLocal in _objDataDoc.SelectNodes("/chummer/chummer") where xmlNodeLocal.Attributes?["file"] != null select xmlNodeLocal.Attributes["file"].InnerText).ToList();
            strs.Sort();
            foreach (string str in strs)
                cboFile.Items.Add(str);
            cboFile.SelectedIndex = 0;
        }
        private void txtSearch_GotFocus(object sender, EventArgs e)
        {
            //txtSearch.SelectAll();
        }

        private void txtSearch_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == '\r') && !txtSearch.AcceptsReturn)
                btnSearch.PerformClick();
        }
        #endregion

        #region Methods
        private void LoadSection()
        {
            if (_blnLoading || cboSection.SelectedIndex < 0)
                return;
            Cursor = Cursors.WaitCursor;
            dgvTranslate.Visible = false;
            dgvSection.Visible = true;
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("english");
            dataTable.Columns.Add("text");
            dataTable.Columns.Add("book");
            dataTable.Columns.Add("page");
            dataTable.Columns.Add("translated");
            string strFileName = cboFile.Text;
            XmlNode selectSingleNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file=\"" + strFileName + "\"]/" + cboSection.Text);
            if (selectSingleNode != null)
            {
                XmlNodeList childNodes = selectSingleNode.ChildNodes;
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(Path.Combine(ApplicationPath, "data", strFileName));
                foreach (XmlNode childNode in childNodes)
                {
                    string strName = string.Empty;
                    string strPage = string.Empty;
                    string strTranslated = string.Empty;
                    string strSource = string.Empty;
                    bool blnTranslated = false;
                    if (childNode["name"] == null)
                    {
                        strName = childNode.InnerText;
                        strTranslated = childNode.Attributes["translate"]?.InnerText ?? string.Empty;
                        blnTranslated = strTranslated == System.Boolean.TrueString;
                    }
                    else
                    {
                        strName = childNode["name"].InnerText;
                        strPage = childNode["page"]?.InnerText ?? string.Empty;
                        XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode("/chummer/" + cboSection.Text + "/*[name=\"" + strName + "\"]");
                        strSource = xmlNodeLocal?["source"]?.InnerText ?? string.Empty;
                        strTranslated = childNode["translate"]?.InnerText ?? string.Empty;
                        XmlNode xmlNodeAttributesTranslated = childNode.Attributes?["translated"];
                        blnTranslated = xmlNodeAttributesTranslated != null
                            ? xmlNodeAttributesTranslated.InnerText == System.Boolean.TrueString
                            : strName != strTranslated;
                    }
                    if (!chkOnlyTranslation.Checked || strName == strTranslated)
                    {
                        object[] objArray = { strName, strTranslated, strSource, strPage, blnTranslated };
                        dataTable.Rows.Add(objArray);
                    }
                }
            }
            var dataSet = new DataSet("strings");
            dataSet.Tables.Add(dataTable);
            dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvSection.DataSource = dataSet;
            dgvSection.DataMember = "strings";
            foreach (DataGridViewRow row in dgvSection.Rows)
            {
                TranslatedIndicator(row);
            }
            Cursor = Cursors.Default;
        }

        BackgroundWorker _workerSectionLoader = new BackgroundWorker();
        List<string> _lstSectionStrings = null;

        private void FinishLoadingSections(object sender, RunWorkerCompletedEventArgs e)
        {
            string strOldSelected = cboSection.SelectedValue?.ToString();

            _blnLoading = true;
            cboSection.BeginUpdate();
            cboSection.DataSource = _lstSectionStrings;
            cboSection.Enabled = true;
            cboSection.EndUpdate();
            _blnLoading = false;

            cboSection.SelectedValue = strOldSelected;
            if (cboSection.SelectedIndex == -1 && cboSection.Items.Count > 0)
                cboSection.SelectedIndex = 0;
        }

        private void LoadSections(object sender, DoWorkEventArgs e)
        {
            _lstSectionStrings = null;
            XmlNode xmlNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]");
            if (xmlNode != null)
            {
                _lstSectionStrings = (from XmlNode childNode in xmlNode.ChildNodes select childNode.Name).ToList();
                _lstSectionStrings.Sort();
            }
        }

        private void LoadStrings()
        {
            Cursor = Cursors.WaitCursor;
            dgvTranslate.Visible = true;
            dgvSection.Visible = false;
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(ApplicationPath, "lang", "en-us.xml"));
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("key");
            dataTable.Columns.Add("english");
            dataTable.Columns.Add("text");
            dataTable.Columns.Add("translated");
            //XmlNodeList xmlNodeList = _objTranslationDoc.SelectNodes("/chummer/strings/string");
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/chummer/strings/string");
            foreach (XmlNode xmlNodeEnglish in xmlNodeList)
            {
                string strKey = xmlNodeEnglish["key"]?.InnerText ?? string.Empty;
                string strEnglish = xmlNodeEnglish["text"]?.InnerText ?? string.Empty;
                string strTranslated = strEnglish;
                var blnTranslated = false;
                XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode("/chummer/strings/string[key = \"" + strKey + "\"]");
                if (xmlNodeLocal != null)
                {
                    strTranslated = xmlNodeLocal["text"]?.InnerText;
                    XmlNode xmlNodeAttributesTranslated = xmlNodeEnglish.Attributes?["translated"];
                    blnTranslated = xmlNodeAttributesTranslated != null
                        ? xmlNodeAttributesTranslated.InnerText == System.Boolean.TrueString
                        : strEnglish != strTranslated;
                }
                if (!chkOnlyTranslation.Checked || string.IsNullOrWhiteSpace(strTranslated) || strEnglish == strTranslated)
                {
                    DataRowCollection rows = dataTable.Rows;
                    object[] objArray = { strKey, strEnglish, strTranslated, blnTranslated };
                    rows.Add(objArray);
                }
            }
            var dataSet = new DataSet("strings");
            dataSet.Tables.Add(dataTable);
            dgvTranslate.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvTranslate.DataSource = dataSet;
            dgvTranslate.DataMember = "strings";
            foreach (DataGridViewRow row in dgvTranslate.Rows)
            {
                TranslatedIndicator(row);
            }
            cboSection.Visible = false;
            Cursor = Cursors.Default;
        }

        private void Save(XmlDocument objXmlDocument, bool blnData = true)
        {
            string strPath = Path.Combine(ApplicationPath, "lang", Code + ( blnData ? "_data.xml" : ".xml"));
            var xwsSettings = new XmlWriterSettings { IndentChars = ("\t"), Indent = true};
            using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
            {
                objXmlDocument.Save(xwWriter);
            }
        }
        #endregion

        #region Properties
        public string Language
        {
            get
            {
                return _strLanguage;
            }
        }

        public string Code
        {
            get
            {
                return _strCode;
            }
        }

        public string ApplicationPath
        {
            get
            {
                return _strPath;
            }
        }
        #endregion
    }
}
