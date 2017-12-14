#define DELETE

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Translator
{
    public partial class frmMain
    {
        private static readonly TextInfo _objEnUSTextInfo = (new CultureInfo("en-US", false)).TextInfo;
        private static readonly string PATH = Application.StartupPath;

        public frmMain()
        {
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
            string str = _objEnUSTextInfo.ToTitleCase(txtLanguageName.Text) + " (" + lower.ToUpper() + ")";

            XmlDocument objDataDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = objDataDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            objDataDoc.AppendChild(xmlDeclaration);
            XmlNode xmlRootChummerNode = objDataDoc.CreateElement("chummer");
            XmlNode xmlVersionNode = objDataDoc.CreateElement("version");
            xmlVersionNode.InnerText = "-500";
            xmlRootChummerNode.AppendChild(xmlVersionNode);
            objDataDoc.AppendChild(xmlRootChummerNode);

            ProcessArmor(objDataDoc);
            ProcessBioware(objDataDoc);
            ProcessBooks(objDataDoc);
            ProcessComplexForms(objDataDoc);
            ProcessCritterPowers(objDataDoc);
            ProcessCritters(objDataDoc);
            ProcessCyberware(objDataDoc);
            ProcessEchoes(objDataDoc);
            ProcessGear(objDataDoc);
            ProcessImprovements(objDataDoc);
            ProcessLicenses(objDataDoc);
            ProcessLifestyles(objDataDoc);
            ProcessMartialArts(objDataDoc);
            ProcessMentors(objDataDoc);
            ProcessMetamagic(objDataDoc);
            ProcessMetatypes(objDataDoc);
            ProcessPowers(objDataDoc);
            ProcessPriorities(objDataDoc);
            ProcessPrograms(objDataDoc);
            ProcessQualities(objDataDoc);
            ProcessSkills(objDataDoc);
            ProcessSpells(objDataDoc);
            ProcessStreams(objDataDoc);
            ProcessTraditions(objDataDoc);
            ProcessVehicles(objDataDoc);
            ProcessWeapons(objDataDoc);
            objDataDoc.Save(Path.Combine(PATH, "lang", lower + "_data.xml"));

            XmlDocument objDoc = new XmlDocument();
            xmlDeclaration = objDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            objDoc.AppendChild(xmlDeclaration);
            xmlRootChummerNode = objDoc.CreateElement("chummer");
            xmlVersionNode = objDoc.CreateElement("version");
            xmlVersionNode.InnerText = "-500";
            XmlNode xmlNameNode = objDoc.CreateElement("name");
            xmlNameNode.InnerText = str;
            xmlRootChummerNode.AppendChild(xmlVersionNode);
            xmlRootChummerNode.AppendChild(xmlNameNode);
            objDoc.AppendChild(xmlRootChummerNode);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(PATH, "lang", "en-US.xml"));
            XmlNode xmlStringsNode = xmlDocument.SelectSingleNode("/chummer/strings");
            if (xmlStringsNode != null)
            {
                xmlRootChummerNode.AppendChild(objDoc.ImportNode(xmlStringsNode, true));
            }
            objDoc.Save(Path.Combine(PATH, "lang", lower + ".xml"));

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
            foreach (string str in Directory.EnumerateFiles(Path.Combine(PATH, "lang"), "*.xml"))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(str);
                string strInnerText = xmlDocument.SelectSingleNode("/chummer/name")?.InnerText;
                if (!string.IsNullOrEmpty(strInnerText))
                    cboLanguages.Items.Add(strInnerText);
            }
        }
        #endregion Methods

        #region Process Methods
        private static void ProcessArmor(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "armor.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootArmorFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"armor.xml\"]");
            if (xmlRootArmorFileNode == null)
            {
                xmlRootArmorFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "armor.xml";
                xmlRootArmorFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootArmorFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootArmorFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootArmorFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlArmorNodesParent = objDataDoc.CreateElement("armors");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorId;
                            xmlArmorNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlArmorNode = objDataDoc.CreateElement("armor");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataArmorId;
                        xmlArmorNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataArmorName;
                        xmlArmorNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataArmorName;
                        xmlArmorNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorNode.Attributes.Append(xmlExistsAttribute);
                    }
                    #else
                    {
                        xmlArmorNodesParent.RemoveChild(xmlArmorNode);
                    }
                    #endif
                }
            }

            // Process Armor Mods

            XmlNode xmlArmorModNodesParent = xmlRootArmorFileNode.SelectSingleNode("mods");
            if (xmlArmorModNodesParent == null)
            {
                xmlArmorModNodesParent = objDataDoc.CreateElement("mods");
                xmlRootArmorFileNode.AppendChild(xmlArmorModNodesParent);
            }

            XmlNode xmlDataArmorModNodeList = xmlDataDocument.SelectSingleNode("/chummer/mods");
            if (xmlDataArmorModNodeList != null)
            {
                foreach (XmlNode xmlDataArmorModNode in xmlDataArmorModNodeList.SelectNodes("mod"))
                {
                    string strDataArmorModId = xmlDataArmorModNode["id"].InnerText;
                    string strDataArmorModName = xmlDataArmorModNode["name"].InnerText;
                    XmlNode xmlArmorModNode = xmlArmorModNodesParent.SelectSingleNode("mod[name=\"" + strDataArmorModName + "\"]");
                    if (xmlArmorModNode != null)
                    {
                        if (xmlArmorModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorModId;
                            xmlArmorModNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlArmorModNode = objDataDoc.CreateElement("mod");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataArmorModId;
                        xmlArmorModNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataArmorModName;
                        xmlArmorModNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataArmorModName;
                        xmlArmorModNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
                    #if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorModNode.Attributes.Append(xmlExistsAttribute);
                    }
                    #else
                    {
                        xmlArmorModNodesParent.RemoveChild(xmlArmorModNode);
                    }
                    #endif
                }
            }
        }

        private static void ProcessBioware(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "bioware.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootBiowareFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"bioware.xml\"]");
            if (xmlRootBiowareFileNode == null)
            {
                xmlRootBiowareFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "bioware.xml";
                xmlRootBiowareFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBiowareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootBiowareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootBiowareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlBiowareNodesParent = objDataDoc.CreateElement("biowares");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBiowareId;
                            xmlBiowareNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlBiowareNode = objDataDoc.CreateElement("bioware");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataBiowareId;
                        xmlBiowareNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataBiowareName;
                        xmlBiowareNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataBiowareName;
                        xmlBiowareNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlBiowareNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlBiowareNodesParent.RemoveChild(xmlBiowareNode);
                    }
#endif
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootBiowareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = objDataDoc.CreateElement("grades");
                xmlRootBiowareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XmlNode xmlDataGradeNodeList = xmlDataDocument.SelectSingleNode("/chummer/grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XmlNode xmlDataGradeNode in xmlDataGradeNodeList.SelectNodes("grade"))
                {
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[name=\"" + strDataGradeName + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGradeNode = objDataDoc.CreateElement("grade");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGradeId;
                        xmlGradeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                    }
#endif
                }
            }
        }

        private static void ProcessBooks(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "books.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootBooksFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"books.xml\"]");
            if (xmlRootBooksFileNode == null)
            {
                xmlRootBooksFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "books.xml";
                xmlRootBooksFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBooksFileNode);
            }

            XmlNode xmlBookNodesParent = xmlRootBooksFileNode.SelectSingleNode("books");
            if (xmlBookNodesParent == null)
            {
                xmlBookNodesParent = objDataDoc.CreateElement("books");
                xmlRootBooksFileNode.AppendChild(xmlBookNodesParent);
            }

            XmlNode xmlDataBookNodeList = xmlDataDocument.SelectSingleNode("/chummer/books");
            if (xmlDataBookNodeList != null)
            {
                foreach (XmlNode xmlDataBookNode in xmlDataBookNodeList.SelectNodes("book"))
                {
                    string strDataBookId = xmlDataBookNode["id"].InnerText;
                    string strDataBookName = xmlDataBookNode["name"].InnerText;
                    XmlNode xmlBookNode = xmlBookNodesParent.SelectSingleNode("book[name=\"" + strDataBookName + "\"]");
                    if (xmlBookNode != null)
                    {
                        if (xmlBookNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBookId;
                            xmlBookNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlBookNode = objDataDoc.CreateElement("book");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataBookId;
                        xmlBookNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataBookName;
                        xmlBookNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataBookName;
                        xmlBookNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlBookNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlBookNodesParent.RemoveChild(xmlBookNode);
                    }
#endif
                }
            }
        }

        private static void ProcessComplexForms(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "complexforms.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootComplexFormsFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"complexforms.xml\"]");
            if (xmlRootComplexFormsFileNode == null)
            {
                xmlRootComplexFormsFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "complexforms.xml";
                xmlRootComplexFormsFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootComplexFormsFileNode);
            }

            XmlNode xmlComplexFormNodesParent = xmlRootComplexFormsFileNode.SelectSingleNode("complexforms");
            if (xmlComplexFormNodesParent == null)
            {
                xmlComplexFormNodesParent = objDataDoc.CreateElement("complexforms");
                xmlRootComplexFormsFileNode.AppendChild(xmlComplexFormNodesParent);
            }

            XmlNode xmlDataComplexFormNodeList = xmlDataDocument.SelectSingleNode("/chummer/complexforms");
            if (xmlDataComplexFormNodeList != null)
            {
                foreach (XmlNode xmlDataComplexFormNode in xmlDataComplexFormNodeList.SelectNodes("complexform"))
                {
                    string strDataComplexFormId = xmlDataComplexFormNode["id"].InnerText;
                    string strDataComplexFormName = xmlDataComplexFormNode["name"].InnerText;
                    XmlNode xmlComplexFormNode = xmlComplexFormNodesParent.SelectSingleNode("complexform[name=\"" + strDataComplexFormName + "\"]");
                    if (xmlComplexFormNode != null)
                    {
                        if (xmlComplexFormNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataComplexFormId;
                            xmlComplexFormNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlComplexFormNode = objDataDoc.CreateElement("complexform");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataComplexFormId;
                        xmlComplexFormNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataComplexFormName;
                        xmlComplexFormNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataComplexFormName;
                        xmlComplexFormNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlComplexFormNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlComplexFormNodesParent.RemoveChild(xmlComplexFormNode);
                    }
#endif
                }
            }
        }

        private static void ProcessCritterPowers(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "critterpowers.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPowerFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"critterpowers.xml\"]");
            if (xmlRootPowerFileNode == null)
            {
                xmlRootPowerFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "critterpowers.xml";
                xmlRootPowerFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootPowerFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootPowerFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlPowerNodesParent = objDataDoc.CreateElement("powers");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlPowerNode = objDataDoc.CreateElement("power");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataPowerId;
                        xmlPowerNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPowerNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                    }
