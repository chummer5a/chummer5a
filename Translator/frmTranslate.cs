using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Translator
{
    public partial class frmTranslate : Form
    {
        private bool _blnLoading;
        private readonly XmlDocument _objDataDoc = new XmlDocument();
        private readonly XmlDocument _objTranslationDoc = new XmlDocument();
        private string _strCode = string.Empty;
        private string _strPath = string.Empty;

        public frmTranslate()
        {
            InitializeComponent();
        }

        public string Language { get; set; }

        #region Control Events
        private void chkOnlyTranslation_CheckedChanged(object sender, EventArgs e)
        {
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
            XmlAttribute itemOf;
            XmlAttribute str;
            if (_blnLoading || (e.RowIndex < 0))
                return;
            bool flag = true;
            DataGridViewRow item = dgvSection.Rows[e.RowIndex];
            var dataGridViewCheckBoxCell = item.Cells["sectionTranslated"] as DataGridViewCheckBoxCell;
            if (dataGridViewCheckBoxCell.Value is DBNull)
                flag = false;
            else if (!Convert.ToBoolean(dataGridViewCheckBoxCell.Value))
                flag = false;
            if (!(item.Cells["sectionEnglish"].Value.ToString() == item.Cells["sectionText"].Value.ToString()) || flag)
                item.DefaultCellStyle.BackColor = Color.Empty;
            else
                item.DefaultCellStyle.BackColor = Color.Wheat;
            string str1 = item.Cells["sectionText"].Value.ToString();
            string str2 = item.Cells["sectionEnglish"].Value.ToString();
            string str3 = item.Cells["sectionPage"].Value.ToString();
            XmlDocument xmlDocument = _objDataDoc;
            string[] text =
            {
                "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "//name[text()=\"",
                str2, "\"]/.."
            };
            XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode(string.Concat(text));
            if (xmlNodeLocal == null)
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                string[] strArrays =
                {
                    "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "/*[text()=\"",
                    str2, "\"]"
                };
                xmlNodeLocal = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
                xmlNodeLocal.Attributes["translate"].InnerText = str1;
                if (!flag)
                {
                    str = xmlNodeLocal.Attributes["translated"];
                    if (str != null)
                        xmlNodeLocal.Attributes.Remove(str);
                }
                else
                {
                    str = xmlNodeLocal.Attributes["translated"];
                    if (str == null)
                        str = _objDataDoc.CreateAttribute("translated");
                    else
                        str.InnerText = flag.ToString();
                    str.InnerText = flag.ToString();
                    xmlNodeLocal.Attributes.Append(str);
                }
            }
            else
            {
                xmlNodeLocal["translate"].InnerText = str1;
                try
                {
                    xmlNodeLocal["page"].InnerText = str3;
                }
                catch
                {
                }
                if (!flag)
                {
                    itemOf = xmlNodeLocal.Attributes["translated"];
                    if (itemOf != null)
                        xmlNodeLocal.Attributes.Remove(itemOf);
                }
                else
                {
                    itemOf = xmlNodeLocal.Attributes["translated"];
                    if (itemOf == null)
                    {
                        itemOf = _objDataDoc.CreateAttribute("translated");
                        itemOf.InnerText = flag.ToString();
                        xmlNodeLocal.Attributes.Append(itemOf);
                    }
                    else
                    {
                        itemOf.InnerText = flag.ToString();
                    }
                }
            }
            Save(_objDataDoc);
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
                if (!(row.Cells["sectionEnglish"].Value.ToString() == row.Cells["sectionText"].Value.ToString()) ||
                    Convert.ToBoolean(row.Cells["sectionTranslated"].Value))
                    continue;
                row.DefaultCellStyle.BackColor = Color.Wheat;
            }
        }

        private void dgvTranslate_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_blnLoading || (e.RowIndex < 0))
                return;
            bool flag = true;
            DataGridViewRow item = dgvTranslate.Rows[e.RowIndex];
            var dataGridViewCheckBoxCell = item.Cells["translated"] as DataGridViewCheckBoxCell;
            if (dataGridViewCheckBoxCell?.Value is DBNull)
                flag = false;
            if (item.Cells["english"].Value.ToString() != item.Cells["text"].Value.ToString() || flag)
                item.DefaultCellStyle.BackColor = Color.Empty;
            else
                item.DefaultCellStyle.BackColor = Color.Wheat;
            string str = item.Cells["text"].Value.ToString();
            string str1 = item.Cells["key"].Value.ToString();
            XmlNode xmlNodeLocal = _objTranslationDoc.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", str1, "\"]"));
            if (xmlNodeLocal != null)
            {
                xmlNodeLocal["text"].InnerText = str;
            }
            else
            {
                XmlNode newNode = _objTranslationDoc.CreateNode(XmlNodeType.Element, "string", null);

                XmlElement elem = _objTranslationDoc.CreateElement("key");
                XmlText text = _objTranslationDoc.CreateTextNode(str1);
                newNode.AppendChild(elem);
                newNode.LastChild.AppendChild(text);

                elem = _objTranslationDoc.CreateElement("text");
                text = _objTranslationDoc.CreateTextNode(str);
                newNode.AppendChild(elem);
                newNode.LastChild.AppendChild(text);

                XmlNode root = _objTranslationDoc.SelectSingleNode("/chummer/strings/.");
                root.AppendChild(newNode);
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
                if (!(row.Cells["english"].Value.ToString() == row.Cells["text"].Value.ToString()) ||
                    Convert.ToBoolean(row.Cells["translated"].Value))
                    continue;
                row.DefaultCellStyle.BackColor = Color.Wheat;
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
            List<string> strs = new List<string>();
            Text = string.Concat("Translating ", Language);
            SetPath();
            LoadLanguageFiles();
            cboFile.Items.Add("Strings");
            foreach (XmlNode xmlNodeLocal in _objDataDoc.SelectNodes("/chummer/chummer"))
            {
                if (xmlNodeLocal.Attributes["file"] == null)
                    continue;
                strs.Add(xmlNodeLocal.Attributes["file"].InnerText);
            }
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
            string text = cboFile.Text;
            XmlNodeList childNodes =
                _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]/", cboSection.Text))
                    .ChildNodes;
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(string.Concat(_strPath, "data\\", text));
            foreach (XmlNode childNode in childNodes)
            {
                string innerText = string.Empty;
                string str = string.Empty;
                string innerText1 = string.Empty;
                string str1 = string.Empty;
                bool flag = false;
                if (childNode["name"] == null)
                {
                    innerText = childNode.InnerText;
                    if (childNode.Attributes["translate"] != null)
                        innerText1 = childNode.Attributes["translate"].InnerText;
                    if (childNode.Attributes["translated"] != null)
                        flag = Convert.ToBoolean(childNode.Attributes["translated"].InnerText);
                }
                else
                {
                    innerText = childNode["name"].InnerText;
                    if (childNode["page"] != null)
                        str = childNode["page"].InnerText;
                    try
                    {
                        string[] strArrays = { "/chummer/", cboSection.Text, "/*[name=\"", innerText, "\"]" };
                        XmlNode xmlNodeLocal = xmlDocument.SelectSingleNode(string.Concat(strArrays));
                        if (xmlNodeLocal != null)
                            str1 = xmlNodeLocal["source"].InnerText;
                    }
                    catch
                    {
                    }
                    innerText1 = childNode["translate"].InnerText;
                    if (childNode.Attributes["translated"] != null)
                        flag = Convert.ToBoolean(childNode.Attributes["translated"].InnerText);
                }
                if ((!chkOnlyTranslation.Checked || innerText != innerText1) && chkOnlyTranslation.Checked)
                    continue;
                DataRowCollection rows = dataTable.Rows;
                object[] objArray = { innerText, innerText1, str1, str, flag };
                rows.Add(objArray);
            }
            var dataSet = new DataSet("strings");
            dataSet.Tables.Add(dataTable);
            dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvSection.DataSource = dataSet;
            dgvSection.DataMember = "strings";
            foreach (DataGridViewRow row in dgvSection.Rows)
            {
                if (row.Cells["sectionEnglish"].Value.ToString() != row.Cells["sectionText"].Value.ToString() ||
                    Convert.ToBoolean(row.Cells["sectionTranslated"].Value))
                    continue;
                row.DefaultCellStyle.BackColor = Color.Wheat;
            }
        }

        private void LoadSections()
        {
            cboSection.Items.Clear();
            string text = cboFile.Text;
            List<string> strs = new List<string>();
            XmlNode xmlNode = _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]"));
            foreach (XmlNode childNode in xmlNode.ChildNodes)
                strs.Add(childNode.Name);
            strs.Sort();
            foreach (string str in strs)
                cboSection.Items.Add(str);
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
                    string innerText = xmlNodeEnglish["key"].InnerText;
                    string str = xmlNodeEnglish["text"].InnerText;
                    XmlNode xmlNodeLocal =
                        xmlDocument.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", innerText, "\"]"));
                    string innerText1 = string.Empty;
                    if (xmlNodeLocal != null)
                        innerText1 = xmlNodeLocal["text"].InnerText;
                    var flag = false;
                    if (xmlNodeEnglish.Attributes["translated"] != null)
                        flag = Convert.ToBoolean(xmlNodeEnglish.Attributes["translated"].InnerText);
                    if (chkOnlyTranslation.Checked && (str == innerText1 || string.IsNullOrWhiteSpace(innerText1)) || !chkOnlyTranslation.Checked)
                    {
                        DataRowCollection rows = dataTable.Rows;
                        object[] objArray = { innerText, str, innerText1, flag };
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
                if (!(row.Cells["english"].Value.ToString() == row.Cells["text"].Value.ToString()) ||
                    Convert.ToBoolean(row.Cells["translated"].Value))
                    continue;
                row.DefaultCellStyle.BackColor = Color.Wheat;
            }
            cboSection.Visible = false;
        }

        private void SetPath()
        {
            _strPath = Application.StartupPath;
            if (!_strPath.EndsWith("\\"))
            {
                frmTranslate _frmTranslate = this;
                _frmTranslate._strPath = string.Concat(_frmTranslate._strPath, "\\");
            }
        }
        #endregion

        private void Save(XmlDocument objXmlDocument, bool blnData = true)
        {
            string strPath = string.Empty;
            strPath = string.Concat(_strPath, "lang\\", _strCode, blnData ? "_data.xml" : ".xml");
            XmlWriterSettings xwsSettings = new XmlWriterSettings { IndentChars = ("\t"), Indent = true};
            using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
            {
                objXmlDocument.Save(xwWriter);
            }
        }
    }
}
