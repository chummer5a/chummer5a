using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace Translator
{
	public class frmTranslate : Form
	{
		private string _strLang = string.Empty;

		private string _strPath = string.Empty;

		private string _strCode = string.Empty;

		private bool _blnLoading;

		private XmlDocument _objDataDoc = new XmlDocument();

		private XmlDocument _objDoc = new XmlDocument();

		XmlWriterSettings xwsSettings = new XmlWriterSettings();

		private IContainer components;

		private ComboBox cboFile;

		private DataGridView dgvTranslate;

		private ComboBox cboSection;

		private DataGridViewTextBoxColumn key;

		private DataGridViewTextBoxColumn english;

		private DataGridViewTextBoxColumn text;

		private DataGridViewCheckBoxColumn Translated;

		private CheckBox chkOnlyTranslation;

		private DataGridView dgvSection;

		private DataGridViewTextBoxColumn sectionEnglish;

		private DataGridViewTextBoxColumn sectionText;

		private DataGridViewTextBoxColumn sectionBook;

		private DataGridViewTextBoxColumn sectionPage;

		private DataGridViewCheckBoxColumn sectionTranslated;

		private Button btnSearch;

		private TextBox txtSearch;

		public string Language
		{
			get
			{
				return _strLang;
			}
			set
			{
				_strLang = value;
			}
		}

		public frmTranslate()
		{
			xwsSettings.IndentChars = "\t";
			xwsSettings.Indent = true;
			xwsSettings.NewLineChars = "\n";
			InitializeComponent();
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
				{
					for (int j = 0; j <= columnCount; j++)
					{
						if ((i > rowIndex || j > columnIndex) && dgvSection.Rows[i].Cells[j].Value.ToString().IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							dgvSection.ClearSelection();
							dgvSection.Rows[i].Cells[j].Selected = true;
							dgvSection.FirstDisplayedScrollingRowIndex = i;
							dgvSection.Select();
							return;
						}
					}
				}
				for (int k = 0; k < rowIndex; k++)
				{
					for (int l = 0; l <= columnCount; l++)
					{
						if (dgvSection.Rows[k].Cells[l].Value.ToString().IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							dgvSection.ClearSelection();
							dgvSection.Rows[k].Cells[l].Selected = true;
							dgvSection.FirstDisplayedScrollingRowIndex = k;
							dgvSection.Select();
							return;
						}
					}
				}
				MessageBox.Show("Search text was not found.");
				return;
			}
			int num = dgvTranslate.RowCount;
			int columnCount1 = dgvTranslate.ColumnCount;
			int rowIndex1 = dgvTranslate.SelectedCells[0].RowIndex;
			int columnIndex1 = dgvTranslate.SelectedCells[0].ColumnIndex;
			for (int m = rowIndex1; m < num; m++)
			{
				for (int n = 0; n < columnCount1; n++)
				{
					if ((m > rowIndex1 || n > columnIndex1) && dgvTranslate.Rows[m].Cells[n].Value.ToString().IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						dgvTranslate.ClearSelection();
						dgvTranslate.Rows[m].Cells[n].Selected = true;
						dgvTranslate.FirstDisplayedScrollingRowIndex = m;
						dgvTranslate.Select();
						return;
					}
				}
			}
			for (int o = 0; o < rowIndex1; o++)
			{
				for (int p = 0; p < columnCount1; p++)
				{
					if (dgvTranslate.Rows[o].Cells[p].Value.ToString().IndexOf(txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						dgvTranslate.ClearSelection();
						dgvTranslate.Rows[o].Cells[p].Selected = true;
						dgvTranslate.FirstDisplayedScrollingRowIndex = o;
						dgvTranslate.Select();
						return;
					}
				}
			}
			MessageBox.Show("Search text was not found.");
		}

		private void cboFile_SelectedIndexChanged(object sender, EventArgs e)
		{
			_blnLoading = true;
			if (cboFile.SelectedIndex == 0)
			{
				LoadStrings();
			}
			else if (cboFile.SelectedIndex > 0)
			{
				LoadSections();
			}
			_blnLoading = false;
		}

		private void cboSection_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadSection();
		}

		private void chkOnlyTranslation_CheckedChanged(object sender, EventArgs e)
		{
			if (cboFile.Text == "Strings")
			{
				LoadStrings();
				return;
			}
			LoadSection();
		}

		private void dgvSection_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex == 4 && e.RowIndex != -1)
			{
				dgvSection.EndEdit();
			}
		}

		private void dgvSection_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			XmlAttribute itemOf;
			XmlAttribute str;
			if (_blnLoading || e.RowIndex < 0)
			{
				return;
			}
			bool flag = true;
			DataGridViewRow item = dgvSection.Rows[e.RowIndex];
			DataGridViewCheckBoxCell dataGridViewCheckBoxCell = item.Cells["sectionTranslated"] as DataGridViewCheckBoxCell;
			if (dataGridViewCheckBoxCell.Value is DBNull)
			{
				flag = false;
			}
			else if (!Convert.ToBoolean(dataGridViewCheckBoxCell.Value))
			{
				flag = false;
			}
			if (!(item.Cells["sectionEnglish"].Value.ToString() == item.Cells["sectionText"].Value.ToString()) || flag)
			{
				item.DefaultCellStyle.BackColor = Color.Empty;
			}
			else
			{
				item.DefaultCellStyle.BackColor = Color.Wheat;
			}
			string str1 = item.Cells["sectionText"].Value.ToString();
			string str2 = item.Cells["sectionEnglish"].Value.ToString();
			string str3 = item.Cells["sectionPage"].Value.ToString();
			XmlDocument xmlDocument = _objDataDoc;
			string[] text = new string[] { "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "//name[text()=\"", str2, "\"]/.." };
			XmlNode xmlNodes = xmlDocument.SelectSingleNode(string.Concat(text));
			if (xmlNodes == null)
			{
				XmlDocument xmlDocument1 = _objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file=\"", cboFile.Text, "\"]/", cboSection.Text, "/*[text()=\"", str2, "\"]" };
				xmlNodes = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				xmlNodes.Attributes["translate"].InnerText = str1;
				if (!flag)
				{
					str = xmlNodes.Attributes["translated"];
					if (str != null)
					{
						xmlNodes.Attributes.Remove(str);
					}
				}
				else
				{
					str = xmlNodes.Attributes["translated"];
					if (str == null)
					{
						str = _objDataDoc.CreateAttribute("translated");
					}
					else
					{
						str.InnerText = flag.ToString();
					}
					str.InnerText = flag.ToString();
					xmlNodes.Attributes.Append(str);
				}
			}
			else
			{
				xmlNodes["translate"].InnerText = str1;
				try
				{
					xmlNodes["page"].InnerText = str3;
				}
				catch
				{
				}
				if (!flag)
				{
					itemOf = xmlNodes.Attributes["translated"];
					if (itemOf != null)
					{
						xmlNodes.Attributes.Remove(itemOf);
					}
				}
				else
				{
					itemOf = xmlNodes.Attributes["translated"];
					if (itemOf == null)
					{
						itemOf = _objDataDoc.CreateAttribute("translated");
						itemOf.InnerText = flag.ToString();
						xmlNodes.Attributes.Append(itemOf);
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
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				txtSearch.Focus();
			}
		}

		private void dgvSection_Sorted(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in (IEnumerable)dgvSection.Rows)
			{
				if (!(row.Cells["sectionEnglish"].Value.ToString() == row.Cells["sectionText"].Value.ToString()) || Convert.ToBoolean(row.Cells["sectionTranslated"].Value))
				{
					continue;
				}
				row.DefaultCellStyle.BackColor = Color.Wheat;
			}
		}

		private void dgvTranslate_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (_blnLoading || e.RowIndex < 0)
			{
				return;
			}
			bool flag = true;
			DataGridViewRow item = dgvTranslate.Rows[e.RowIndex];
			if ((item.Cells["translated"] as DataGridViewCheckBoxCell).Value is DBNull)
			{
				flag = false;
			}
			if (!(item.Cells["english"].Value.ToString() == item.Cells["text"].Value.ToString()) || flag)
			{
				item.DefaultCellStyle.BackColor = Color.Empty;
			}
			else
			{
				item.DefaultCellStyle.BackColor = Color.Wheat;
			}
			string str = item.Cells["text"].Value.ToString();
			string str1 = item.Cells["key"].Value.ToString();
			XmlNode xmlNodes = _objDoc.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", str1, "\"]"));
			xmlNodes["text"].InnerText = str;
			Save(_objDoc, false);
		}

		private void dgvTranslate_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3)
			{
				btnSearch.PerformClick();
				return;
			}
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				txtSearch.Focus();
			}
		}

		private void dgvTranslate_Sorted(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in (IEnumerable)dgvTranslate.Rows)
			{
				if (!(row.Cells["english"].Value.ToString() == row.Cells["text"].Value.ToString()) || Convert.ToBoolean(row.Cells["translated"].Value))
				{
					continue;
				}
				row.DefaultCellStyle.BackColor = Color.Wheat;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void frmTranslate_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3)
			{
				btnSearch.PerformClick();
				return;
			}
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				txtSearch.Focus();
			}
		}

		private void frmTranslate_Load(object sender, EventArgs e)
		{
			List<string> strs = new List<string>();
			Text = string.Concat("Translating ", _strLang);
			SetPath();
			LoadLanaguageFiles();
			cboFile.Items.Add("Strings");
			foreach (XmlNode xmlNodes in _objDataDoc.SelectNodes("/chummer/chummer"))
			{
				if (xmlNodes.Attributes["file"] == null)
				{
					continue;
				}
				strs.Add(xmlNodes.Attributes["file"].InnerText);
			}
			strs.Sort();
			foreach (string str in strs)
			{
				cboFile.Items.Add(str);
			}
			cboFile.SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
			DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			cboFile = new ComboBox();
			dgvTranslate = new DataGridView();
			key = new DataGridViewTextBoxColumn();
			english = new DataGridViewTextBoxColumn();
			text = new DataGridViewTextBoxColumn();
			Translated = new DataGridViewCheckBoxColumn();
			cboSection = new ComboBox();
			chkOnlyTranslation = new CheckBox();
			dgvSection = new DataGridView();
			sectionEnglish = new DataGridViewTextBoxColumn();
			sectionText = new DataGridViewTextBoxColumn();
			sectionBook = new DataGridViewTextBoxColumn();
			sectionPage = new DataGridViewTextBoxColumn();
			sectionTranslated = new DataGridViewCheckBoxColumn();
			btnSearch = new Button();
			txtSearch = new TextBox();
			((ISupportInitialize)dgvTranslate).BeginInit();
			((ISupportInitialize)dgvSection).BeginInit();
			SuspendLayout();
			cboFile.DropDownStyle = ComboBoxStyle.DropDownList;
			cboFile.FormattingEnabled = true;
			cboFile.Location = new Point(12, 12);
			cboFile.Name = "cboFile";
			cboFile.Size = new Size(218, 21);
			cboFile.TabIndex = 0;
			cboFile.SelectedIndexChanged += new EventHandler(cboFile_SelectedIndexChanged);
			dgvTranslate.AllowUserToAddRows = false;
			dgvTranslate.AllowUserToDeleteRows = false;
			dgvTranslate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			dgvTranslate.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewColumnCollection columns = dgvTranslate.Columns;
			DataGridViewColumn[] translated = new DataGridViewColumn[] { key, english, text, Translated };
			columns.AddRange(translated);
			dgvTranslate.Location = new Point(12, 39);
			dgvTranslate.MultiSelect = false;
			dgvTranslate.Name = "dgvTranslate";
			dgvTranslate.RowHeadersVisible = false;
			dgvTranslate.Size = new Size(1197, 476);
			dgvTranslate.TabIndex = 7;
			dgvTranslate.CellValueChanged += new DataGridViewCellEventHandler(dgvTranslate_CellValueChanged);
			dgvTranslate.Sorted += new EventHandler(dgvTranslate_Sorted);
			dgvTranslate.KeyDown += new KeyEventHandler(dgvTranslate_KeyDown);
			key.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			key.DataPropertyName = "key";
			dataGridViewCellStyle.WrapMode = DataGridViewTriState.True;
			key.DefaultCellStyle = dataGridViewCellStyle;
			key.HeaderText = "Key";
			key.Name = "key";
			key.ReadOnly = true;
			key.Width = 300;
			english.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			english.DataPropertyName = "english";
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			english.DefaultCellStyle = dataGridViewCellStyle1;
			english.HeaderText = "English";
			english.Name = "english";
			english.ReadOnly = true;
			english.Width = 400;
			text.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			text.DataPropertyName = "text";
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
			text.DefaultCellStyle = dataGridViewCellStyle2;
			text.HeaderText = "Text";
			text.Name = "text";
			text.Width = 400;
			Translated.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			Translated.DataPropertyName = "translated";
			Translated.HeaderText = "Translated";
			Translated.Name = "Translated";
			Translated.SortMode = DataGridViewColumnSortMode.Automatic;
			Translated.Width = 70;
			cboSection.DropDownStyle = ComboBoxStyle.DropDownList;
			cboSection.FormattingEnabled = true;
			cboSection.Location = new Point(236, 12);
			cboSection.Name = "cboSection";
			cboSection.Size = new Size(218, 21);
			cboSection.TabIndex = 1;
			cboSection.Visible = false;
			cboSection.SelectedIndexChanged += new EventHandler(cboSection_SelectedIndexChanged);
			chkOnlyTranslation.AutoSize = true;
			chkOnlyTranslation.Location = new Point(460, 14);
			chkOnlyTranslation.Name = "chkOnlyTranslation";
			chkOnlyTranslation.Size = new Size(199, 17);
			chkOnlyTranslation.TabIndex = 2;
			chkOnlyTranslation.Text = "Only Show Text Needing Translation";
			chkOnlyTranslation.UseVisualStyleBackColor = true;
			chkOnlyTranslation.CheckedChanged += new EventHandler(chkOnlyTranslation_CheckedChanged);
			dgvSection.AllowUserToAddRows = false;
			dgvSection.AllowUserToDeleteRows = false;
			dgvSection.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			dgvSection.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewColumnCollection dataGridViewColumnCollections = dgvSection.Columns;
			DataGridViewColumn[] dataGridViewColumnArray = new DataGridViewColumn[] { sectionEnglish, sectionText, sectionBook, sectionPage, sectionTranslated };
			dataGridViewColumnCollections.AddRange(dataGridViewColumnArray);
			dgvSection.Location = new Point(12, 39);
			dgvSection.MultiSelect = false;
			dgvSection.Name = "dgvSection";
			dgvSection.RowHeadersVisible = false;
			dgvSection.Size = new Size(1197, 476);
			dgvSection.TabIndex = 5;
			dgvSection.Visible = false;
			dgvSection.CellValueChanged += new DataGridViewCellEventHandler(dgvSection_CellValueChanged);
			dgvSection.Sorted += new EventHandler(dgvSection_Sorted);
			dgvSection.KeyDown += new KeyEventHandler(dgvSection_KeyDown);
			dgvSection.CellMouseUp += new DataGridViewCellMouseEventHandler(dgvSection_CellMouseUp);
			sectionEnglish.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			sectionEnglish.DataPropertyName = "english";
			dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
			sectionEnglish.DefaultCellStyle = dataGridViewCellStyle3;
			sectionEnglish.HeaderText = "English";
			sectionEnglish.Name = "sectionEnglish";
			sectionEnglish.ReadOnly = true;
			sectionEnglish.Width = 480;
			sectionText.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			sectionText.DataPropertyName = "text";
			dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
			sectionText.DefaultCellStyle = dataGridViewCellStyle4;
			sectionText.HeaderText = "Text";
			sectionText.Name = "sectionText";
			sectionText.Width = 480;
			sectionBook.DataPropertyName = "book";
			sectionBook.HeaderText = "Book";
			sectionBook.Name = "sectionBook";
			sectionBook.ReadOnly = true;
			sectionBook.Width = 70;
			sectionPage.DataPropertyName = "page";
			sectionPage.HeaderText = "Page";
			sectionPage.Name = "sectionPage";
			sectionPage.Width = 70;
			sectionTranslated.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			sectionTranslated.DataPropertyName = "translated";
			sectionTranslated.HeaderText = "Translated";
			sectionTranslated.Name = "sectionTranslated";
			sectionTranslated.SortMode = DataGridViewColumnSortMode.Automatic;
			sectionTranslated.Width = 70;
			btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSearch.Location = new Point(1134, 10);
			btnSearch.Name = "btnSearch";
			btnSearch.Size = new Size(75, 23);
			btnSearch.TabIndex = 4;
			btnSearch.Text = "Search";
			btnSearch.UseVisualStyleBackColor = true;
			btnSearch.Click += new EventHandler(btnSearch_Click);
			txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			txtSearch.Location = new Point(962, 12);
			txtSearch.Name = "txtSearch";
			txtSearch.Size = new Size(166, 20);
			txtSearch.TabIndex = 3;
			txtSearch.KeyPress += new KeyPressEventHandler(txtSearch_KeyPressed);
			txtSearch.GotFocus += new EventHandler(txtSearch_GotFocus);
			AutoScaleDimensions = new SizeF(6f, 13f);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1221, 527);
			Controls.Add(txtSearch);
			Controls.Add(btnSearch);
			Controls.Add(chkOnlyTranslation);
			Controls.Add(cboSection);
			Controls.Add(cboFile);
			Controls.Add(dgvSection);
			Controls.Add(dgvTranslate);
			KeyPreview = true;
			Name = "frmTranslate";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Translating";
			Load += new EventHandler(frmTranslate_Load);
			KeyDown += new KeyEventHandler(frmTranslate_KeyDown);
			((ISupportInitialize)dgvTranslate).EndInit();
			((ISupportInitialize)dgvSection).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		private void LoadLanaguageFiles()
		{
			_strCode = _strLang.Substring(_strLang.IndexOf("(") + 1, 2);
			_objDoc.Load(string.Concat(_strPath, "lang\\", _strCode, ".xml"));
			_objDataDoc.Load(string.Concat(_strPath, "lang\\", _strCode, "_data.xml"));
		}

		private void LoadSection()
		{
			if (cboSection.SelectedIndex < 0)
			{
				return;
			}
			dgvTranslate.Visible = false;
			dgvSection.Visible = true;
			DataTable dataTable = new DataTable("strings");
			dataTable.Columns.Add("english");
			dataTable.Columns.Add("text");
			dataTable.Columns.Add("book");
			dataTable.Columns.Add("page");
			dataTable.Columns.Add("translated");
			string text = cboFile.Text;
			XmlNodeList childNodes = _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]/", cboSection.Text)).ChildNodes;
			XmlDocument xmlDocument = new XmlDocument();
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
					{
						innerText1 = childNode.Attributes["translate"].InnerText;
					}
					if (childNode.Attributes["translated"] != null)
					{
						flag = Convert.ToBoolean(childNode.Attributes["translated"].InnerText);
					}
				}
				else
				{
					innerText = childNode["name"].InnerText;
					if (childNode["page"] != null)
					{
						str = childNode["page"].InnerText;
					}
					try
					{
						string[] strArrays = new string[] { "/chummer/", cboSection.Text, "/*[name=\"", innerText, "\"]" };
						XmlNode xmlNodes = xmlDocument.SelectSingleNode(string.Concat(strArrays));
						if (xmlNodes != null)
						{
							str1 = xmlNodes["source"].InnerText;
						}
					}
					catch
					{
					}
					innerText1 = childNode["translate"].InnerText;
					if (childNode.Attributes["translated"] != null)
					{
						flag = Convert.ToBoolean(childNode.Attributes["translated"].InnerText);
					}
				}
				if ((!chkOnlyTranslation.Checked || !(innerText == innerText1)) && chkOnlyTranslation.Checked)
				{
					continue;
				}
				DataRowCollection rows = dataTable.Rows;
				object[] objArray = new object[] { innerText, innerText1, str1, str, flag };
				rows.Add(objArray);
			}
			DataSet dataSet = new DataSet("strings");
			dataSet.Tables.Add(dataTable);
			dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			dgvSection.DataSource = dataSet;
			dgvSection.DataMember = "strings";
			foreach (DataGridViewRow row in (IEnumerable)dgvSection.Rows)
			{
				if (!(row.Cells["sectionEnglish"].Value.ToString() == row.Cells["sectionText"].Value.ToString()) || Convert.ToBoolean(row.Cells["sectionTranslated"].Value))
				{
					continue;
				}
				row.DefaultCellStyle.BackColor = Color.Wheat;
			}
		}

		private void LoadSections()
		{
			cboSection.Items.Clear();
			string text = cboFile.Text;
			List<string> strs = new List<string>();
			XmlNode xmlNodes = _objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]"));
			foreach (XmlNode childNode in xmlNodes.ChildNodes)
			{
				strs.Add(childNode.Name);
			}
			strs.Sort();
			foreach (string str in strs)
			{
				cboSection.Items.Add(str);
			}
			cboSection.Visible = true;
		}

		private void LoadStrings()
		{
			dgvTranslate.Visible = true;
			dgvSection.Visible = false;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(_strPath, "lang\\en-US.xml"));
			DataTable dataTable = new DataTable("strings");
			dataTable.Columns.Add("key");
			dataTable.Columns.Add("english");
			dataTable.Columns.Add("text");
			dataTable.Columns.Add("translated");
			foreach (XmlNode xmlNodes in _objDoc.SelectNodes("/chummer/strings/string"))
			{
				try
				{
					string innerText = xmlNodes["key"].InnerText;
					string str = xmlNodes["text"].InnerText;
					XmlNode xmlNodes1 = xmlDocument.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", innerText, "\"]"));
					string innerText1 = string.Empty;
					if (xmlNodes1 != null)
					{
						innerText1 = xmlNodes1["text"].InnerText;
					}
					bool flag = false;
					if (xmlNodes.Attributes["translated"] != null)
					{
						flag = Convert.ToBoolean(xmlNodes.Attributes["translated"].InnerText);
					}
					if (chkOnlyTranslation.Checked && str == innerText1 || !chkOnlyTranslation.Checked)
					{
						DataRowCollection rows = dataTable.Rows;
						object[] objArray = new object[] { innerText, innerText1, str, flag };
						rows.Add(objArray);
					}
				}
				catch
				{
				}
			}
			DataSet dataSet = new DataSet("strings");
			dataSet.Tables.Add(dataTable);
			dgvTranslate.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			dgvTranslate.DataSource = dataSet;
			dgvTranslate.DataMember = "strings";
			foreach (DataGridViewRow row in (IEnumerable)dgvTranslate.Rows)
			{
				if (!(row.Cells["english"].Value.ToString() == row.Cells["text"].Value.ToString()) || Convert.ToBoolean(row.Cells["translated"].Value))
				{
					continue;
				}
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

		private void txtSearch_GotFocus(object sender, EventArgs e)
		{
			txtSearch.SelectAll();
		}

		private void txtSearch_KeyPressed(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' && !txtSearch.AcceptsReturn)
			{
				btnSearch.PerformClick();
			}
		}

		private void Save(XmlDocument objXmlDocument, bool blnData = true)
		{
			string strPath = string.Empty;
			strPath = string.Concat(_strPath, "lang\\", _strCode, blnData ? "_data.xml" : ".xml");
			using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
				objXmlDocument.Save(xwWriter);
		}
	}
}