#endif
                }
            }
        }

        private static void ProcessCritters(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "critters.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetatypeFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"critters.xml\"]");
            if (xmlRootMetatypeFileNode == null)
            {
                xmlRootMetatypeFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "critters.xml";
                xmlRootMetatypeFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlMetatypeNodesParent = objDataDoc.CreateElement("metatypes");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMetatypeNode = objDataDoc.CreateElement("metatype");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMetatypeId;
                        xmlMetatypeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMetatypeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                    }
#endif
                }
            }
        }

        private static void ProcessCyberware(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "cyberware.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootCyberwareFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"cyberware.xml\"]");
            if (xmlRootCyberwareFileNode == null)
            {
                xmlRootCyberwareFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "cyberware.xml";
                xmlRootCyberwareFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootCyberwareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootCyberwareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlCyberwareNodesParent = objDataDoc.CreateElement("cyberwares");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataCyberwareId;
                            xmlCyberwareNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlCyberwareNode = objDataDoc.CreateElement("cyberware");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataCyberwareId;
                        xmlCyberwareNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataCyberwareName;
                        xmlCyberwareNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataCyberwareName;
                        xmlCyberwareNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlCyberwareNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlCyberwareNodesParent.RemoveChild(xmlCyberwareNode);
                    }
#endif
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = objDataDoc.CreateElement("grades");
                xmlRootCyberwareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XmlNode xmlDataGradeNodeList = xmlDataDocument.SelectSingleNode("/chummer/grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XmlNode xmlDataGradeNode in xmlDataGradeNodeList.SelectNodes("grade"))
                {
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[name=\"" + strDataGradeName + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGradeNode = objDataDoc.CreateElement("grade");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGradeId;
                        xmlGradeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGradeName;
                        xmlGradeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                    }
#endif
                }
            }

            // Remove Cybersuites

            XmlNode xmlRemoveNode = xmlRootCyberwareFileNode.SelectSingleNode("suites");
            if (xmlRemoveNode != null)
            {
                xmlRootCyberwareFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessEchoes(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "echoes.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootEchoesFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"echoes.xml\"]");
            if (xmlRootEchoesFileNode == null)
            {
                xmlRootEchoesFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "echoes.xml";
                xmlRootEchoesFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootEchoesFileNode);
            }

            XmlNode xmlEchoNodesParent = xmlRootEchoesFileNode.SelectSingleNode("echoes");
            if (xmlEchoNodesParent == null)
            {
                xmlEchoNodesParent = objDataDoc.CreateElement("echoes");
                xmlRootEchoesFileNode.AppendChild(xmlEchoNodesParent);
            }

            XmlNode xmlDataEchoNodeList = xmlDataDocument.SelectSingleNode("/chummer/echoes");
            if (xmlDataEchoNodeList != null)
            {
                foreach (XmlNode xmlDataEchoNode in xmlDataEchoNodeList.SelectNodes("echo"))
                {
                    string strDataEchoId = xmlDataEchoNode["id"].InnerText;
                    string strDataEchoName = xmlDataEchoNode["name"].InnerText;
                    XmlNode xmlEchoNode = xmlEchoNodesParent.SelectSingleNode("echo[name=\"" + strDataEchoName + "\"]");
                    if (xmlEchoNode != null)
                    {
                        if (xmlEchoNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEchoId;
                            xmlEchoNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlEchoNode = objDataDoc.CreateElement("echo");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataEchoId;
                        xmlEchoNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataEchoName;
                        xmlEchoNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataEchoName;
                        xmlEchoNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlEchoNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlEchoNodesParent.RemoveChild(xmlEchoNode);
                    }
#endif
                }
            }
        }

        private static void ProcessGear(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "gears.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootGearFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"gears.xml\"]");
            if (xmlRootGearFileNode == null)
            {
                xmlRootGearFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "gears.xml";
                xmlRootGearFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootGearFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootGearFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootGearFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlGearNodesParent = objDataDoc.CreateElement("gears");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGearId;
                            xmlGearNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGearNode = objDataDoc.CreateElement("gear");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGearId;
                        xmlGearNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGearName;
                        xmlGearNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGearName;
                        xmlGearNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGearNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlGearNodesParent.RemoveChild(xmlGearNode);
                    }
