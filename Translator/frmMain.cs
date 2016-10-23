using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Translator
{
	public class frmMain : Form
	{
		private string _strPath = "";

		private bool _blnDelete = true;

		private XmlDocument _objDataDoc;

		private XmlDocument _objDoc;

		private IContainer components;

		private GroupBox gbxCreate;

		private Button cmdCreate;

		private TextBox txtLanguageCode;

		private Label lblLanguageCode;

		private GroupBox groupBox1;

		private ComboBox cboLanguages;

		private Button cmdEdit;

		private TextBox txtLanguageName;

		private Label label1;

		public frmMain()
		{
			this.InitializeComponent();
		}

		private void cmdCreate_Click(object sender, EventArgs e)
		{
			if (this.txtLanguageCode.Text.Length != 2)
			{
				MessageBox.Show("You must provide a two character language abbreviation.");
				return;
			}
			if (this.txtLanguageName.Text == "")
			{
				MessageBox.Show("You must provide a language name.");
				return;
			}
			string lower = this.txtLanguageCode.Text.ToLower();
			string str = string.Concat(this.ToTitle(this.txtLanguageName.Text), " (", lower.ToUpper(), ")");
			this._objDataDoc = new XmlDocument();
			XmlDeclaration xmlDeclaration = this._objDataDoc.CreateXmlDeclaration("1.0", "utf-8", "");
			this._objDataDoc.AppendChild(xmlDeclaration);
			XmlNode xmlNodes = this._objDataDoc.CreateElement("chummer");
			XmlNode xmlNodes1 = this._objDataDoc.CreateElement("version");
			xmlNodes1.InnerText = "-500";
			xmlNodes.AppendChild(xmlNodes1);
			this._objDataDoc.AppendChild(xmlNodes);
			this.ProcessArmor();
			this.ProcessBioware();
			this.ProcessBooks();
			this.ProcessComplexForms();
			this.ProcessCritterPowers();
			this.ProcessCritters();
			this.ProcessCyberware();
			this.ProcessEchoes();
			this.ProcessGear();
			this.ProcessImprovements();
			this.ProcessLicenses();
			this.ProcessLifestyles();
			this.ProcessMartialArts();
			this.ProcessMentors();
			this.ProcessMetamagic();
			this.ProcessMetatypes();
			this.ProcessPowers();
			this.ProcessPriorities();
			this.ProcessPrograms();
			this.ProcessQualities();
			this.ProcessSkills();
			this.ProcessSpells();
			this.ProcessStreams();
			this.ProcessTraditions();
			this.ProcessVehicles();
			this.ProcessWeapons();
			string str1 = string.Concat(this._strPath, "lang\\", lower, "_data.xml");
			this._objDataDoc.Save(str1);
			this._objDoc = new XmlDocument();
			XmlWriterSettings xwsSettings = new XmlWriterSettings {IndentChars = ("\t")};
			xmlDeclaration = this._objDoc.CreateXmlDeclaration("1.0", "utf-8", "");
			this._objDoc.AppendChild(xmlDeclaration);
			xmlNodes = this._objDoc.CreateElement("chummer");
			xmlNodes1 = this._objDoc.CreateElement("version");
			xmlNodes1.InnerText = "-500";
			XmlNode xmlNodes2 = this._objDoc.CreateElement("name");
			xmlNodes2.InnerText = str;
			xmlNodes.AppendChild(xmlNodes1);
			xmlNodes.AppendChild(xmlNodes2);
			this._objDoc.AppendChild(xmlNodes);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "lang\\en-US.xml"));
			XmlNode xmlNodes3 = xmlDocument.SelectSingleNode("/chummer/strings");
			XmlNode xmlNodes4 = this._objDoc.CreateElement("strings");
			xmlNodes4 = this._objDoc.ImportNode(xmlNodes3, true);
			xmlNodes.AppendChild(xmlNodes4);
			str1 = string.Concat(this._strPath, "lang\\", lower, ".xml");
			this._objDoc.Save(str1);

			this.LoadLanguageList();
			frmTranslate _frmTranslate = new frmTranslate()
			{
				Language = str
			};
			_frmTranslate.ShowDialog(this);
		}

		private void cmdEdit_Click(object sender, EventArgs e)
		{
			if (this.cboLanguages.SelectedIndex == -1)
			{
				return;
			}
			frmTranslate _frmTranslate = new frmTranslate()
			{
				Language = this.cboLanguages.Text
			};
			_frmTranslate.ShowDialog(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			this.SetPath();
			this.LoadLanguageList();
		}

		private void InitializeComponent()
		{
			this.gbxCreate = new GroupBox();
			this.cmdCreate = new Button();
			this.txtLanguageCode = new TextBox();
			this.lblLanguageCode = new Label();
			this.groupBox1 = new GroupBox();
			this.cmdEdit = new Button();
			this.cboLanguages = new ComboBox();
			this.txtLanguageName = new TextBox();
			this.label1 = new Label();
			this.gbxCreate.SuspendLayout();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this.gbxCreate.Controls.Add(this.txtLanguageName);
			this.gbxCreate.Controls.Add(this.label1);
			this.gbxCreate.Controls.Add(this.cmdCreate);
			this.gbxCreate.Controls.Add(this.txtLanguageCode);
			this.gbxCreate.Controls.Add(this.lblLanguageCode);
			this.gbxCreate.Location = new Point(12, 12);
			this.gbxCreate.Name = "gbxCreate";
			this.gbxCreate.Size = new System.Drawing.Size(201, 107);
			this.gbxCreate.TabIndex = 0;
			this.gbxCreate.TabStop = false;
			this.gbxCreate.Text = "Create a Language File";
			this.cmdCreate.Location = new Point(103, 71);
			this.cmdCreate.Name = "cmdCreate";
			this.cmdCreate.Size = new System.Drawing.Size(75, 23);
			this.cmdCreate.TabIndex = 3;
			this.cmdCreate.Text = "Create";
			this.cmdCreate.UseVisualStyleBackColor = true;
			this.cmdCreate.Click += new EventHandler(this.cmdCreate_Click);
			this.txtLanguageCode.Location = new Point(133, 19);
			this.txtLanguageCode.MaxLength = 2;
			this.txtLanguageCode.Name = "txtLanguageCode";
			this.txtLanguageCode.Size = new System.Drawing.Size(45, 20);
			this.txtLanguageCode.TabIndex = 1;
			this.lblLanguageCode.AutoSize = true;
			this.lblLanguageCode.Location = new Point(10, 22);
			this.lblLanguageCode.Name = "lblLanguageCode";
			this.lblLanguageCode.Size = new System.Drawing.Size(83, 13);
			this.lblLanguageCode.TabIndex = 143;
			this.lblLanguageCode.Tag = "";
			this.lblLanguageCode.Text = "Language Code";
			this.groupBox1.Controls.Add(this.cmdEdit);
			this.groupBox1.Controls.Add(this.cboLanguages);
			this.groupBox1.Location = new Point(233, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(201, 107);
			this.groupBox1.TabIndex = 146;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit Language File";
			this.cmdEdit.Location = new Point(63, 58);
			this.cmdEdit.Name = "cmdEdit";
			this.cmdEdit.Size = new System.Drawing.Size(75, 23);
			this.cmdEdit.TabIndex = 5;
			this.cmdEdit.Text = "Edit";
			this.cmdEdit.UseVisualStyleBackColor = true;
			this.cmdEdit.Click += new EventHandler(this.cmdEdit_Click);
			this.cboLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cboLanguages.FormattingEnabled = true;
			this.cboLanguages.Location = new Point(40, 19);
			this.cboLanguages.Name = "cboLanguages";
			this.cboLanguages.Size = new System.Drawing.Size(122, 21);
			this.cboLanguages.TabIndex = 4;
			this.txtLanguageName.Location = new Point(102, 45);
			this.txtLanguageName.MaxLength = 100;
			this.txtLanguageName.Name = "txtLanguageName";
			this.txtLanguageName.Size = new System.Drawing.Size(76, 20);
			this.txtLanguageName.TabIndex = 2;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(10, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(86, 13);
			this.label1.TabIndex = 146;
			this.label1.Tag = "";
			this.label1.Text = "Language Name";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(445, 127);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.gbxCreate);
			base.Name = "frmMain";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Chummer Translator";
			base.Load += new EventHandler(this.frmMain_Load);
			this.gbxCreate.ResumeLayout(false);
			this.gbxCreate.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		private void LoadLanguageList()
		{
			this.cboLanguages.Items.Clear();
			foreach (string str in Directory.EnumerateFiles(string.Concat(this._strPath, "lang"), "*.xml"))
			{
				if ((new FileInfo(str)).Name.Length != 6)
				{
					continue;
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(str);
				string innerText = xmlDocument.SelectSingleNode("/chummer/name").InnerText;
				this.cboLanguages.Items.Add(innerText);
			}
		}

		private void ProcessArmor()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\armor.xml"));
			string str = "armor.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/armors/armor"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/armors/armor[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/armors"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("armors");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("armor");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/armors/armor"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/armors/armor[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/armors")).RemoveChild(xmlNodes10);
				}
			}
			foreach (XmlNode xmlNodes11 in xmlDocument.SelectNodes("/chummer/mods/mod"))
			{
				string str2 = "";
				string str3 = xmlNodes11["name"].InnerText;
				str2 = xmlNodes11["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/mods/mod[name=\"", str3, "\"]" };
				XmlNode xmlNodes12 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes12 != null)
				{
					if (xmlNodes12["id"] != null)
					{
						continue;
					}
					xmlNodes12.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes12["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods"));
					if (xmlNodes13 == null)
					{
						xmlNodes13 = this._objDataDoc.CreateElement("mods");
						XmlNode xmlNodes14 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes14 == null)
						{
							xmlNodes14 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes14.Attributes.Append(xmlAttribute4);
							xmlNodes15.AppendChild(xmlNodes14);
						}
						xmlNodes14.AppendChild(xmlNodes13);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("mod");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText4["page"].InnerText = xmlNodes11["page"].InnerText;
					xmlNodes13.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods/mod"));
			foreach (XmlNode xmlNodes16 in xmlNodeLists2)
			{
				xmlNodes16.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/mods/mod[name = \"", xmlNodes16["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes16.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods")).RemoveChild(xmlNodes16);
				}
			}
		}

		private void ProcessBioware()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\bioware.xml"));
			string str = "bioware.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/biowares/bioware"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/biowares/bioware[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/biowares"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("biowares");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("bioware");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/biowares/bioware"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/biowares/bioware[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/biowares")).RemoveChild(xmlNodes10);
				}
			}
			foreach (XmlNode xmlNodes11 in xmlDocument.SelectNodes("/chummer/grades/grade"))
			{
				string str2 = "";
				string str3 = xmlNodes11["name"].InnerText;
				if (xmlNodes11["id"] != null)
				{
					str2 = xmlNodes11["id"].InnerText;
				}
				else
				{
					str2 = Guid.NewGuid().ToString();
					xmlNodes11.PrependChild(xmlDocument.CreateElement("id"));
					xmlNodes11["id"].InnerText = str2;
				}
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/grades/grade[name=\"", str3, "\"]" };
				XmlNode xmlNodes12 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes12 != null)
				{
					if (xmlNodes12["id"] != null)
					{
						continue;
					}
					xmlNodes12.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes12["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades"));
					if (xmlNodes13 == null)
					{
						xmlNodes13 = this._objDataDoc.CreateElement("grades");
						XmlNode xmlNodes14 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes14 == null)
						{
							xmlNodes14 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes14.Attributes.Append(xmlAttribute4);
							xmlNodes15.AppendChild(xmlNodes14);
						}
						xmlNodes14.AppendChild(xmlNodes13);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("grade");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText4["page"].InnerText = xmlNodes11["page"].InnerText;
					xmlNodes13.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades/grade"));
			foreach (XmlNode xmlNodes16 in xmlNodeLists2)
			{
				xmlNodes16.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/grades/grade[name = \"", xmlNodes16["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes16.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades")).RemoveChild(xmlNodes16);
				}
			}
		}

		private void ProcessBooks()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\books.xml"));
			string str = "books.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/books/book"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/books/book[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/books"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("books");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("book");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/books/book"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/books/book[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/books")).RemoveChild(xmlNodes5);
				}
			}
		}

		private void ProcessComplexForms()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\complexforms.xml"));
			string str = "complexforms.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/complexforms/complexform"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/complexforms/complexform[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/complexforms"));
					if (xmlNodes2 == null)
					{
						XmlNode xmlNodes3 = this._objDataDoc.CreateElement("chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes3.Attributes.Append(xmlAttribute);
						xmlNodes2 = this._objDataDoc.CreateElement("complexforms");
						xmlNodes3.AppendChild(xmlNodes2);
						this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes3);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("complexform");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/complexforms/complexform"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				xmlNodes4.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/complexforms/complexform[name = \"", xmlNodes4["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes4.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/complexforms")).RemoveChild(xmlNodes4);
				}
			}
		}

		private void ProcessCritterPowers()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\critterpowers.xml"));
			string str = "critterpowers.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/powers/power"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/powers/power[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("powers");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("power");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers/power"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/powers/power[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers")).RemoveChild(xmlNodes10);
				}
			}
		}

		private void ProcessCritters()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\critters.xml"));
			string str = "critters.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/metatypes/metatype"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("metatypes");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("metatype");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/metatypes/metatype[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes")).RemoveChild(xmlNodes10);
				}
			}
		}

		private void ProcessCyberware()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\cyberware.xml"));
			string str = "cyberware.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/cyberwares/cyberware"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/cyberwares/cyberware[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/cyberwares"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("cyberwares");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("cyberware");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/cyberwares/cyberware"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/cyberwares/cyberware[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/cyberwares")).RemoveChild(xmlNodes10);
				}
			}
			foreach (XmlNode xmlNodes11 in xmlDocument.SelectNodes("/chummer/grades/grade"))
			{
				string str2 = "";
				string str3 = xmlNodes11["name"].InnerText;
				str2 = xmlNodes11["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/grades/grade[name=\"", str3, "\"]" };
				XmlNode xmlNodes12 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes12 != null)
				{
					if (xmlNodes12["id"] != null)
					{
						continue;
					}
					xmlNodes12.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes12["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades"));
					if (xmlNodes13 == null)
					{
						xmlNodes13 = this._objDataDoc.CreateElement("grades");
						XmlNode xmlNodes14 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes14 == null)
						{
							xmlNodes14 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes14.Attributes.Append(xmlAttribute4);
							xmlNodes15.AppendChild(xmlNodes14);
						}
						xmlNodes14.AppendChild(xmlNodes13);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("grade");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes11["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText4["page"].InnerText = xmlNodes11["page"].InnerText;
					xmlNodes13.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades/grade"));
			foreach (XmlNode xmlNodes16 in xmlNodeLists2)
			{
				xmlNodes16.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/grades/grade[name = \"", xmlNodes16["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes16.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/grades")).RemoveChild(xmlNodes16);
				}
			}
			try
			{
				XmlNode xmlNodes17 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes18 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/suites"));
				xmlNodes17.RemoveChild(xmlNodes18);
			}
			catch
			{
			}
		}

		private void ProcessEchoes()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\echoes.xml"));
			string str = "echoes.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/echoes/echo"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/echoes/echo[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/echoes"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("echoes");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("echo");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText2["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/echoes/echo"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/echoes/echo[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/echoes")).RemoveChild(xmlNodes5);
				}
			}
		}

		private void ProcessGear()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\gear.xml"));
			string str = "gear.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/gears/gear"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/gears/gear[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gears"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("gears");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("gear");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gears/gear"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/gears/gear[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gears")).RemoveChild(xmlNodes10);
				}
			}
		}

		private void ProcessImprovements()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\improvements.xml"));
			string str = "improvements.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/improvements/improvement"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/improvements/improvement[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/improvements"));
					if (xmlNodes2 == null)
					{
						XmlNode xmlNodes3 = this._objDataDoc.CreateElement("chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes3.Attributes.Append(xmlAttribute);
						xmlNodes2 = this._objDataDoc.CreateElement("improvements");
						xmlNodes3.AppendChild(xmlNodes2);
						this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes3);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("improvement");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/improvements/improvement"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				xmlNodes4.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/improvements/improvement[name = \"", xmlNodes4["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes4.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/improvements")).RemoveChild(xmlNodes4);
				}
			}
		}

		private void ProcessLicenses()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\licenses.xml"));
			string str = "licenses.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/licenses/license"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/licenses/license[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/licenses"));
				if (xmlNodes1 == null)
				{
					XmlNode xmlNodes2 = this._objDataDoc.CreateElement("chummer");
					XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
					xmlAttribute.Value = str;
					xmlNodes2.Attributes.Append(xmlAttribute);
					xmlNodes1 = this._objDataDoc.CreateElement("licenses");
					xmlNodes2.AppendChild(xmlNodes1);
					this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes2);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("license");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/licenses/license"));
			foreach (XmlNode xmlNodes3 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/licenses/license[text() = \"", xmlNodes3.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/licenses")).RemoveChild(xmlNodes3);
			}
		}

		private void ProcessLifestyles()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\lifestyles.xml"));
			string str = "lifestyles.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/lifestyles/lifestyle[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/lifestyles"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("lifestyles");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("lifestyle");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText2["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/lifestyles/lifestyle"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/lifestyles/lifestyle[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/lifestyles")).RemoveChild(xmlNodes5);
				}
			}
			foreach (XmlNode xmlNodes6 in xmlDocument.SelectNodes("/chummer/qualities/quality"))
			{
				string str1 = "";
				string str2 = xmlNodes6["name"].InnerText;
				str1 = xmlNodes6["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/qualities/quality[name=\"", str2, "\"]" };
				XmlNode xmlNodes7 = xmlDocument2.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes7 != null)
				{
					if (xmlNodes7["id"] != null)
					{
						continue;
					}
					xmlNodes7.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes7["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities"));
					if (xmlNodes8 == null)
					{
						XmlNode xmlNodes9 = this._objDataDoc.CreateElement("chummer");
						XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
						xmlAttribute2.Value = str;
						xmlNodes9.Attributes.Append(xmlAttribute2);
						xmlNodes8 = this._objDataDoc.CreateElement("qualities");
						xmlNodes9.AppendChild(xmlNodes8);
						this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes9);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("quality");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes6["page"].InnerText;
					xmlNodes8.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities/quality"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/qualities/quality[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities")).RemoveChild(xmlNodes10);
				}
			}
			try
			{
				XmlNode xmlNodes11 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes12 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/comforts"));
				xmlNodes11.RemoveChild(xmlNodes12);
			}
			catch
			{
			}
			try
			{
				XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes14 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/entertainments"));
				xmlNodes13.RemoveChild(xmlNodes14);
			}
			catch
			{
			}
			try
			{
				XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes16 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/necessities"));
				xmlNodes15.RemoveChild(xmlNodes16);
			}
			catch
			{
			}
			try
			{
				XmlNode xmlNodes17 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes18 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/neighborhoods"));
				xmlNodes17.RemoveChild(xmlNodes18);
			}
			catch
			{
			}
			try
			{
				XmlNode xmlNodes19 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes20 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/securities"));
				xmlNodes19.RemoveChild(xmlNodes20);
			}
			catch
			{
			}
		}

		private void ProcessMartialArts()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\martialarts.xml"));
			string str = "martialarts.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/martialarts/martialart"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/martialarts/martialart[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 == null)
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/martialarts"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("martialarts");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("martialart");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText2["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
				else if (xmlNodes1["id"] == null)
				{
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/martialarts/martialart[name=\"", innerText1, "\"]/advantages" };
				XmlNode xmlNodes5 = xmlDocument2.SelectSingleNode(string.Concat(strArrays1));
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays2 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/martialarts/martialart[name=\"", innerText1, "\"]" };
				xmlNodes1 = xmlDocument3.SelectSingleNode(string.Concat(strArrays2));
				try
				{
					xmlNodes1.RemoveChild(xmlNodes5);
				}
				catch
				{
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/martialarts/martialart"));
			foreach (XmlNode xmlNodes6 in xmlNodeLists)
			{
				xmlNodes6.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/martialarts/martialart[name = \"", xmlNodes6["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes6.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/martialarts")).RemoveChild(xmlNodes6);
				}
			}
			foreach (XmlNode xmlNodes7 in xmlDocument.SelectNodes("/chummer/techniques/technique"))
			{
				string str1 = "";
				string str2 = xmlNodes7["name"].InnerText;
				str1 = xmlNodes7["id"].InnerText;
				XmlDocument xmlDocument4 = this._objDataDoc;
				string[] strArrays3 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/techniques/technique[name=\"", str2, "\"]" };
				XmlNode xmlNodes8 = xmlDocument4.SelectSingleNode(string.Concat(strArrays3));
				if (xmlNodes8 != null)
				{
					if (xmlNodes8["id"] != null)
					{
						continue;
					}
					xmlNodes8.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes8["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/techniques"));
					if (xmlNodes9 == null)
					{
						XmlNode xmlNodes10 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes10 == null)
						{
							xmlNodes10 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes10.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes10);
						}
						xmlNodes9 = this._objDataDoc.CreateElement("techniques");
						xmlNodes10.AppendChild(xmlNodes9);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("technique");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes7["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes7["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes7["page"].InnerText;
					xmlNodes9.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/techniques/technique"));
			foreach (XmlNode xmlNodes11 in xmlNodeLists1)
			{
				xmlNodes11.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/techniques/technique[name = \"", xmlNodes11["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes11.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/techniques")).RemoveChild(xmlNodes11);
				}
			}
			try
			{
				XmlNode xmlNodes12 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/maneuvers"));
				xmlNodes12.RemoveChild(xmlNodes13);
			}
			catch
			{
			}
		}

		private void ProcessMentors()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\mentors.xml"));
			string str = "mentors.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/mentors/mentor"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/mentors/mentor[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 == null)
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mentors"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("mentors");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes1 = this._objDataDoc.CreateElement("mentor");
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("name"));
					xmlNodes1["name"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("translate"));
					xmlNodes1["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("page"));
					xmlNodes1["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(xmlNodes1);
				}
				else if (xmlNodes1["id"] == null)
				{
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				if (xmlNodes1["advantage"] == null)
				{
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("advantage"));
					xmlNodes1["advantage"].InnerText = xmlNodes["advantage"].InnerText;
				}
				if (xmlNodes1["disadvantage"] == null)
				{
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("disadvantage"));
					xmlNodes1["disadvantage"].InnerText = xmlNodes["disadvantage"].InnerText;
				}
				if (xmlNodes1["choices"] == null)
				{
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("choices"));
				}
				foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes(string.Concat("/chummer/mentors/mentor[name=\"", innerText1, "\"]/choices/choice")))
				{
					XmlDocument xmlDocument2 = this._objDataDoc;
					object[] item = new object[] { "/chummer/chummer[@file = \"", str, "\"]/mentors/mentor[name=\"", innerText1, "\"]/choices/choice[name=\"", xmlNodes5["name"], "\"]" };
					if (xmlDocument2.SelectSingleNode(string.Concat(item)) != null)
					{
						continue;
					}
					XmlNode xmlNodes6 = this._objDataDoc.CreateElement("choice");
					XmlNode innerText2 = this._objDataDoc.CreateElement("name");
					innerText2.InnerText = xmlNodes5["name"].InnerText;
					xmlNodes6.AppendChild(innerText2);
					XmlNode innerText3 = this._objDataDoc.CreateElement("translate");
					innerText3.InnerText = xmlNodes5["name"].InnerText;
					xmlNodes6.AppendChild(innerText3);
					if (xmlNodes5.Attributes.Count > 0)
					{
						XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("set");
						xmlAttribute1.Value = xmlNodes5.Attributes["set"].InnerText;
						xmlNodes6.Attributes.Append(xmlAttribute1);
					}
					xmlNodes1["choices"].AppendChild(xmlNodes6);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mentors/mentor"));
			foreach (XmlNode xmlNodes7 in xmlNodeLists)
			{
				xmlNodes7.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/mentors/mentor[name = \"", xmlNodes7["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute2.Value = "False";
					xmlNodes7.Attributes.Append(xmlAttribute2);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mentors")).RemoveChild(xmlNodes7);
				}
			}
		}

		private void ProcessMetamagic()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\metamagic.xml"));
			string str = "metamagic.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/metamagics/metamagic"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/metamagics/metamagic[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metamagics"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("metamagics");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("metamagic");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText2["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metamagics/metamagic"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/metamagics/metamagic[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metamagics")).RemoveChild(xmlNodes5);
				}
			}
			foreach (XmlNode xmlNodes6 in xmlDocument.SelectNodes("/chummer/arts/art"))
			{
				string str1 = "";
				string str2 = xmlNodes6["name"].InnerText;
				str1 = xmlNodes6["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/arts/art[name=\"", str2, "\"]" };
				XmlNode xmlNodes7 = xmlDocument2.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes7 != null)
				{
					if (xmlNodes7["id"] != null)
					{
						continue;
					}
					xmlNodes7.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes7["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/arts"));
					if (xmlNodes8 == null)
					{
						XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes9 == null)
						{
							xmlNodes9 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes9.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes9);
						}
						xmlNodes8 = this._objDataDoc.CreateElement("arts");
						xmlNodes9.AppendChild(xmlNodes8);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("art");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes6["page"].InnerText;
					xmlNodes8.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/arts/art"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/arts/art[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/arts")).RemoveChild(xmlNodes10);
				}
			}
		}

		private void ProcessMetatypes()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\metatypes.xml"));
			string str = "metatypes.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/metatypes/metatype"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 == null)
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("metatypes");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							xmlNodes9.AppendChild(xmlNodes8);
						}
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("metatype");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
				else if (xmlNodes6["id"] == null)
				{
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype[name=\"", innerText2, "\"]/metavariants" };
				XmlNode xmlNodes10 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				XmlDocument xmlDocument4 = this._objDataDoc;
				string[] strArrays2 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype[name=\"", innerText2, "\"]" };
				xmlNodes6 = xmlDocument4.SelectSingleNode(string.Concat(strArrays2));
				try
				{
					xmlNodes6.RemoveChild(xmlNodes10);
				}
				catch
				{
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes/metatype"));
			foreach (XmlNode xmlNodes11 in xmlNodeLists1)
			{
				xmlNodes11.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/metatypes/metatype[name = \"", xmlNodes11["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes11.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/metatypes")).RemoveChild(xmlNodes11);
				}
			}
		}

		private void ProcessPowers()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\powers.xml"));
			string str = "powers.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/powers/power"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/powers/power[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("powers");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlNode xmlNodes4 = this._objDataDoc.SelectSingleNode("/chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							xmlNodes4.AppendChild(xmlNodes3);
						}
						xmlNodes3.AppendChild(xmlNodes2);
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("power");
					innerText2.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText2["id"].InnerText = innerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText2["name"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
					innerText2.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText2["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers/power"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/powers/power[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/powers")).RemoveChild(xmlNodes5);
				}
			}
			foreach (XmlNode xmlNodes6 in xmlDocument.SelectNodes("/chummer/enhancements/enhancement"))
			{
				string str1 = "";
				string str2 = xmlNodes6["name"].InnerText;
				str1 = xmlNodes6["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/enhancements/enhancement[name=\"", str2, "\"]" };
				XmlNode xmlNodes7 = xmlDocument2.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes7 != null)
				{
					if (xmlNodes7["id"] != null)
					{
						continue;
					}
					xmlNodes7.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes7["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/enhancements"));
					if (xmlNodes8 == null)
					{
						XmlNode xmlNodes9 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes9 == null)
						{
							xmlNodes9 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes9.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes9);
						}
						xmlNodes8 = this._objDataDoc.CreateElement("enhancements");
						xmlNodes9.AppendChild(xmlNodes8);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("enhancement");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes6["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes6["page"].InnerText;
					xmlNodes8.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/enhancements/enhancement"));
			foreach (XmlNode xmlNodes10 in xmlNodeLists1)
			{
				xmlNodes10.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/enhancements/enhancement[name = \"", xmlNodes10["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes10.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/enhancements")).RemoveChild(xmlNodes10);
				}
			}
		}

		private void ProcessPriorities()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\priorities.xml"));
			string str = "priorities.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/priorities/priority"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/priorities/priority[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/priorities"));
					if (xmlNodes7 == null)
					{
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
						}
						xmlNodes7 = this._objDataDoc.CreateElement("priorities");
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("priority");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/priorities/priority"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				xmlNodes9.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/priorities/priority[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes9.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/priorities")).RemoveChild(xmlNodes9);
				}
			}
			foreach (XmlNode xmlNodes10 in xmlDocument.SelectNodes("/chummer/gameplayoptions/gameplayoption"))
			{
				string str2 = "";
				string str3 = xmlNodes10["name"].InnerText;
				str2 = xmlNodes10["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/gameplayoptions/gameplayoption[name=\"", str3, "\"]" };
				XmlNode xmlNodes11 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes11 != null)
				{
					if (xmlNodes11["id"] != null)
					{
						continue;
					}
					xmlNodes11.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes11["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes12 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gameplayoptions"));
					if (xmlNodes12 == null)
					{
						XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes13 == null)
						{
							xmlNodes13 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes13.Attributes.Append(xmlAttribute4);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes13);
						}
						xmlNodes12 = this._objDataDoc.CreateElement("gameplayoptions");
						xmlNodes13.AppendChild(xmlNodes12);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("gameplayoption");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes10["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes10["name"].InnerText;
					xmlNodes12.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gameplayoptions/gameplayoption"));
			foreach (XmlNode xmlNodes14 in xmlNodeLists2)
			{
				xmlNodes14.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/gameplayoptions/gameplayoption[name = \"", xmlNodes14["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes14.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/gameplayoptions")).RemoveChild(xmlNodes14);
				}
			}
			try
			{
				XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes16 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/maneuvers"));
				xmlNodes15.RemoveChild(xmlNodes16);
			}
			catch
			{
			}
		}

		private void ProcessPrograms()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\programs.xml"));
			string str = "programs.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/programs/program"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/programs/program[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/programs"));
					if (xmlNodes7 == null)
					{
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
						}
						xmlNodes7 = this._objDataDoc.CreateElement("programs");
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("program");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/programs/program"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				xmlNodes9.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/programs/program[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes9.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/programs")).RemoveChild(xmlNodes9);
				}
			}
			try
			{
				XmlNode xmlNodes10 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes11 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/options"));
				xmlNodes10.RemoveChild(xmlNodes11);
			}
			catch
			{
			}
		}

		private void ProcessQualities()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\qualities.xml"));
			string str = "qualities.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/qualities/quality"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/qualities/quality[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities"));
					if (xmlNodes7 == null)
					{
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
						}
						xmlNodes7 = this._objDataDoc.CreateElement("qualities");
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("quality");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities/quality"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				xmlNodes9.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/qualities/quality[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes9.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/qualities")).RemoveChild(xmlNodes9);
				}
			}
		}

		private void ProcessSkills()
		{
			string[] innerText;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\skills.xml"));
			string str = "skills.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("type");
				xmlAttribute1.Value = xmlNodes.Attributes["type"].InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				XmlAttribute innerText2 = this._objDataDoc.CreateAttribute("translate");
				innerText2.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(innerText2);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/skillgroups/skillgroup"))
			{
				XmlDocument xmlDocument2 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/skillgroups/skillgroup[text()=\"", xmlNodes5.InnerText, "\"]" };
				if (xmlDocument2.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes6 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skillgroups"));
				if (xmlNodes6 == null)
				{
					xmlNodes6 = this._objDataDoc.CreateElement("skillgroups");
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
						xmlAttribute2.Value = str;
						xmlNodes7.Attributes.Append(xmlAttribute2);
						xmlNodes8.AppendChild(xmlNodes7);
					}
					xmlNodes7.AppendChild(xmlNodes6);
				}
				XmlNode innerText3 = this._objDataDoc.CreateElement("skillgroup");
				innerText3.InnerText = xmlNodes5.InnerText;
				XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("type");
				xmlAttribute3.Value = xmlNodes5.Attributes["type"].InnerText;
				innerText3.Attributes.Append(xmlAttribute3);
				XmlAttribute innerText4 = this._objDataDoc.CreateAttribute("translate");
				innerText4.Value = xmlNodes5.InnerText;
				innerText3.Attributes.Append(innerText4);
				xmlNodes6.AppendChild(innerText3);
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skillgroups/skillgroup"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/skillgroups/skillgroup[text() = \"", xmlNodes9.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skillgroups")).RemoveChild(xmlNodes9);
			}
			foreach (XmlNode xmlNodes10 in xmlDocument.SelectNodes("/chummer/skills/skill"))
			{
				string str1 = "";
				string str2 = xmlNodes10["name"].InnerText;
				str1 = xmlNodes10["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/skills/skill[name=\"", str2, "\"]" };
				XmlNode innerText5 = xmlDocument3.SelectSingleNode(string.Concat(innerText));
				if (innerText5 == null)
				{
					XmlNode xmlNodes11 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skills"));
					if (xmlNodes11 == null)
					{
						XmlNode xmlNodes12 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes12 == null)
						{
							xmlNodes12 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes12.Attributes.Append(xmlAttribute4);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes12);
						}
						xmlNodes11 = this._objDataDoc.CreateElement("skills");
						xmlNodes12.AppendChild(xmlNodes11);
					}
					innerText5 = this._objDataDoc.CreateElement("skill");
					innerText5.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText5["id"].InnerText = str1;
					innerText5.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText5["name"].InnerText = xmlNodes10["name"].InnerText;
					innerText5.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText5["translate"].InnerText = xmlNodes10["name"].InnerText;
					xmlNodes11.AppendChild(innerText5);
				}
				else if (innerText5["id"] == null)
				{
					innerText5.PrependChild(this._objDataDoc.CreateElement("id"));
					innerText5["id"].InnerText = str1;
				}
				XmlNode item = innerText5["specs"];
				if (item == null)
				{
					item = this._objDataDoc.CreateElement("specs");
					innerText5.AppendChild(item);
				}
				foreach (XmlNode xmlNodes13 in xmlDocument.SelectNodes(string.Concat("/chummer/skills/skill[name=\"", str2, "\"]/specs/spec")))
				{
					XmlDocument xmlDocument4 = this._objDataDoc;
					innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/skills/skill[name=\"", str2, "\"]/specs/spec[text()=\"", xmlNodes13.InnerText, "\"]" };
					if (xmlDocument4.SelectSingleNode(string.Concat(innerText)) != null)
					{
						continue;
					}
					XmlNode innerText6 = this._objDataDoc.CreateElement("spec");
					innerText6.InnerText = xmlNodes13.InnerText;
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("translate");
					xmlAttribute5.InnerText = xmlNodes13.InnerText;
					innerText6.Attributes.Append(xmlAttribute5);
					item.AppendChild(innerText6);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skills/skill"));
			foreach (XmlNode xmlNodes14 in xmlNodeLists2)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/skills/skill[name = \"", xmlNodes14["name"].InnerText, "\"]")) != null)
				{
					XmlDocument xmlDocument5 = this._objDataDoc;
					innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/skills/skill[name = \"", xmlNodes14["name"].InnerText, "\"]/specs/spec" };
					foreach (XmlNode xmlNodes15 in xmlDocument5.SelectNodes(string.Concat(innerText)))
					{
						innerText = new string[] { "/chummer/skills/skill[name = \"", xmlNodes14["name"].InnerText, "\"]/specs/spec[text() = \"", xmlNodes15.InnerText, "\"]" };
						if (xmlDocument.SelectSingleNode(string.Concat(innerText)) != null)
						{
							continue;
						}
						if (!this._blnDelete)
						{
							XmlAttribute xmlAttribute6 = this._objDataDoc.CreateAttribute("exists");
							xmlAttribute6.Value = "False";
							xmlNodes15.Attributes.Append(xmlAttribute6);
						}
						else
						{
							xmlNodes14["specs"].RemoveChild(xmlNodes15);
						}
					}
				}
				else if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute7 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute7.Value = "False";
					xmlNodes14.Attributes.Append(xmlAttribute7);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/skills")).RemoveChild(xmlNodes14);
				}
			}
			foreach (XmlNode xmlNodes16 in xmlDocument.SelectNodes("/chummer/knowledgeskills/skill"))
			{
				string str3 = "";
				string str4 = xmlNodes16["name"].InnerText;
				str3 = xmlNodes16["id"].InnerText;
				XmlDocument xmlDocument6 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/knowledgeskills/skill[name=\"", str4, "\"]" };
				XmlNode innerText7 = xmlDocument6.SelectSingleNode(string.Concat(innerText));
				if (innerText7 == null)
				{
					XmlNode xmlNodes17 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/knowledgeskills"));
					if (xmlNodes17 == null)
					{
						XmlNode xmlNodes18 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes18 == null)
						{
							xmlNodes18 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute8 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute8.Value = str;
							xmlNodes18.Attributes.Append(xmlAttribute8);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes18);
						}
						xmlNodes17 = this._objDataDoc.CreateElement("knowledgeskills");
						xmlNodes18.AppendChild(xmlNodes17);
					}
					innerText7 = this._objDataDoc.CreateElement("skill");
					innerText7.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText7["id"].InnerText = str3;
					innerText7.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText7["name"].InnerText = xmlNodes16["name"].InnerText;
					innerText7.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText7["translate"].InnerText = xmlNodes16["name"].InnerText;
					xmlNodes17.AppendChild(innerText7);
				}
				else if (innerText7["id"] == null)
				{
					innerText7.PrependChild(this._objDataDoc.CreateElement("id"));
					innerText7["id"].InnerText = str3;
				}
				XmlNode item1 = innerText7["specs"];
				if (item1 == null)
				{
					item1 = this._objDataDoc.CreateElement("specs");
					innerText7.AppendChild(item1);
				}
				foreach (XmlNode xmlNodes19 in xmlDocument.SelectNodes(string.Concat("/chummer/knowledgeskills/skill[name=\"", str4, "\"]/specs/spec")))
				{
					XmlDocument xmlDocument7 = this._objDataDoc;
					innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/knowledgeskills/skill[name=\"", str4, "\"]/specs/spec[text()=\"", xmlNodes19.InnerText, "\"]" };
					if (xmlDocument7.SelectSingleNode(string.Concat(innerText)) != null)
					{
						continue;
					}
					XmlNode innerText8 = this._objDataDoc.CreateElement("spec");
					innerText8.InnerText = xmlNodes19.InnerText;
					XmlAttribute innerText9 = this._objDataDoc.CreateAttribute("translate");
					innerText9.InnerText = xmlNodes19.InnerText;
					innerText8.Attributes.Append(innerText9);
					item1.AppendChild(innerText8);
				}
			}
			xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/knowledgeskills/skill"));
			foreach (XmlNode xmlNodes20 in xmlNodeLists2)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/knowledgeskills/skill[name = \"", xmlNodes20["name"].InnerText, "\"]")) != null)
				{
					XmlDocument xmlDocument8 = this._objDataDoc;
					innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/knowledgeskills/skill[name = \"", xmlNodes20["name"].InnerText, "\"]/specs/spec" };
					foreach (XmlNode xmlNodes21 in xmlDocument8.SelectNodes(string.Concat(innerText)))
					{
						innerText = new string[] { "/chummer/knowledgeskills/skill[name = \"", xmlNodes20["name"].InnerText, "\"]/specs/spec[text() = \"", xmlNodes21.InnerText, "\"]" };
						if (xmlDocument.SelectSingleNode(string.Concat(innerText)) != null)
						{
							continue;
						}
						if (!this._blnDelete)
						{
							XmlAttribute xmlAttribute9 = this._objDataDoc.CreateAttribute("exists");
							xmlAttribute9.Value = "False";
							xmlNodes21.Attributes.Append(xmlAttribute9);
						}
						else
						{
							xmlNodes20["specs"].RemoveChild(xmlNodes21);
						}
					}
				}
				else if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute10 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute10.Value = "False";
					xmlNodes20.Attributes.Append(xmlAttribute10);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/knowledgeskills")).RemoveChild(xmlNodes20);
				}
			}
		}

		private void ProcessSpells()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\spells.xml"));
			string str = "spells.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/spells/spell"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/spells/spell[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spells"));
					if (xmlNodes7 == null)
					{
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
						}
						xmlNodes7 = this._objDataDoc.CreateElement("spells");
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("spell");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spells/spell"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				xmlNodes9.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/spells/spell[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes9.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spells")).RemoveChild(xmlNodes9);
				}
			}
		}

		private void ProcessStreams()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\streams.xml"));
			string str = "streams.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/traditions/tradition"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/traditions/tradition[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 == null)
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions"));
					if (xmlNodes2 == null)
					{
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes3);
						}
						xmlNodes2 = this._objDataDoc.CreateElement("traditions");
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes1 = this._objDataDoc.CreateElement("tradition");
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("name"));
					xmlNodes1["name"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("translate"));
					xmlNodes1["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("page"));
					xmlNodes1["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(xmlNodes1);
				}
				else if (xmlNodes1["id"] == null)
				{
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				if (xmlNodes1["spirits"] == null)
				{
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("spirits"));
				}
				foreach (XmlNode xmlNodes4 in xmlDocument.SelectNodes(string.Concat("/chummer/traditions/tradition[name=\"", innerText1, "\"]/spirits/spirit")))
				{
					XmlDocument xmlDocument2 = this._objDataDoc;
					object[] item = new object[] { "/chummer/chummer[@file = \"", str, "\"]/traditions/tradition[name=\"", innerText1, "\"]/spirits/spirit[name=\"", xmlNodes4["name"], "\"]" };
					if (xmlDocument2.SelectSingleNode(string.Concat(item)) != null)
					{
						continue;
					}
					XmlNode innerText2 = this._objDataDoc.CreateElement("spirit");
					innerText2.InnerText = xmlNodes4.InnerText;
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
					xmlAttribute1.InnerText = xmlNodes4.InnerText;
					innerText2.Attributes.Append(xmlAttribute1);
					xmlNodes1["spirits"].AppendChild(innerText2);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions/tradition"));
			foreach (XmlNode xmlNodes5 in xmlNodeLists)
			{
				xmlNodes5.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/traditions/tradition[name = \"", xmlNodes5["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute2.Value = "False";
					xmlNodes5.Attributes.Append(xmlAttribute2);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions")).RemoveChild(xmlNodes5);
				}
			}
		}

		private void ProcessTraditions()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\traditions.xml"));
			string str = "traditions.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/traditions/tradition"))
			{
				string innerText = "";
				string innerText1 = xmlNodes["name"].InnerText;
				innerText = xmlNodes["id"].InnerText;
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/traditions/tradition[name=\"", innerText1, "\"]" };
				XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes1 != null)
				{
					if (xmlNodes1["id"] != null)
					{
						continue;
					}
					xmlNodes1.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
				}
				else
				{
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions"));
					if (xmlNodes2 == null)
					{
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes3 == null)
						{
							xmlNodes3 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
							xmlAttribute.Value = str;
							xmlNodes3.Attributes.Append(xmlAttribute);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes3);
						}
						xmlNodes2 = this._objDataDoc.CreateElement("traditions");
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes1 = this._objDataDoc.CreateElement("tradition");
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("id"));
					xmlNodes1["id"].InnerText = innerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("name"));
					xmlNodes1["name"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("translate"));
					xmlNodes1["translate"].InnerText = xmlNodes["name"].InnerText;
					xmlNodes1.AppendChild(this._objDataDoc.CreateElement("page"));
					xmlNodes1["page"].InnerText = xmlNodes["page"].InnerText;
					xmlNodes2.AppendChild(xmlNodes1);
				}
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions/tradition"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				xmlNodes4.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/traditions/tradition[name = \"", xmlNodes4["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute1.Value = "False";
					xmlNodes4.Attributes.Append(xmlAttribute1);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/traditions")).RemoveChild(xmlNodes4);
				}
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/spirits/spirit"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/spirits/spirit[name=\"", innerText2, "\"]" };
				XmlNode innerText3 = xmlDocument2.SelectSingleNode(string.Concat(strArrays1));
				if (innerText3 != null)
				{
					if (innerText3["id"] != null)
					{
						continue;
					}
					innerText3.PrependChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes6 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spirits"));
					if (xmlNodes6 == null)
					{
						XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes7 == null)
						{
							xmlNodes7 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes7.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes7);
						}
						xmlNodes6 = this._objDataDoc.CreateElement("spirits");
						xmlNodes7.AppendChild(xmlNodes6);
					}
					innerText3 = this._objDataDoc.CreateElement("spirit");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
					xmlNodes6.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spirits/spirit"));
			foreach (XmlNode xmlNodes8 in xmlNodeLists1)
			{
				xmlNodes8.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/spirits/spirit[name = \"", xmlNodes8["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes8.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/spirits")).RemoveChild(xmlNodes8);
				}
			}
			foreach (XmlNode xmlNodes9 in xmlDocument.SelectNodes("/chummer/drainattributes/drainattribute"))
			{
				string str2 = "";
				string str3 = xmlNodes9["name"].InnerText;
				str2 = xmlNodes9["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays2 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/drainattributes/drainattribute[name=\"", str3, "\"]" };
				XmlNode innerText4 = xmlDocument3.SelectSingleNode(string.Concat(strArrays2));
				if (innerText4 != null)
				{
					if (innerText4["id"] != null)
					{
						continue;
					}
					innerText4.PrependChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes10 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/drainattributes"));
					if (xmlNodes10 == null)
					{
						XmlNode xmlNodes11 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes11 == null)
						{
							xmlNodes11 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes11.Attributes.Append(xmlAttribute4);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes11);
						}
						xmlNodes10 = this._objDataDoc.CreateElement("drainattributes");
						xmlNodes11.AppendChild(xmlNodes10);
					}
					innerText4 = this._objDataDoc.CreateElement("drainattribute");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes9["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes9["name"].InnerText;
					xmlNodes10.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/drainattributes/drainattribute"));
			foreach (XmlNode xmlNodes12 in xmlNodeLists2)
			{
				xmlNodes12.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/drainattributes/drainattribute[name = \"", xmlNodes12["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes12.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/drainattributes")).RemoveChild(xmlNodes12);
				}
			}
		}

		private void ProcessVehicles()
		{
			string[] innerText;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\vehicles.xml"));
			string str = "vehicles.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/modcategories/category"))
			{
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/modcategories/category[text()=\"", xmlNodes5.InnerText, "\"]" };
				if (xmlDocument2.SelectSingleNode(string.Concat(strArrays)) != null)
				{
					continue;
				}
				XmlNode xmlNodes6 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/modcategories"));
				if (xmlNodes6 == null)
				{
					xmlNodes6 = this._objDataDoc.CreateElement("modcategories");
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes7 == null)
					{
						xmlNodes7 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
						xmlAttribute2.Value = str;
						xmlNodes7.Attributes.Append(xmlAttribute2);
						xmlNodes8.AppendChild(xmlNodes7);
					}
					xmlNodes7.AppendChild(xmlNodes6);
				}
				XmlNode innerText2 = this._objDataDoc.CreateElement("category");
				innerText2.InnerText = xmlNodes5.InnerText;
				XmlAttribute innerText3 = this._objDataDoc.CreateAttribute("translate");
				innerText3.Value = xmlNodes5.InnerText;
				innerText2.Attributes.Append(innerText3);
				xmlNodes6.AppendChild(innerText2);
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/modcategories/category"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/modcategories/category[text() = \"", xmlNodes9.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/modcategories")).RemoveChild(xmlNodes9);
			}
			try
			{
				XmlNode xmlNodes10 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
				XmlNode xmlNodes11 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/limits"));
				xmlNodes10.RemoveChild(xmlNodes11);
			}
			catch
			{
			}
			foreach (XmlNode xmlNodes12 in xmlDocument.SelectNodes("/chummer/vehicles/vehicle"))
			{
				string str1 = "";
				string str2 = xmlNodes12["name"].InnerText;
				str1 = xmlNodes12["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/vehicles/vehicle[name=\"", str2, "\"]" };
				XmlNode xmlNodes13 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes13 != null)
				{
					if (xmlNodes13["id"] != null)
					{
						continue;
					}
					xmlNodes13.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes13["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes14 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/vehicles"));
					if (xmlNodes14 == null)
					{
						XmlNode xmlNodes15 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes15 == null)
						{
							xmlNodes15 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute3.Value = str;
							xmlNodes15.Attributes.Append(xmlAttribute3);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes15);
						}
						xmlNodes14 = this._objDataDoc.CreateElement("vehicles");
						xmlNodes15.AppendChild(xmlNodes14);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("vehicle");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str1;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes12["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes12["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText4["page"].InnerText = str1;
					xmlNodes14.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/vehicles/vehicle"));
			foreach (XmlNode xmlNodes16 in xmlNodeLists2)
			{
				xmlNodes16.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/vehicles/vehicle[name = \"", xmlNodes16["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute4.Value = "False";
					xmlNodes16.Attributes.Append(xmlAttribute4);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/vehicles")).RemoveChild(xmlNodes16);
				}
			}
			foreach (XmlNode xmlNodes17 in xmlDocument.SelectNodes("/chummer/mods/mod"))
			{
				string str3 = "";
				string str4 = xmlNodes17["name"].InnerText;
				str3 = xmlNodes17["id"].InnerText;
				XmlDocument xmlDocument4 = this._objDataDoc;
				innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/mods/mod[name=\"", str4, "\"]" };
				XmlNode xmlNodes18 = xmlDocument4.SelectSingleNode(string.Concat(innerText));
				if (xmlNodes18 != null)
				{
					if (xmlNodes18["id"] != null)
					{
						continue;
					}
					xmlNodes18.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes18["id"].InnerText = str3;
				}
				else
				{
					XmlNode xmlNodes19 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods"));
					if (xmlNodes19 == null)
					{
						XmlNode xmlNodes20 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes20 == null)
						{
							xmlNodes20 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute5.Value = str;
							xmlNodes20.Attributes.Append(xmlAttribute5);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes20);
						}
						xmlNodes19 = this._objDataDoc.CreateElement("mods");
						xmlNodes20.AppendChild(xmlNodes19);
					}
					XmlNode innerText5 = this._objDataDoc.CreateElement("mod");
					innerText5.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText5["id"].InnerText = str3;
					innerText5.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText5["name"].InnerText = xmlNodes17["name"].InnerText;
					innerText5.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText5["translate"].InnerText = xmlNodes17["name"].InnerText;
					innerText5.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText5["page"].InnerText = str3;
					xmlNodes19.AppendChild(innerText5);
				}
			}
			XmlNodeList xmlNodeLists3 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods/mod"));
			foreach (XmlNode xmlNodes21 in xmlNodeLists3)
			{
				xmlNodes21.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/mods/mod[name = \"", xmlNodes21["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute6 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute6.Value = "False";
					xmlNodes21.Attributes.Append(xmlAttribute6);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/mods")).RemoveChild(xmlNodes21);
				}
			}
		}

		private void ProcessWeapons()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(string.Concat(this._strPath, "data\\weapons.xml"));
			string str = "weapons.xml";
			foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
			{
				XmlDocument xmlDocument1 = this._objDataDoc;
				string[] innerText = new string[] { "/chummer/chummer[@file = \"", str, "\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]" };
				if (xmlDocument1.SelectSingleNode(string.Concat(innerText)) != null)
				{
					continue;
				}
				XmlNode xmlNodes1 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories"));
				if (xmlNodes1 == null)
				{
					xmlNodes1 = this._objDataDoc.CreateElement("categories");
					XmlNode xmlNodes2 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
					if (xmlNodes2 == null)
					{
						xmlNodes2 = this._objDataDoc.CreateElement("chummer");
						XmlNode xmlNodes3 = this._objDataDoc.SelectSingleNode("/chummer");
						XmlAttribute xmlAttribute = this._objDataDoc.CreateAttribute("file");
						xmlAttribute.Value = str;
						xmlNodes2.Attributes.Append(xmlAttribute);
						xmlNodes3.AppendChild(xmlNodes2);
					}
					xmlNodes2.AppendChild(xmlNodes1);
				}
				XmlNode innerText1 = this._objDataDoc.CreateElement("category");
				innerText1.InnerText = xmlNodes.InnerText;
				XmlAttribute xmlAttribute1 = this._objDataDoc.CreateAttribute("translate");
				xmlAttribute1.Value = xmlNodes.InnerText;
				innerText1.Attributes.Append(xmlAttribute1);
				xmlNodes1.AppendChild(innerText1);
			}
			XmlNodeList xmlNodeLists = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories/category"));
			foreach (XmlNode xmlNodes4 in xmlNodeLists)
			{
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
				{
					continue;
				}
				this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/categories")).RemoveChild(xmlNodes4);
			}
			foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/weapons/weapon"))
			{
				string str1 = "";
				string innerText2 = xmlNodes5["name"].InnerText;
				str1 = xmlNodes5["id"].InnerText;
				XmlDocument xmlDocument2 = this._objDataDoc;
				string[] strArrays = new string[] { "/chummer/chummer[@file = \"", str, "\"]/weapons/weapon[name=\"", innerText2, "\"]" };
				XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat(strArrays));
				if (xmlNodes6 != null)
				{
					if (xmlNodes6["id"] != null)
					{
						continue;
					}
					xmlNodes6.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes6["id"].InnerText = str1;
				}
				else
				{
					XmlNode xmlNodes7 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/weapons"));
					if (xmlNodes7 == null)
					{
						XmlNode xmlNodes8 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes8 == null)
						{
							xmlNodes8 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute2 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute2.Value = str;
							xmlNodes8.Attributes.Append(xmlAttribute2);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
						}
						xmlNodes7 = this._objDataDoc.CreateElement("weapons");
						xmlNodes8.AppendChild(xmlNodes7);
					}
					XmlNode innerText3 = this._objDataDoc.CreateElement("weapon");
					innerText3.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText3["id"].InnerText = str1;
					innerText3.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
					innerText3.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText3["page"].InnerText = str1;
					xmlNodes7.AppendChild(innerText3);
				}
			}
			XmlNodeList xmlNodeLists1 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/weapons/weapon"));
			foreach (XmlNode xmlNodes9 in xmlNodeLists1)
			{
				xmlNodes9.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/weapons/weapon[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute3 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute3.Value = "False";
					xmlNodes9.Attributes.Append(xmlAttribute3);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/weapons")).RemoveChild(xmlNodes9);
				}
			}
			foreach (XmlNode xmlNodes10 in xmlDocument.SelectNodes("/chummer/accessories/accessory"))
			{
				string str2 = "";
				string str3 = xmlNodes10["name"].InnerText;
				str2 = xmlNodes10["id"].InnerText;
				XmlDocument xmlDocument3 = this._objDataDoc;
				string[] strArrays1 = new string[] { "/chummer/chummer[@file = \"", str, "\"]/accessories/accessory[name=\"", str3, "\"]" };
				XmlNode xmlNodes11 = xmlDocument3.SelectSingleNode(string.Concat(strArrays1));
				if (xmlNodes11 != null)
				{
					if (xmlNodes11["id"] != null)
					{
						continue;
					}
					xmlNodes11.PrependChild(this._objDataDoc.CreateElement("id"));
					xmlNodes11["id"].InnerText = str2;
				}
				else
				{
					XmlNode xmlNodes12 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/accessories"));
					if (xmlNodes12 == null)
					{
						XmlNode xmlNodes13 = this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]"));
						if (xmlNodes13 == null)
						{
							xmlNodes13 = this._objDataDoc.CreateElement("chummer");
							XmlAttribute xmlAttribute4 = this._objDataDoc.CreateAttribute("file");
							xmlAttribute4.Value = str;
							xmlNodes13.Attributes.Append(xmlAttribute4);
							this._objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes13);
						}
						xmlNodes12 = this._objDataDoc.CreateElement("accessories");
						xmlNodes13.AppendChild(xmlNodes12);
					}
					XmlNode innerText4 = this._objDataDoc.CreateElement("accessory");
					innerText4.AppendChild(this._objDataDoc.CreateElement("id"));
					innerText4["id"].InnerText = str2;
					innerText4.AppendChild(this._objDataDoc.CreateElement("name"));
					innerText4["name"].InnerText = xmlNodes10["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("translate"));
					innerText4["translate"].InnerText = xmlNodes10["name"].InnerText;
					innerText4.AppendChild(this._objDataDoc.CreateElement("page"));
					innerText4["page"].InnerText = str2;
					xmlNodes12.AppendChild(innerText4);
				}
			}
			XmlNodeList xmlNodeLists2 = this._objDataDoc.SelectNodes(string.Concat("/chummer/chummer[@file = \"", str, "\"]/accessories/accessory"));
			foreach (XmlNode xmlNodes14 in xmlNodeLists2)
			{
				xmlNodes14.Attributes.RemoveAll();
				if (xmlDocument.SelectSingleNode(string.Concat("/chummer/accessories/accessory[name = \"", xmlNodes14["name"].InnerText, "\"]")) != null)
				{
					continue;
				}
				if (!this._blnDelete)
				{
					XmlAttribute xmlAttribute5 = this._objDataDoc.CreateAttribute("exists");
					xmlAttribute5.Value = "False";
					xmlNodes14.Attributes.Append(xmlAttribute5);
				}
				else
				{
					this._objDataDoc.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"", str, "\"]/accessories")).RemoveChild(xmlNodes14);
				}
			}
		}

		private void SetPath()
		{
			this._strPath = Application.StartupPath;
			if (!this._strPath.EndsWith("\\"))
			{
				frmMain _frmMain = this;
				_frmMain._strPath = string.Concat(_frmMain._strPath, "\\");
			}
		}

		private string ToTitle(string stringIn)
		{
			return (new CultureInfo("en-US", false)).TextInfo.ToTitleCase(stringIn);
		}
	}
}