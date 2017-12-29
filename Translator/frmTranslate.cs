using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly BackgroundWorker _workerSectionLoader = new BackgroundWorker();
        private readonly BackgroundWorker _workerStringsLoader = new BackgroundWorker();

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
            _strCode = Language.Substring(Language.IndexOf('(') + 1, 5).ToLower();

            InitializeComponent();

            _workerSectionLoader.WorkerReportsProgress = true;
            _workerSectionLoader.WorkerSupportsCancellation = false;
            _workerSectionLoader.DoWork += DoLoadSection;
            _workerSectionLoader.ProgressChanged += RefreshProgressBar;
            _workerSectionLoader.RunWorkerCompleted += FinishLoadingSection;

            _workerStringsLoader.WorkerReportsProgress = true;
            _workerStringsLoader.WorkerSupportsCancellation = false;
            _workerStringsLoader.DoWork += DoLoadStrings;
            _workerStringsLoader.ProgressChanged += RefreshProgressBar;
            _workerStringsLoader.RunWorkerCompleted += FinishLoadingStrings;
        }

        #region Control Events
        private void frmTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.MainForm.OpenTranslateWindows.Remove(this);
            Dispose(true);
        }

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
            pbTranslateProgressBar.Value = 0;
            string strNeedle = txtSearch.Text;
            int rowCount = dgvSection.RowCount;
            int columnCount = dgvSection.ColumnCount;
            pbTranslateProgressBar.Step = 1;
            pbTranslateProgressBar.Maximum = rowCount * columnCount;
            int rowIndex = dgvSection.SelectedCells[0].RowIndex;
            for (int i = rowIndex; i < rowCount; ++i)
            {
                DataGridViewCellCollection objCurrentRowCells = dgvSection.Rows[i].Cells;
                for (int j = 0; j < columnCount; j++)
                {
                    DataGridViewCell objCurrentCell = objCurrentRowCells[j];
                    if (objCurrentCell.Value.ToString().IndexOf(strNeedle, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        dgvSection.ClearSelection();
                        objCurrentCell.Selected = true;
                        dgvSection.FirstDisplayedScrollingRowIndex = i;
                        dgvSection.Select();
                        pbTranslateProgressBar.Value = 0;
                        Cursor = Cursors.Default;
                        return;
                    }
                    pbTranslateProgressBar.PerformStep();
                }
            }
            for (int i = 0; i < rowIndex; ++i)
            {
                DataGridViewCellCollection objCurrentRowCells = dgvSection.Rows[i].Cells;
                for (int j = 0; j < columnCount; j++)
                {
                    DataGridViewCell objCurrentCell = objCurrentRowCells[j];
                    if (objCurrentCell.Value.ToString().IndexOf(strNeedle, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        dgvSection.ClearSelection();
                        objCurrentCell.Selected = true;
                        dgvSection.FirstDisplayedScrollingRowIndex = i;
                        dgvSection.Select();
                        pbTranslateProgressBar.Value = 0;
                        Cursor = Cursors.Default;
                        return;
                    }
                    pbTranslateProgressBar.PerformStep();
                }
            }
            Cursor = Cursors.Default;
            MessageBox.Show("Search text was not found.");
        }

        private void cboFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _blnLoading = true;
            if (cboFile.SelectedIndex == 0)
            {
                cboSection.Visible = false;
                LoadStrings();
            }
            else if (cboFile.SelectedIndex > 0)
            {
                cboSection.Visible = true;
                LoadSections();
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
            bool flag = Convert.ToBoolean(item.Cells["Translated?"].Value);
            TranslatedIndicator(item);
            string strTranslated = item.Cells["Text"].Value.ToString();
            string strEnglish = item.Cells["English"].Value.ToString();
            string strPage = item.Cells[cboFile.Text == "books.xml" ? "Code" : "Page"].Value.ToString();
            string strSection = cboSection.Text;
            if (strSection == "[Show All Sections]")
                strSection = "*";
            XmlNode xmlNodeLocal = _objDataDoc.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]/" + strSection + "//name[text()=\"" + strEnglish + "\"]/..");
            if (xmlNodeLocal == null)
            {
                xmlNodeLocal = _objDataDoc.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]/" + strSection + "/*[text()=\"" + strEnglish + "\"]");
                if (xmlNodeLocal?.Attributes != null)
                {
                    xmlNodeLocal.Attributes["translate"].InnerText = strTranslated;
                    XmlAttribute objAttrib = xmlNodeLocal.Attributes?["translated"];
                    if (objAttrib != null)
                    {
                        if (!flag)
                            xmlNodeLocal.Attributes.Remove(objAttrib);
                        else
                            objAttrib.InnerText = bool.TrueString;
                    }
                    else if (flag)
                    {
                        objAttrib = _objDataDoc.CreateAttribute("translated");
                        objAttrib.InnerText = bool.TrueString;
                        xmlNodeLocal.Attributes.Append(objAttrib);
                    }
                }
            }
            else
            {
                XmlElement element = xmlNodeLocal["translate"];
                if (element != null) element.InnerText = strTranslated;
                XmlElement xmlElement = xmlNodeLocal.Name == "book" ? xmlNodeLocal["altcode"] : xmlNodeLocal["altpage"];
                if (xmlElement != null) xmlElement.InnerText = strPage;

                XmlAttribute objAttrib = xmlNodeLocal.Attributes?["translated"];
                if (objAttrib != null)
                {
                    if (!flag)
                        xmlNodeLocal.Attributes.Remove(objAttrib);
                    else
                        objAttrib.InnerText = bool.TrueString;
                }
                else if (flag)
                {
                    objAttrib = _objDataDoc.CreateAttribute("translated");
                    objAttrib.InnerText = bool.TrueString;
                    xmlNodeLocal.Attributes.Append(objAttrib);
                }
            }
            Save(_objDataDoc);
        }

        private static void TranslatedIndicator(DataGridViewRow item)
        {
            if (Convert.ToBoolean(item.Cells["Translated?"].Value))
            {
                item.DefaultCellStyle.BackColor = Color.Empty;
                return;
            }
            item.DefaultCellStyle.BackColor = item.Cells["English"].Value.ToString() == item.Cells["Text"].Value.ToString() ? Color.Wheat : Color.Empty;
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
            string strText = item.Cells["Text"].Value.ToString();
            string strKey = item.Cells["Key"].Value.ToString();
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

        #region BackgroundWorker Events
        private void DoLoadStrings(object sender, DoWorkEventArgs e)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(ApplicationPath, "lang", "en-us.xml"));
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("Key");
            dataTable.Columns.Add("English");
            dataTable.Columns.Add("Text");
            dataTable.Columns.Add("Translated?");
            //XmlNodeList xmlNodeList = _objTranslationDoc.SelectNodes("/chummer/strings/string");
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/chummer/strings/string");

            int intSegmentsToProcess = xmlNodeList.Count;
            int intSegmentsProcessed = 0;
            object intSegmentsProcessedLock = new object();

            object[][] arrayRowsToDisplay = new object[xmlNodeList.Count][];
            object arrayRowsToDisplayLock = new object();
            Parallel.For(0, xmlNodeList.Count, i =>
            {
                XmlNode xmlNodeEnglish = xmlNodeList[i];
                string strKey = xmlNodeEnglish["key"]?.InnerText ?? string.Empty;
                string strEnglish = xmlNodeEnglish["text"]?.InnerText ?? string.Empty;
                string strTranslated = strEnglish;
                var blnTranslated = false;
                XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode("/chummer/strings/string[key = \"" + strKey + "\"]");
                if (xmlNodeLocal != null)
                {
                    strTranslated = xmlNodeLocal["text"]?.InnerText ?? string.Empty;
                    XmlNode xmlNodeAttributesTranslated = xmlNodeLocal.Attributes?["translated"];
                    blnTranslated = xmlNodeAttributesTranslated != null
                        ? xmlNodeAttributesTranslated.InnerText == System.Boolean.TrueString
                        : strEnglish != strTranslated;
                }
                if (!blnTranslated || !chkOnlyTranslation.Checked)
                {
                    object[] objArray = { strKey, strEnglish, strTranslated, blnTranslated };
                    lock (arrayRowsToDisplayLock)
                        arrayRowsToDisplay[i] = objArray;
                }
                lock (intSegmentsProcessedLock)
                    intSegmentsProcessed += 1;
                _workerStringsLoader.ReportProgress(intSegmentsProcessed * 100 / intSegmentsToProcess);
            });
            DataRowCollection objDataTableRows = dataTable.Rows;
            foreach (object[] objArray in arrayRowsToDisplay)
            {
                if (objArray != null)
                    objDataTableRows.Add(objArray);
            }
            var dataSet = new DataSet("strings");
            dataSet.Tables.Add(dataTable);
            e.Result = dataSet;
        }

        private void FinishLoadingStrings(object sender, RunWorkerCompletedEventArgs e)
        {
            dgvTranslate.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvTranslate.DataSource = e.Result as DataSet;
            dgvTranslate.DataMember = "strings";
            dgvTranslate.Columns[0].FillWeight = 1f;
            dgvTranslate.Columns[1].FillWeight = 4.25f;
            dgvTranslate.Columns[2].FillWeight = 4.25f;
            dgvTranslate.Columns[3].FillWeight = 0.5f;
            foreach (DataGridViewRow row in dgvTranslate.Rows)
            {
                TranslatedIndicator(row);
            }
            dgvTranslate.Visible = true;
            dgvSection.Visible = false;
            pbTranslateProgressBar.Value = 0;
            Cursor = Cursors.Default;
        }

        private void DoLoadSection(object sender, DoWorkEventArgs e)
        {
            string[] strArgs = e.Argument as string[];
            string strFileName = strArgs[0];
            string strSection = strArgs[1];
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("English");
            dataTable.Columns.Add("Text");
            if (strFileName == "books.xml")
            {
                dataTable.Columns.Add("English Code");
                dataTable.Columns.Add("Code");
            }
            else
            {
                dataTable.Columns.Add("Book");
                dataTable.Columns.Add("Page");
            }
            dataTable.Columns.Add("Translated?");
            if (strSection == "[Show All Sections]")
                strSection = "*";
            XmlNodeList xmlBaseList = _objDataDoc.SelectNodes("/chummer/chummer[@file=\"" + strFileName + "\"]/" + strSection);
            int intSegmentsToProcess = 0;
            foreach (XmlNode xmlNodeToShow in xmlBaseList)
            {
                if (xmlNodeToShow.HasChildNodes)
                    intSegmentsToProcess += xmlNodeToShow.ChildNodes.Count;
            }
            int intSegmentsProcessed = 0;
            object intSegmentsProcessedLock = new object();

            foreach (XmlNode xmlNodeToShow in xmlBaseList)
            {
                XmlNodeList xmlChildNodes = xmlNodeToShow.ChildNodes;
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(Path.Combine(ApplicationPath, "data", strFileName));
                object[][] arrayRowsToDisplay = new object[xmlChildNodes.Count][];
                object arrayRowsToDisplayLock = new object();
                Parallel.For(0, xmlChildNodes.Count, i =>
                {
                    XmlNode xmlChildNode = xmlChildNodes[i];
                    string strName = string.Empty;
                    string strPage = string.Empty;
                    string strTranslated = string.Empty;
                    string strSource = string.Empty;
                    bool blnTranslated = false;
                    XmlNode xmlChildNameNode = xmlChildNode["name"];
                    if (xmlChildNameNode == null)
                    {
                        strName = xmlChildNode.InnerText;
                        strTranslated = xmlChildNode.Attributes?["translate"]?.InnerText ?? string.Empty;
                        blnTranslated = strName != strTranslated || xmlChildNode.Attributes?["translated"]?.InnerText == System.Boolean.TrueString;
                    }
                    else
                    {
                        strName = xmlChildNameNode.InnerText;
                        strPage = (strFileName == "books.xml" ? xmlChildNode["altcode"]?.InnerText : xmlChildNode["altpage"]?.InnerText ) ?? string.Empty;
                        XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode("/chummer/" + strSection + "/*[name=\"" + strName + "\"]");
                        strSource = xmlNodeLocal?["source"]?.InnerText ?? string.Empty;
                        strTranslated = xmlChildNode["translate"]?.InnerText ?? string.Empty;
                        blnTranslated = strName != strTranslated || xmlChildNode.Attributes?["translated"]?.InnerText == System.Boolean.TrueString;
                    }
                    if (!blnTranslated || !chkOnlyTranslation.Checked)
                    {
                        object[] objArray = { strName, strTranslated, strSource, strPage, blnTranslated };
                        lock (arrayRowsToDisplayLock)
                            arrayRowsToDisplay[i] = objArray;
                    }
                    lock (intSegmentsProcessedLock)
                        intSegmentsProcessed += 1;
                    _workerSectionLoader.ReportProgress(intSegmentsProcessed * 100 / intSegmentsToProcess);
                });
                DataRowCollection objDataTableRows = dataTable.Rows;
                foreach (object[] objArray in arrayRowsToDisplay)
                {
                    if (objArray != null)
                        objDataTableRows.Add(objArray);
                }
            }

            DataSet dataSet = new DataSet("strings");
            dataSet.Tables.Add(dataTable);
            e.Result = dataSet;
        }

        private void RefreshProgressBar(object sender, ProgressChangedEventArgs e)
        {
            pbTranslateProgressBar.Value = (e.ProgressPercentage * pbTranslateProgressBar.Maximum) / 100;
        }

        private void FinishLoadingSection(object sender, RunWorkerCompletedEventArgs e)
        {
            dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvSection.DataSource = e.Result as DataSet;
            dgvSection.DataMember = "strings";
            dgvSection.Columns[0].FillWeight = 4.25f;
            dgvSection.Columns[1].FillWeight = 4.25f;
            dgvSection.Columns[2].FillWeight = 0.5f;
            dgvSection.Columns[3].FillWeight = 0.5f;
            dgvSection.Columns[4].FillWeight = 0.5f;
            foreach (DataGridViewRow row in dgvSection.Rows)
            {
                TranslatedIndicator(row);
            }
            dgvTranslate.Visible = false;
            dgvSection.Visible = true;
            pbTranslateProgressBar.Value = 0;
            Cursor = Cursors.Default;
        }
        #endregion BackgroundWorker Events

        #region Methods
        private void LoadSection()
        {
            if (_blnLoading || cboSection.SelectedIndex < 0 || _workerSectionLoader.IsBusy)
                return;

            Cursor = Cursors.WaitCursor;
            pbTranslateProgressBar.Value = 0;

            string[] strArgs = { cboFile.Text, cboSection.Text };

            _workerSectionLoader.RunWorkerAsync(strArgs);
        }

        private void LoadSections()
        {
            Cursor = Cursors.WaitCursor;
            List<string> lstSectionStrings = null;
            XmlNode xmlNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file=\"" + cboFile.Text + "\"]");
            if (xmlNode != null)
            {
                lstSectionStrings = (from XmlNode childNode in xmlNode.ChildNodes select childNode.Name).ToList();
                lstSectionStrings.Sort();
            }

            if (lstSectionStrings.Count > 0)
            {
                lstSectionStrings.Insert(0, "[Show All Sections]");
            }

            string strOldSelected = cboSection.SelectedValue?.ToString() ?? string.Empty;

            _blnLoading = true;
            cboSection.BeginUpdate();
            cboSection.Items.Clear();
            cboSection.Items.AddRange(lstSectionStrings.ToArray());
            cboSection.EndUpdate();
            _blnLoading = false;

            if (string.IsNullOrEmpty(strOldSelected))
                cboSection.SelectedValue = -1;
            else
            {
                cboSection.SelectedValue = strOldSelected;
                if (cboSection.SelectedIndex == -1 && cboSection.Items.Count > 0)
                    cboSection.SelectedIndex = 0;
            }
            Cursor = Cursors.Default;
        }

        private void LoadStrings()
        {
            Cursor = Cursors.WaitCursor;
            pbTranslateProgressBar.Value = 0;

            _workerStringsLoader.RunWorkerAsync();
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