#endif
                }
            }
        }

        private static void ProcessImprovements(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "improvements.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootImprovementsFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"improvements.xml\"]");
            if (xmlRootImprovementsFileNode == null)
            {
                xmlRootImprovementsFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "improvements.xml";
                xmlRootImprovementsFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootImprovementsFileNode);
            }

            XmlNode xmlImprovementNodesParent = xmlRootImprovementsFileNode.SelectSingleNode("improvements");
            if (xmlImprovementNodesParent == null)
            {
                xmlImprovementNodesParent = objDataDoc.CreateElement("improvements");
                xmlRootImprovementsFileNode.AppendChild(xmlImprovementNodesParent);
            }

            XmlNode xmlDataImprovementNodeList = xmlDataDocument.SelectSingleNode("/chummer/improvements");
            if (xmlDataImprovementNodeList != null)
            {
                foreach (XmlNode xmlDataImprovementNode in xmlDataImprovementNodeList.SelectNodes("improvement"))
                {
                    string strDataImprovementId = xmlDataImprovementNode["id"].InnerText;
                    string strDataImprovementName = xmlDataImprovementNode["name"].InnerText;
                    XmlNode xmlImprovementNode = xmlImprovementNodesParent.SelectSingleNode("improvement[name=\"" + strDataImprovementName + "\"]");
                    if (xmlImprovementNode != null)
                    {
                        if (xmlImprovementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataImprovementId;
                            xmlImprovementNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlImprovementNode = objDataDoc.CreateElement("improvement");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataImprovementId;
                        xmlImprovementNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataImprovementName;
                        xmlImprovementNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataImprovementName;
                        xmlImprovementNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlImprovementNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlImprovementNodesParent.RemoveChild(xmlImprovementNode);
                    }
#endif
                }
            }
        }

        private static void ProcessLicenses(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "licenses.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootLicenseFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"licenses.xml\"]");
            if (xmlRootLicenseFileNode == null)
            {
                xmlRootLicenseFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "licenses.xml";
                xmlRootLicenseFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLicenseFileNode);
            }

            // Process Licenses

            XmlNode xmlLicenseNodesParent = xmlRootLicenseFileNode.SelectSingleNode("licenses");

            if (xmlLicenseNodesParent == null)
            {
                xmlLicenseNodesParent = objDataDoc.CreateElement("licenses");
                xmlRootLicenseFileNode.AppendChild(xmlLicenseNodesParent);
            }

            XmlNode xmlDataLicenseNodeList = xmlDataDocument.SelectSingleNode("/chummer/licenses");
            if (xmlDataLicenseNodeList != null)
            {
                foreach (XmlNode xmlDataLicenseNode in xmlDataLicenseNodeList.SelectNodes("license"))
                {
                    if (xmlLicenseNodesParent.SelectSingleNode("license[text()=\"" + xmlDataLicenseNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlLicenseNode = objDataDoc.CreateElement("license");
                        xmlLicenseNode.InnerText = xmlDataLicenseNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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

        private static void ProcessLifestyles(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "lifestyles.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootLifestyleFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"lifestyles.xml\"]");
            if (xmlRootLifestyleFileNode == null)
            {
                xmlRootLifestyleFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "lifestyles.xml";
                xmlRootLifestyleFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLifestyleFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootLifestyleFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlLifestyleNodesParent = objDataDoc.CreateElement("lifestyles");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataLifestyleId;
                            xmlLifestyleNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlLifestyleNode = objDataDoc.CreateElement("lifestyle");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataLifestyleId;
                        xmlLifestyleNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataLifestyleName;
                        xmlLifestyleNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataLifestyleName;
                        xmlLifestyleNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlLifestyleNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlLifestyleNodesParent.RemoveChild(xmlLifestyleNode);
                    }
#endif
                }
            }

            // Process Lifestyle Qualities

            XmlNode xmlQualityNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("qualities");
            if (xmlQualityNodesParent == null)
            {
                xmlQualityNodesParent = objDataDoc.CreateElement("qualities");
                xmlRootLifestyleFileNode.AppendChild(xmlQualityNodesParent);
            }

            XmlNode xmlDataQualityNodeList = xmlDataDocument.SelectSingleNode("/chummer/qualities");
            if (xmlDataQualityNodeList != null)
            {
                foreach (XmlNode xmlDataQualityNode in xmlDataQualityNodeList.SelectNodes("quality"))
                {
                    string strDataQualityId = xmlDataQualityNode["id"].InnerText;
                    string strDataQualityName = xmlDataQualityNode["name"].InnerText;
                    XmlNode xmlQualityNode = xmlQualityNodesParent.SelectSingleNode("quality[name=\"" + strDataQualityName + "\"]");
                    if (xmlQualityNode != null)
                    {
                        if (xmlQualityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataQualityId;
                            xmlQualityNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlQualityNode = objDataDoc.CreateElement("quality");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataQualityId;
                        xmlQualityNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlQualityNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlQualityNodesParent.RemoveChild(xmlQualityNode);
                    }
#endif
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

        private static void ProcessMartialArts(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "martialarts.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMartialArtFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"martialarts.xml\"]");
            if (xmlRootMartialArtFileNode == null)
            {
                xmlRootMartialArtFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "martialarts.xml";
                xmlRootMartialArtFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMartialArtFileNode);
            }

            // Process Martial Arts

            XmlNode xmlMartialArtNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("martialarts");
            if (xmlMartialArtNodesParent == null)
            {
                xmlMartialArtNodesParent = objDataDoc.CreateElement("martialarts");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMartialArtId;
                            xmlMartialArtNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMartialArtNode = objDataDoc.CreateElement("martialart");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMartialArtId;
                        xmlMartialArtNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMartialArtName;
                        xmlMartialArtNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMartialArtName;
                        xmlMartialArtNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMartialArtNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlMartialArtNodesParent.RemoveChild(xmlMartialArtNode);
                    }
#endif
                }
            }

            // Process Techniques

            XmlNode xmlTechniqueNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("techniques");
            if (xmlTechniqueNodesParent == null)
            {
                xmlTechniqueNodesParent = objDataDoc.CreateElement("techniques");
                xmlRootMartialArtFileNode.AppendChild(xmlTechniqueNodesParent);
            }

            XmlNode xmlDataTechniqueNodeList = xmlDataDocument.SelectSingleNode("/chummer/techniques");
            if (xmlDataTechniqueNodeList != null)
            {
                foreach (XmlNode xmlDataTechniqueNode in xmlDataTechniqueNodeList.SelectNodes("technique"))
                {
                    string strDataTechniqueId = xmlDataTechniqueNode["id"].InnerText;
                    string strDataTechniqueName = xmlDataTechniqueNode["name"].InnerText;
                    XmlNode xmlTechniqueNode = xmlTechniqueNodesParent.SelectSingleNode("technique[name=\"" + strDataTechniqueName + "\"]");
                    if (xmlTechniqueNode != null)
                    {
                        if (xmlTechniqueNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTechniqueId;
                            xmlTechniqueNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTechniqueNode = objDataDoc.CreateElement("technique");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTechniqueId;
                        xmlTechniqueNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTechniqueName;
                        xmlTechniqueNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTechniqueName;
                        xmlTechniqueNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTechniqueNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlTechniqueNodesParent.RemoveChild(xmlTechniqueNode);
                    }
#endif
                }
            }

            // Remove Maneuvers

            XmlNode xmlRemoveNode = xmlRootMartialArtFileNode.SelectSingleNode("maneuvers");
            if (xmlRemoveNode != null)
            {
                xmlRootMartialArtFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessMentors(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "mentors.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMentorFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"mentors.xml\"]");
            if (xmlRootMentorFileNode == null)
            {
                xmlRootMentorFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "mentors.xml";
                xmlRootMentorFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMentorFileNode);
            }

            // Process Mentors

            XmlNode xmlMentorNodesParent = xmlRootMentorFileNode.SelectSingleNode("mentors");
            if (xmlMentorNodesParent == null)
            {
                xmlMentorNodesParent = objDataDoc.CreateElement("mentors");
                xmlRootMentorFileNode.AppendChild(xmlMentorNodesParent);
            }

            XmlNode xmlDataMentorNodeList = xmlDataDocument.SelectSingleNode("/chummer/mentors");
            if (xmlDataMentorNodeList != null)
            {
                foreach (XmlNode xmlDataMentorNode in xmlDataMentorNodeList.SelectNodes("mentor"))
                {
                    string strDataMentorName = xmlDataMentorNode["name"].InnerText;
                    string strDataMentorId = xmlDataMentorNode["id"].InnerText;
                    string strDataMentorAdvantage = xmlDataMentorNode["advantage"].InnerText;
                    string strDataMentorDisadvantage = xmlDataMentorNode["disadvantage"].InnerText;
                    XmlNode xmlMentorNode = xmlRootMentorFileNode.SelectSingleNode("mentors/mentor[name=\"" + strDataMentorName + "\"]");
                    if (xmlMentorNode != null)
                    {
                        if (xmlMentorNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMentorId;
                            xmlMentorNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMentorNode["advantage"] == null)
                        {
                            XmlNode xmlAdvantageElement = objDataDoc.CreateElement("advantage");
                            xmlAdvantageElement.InnerText = strDataMentorAdvantage;
                            xmlMentorNode.AppendChild(xmlAdvantageElement);
                        }

                        if (xmlMentorNode["disadvantage"] == null)
                        {
                            XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("disadvantage");
                            xmlDisadvantageElement.InnerText = strDataMentorDisadvantage;
                            xmlMentorNode.AppendChild(xmlDisadvantageElement);
                        }
                    }
                    else
                    {
                        xmlMentorNode = objDataDoc.CreateElement("mentor");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMentorId;
                        xmlMentorNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMentorName;
                        xmlMentorNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMentorName;
                        xmlMentorNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlAdvantageElement = objDataDoc.CreateElement("advantage");
                        xmlAdvantageElement.InnerText = strDataMentorAdvantage;
                        xmlMentorNode.AppendChild(xmlAdvantageElement);

                        XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("disadvantage");
                        xmlDisadvantageElement.InnerText = strDataMentorDisadvantage;
                        xmlMentorNode.AppendChild(xmlDisadvantageElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataMentorNode["page"].InnerText;
                        xmlMentorNode.AppendChild(xmlPageElement);

                        xmlMentorNodesParent.AppendChild(xmlMentorNode);
                    }

                    XmlNode xmlDataMentorChoicesNode = xmlDataMentorNode["choices"];
                    if (xmlDataMentorChoicesNode != null)
                    {
                        XmlNode xmlMentorChoicesNode = xmlMentorNode["choices"];
                        if (xmlMentorChoicesNode == null)
                        {
                            xmlMentorChoicesNode = objDataDoc.CreateElement("choices");
                            xmlMentorNode.AppendChild(xmlMentorChoicesNode);
                        }

                        foreach (XmlNode xmlDataChoiceNode in xmlDataMentorChoicesNode.SelectNodes("choice"))
                        {
                            string strDataChoiceName = xmlDataChoiceNode["name"]?.InnerText ?? string.Empty;
                            XmlNode xmlChoiceNode = xmlMentorChoicesNode.SelectSingleNode("choice[name=\"" + strDataChoiceName + "\"]");
                            if (xmlChoiceNode == null)
                            {
                                xmlChoiceNode = objDataDoc.CreateElement("choice");

                                XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                                xmlNameElement.InnerText = strDataChoiceName;
                                xmlChoiceNode.AppendChild(xmlNameElement);

                                XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                                xmlTranslateElement.InnerText = strDataChoiceName;
                                xmlChoiceNode.AppendChild(xmlTranslateElement);

                                foreach (XmlAttribute xmlDataChoiceNodeAttribute in xmlDataChoiceNode.Attributes)
                                {
                                    XmlAttribute xmlChoiceNodeAttribute = objDataDoc.CreateAttribute(xmlDataChoiceNodeAttribute.Name);
                                    xmlChoiceNodeAttribute.Value = xmlDataChoiceNodeAttribute.InnerText;
                                    xmlChoiceNode.Attributes.Append(xmlChoiceNodeAttribute);
                                }

                                xmlMentorChoicesNode.AppendChild(xmlChoiceNode);
                            }
                        }
                    }
                }
            }
            foreach (XmlNode xmlMentorNode in xmlMentorNodesParent.SelectNodes("mentor"))
            {
                xmlMentorNode.Attributes.RemoveAll();
                if (xmlDataMentorNodeList?.SelectSingleNode("mentor[name = \"" + xmlMentorNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMentorNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlMentorNodesParent.RemoveChild(xmlMentorNode);
                    }
#endif
                }
            }
        }

        private static void ProcessMetamagic(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "metamagics.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetamagicFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagics.xml\"]");
            if (xmlRootMetamagicFileNode == null)
            {
                xmlRootMetamagicFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "metamagics.xml";
                xmlRootMetamagicFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetamagicFileNode);
            }

            // Process Streams

            XmlNode xmlMetamagicNodesParent = xmlRootMetamagicFileNode.SelectSingleNode("metamagics");
            if (xmlMetamagicNodesParent == null)
            {
                xmlMetamagicNodesParent = objDataDoc.CreateElement("metamagics");
                xmlRootMetamagicFileNode.AppendChild(xmlMetamagicNodesParent);
            }

            XmlNode xmlDataMetamagicNodeList = xmlDataDocument.SelectSingleNode("/chummer/metamagics");
            if (xmlDataMetamagicNodeList != null)
            {
                foreach (XmlNode xmlDataMetamagicNode in xmlDataMetamagicNodeList.SelectNodes("metamagic"))
                {
                    string strDataMetamagicName = xmlDataMetamagicNode["name"].InnerText;
                    string strDataMetamagicId = xmlDataMetamagicNode["id"].InnerText;
                    XmlNode xmlMetamagicNode = xmlMetamagicNodesParent.SelectSingleNode("metamagic[name=\"" + strDataMetamagicName + "\"]");
                    if (xmlMetamagicNode != null)
                    {
                        if (xmlMetamagicNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetamagicId;
                            xmlMetamagicNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMetamagicNode = objDataDoc.CreateElement("metamagic");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMetamagicId;
                        xmlMetamagicNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMetamagicName;
                        xmlMetamagicNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMetamagicName;
                        xmlMetamagicNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataMetamagicNode["page"].InnerText;
                        xmlMetamagicNode.AppendChild(xmlPageElement);

                        xmlMetamagicNodesParent.AppendChild(xmlMetamagicNode);
                    }
                }
            }
            foreach (XmlNode xmlMetamagicNode in xmlMetamagicNodesParent.SelectNodes("metamagic"))
            {
                xmlMetamagicNode.Attributes.RemoveAll();
                if (xmlDataMetamagicNodeList?.SelectSingleNode("metamagic[name = \"" + xmlMetamagicNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMetamagicNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlMetamagicNodesParent.RemoveChild(xmlMetamagicNode);
                    }
#endif
                }
            }

            // Process Arts

            XmlNode xmlArtNodesParent = xmlRootMetamagicFileNode.SelectSingleNode("arts");
            if (xmlArtNodesParent == null)
            {
                xmlArtNodesParent = objDataDoc.CreateElement("arts");
                xmlRootMetamagicFileNode.AppendChild(xmlArtNodesParent);
            }

            XmlNode xmlDataArtNodeList = xmlDataDocument.SelectSingleNode("/chummer/arts");
            if (xmlDataArtNodeList != null)
            {
                foreach (XmlNode xmlDataArtNode in xmlDataArtNodeList.SelectNodes("art"))
                {
                    string strDataArtId = xmlDataArtNode["id"].InnerText;
                    string strDataArtName = xmlDataArtNode["name"].InnerText;
                    XmlNode xmlArtNode = xmlArtNodesParent.SelectSingleNode("art[name=\"" + strDataArtName + "\"]");
                    if (xmlArtNode != null)
                    {
                        if (xmlArtNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArtId;
                            xmlArtNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlArtNode = objDataDoc.CreateElement("art");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataArtId;
                        xmlArtNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataArtName;
                        xmlArtNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataArtName;
                        xmlArtNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataArtNode["page"].InnerText;
                        xmlArtNode.AppendChild(xmlPageElement);

                        xmlArtNodesParent.AppendChild(xmlArtNode);
                    }
                }
            }
            foreach (XmlNode xmlArtNode in xmlArtNodesParent.SelectNodes("art"))
            {
                xmlArtNode.Attributes.RemoveAll();
                if (xmlDataArtNodeList?.SelectSingleNode("art[name = \"" + xmlArtNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArtNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlArtNodesParent.RemoveChild(xmlArtNode);
                    }
#endif
                }
            }
        }

        private static void ProcessMetatypes(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "metatypes.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetatypeFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metatypes.xml\"]");
            if (xmlRootMetatypeFileNode == null)
            {
                xmlRootMetatypeFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "metatypes.xml";
                xmlRootMetatypeFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlMetatypeNodesParent = objDataDoc.CreateElement("metatypes");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlMetatypeNode = objDataDoc.CreateElement("metatype");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataMetatypeId;
                        xmlMetatypeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataMetatypeName;
                        xmlMetatypeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataMetatypeNode["page"].InnerText;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }

                    // Process Metavariants

                    XmlNode xmlMetavariantNodesParent = xmlMetatypeNode.SelectSingleNode("metavariants");
                    XmlNode xmlDataMetavariantNodeList = xmlDataMetatypeNode.SelectSingleNode("metavariants");

                    if (xmlDataMetavariantNodeList != null)
                    {
                        if (xmlMetavariantNodesParent == null)
                        {
                            xmlMetavariantNodesParent = objDataDoc.CreateElement("metavariants");
                            xmlMetatypeNode.AppendChild(xmlMetavariantNodesParent);
                        }
                        foreach (XmlNode xmlDataMetavariantNode in xmlDataMetavariantNodeList.SelectNodes("metavariant"))
                        {
                            string strDataMetavariantName = xmlDataMetavariantNode["name"].InnerText;
                            string strDataMetavariantId = xmlDataMetavariantNode["id"].InnerText;
                            XmlNode xmlMetavariantNode = xmlMetavariantNodesParent.SelectSingleNode("metavariant[name=\"" + strDataMetavariantName + "\"]");
                            if (xmlMetavariantNode != null)
                            {
                                if (xmlMetavariantNode["id"] == null)
                                {
                                    XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                                    xmlIdElement.InnerText = strDataMetavariantId;
                                    xmlMetavariantNode.PrependChild(xmlIdElement);
                                }
                            }
                            else
                            {
                                xmlMetavariantNode = objDataDoc.CreateElement("metavariant");

                                XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                                xmlIdElement.InnerText = strDataMetavariantId;
                                xmlMetavariantNode.AppendChild(xmlIdElement);

                                XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                                xmlNameElement.InnerText = strDataMetavariantName;
                                xmlMetavariantNode.AppendChild(xmlNameElement);

                                XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                                xmlTranslateElement.InnerText = strDataMetavariantName;
                                xmlMetavariantNode.AppendChild(xmlTranslateElement);

                                XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                                xmlPageElement.InnerText = xmlDataMetavariantNode["page"].InnerText;
                                xmlMetavariantNode.AppendChild(xmlPageElement);

                                xmlMetavariantNodesParent.AppendChild(xmlMetavariantNode);
                            }
                        }
                        foreach (XmlNode xmlMetavariantNode in xmlMetavariantNodesParent.SelectNodes("metavariant"))
                        {
                            xmlMetavariantNode.Attributes.RemoveAll();
                            if (xmlDataMetavariantNodeList?.SelectSingleNode("metavariant[name = \"" + xmlMetavariantNode["name"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                                {
                                    XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                    xmlExistsAttribute.Value = "False";
                                    xmlMetavariantNode.Attributes.Append(xmlExistsAttribute);
                                }
#else
                                {
                                    xmlMetavariantNodesParent.RemoveChild(xmlMetavariantNode);
                                }
#endif
                            }
                        }
                    }
                    else if (xmlMetavariantNodesParent != null)
                    {
#if !DELETE
                        {
                            XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                            xmlExistsAttribute.Value = "False";
                            xmlMetatypeNode.Attributes.Append(xmlExistsAttribute);
                        }
#else
                        {
                            xmlMetatypeNode.RemoveChild(xmlMetavariantNodesParent);
                        }
#endif
                    }
                }
            }
            foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodesParent.SelectNodes("metatype"))
            {
                xmlMetatypeNode.Attributes.RemoveAll();
                if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[name = \"" + xmlMetatypeNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlMetatypeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                    }
#endif
                }
            }
        }

        private static void ProcessPowers(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "powers.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPowerFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"powers.xml\"]");
            if (xmlRootPowerFileNode == null)
            {
                xmlRootPowerFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "powers.xml";
                xmlRootPowerFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = objDataDoc.CreateElement("powers");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlPowerNode = objDataDoc.CreateElement("power");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataPowerId;
                        xmlPowerNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPowerNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                    }
