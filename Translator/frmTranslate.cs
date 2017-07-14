using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
// ReSharper disable LocalizableElement

namespace Translator
{
    public partial class FrmTranslate : Form
    {
        private bool _blnLoading;
        private readonly XmlDocument _objDataDoc = new XmlDocument();
        private readonly XmlDocument _objTranslationDoc = new XmlDocument();
        private string _strCode = string.Empty;
        private string _strPath = string.Empty;

        public FrmTranslate()
        {
            InitializeComponent();
        }

        public string Language { get; set; }

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
                            return;
                        }
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
                        return;
                    }
            MessageBox.Show("Search text was not found.");
        }

        private void cboFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _blnLoading = true;
            if (cboFile.SelectedIndex == 0)
                LoadStrings();
            else if (cboFile.SelectedIndex > 0)
                LoadSections();
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
            string[] strConcatNode =
            {
                "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "//name[text()=\"",
                strEnglish, "\"]/.."
            };
            XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode(string.Concat(strConcatNode));
            if (xmlNodeLocal == null)
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                string[] strArrays =
                {
                    "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "/*[text()=\"",
                    strEnglish, "\"]"
                };
                xmlNodeLocal = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
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
                            objAttrib = _objDataDoc.CreateAttribute("translated");
                        else
                            objAttrib.InnerText = true.ToString();
                        objAttrib.InnerText = true.ToString();
                        xmlNodeLocal.Attributes.Append(objAttrib);
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
                        itemOf.InnerText = true.ToString();
                        xmlNodeLocal.Attributes?.Append(itemOf);
                    }
                    else
                    {
                        itemOf.InnerText = true.ToString();
                    }
                }
            }
            Save(_objDataDoc);
        }

        private static void TranslatedIndicator(DataGridViewRow item)
        {
            bool blnTranslated = Convert.ToBoolean(item.Cells["translated"].Value);
            if (blnTranslated)
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
            XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", strKey, "\"]"));
            if (xmlNodeLocal != null)
            {
                XmlElement xmlElement = xmlNodeLocal["text"];
                if (xmlElement != null) xmlElement.InnerText = strText;
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
            Text = string.Concat("Translating ", Language);
            SetPath();
            LoadLanguageFiles();
            cboFile.Items.Add("Strings");
            List<string> strs = (from XmlNode xmlNodeLocal in _objDataDoc.SelectNodes("/chummer/chummer") where xmlNodeLocal.Attributes?["file"] != null select xmlNodeLocal.Attributes["file"].InnerText).ToList();
            strs.Sort();
            foreach (string str in strs)
                cboFile.Items.Add(str);
            cboFile.SelectedIndex = 0;
        }
        private void txtSearch_GotFocus(object sender, EventArgs e)
        {
            txtSearch.SelectAll();
        }

        private void txtSearch_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == '\r') && !txtSearch.AcceptsReturn)
                btnSearch.PerformClick();
        }
        #endregion

        #region Methods
        private void LoadLanguageFiles()
        {
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            _strCode = Language.Substring(Language.IndexOf("(") + 1, 2);
            _objTranslationDoc.Load(string.Concat(_strPath, "lang\\", _strCode, ".xml"));
            _objDataDoc.Load(string.Concat(_strPath, "lang\\", _strCode, "_data.xml"));
        }

        private void LoadSection()
        {
            if (cboSection.SelectedIndex < 0)
                return;
            dgvTranslate.Visible = false;
            dgvSection.Visible = true;
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("english");
            dataTable.Columns.Add("text");
            dataTable.Columns.Add("book");
            dataTable.Columns.Add("page");
            dataTable.Columns.Add("translated");
            string strFileName = cboFile.Text;
            XmlNode selectSingleNode = _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", strFileName, "\"]/", cboSection.Text));
            if (selectSingleNode != null)
            {
                XmlNodeList childNodes =
                    selectSingleNode
                        .ChildNodes;
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(string.Concat(_strPath, "data\\", strFileName));
                foreach (XmlNode childNode in childNodes)
                {
                    string strName;
                    string strPage = string.Empty;
                    string strTranslated = string.Empty;
                    string strSource = string.Empty;
                    bool blnTranslated = false;
                    if (childNode["name"] == null)
                    {
                        strName = childNode.InnerText;
                        if (childNode.Attributes?["translate"] != null)
                            strTranslated = childNode.Attributes["translate"].InnerText;
                        if (childNode.Attributes?["translated"] != null)
                            blnTranslated = Convert.ToBoolean(childNode.Attributes["translated"].InnerText);
                    }
                    else
                    {
                        strName = childNode["name"].InnerText;
                        if (childNode["page"] != null)
                            strPage = childNode["page"].InnerText;
                        string[] strArrays = { "/chummer/", cboSection.Text, "/*[name=\"", strName, "\"]" };
                        XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode(string.Concat(strArrays));
                        if (xmlNodeLocal != null)
                            strSource = xmlNodeLocal["source"]?.InnerText;
                        strTranslated = childNode["translate"]?.InnerText;
                        blnTranslated = childNode.Attributes?["translated"] != null
                            ? Convert.ToBoolean(childNode.Attributes["translated"].InnerText)
                            : strName != strTranslated;
                    }
                    if ((!chkOnlyTranslation.Checked || strName != strTranslated) && chkOnlyTranslation.Checked)
                        continue;
                    DataRowCollection rows = dataTable.Rows;
                    object[] objArray = { strName, strTranslated, strSource, strPage, blnTranslated };
                    rows.Add(objArray);
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
        }

        private void LoadSections()
        {
            cboSection.Items.Clear();
            string strFileName = cboFile.Text;
            XmlNode xmlNode = _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", strFileName, "\"]"));
            if (xmlNode != null)
            {
                List<string> strs = (from XmlNode childNode in xmlNode.ChildNodes select childNode.Name).ToList();
                strs.Sort();
                foreach (string str in strs)
                    cboSection.Items.Add(str);
            }
            cboSection.Visible = true;
        }

        private void LoadStrings()
        {
            dgvTranslate.Visible = true;
            dgvSection.Visible = false;
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(string.Concat(_strPath, "lang\\en-US.xml"));
            var dataTable = new DataTable("strings");
            dataTable.Columns.Add("key");
            dataTable.Columns.Add("english");
            dataTable.Columns.Add("text");
            dataTable.Columns.Add("translated");
            //XmlNodeList xmlNodeList = _objTranslationDoc.SelectNodes("/chummer/strings/string");
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/chummer/strings/string");
            if (xmlNodeList != null)
                foreach (XmlNode xmlNodeEnglish in xmlNodeList)
                {
                    string strKey = xmlNodeEnglish["key"]?.InnerText;
                    string strEnglish = xmlNodeEnglish["text"]?.InnerText;
                    string strTranslated = xmlNodeEnglish["text"]?.InnerText;
                    var blnTranslated = false;
                    XmlNode xmlNodeLocal =
                        _objTranslationDoc.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", strKey, "\"]"));
                    if (xmlNodeLocal != null)
                    {
                        strTranslated = xmlNodeLocal["text"]?.InnerText;
                        blnTranslated = xmlNodeEnglish.Attributes?["translated"] != null
                            ? Convert.ToBoolean(xmlNodeEnglish.Attributes["translated"].InnerText)
                            : strEnglish != strTranslated;
                    }
                    if (chkOnlyTranslation.Checked && (strEnglish == strTranslated || string.IsNullOrWhiteSpace(strTranslated)) || !chkOnlyTranslation.Checked)
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
        }

        private void SetPath()
        {
            _strPath = Application.StartupPath;
            if (!_strPath.EndsWith("\\"))
            {
                FrmTranslate frmTranslate = this;
                frmTranslate._strPath = string.Concat(frmTranslate._strPath, "\\");
            }
        }
        #endregion

        private void Save(XmlDocument objXmlDocument, bool blnData = true)
        {
            string strPath = string.Concat(_strPath, "lang\\", _strCode, blnData ? "_data.xml" : ".xml");
            var xwsSettings = new XmlWriterSettings { IndentChars = ("\t"), Indent = true};
            using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
            {
                objXmlDocument.Save(xwWriter);
            }
        }
    }
}
