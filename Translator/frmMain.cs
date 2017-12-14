using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Translator
{
    public partial class frmMain
    {
        private readonly bool _blnDelete = true;

        private XmlDocument _objDataDoc;

        private XmlDocument _objDoc;
        private readonly string _strPath = string.Empty;

        public frmMain()
        {
            _strPath = Application.StartupPath;
            InitializeComponent();
        }

        #region Control Events

        private void cmdCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLanguageName.Text))
            {
                MessageBox.Show("You must provide a language name.");
                return;
            }
            if (txtLanguageCode.Text.Length != 5)
            {
                MessageBox.Show("You must provide a five character language abbreviation.");
                return;
            }

            Cursor = Cursors.WaitCursor;
            string lower = txtLanguageCode.Text.ToLower();
            string str = ToTitle(txtLanguageName.Text) + " (" + lower.ToUpper() + ")";

            _objDataDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = _objDataDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            _objDataDoc.AppendChild(xmlDeclaration);
            XmlNode xmlRootChummerNode = _objDataDoc.CreateElement("chummer");
            XmlNode xmlVersionNode = _objDataDoc.CreateElement("version");
            xmlVersionNode.InnerText = "-500";
            xmlRootChummerNode.AppendChild(xmlVersionNode);
            _objDataDoc.AppendChild(xmlRootChummerNode);

            ProcessArmor();
            ProcessBioware();
            ProcessBooks();
            ProcessComplexForms();
            ProcessCritterPowers();
            ProcessCritters();
            ProcessCyberware();
            ProcessEchoes();
            ProcessGear();
            ProcessImprovements();
            ProcessLicenses();
            ProcessLifestyles();
            ProcessMartialArts();
            ProcessMentors();
            ProcessMetamagic();
            ProcessMetatypes();
            ProcessPowers();
            ProcessPriorities();
            ProcessPrograms();
            ProcessQualities();
            ProcessSkills();
            ProcessSpells();
            ProcessStreams();
            ProcessTraditions();
            ProcessVehicles();
            ProcessWeapons();
            _objDataDoc.Save(Path.Combine(_strPath, "lang", lower + "_data.xml"));

            _objDoc = new XmlDocument();
            xmlDeclaration = _objDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            _objDoc.AppendChild(xmlDeclaration);
            xmlRootChummerNode = _objDoc.CreateElement("chummer");
            xmlVersionNode = _objDoc.CreateElement("version");
            xmlVersionNode.InnerText = "-500";
            XmlNode xmlNameNode = _objDoc.CreateElement("name");
            xmlNameNode.InnerText = str;
            xmlRootChummerNode.AppendChild(xmlVersionNode);
            xmlRootChummerNode.AppendChild(xmlNameNode);
            _objDoc.AppendChild(xmlRootChummerNode);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "lang", "en-US.xml"));
            XmlNode xmlStringsNode = xmlDocument.SelectSingleNode("/chummer/strings");
            if (xmlStringsNode != null)
            {
                xmlRootChummerNode.AppendChild(_objDoc.ImportNode(xmlStringsNode, true));
            }
            _objDoc.Save(Path.Combine(_strPath, "lang", lower + ".xml"));

            LoadLanguageList();
            using (var frmTranslate = new frmTranslate(str))
            {
                frmTranslate.ShowDialog(this);
                Cursor = Cursors.Default;
            }
        }

        private void cmdEdit_Click(object sender, EventArgs e)
        {
            if (cboLanguages.SelectedIndex == -1)
                return;
            Cursor = Cursors.WaitCursor;
            using (var frmTranslate = new frmTranslate(cboLanguages.Text))
            {
                frmTranslate.ShowDialog();
                Cursor = Cursors.Default;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadLanguageList();
        }

        #endregion Control Events

        #region Methods

        private void LoadLanguageList()
        {
            cboLanguages.Items.Clear();
            foreach (string str in Directory.EnumerateFiles(Path.Combine(_strPath, "lang"), "*.xml"))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(str);
                string strInnerText = xmlDocument.SelectSingleNode("/chummer/name")?.InnerText;
                if (!string.IsNullOrEmpty(strInnerText))
                    cboLanguages.Items.Add(strInnerText);
            }
        }

        private void ProcessArmor()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "armor.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootArmorFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"armor.xml\"]");
            if (xmlRootArmorFileNode == null)
            {
                xmlRootArmorFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "armor.xml";
                xmlRootArmorFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootArmorFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootArmorFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootArmorFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Armors

            XmlNode xmlArmorNodesParent = xmlRootArmorFileNode.SelectSingleNode("armors");
            if (xmlArmorNodesParent == null)
            {
                xmlArmorNodesParent = _objDataDoc.CreateElement("armors");
                xmlRootArmorFileNode.AppendChild(xmlArmorNodesParent);
            }

            XmlNode xmlDataArmorNodeList = xmlDataDocument.SelectSingleNode("/chummer/armors");
            if (xmlDataArmorNodeList != null)
            {
                foreach (XmlNode xmlDataArmorNode in xmlDataArmorNodeList.SelectNodes("armor"))
                {
                    string strDataArmorName = xmlDataArmorNode["name"].InnerText;
                    string strDataArmorId = xmlDataArmorNode["id"].InnerText;
                    XmlNode xmlArmorNode = xmlArmorNodesParent.SelectSingleNode("armor[name=\"" + strDataArmorName + "\"]");
                    if (xmlArmorNode != null)
                    {
                        if (xmlArmorNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorId;
                            xmlArmorNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlArmorNode = _objDataDoc.CreateElement("armor");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataArmorId;
                        xmlArmorNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataArmorName;
                        xmlArmorNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataArmorName;
                        xmlArmorNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataArmorNode["page"].InnerText;
                        xmlArmorNode.AppendChild(xmlPageElement);

                        xmlArmorNodesParent.AppendChild(xmlArmorNode);
                    }
                }
            }
            foreach (XmlNode xmlArmorNode in xmlArmorNodesParent.SelectNodes("armor"))
            {
                xmlArmorNode.Attributes.RemoveAll();
                if (xmlDataArmorNodeList?.SelectSingleNode("armor[name = \"" + xmlArmorNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlArmorNodesParent.RemoveChild(xmlArmorNode);
                    }
                }
            }

            // Process Armor Mods

            XmlNode xmlArmorModNodesParent = xmlRootArmorFileNode.SelectSingleNode("mods");
            if (xmlArmorModNodesParent == null)
            {
                xmlArmorModNodesParent = _objDataDoc.CreateElement("mods");
                xmlRootArmorFileNode.AppendChild(xmlArmorModNodesParent);
            }

            XmlNode xmlDataArmorModNodeList = xmlDataDocument.SelectSingleNode("/chummer/mods");
            if (xmlDataArmorModNodeList != null)
            {
                foreach (XmlNode xmlDataArmorModNode in xmlDataArmorModNodeList.SelectNodes("mod"))
                {
                    string strDataArmorModId = xmlDataArmorModNode["id"].InnerText;
                    string strDataArmorModName = xmlDataArmorModNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlArmorModNode = xmlArmorModNodesParent.SelectSingleNode("mod[name=\"" + strDataArmorModName + "\"]");
                    if (xmlArmorModNode != null)
                    {
                        if (xmlArmorModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorModId;
                            xmlArmorModNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlArmorModNode = _objDataDoc.CreateElement("mod");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataArmorModId;
                        xmlArmorModNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataArmorModName;
                        xmlArmorModNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataArmorModName;
                        xmlArmorModNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataArmorModNode["page"].InnerText;
                        xmlArmorModNode.AppendChild(xmlPageElement);

                        xmlArmorModNodesParent.AppendChild(xmlArmorModNode);
                    }
                }
            }
            foreach (XmlNode xmlArmorModNode in xmlArmorModNodesParent.SelectNodes("mod"))
            {
                xmlArmorModNode.Attributes.RemoveAll();
                if (xmlDataArmorModNodeList?.SelectSingleNode("mod[name = \"" + xmlArmorModNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorModNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlArmorModNodesParent.RemoveChild(xmlArmorModNode);
                    }
                }
            }
        }

        private void ProcessBioware()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "bioware.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootBiowareFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"bioware.xml\"]");
            if (xmlRootBiowareFileNode == null)
            {
                xmlRootBiowareFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "bioware.xml";
                xmlRootBiowareFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBiowareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootBiowareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootBiowareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Biowares

            XmlNode xmlBiowareNodesParent = xmlRootBiowareFileNode.SelectSingleNode("biowares");
            if (xmlBiowareNodesParent == null)
            {
                xmlBiowareNodesParent = _objDataDoc.CreateElement("biowares");
                xmlRootBiowareFileNode.AppendChild(xmlBiowareNodesParent);
            }

            XmlNode xmlDataBiowareNodeList = xmlDataDocument.SelectSingleNode("/chummer/biowares");
            if (xmlDataBiowareNodeList != null)
            {
                foreach (XmlNode xmlDataBiowareNode in xmlDataBiowareNodeList.SelectNodes("bioware"))
                {
                    string strDataBiowareName = xmlDataBiowareNode["name"].InnerText;
                    string strDataBiowareId = xmlDataBiowareNode["id"].InnerText;
                    XmlNode xmlBiowareNode = xmlRootBiowareFileNode.SelectSingleNode("biowares/bioware[name=\"" + strDataBiowareName + "\"]");
                    if (xmlBiowareNode != null)
                    {
                        if (xmlBiowareNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBiowareId;
                            xmlBiowareNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlBiowareNode = _objDataDoc.CreateElement("bioware");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataBiowareId;
                        xmlBiowareNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataBiowareName;
                        xmlBiowareNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataBiowareName;
                        xmlBiowareNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataBiowareNode["page"].InnerText;
                        xmlBiowareNode.AppendChild(xmlPageElement);

                        xmlBiowareNodesParent.AppendChild(xmlBiowareNode);
                    }
                }
            }
            foreach (XmlNode xmlBiowareNode in xmlBiowareNodesParent.SelectNodes("bioware"))
            {
                xmlBiowareNode.Attributes.RemoveAll();
                if (xmlDataBiowareNodeList?.SelectSingleNode("bioware[name = \"" + xmlBiowareNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlBiowareNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlBiowareNodesParent.RemoveChild(xmlBiowareNode);
                    }
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootBiowareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = _objDataDoc.CreateElement("grades");
                xmlRootBiowareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XmlNode xmlDataGradeNodeList = xmlDataDocument.SelectSingleNode("/chummer/grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XmlNode xmlDataGradeNode in xmlDataGradeNodeList.SelectNodes("grade"))
                {
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[name=\"" + strDataGradeName + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGradeNode = _objDataDoc.CreateElement("grade");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGradeId;
                        xmlGradeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataGradeNode["page"].InnerText;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }
            foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
            {
                xmlGradeNode.Attributes.RemoveAll();
                if (xmlDataGradeNodeList?.SelectSingleNode("grade[name = \"" + xmlGradeNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                    }
                }
            }
        }

        private void ProcessBooks()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "books.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootBooksFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"books.xml\"]");
            if (xmlRootBooksFileNode == null)
            {
                xmlRootBooksFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "books.xml";
                xmlRootBooksFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBooksFileNode);
            }

            XmlNode xmlBookNodesParent = xmlRootBooksFileNode.SelectSingleNode("books");
            if (xmlBookNodesParent == null)
            {
                xmlBookNodesParent = _objDataDoc.CreateElement("books");
                xmlRootBooksFileNode.AppendChild(xmlBookNodesParent);
            }

            XmlNode xmlDataBookNodeList = xmlDataDocument.SelectSingleNode("/chummer/books");
            if (xmlDataBookNodeList != null)
            {
                foreach (XmlNode xmlDataBookNode in xmlDataBookNodeList.SelectNodes("book"))
                {
                    string strDataBookId = xmlDataBookNode["id"].InnerText;
                    string strDataBookName = xmlDataBookNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlBookNode = xmlBookNodesParent.SelectSingleNode("book[name=\"" + strDataBookName + "\"]");
                    if (xmlBookNode != null)
                    {
                        if (xmlBookNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBookId;
                            xmlBookNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlBookNode = _objDataDoc.CreateElement("book");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataBookId;
                        xmlBookNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataBookName;
                        xmlBookNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataBookName;
                        xmlBookNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataBookNode["page"].InnerText;
                        xmlBookNode.AppendChild(xmlPageElement);

                        xmlBookNodesParent.AppendChild(xmlBookNode);
                    }
                }
            }
            foreach (XmlNode xmlBookNode in xmlBookNodesParent.SelectNodes("book"))
            {
                xmlBookNode.Attributes.RemoveAll();
                if (xmlDataBookNodeList?.SelectSingleNode("book[name = \"" + xmlBookNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlBookNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlBookNodesParent.RemoveChild(xmlBookNode);
                    }
                }
            }
        }

        private void ProcessComplexForms()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "complexforms.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootComplexFormsFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"complexforms.xml\"]");
            if (xmlRootComplexFormsFileNode == null)
            {
                xmlRootComplexFormsFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "complexforms.xml";
                xmlRootComplexFormsFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootComplexFormsFileNode);
            }

            XmlNode xmlComplexFormNodesParent = xmlRootComplexFormsFileNode.SelectSingleNode("complexforms");
            if (xmlComplexFormNodesParent == null)
            {
                xmlComplexFormNodesParent = _objDataDoc.CreateElement("complexforms");
                xmlRootComplexFormsFileNode.AppendChild(xmlComplexFormNodesParent);
            }

            XmlNode xmlDataComplexFormNodeList = xmlDataDocument.SelectSingleNode("/chummer/complexforms");
            if (xmlDataComplexFormNodeList != null)
            {
                foreach (XmlNode xmlDataComplexFormNode in xmlDataComplexFormNodeList.SelectNodes("complexform"))
                {
                    string strDataComplexFormId = xmlDataComplexFormNode["id"].InnerText;
                    string strDataComplexFormName = xmlDataComplexFormNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlComplexFormNode = xmlComplexFormNodesParent.SelectSingleNode("complexform[name=\"" + strDataComplexFormName + "\"]");
                    if (xmlComplexFormNode != null)
                    {
                        if (xmlComplexFormNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataComplexFormId;
                            xmlComplexFormNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlComplexFormNode = _objDataDoc.CreateElement("complexform");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataComplexFormId;
                        xmlComplexFormNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataComplexFormName;
                        xmlComplexFormNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataComplexFormName;
                        xmlComplexFormNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataComplexFormNode["page"].InnerText;
                        xmlComplexFormNode.AppendChild(xmlPageElement);

                        xmlComplexFormNodesParent.AppendChild(xmlComplexFormNode);
                    }
                }
            }
            foreach (XmlNode xmlComplexFormNode in xmlComplexFormNodesParent.SelectNodes("complexform"))
            {
                xmlComplexFormNode.Attributes.RemoveAll();
                if (xmlDataComplexFormNodeList?.SelectSingleNode("complexform[name = \"" + xmlComplexFormNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlComplexFormNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlComplexFormNodesParent.RemoveChild(xmlComplexFormNode);
                    }
                }
            }
        }

        private void ProcessCritterPowers()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "critterpowers.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPowerFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"critterpowers.xml\"]");
            if (xmlRootPowerFileNode == null)
            {
                xmlRootPowerFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "critterpowers.xml";
                xmlRootPowerFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootPowerFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootPowerFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = _objDataDoc.CreateElement("powers");
                xmlRootPowerFileNode.AppendChild(xmlPowerNodesParent);
            }

            XmlNode xmlDataPowerNodeList = xmlDataDocument.SelectSingleNode("/chummer/powers");
            if (xmlDataPowerNodeList != null)
            {
                foreach (XmlNode xmlDataPowerNode in xmlDataPowerNodeList.SelectNodes("power"))
                {
                    string strDataPowerName = xmlDataPowerNode["name"].InnerText;
                    string strDataPowerId = xmlDataPowerNode["id"].InnerText;
                    XmlNode xmlPowerNode = xmlPowerNodesParent.SelectSingleNode("power[name=\"" + strDataPowerName + "\"]");
                    if (xmlPowerNode != null)
                    {
                        if (xmlPowerNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlPowerNode = _objDataDoc.CreateElement("power");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataPowerId;
                        xmlPowerNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataPowerNode["page"].InnerText;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }
            foreach (XmlNode xmlPowerNode in xmlPowerNodesParent.SelectNodes("power"))
            {
                xmlPowerNode.Attributes.RemoveAll();
                if (xmlDataPowerNodeList?.SelectSingleNode("power[name = \"" + xmlPowerNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPowerNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                    }
                }
            }
        }

        private void ProcessCritters()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "critters.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetatypeFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"critters.xml\"]");
            if (xmlRootMetatypeFileNode == null)
            {
                xmlRootMetatypeFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "critters.xml";
                xmlRootMetatypeFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Metatypes

            XmlNode xmlMetatypeNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("metatypes");
            if (xmlMetatypeNodesParent == null)
            {
                xmlMetatypeNodesParent = _objDataDoc.CreateElement("metatypes");
                xmlRootMetatypeFileNode.AppendChild(xmlMetatypeNodesParent);
            }

            XmlNode xmlDataMetatypeNodeList = xmlDataDocument.SelectSingleNode("/chummer/metatypes");
            if (xmlDataMetatypeNodeList != null)
            {
                foreach (XmlNode xmlDataMetatypeNode in xmlDataMetatypeNodeList.SelectNodes("metatype"))
                {
                    string strDataMetatypeName = xmlDataMetatypeNode["name"].InnerText;
                    string strDataMetatypeId = xmlDataMetatypeNode["id"].InnerText;
                    XmlNode xmlMetatypeNode = xmlMetatypeNodesParent.SelectSingleNode("metatype[name=\"" + strDataMetatypeName + "\"]");
                    if (xmlMetatypeNode != null)
                    {
                        if (xmlMetatypeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMetatypeNode = _objDataDoc.CreateElement("metatype");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMetatypeId;
                        xmlMetatypeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataMetatypeNode["page"].InnerText;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }
                }
            }
            foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodesParent.SelectNodes("metatype"))
            {
                xmlMetatypeNode.Attributes.RemoveAll();
                if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[name = \"" + xmlMetatypeNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMetatypeNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                    }
                }
            }
        }

        private void ProcessCyberware()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "cyberware.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootCyberwareFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"cyberware.xml\"]");
            if (xmlRootCyberwareFileNode == null)
            {
                xmlRootCyberwareFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "cyberware.xml";
                xmlRootCyberwareFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootCyberwareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootCyberwareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Cyberwares

            XmlNode xmlCyberwareNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("cyberwares");
            if (xmlCyberwareNodesParent == null)
            {
                xmlCyberwareNodesParent = _objDataDoc.CreateElement("cyberwares");
                xmlRootCyberwareFileNode.AppendChild(xmlCyberwareNodesParent);
            }

            XmlNode xmlDataCyberwareNodeList = xmlDataDocument.SelectSingleNode("/chummer/cyberwares");
            if (xmlDataCyberwareNodeList != null)
            {
                foreach (XmlNode xmlDataCyberwareNode in xmlDataCyberwareNodeList.SelectNodes("cyberware"))
                {
                    string strDataCyberwareName = xmlDataCyberwareNode["name"].InnerText;
                    string strDataCyberwareId = xmlDataCyberwareNode["id"].InnerText;
                    XmlNode xmlCyberwareNode = xmlRootCyberwareFileNode.SelectSingleNode("cyberwares/cyberware[name=\"" + strDataCyberwareName + "\"]");
                    if (xmlCyberwareNode != null)
                    {
                        if (xmlCyberwareNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataCyberwareId;
                            xmlCyberwareNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlCyberwareNode = _objDataDoc.CreateElement("cyberware");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataCyberwareId;
                        xmlCyberwareNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataCyberwareName;
                        xmlCyberwareNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataCyberwareName;
                        xmlCyberwareNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataCyberwareNode["page"].InnerText;
                        xmlCyberwareNode.AppendChild(xmlPageElement);

                        xmlCyberwareNodesParent.AppendChild(xmlCyberwareNode);
                    }
                }
            }
            foreach (XmlNode xmlCyberwareNode in xmlCyberwareNodesParent.SelectNodes("cyberware"))
            {
                xmlCyberwareNode.Attributes.RemoveAll();
                if (xmlDataCyberwareNodeList?.SelectSingleNode("cyberware[name = \"" + xmlCyberwareNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlCyberwareNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlCyberwareNodesParent.RemoveChild(xmlCyberwareNode);
                    }
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = _objDataDoc.CreateElement("grades");
                xmlRootCyberwareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XmlNode xmlDataGradeNodeList = xmlDataDocument.SelectSingleNode("/chummer/grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XmlNode xmlDataGradeNode in xmlDataGradeNodeList.SelectNodes("grade"))
                {
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[name=\"" + strDataGradeName + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGradeNode = _objDataDoc.CreateElement("grade");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGradeId;
                        xmlGradeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataGradeNode["page"].InnerText;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }
            foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
            {
                xmlGradeNode.Attributes.RemoveAll();
                if (xmlDataGradeNodeList?.SelectSingleNode("grade[name = \"" + xmlGradeNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                    }
                }
            }

            // Remove Cybersuites

            XmlNode xmlCybersuitesNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("suites");
            if (xmlCybersuitesNodesParent != null)
            {
                xmlRootCyberwareFileNode.RemoveChild(xmlCybersuitesNodesParent);
            }
        }

        private void ProcessEchoes()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "echoes.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootEchoesFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"echoes.xml\"]");
            if (xmlRootEchoesFileNode == null)
            {
                xmlRootEchoesFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "echoes.xml";
                xmlRootEchoesFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootEchoesFileNode);
            }

            XmlNode xmlEchoNodesParent = xmlRootEchoesFileNode.SelectSingleNode("echoes");
            if (xmlEchoNodesParent == null)
            {
                xmlEchoNodesParent = _objDataDoc.CreateElement("echoes");
                xmlRootEchoesFileNode.AppendChild(xmlEchoNodesParent);
            }

            XmlNode xmlDataEchoNodeList = xmlDataDocument.SelectSingleNode("/chummer/echoes");
            if (xmlDataEchoNodeList != null)
            {
                foreach (XmlNode xmlDataEchoNode in xmlDataEchoNodeList.SelectNodes("echo"))
                {
                    string strDataEchoId = xmlDataEchoNode["id"].InnerText;
                    string strDataEchoName = xmlDataEchoNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlEchoNode = xmlEchoNodesParent.SelectSingleNode("echo[name=\"" + strDataEchoName + "\"]");
                    if (xmlEchoNode != null)
                    {
                        if (xmlEchoNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEchoId;
                            xmlEchoNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlEchoNode = _objDataDoc.CreateElement("echo");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataEchoId;
                        xmlEchoNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataEchoName;
                        xmlEchoNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataEchoName;
                        xmlEchoNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataEchoNode["page"].InnerText;
                        xmlEchoNode.AppendChild(xmlPageElement);

                        xmlEchoNodesParent.AppendChild(xmlEchoNode);
                    }
                }
            }
            foreach (XmlNode xmlEchoNode in xmlEchoNodesParent.SelectNodes("echo"))
            {
                xmlEchoNode.Attributes.RemoveAll();
                if (xmlDataEchoNodeList?.SelectSingleNode("echo[name = \"" + xmlEchoNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlEchoNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlEchoNodesParent.RemoveChild(xmlEchoNode);
                    }
                }
            }
        }

        private void ProcessGear()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "gears.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootGearFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"gears.xml\"]");
            if (xmlRootGearFileNode == null)
            {
                xmlRootGearFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "gears.xml";
                xmlRootGearFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootGearFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootGearFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootGearFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Gears

            XmlNode xmlGearNodesParent = xmlRootGearFileNode.SelectSingleNode("gears");
            if (xmlGearNodesParent == null)
            {
                xmlGearNodesParent = _objDataDoc.CreateElement("gears");
                xmlRootGearFileNode.AppendChild(xmlGearNodesParent);
            }

            XmlNode xmlDataGearNodeList = xmlDataDocument.SelectSingleNode("/chummer/gears");
            if (xmlDataGearNodeList != null)
            {
                foreach (XmlNode xmlDataGearNode in xmlDataGearNodeList.SelectNodes("gear"))
                {
                    string strDataGearName = xmlDataGearNode["name"].InnerText;
                    string strDataGearId = xmlDataGearNode["id"].InnerText;
                    XmlNode xmlGearNode = xmlGearNodesParent.SelectSingleNode("gear[name=\"" + strDataGearName + "\"]");
                    if (xmlGearNode != null)
                    {
                        if (xmlGearNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGearId;
                            xmlGearNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGearNode = _objDataDoc.CreateElement("gear");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGearId;
                        xmlGearNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGearName;
                        xmlGearNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGearName;
                        xmlGearNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataGearNode["page"].InnerText;
                        xmlGearNode.AppendChild(xmlPageElement);

                        xmlGearNodesParent.AppendChild(xmlGearNode);
                    }
                }
            }
            foreach (XmlNode xmlGearNode in xmlGearNodesParent.SelectNodes("gear"))
            {
                xmlGearNode.Attributes.RemoveAll();
                if (xmlDataGearNodeList?.SelectSingleNode("gear[name = \"" + xmlGearNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGearNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlGearNodesParent.RemoveChild(xmlGearNode);
                    }
                }
            }
        }

        private void ProcessImprovements()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "improvements.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootImprovementsFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"improvements.xml\"]");
            if (xmlRootImprovementsFileNode == null)
            {
                xmlRootImprovementsFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "improvements.xml";
                xmlRootImprovementsFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootImprovementsFileNode);
            }

            XmlNode xmlImprovementNodesParent = xmlRootImprovementsFileNode.SelectSingleNode("improvements");
            if (xmlImprovementNodesParent == null)
            {
                xmlImprovementNodesParent = _objDataDoc.CreateElement("improvements");
                xmlRootImprovementsFileNode.AppendChild(xmlImprovementNodesParent);
            }

            XmlNode xmlDataImprovementNodeList = xmlDataDocument.SelectSingleNode("/chummer/improvements");
            if (xmlDataImprovementNodeList != null)
            {
                foreach (XmlNode xmlDataImprovementNode in xmlDataImprovementNodeList.SelectNodes("improvement"))
                {
                    string strDataImprovementId = xmlDataImprovementNode["id"].InnerText;
                    string strDataImprovementName = xmlDataImprovementNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlImprovementNode = xmlImprovementNodesParent.SelectSingleNode("improvement[name=\"" + strDataImprovementName + "\"]");
                    if (xmlImprovementNode != null)
                    {
                        if (xmlImprovementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataImprovementId;
                            xmlImprovementNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlImprovementNode = _objDataDoc.CreateElement("improvement");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataImprovementId;
                        xmlImprovementNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataImprovementName;
                        xmlImprovementNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataImprovementName;
                        xmlImprovementNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataImprovementNode["page"].InnerText;
                        xmlImprovementNode.AppendChild(xmlPageElement);

                        xmlImprovementNodesParent.AppendChild(xmlImprovementNode);
                    }
                }
            }
            foreach (XmlNode xmlImprovementNode in xmlImprovementNodesParent.SelectNodes("improvement"))
            {
                xmlImprovementNode.Attributes.RemoveAll();
                if (xmlDataImprovementNodeList?.SelectSingleNode("improvement[name = \"" + xmlImprovementNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlImprovementNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlImprovementNodesParent.RemoveChild(xmlImprovementNode);
                    }
                }
            }
        }

        private void ProcessLicenses()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "licenses.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootLicenseFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"licenses.xml\"]");
            if (xmlRootLicenseFileNode == null)
            {
                xmlRootLicenseFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "licenses.xml";
                xmlRootLicenseFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLicenseFileNode);
            }

            // Process Licenses

            XmlNode xmlLicenseNodesParent = xmlRootLicenseFileNode.SelectSingleNode("licenses");

            if (xmlLicenseNodesParent == null)
            {
                xmlLicenseNodesParent = _objDataDoc.CreateElement("licenses");
                xmlRootLicenseFileNode.AppendChild(xmlLicenseNodesParent);
            }

            XmlNode xmlDataLicenseNodeList = xmlDataDocument.SelectSingleNode("/chummer/licenses");
            if (xmlDataLicenseNodeList != null)
            {
                foreach (XmlNode xmlDataLicenseNode in xmlDataLicenseNodeList.SelectNodes("license"))
                {
                    if (xmlLicenseNodesParent.SelectSingleNode("license[text()=\"" + xmlDataLicenseNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlLicenseNode = _objDataDoc.CreateElement("license");
                        xmlLicenseNode.InnerText = xmlDataLicenseNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataLicenseNode.InnerText;
                        xmlLicenseNode.Attributes.Append(xmlTranslateAttribute);
                        xmlLicenseNodesParent.AppendChild(xmlLicenseNode);
                    }
                }
            }
            foreach (XmlNode xmlLicenseNode in xmlLicenseNodesParent.SelectNodes("license"))
            {
                if (xmlDataLicenseNodeList?.SelectSingleNode("license[text() = \"" + xmlLicenseNode.InnerText + "\"]") == null)
                {
                    xmlLicenseNodesParent.RemoveChild(xmlLicenseNode);
                }
            }
        }

        private void ProcessLifestyles()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "lifestyles.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootLifestyleFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"lifestyles.xml\"]");
            if (xmlRootLifestyleFileNode == null)
            {
                xmlRootLifestyleFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "lifestyles.xml";
                xmlRootLifestyleFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLifestyleFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootLifestyleFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Lifestyles

            XmlNode xmlLifestyleNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("lifestyles");
            if (xmlLifestyleNodesParent == null)
            {
                xmlLifestyleNodesParent = _objDataDoc.CreateElement("lifestyles");
                xmlRootLifestyleFileNode.AppendChild(xmlLifestyleNodesParent);
            }

            XmlNode xmlDataLifestyleNodeList = xmlDataDocument.SelectSingleNode("/chummer/lifestyles");
            if (xmlDataLifestyleNodeList != null)
            {
                foreach (XmlNode xmlDataLifestyleNode in xmlDataLifestyleNodeList.SelectNodes("lifestyle"))
                {
                    string strDataLifestyleName = xmlDataLifestyleNode["name"].InnerText;
                    string strDataLifestyleId = xmlDataLifestyleNode["id"].InnerText;
                    XmlNode xmlLifestyleNode = xmlLifestyleNodesParent.SelectSingleNode("lifestyle[name=\"" + strDataLifestyleName + "\"]");
                    if (xmlLifestyleNode != null)
                    {
                        if (xmlLifestyleNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataLifestyleId;
                            xmlLifestyleNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlLifestyleNode = _objDataDoc.CreateElement("lifestyle");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataLifestyleId;
                        xmlLifestyleNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataLifestyleName;
                        xmlLifestyleNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataLifestyleName;
                        xmlLifestyleNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataLifestyleNode["page"].InnerText;
                        xmlLifestyleNode.AppendChild(xmlPageElement);

                        xmlLifestyleNodesParent.AppendChild(xmlLifestyleNode);
                    }
                }
            }
            foreach (XmlNode xmlLifestyleNode in xmlLifestyleNodesParent.SelectNodes("lifestyle"))
            {
                xmlLifestyleNode.Attributes.RemoveAll();
                if (xmlDataLifestyleNodeList?.SelectSingleNode("lifestyle[name = \"" + xmlLifestyleNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlLifestyleNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlLifestyleNodesParent.RemoveChild(xmlLifestyleNode);
                    }
                }
            }

            // Process Lifestyle Qualities

            XmlNode xmlQualityNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("qualities");
            if (xmlQualityNodesParent == null)
            {
                xmlQualityNodesParent = _objDataDoc.CreateElement("qualities");
                xmlRootLifestyleFileNode.AppendChild(xmlQualityNodesParent);
            }

            XmlNode xmlDataQualityNodeList = xmlDataDocument.SelectSingleNode("/chummer/qualities");
            if (xmlDataQualityNodeList != null)
            {
                foreach (XmlNode xmlDataQualityNode in xmlDataQualityNodeList.SelectNodes("quality"))
                {
                    string strDataQualityId = xmlDataQualityNode["id"].InnerText;
                    string strDataQualityName = xmlDataQualityNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlQualityNode = xmlQualityNodesParent.SelectSingleNode("quality[name=\"" + strDataQualityName + "\"]");
                    if (xmlQualityNode != null)
                    {
                        if (xmlQualityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataQualityId;
                            xmlQualityNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlQualityNode = _objDataDoc.CreateElement("quality");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataQualityId;
                        xmlQualityNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataQualityNode["page"].InnerText;
                        xmlQualityNode.AppendChild(xmlPageElement);

                        xmlQualityNodesParent.AppendChild(xmlQualityNode);
                    }
                }
            }
            foreach (XmlNode xmlQualityNode in xmlQualityNodesParent.SelectNodes("quality"))
            {
                xmlQualityNode.Attributes.RemoveAll();
                if (xmlDataQualityNodeList?.SelectSingleNode("quality[name = \"" + xmlQualityNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlQualityNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlQualityNodesParent.RemoveChild(xmlQualityNode);
                    }
                }
            }

            // Remove Comforts, Entertainments, Necessities, Neighorhoods, and Securities

            XmlNode xmlRemoveNode = xmlRootLifestyleFileNode.SelectSingleNode("comforts");
            if (xmlRemoveNode != null)
            {
                xmlRootLifestyleFileNode.RemoveChild(xmlRemoveNode);
            }
            xmlRemoveNode = xmlRootLifestyleFileNode.SelectSingleNode("entertainments");
            if (xmlRemoveNode != null)
            {
                xmlRootLifestyleFileNode.RemoveChild(xmlRemoveNode);
            }
            xmlRemoveNode = xmlRootLifestyleFileNode.SelectSingleNode("necessities");
            if (xmlRemoveNode != null)
            {
                xmlRootLifestyleFileNode.RemoveChild(xmlRemoveNode);
            }
            xmlRemoveNode = xmlRootLifestyleFileNode.SelectSingleNode("neighborhoods");
            if (xmlRemoveNode != null)
            {
                xmlRootLifestyleFileNode.RemoveChild(xmlRemoveNode);
            }
            xmlRemoveNode = xmlRootLifestyleFileNode.SelectSingleNode("securities");
            if (xmlRemoveNode != null)
            {
                xmlRootLifestyleFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private void ProcessMartialArts()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "martialarts.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMartialArtFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"martialarts.xml\"]");
            if (xmlRootMartialArtFileNode == null)
            {
                xmlRootMartialArtFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "martialarts.xml";
                xmlRootMartialArtFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMartialArtFileNode);
            }

            // Process Martial Arts

            XmlNode xmlMartialArtNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("martialarts");
            if (xmlMartialArtNodesParent == null)
            {
                xmlMartialArtNodesParent = _objDataDoc.CreateElement("martialarts");
                xmlRootMartialArtFileNode.AppendChild(xmlMartialArtNodesParent);
            }

            XmlNode xmlDataMartialArtNodeList = xmlDataDocument.SelectSingleNode("/chummer/martialarts");
            if (xmlDataMartialArtNodeList != null)
            {
                foreach (XmlNode xmlDataMartialArtNode in xmlDataMartialArtNodeList.SelectNodes("martialart"))
                {
                    string strDataMartialArtName = xmlDataMartialArtNode["name"].InnerText;
                    string strDataMartialArtId = xmlDataMartialArtNode["id"].InnerText;
                    XmlNode xmlMartialArtNode = xmlRootMartialArtFileNode.SelectSingleNode("martialarts/martialart[name=\"" + strDataMartialArtName + "\"]");
                    if (xmlMartialArtNode != null)
                    {
                        if (xmlMartialArtNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMartialArtId;
                            xmlMartialArtNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMartialArtNode = _objDataDoc.CreateElement("martialart");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMartialArtId;
                        xmlMartialArtNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMartialArtName;
                        xmlMartialArtNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMartialArtName;
                        xmlMartialArtNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataMartialArtNode["page"].InnerText;
                        xmlMartialArtNode.AppendChild(xmlPageElement);

                        xmlMartialArtNodesParent.AppendChild(xmlMartialArtNode);
                    }
                }
            }
            foreach (XmlNode xmlMartialArtNode in xmlMartialArtNodesParent.SelectNodes("martialart"))
            {
                // Remove Advantages from within MartialArt
                XmlNode xmlRemoveAdvantageNode = xmlMartialArtNode.SelectSingleNode("advantages");
                if (xmlRemoveAdvantageNode != null)
                {
                    xmlMartialArtNode.RemoveChild(xmlRemoveAdvantageNode);
                }

                xmlMartialArtNode.Attributes.RemoveAll();
                if (xmlDataMartialArtNodeList?.SelectSingleNode("martialart[name = \"" + xmlMartialArtNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMartialArtNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlMartialArtNodesParent.RemoveChild(xmlMartialArtNode);
                    }
                }
            }

            // Process Techniques

            XmlNode xmlTechniqueNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("techniques");
            if (xmlTechniqueNodesParent == null)
            {
                xmlTechniqueNodesParent = _objDataDoc.CreateElement("techniques");
                xmlRootMartialArtFileNode.AppendChild(xmlTechniqueNodesParent);
            }

            XmlNode xmlDataTechniqueNodeList = xmlDataDocument.SelectSingleNode("/chummer/techniques");
            if (xmlDataTechniqueNodeList != null)
            {
                foreach (XmlNode xmlDataTechniqueNode in xmlDataTechniqueNodeList.SelectNodes("technique"))
                {
                    string strDataTechniqueId = xmlDataTechniqueNode["id"].InnerText;
                    string strDataTechniqueName = xmlDataTechniqueNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlTechniqueNode = xmlTechniqueNodesParent.SelectSingleNode("technique[name=\"" + strDataTechniqueName + "\"]");
                    if (xmlTechniqueNode != null)
                    {
                        if (xmlTechniqueNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTechniqueId;
                            xmlTechniqueNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTechniqueNode = _objDataDoc.CreateElement("technique");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTechniqueId;
                        xmlTechniqueNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTechniqueName;
                        xmlTechniqueNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTechniqueName;
                        xmlTechniqueNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataTechniqueNode["page"].InnerText;
                        xmlTechniqueNode.AppendChild(xmlPageElement);

                        xmlTechniqueNodesParent.AppendChild(xmlTechniqueNode);
                    }
                }
            }
            foreach (XmlNode xmlTechniqueNode in xmlTechniqueNodesParent.SelectNodes("technique"))
            {
                xmlTechniqueNode.Attributes.RemoveAll();
                if (xmlDataTechniqueNodeList?.SelectSingleNode("technique[name = \"" + xmlTechniqueNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTechniqueNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlTechniqueNodesParent.RemoveChild(xmlTechniqueNode);
                    }
                }
            }

            // Remove Maneuvers

            XmlNode xmlRemoveNode = xmlRootMartialArtFileNode.SelectSingleNode("maneuvers");
            if (xmlRemoveNode != null)
            {
                xmlRootMartialArtFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private void ProcessMentors()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "mentors.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/mentors/mentor"))
            {
                string innerText = string.Empty;
                string innerText1 = xmlNodes["name"].InnerText;
                innerText = xmlNodes["id"].InnerText;
                XmlDocument xmlDocument1 = _objDataDoc;
                XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]/mentors/mentor[name=\"" + innerText1 + "\"]");
                if (xmlNodes1 == null)
                {
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]/mentors");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("mentors");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]");
                        if (xmlNodes3 == null)
                        {
                            xmlNodes3 = _objDataDoc.CreateElement("chummer");
                            XmlNode xmlNodes4 = _objDataDoc.SelectSingleNode("/chummer");
                            XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                            xmlAttribute.Value = "mentors.xml";
                            xmlNodes3.Attributes.Append(xmlAttribute);
                            xmlNodes4.AppendChild(xmlNodes3);
                        }
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes1 = _objDataDoc.CreateElement("mentor");
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("id"));
                    xmlNodes1["id"].InnerText = innerText;
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("name"));
                    xmlNodes1["name"].InnerText = xmlNodes["name"].InnerText;
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("translate"));
                    xmlNodes1["translate"].InnerText = xmlNodes["name"].InnerText;
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("page"));
                    xmlNodes1["page"].InnerText = xmlNodes["page"].InnerText;
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                else if (xmlNodes1["id"] == null)
                {
                    xmlNodes1.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes1["id"].InnerText = innerText;
                }
                if (xmlNodes1["advantage"] == null)
                {
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("advantage"));
                    xmlNodes1["advantage"].InnerText = xmlNodes["advantage"].InnerText;
                }
                if (xmlNodes1["disadvantage"] == null)
                {
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("disadvantage"));
                    xmlNodes1["disadvantage"].InnerText = xmlNodes["disadvantage"].InnerText;
                }
                if (xmlNodes1["choices"] == null)
                    xmlNodes1.AppendChild(_objDataDoc.CreateElement("choices"));
                foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/mentors/mentor[name=\"" + innerText1 + "\"]/choices/choice"))
                {
                    XmlDocument xmlDocument2 = _objDataDoc;
                    if (xmlDocument2.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]/mentors/mentor[name=\"" + innerText1 + "\"]/choices/choice[name=\"" + xmlNodes5["name"]?.InnerText + "\"]") != null)
                        continue;
                    XmlNode xmlNodes6 = _objDataDoc.CreateElement("choice");
                    XmlNode innerText2 = _objDataDoc.CreateElement("name");
                    innerText2.InnerText = xmlNodes5["name"].InnerText;
                    xmlNodes6.AppendChild(innerText2);
                    XmlNode innerText3 = _objDataDoc.CreateElement("translate");
                    innerText3.InnerText = xmlNodes5["name"].InnerText;
                    xmlNodes6.AppendChild(innerText3);
                    if (xmlNodes5.Attributes.Count > 0)
                    {
                        XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("set");
                        xmlAttribute1.Value = xmlNodes5.Attributes["set"].InnerText;
                        xmlNodes6.Attributes.Append(xmlAttribute1);
                    }
                    xmlNodes1["choices"].AppendChild(xmlNodes6);
                }
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"mentors.xml\"]/mentors/mentor");
            foreach (XmlNode xmlNodes7 in xmlNodeLists)
            {
                xmlNodes7.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode("/chummer/mentors/mentor[name = \"" + xmlNodes7["name"].InnerText + "\"]") != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute2.Value = "False";
                    xmlNodes7.Attributes.Append(xmlAttribute2);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]/mentors").RemoveChild(xmlNodes7);
                }
            }
        }

        private void ProcessMetamagic()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "metamagic.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/metamagics/metamagic"))
            {
                string innerText = string.Empty;
                string innerText1 = xmlNodes["name"].InnerText;
                innerText = xmlNodes["id"].InnerText;
                XmlDocument xmlDocument1 = _objDataDoc;
                XmlNode xmlNodes1 = xmlDocument1.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/metamagics/metamagic[name=\"" + innerText1 + "\"]");
                if (xmlNodes1 != null)
                {
                    if (xmlNodes1["id"] != null)
                        continue;
                    xmlNodes1.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes1["id"].InnerText = innerText;
                }
                else
                {
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/metamagics");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("metamagics");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]");
                        if (xmlNodes3 == null)
                        {
                            xmlNodes3 = _objDataDoc.CreateElement("chummer");
                            XmlNode xmlNodes4 = _objDataDoc.SelectSingleNode("/chummer");
                            XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                            xmlAttribute.Value = "metamagic.xml";
                            xmlNodes3.Attributes.Append(xmlAttribute);
                            xmlNodes4.AppendChild(xmlNodes3);
                        }
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    XmlNode innerText2 = _objDataDoc.CreateElement("metamagic");
                    innerText2.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText2["id"].InnerText = innerText;
                    innerText2.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText2["name"].InnerText = xmlNodes["name"].InnerText;
                    innerText2.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText2["translate"].InnerText = xmlNodes["name"].InnerText;
                    innerText2.AppendChild(_objDataDoc.CreateElement("page"));
                    innerText2["page"].InnerText = xmlNodes["page"].InnerText;
                    xmlNodes2.AppendChild(innerText2);
                }
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"metamagic.xml\"]/metamagics/metamagic");
            foreach (XmlNode xmlNodes5 in xmlNodeLists)
            {
                xmlNodes5.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + xmlNodes5["name"].InnerText + "\"]") != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute1.Value = "False";
                    xmlNodes5.Attributes.Append(xmlAttribute1);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/metamagics").RemoveChild(xmlNodes5);
                }
            }
            foreach (XmlNode xmlNodes6 in xmlDocument.SelectNodes("/chummer/arts/art"))
            {
                string str1 = string.Empty;
                string str2 = xmlNodes6["name"].InnerText;
                str1 = xmlNodes6["id"].InnerText;
                XmlDocument xmlDocument2 = _objDataDoc;
                XmlNode xmlNodes7 = xmlDocument2.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/arts/art[name=\"" + str2 + "\"]");
                if (xmlNodes7 != null)
                {
                    if (xmlNodes7["id"] != null)
                        continue;
                    xmlNodes7.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes7["id"].InnerText = str1;
                }
                else
                {
                    XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/arts");
                    if (xmlNodes8 == null)
                    {
                        XmlNode xmlNodes9 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]");
                        if (xmlNodes9 == null)
                        {
                            xmlNodes9 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute2.Value = "metamagic.xml";
                            xmlNodes9.Attributes.Append(xmlAttribute2);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes9);
                        }
                        xmlNodes8 = _objDataDoc.CreateElement("arts");
                        xmlNodes9.AppendChild(xmlNodes8);
                    }
                    XmlNode innerText3 = _objDataDoc.CreateElement("art");
                    innerText3.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText3["id"].InnerText = str1;
                    innerText3.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText3["name"].InnerText = xmlNodes6["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText3["translate"].InnerText = xmlNodes6["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("page"));
                    innerText3["page"].InnerText = xmlNodes6["page"].InnerText;
                    xmlNodes8.AppendChild(innerText3);
                }
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"metamagic.xml\"]/arts/art");
            foreach (XmlNode xmlNodes10 in xmlNodeLists1)
            {
                xmlNodes10.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode("/chummer/arts/art[name = \"" + xmlNodes10["name"].InnerText + "\"]") != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute3.Value = "False";
                    xmlNodes10.Attributes.Append(xmlAttribute3);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]/arts").RemoveChild(xmlNodes10);
                }
            }
        }

        private void ProcessMetatypes()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "metatypes.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/categories/category[text()=\"" + xmlNodes.InnerText + "\"]") != null)
                    continue;
                XmlNode xmlNodes1 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "metatypes.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("translate");
                xmlAttribute1.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"metatypes.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode("/chummer/categories/category[text() = \"" + xmlNodes4.InnerText + "\"]") != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/metatypes/metatype"))
            {
                string str1 = string.Empty;
                string innerText2 = xmlNodes5["name"].InnerText;
                str1 = xmlNodes5["id"].InnerText;
                XmlDocument xmlDocument2 = _objDataDoc;
                XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes/metatype[name=\"" + innerText2 + "\"]");
                if (xmlNodes6 == null)
                {
                    XmlNode xmlNodes7 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes");
                    if (xmlNodes7 == null)
                    {
                        xmlNodes7 = _objDataDoc.CreateElement("metatypes");
                        XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]");
                        if (xmlNodes8 == null)
                        {
                            xmlNodes8 = _objDataDoc.CreateElement("chummer");
                            XmlNode xmlNodes9 = _objDataDoc.SelectSingleNode("/chummer");
                            XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute2.Value = "metatypes.xml";
                            xmlNodes8.Attributes.Append(xmlAttribute2);
                            xmlNodes9.AppendChild(xmlNodes8);
                        }
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    XmlNode innerText3 = _objDataDoc.CreateElement("metatype");
                    innerText3.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText3["id"].InnerText = str1;
                    innerText3.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("page"));
                    innerText3["page"].InnerText = xmlNodes5["page"].InnerText;
                    xmlNodes7.AppendChild(innerText3);
                }
                else if (xmlNodes6["id"] == null)
                {
                    xmlNodes6.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes6["id"].InnerText = str1;
                }
                XmlDocument xmlDocument3 = _objDataDoc;
                XmlNode xmlNodes10 = xmlDocument3.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes/metatype[name=\"" + innerText2 + "\"]/metavariants");
                XmlDocument xmlDocument4 = _objDataDoc;
                xmlNodes6 = xmlDocument4.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes/metatype[name=\"" + innerText2 + "\"]");
                try
                {
                    xmlNodes6.RemoveChild(xmlNodes10);
                }
                catch
                {
                }
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes/metatype");
            foreach (XmlNode xmlNodes11 in xmlNodeLists1)
            {
                xmlNodes11.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + xmlNodes11["name"].InnerText + "\"]") != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute3.Value = "False";
                    xmlNodes11.Attributes.Append(xmlAttribute3);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]/metatypes").RemoveChild(xmlNodes11);
                }
            }
        }

        private void ProcessPowers()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "powers.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPowerFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"powers.xml\"]");
            if (xmlRootPowerFileNode == null)
            {
                xmlRootPowerFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "powers.xml";
                xmlRootPowerFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = _objDataDoc.CreateElement("powers");
                xmlRootPowerFileNode.AppendChild(xmlPowerNodesParent);
            }

            XmlNode xmlDataPowerNodeList = xmlDataDocument.SelectSingleNode("/chummer/powers");
            if (xmlDataPowerNodeList != null)
            {
                foreach (XmlNode xmlDataPowerNode in xmlDataPowerNodeList.SelectNodes("power"))
                {
                    string strDataPowerName = xmlDataPowerNode["name"].InnerText;
                    string strDataPowerId = xmlDataPowerNode["id"].InnerText;
                    XmlNode xmlPowerNode = xmlPowerNodesParent.SelectSingleNode("power[name=\"" + strDataPowerName + "\"]");
                    if (xmlPowerNode != null)
                    {
                        if (xmlPowerNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlPowerNode = _objDataDoc.CreateElement("power");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataPowerId;
                        xmlPowerNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataPowerNode["page"].InnerText;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }
            foreach (XmlNode xmlPowerNode in xmlPowerNodesParent.SelectNodes("power"))
            {
                xmlPowerNode.Attributes.RemoveAll();
                if (xmlDataPowerNodeList?.SelectSingleNode("power[name = \"" + xmlPowerNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPowerNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                    }
                }
            }

            // Process Enhancements

            XmlNode xmlEnhancementNodesParent = xmlRootPowerFileNode.SelectSingleNode("enhancements");
            if (xmlEnhancementNodesParent == null)
            {
                xmlEnhancementNodesParent = _objDataDoc.CreateElement("enhancements");
                xmlRootPowerFileNode.AppendChild(xmlEnhancementNodesParent);
            }

            XmlNode xmlDataEnhancementNodeList = xmlDataDocument.SelectSingleNode("/chummer/enhancements");
            if (xmlDataEnhancementNodeList != null)
            {
                foreach (XmlNode xmlDataEnhancementNode in xmlDataEnhancementNodeList.SelectNodes("enhancement"))
                {
                    string strDataEnhancementId = xmlDataEnhancementNode["id"].InnerText;
                    string strDataEnhancementName = xmlDataEnhancementNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlEnhancementNode = xmlEnhancementNodesParent.SelectSingleNode("enhancement[name=\"" + strDataEnhancementName + "\"]");
                    if (xmlEnhancementNode != null)
                    {
                        if (xmlEnhancementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEnhancementId;
                            xmlEnhancementNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlEnhancementNode = _objDataDoc.CreateElement("enhancement");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataEnhancementId;
                        xmlEnhancementNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataEnhancementName;
                        xmlEnhancementNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataEnhancementName;
                        xmlEnhancementNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataEnhancementNode["page"].InnerText;
                        xmlEnhancementNode.AppendChild(xmlPageElement);

                        xmlEnhancementNodesParent.AppendChild(xmlEnhancementNode);
                    }
                }
            }
            foreach (XmlNode xmlEnhancementNode in xmlEnhancementNodesParent.SelectNodes("enhancement"))
            {
                xmlEnhancementNode.Attributes.RemoveAll();
                if (xmlDataEnhancementNodeList?.SelectSingleNode("enhancement[name = \"" + xmlEnhancementNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlEnhancementNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlEnhancementNodesParent.RemoveChild(xmlEnhancementNode);
                    }
                }
            }
        }

        private void ProcessPriorities()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "priorities.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"priorities.xml\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes1 =
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 =
                        _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "priorities.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("translate");
                xmlAttribute1.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"priorities.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/priorities/priority"))
            {
                string str1 = string.Empty;
                string innerText2 = xmlNodes5["name"].InnerText;
                str1 = xmlNodes5["id"].InnerText;
                XmlDocument xmlDocument2 = _objDataDoc;
                XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"priorities.xml\"]/priorities/priority[name=\"", innerText2, "\"]"));
                if (xmlNodes6 != null)
                {
                    if (xmlNodes6["id"] != null)
                        continue;
                    xmlNodes6.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes6["id"].InnerText = str1;
                }
                else
                {
                    XmlNode xmlNodes7 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/priorities");
                    if (xmlNodes7 == null)
                    {
                        XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]");
                        if (xmlNodes8 == null)
                        {
                            xmlNodes8 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute2.Value = "priorities.xml";
                            xmlNodes8.Attributes.Append(xmlAttribute2);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
                        }
                        xmlNodes7 = _objDataDoc.CreateElement("priorities");
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    XmlNode innerText3 = _objDataDoc.CreateElement("priority");
                    innerText3.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText3["id"].InnerText = str1;
                    innerText3.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
                    xmlNodes7.AppendChild(innerText3);
                }
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"priorities.xml\"]/priorities/priority");
            foreach (XmlNode xmlNodes9 in xmlNodeLists1)
            {
                xmlNodes9.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/priorities/priority[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute3.Value = "False";
                    xmlNodes9.Attributes.Append(xmlAttribute3);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/priorities").RemoveChild(xmlNodes9);
                }
            }
            foreach (XmlNode xmlNodes10 in xmlDocument.SelectNodes("/chummer/gameplayoptions/gameplayoption"))
            {
                string str2 = string.Empty;
                string str3 = xmlNodes10["name"].InnerText;
                str2 = xmlNodes10["id"].InnerText;
                XmlDocument xmlDocument3 = _objDataDoc;
                XmlNode xmlNodes11 = xmlDocument3.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"priorities.xml\"]/gameplayoptions/gameplayoption[name=\"", str3, "\"]"));
                if (xmlNodes11 != null)
                {
                    if (xmlNodes11["id"] != null)
                        continue;
                    xmlNodes11.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes11["id"].InnerText = str2;
                }
                else
                {
                    XmlNode xmlNodes12 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/gameplayoptions");
                    if (xmlNodes12 == null)
                    {
                        XmlNode xmlNodes13 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]");
                        if (xmlNodes13 == null)
                        {
                            xmlNodes13 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute4 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute4.Value = "priorities.xml";
                            xmlNodes13.Attributes.Append(xmlAttribute4);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes13);
                        }
                        xmlNodes12 = _objDataDoc.CreateElement("gameplayoptions");
                        xmlNodes13.AppendChild(xmlNodes12);
                    }
                    XmlNode innerText4 = _objDataDoc.CreateElement("gameplayoption");
                    innerText4.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText4["id"].InnerText = str2;
                    innerText4.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText4["name"].InnerText = xmlNodes10["name"].InnerText;
                    innerText4.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText4["translate"].InnerText = xmlNodes10["name"].InnerText;
                    xmlNodes12.AppendChild(innerText4);
                }
            }
            XmlNodeList xmlNodeLists2 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"priorities.xml\"]/gameplayoptions/gameplayoption");
            foreach (XmlNode xmlNodes14 in xmlNodeLists2)
            {
                xmlNodes14.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/gameplayoptions/gameplayoption[name = \"", xmlNodes14["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute5 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute5.Value = "False";
                    xmlNodes14.Attributes.Append(xmlAttribute5);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/gameplayoptions").RemoveChild(xmlNodes14);
                }
            }
            try
            {
                XmlNode xmlNodes15 =
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]");
                XmlNode xmlNodes16 =
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]/maneuvers");
                xmlNodes15.RemoveChild(xmlNodes16);
            }
            catch
            {
            }
        }

        private void ProcessPrograms()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "programs.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"programs.xml\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes1 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "programs.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("translate");
                xmlAttribute1.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"programs.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/programs/program"))
            {
                string str1 = string.Empty;
                string innerText2 = xmlNodes5["name"].InnerText;
                str1 = xmlNodes5["id"].InnerText;
                XmlDocument xmlDocument2 = _objDataDoc;
                XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"programs.xml\"]/programs/program[name=\"", innerText2, "\"]"));
                if (xmlNodes6 != null)
                {
                    if (xmlNodes6["id"] != null)
                        continue;
                    xmlNodes6.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes6["id"].InnerText = str1;
                }
                else
                {
                    XmlNode xmlNodes7 =
                        _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]/programs");
                    if (xmlNodes7 == null)
                    {
                        XmlNode xmlNodes8 =
                            _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]");
                        if (xmlNodes8 == null)
                        {
                            xmlNodes8 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute2.Value = "programs.xml";
                            xmlNodes8.Attributes.Append(xmlAttribute2);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
                        }
                        xmlNodes7 = _objDataDoc.CreateElement("programs");
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    XmlNode innerText3 = _objDataDoc.CreateElement("program");
                    innerText3.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText3["id"].InnerText = str1;
                    innerText3.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
                    xmlNodes7.AppendChild(innerText3);
                }
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"programs.xml\"]/programs/program");
            foreach (XmlNode xmlNodes9 in xmlNodeLists1)
            {
                xmlNodes9.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/programs/program[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute3.Value = "False";
                    xmlNodes9.Attributes.Append(xmlAttribute3);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]/programs").RemoveChild(xmlNodes9);
                }
            }
            try
            {
                XmlNode xmlNodes10 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]");
                XmlNode xmlNodes11 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]/options");
                xmlNodes10.RemoveChild(xmlNodes11);
            }
            catch
            {
            }
        }

        private void ProcessQualities()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "qualities.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"qualities.xml\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes1 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "qualities.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("translate");
                xmlAttribute1.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"qualities.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/qualities/quality"))
            {
                string str1 = string.Empty;
                string innerText2 = xmlNodes5["name"].InnerText;
                str1 = xmlNodes5["id"].InnerText;
                XmlDocument xmlDocument2 = _objDataDoc;
                XmlNode xmlNodes6 = xmlDocument2.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"qualities.xml\"]/qualities/quality[name=\"", innerText2, "\"]"));
                if (xmlNodes6 != null)
                {
                    if (xmlNodes6["id"] != null)
                        continue;
                    xmlNodes6.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes6["id"].InnerText = str1;
                }
                else
                {
                    XmlNode xmlNodes7 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]/qualities");
                    if (xmlNodes7 == null)
                    {
                        XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]");
                        if (xmlNodes8 == null)
                        {
                            xmlNodes8 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute2.Value = "qualities.xml";
                            xmlNodes8.Attributes.Append(xmlAttribute2);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes8);
                        }
                        xmlNodes7 = _objDataDoc.CreateElement("qualities");
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    XmlNode innerText3 = _objDataDoc.CreateElement("quality");
                    innerText3.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText3["id"].InnerText = str1;
                    innerText3.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText3["name"].InnerText = xmlNodes5["name"].InnerText;
                    innerText3.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText3["translate"].InnerText = xmlNodes5["name"].InnerText;
                    xmlNodes7.AppendChild(innerText3);
                }
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"qualities.xml\"]/qualities/quality");
            foreach (XmlNode xmlNodes9 in xmlNodeLists1)
            {
                xmlNodes9.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/qualities/quality[name = \"", xmlNodes9["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute3.Value = "False";
                    xmlNodes9.Attributes.Append(xmlAttribute3);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]/qualities").RemoveChild(xmlNodes9);
                }
            }
        }

        private void ProcessSkills()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "skills.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes1 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "skills.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("type");
                xmlAttribute1.Value = xmlNodes.Attributes["type"].InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                XmlAttribute innerText2 = _objDataDoc.CreateAttribute("translate");
                innerText2.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(innerText2);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"skills.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/skillgroups/skillgroup"))
            {
                XmlDocument xmlDocument2 = _objDataDoc;
                if (xmlDocument2.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/skillgroups/skillgroup[text()=\"", xmlNodes5.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes6 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/skillgroups");
                if (xmlNodes6 == null)
                {
                    xmlNodes6 = _objDataDoc.CreateElement("skillgroups");
                    XmlNode xmlNodes7 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]");
                    if (xmlNodes7 == null)
                    {
                        xmlNodes7 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                        xmlAttribute2.Value = "skills.xml";
                        xmlNodes7.Attributes.Append(xmlAttribute2);
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    xmlNodes7.AppendChild(xmlNodes6);
                }
                XmlNode innerText3 = _objDataDoc.CreateElement("skillgroup");
                innerText3.InnerText = xmlNodes5.InnerText;
                XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("type");
                xmlAttribute3.Value = xmlNodes5.Attributes["type"].InnerText;
                innerText3.Attributes.Append(xmlAttribute3);
                XmlAttribute innerText4 = _objDataDoc.CreateAttribute("translate");
                innerText4.Value = xmlNodes5.InnerText;
                innerText3.Attributes.Append(innerText4);
                xmlNodes6.AppendChild(innerText3);
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"skills.xml\"]/skillgroups/skillgroup");
            foreach (XmlNode xmlNodes9 in xmlNodeLists1)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/skillgroups/skillgroup[text() = \"", xmlNodes9.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/skillgroups").RemoveChild(xmlNodes9);
            }
            foreach (XmlNode xmlNodes10 in xmlDocument.SelectNodes("/chummer/skills/skill"))
            {
                string str1 = string.Empty;
                string str2 = xmlNodes10["name"].InnerText;
                str1 = xmlNodes10["id"].InnerText;
                XmlDocument xmlDocument3 = _objDataDoc;
                XmlNode innerText5 = xmlDocument3.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/skills/skill[name=\"", str2, "\"]"));
                if (innerText5 == null)
                {
                    XmlNode xmlNodes11 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/skills");
                    if (xmlNodes11 == null)
                    {
                        XmlNode xmlNodes12 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]");
                        if (xmlNodes12 == null)
                        {
                            xmlNodes12 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute4 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute4.Value = "skills.xml";
                            xmlNodes12.Attributes.Append(xmlAttribute4);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes12);
                        }
                        xmlNodes11 = _objDataDoc.CreateElement("skills");
                        xmlNodes12.AppendChild(xmlNodes11);
                    }
                    innerText5 = _objDataDoc.CreateElement("skill");
                    innerText5.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText5["id"].InnerText = str1;
                    innerText5.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText5["name"].InnerText = xmlNodes10["name"].InnerText;
                    innerText5.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText5["translate"].InnerText = xmlNodes10["name"].InnerText;
                    xmlNodes11.AppendChild(innerText5);
                }
                else if (innerText5["id"] == null)
                {
                    innerText5.PrependChild(_objDataDoc.CreateElement("id"));
                    innerText5["id"].InnerText = str1;
                }
                XmlNode item = innerText5["specs"];
                if (item == null)
                {
                    item = _objDataDoc.CreateElement("specs");
                    innerText5.AppendChild(item);
                }
                foreach (XmlNode xmlNodes13 in xmlDocument.SelectNodes(string.Concat("/chummer/skills/skill[name=\"", str2, "\"]/specs/spec")))
                {
                    XmlDocument xmlDocument4 = _objDataDoc;
                    if (xmlDocument4.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/skills/skill[name=\"" + str2 + "\"]/specs/spec[text()=\"" + xmlNodes13.InnerText + "\"]") != null)
                        continue;
                    XmlNode innerText6 = _objDataDoc.CreateElement("spec");
                    innerText6.InnerText = xmlNodes13.InnerText;
                    XmlAttribute xmlAttribute5 = _objDataDoc.CreateAttribute("translate");
                    xmlAttribute5.InnerText = xmlNodes13.InnerText;
                    innerText6.Attributes.Append(xmlAttribute5);
                    item.AppendChild(innerText6);
                }
            }
            XmlNodeList xmlNodeLists2 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"skills.xml\"]/skills/skill");
            foreach (XmlNode xmlNodes14 in xmlNodeLists2)
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/skills/skill[name = \"", xmlNodes14["name"].InnerText, "\"]")) != null)
                {
                    XmlDocument xmlDocument5 = _objDataDoc;
                    foreach (XmlNode xmlNodes15 in xmlDocument5.SelectNodes(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/skills/skill[name = \"", xmlNodes14["name"].InnerText, "\"]/specs/spec")))
                    {
                        if (xmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + xmlNodes14["name"].InnerText + "\"]/specs/spec[text() = \"" + xmlNodes15.InnerText + "\"]") != null)
                            continue;
                        if (!_blnDelete)
                        {
                            XmlAttribute xmlAttribute6 = _objDataDoc.CreateAttribute("exists");
                            xmlAttribute6.Value = "False";
                            xmlNodes15.Attributes.Append(xmlAttribute6);
                        }
                        else
                        {
                            xmlNodes14["specs"].RemoveChild(xmlNodes15);
                        }
                    }
                }
                else if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute7 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute7.Value = "False";
                    xmlNodes14.Attributes.Append(xmlAttribute7);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/skills").RemoveChild(xmlNodes14);
                }
            foreach (XmlNode xmlNodes16 in xmlDocument.SelectNodes("/chummer/knowledgeskills/skill"))
            {
                string str3 = string.Empty;
                string str4 = xmlNodes16["name"].InnerText;
                str3 = xmlNodes16["id"].InnerText;
                XmlDocument xmlDocument6 = _objDataDoc;
                XmlNode innerText7 = xmlDocument6.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills/skill[name=\"", str4, "\"]"));
                if (innerText7 == null)
                {
                    XmlNode xmlNodes17 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills");
                    if (xmlNodes17 == null)
                    {
                        XmlNode xmlNodes18 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]");
                        if (xmlNodes18 == null)
                        {
                            xmlNodes18 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute8 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute8.Value = "skills.xml";
                            xmlNodes18.Attributes.Append(xmlAttribute8);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes18);
                        }
                        xmlNodes17 = _objDataDoc.CreateElement("knowledgeskills");
                        xmlNodes18.AppendChild(xmlNodes17);
                    }
                    innerText7 = _objDataDoc.CreateElement("skill");
                    innerText7.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText7["id"].InnerText = str3;
                    innerText7.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText7["name"].InnerText = xmlNodes16["name"].InnerText;
                    innerText7.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText7["translate"].InnerText = xmlNodes16["name"].InnerText;
                    xmlNodes17.AppendChild(innerText7);
                }
                else if (innerText7["id"] == null)
                {
                    innerText7.PrependChild(_objDataDoc.CreateElement("id"));
                    innerText7["id"].InnerText = str3;
                }
                XmlNode item1 = innerText7["specs"];
                if (item1 == null)
                {
                    item1 = _objDataDoc.CreateElement("specs");
                    innerText7.AppendChild(item1);
                }
                foreach (XmlNode xmlNodes19 in xmlDocument.SelectNodes(string.Concat("/chummer/knowledgeskills/skill[name=\"", str4, "\"]/specs/spec")))
                {
                    XmlDocument xmlDocument7 = _objDataDoc;
                    if (xmlDocument7.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills/skill[name=\"" + str4 + "\"]/specs/spec[text()=\"" + xmlNodes19.InnerText + "\"]") != null)
                        continue;
                    XmlNode innerText8 = _objDataDoc.CreateElement("spec");
                    innerText8.InnerText = xmlNodes19.InnerText;
                    XmlAttribute innerText9 = _objDataDoc.CreateAttribute("translate");
                    innerText9.InnerText = xmlNodes19.InnerText;
                    innerText8.Attributes.Append(innerText9);
                    item1.AppendChild(innerText8);
                }
            }
            xmlNodeLists2 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills/skill");
            foreach (XmlNode xmlNodes20 in xmlNodeLists2)
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/knowledgeskills/skill[name = \"", xmlNodes20["name"].InnerText, "\"]")) != null)
                {
                    XmlDocument xmlDocument8 = _objDataDoc;
                    foreach (XmlNode xmlNodes21 in xmlDocument8.SelectNodes(string.Concat("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills/skill[name = \"", xmlNodes20["name"].InnerText, "\"]/specs/spec")))
                    {
                        if (xmlDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + xmlNodes20["name"].InnerText + "\"]/specs/spec[text() = \"" + xmlNodes21.InnerText + "\"]") != null)
                            continue;
                        if (!_blnDelete)
                        {
                            XmlAttribute xmlAttribute9 = _objDataDoc.CreateAttribute("exists");
                            xmlAttribute9.Value = "False";
                            xmlNodes21.Attributes.Append(xmlAttribute9);
                        }
                        else
                        {
                            xmlNodes20["specs"].RemoveChild(xmlNodes21);
                        }
                    }
                }
                else if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute10 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute10.Value = "False";
                    xmlNodes20.Attributes.Append(xmlAttribute10);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]/knowledgeskills").RemoveChild(xmlNodes20);
                }
        }

        private void ProcessSpells()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "spell.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootSpellFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"spell.xml\"]");
            if (xmlRootSpellFileNode == null)
            {
                xmlRootSpellFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "spell.xml";
                xmlRootSpellFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootSpellFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootSpellFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootSpellFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Spells

            XmlNode xmlSpellNodesParent = xmlRootSpellFileNode.SelectSingleNode("spells");
            if (xmlSpellNodesParent == null)
            {
                xmlSpellNodesParent = _objDataDoc.CreateElement("spells");
                xmlRootSpellFileNode.AppendChild(xmlSpellNodesParent);
            }

            XmlNode xmlDataSpellNodeList = xmlDataDocument.SelectSingleNode("/chummer/spells");
            if (xmlDataSpellNodeList != null)
            {
                foreach (XmlNode xmlDataSpellNode in xmlDataSpellNodeList.SelectNodes("spell"))
                {
                    string strDataSpellName = xmlDataSpellNode["name"].InnerText;
                    string strDataSpellId = xmlDataSpellNode["id"].InnerText;
                    XmlNode xmlSpellNode = xmlSpellNodesParent.SelectSingleNode("spell[name=\"" + strDataSpellName + "\"]");
                    if (xmlSpellNode != null)
                    {
                        if (xmlSpellNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpellId;
                            xmlSpellNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpellNode = _objDataDoc.CreateElement("spell");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpellId;
                        xmlSpellNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpellName;
                        xmlSpellNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpellName;
                        xmlSpellNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataSpellNode["page"].InnerText;
                        xmlSpellNode.AppendChild(xmlPageElement);

                        xmlSpellNodesParent.AppendChild(xmlSpellNode);
                    }
                }
            }
            foreach (XmlNode xmlSpellNode in xmlSpellNodesParent.SelectNodes("spell"))
            {
                xmlSpellNode.Attributes.RemoveAll();
                if (xmlDataSpellNodeList?.SelectSingleNode("spell[name = \"" + xmlSpellNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpellNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlSpellNodesParent.RemoveChild(xmlSpellNode);
                    }
                }
            }
        }

        private void ProcessStreams()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "streams.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootTraditionFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"streams.xml\"]");
            if (xmlRootTraditionFileNode == null)
            {
                xmlRootTraditionFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "streams.xml";
                xmlRootTraditionFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Streams

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = _objDataDoc.CreateElement("traditions");
                xmlRootTraditionFileNode.AppendChild(xmlTraditionNodesParent);
            }

            XmlNode xmlDataTraditionNodeList = xmlDataDocument.SelectSingleNode("/chummer/traditions");
            if (xmlDataTraditionNodeList != null)
            {
                foreach (XmlNode xmlDataTraditionNode in xmlDataTraditionNodeList.SelectNodes("tradition"))
                {
                    string strDataTraditionName = xmlDataTraditionNode["name"].InnerText;
                    string strDataTraditionId = xmlDataTraditionNode["id"].InnerText;
                    XmlNode xmlTraditionNode = xmlTraditionNodesParent.SelectSingleNode("tradition[name=\"" + strDataTraditionName + "\"]");
                    if (xmlTraditionNode != null)
                    {
                        if (xmlTraditionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTraditionNode = _objDataDoc.CreateElement("tradition");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTraditionId;
                        xmlTraditionNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataTraditionNode["page"].InnerText;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }
            foreach (XmlNode xmlTraditionNode in xmlTraditionNodesParent.SelectNodes("tradition"))
            {
                xmlTraditionNode.Attributes.RemoveAll();
                if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[name = \"" + xmlTraditionNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTraditionNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                    }
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = _objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XmlNode xmlDataSpiritNodeList = xmlDataDocument.SelectSingleNode("/chummer/spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XmlNode xmlDataSpiritNode in xmlDataSpiritNodeList.SelectNodes("spirit"))
                {
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[name=\"" + strDataSpiritName + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpiritNode = _objDataDoc.CreateElement("spirit");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpiritId;
                        xmlSpiritNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataSpiritNode["page"].InnerText;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }
            foreach (XmlNode xmlSpiritNode in xmlSpiritNodesParent.SelectNodes("spirit"))
            {
                xmlSpiritNode.Attributes.RemoveAll();
                if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[name = \"" + xmlSpiritNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpiritNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                    }
                }
            }
        }

        private void ProcessTraditions()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "traditions.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootTraditionFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"traditions.xml\"]");
            if (xmlRootTraditionFileNode == null)
            {
                xmlRootTraditionFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "traditions.xml";
                xmlRootTraditionFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Traditions

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = _objDataDoc.CreateElement("traditions");
                xmlRootTraditionFileNode.AppendChild(xmlTraditionNodesParent);
            }

            XmlNode xmlDataTraditionNodeList = xmlDataDocument.SelectSingleNode("/chummer/traditions");
            if (xmlDataTraditionNodeList != null)
            {
                foreach (XmlNode xmlDataTraditionNode in xmlDataTraditionNodeList.SelectNodes("tradition"))
                {
                    string strDataTraditionName = xmlDataTraditionNode["name"].InnerText;
                    string strDataTraditionId = xmlDataTraditionNode["id"].InnerText;
                    XmlNode xmlTraditionNode = xmlTraditionNodesParent.SelectSingleNode("tradition[name=\"" + strDataTraditionName + "\"]");
                    if (xmlTraditionNode != null)
                    {
                        if (xmlTraditionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTraditionNode = _objDataDoc.CreateElement("tradition");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTraditionId;
                        xmlTraditionNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataTraditionNode["page"].InnerText;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }
            foreach (XmlNode xmlTraditionNode in xmlTraditionNodesParent.SelectNodes("tradition"))
            {
                xmlTraditionNode.Attributes.RemoveAll();
                if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[name = \"" + xmlTraditionNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTraditionNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                    }
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = _objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XmlNode xmlDataSpiritNodeList = xmlDataDocument.SelectSingleNode("/chummer/spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XmlNode xmlDataSpiritNode in xmlDataSpiritNodeList.SelectNodes("spirit"))
                {
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[name=\"" + strDataSpiritName + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpiritNode = _objDataDoc.CreateElement("spirit");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpiritId;
                        xmlSpiritNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataSpiritNode["page"].InnerText;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }
            foreach (XmlNode xmlSpiritNode in xmlSpiritNodesParent.SelectNodes("spirit"))
            {
                xmlSpiritNode.Attributes.RemoveAll();
                if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[name = \"" + xmlSpiritNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpiritNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                    }
                }
            }

            // Process Drain Attributes

            XmlNode xmlDrainAttributeNodesParent = xmlRootTraditionFileNode.SelectSingleNode("drainattributes");
            if (xmlDrainAttributeNodesParent == null)
            {
                xmlDrainAttributeNodesParent = _objDataDoc.CreateElement("drainattributes");
                xmlRootTraditionFileNode.AppendChild(xmlDrainAttributeNodesParent);
            }

            XmlNode xmlDataDrainAttributeNodeList = xmlDataDocument.SelectSingleNode("/chummer/drainattributes");
            if (xmlDataDrainAttributeNodeList != null)
            {
                foreach (XmlNode xmlDataDrainAttributeNode in xmlDataDrainAttributeNodeList.SelectNodes("drainattribute"))
                {
                    string strDataDrainAttributeId = xmlDataDrainAttributeNode["id"].InnerText;
                    string strDataDrainAttributeName = xmlDataDrainAttributeNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlDrainAttributeNode = xmlDrainAttributeNodesParent.SelectSingleNode("drainattribute[name=\"" + strDataDrainAttributeName + "\"]");
                    if (xmlDrainAttributeNode != null)
                    {
                        if (xmlDrainAttributeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataDrainAttributeId;
                            xmlDrainAttributeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlDrainAttributeNode = _objDataDoc.CreateElement("drainattribute");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataDrainAttributeId;
                        xmlDrainAttributeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataDrainAttributeName;
                        xmlDrainAttributeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataDrainAttributeName;
                        xmlDrainAttributeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataDrainAttributeNode["page"].InnerText;
                        xmlDrainAttributeNode.AppendChild(xmlPageElement);

                        xmlDrainAttributeNodesParent.AppendChild(xmlDrainAttributeNode);
                    }
                }
            }
            foreach (XmlNode xmlDrainAttributeNode in xmlDrainAttributeNodesParent.SelectNodes("drainattribute"))
            {
                xmlDrainAttributeNode.Attributes.RemoveAll();
                if (xmlDataDrainAttributeNodeList?.SelectSingleNode("drainattribute[name = \"" + xmlDrainAttributeNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlDrainAttributeNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlDrainAttributeNodesParent.RemoveChild(xmlDrainAttributeNode);
                    }
                }
            }
        }

        private void ProcessVehicles()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(_strPath, "data", "vehicles.xml"));
            foreach (XmlNode xmlNodes in xmlDocument.SelectNodes("/chummer/categories/category"))
            {
                XmlDocument xmlDocument1 = _objDataDoc;
                if (xmlDocument1.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"vehicles.xml\"]/categories/category[text()=\"", xmlNodes.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes1 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/categories");
                if (xmlNodes1 == null)
                {
                    xmlNodes1 = _objDataDoc.CreateElement("categories");
                    XmlNode xmlNodes2 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
                    if (xmlNodes2 == null)
                    {
                        xmlNodes2 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes3 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                        xmlAttribute.Value = "vehicles.xml";
                        xmlNodes2.Attributes.Append(xmlAttribute);
                        xmlNodes3.AppendChild(xmlNodes2);
                    }
                    xmlNodes2.AppendChild(xmlNodes1);
                }
                XmlNode innerText1 = _objDataDoc.CreateElement("category");
                innerText1.InnerText = xmlNodes.InnerText;
                XmlAttribute xmlAttribute1 = _objDataDoc.CreateAttribute("translate");
                xmlAttribute1.Value = xmlNodes.InnerText;
                innerText1.Attributes.Append(xmlAttribute1);
                xmlNodes1.AppendChild(innerText1);
            }
            XmlNodeList xmlNodeLists = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"vehicles.xml\"]/categories/category");
            foreach (XmlNode xmlNodes4 in xmlNodeLists)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/categories/category[text() = \"", xmlNodes4.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/categories").RemoveChild(xmlNodes4);
            }
            foreach (XmlNode xmlNodes5 in xmlDocument.SelectNodes("/chummer/modcategories/category"))
            {
                XmlDocument xmlDocument2 = _objDataDoc;
                if (xmlDocument2.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"vehicles.xml\"]/modcategories/category[text()=\"", xmlNodes5.InnerText, "\"]")) != null)
                    continue;
                XmlNode xmlNodes6 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/modcategories");
                if (xmlNodes6 == null)
                {
                    xmlNodes6 = _objDataDoc.CreateElement("modcategories");
                    XmlNode xmlNodes7 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
                    if (xmlNodes7 == null)
                    {
                        xmlNodes7 = _objDataDoc.CreateElement("chummer");
                        XmlNode xmlNodes8 = _objDataDoc.SelectSingleNode("/chummer");
                        XmlAttribute xmlAttribute2 = _objDataDoc.CreateAttribute("file");
                        xmlAttribute2.Value = "vehicles.xml";
                        xmlNodes7.Attributes.Append(xmlAttribute2);
                        xmlNodes8.AppendChild(xmlNodes7);
                    }
                    xmlNodes7.AppendChild(xmlNodes6);
                }
                XmlNode innerText2 = _objDataDoc.CreateElement("category");
                innerText2.InnerText = xmlNodes5.InnerText;
                XmlAttribute innerText3 = _objDataDoc.CreateAttribute("translate");
                innerText3.Value = xmlNodes5.InnerText;
                innerText2.Attributes.Append(innerText3);
                xmlNodes6.AppendChild(innerText2);
            }
            XmlNodeList xmlNodeLists1 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"vehicles.xml\"]/modcategories/category");
            foreach (XmlNode xmlNodes9 in xmlNodeLists1)
            {
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/modcategories/category[text() = \"", xmlNodes9.InnerText, "\"]")) != null)
                    continue;
                _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/modcategories").RemoveChild(xmlNodes9);
            }
            try
            {
                XmlNode xmlNodes10 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
                XmlNode xmlNodes11 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/limits");
                xmlNodes10.RemoveChild(xmlNodes11);
            }
            catch
            {
            }
            foreach (XmlNode xmlNodes12 in xmlDocument.SelectNodes("/chummer/vehicles/vehicle"))
            {
                string str1 = string.Empty;
                string str2 = xmlNodes12["name"].InnerText;
                str1 = xmlNodes12["id"].InnerText;
                XmlDocument xmlDocument3 = _objDataDoc;
                XmlNode xmlNodes13 = xmlDocument3.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"vehicles.xml\"]/vehicles/vehicle[name=\"", str2, "\"]"));
                if (xmlNodes13 != null)
                {
                    if (xmlNodes13["id"] != null)
                        continue;
                    xmlNodes13.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes13["id"].InnerText = str1;
                }
                else
                {
                    XmlNode xmlNodes14 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/vehicles");
                    if (xmlNodes14 == null)
                    {
                        XmlNode xmlNodes15 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
                        if (xmlNodes15 == null)
                        {
                            xmlNodes15 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute3 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute3.Value = "vehicles.xml";
                            xmlNodes15.Attributes.Append(xmlAttribute3);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes15);
                        }
                        xmlNodes14 = _objDataDoc.CreateElement("vehicles");
                        xmlNodes15.AppendChild(xmlNodes14);
                    }
                    XmlNode innerText4 = _objDataDoc.CreateElement("vehicle");
                    innerText4.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText4["id"].InnerText = str1;
                    innerText4.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText4["name"].InnerText = xmlNodes12["name"].InnerText;
                    innerText4.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText4["translate"].InnerText = xmlNodes12["name"].InnerText;
                    innerText4.AppendChild(_objDataDoc.CreateElement("page"));
                    innerText4["page"].InnerText = str1;
                    xmlNodes14.AppendChild(innerText4);
                }
            }
            XmlNodeList xmlNodeLists2 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"vehicles.xml\"]/vehicles/vehicle");
            foreach (XmlNode xmlNodes16 in xmlNodeLists2)
            {
                xmlNodes16.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/vehicles/vehicle[name = \"", xmlNodes16["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute4 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute4.Value = "False";
                    xmlNodes16.Attributes.Append(xmlAttribute4);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/vehicles").RemoveChild(xmlNodes16);
                }
            }
            foreach (XmlNode xmlNodes17 in xmlDocument.SelectNodes("/chummer/mods/mod"))
            {
                string str3 = string.Empty;
                string str4 = xmlNodes17["name"].InnerText;
                str3 = xmlNodes17["id"].InnerText;
                XmlDocument xmlDocument4 = _objDataDoc;
                XmlNode xmlNodes18 = xmlDocument4.SelectSingleNode(string.Concat("/chummer/chummer[@file = \"vehicles.xml\"]/mods/mod[name=\"", str4, "\"]"));
                if (xmlNodes18 != null)
                {
                    if (xmlNodes18["id"] != null)
                        continue;
                    xmlNodes18.PrependChild(_objDataDoc.CreateElement("id"));
                    xmlNodes18["id"].InnerText = str3;
                }
                else
                {
                    XmlNode xmlNodes19 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/mods");
                    if (xmlNodes19 == null)
                    {
                        XmlNode xmlNodes20 = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
                        if (xmlNodes20 == null)
                        {
                            xmlNodes20 = _objDataDoc.CreateElement("chummer");
                            XmlAttribute xmlAttribute5 = _objDataDoc.CreateAttribute("file");
                            xmlAttribute5.Value = "vehicles.xml";
                            xmlNodes20.Attributes.Append(xmlAttribute5);
                            _objDataDoc.SelectSingleNode("/chummer").AppendChild(xmlNodes20);
                        }
                        xmlNodes19 = _objDataDoc.CreateElement("mods");
                        xmlNodes20.AppendChild(xmlNodes19);
                    }
                    XmlNode innerText5 = _objDataDoc.CreateElement("mod");
                    innerText5.AppendChild(_objDataDoc.CreateElement("id"));
                    innerText5["id"].InnerText = str3;
                    innerText5.AppendChild(_objDataDoc.CreateElement("name"));
                    innerText5["name"].InnerText = xmlNodes17["name"].InnerText;
                    innerText5.AppendChild(_objDataDoc.CreateElement("translate"));
                    innerText5["translate"].InnerText = xmlNodes17["name"].InnerText;
                    innerText5.AppendChild(_objDataDoc.CreateElement("page"));
                    innerText5["page"].InnerText = str3;
                    xmlNodes19.AppendChild(innerText5);
                }
            }
            XmlNodeList xmlNodeLists3 = _objDataDoc.SelectNodes("/chummer/chummer[@file = \"vehicles.xml\"]/mods/mod");
            foreach (XmlNode xmlNodes21 in xmlNodeLists3)
            {
                xmlNodes21.Attributes.RemoveAll();
                if (xmlDocument.SelectSingleNode(string.Concat("/chummer/mods/mod[name = \"", xmlNodes21["name"].InnerText, "\"]")) != null)
                    continue;
                if (!_blnDelete)
                {
                    XmlAttribute xmlAttribute6 = _objDataDoc.CreateAttribute("exists");
                    xmlAttribute6.Value = "False";
                    xmlNodes21.Attributes.Append(xmlAttribute6);
                }
                else
                {
                    _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]/mods").RemoveChild(xmlNodes21);
                }
            }
        }

        private void ProcessWeapons()
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(_strPath, "data", "weapons.xml"));

            XmlNode xmlRootNode = _objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = _objDataDoc.CreateElement("chummer");
                _objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootWeaponFileNode = _objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"weapons.xml\"]");
            if (xmlRootWeaponFileNode == null)
            {
                xmlRootWeaponFileNode = _objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = _objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "weapons.xml";
                xmlRootWeaponFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootWeaponFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = _objDataDoc.CreateElement("categories");
                xmlRootWeaponFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = _objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = _objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlCategoryNode in xmlCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Weapons

            XmlNode xmlWeaponNodesParent = xmlRootWeaponFileNode.SelectSingleNode("weapons");
            if (xmlWeaponNodesParent == null)
            {
                xmlWeaponNodesParent = _objDataDoc.CreateElement("weapons");
                xmlRootWeaponFileNode.AppendChild(xmlWeaponNodesParent);
            }

            XmlNode xmlDataWeaponNodeList = xmlDataDocument.SelectSingleNode("/chummer/weapons");
            if (xmlDataWeaponNodeList != null)
            {
                foreach (XmlNode xmlDataWeaponNode in xmlDataWeaponNodeList.SelectNodes("weapon"))
                {
                    string strDataWeaponName = xmlDataWeaponNode["name"].InnerText;
                    string strDataWeaponId = xmlDataWeaponNode["id"].InnerText;
                    XmlNode xmlWeaponNode = xmlWeaponNodesParent.SelectSingleNode("weapon[name=\"" + strDataWeaponName + "\"]");
                    if (xmlWeaponNode != null)
                    {
                        if (xmlWeaponNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponId;
                            xmlWeaponNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlWeaponNode = _objDataDoc.CreateElement("weapon");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataWeaponId;
                        xmlWeaponNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataWeaponName;
                        xmlWeaponNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataWeaponName;
                        xmlWeaponNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataWeaponNode["page"].InnerText;
                        xmlWeaponNode.AppendChild(xmlPageElement);

                        xmlWeaponNodesParent.AppendChild(xmlWeaponNode);
                    }
                }
            }
            foreach (XmlNode xmlWeaponNode in xmlWeaponNodesParent.SelectNodes("weapon"))
            {
                xmlWeaponNode.Attributes.RemoveAll();
                if (xmlDataWeaponNodeList?.SelectSingleNode("weapon[name = \"" + xmlWeaponNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlWeaponNodesParent.RemoveChild(xmlWeaponNode);
                    }
                }
            }

            // Process Weapon Mods

            XmlNode xmlAccessoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("accessories");
            if (xmlAccessoryNodesParent == null)
            {
                xmlAccessoryNodesParent = _objDataDoc.CreateElement("accessories");
                xmlRootWeaponFileNode.AppendChild(xmlAccessoryNodesParent);
            }

            XmlNode xmlDataAccessoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/accessories");
            if (xmlDataAccessoryNodeList != null)
            {
                foreach (XmlNode xmlDataAccessoryNode in xmlDataAccessoryNodeList.SelectNodes("accessory"))
                {
                    string strDataAccessoryId = xmlDataAccessoryNode["id"].InnerText;
                    string strDataAccessoryName = xmlDataAccessoryNode["name"].InnerText;
                    XmlDocument xmlDocument3 = _objDataDoc;
                    XmlNode xmlAccessoryNode = xmlAccessoryNodesParent.SelectSingleNode("accessory[name=\"" + strDataAccessoryName + "\"]");
                    if (xmlAccessoryNode != null)
                    {
                        if (xmlAccessoryNode["id"] == null)
                        {
                            XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataAccessoryId;
                            xmlAccessoryNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlAccessoryNode = _objDataDoc.CreateElement("accessory");

                        XmlNode xmlIdElement = _objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataAccessoryId;
                        xmlAccessoryNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = _objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataAccessoryName;
                        xmlAccessoryNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = _objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataAccessoryName;
                        xmlAccessoryNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = _objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataAccessoryNode["page"].InnerText;
                        xmlAccessoryNode.AppendChild(xmlPageElement);

                        xmlAccessoryNodesParent.AppendChild(xmlAccessoryNode);
                    }
                }
            }
            foreach (XmlNode xmlAccessoryNode in xmlAccessoryNodesParent.SelectNodes("accessory"))
            {
                xmlAccessoryNode.Attributes.RemoveAll();
                if (xmlDataAccessoryNodeList?.SelectSingleNode("accessory[name = \"" + xmlAccessoryNode["name"]?.InnerText + "\"]") == null)
                {
                    if (!_blnDelete)
                    {
                        XmlAttribute xmlExistsAttribute = _objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlAccessoryNode.Attributes.Append(xmlExistsAttribute);
                    }
                    else
                    {
                        xmlAccessoryNodesParent.RemoveChild(xmlAccessoryNode);
                    }
                }
            }
        }
        #endregion Methods

        #region Helpers
        private static readonly TextInfo _objEnUSTextInfo = (new CultureInfo("en-US", false)).TextInfo;
        private static string ToTitle(string stringIn)
        {
            return _objEnUSTextInfo.ToTitleCase(stringIn);
        }
        #endregion Helpers
    }
}