#endif
                }
            }

            // Process Enhancements

            XmlNode xmlEnhancementNodesParent = xmlRootPowerFileNode.SelectSingleNode("enhancements");
            if (xmlEnhancementNodesParent == null)
            {
                xmlEnhancementNodesParent = objDataDoc.CreateElement("enhancements");
                xmlRootPowerFileNode.AppendChild(xmlEnhancementNodesParent);
            }

            XmlNode xmlDataEnhancementNodeList = xmlDataDocument.SelectSingleNode("/chummer/enhancements");
            if (xmlDataEnhancementNodeList != null)
            {
                foreach (XmlNode xmlDataEnhancementNode in xmlDataEnhancementNodeList.SelectNodes("enhancement"))
                {
                    string strDataEnhancementId = xmlDataEnhancementNode["id"].InnerText;
                    string strDataEnhancementName = xmlDataEnhancementNode["name"].InnerText;
                    XmlNode xmlEnhancementNode = xmlEnhancementNodesParent.SelectSingleNode("enhancement[name=\"" + strDataEnhancementName + "\"]");
                    if (xmlEnhancementNode != null)
                    {
                        if (xmlEnhancementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEnhancementId;
                            xmlEnhancementNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlEnhancementNode = objDataDoc.CreateElement("enhancement");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataEnhancementId;
                        xmlEnhancementNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataEnhancementName;
                        xmlEnhancementNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataEnhancementName;
                        xmlEnhancementNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlEnhancementNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlEnhancementNodesParent.RemoveChild(xmlEnhancementNode);
                    }
#endif
                }
            }
        }

        private static void ProcessPriorities(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "priorities.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPriorityFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"priorities.xml\"]");
            if (xmlRootPriorityFileNode == null)
            {
                xmlRootPriorityFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "priorities.xml";
                xmlRootPriorityFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPriorityFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootPriorityFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootPriorityFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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

            // Process Priorities

            XmlNode xmlPriorityNodesParent = xmlRootPriorityFileNode.SelectSingleNode("priorities");
            if (xmlPriorityNodesParent == null)
            {
                xmlPriorityNodesParent = objDataDoc.CreateElement("priorities");
                xmlRootPriorityFileNode.AppendChild(xmlPriorityNodesParent);
            }

            XmlNode xmlDataPriorityNodeList = xmlDataDocument.SelectSingleNode("/chummer/priorities");
            if (xmlDataPriorityNodeList != null)
            {
                foreach (XmlNode xmlDataPriorityNode in xmlDataPriorityNodeList.SelectNodes("priority"))
                {
                    string strDataPriorityName = xmlDataPriorityNode["name"].InnerText;
                    string strDataPriorityId = xmlDataPriorityNode["id"].InnerText;
                    XmlNode xmlPriorityNode = xmlRootPriorityFileNode.SelectSingleNode("priorities/priority[name=\"" + strDataPriorityName + "\"]");
                    if (xmlPriorityNode != null)
                    {
                        if (xmlPriorityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPriorityId;
                            xmlPriorityNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlPriorityNode = objDataDoc.CreateElement("priority");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataPriorityId;
                        xmlPriorityNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPriorityName;
                        xmlPriorityNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPriorityName;
                        xmlPriorityNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataPriorityNode["page"].InnerText;
                        xmlPriorityNode.AppendChild(xmlPageElement);

                        xmlPriorityNodesParent.AppendChild(xmlPriorityNode);
                    }
                }
            }
            foreach (XmlNode xmlPriorityNode in xmlPriorityNodesParent.SelectNodes("priority"))
            {
                xmlPriorityNode.Attributes.RemoveAll();
                if (xmlDataPriorityNodeList?.SelectSingleNode("priority[name = \"" + xmlPriorityNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPriorityNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlPriorityNodesParent.RemoveChild(xmlPriorityNode);
                    }
#endif
                }
            }

            // Process Gameplay Options

            XmlNode xmlGameplayOptionNodesParent = xmlRootPriorityFileNode.SelectSingleNode("gameplayoptions");
            if (xmlGameplayOptionNodesParent == null)
            {
                xmlGameplayOptionNodesParent = objDataDoc.CreateElement("gameplayoptions");
                xmlRootPriorityFileNode.AppendChild(xmlGameplayOptionNodesParent);
            }

            XmlNode xmlDataGameplayOptionNodeList = xmlDataDocument.SelectSingleNode("/chummer/gameplayoptions");
            if (xmlDataGameplayOptionNodeList != null)
            {
                foreach (XmlNode xmlDataGameplayOptionNode in xmlDataGameplayOptionNodeList.SelectNodes("gameplayoption"))
                {
                    string strDataGameplayOptionId = xmlDataGameplayOptionNode["id"].InnerText;
                    string strDataGameplayOptionName = xmlDataGameplayOptionNode["name"].InnerText;
                    XmlNode xmlGameplayOptionNode = xmlGameplayOptionNodesParent.SelectSingleNode("gameplayoption[name=\"" + strDataGameplayOptionName + "\"]");
                    if (xmlGameplayOptionNode != null)
                    {
                        if (xmlGameplayOptionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGameplayOptionId;
                            xmlGameplayOptionNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlGameplayOptionNode = objDataDoc.CreateElement("gameplayoption");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataGameplayOptionId;
                        xmlGameplayOptionNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataGameplayOptionName;
                        xmlGameplayOptionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataGameplayOptionName;
                        xmlGameplayOptionNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataGameplayOptionNode["page"].InnerText;
                        xmlGameplayOptionNode.AppendChild(xmlPageElement);

                        xmlGameplayOptionNodesParent.AppendChild(xmlGameplayOptionNode);
                    }
                }
            }
            foreach (XmlNode xmlGameplayOptionNode in xmlGameplayOptionNodesParent.SelectNodes("gameplayoption"))
            {
                xmlGameplayOptionNode.Attributes.RemoveAll();
                if (xmlDataGameplayOptionNodeList?.SelectSingleNode("gameplayoption[name = \"" + xmlGameplayOptionNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGameplayOptionNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlGameplayOptionNodesParent.RemoveChild(xmlGameplayOptionNode);
                    }
#endif
                }
            }

            // Remove Maneuvers

            XmlNode xmlRemoveNode = xmlRootPriorityFileNode.SelectSingleNode("maneuvers");
            if (xmlRemoveNode != null)
            {
                xmlRootPriorityFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessPrograms(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "programs.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootProgramFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"programs.xml\"]");
            if (xmlRootProgramFileNode == null)
            {
                xmlRootProgramFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "programs.xml";
                xmlRootProgramFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootProgramFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootProgramFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootProgramFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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

            // Process Programs

            XmlNode xmlProgramNodesParent = xmlRootProgramFileNode.SelectSingleNode("programs");
            if (xmlProgramNodesParent == null)
            {
                xmlProgramNodesParent = objDataDoc.CreateElement("programs");
                xmlRootProgramFileNode.AppendChild(xmlProgramNodesParent);
            }

            XmlNode xmlDataProgramNodeList = xmlDataDocument.SelectSingleNode("/chummer/programs");
            if (xmlDataProgramNodeList != null)
            {
                foreach (XmlNode xmlDataProgramNode in xmlDataProgramNodeList.SelectNodes("program"))
                {
                    string strDataProgramName = xmlDataProgramNode["name"].InnerText;
                    string strDataProgramId = xmlDataProgramNode["id"].InnerText;
                    XmlNode xmlProgramNode = xmlProgramNodesParent.SelectSingleNode("program[name=\"" + strDataProgramName + "\"]");
                    if (xmlProgramNode != null)
                    {
                        if (xmlProgramNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataProgramId;
                            xmlProgramNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlProgramNode = objDataDoc.CreateElement("program");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataProgramId;
                        xmlProgramNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataProgramName;
                        xmlProgramNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataProgramName;
                        xmlProgramNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataProgramNode["page"].InnerText;
                        xmlProgramNode.AppendChild(xmlPageElement);

                        xmlProgramNodesParent.AppendChild(xmlProgramNode);
                    }
                }
            }
            foreach (XmlNode xmlProgramNode in xmlProgramNodesParent.SelectNodes("program"))
            {
                xmlProgramNode.Attributes.RemoveAll();
                if (xmlDataProgramNodeList?.SelectSingleNode("program[name = \"" + xmlProgramNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlProgramNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlProgramNodesParent.RemoveChild(xmlProgramNode);
                    }
#endif
                }
            }

            // Remove Options

            XmlNode xmlRemoveNode = xmlRootProgramFileNode.SelectSingleNode("options");
            if (xmlRemoveNode != null)
            {
                xmlRootProgramFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessQualities(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "qualities.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootQualityFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"qualities.xml\"]");
            if (xmlRootQualityFileNode == null)
            {
                xmlRootQualityFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "qualities.xml";
                xmlRootQualityFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootQualityFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootQualityFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootQualityFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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

            // Process Qualities

            XmlNode xmlQualityNodesParent = xmlRootQualityFileNode.SelectSingleNode("qualities");
            if (xmlQualityNodesParent == null)
            {
                xmlQualityNodesParent = objDataDoc.CreateElement("qualities");
                xmlRootQualityFileNode.AppendChild(xmlQualityNodesParent);
            }

            XmlNode xmlDataQualityNodeList = xmlDataDocument.SelectSingleNode("/chummer/qualities");
            if (xmlDataQualityNodeList != null)
            {
                foreach (XmlNode xmlDataQualityNode in xmlDataQualityNodeList.SelectNodes("quality"))
                {
                    string strDataQualityName = xmlDataQualityNode["name"].InnerText;
                    string strDataQualityId = xmlDataQualityNode["id"].InnerText;
                    XmlNode xmlQualityNode = xmlQualityNodesParent.SelectSingleNode("quality[name=\"" + strDataQualityName + "\"]");
                    if (xmlQualityNode != null)
                    {
                        if (xmlQualityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataQualityId;
                            xmlQualityNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlQualityNode = objDataDoc.CreateElement("quality");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataQualityId;
                        xmlQualityNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataQualityName;
                        xmlQualityNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlQualityNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlQualityNodesParent.RemoveChild(xmlQualityNode);
                    }
#endif
                }
            }
        }

        private static void ProcessSkills(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "skills.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootSkillFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"skills.xml\"]");
            if (xmlRootSkillFileNode == null)
            {
                xmlRootSkillFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "skills.xml";
                xmlRootSkillFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootSkillFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootSkillFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootSkillFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        XmlAttribute xmlTypeAttribute = objDataDoc.CreateAttribute("type");
                        xmlTypeAttribute.Value = xmlDataCategoryNode.Attributes?["type"]?.InnerText ?? string.Empty;
                        xmlCategoryNode.Attributes.Append(xmlTypeAttribute);
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

            // Process Skill Groups

            XmlNode xmlSkillGroupNodesParent = xmlRootSkillFileNode.SelectSingleNode("skillgroups");

            if (xmlSkillGroupNodesParent == null)
            {
                xmlSkillGroupNodesParent = objDataDoc.CreateElement("skillgroups");
                xmlRootSkillFileNode.AppendChild(xmlSkillGroupNodesParent);
            }

            XmlNode xmlDataSkillGroupNodeList = xmlDataDocument.SelectSingleNode("/chummer/skillgroups");
            if (xmlDataSkillGroupNodeList != null)
            {
                foreach (XmlNode xmlDataSkillGroupNode in xmlDataSkillGroupNodeList.SelectNodes("name"))
                {
                    if (xmlSkillGroupNodesParent.SelectSingleNode("name[text()=\"" + xmlDataSkillGroupNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlSkillGroupNode = objDataDoc.CreateElement("name");
                        xmlSkillGroupNode.InnerText = xmlDataSkillGroupNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataSkillGroupNode.InnerText;
                        xmlSkillGroupNode.Attributes.Append(xmlTranslateAttribute);
                        xmlSkillGroupNodesParent.AppendChild(xmlSkillGroupNode);
                    }
                }
            }
            foreach (XmlNode xmlSkillGroupNode in xmlSkillGroupNodesParent.SelectNodes("name"))
            {
                if (xmlDataSkillGroupNodeList?.SelectSingleNode("name[text() = \"" + xmlSkillGroupNode.InnerText + "\"]") == null)
                {
                    xmlSkillGroupNodesParent.RemoveChild(xmlSkillGroupNode);
                }
            }

            // Process Skills

            XmlNode xmlSkillNodesParent = xmlRootSkillFileNode.SelectSingleNode("skills");
            if (xmlSkillNodesParent == null)
            {
                xmlSkillNodesParent = objDataDoc.CreateElement("skills");
                xmlRootSkillFileNode.AppendChild(xmlSkillNodesParent);
            }

            XmlNode xmlDataSkillNodeList = xmlDataDocument.SelectSingleNode("/chummer/skills");
            if (xmlDataSkillNodeList != null)
            {
                foreach (XmlNode xmlDataSkillNode in xmlDataSkillNodeList.SelectNodes("skill"))
                {
                    string strDataSkillName = xmlDataSkillNode["name"].InnerText;
                    string strDataSkillId = xmlDataSkillNode["id"].InnerText;
                    XmlNode xmlSkillNode = xmlRootSkillFileNode.SelectSingleNode("skills/skill[name=\"" + strDataSkillName + "\"]");
                    if (xmlSkillNode != null)
                    {
                        if (xmlSkillNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSkillId;
                            xmlSkillNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSkillNode = objDataDoc.CreateElement("skill");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSkillId;
                        xmlSkillNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSkillName;
                        xmlSkillNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSkillName;
                        xmlSkillNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataSkillNode["page"].InnerText;
                        xmlSkillNode.AppendChild(xmlPageElement);

                        xmlSkillNodesParent.AppendChild(xmlSkillNode);
                    }

                    XmlNode xmlSkillSpecsNode = xmlSkillNode["specs"];
                    if (xmlSkillSpecsNode == null)
                    {
                        xmlSkillSpecsNode = objDataDoc.CreateElement("specs");
                        xmlSkillNode.AppendChild(xmlSkillSpecsNode);
                    }
                    XmlNode xmlDataSkillSpecsNodeList = xmlDataSkillNode.SelectSingleNode("specs");
                    foreach (XmlNode xmlDataSpecNode in xmlDataSkillSpecsNodeList.SelectNodes("spec"))
                    {
                        string strSpecName = xmlDataSpecNode.InnerText;
                        XmlNode xmlSpecNode = xmlSkillSpecsNode.SelectSingleNode("spec[text()=\"" + strSpecName + "\"]");
                        if (xmlSpecNode == null)
                        {
                            xmlSpecNode = objDataDoc.CreateElement("spec");
                            xmlSpecNode.InnerText = strSpecName;
                            XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                            xmlTranslateAttribute.InnerText = strSpecName;
                            xmlSpecNode.Attributes.Append(xmlTranslateAttribute);
                            xmlSkillSpecsNode.AppendChild(xmlSpecNode);
                        }
                    }
                }
            }
            foreach (XmlNode xmlSkillNode in xmlSkillNodesParent.SelectNodes("skill"))
            {
                xmlSkillNode.Attributes.RemoveAll();
                XmlNode xmlDataSkillNode = xmlDataSkillNodeList?.SelectSingleNode("skill[name = \"" + xmlSkillNode["name"]?.InnerText + "\"]");
                if (xmlDataSkillNode == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSkillNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlSkillNodesParent.RemoveChild(xmlSkillNode);
                    }
#endif
                }
                else
                {
                    XmlNode xmlSkillNodeSpecsParent = xmlSkillNode.SelectSingleNode("specs");
                    if (xmlSkillNodeSpecsParent != null)
                    {
                        xmlSkillNodeSpecsParent.Attributes.RemoveAll();
                        XmlNode xmlDataSkillNodeSpecsParent = xmlDataSkillNode.SelectSingleNode("specs");
                        if (xmlDataSkillNodeSpecsParent == null)
                        {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSkillNodeSpecsParent.Attributes.Append(xmlExistsAttribute);
                            }
#else
                            {
                                xmlSkillNode.RemoveChild(xmlSkillNodeSpecsParent);
                            }
#endif
                        }
                        else
                        {
                            foreach (XmlNode xmlSpecNode in xmlSkillNodeSpecsParent.SelectNodes("spec"))
                            {
                                if (xmlDataSkillNodeSpecsParent.SelectSingleNode("spec[text() = \"" + xmlSpecNode.InnerText + "\"]") == null)
                                {
#if !DELETE
                                    {
                                        XmlAttribute xmlExistsAttribute = xmlSpecNode.Attributes["exists"];
                                        if (xmlExistsAttribute == null)
                                        {
                                            xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                            xmlExistsAttribute.Value = "False";
                                            xmlSpecNode.Attributes.Append(xmlExistsAttribute);
                                        }
                                        else
                                            xmlExistsAttribute.Value = "False";
                                    }
#else
                                    {
                                        xmlSkillNodeSpecsParent.RemoveChild(xmlSpecNode);
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }

            // Process Knowledge Skills

            XmlNode xmlKnowledgeSkillNodesParent = xmlRootSkillFileNode.SelectSingleNode("knowledgeskills");
            if (xmlKnowledgeSkillNodesParent == null)
            {
                xmlKnowledgeSkillNodesParent = objDataDoc.CreateElement("knowledgeskills");
                xmlRootSkillFileNode.AppendChild(xmlKnowledgeSkillNodesParent);
            }

            XmlNode xmlDataKnowledgeSkillNodeList = xmlDataDocument.SelectSingleNode("/chummer/knowledgeskills");
            if (xmlDataKnowledgeSkillNodeList != null)
            {
                foreach (XmlNode xmlDataKnowledgeSkillNode in xmlDataKnowledgeSkillNodeList.SelectNodes("skill"))
                {
                    string strDataKnowledgeSkillId = xmlDataKnowledgeSkillNode["id"].InnerText;
                    string strDataKnowledgeSkillName = xmlDataKnowledgeSkillNode["name"].InnerText;
                    XmlNode xmlKnowledgeSkillNode = xmlKnowledgeSkillNodesParent.SelectSingleNode("skill[name=\"" + strDataKnowledgeSkillName + "\"]");
                    if (xmlKnowledgeSkillNode != null)
                    {
                        if (xmlKnowledgeSkillNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataKnowledgeSkillId;
                            xmlKnowledgeSkillNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlKnowledgeSkillNode = objDataDoc.CreateElement("skill");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataKnowledgeSkillId;
                        xmlKnowledgeSkillNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataKnowledgeSkillName;
                        xmlKnowledgeSkillNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataKnowledgeSkillName;
                        xmlKnowledgeSkillNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataKnowledgeSkillNode["page"].InnerText;
                        xmlKnowledgeSkillNode.AppendChild(xmlPageElement);

                        xmlKnowledgeSkillNodesParent.AppendChild(xmlKnowledgeSkillNode);
                    }

                    XmlNode xmlKnowledgeSkillSpecsNode = xmlKnowledgeSkillNode["specs"];
                    if (xmlKnowledgeSkillSpecsNode == null)
                    {
                        xmlKnowledgeSkillSpecsNode = objDataDoc.CreateElement("specs");
                        xmlKnowledgeSkillNode.AppendChild(xmlKnowledgeSkillSpecsNode);
                    }
                    XmlNode xmlDataKnowledgeSkillSpecsNodeList = xmlDataKnowledgeSkillNode.SelectSingleNode("specs");
                    foreach (XmlNode xmlDataSpecNode in xmlDataKnowledgeSkillSpecsNodeList.SelectNodes("spec"))
                    {
                        string strSpecName = xmlDataSpecNode.InnerText;
                        XmlNode xmlSpecNode = xmlKnowledgeSkillSpecsNode.SelectSingleNode("spec[text()=\"" + strSpecName + "\"]");
                        if (xmlSpecNode == null)
                        {
                            xmlSpecNode = objDataDoc.CreateElement("spec");
                            xmlSpecNode.InnerText = strSpecName;
                            XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                            xmlTranslateAttribute.InnerText = strSpecName;
                            xmlSpecNode.Attributes.Append(xmlTranslateAttribute);
                            xmlKnowledgeSkillSpecsNode.AppendChild(xmlSpecNode);
                        }
                    }
                }
            }
            foreach (XmlNode xmlKnowledgeSkillNode in xmlKnowledgeSkillNodesParent.SelectNodes("skill"))
            {
                xmlKnowledgeSkillNode.Attributes.RemoveAll();
                XmlNode xmlDataKnowledgeSkillNode = xmlDataKnowledgeSkillNodeList?.SelectSingleNode("skill[name = \"" + xmlKnowledgeSkillNode["name"]?.InnerText + "\"]");
                if (xmlDataKnowledgeSkillNode == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlKnowledgeSkillNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlKnowledgeSkillNodesParent.RemoveChild(xmlKnowledgeSkillNode);
                    }
#endif
                }
                else
                {
                    XmlNode xmlSkillNodeSpecsParent = xmlKnowledgeSkillNode.SelectSingleNode("specs");
                    if (xmlSkillNodeSpecsParent != null)
                    {
                        xmlSkillNodeSpecsParent.Attributes.RemoveAll();
                        XmlNode xmlDataSkillNodeSpecsParent = xmlDataKnowledgeSkillNode.SelectSingleNode("specs");
                        if (xmlDataSkillNodeSpecsParent == null)
                        {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSkillNodeSpecsParent.Attributes.Append(xmlExistsAttribute);
                            }
#else
                            {
                                xmlKnowledgeSkillNode.RemoveChild(xmlSkillNodeSpecsParent);
                            }
#endif
                        }
                        else
                        {
                            foreach (XmlNode xmlSpecNode in xmlSkillNodeSpecsParent.SelectNodes("spec"))
                            {
                                if (xmlDataSkillNodeSpecsParent.SelectSingleNode("spec[text() = \"" + xmlSpecNode.InnerText + "\"]") == null)
                                {
#if !DELETE
                                    {
                                        XmlAttribute xmlExistsAttribute = xmlSpecNode.Attributes["exists"];
                                        if (xmlExistsAttribute == null)
                                        {
                                            xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                            xmlExistsAttribute.Value = "False";
                                            xmlSpecNode.Attributes.Append(xmlExistsAttribute);
                                        }
                                        else
                                            xmlExistsAttribute.Value = "False";
                                    }
#else
                                    {
                                        xmlSkillNodeSpecsParent.RemoveChild(xmlSpecNode);
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessSpells(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "spells.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootSpellFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"spells.xml\"]");
            if (xmlRootSpellFileNode == null)
            {
                xmlRootSpellFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "spells.xml";
                xmlRootSpellFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootSpellFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootSpellFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootSpellFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlSpellNodesParent = objDataDoc.CreateElement("spells");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpellId;
                            xmlSpellNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpellNode = objDataDoc.CreateElement("spell");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpellId;
                        xmlSpellNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpellName;
                        xmlSpellNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpellName;
                        xmlSpellNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpellNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlSpellNodesParent.RemoveChild(xmlSpellNode);
                    }
#endif
                }
            }
        }

        private static void ProcessStreams(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "streams.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootTraditionFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"streams.xml\"]");
            if (xmlRootTraditionFileNode == null)
            {
                xmlRootTraditionFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "streams.xml";
                xmlRootTraditionFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Streams

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = objDataDoc.CreateElement("traditions");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTraditionNode = objDataDoc.CreateElement("tradition");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTraditionId;
                        xmlTraditionNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTraditionNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                    }
#endif
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XmlNode xmlDataSpiritNodeList = xmlDataDocument.SelectSingleNode("/chummer/spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XmlNode xmlDataSpiritNode in xmlDataSpiritNodeList.SelectNodes("spirit"))
                {
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[name=\"" + strDataSpiritName + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpiritNode = objDataDoc.CreateElement("spirit");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpiritId;
                        xmlSpiritNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpiritNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                    }
#endif
                }
            }
        }

        private static void ProcessTraditions(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "traditions.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootTraditionFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"traditions.xml\"]");
            if (xmlRootTraditionFileNode == null)
            {
                xmlRootTraditionFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "traditions.xml";
                xmlRootTraditionFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Traditions

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = objDataDoc.CreateElement("traditions");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlTraditionNode = objDataDoc.CreateElement("tradition");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTraditionId;
                        xmlTraditionNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTraditionName;
                        xmlTraditionNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTraditionNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                    }
#endif
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XmlNode xmlDataSpiritNodeList = xmlDataDocument.SelectSingleNode("/chummer/spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XmlNode xmlDataSpiritNode in xmlDataSpiritNodeList.SelectNodes("spirit"))
                {
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[name=\"" + strDataSpiritName + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlSpiritNode = objDataDoc.CreateElement("spirit");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataSpiritId;
                        xmlSpiritNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSpiritName;
                        xmlSpiritNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpiritNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                    }
#endif
                }
            }

            // Process Drain Attributes

            XmlNode xmlDrainAttributeNodesParent = xmlRootTraditionFileNode.SelectSingleNode("drainattributes");
            if (xmlDrainAttributeNodesParent == null)
            {
                xmlDrainAttributeNodesParent = objDataDoc.CreateElement("drainattributes");
                xmlRootTraditionFileNode.AppendChild(xmlDrainAttributeNodesParent);
            }

            XmlNode xmlDataDrainAttributeNodeList = xmlDataDocument.SelectSingleNode("/chummer/drainattributes");
            if (xmlDataDrainAttributeNodeList != null)
            {
                foreach (XmlNode xmlDataDrainAttributeNode in xmlDataDrainAttributeNodeList.SelectNodes("drainattribute"))
                {
                    string strDataDrainAttributeId = xmlDataDrainAttributeNode["id"].InnerText;
                    string strDataDrainAttributeName = xmlDataDrainAttributeNode["name"].InnerText;
                    XmlNode xmlDrainAttributeNode = xmlDrainAttributeNodesParent.SelectSingleNode("drainattribute[name=\"" + strDataDrainAttributeName + "\"]");
                    if (xmlDrainAttributeNode != null)
                    {
                        if (xmlDrainAttributeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataDrainAttributeId;
                            xmlDrainAttributeNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlDrainAttributeNode = objDataDoc.CreateElement("drainattribute");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataDrainAttributeId;
                        xmlDrainAttributeNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataDrainAttributeName;
                        xmlDrainAttributeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataDrainAttributeName;
                        xmlDrainAttributeNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlDrainAttributeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlDrainAttributeNodesParent.RemoveChild(xmlDrainAttributeNode);
                    }
#endif
                }
            }
        }

        private static void ProcessVehicles(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "vehicles.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootVehicleFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vehicles.xml\"]");
            if (xmlRootVehicleFileNode == null)
            {
                xmlRootVehicleFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "vehicles.xml";
                xmlRootVehicleFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootVehicleFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootVehicleFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootVehicleFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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

            // Process Mod Categories

            XmlNode xmlModCategoryNodesParent = xmlRootVehicleFileNode.SelectSingleNode("modcategories");

            if (xmlModCategoryNodesParent == null)
            {
                xmlModCategoryNodesParent = objDataDoc.CreateElement("modcategories");
                xmlRootVehicleFileNode.AppendChild(xmlModCategoryNodesParent);
            }

            XmlNode xmlDataModCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/modcategories");
            if (xmlDataModCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataModCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlModCategoryNode in xmlModCategoryNodesParent.SelectNodes("category"))
            {
                if (xmlDataModCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlModCategoryNode.InnerText + "\"]") == null)
                {
                    xmlModCategoryNodesParent.RemoveChild(xmlModCategoryNode);
                }
            }

            // Process Vehicles

            XmlNode xmlVehicleNodesParent = xmlRootVehicleFileNode.SelectSingleNode("vehicles");
            if (xmlVehicleNodesParent == null)
            {
                xmlVehicleNodesParent = objDataDoc.CreateElement("vehicles");
                xmlRootVehicleFileNode.AppendChild(xmlVehicleNodesParent);
            }

            XmlNode xmlDataVehicleNodeList = xmlDataDocument.SelectSingleNode("/chummer/vehicles");
            if (xmlDataVehicleNodeList != null)
            {
                foreach (XmlNode xmlDataVehicleNode in xmlDataVehicleNodeList.SelectNodes("vehicle"))
                {
                    string strDataVehicleName = xmlDataVehicleNode["name"].InnerText;
                    string strDataVehicleId = xmlDataVehicleNode["id"].InnerText;
                    XmlNode xmlVehicleNode = xmlVehicleNodesParent.SelectSingleNode("vehicle[name=\"" + strDataVehicleName + "\"]");
                    if (xmlVehicleNode != null)
                    {
                        if (xmlVehicleNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataVehicleId;
                            xmlVehicleNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlVehicleNode = objDataDoc.CreateElement("vehicle");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataVehicleId;
                        xmlVehicleNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataVehicleName;
                        xmlVehicleNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataVehicleName;
                        xmlVehicleNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataVehicleNode["page"].InnerText;
                        xmlVehicleNode.AppendChild(xmlPageElement);

                        xmlVehicleNodesParent.AppendChild(xmlVehicleNode);
                    }
                }
            }
            foreach (XmlNode xmlVehicleNode in xmlVehicleNodesParent.SelectNodes("vehicle"))
            {
                xmlVehicleNode.Attributes.RemoveAll();
                if (xmlDataVehicleNodeList?.SelectSingleNode("vehicle[name = \"" + xmlVehicleNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlVehicleNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlVehicleNodesParent.RemoveChild(xmlVehicleNode);
                    }
#endif
                }
            }

            // Process Vehicle Mods

            XmlNode xmlVehicleModNodesParent = xmlRootVehicleFileNode.SelectSingleNode("mods");
            if (xmlVehicleModNodesParent == null)
            {
                xmlVehicleModNodesParent = objDataDoc.CreateElement("mods");
                xmlRootVehicleFileNode.AppendChild(xmlVehicleModNodesParent);
            }

            XmlNode xmlDataVehicleModNodeList = xmlDataDocument.SelectSingleNode("/chummer/mods");
            if (xmlDataVehicleModNodeList != null)
            {
                foreach (XmlNode xmlDataVehicleModNode in xmlDataVehicleModNodeList.SelectNodes("mod"))
                {
                    string strDataVehicleModId = xmlDataVehicleModNode["id"].InnerText;
                    string strDataVehicleModName = xmlDataVehicleModNode["name"].InnerText;
                    XmlNode xmlVehicleModNode = xmlVehicleModNodesParent.SelectSingleNode("mod[name=\"" + strDataVehicleModName + "\"]");
                    if (xmlVehicleModNode != null)
                    {
                        if (xmlVehicleModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataVehicleModId;
                            xmlVehicleModNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlVehicleModNode = objDataDoc.CreateElement("mod");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataVehicleModId;
                        xmlVehicleModNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataVehicleModName;
                        xmlVehicleModNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataVehicleModName;
                        xmlVehicleModNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataVehicleModNode["page"].InnerText;
                        xmlVehicleModNode.AppendChild(xmlPageElement);

                        xmlVehicleModNodesParent.AppendChild(xmlVehicleModNode);
                    }
                }
            }
            foreach (XmlNode xmlVehicleModNode in xmlVehicleModNodesParent.SelectNodes("mod"))
            {
                xmlVehicleModNode.Attributes.RemoveAll();
                if (xmlDataVehicleModNodeList?.SelectSingleNode("mod[name = \"" + xmlVehicleModNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlVehicleModNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlVehicleModNodesParent.RemoveChild(xmlVehicleModNode);
                    }
#endif
                }
            }

            // Process Weapon Mounts

            XmlNode xmlWeaponMountNodesParent = xmlRootVehicleFileNode.SelectSingleNode("weaponmounts");
            if (xmlWeaponMountNodesParent == null)
            {
                xmlWeaponMountNodesParent = objDataDoc.CreateElement("weaponmounts");
                xmlRootVehicleFileNode.AppendChild(xmlWeaponMountNodesParent);
            }

            XmlNode xmlDataWeaponMountNodeList = xmlDataDocument.SelectSingleNode("/chummer/weaponmounts");
            if (xmlDataWeaponMountNodeList != null)
            {
                foreach (XmlNode xmlDataWeaponMountNode in xmlDataWeaponMountNodeList.SelectNodes("weaponmount"))
                {
                    string strDataWeaponMountId = xmlDataWeaponMountNode["id"].InnerText;
                    string strDataWeaponMountName = xmlDataWeaponMountNode["name"].InnerText;
                    XmlNode xmlWeaponMountNode = xmlWeaponMountNodesParent.SelectSingleNode("weaponmount[name=\"" + strDataWeaponMountName + "\"]");
                    if (xmlWeaponMountNode != null)
                    {
                        if (xmlWeaponMountNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponMountId;
                            xmlWeaponMountNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlWeaponMountNode = objDataDoc.CreateElement("weaponmount");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataWeaponMountId;
                        xmlWeaponMountNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataWeaponMountName;
                        xmlWeaponMountNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataWeaponMountName;
                        xmlWeaponMountNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
                        xmlPageElement.InnerText = xmlDataWeaponMountNode["page"].InnerText;
                        xmlWeaponMountNode.AppendChild(xmlPageElement);

                        xmlWeaponMountNodesParent.AppendChild(xmlWeaponMountNode);
                    }
                }
            }
            foreach (XmlNode xmlWeaponMountNode in xmlWeaponMountNodesParent.SelectNodes("weaponmount"))
            {
                xmlWeaponMountNode.Attributes.RemoveAll();
                if (xmlDataWeaponMountNodeList?.SelectSingleNode("weaponmount[name = \"" + xmlWeaponMountNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponMountNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlWeaponMountNodesParent.RemoveChild(xmlWeaponMountNode);
                    }
#endif
                }
            }

            // Remove Cybersuites

            XmlNode xmlRemoveNode = xmlRootVehicleFileNode.SelectSingleNode("limits");
            if (xmlRemoveNode != null)
            {
                xmlRootVehicleFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessWeapons(XmlDocument objDataDoc)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "weapons.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootWeaponFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"weapons.xml\"]");
            if (xmlRootWeaponFileNode == null)
            {
                xmlRootWeaponFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "weapons.xml";
                xmlRootWeaponFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootWeaponFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootWeaponFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
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
                xmlWeaponNodesParent = objDataDoc.CreateElement("weapons");
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
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponId;
                            xmlWeaponNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlWeaponNode = objDataDoc.CreateElement("weapon");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataWeaponId;
                        xmlWeaponNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataWeaponName;
                        xmlWeaponNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataWeaponName;
                        xmlWeaponNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlWeaponNodesParent.RemoveChild(xmlWeaponNode);
                    }
#endif
                }
            }

            // Process Weapon Mods

            XmlNode xmlAccessoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("accessories");
            if (xmlAccessoryNodesParent == null)
            {
                xmlAccessoryNodesParent = objDataDoc.CreateElement("accessories");
                xmlRootWeaponFileNode.AppendChild(xmlAccessoryNodesParent);
            }

            XmlNode xmlDataAccessoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/accessories");
            if (xmlDataAccessoryNodeList != null)
            {
                foreach (XmlNode xmlDataAccessoryNode in xmlDataAccessoryNodeList.SelectNodes("accessory"))
                {
                    string strDataAccessoryId = xmlDataAccessoryNode["id"].InnerText;
                    string strDataAccessoryName = xmlDataAccessoryNode["name"].InnerText;
                    XmlNode xmlAccessoryNode = xmlAccessoryNodesParent.SelectSingleNode("accessory[name=\"" + strDataAccessoryName + "\"]");
                    if (xmlAccessoryNode != null)
                    {
                        if (xmlAccessoryNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataAccessoryId;
                            xmlAccessoryNode.PrependChild(xmlIdElement);
                        }
                    }
                    else
                    {
                        xmlAccessoryNode = objDataDoc.CreateElement("accessory");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataAccessoryId;
                        xmlAccessoryNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataAccessoryName;
                        xmlAccessoryNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataAccessoryName;
                        xmlAccessoryNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("page");
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
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlAccessoryNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlAccessoryNodesParent.RemoveChild(xmlAccessoryNode);
                    }
#endif
                }
            }
        }
#endregion Process Methods
    }
}
