/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

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
        private readonly BackgroundWorker _workerSectionLoader = new BackgroundWorker();
        private bool _blnQueueSectionLoaderRun;
        private readonly string[] _strSectionLoaderArgs = new string[2];
        private readonly BackgroundWorker _workerStringsLoader = new BackgroundWorker();
        private bool _blnQueueStringsLoaderRun;

        [Obsolete("This constructor is for use by form designers only.", true)]
        public frmTranslate()
        {
            InitializeComponent();
        }

        public frmTranslate(string strLanguage)
        {
            ApplicationPath = Application.StartupPath;
            Language = strLanguage;
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            Code = Language.Substring(Language.IndexOf('(') + 1, 5).ToLower();

            InitializeComponent();

            _workerSectionLoader.WorkerReportsProgress = true;
            _workerSectionLoader.WorkerSupportsCancellation = true;
            _workerSectionLoader.DoWork += DoLoadSection;
            _workerSectionLoader.ProgressChanged += RefreshProgressBar;
            _workerSectionLoader.RunWorkerCompleted += FinishLoadingSection;

            _workerStringsLoader.WorkerReportsProgress = true;
            _workerStringsLoader.WorkerSupportsCancellation = true;
            _workerStringsLoader.DoWork += DoLoadStrings;
            _workerStringsLoader.ProgressChanged += RefreshProgressBar;
            _workerStringsLoader.RunWorkerCompleted += FinishLoadingStrings;
        }

        private void RunQueuedWorkers(object sender, EventArgs e)
        {
            if (_blnQueueSectionLoaderRun)
            {
                if (!_workerSectionLoader.IsBusy)
                    _workerSectionLoader.RunWorkerAsync();
            }

            if (_blnQueueStringsLoaderRun)
            {
                if (!_workerStringsLoader.IsBusy)
                    _workerStringsLoader.RunWorkerAsync();
            }
        }

        #region Control Events

        private void frmTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Idle -= RunQueuedWorkers;

            if (_workerSectionLoader.IsBusy)
                _workerSectionLoader.CancelAsync();
            if (_workerStringsLoader.IsBusy)
                _workerStringsLoader.CancelAsync();

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
            int rowIndex = dgvSection.SelectedCells.Count > 0 ? dgvSection.SelectedCells[0].RowIndex : 0;
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
            string strId = item.Cells["Id"].Value.ToString();
            string strSection = cboSection.Text;
            if (strSection == "[Show All Sections]")
                strSection = "*";
            string strBaseXPath = "/chummer/chummer[@file=\"" + cboFile.Text + "\"]/" + strSection + '/';
            if (cboFile.Text == "tips.xml")
            {
                XmlNode xmlNodeLocal = _objDataDoc.SelectSingleNode(strBaseXPath + "/id[text()=\"" + strId + "\"]/..") ??
                                       _objDataDoc.SelectSingleNode(strBaseXPath + "/text[text()=\"" + strEnglish + "\"]/..");
                if (xmlNodeLocal != null)
                {
                    XmlElement element = xmlNodeLocal["translate"];
                    if (element != null)
                        element.InnerText = strTranslated;

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
                        xmlNodeLocal.Attributes?.Append(objAttrib);
                    }
                }
            }
            else
            {
                string strPage = item.Cells[cboFile.Text == "books.xml" ? "Code" : "Page"].Value.ToString();
                bool blnHasNameOnPage = cboFile.Text == "qualities.xml";
                string strNameOnPage = blnHasNameOnPage ? item.Cells["NameOnPage"].Value.ToString() : string.Empty;
                XmlNode xmlNodeLocal = _objDataDoc.SelectSingleNode(strBaseXPath + "/id[text()=\"" + strId + "\"]/..") ??
                                       _objDataDoc.SelectSingleNode(strBaseXPath + "/name[text()=\"" + strEnglish + "\"]/..");
                if (xmlNodeLocal == null)
                {
                    xmlNodeLocal = _objDataDoc.SelectSingleNode(strBaseXPath + "*[text()=\"" + strEnglish + "\"]");
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
                    element = xmlNodeLocal.Name == "book" ? xmlNodeLocal["altcode"] : xmlNodeLocal["altpage"];
                    if (element != null) element.InnerText = strPage;
                    if (blnHasNameOnPage)
                    {
                        element = xmlNodeLocal["altnameonpage"];
                        if (string.IsNullOrWhiteSpace(strNameOnPage) && element != null)
                        {
                            xmlNodeLocal.RemoveChild(element);
                        }
                        else
                        {
                            if (element == null)
                            {
                                element = _objDataDoc.CreateElement("altnameonpage");
                                xmlNodeLocal.AppendChild(element);
                            }

                            element.InnerText = strNameOnPage;
                        }
                    }


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
                        xmlNodeLocal.Attributes?.Append(objAttrib);
                    }
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

            Application.Idle += RunQueuedWorkers;
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
            _blnQueueStringsLoaderRun = false;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(ApplicationPath, "lang", "en-us.xml"));
            if (_workerStringsLoader.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            DataTable dataTable = new DataTable("strings");
            dataTable.Columns.Add("Key");
            dataTable.Columns.Add("English");
            dataTable.Columns.Add("Text");
            dataTable.Columns.Add("Translated?");
            //XmlNodeList xmlNodeList = _objTranslationDoc.SelectNodes("/chummer/strings/string");
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/chummer/strings/string");
            if (xmlNodeList != null)
            {
                int intSegmentsToProcess = xmlNodeList.Count;
                int intSegmentsProcessed = 0;
                object intSegmentsProcessedLock = new object();
                object[][] arrayRowsToDisplay = new object[xmlNodeList.Count][];
                object arrayRowsToDisplayLock = new object();
                if (_workerStringsLoader.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    Parallel.For(0, xmlNodeList.Count, i =>
                    {
                        XmlNode xmlNodeEnglish = xmlNodeList[i];
                        string strKey = xmlNodeEnglish["key"]?.InnerText ?? string.Empty;
                        string strEnglish = xmlNodeEnglish["text"]?.InnerText ?? string.Empty;
                        string strTranslated = strEnglish;
                        bool blnTranslated = false;
                        XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode("/chummer/strings/string[key = \"" + strKey + "\"]");
                        if (xmlNodeLocal != null)
                        {
                            strTranslated = xmlNodeLocal["text"]?.InnerText ?? string.Empty;
                            XmlNode xmlNodeAttributesTranslated = xmlNodeLocal.Attributes?["translated"];
                            blnTranslated = xmlNodeAttributesTranslated != null
                                ? xmlNodeAttributesTranslated.InnerText == bool.TrueString
                                : strEnglish != strTranslated;
                        }

                        if (!blnTranslated || !chkOnlyTranslation.Checked)
                        {
                            object[] objArray = {strKey, strEnglish, strTranslated, blnTranslated};
                            lock (arrayRowsToDisplayLock)
                                arrayRowsToDisplay[i] = objArray;
                        }

                        lock (intSegmentsProcessedLock)
                            intSegmentsProcessed += 1;
                        _workerStringsLoader.ReportProgress(intSegmentsProcessed * 100 / intSegmentsToProcess);
                        if (_workerStringsLoader.CancellationPending)
                            throw new OperationCanceledException();
                    });
                }
                catch (OperationCanceledException)
                {
                    e.Cancel = true;
                    return;
                }

                if (_workerStringsLoader.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                DataRowCollection objDataTableRows = dataTable.Rows;
                foreach (object[] objArray in arrayRowsToDisplay)
                {
                    if (objArray != null)
                        objDataTableRows.Add(objArray);
                }

                DataSet dataSet = new DataSet("strings");
                dataSet.Tables.Add(dataTable);
                e.Result = dataSet;
                if (_workerStringsLoader.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void FinishLoadingStrings(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
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
            }

            pbTranslateProgressBar.Value = 0;
            Cursor = Cursors.Default;
        }

        private void DoLoadSection(object sender, DoWorkEventArgs e)
        {
            _blnQueueSectionLoaderRun = false;
            string strFileName = _strSectionLoaderArgs[0];
            string strSection = _strSectionLoaderArgs[1];
            bool blnHasNameOnPage = strFileName == "qualities.xml";
            DataTable dataTable = new DataTable("strings");
            dataTable.Columns.Add("Id");
            dataTable.Columns.Add("English");
            dataTable.Columns.Add("Text");
            if (strFileName == "books.xml")
            {
                dataTable.Columns.Add("English Code");
                dataTable.Columns.Add("Code");
            }
            else if (strFileName != "tips.xml")
            {
                dataTable.Columns.Add("Book");
                dataTable.Columns.Add("Page");
            }

            if (strFileName == "mentors.xml" || strFileName == "paragons.xml")
            {
                dataTable.Columns.Add("Advantage");
                dataTable.Columns.Add("Translated Advantage");
                dataTable.Columns.Add("Disadvantage");
                dataTable.Columns.Add("Translated Disadvantage");

            }

            dataTable.Columns.Add("Translated?");
            if (blnHasNameOnPage)
                dataTable.Columns.Add("NameOnPage");
            if (strSection == "[Show All Sections]")
                strSection = "*";
            if (_workerSectionLoader.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            XmlNodeList xmlBaseList = _objDataDoc.SelectNodes("/chummer/chummer[@file=\"" + strFileName + "\"]/" + strSection);
            int intSegmentsToProcess = 0;
            if (xmlBaseList != null)
            {
                foreach (XmlNode xmlNodeToShow in xmlBaseList)
                {
                    if (xmlNodeToShow.HasChildNodes)
                        intSegmentsToProcess += xmlNodeToShow.ChildNodes.Count;
                }

                int intSegmentsProcessed = 0;
                object intSegmentsProcessedLock = new object();
                foreach (XmlNode xmlNodeToShow in xmlBaseList)
                {
                    if (_workerSectionLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    XmlNodeList xmlChildNodes = xmlNodeToShow.ChildNodes;
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(Path.Combine(ApplicationPath, "data", strFileName));
                    object[][] arrayRowsToDisplay = new object[xmlChildNodes.Count][];
                    object arrayRowsToDisplayLock = new object();
                    try
                    {
                        Parallel.For(0, xmlChildNodes.Count, i =>
                        {
                            XmlNode xmlChildNode = xmlChildNodes[i];
                            string strId = xmlChildNode["id"]?.InnerText ?? string.Empty;
                            string strTranslated;
                            bool blnTranslated;
                            if (strFileName == "tips.xml")
                            {
                                string strText = xmlChildNode["text"]?.InnerText ?? string.Empty;
                                strTranslated = xmlChildNode["translate"]?.InnerText ?? string.Empty;
                                blnTranslated = strText != strTranslated || xmlChildNode.Attributes?["translated"]?.InnerText == bool.TrueString;

                                if (!blnTranslated || !chkOnlyTranslation.Checked)
                                {
                                    object[] objArray = new object[] { strId, strText, strTranslated, blnTranslated };
                                    lock (arrayRowsToDisplayLock)
                                        arrayRowsToDisplay[i] = objArray;
                                }
                            }
                            else
                            {
                                string strName;
                                string strPage            = string.Empty;
                                string strSource          = string.Empty;
                                string strNameOnPage      = string.Empty;
                                string strAdvantage       = string.Empty;
                                string strAdvantageAlt    = string.Empty;
                                string strDisadvantage    = string.Empty;
                                string strDisadvantageAlt = string.Empty;

                                bool blnAdvantage      = strFileName == "mentors.xml" || strFileName == "paragons.xml";
                                XmlNode xmlChildNameNode = xmlChildNode["name"];
                                if (xmlChildNameNode == null)
                                {
                                    strName = xmlChildNode.InnerText;
                                    strTranslated = xmlChildNode.Attributes?["translate"]?.InnerText ?? string.Empty;
                                    blnTranslated = strName != strTranslated || xmlChildNode.Attributes?["translated"]?.InnerText == bool.TrueString;
                                }
                                else
                                {
                                    strName = xmlChildNameNode.InnerText;
                                    strPage = (strFileName == "books.xml" ? xmlChildNode["altcode"]?.InnerText : xmlChildNode["altpage"]?.InnerText) ?? string.Empty;
                                    // if we have an Id get the Node using it
                                    XmlNode xmlNodeLocal = !string.IsNullOrEmpty(strId)
                                        ? xmlDocument.SelectSingleNode("/chummer/" + strSection + "/*[id=\"" + strId + "\"]")
                                        : xmlDocument.SelectSingleNode("/chummer/" + strSection + "/*[name=\"" + strName + "\"]");
                                    if (xmlNodeLocal == null) MessageBox.Show(strName);
                                    strSource = xmlNodeLocal?["source"]?.InnerText ?? string.Empty;
                                    strTranslated = xmlChildNode["translate"]?.InnerText ?? string.Empty;
                                    blnTranslated = strName != strTranslated || xmlChildNode.Attributes?["translated"]?.InnerText == bool.TrueString;
                                    if (blnHasNameOnPage)
                                        strNameOnPage = xmlChildNode["altnameonpage"]?.InnerText ?? string.Empty;
                                    if (blnAdvantage)
                                    {
                                        strAdvantage       = xmlNodeLocal?["advantage"]?.InnerText    ?? string.Empty;
                                        strDisadvantage    = xmlNodeLocal?["disadvantage"]?.InnerText ?? string.Empty;
                                        strAdvantageAlt    = xmlChildNode["altadvantage"]?.InnerText ?? string.Empty;
                                        strDisadvantageAlt = xmlChildNode["altdisadvantage"]?.InnerText ?? string.Empty;

                                        blnTranslated =
                                            (strName != strTranslated ||
                                             xmlChildNode.Attributes?["translated"]?.InnerText == bool.TrueString ||
                                             strAdvantage != strAdvantageAlt || strDisadvantage != strDisadvantageAlt);
                                    }
                                }

                                if (!blnTranslated || !chkOnlyTranslation.Checked)
                                {
                                    object[] objArray;
                                    if (blnHasNameOnPage)
                                        objArray = new object[]
                                        {
                                            strId, strName, strTranslated, strSource, strPage, blnTranslated,
                                            strNameOnPage
                                        };
                                    else if (blnAdvantage)
                                    {
                                        if (blnHasNameOnPage)
                                        {
                                            objArray = new object[]
                                            {
                                                strId, strName, strTranslated, strSource, strPage, strAdvantage,
                                                strAdvantageAlt, strDisadvantage, strDisadvantageAlt, blnTranslated,
                                                strNameOnPage
                                            };
                                        }
                                        else
                                        {
                                            objArray = new object[]
                                            {
                                                strId, strName, strTranslated, strSource, strPage, strAdvantage,
                                                strAdvantageAlt, strDisadvantage, strDisadvantageAlt, blnTranslated
                                            };

                                        }
                                    }
                                    else
                                        objArray = new object[]
                                            {strId, strName, strTranslated, strSource, strPage, blnTranslated};
                                    lock (arrayRowsToDisplayLock)
                                        arrayRowsToDisplay[i] = objArray;
                                }
                            }

                            lock (intSegmentsProcessedLock)
                                intSegmentsProcessed += 1;
                            _workerSectionLoader.ReportProgress(intSegmentsProcessed * 100 / intSegmentsToProcess);
                            if (_workerStringsLoader.CancellationPending)
                                throw new OperationCanceledException();
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (_workerStringsLoader.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

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
                if (_workerStringsLoader.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            else
                e.Cancel = true;
        }

        private void RefreshProgressBar(object sender, ProgressChangedEventArgs e)
        {
            pbTranslateProgressBar.Value = (e.ProgressPercentage * pbTranslateProgressBar.Maximum) / 100;
        }

        private void FinishLoadingSection(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvSection.DataSource = e.Result as DataSet;
                dgvSection.DataMember = "strings";
                dgvSection.Columns[0].FillWeight = 0.5f;
                dgvSection.Columns[1].FillWeight = 4.0f;
                dgvSection.Columns[2].FillWeight = 4.0f;
                dgvSection.Columns[3].FillWeight = 0.5f;
                if (cboFile.Text != "tips.xml")
                {
                    dgvSection.Columns[4].FillWeight = 0.5f;
                    dgvSection.Columns[5].FillWeight = 0.5f;
                }
                if (cboFile.Text == "qualities.xml")
                {
                    dgvSection.Columns[1].FillWeight = 3.5f;
                    dgvSection.Columns[2].FillWeight = 3.5f;
                    dgvSection.Columns[6].FillWeight = 1f;
                }

                foreach (DataGridViewRow row in dgvSection.Rows)
                {
                    TranslatedIndicator(row);
                }

                dgvTranslate.Visible = false;
                dgvSection.Visible = true;
            }

            pbTranslateProgressBar.Value = 0;
            Cursor = Cursors.Default;
        }

        #endregion BackgroundWorker Events

        #region Methods

        private void LoadSection()
        {
            if (_blnLoading || cboSection.SelectedIndex < 0)
                return;

            if (_workerSectionLoader.IsBusy)
                _workerSectionLoader.CancelAsync();

            Cursor = Cursors.WaitCursor;
            pbTranslateProgressBar.Value = 0;

            _strSectionLoaderArgs[0] = cboFile.Text;
            _strSectionLoaderArgs[1] = cboSection.Text;

            _blnQueueSectionLoaderRun = true;
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

                if (lstSectionStrings.Count > 0)
                {
                    lstSectionStrings.Insert(0, "[Show All Sections]");
                }
            }

            string strOldSelected = cboSection.SelectedValue?.ToString() ?? string.Empty;

            _blnLoading = true;
            cboSection.BeginUpdate();
            cboSection.Items.Clear();
            if (lstSectionStrings != null)
                foreach (string strSection in lstSectionStrings)
                    cboSection.Items.Add(strSection);
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
            if (_workerStringsLoader.IsBusy)
                _workerStringsLoader.CancelAsync();

            Cursor = Cursors.WaitCursor;
            pbTranslateProgressBar.Value = 0;

            _blnQueueStringsLoaderRun = true;
        }

        private void Save(XmlDocument objXmlDocument, bool blnData = true)
        {
            string strPath = Path.Combine(ApplicationPath, "lang", Code + (blnData ? "_data.xml" : ".xml"));
            XmlWriterSettings xwsSettings = new XmlWriterSettings {IndentChars = ("\t"), Indent = true};
            using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
            {
                objXmlDocument.Save(xwWriter);
            }
        }

        #endregion

        #region Properties

        public string Language { get; }

        public string Code { get; }

        public string ApplicationPath { get; }

        #endregion
    }
}
