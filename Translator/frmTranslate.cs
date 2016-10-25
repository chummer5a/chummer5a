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
		private string _strLang = "";

		private string _strPath = "";

		private string _strCode = "";

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
				return this._strLang;
			}
			set
			{
				this._strLang = value;
			}
		}

		public frmTranslate()
		{
			xwsSettings.IndentChars = "\t";
			xwsSettings.Indent = true;
			xwsSettings.NewLineChars = "\n";
			this.InitializeComponent();
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			if (this.dgvSection.Visible)
			{
				int rowCount = this.dgvSection.RowCount;
				int columnCount = this.dgvSection.ColumnCount;
				int rowIndex = this.dgvSection.SelectedCells[0].RowIndex;
				int columnIndex = this.dgvSection.SelectedCells[0].ColumnIndex;
				for (int i = rowIndex; i <= rowCount; i++)
				{
					for (int j = 0; j <= columnCount; j++)
					{
						if ((i > rowIndex || j > columnIndex) && this.dgvSection.Rows[i].Cells[j].Value.ToString().IndexOf(this.txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							this.dgvSection.ClearSelection();
							this.dgvSection.Rows[i].Cells[j].Selected = true;
							this.dgvSection.FirstDisplayedScrollingRowIndex = i;
							this.dgvSection.Select();
							return;
						}
					}
				}
				for (int k = 0; k < rowIndex; k++)
				{
					for (int l = 0; l <= columnCount; l++)
					{
						if (this.dgvSection.Rows[k].Cells[l].Value.ToString().IndexOf(this.txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
						{
							this.dgvSection.ClearSelection();
							this.dgvSection.Rows[k].Cells[l].Selected = true;
							this.dgvSection.FirstDisplayedScrollingRowIndex = k;
							this.dgvSection.Select();
							return;
						}
					}
				}
				MessageBox.Show("Search text was not found.");
				return;
			}
			int num = this.dgvTranslate.RowCount;
			int columnCount1 = this.dgvTranslate.ColumnCount;
			int rowIndex1 = this.dgvTranslate.SelectedCells[0].RowIndex;
			int columnIndex1 = this.dgvTranslate.SelectedCells[0].ColumnIndex;
			for (int m = rowIndex1; m < num; m++)
			{
				for (int n = 0; n < columnCount1; n++)
				{
					if ((m > rowIndex1 || n > columnIndex1) && this.dgvTranslate.Rows[m].Cells[n].Value.ToString().IndexOf(this.txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						this.dgvTranslate.ClearSelection();
						this.dgvTranslate.Rows[m].Cells[n].Selected = true;
						this.dgvTranslate.FirstDisplayedScrollingRowIndex = m;
						this.dgvTranslate.Select();
						return;
					}
				}
			}
			for (int o = 0; o < rowIndex1; o++)
			{
				for (int p = 0; p < columnCount1; p++)
				{
					if (this.dgvTranslate.Rows[o].Cells[p].Value.ToString().IndexOf(this.txtSearch.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						this.dgvTranslate.ClearSelection();
						this.dgvTranslate.Rows[o].Cells[p].Selected = true;
						this.dgvTranslate.FirstDisplayedScrollingRowIndex = o;
						this.dgvTranslate.Select();
						return;
					}
				}
			}
			MessageBox.Show("Search text was not found.");
		}

		private void cboFile_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._blnLoading = true;
			if (this.cboFile.SelectedIndex == 0)
			{
				this.LoadStrings();
			}
			else if (this.cboFile.SelectedIndex > 0)
			{
				this.LoadSections();
			}
			this._blnLoading = false;
		}

		private void cboSection_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.LoadSection();
		}

		private void chkOnlyTranslation_CheckedChanged(object sender, EventArgs e)
		{
			if (this.cboFile.Text == "Strings")
			{
				this.LoadStrings();
				return;
			}
			this.LoadSection();
		}

		private void dgvSection_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex == 4 && e.RowIndex != -1)
			{
				this.dgvSection.EndEdit();
			}
		}

		private void dgvSection_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			XmlAttribute itemOf;
			XmlAttribute str;
			if (this._blnLoading || e.RowIndex < 0)
			{
				return;
			}
			bool flag = true;
			DataGridViewRow item = this.dgvSection.Rows[e.RowIndex];
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
			XmlDocument xmlDocument = this._objDataDoc;
			string[] text = new string[] { "/chummer/chummer[@file=\"", this.cboFile.Text, "\"]/", this.cboSection.Text, "//name[text()=\"", str2, "\"]/.." };
			XmlNode xmlNodes = xmlDocument.SelectSingleNode(string.Concat(text));
			if (xmlNodes == null)
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file=\"", this.cboFile.Text, "\"]/", this.cboSection.Text, "/*[text()=\"", str2, "\"]" };
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
						str = this._objDataDoc.CreateAttribute("translated");
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
						itemOf = this._objDataDoc.CreateAttribute("translated");
						itemOf.InnerText = flag.ToString();
						xmlNodes.Attributes.Append(itemOf);
					}
					else
					{
						itemOf.InnerText = flag.ToString();
					}
				}
			}
			Save(this._objDataDoc);
		}

		private void dgvSection_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3)
			{
				this.btnSearch.PerformClick();
				return;
			}
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				this.txtSearch.Focus();
			}
		}

		private void dgvSection_Sorted(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in (IEnumerable)this.dgvSection.Rows)
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
			if (this._blnLoading || e.RowIndex < 0)
			{
				return;
			}
			bool flag = true;
			DataGridViewRow item = this.dgvTranslate.Rows[e.RowIndex];
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
			XmlNode xmlNodes = this._objDoc.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", str1, "\"]"));
			xmlNodes["text"].InnerText = str;
			Save(this._objDoc, false);
		}

		private void dgvTranslate_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3)
			{
				this.btnSearch.PerformClick();
				return;
			}
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				this.txtSearch.Focus();
			}
		}

		private void dgvTranslate_Sorted(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in (IEnumerable)this.dgvTranslate.Rows)
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
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void frmTranslate_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3)
			{
				this.btnSearch.PerformClick();
				return;
			}
			if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
			{
				this.txtSearch.Focus();
			}
		}

		private void frmTranslate_Load(object sender, EventArgs e)
		{
			List<string> strs = new List<string>();
			this.Text = string.Concat("Translating ", this._strLang);
			this.SetPath();
			this.LoadLanaguageFiles();
			this.cboFile.Items.Add("Strings");
			foreach (XmlNode xmlNodes in this._objDataDoc.SelectNodes("/chummer/chummer"))
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
				this.cboFile.Items.Add(str);
			}
			this.cboFile.SelectedIndex = 0;
		}

		private void InitializeComponent()
		{
			DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			this.cboFile = new ComboBox();
			this.dgvTranslate = new DataGridView();
			this.key = new DataGridViewTextBoxColumn();
			this.english = new DataGridViewTextBoxColumn();
			this.text = new DataGridViewTextBoxColumn();
			this.Translated = new DataGridViewCheckBoxColumn();
			this.cboSection = new ComboBox();
			this.chkOnlyTranslation = new CheckBox();
			this.dgvSection = new DataGridView();
			this.sectionEnglish = new DataGridViewTextBoxColumn();
			this.sectionText = new DataGridViewTextBoxColumn();
			this.sectionBook = new DataGridViewTextBoxColumn();
			this.sectionPage = new DataGridViewTextBoxColumn();
			this.sectionTranslated = new DataGridViewCheckBoxColumn();
			this.btnSearch = new Button();
			this.txtSearch = new TextBox();
			((ISupportInitialize)this.dgvTranslate).BeginInit();
			((ISupportInitialize)this.dgvSection).BeginInit();
			base.SuspendLayout();
			this.cboFile.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cboFile.FormattingEnabled = true;
			this.cboFile.Location = new Point(12, 12);
			this.cboFile.Name = "cboFile";
			this.cboFile.Size = new System.Drawing.Size(218, 21);
			this.cboFile.TabIndex = 0;
			this.cboFile.SelectedIndexChanged += new EventHandler(this.cboFile_SelectedIndexChanged);
			this.dgvTranslate.AllowUserToAddRows = false;
			this.dgvTranslate.AllowUserToDeleteRows = false;
			this.dgvTranslate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.dgvTranslate.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewColumnCollection columns = this.dgvTranslate.Columns;
			DataGridViewColumn[] translated = new DataGridViewColumn[] { this.key, this.english, this.text, this.Translated };
			columns.AddRange(translated);
			this.dgvTranslate.Location = new Point(12, 39);
			this.dgvTranslate.MultiSelect = false;
			this.dgvTranslate.Name = "dgvTranslate";
			this.dgvTranslate.RowHeadersVisible = false;
			this.dgvTranslate.Size = new System.Drawing.Size(1197, 476);
			this.dgvTranslate.TabIndex = 7;
			this.dgvTranslate.CellValueChanged += new DataGridViewCellEventHandler(this.dgvTranslate_CellValueChanged);
			this.dgvTranslate.Sorted += new EventHandler(this.dgvTranslate_Sorted);
			this.dgvTranslate.KeyDown += new KeyEventHandler(this.dgvTranslate_KeyDown);
			this.key.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.key.DataPropertyName = "key";
			dataGridViewCellStyle.WrapMode = DataGridViewTriState.True;
			this.key.DefaultCellStyle = dataGridViewCellStyle;
			this.key.HeaderText = "Key";
			this.key.Name = "key";
			this.key.ReadOnly = true;
			this.key.Width = 300;
			this.english.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.english.DataPropertyName = "english";
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			this.english.DefaultCellStyle = dataGridViewCellStyle1;
			this.english.HeaderText = "English";
			this.english.Name = "english";
			this.english.ReadOnly = true;
			this.english.Width = 400;
			this.text.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.text.DataPropertyName = "text";
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
			this.text.DefaultCellStyle = dataGridViewCellStyle2;
			this.text.HeaderText = "Text";
			this.text.Name = "text";
			this.text.Width = 400;
			this.Translated.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.Translated.DataPropertyName = "translated";
			this.Translated.HeaderText = "Translated";
			this.Translated.Name = "Translated";
			this.Translated.SortMode = DataGridViewColumnSortMode.Automatic;
			this.Translated.Width = 70;
			this.cboSection.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cboSection.FormattingEnabled = true;
			this.cboSection.Location = new Point(236, 12);
			this.cboSection.Name = "cboSection";
			this.cboSection.Size = new System.Drawing.Size(218, 21);
			this.cboSection.TabIndex = 1;
			this.cboSection.Visible = false;
			this.cboSection.SelectedIndexChanged += new EventHandler(this.cboSection_SelectedIndexChanged);
			this.chkOnlyTranslation.AutoSize = true;
			this.chkOnlyTranslation.Location = new Point(460, 14);
			this.chkOnlyTranslation.Name = "chkOnlyTranslation";
			this.chkOnlyTranslation.Size = new System.Drawing.Size(199, 17);
			this.chkOnlyTranslation.TabIndex = 2;
			this.chkOnlyTranslation.Text = "Only Show Text Needing Translation";
			this.chkOnlyTranslation.UseVisualStyleBackColor = true;
			this.chkOnlyTranslation.CheckedChanged += new EventHandler(this.chkOnlyTranslation_CheckedChanged);
			this.dgvSection.AllowUserToAddRows = false;
			this.dgvSection.AllowUserToDeleteRows = false;
			this.dgvSection.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.dgvSection.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewColumnCollection dataGridViewColumnCollections = this.dgvSection.Columns;
			DataGridViewColumn[] dataGridViewColumnArray = new DataGridViewColumn[] { this.sectionEnglish, this.sectionText, this.sectionBook, this.sectionPage, this.sectionTranslated };
			dataGridViewColumnCollections.AddRange(dataGridViewColumnArray);
			this.dgvSection.Location = new Point(12, 39);
			this.dgvSection.MultiSelect = false;
			this.dgvSection.Name = "dgvSection";
			this.dgvSection.RowHeadersVisible = false;
			this.dgvSection.Size = new System.Drawing.Size(1197, 476);
			this.dgvSection.TabIndex = 5;
			this.dgvSection.Visible = false;
			this.dgvSection.CellValueChanged += new DataGridViewCellEventHandler(this.dgvSection_CellValueChanged);
			this.dgvSection.Sorted += new EventHandler(this.dgvSection_Sorted);
			this.dgvSection.KeyDown += new KeyEventHandler(this.dgvSection_KeyDown);
			this.dgvSection.CellMouseUp += new DataGridViewCellMouseEventHandler(this.dgvSection_CellMouseUp);
			this.sectionEnglish.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.sectionEnglish.DataPropertyName = "english";
			dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
			this.sectionEnglish.DefaultCellStyle = dataGridViewCellStyle3;
			this.sectionEnglish.HeaderText = "English";
			this.sectionEnglish.Name = "sectionEnglish";
			this.sectionEnglish.ReadOnly = true;
			this.sectionEnglish.Width = 480;
			this.sectionText.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.sectionText.DataPropertyName = "text";
			dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
			this.sectionText.DefaultCellStyle = dataGridViewCellStyle4;
			this.sectionText.HeaderText = "Text";
			this.sectionText.Name = "sectionText";
			this.sectionText.Width = 480;
			this.sectionBook.DataPropertyName = "book";
			this.sectionBook.HeaderText = "Book";
			this.sectionBook.Name = "sectionBook";
			this.sectionBook.ReadOnly = true;
			this.sectionBook.Width = 70;
			this.sectionPage.DataPropertyName = "page";
			this.sectionPage.HeaderText = "Page";
			this.sectionPage.Name = "sectionPage";
			this.sectionPage.Width = 70;
			this.sectionTranslated.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			this.sectionTranslated.DataPropertyName = "translated";
			this.sectionTranslated.HeaderText = "Translated";
			this.sectionTranslated.Name = "sectionTranslated";
			this.sectionTranslated.SortMode = DataGridViewColumnSortMode.Automatic;
			this.sectionTranslated.Width = 70;
			this.btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.btnSearch.Location = new Point(1134, 10);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(75, 23);
			this.btnSearch.TabIndex = 4;
			this.btnSearch.Text = "Search";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
			this.txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.txtSearch.Location = new Point(962, 12);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(166, 20);
			this.txtSearch.TabIndex = 3;
			this.txtSearch.KeyPress += new KeyPressEventHandler(this.txtSearch_KeyPressed);
			this.txtSearch.GotFocus += new EventHandler(this.txtSearch_GotFocus);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(1221, 527);
			base.Controls.Add(this.txtSearch);
			base.Controls.Add(this.btnSearch);
			base.Controls.Add(this.chkOnlyTranslation);
			base.Controls.Add(this.cboSection);
			base.Controls.Add(this.cboFile);
			base.Controls.Add(this.dgvSection);
			base.Controls.Add(this.dgvTranslate);
			base.KeyPreview = true;
			base.Name = "frmTranslate";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Translating";
			base.Load += new EventHandler(this.frmTranslate_Load);
			base.KeyDown += new KeyEventHandler(this.frmTranslate_KeyDown);
			((ISupportInitialize)this.dgvTranslate).EndInit();
			((ISupportInitialize)this.dgvSection).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void LoadLanaguageFiles()
		{
			this._strCode = this._strLang.Substring(this._strLang.IndexOf("(") + 1, 2);
			this._objDoc.Load(string.Concat(this._strPath, "lang\\", this._strCode, ".xml"));
			this._objDataDoc.Load(string.Concat(this._strPath, "lang\\", this._strCode, "_data.xml"));
		}

		private void LoadSection()
		{
			if (this.cboSection.SelectedIndex < 0)
			{
				return;
			}
			this.dgvTranslate.Visible = false;
			this.dgvSection.Visible = true;
			DataTable dataTable = new DataTable("strings");
			dataTable.Columns.Add("english");
			dataTable.Columns.Add("text");
			dataTable.Columns.Add("book");
			dataTable.Columns.Add("page");
			dataTable.Columns.Add("translated");
			string text = this.cboFile.Text;
			XmlNodeList childNodes = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]/", this.cboSection.Text)).ChildNodes;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\", text));
			foreach (XmlNode childNode in childNodes)
			{
				string innerText = "";
				string str = "";
				string innerText1 = "";
				string str1 = "";
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
						string[] strArrays = new string[] { "/chummer/", this.cboSection.Text, "/*[name=\"", innerText, "\"]" };
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
				if ((!this.chkOnlyTranslation.Checked || !(innerText == innerText1)) && this.chkOnlyTranslation.Checked)
				{
					continue;
				}
				DataRowCollection rows = dataTable.Rows;
				object[] objArray = new object[] { innerText, innerText1, str1, str, flag };
				rows.Add(objArray);
			}
			DataSet dataSet = new DataSet("strings");
			dataSet.Tables.Add(dataTable);
			this.dgvSection.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvSection.DataSource = dataSet;
			this.dgvSection.DataMember = "strings";
			foreach (DataGridViewRow row in (IEnumerable)this.dgvSection.Rows)
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
			this.cboSection.Items.Clear();
			string text = this.cboFile.Text;
			List<string> strs = new List<string>();
			XmlNode xmlNodes = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file=\"", text, "\"]"));
			foreach (XmlNode childNode in xmlNodes.ChildNodes)
			{
				strs.Add(childNode.Name);
			}
			strs.Sort();
			foreach (string str in strs)
			{
				this.cboSection.Items.Add(str);
			}
			this.cboSection.Visible = true;
		}

		private void LoadStrings()
		{
			this.dgvTranslate.Visible = true;
			this.dgvSection.Visible = false;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "lang\\en-US.xml"));
			DataTable dataTable = new DataTable("strings");
			dataTable.Columns.Add("key");
			dataTable.Columns.Add("english");
			dataTable.Columns.Add("text");
			dataTable.Columns.Add("translated");
			foreach (XmlNode xmlNodes in this._objDoc.SelectNodes("/chummer/strings/string"))
			{
				try
				{
					string innerText = xmlNodes["key"].InnerText;
					string str = xmlNodes["text"].InnerText;
					XmlNode xmlNodes1 = xmlDocument.SelectSingleNode(string.Concat("/chummer/strings/string[key = \"", innerText, "\"]"));
					string innerText1 = "";
					if (xmlNodes1 != null)
					{
						innerText1 = xmlNodes1["text"].InnerText;
					}
					bool flag = false;
					if (xmlNodes.Attributes["translated"] != null)
					{
						flag = Convert.ToBoolean(xmlNodes.Attributes["translated"].InnerText);
					}
					if (this.chkOnlyTranslation.Checked && str == innerText1 || !this.chkOnlyTranslation.Checked)
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
			this.dgvTranslate.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvTranslate.DataSource = dataSet;
			this.dgvTranslate.DataMember = "strings";
			foreach (DataGridViewRow row in (IEnumerable)this.dgvTranslate.Rows)
			{
				if (!(row.Cells["english"].Value.ToString() == row.Cells["text"].Value.ToString()) || Convert.ToBoolean(row.Cells["translated"].Value))
				{
					continue;
				}
				row.DefaultCellStyle.BackColor = Color.Wheat;
			}
			this.cboSection.Visible = false;
		}

		private void SetPath()
		{
			this._strPath = Application.StartupPath;
			if (!this._strPath.EndsWith("\\"))
			{
				frmTranslate _frmTranslate = this;
				_frmTranslate._strPath = string.Concat(_frmTranslate._strPath, "\\");
			}
		}

		private void txtSearch_GotFocus(object sender, EventArgs e)
		{
			this.txtSearch.SelectAll();
		}

		private void txtSearch_KeyPressed(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' && !this.txtSearch.AcceptsReturn)
			{
				this.btnSearch.PerformClick();
			}
		}

		private void Save(XmlDocument objXmlDocument, bool blnData = true)
		{
			string strPath = "";
			strPath = string.Concat(_strPath, "lang\\", this._strCode, blnData ? "_data.xml" : ".xml");
			using (XmlWriter xwWriter = XmlWriter.Create(strPath, xwsSettings))
				objXmlDocument.Save(xwWriter);
		}
	}
}