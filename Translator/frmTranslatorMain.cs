#define DELETE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Translator
{
    public partial class frmTranslatorMain
    {
        private static readonly TextInfo s_ObjEnUSTextInfo = (new CultureInfo("en-US", false)).TextInfo;
        private static readonly string PATH = Application.StartupPath;
        private readonly BackgroundWorker _workerDataProcessor = new BackgroundWorker();
        private readonly BackgroundWorker _workerStringsProcessor = new BackgroundWorker();
        private string _strLanguageToLoad = string.Empty;
        private static readonly List<frmTranslate> s_LstOpenTranslateWindows = new List<frmTranslate>();

        public frmTranslatorMain()
        {
            InitializeComponent();

            pbProcessProgress.Maximum = s_LstProcessFunctions.Length + 1;

            _workerDataProcessor.WorkerReportsProgress = true;
            _workerDataProcessor.WorkerSupportsCancellation = true;
            _workerDataProcessor.DoWork += DoDataProcessing;
            _workerDataProcessor.ProgressChanged += StepProgressBar;
            _workerDataProcessor.RunWorkerCompleted += FinishDataProcessing;

            _workerStringsProcessor.WorkerReportsProgress = false;
            _workerStringsProcessor.WorkerSupportsCancellation = true;
            _workerStringsProcessor.DoWork += DoStringsProcessing;
            _workerStringsProcessor.RunWorkerCompleted += FinishStringsProcessing;
        }

        #region Control Events
        private void cboLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool blnEnableButtons = cboLanguages.SelectedIndex != -1;
            cmdEdit.Enabled = blnEnableButtons;
            cmdRebuild.Enabled = blnEnableButtons;
        }

        private void txtLanguageCode_TextChanged(object sender, EventArgs e)
        {
            if (txtLanguageCode.TextLength >= 4 && txtLanguageCode.Text.Contains('-'))
            {
                string strLanguageCode = txtLanguageCode.Text;
                int intSelectionStart = txtLanguageCode.SelectionStart;
                int intSelectionLength = txtLanguageCode.SelectionLength;
                int len = strLanguageCode.Length;
                char[] newChars = new char[len];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
                int i2 = 0;
                for (int i = 0; i < len; ++i)
                {
                    char c = strLanguageCode[i];
                    if (c != '-')
                        newChars[i2++] = c;
                    else if (i2 < intSelectionStart)
                        intSelectionStart -= 1;
                    else if (intSelectionLength > 0 && i2 < intSelectionLength + intSelectionStart)
                        intSelectionLength -= 1;
                }
                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                txtLanguageCode.Text = new string(newChars, 0, i2);
                txtLanguageCode.SelectionStart = intSelectionStart;
                txtLanguageCode.SelectionLength = intSelectionLength;
            }

            if (txtLanguageCode.TextLength > 2)
            {
                if (txtRegionCode.TextLength == 0)
                {
                    string strRegionCode = txtLanguageCode.Text.Substring(2);
                    if (strRegionCode.Length > 2)
                    {
                        strRegionCode = strRegionCode.Substring(0, 2);
                    }
                    txtRegionCode.Text = strRegionCode;
                    txtRegionCode.SelectionStart = strRegionCode.Length;
                    txtRegionCode.SelectionLength = 0;
                }
                txtLanguageCode.Text = txtLanguageCode.Text.Substring(0, 2);
            }
            bool blnDoProcess = txtLanguageCode.TextLength == 2 && txtRegionCode.TextLength == 2;
            cmdCreate.Enabled = blnDoProcess;
            if (blnDoProcess)
            {
                string strLowerCode = txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToLower();
                try
                {
                    CultureInfo objSelectedCulture = CultureInfo.GetCultureInfo(strLowerCode);
                    string strName = objSelectedCulture.NativeName;
                    int intCountryNameIndex = strName.LastIndexOf('(');
                    if (intCountryNameIndex != -1)
                        strName = strName.Substring(0, intCountryNameIndex).Trim();
                    txtLanguageName.Text = s_ObjEnUSTextInfo.ToTitleCase(strName);
                }
                catch (CultureNotFoundException)
                {
                }
            }
            if (txtLanguageCode.TextLength == 2)
                txtRegionCode.Select();
        }

        private void txtRegionCode_TextChanged(object sender, EventArgs e)
        {
            bool blnDoProcess = txtLanguageCode.TextLength == 2 && txtRegionCode.TextLength == 2;
            cmdCreate.Enabled = blnDoProcess;
            if (blnDoProcess)
            {
                string strLowerCode = txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToLower();
                try
                {
                    CultureInfo objSelectedCulture = CultureInfo.GetCultureInfo(strLowerCode);
                    string strName = objSelectedCulture.NativeName;
                    int intCountryNameIndex = strName.LastIndexOf('(');
                    if (intCountryNameIndex != -1)
                        strName = s_ObjEnUSTextInfo.ToTitleCase(strName.Substring(0, intCountryNameIndex).Trim());
                    if (strName != "Unknown Locale")
                        txtLanguageName.Text = strName;
                }
                catch (CultureNotFoundException)
                {
                }
            }
            if (txtRegionCode.TextLength == 0)
                txtLanguageCode.Select();
        }

        private void cmdCreate_Click(object sender, EventArgs e)
        {
            if (txtLanguageCode.TextLength != 2)
            {
                MessageBox.Show("You must provide a two characters for the language code.");
                return;
            }
            if (txtRegionCode.TextLength != 2)
            {
                MessageBox.Show("You must provide a two character for the region code.");
                return;
            }
            string strLowerCode = txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToLower();

            if (strLowerCode == "en-us")
            {
                MessageBox.Show("You cannot create a localization with this language code, as it is Chummer5's default and fallback localization.");
                return;
            }

            try
            {
                CultureInfo objSelectedCulture = CultureInfo.GetCultureInfo(strLowerCode);
            }
            catch (CultureNotFoundException)
            {
                if (MessageBox.Show("The language code you provided has a language code that does not comply with ISO 639-1 and/or a region code that does not comply with ISO 3166-1. This may cause issues with Chummer.\n\nAre you sure you wish to use the entered code?",
                    "Language Code Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes)
                    return;
            }

            if (File.Exists(Path.Combine(PATH, "lang", strLowerCode + "_data.xml")) || File.Exists(Path.Combine(PATH, "lang", strLowerCode + ".xml")))
            {
                DialogResult eDialogResult = MessageBox.Show("A translation already exists with the same code as the one you provided. Do you wish to rebuild the existing translation instead of clearing it and starting anew?",
                    "Localization Already Exists", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (eDialogResult)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        {
                            XmlDocument objExistingTranslationDoc = new XmlDocument();
                            objExistingTranslationDoc.Load(Path.Combine(PATH, "lang", strLowerCode + ".xml"));

                            string strToSelect = objExistingTranslationDoc.SelectSingleNode("/chummer/name")?.InnerText;
                            if (!string.IsNullOrEmpty(strToSelect))
                            {
                                int intIndexToSelect = cboLanguages.FindStringExact(strToSelect);
                                if (intIndexToSelect != -1)
                                {
                                    cboLanguages.SelectedIndex = intIndexToSelect;
                                    cmdRebuild_Click(sender, e);
                                }
                            }
                        }
                        return;
                    case DialogResult.No:
                        {
                            try
                            {
                                string strPath = Path.Combine(PATH, "lang", strLowerCode + "_data.xml");
                                if (File.Exists(strPath + ".old"))
                                    File.Delete(strPath + ".old");
                                File.Move(strPath, strPath + ".old");
                                strPath = Path.Combine(PATH, "lang", strLowerCode + "xml");
                                if (File.Exists(strPath + ".old"))
                                    File.Delete(strPath + ".old");
                                File.Move(strPath, strPath + ".old");
                            }
                            catch (IOException)
                            {
                                MessageBox.Show("An error was encountered while trying to move the existing translation into a backup format. Aborting operation.",
                                    "File Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                MessageBox.Show("An error was encountered while trying to move the existing translation into a backup format. Aborting operation.",
                                    "File Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            break;
                        }
                }
            }

            Cursor = Cursors.AppStarting;
            pbProcessProgress.Value = 0;

            frmTranslate frmOpenTranslate = s_LstOpenTranslateWindows.FirstOrDefault(x => x.Language == _strLanguageToLoad);
            if (frmOpenTranslate != null)
            {
                frmOpenTranslate.Close();
                s_LstOpenTranslateWindows.Remove(frmOpenTranslate);
            }

            _strLanguageToLoad = s_ObjEnUSTextInfo.ToTitleCase(txtLanguageName.Text) + " (" + txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToUpper() + ')';
            string[] strArgs = { strLowerCode, _strLanguageToLoad };

            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            cmdCancel.Enabled = true;

            _workerDataProcessor.RunWorkerAsync(strArgs);
            _workerStringsProcessor.RunWorkerAsync(strArgs);
        }

        private void cmdEdit_Click(object sender, EventArgs e)
        {
            if (cboLanguages.SelectedIndex == -1)
                return;
            Cursor = Cursors.AppStarting;
            string strLanguage = cboLanguages.Text;
            frmTranslate frmOpenTranslate = s_LstOpenTranslateWindows.FirstOrDefault(x => x.Language == strLanguage);
            if (frmOpenTranslate != null)
                frmOpenTranslate.Activate();
            else
            {
                frmOpenTranslate = new frmTranslate(cboLanguages.Text);
                s_LstOpenTranslateWindows.Add(frmOpenTranslate);
                frmOpenTranslate.Show();
            }
            Cursor = Cursors.Default;
        }

        private void cmdRebuild_Click(object sender, EventArgs e)
        {
            if (cboLanguages.SelectedIndex == -1)
                return;

            Cursor = Cursors.AppStarting;
            pbProcessProgress.Value = 0;

            _strLanguageToLoad = cboLanguages.Text;

            frmTranslate frmOpenTranslate = s_LstOpenTranslateWindows.FirstOrDefault(x => x.Language == _strLanguageToLoad);
            if (frmOpenTranslate != null)
            {
                frmOpenTranslate.Close();
                s_LstOpenTranslateWindows.Remove(frmOpenTranslate);
            }

            string[] strArgs = { cboLanguages.Text.Substring(cboLanguages.Text.IndexOf('(') + 1, 5).ToLower(), _strLanguageToLoad };

            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            cmdCancel.Enabled = true;

            _workerDataProcessor.RunWorkerAsync(strArgs);
            _workerStringsProcessor.RunWorkerAsync(strArgs);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadLanguageList();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            if (!string.IsNullOrEmpty(_strLanguageToLoad))
            {
                int intParenthesesIndex = _strLanguageToLoad.IndexOf('(');
                if (intParenthesesIndex + 5 < _strLanguageToLoad.Length)
                {
                    string strCode = _strLanguageToLoad.Substring(intParenthesesIndex + 1, 5).ToLower();

                    try
                    {
                        string strPath = Path.Combine(PATH, "lang", strCode + "_data.xml");
                        if (File.Exists(strPath + ".old"))
                        {
                            if (File.Exists(strPath))
                                File.Delete(strPath);
                            File.Move(strPath, strPath + ".old");
                        }
                        strPath = Path.Combine(PATH, "lang", strCode + "xml");
                        if (File.Exists(strPath + ".old"))
                        {
                            if (File.Exists(strPath))
                                File.Delete(strPath);
                            File.Move(strPath, strPath + ".old");
                        }
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("An error was encountered while trying to restore the original translation files. Cancellation may not have been completely successful.",
                            "Backup Restoration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("An error was encountered while trying to restore the original translation files. Cancellation may not have been completely successful.",
                            "Backup Restoration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        private void txtLanguageCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && txtLanguageCode.SelectionLength == 0 && txtLanguageCode.SelectionStart == 2)
            {
                txtRegionCode.Focus();
                txtRegionCode.SelectionStart = 0;
                txtRegionCode.SelectionLength = 0;
            }
        }

        private void txtRegionCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && txtRegionCode.SelectionLength == 0 && txtRegionCode.SelectionStart == 0)
            {
                txtLanguageCode.Focus();
                txtLanguageCode.SelectionStart = 2;
                txtLanguageCode.SelectionLength = 0;
            }
        }
        #endregion Control Events

        #region BackgroundWorker Events
        private void FinishLoading(bool blnWasCancelled)
        {
            cmdCancel.Enabled = false;
            if (!blnWasCancelled && _objDataDocWithPath != null && _objStringsDocWithPath != null)
            {
                _objStringsDocWithPath.Item1.Save(_objStringsDocWithPath.Item2);
                _objDataDocWithPath.Item1.Save(_objDataDocWithPath.Item2);

                LoadLanguageList();
                frmTranslate frmOpenTranslate = new frmTranslate(_strLanguageToLoad);
                s_LstOpenTranslateWindows.Add(frmOpenTranslate);
                frmOpenTranslate.Show();
            }

            pbProcessProgress.Value = 0;
            Cursor = Cursors.Default;
        }

        private void FinishStringsProcessing(object sender, RunWorkerCompletedEventArgs e)
        {
            pbProcessProgress.PerformStep();

            if (!_workerDataProcessor.IsBusy)
            {
                FinishLoading(e.Cancelled);
            }
        }

        private Tuple<XmlDocument, string> _objStringsDocWithPath = null;
        private void DoStringsProcessing(object sender, DoWorkEventArgs e)
        {
            string[] strArgs = e.Argument as string[];
            string strFilePath = Path.Combine(PATH, "lang", strArgs[0] + ".xml");

            XmlDocument objDoc = new XmlDocument();
            if (File.Exists(strFilePath))
                objDoc.Load(strFilePath);
            XmlNode xmlRootChummerNode = objDoc.SelectSingleNode("/chummer");
            if (xmlRootChummerNode == null)
            {
                xmlRootChummerNode = objDoc.CreateElement("chummer");
                objDoc.AppendChild(xmlRootChummerNode);
            }

            XmlNode xmlNameNode = xmlRootChummerNode.SelectSingleNode("name");
            if (xmlNameNode == null)
            {
                xmlNameNode = objDoc.CreateElement("name");
                xmlNameNode.InnerText = strArgs[1];
                xmlRootChummerNode.AppendChild(xmlNameNode);
            }
            else
                _strLanguageToLoad = xmlNameNode.InnerText;

            XmlNode xmlVersionNode = xmlRootChummerNode.SelectSingleNode("version");
            if (xmlVersionNode == null)
            {
                xmlVersionNode = objDoc.CreateElement("version");
                xmlVersionNode.InnerText = "-500";
                xmlRootChummerNode.AppendChild(xmlVersionNode);
            }
            XmlNode xmlTranslatedStringsNode = xmlRootChummerNode.SelectSingleNode("strings");
            if (xmlTranslatedStringsNode == null)
            {
                xmlTranslatedStringsNode = objDoc.CreateElement("strings");
                xmlRootChummerNode.AppendChild(xmlTranslatedStringsNode);
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(PATH, "lang", "en-us.xml"));
            XmlNode xmlStringsNode = xmlDocument.SelectSingleNode("/chummer/strings");
            if (xmlStringsNode != null)
            {
                try
                {

                    foreach (XmlNode xmlStringNode in xmlStringsNode.SelectNodes("string"))
                    {
                        if (_workerStringsProcessor.CancellationPending)
                            break;
                        string strKey = xmlStringNode["key"].InnerText;
                        XmlNode xmlTranslatedStringNode = xmlTranslatedStringsNode.SelectSingleNode("string[key = \"" + strKey + "\"]");
                        if (xmlTranslatedStringNode == null)
                        {
                            xmlTranslatedStringsNode.AppendChild(objDoc.ImportNode(xmlStringNode, true));
                        }
                    }
                    foreach (XmlNode xmlTranslatedStringNode in xmlTranslatedStringsNode.SelectNodes("string"))
                    {
                        if (_workerStringsProcessor.CancellationPending)
                            break;
                        string strKey = xmlTranslatedStringNode["key"].InnerText;
                        XmlNode xmlStringNode = xmlStringsNode.SelectSingleNode("string[key = \"" + strKey + "\"]");
                        if (xmlStringNode == null)
                        {
                            xmlTranslatedStringsNode.RemoveChild(xmlTranslatedStringNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    e.Cancel = true;
                    _objDataDocWithPath = null;
                }
            }
            if (_workerStringsProcessor.CancellationPending)
            {
                e.Cancel = true;
                _objStringsDocWithPath = null;
            }
            else
            {
                _objStringsDocWithPath = new Tuple<XmlDocument, string>(objDoc, strFilePath);
            }
        }

        private void FinishDataProcessing(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!_workerStringsProcessor.IsBusy)
            {
                FinishLoading(e.Cancelled);
            }
        }

        private void StepProgressBar(object sender, ProgressChangedEventArgs e)
        {
            pbProcessProgress.PerformStep();
        }

        private Tuple<XmlDocument, string> _objDataDocWithPath = null;
        private void DoDataProcessing(object sender, DoWorkEventArgs e)
        {
            string[] strArgs = e.Argument as string[];
            string strFilePath = Path.Combine(PATH, "lang", strArgs[0] + "_data.xml");
            XmlDocument objDataDoc = new XmlDocument();
            if (File.Exists(strFilePath))
                objDataDoc.Load(strFilePath);
            XmlNode xmlRootChummerNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootChummerNode == null)
            {
                xmlRootChummerNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootChummerNode);
            }
            XmlNode xmlVersionNode = xmlRootChummerNode.SelectSingleNode("version");
            if (xmlVersionNode == null)
            {
                xmlVersionNode = objDataDoc.CreateElement("version");
                xmlVersionNode.InnerText = "-500";
                xmlRootChummerNode.AppendChild(xmlVersionNode);
            }

            int intFunctionCount = s_LstProcessFunctions.Length;
            try
            {
                for (int i = 0; i < intFunctionCount; ++i)
                {
                    if (_workerDataProcessor.CancellationPending)
                        break;
                    s_LstProcessFunctions[i].Invoke(objDataDoc, _workerDataProcessor);

                    _workerDataProcessor.ReportProgress(i * 100 / (intFunctionCount));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                e.Cancel = true;
                _objDataDocWithPath = null;
            }

            if (_workerDataProcessor.CancellationPending)
            {
                e.Cancel = true;
                _objDataDocWithPath = null;
            }
            else
            {
                _objDataDocWithPath = new Tuple<XmlDocument, string>(objDataDoc, strFilePath);
            }
        }
        #endregion

        #region Methods
        private void LoadLanguageList()
        {
            cboLanguages.Items.Clear();
            foreach (string strPath in Directory.EnumerateFiles(Path.Combine(PATH, "lang"), "*.xml"))
            {
                if (Path.GetFileNameWithoutExtension(strPath) != "en-us")
                {
                    string strInnerText = string.Empty;

                    try
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(strPath);
                        strInnerText = xmlDocument.SelectSingleNode("/chummer/name")?.InnerText;
                    }
                    catch (XmlException)
                    {
                    }

                    if (!string.IsNullOrEmpty(strInnerText))
                        cboLanguages.Items.Add(strInnerText);
                }
            }
        }
        #endregion Methods

        #region Data Processing
        private static readonly Action<XmlDocument, BackgroundWorker>[] s_LstProcessFunctions =
        {
            (x, y) => ProcessArmor(x, y),
            (x, y) => ProcessBioware(x, y),
            (x, y) => ProcessBooks(x, y),
            (x, y) => ProcessComplexForms(x, y),
            (x, y) => ProcessContacts(x, y),
            (x, y) => ProcessCritterPowers(x, y),
            (x, y) => ProcessCritters(x, y),
            (x, y) => ProcessCyberware(x, y),
            (x, y) => ProcessEchoes(x, y),
            (x, y) => ProcessGameplayOptions(x, y),
            (x, y) => ProcessGear(x, y),
            (x, y) => ProcessImprovements(x, y),
            (x, y) => ProcessLicenses(x, y),
            (x, y) => ProcessLifestyles(x, y),
            (x, y) => ProcessMartialArts(x, y),
            (x, y) => ProcessMentors(x, y),
            (x, y) => ProcessMetamagic(x, y),
            (x, y) => ProcessMetatypes(x, y),
            (x, y) => ProcessOptions(x, y),
            (x, y) => ProcessParagons(x, y),
            (x, y) => ProcessPowers(x, y),
            (x, y) => ProcessPriorities(x, y),
            (x, y) => ProcessPrograms(x, y),
            (x, y) => ProcessQualities(x, y),
            (x, y) => ProcessRanges(x, y),
            (x, y) => ProcessSkills(x, y),
            (x, y) => ProcessSpells(x, y),
            (x, y) => ProcessSpiritPowers(x, y),
            (x, y) => ProcessStreams(x, y),
            (x, y) => ProcessTraditions(x, y),
            (x, y) => ProcessVehicles(x, y),
            (x, y) => ProcessVessels(x, y),
            (x, y) => ProcessWeapons(x, y),
        };

        private static void ProcessArmor(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArmorName = xmlDataArmorNode["name"].InnerText;
                    string strDataArmorId = xmlDataArmorNode["id"].InnerText;
                    XmlNode xmlArmorNode = xmlArmorNodesParent.SelectSingleNode("armor[id=\"" + strDataArmorId + "\"]");
                    if (xmlArmorNode != null)
                    {
                        if (xmlArmorNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorId;
                            xmlArmorNode.PrependChild(xmlIdElement);
                        }

                        if (xmlArmorNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataArmorName;
                            xmlArmorNode.AppendChild(xmlNameElement);
                        }

                        if (xmlArmorNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataArmorName;
                            xmlArmorNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlArmorNode["page"];
                        if (xmlArmorNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataArmorNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlArmorNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlArmorNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataArmorNode["page"].InnerText;
                        xmlArmorNode.AppendChild(xmlPageElement);

                        xmlArmorNodesParent.AppendChild(xmlArmorNode);
                    }
                }
            }
            foreach (XmlNode xmlArmorNode in xmlArmorNodesParent.SelectNodes("armor"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlArmorNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlArmorNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlArmorNode.Attributes.RemoveAt(i);
                }
                if (xmlDataArmorNodeList?.SelectSingleNode("armor[id = \"" + xmlArmorNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArmorModId = xmlDataArmorModNode["id"].InnerText;
                    string strDataArmorModName = xmlDataArmorModNode["name"].InnerText;
                    XmlNode xmlArmorModNode = xmlArmorModNodesParent.SelectSingleNode("mod[id=\"" + strDataArmorModId + "\"]");
                    if (xmlArmorModNode != null)
                    {
                        if (xmlArmorModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArmorModId;
                            xmlArmorModNode.PrependChild(xmlIdElement);
                        }

                        if (xmlArmorModNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataArmorModName;
                            xmlArmorModNode.AppendChild(xmlNameElement);
                        }

                        if (xmlArmorModNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataArmorModName;
                            xmlArmorModNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlArmorModNode["page"];
                        if (xmlArmorModNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataArmorModNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlArmorModNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlArmorModNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataArmorModNode["page"].InnerText;
                        xmlArmorModNode.AppendChild(xmlPageElement);

                        xmlArmorModNodesParent.AppendChild(xmlArmorModNode);
                    }
                }
            }
            foreach (XmlNode xmlArmorModNode in xmlArmorModNodesParent.SelectNodes("mod"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlArmorModNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlArmorModNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlArmorModNode.Attributes.RemoveAt(i);
                }
                if (xmlDataArmorModNodeList?.SelectSingleNode("mod[id = \"" + xmlArmorModNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessBioware(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataBiowareName = xmlDataBiowareNode["name"].InnerText;
                    string strDataBiowareId = xmlDataBiowareNode["id"].InnerText;
                    XmlNode xmlBiowareNode = xmlRootBiowareFileNode.SelectSingleNode("biowares/bioware[id=\"" + strDataBiowareId + "\"]");
                    if (xmlBiowareNode != null)
                    {
                        if (xmlBiowareNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBiowareId;
                            xmlBiowareNode.PrependChild(xmlIdElement);
                        }

                        if (xmlBiowareNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataBiowareName;
                            xmlBiowareNode.AppendChild(xmlNameElement);
                        }

                        if (xmlBiowareNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataBiowareName;
                            xmlBiowareNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlBiowareNode["page"];
                        if (xmlBiowareNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataBiowareNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlBiowareNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlBiowareNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataBiowareNode["page"].InnerText;
                        xmlBiowareNode.AppendChild(xmlPageElement);

                        xmlBiowareNodesParent.AppendChild(xmlBiowareNode);
                    }
                }
            }
            foreach (XmlNode xmlBiowareNode in xmlBiowareNodesParent.SelectNodes("bioware"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlBiowareNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlBiowareNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlBiowareNode.Attributes.RemoveAt(i);
                }
                if (xmlDataBiowareNodeList?.SelectSingleNode("bioware[id = \"" + xmlBiowareNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[id=\"" + strDataGradeId + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlGradeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataGradeName;
                            xmlGradeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlGradeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataGradeName;
                            xmlGradeNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlGradeNode["page"];
                        if (xmlGradeNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataGradeNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlGradeNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlGradeNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataGradeNode["page"].InnerText;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }
            foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlGradeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlGradeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlGradeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataGradeNodeList?.SelectSingleNode("grade[id = \"" + xmlGradeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessBooks(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataBookId = xmlDataBookNode["id"].InnerText;
                    string strDataBookName = xmlDataBookNode["name"].InnerText;
                    XmlNode xmlBookNode = xmlBookNodesParent.SelectSingleNode("book[id=\"" + strDataBookId + "\"]");
                    if (xmlBookNode != null)
                    {
                        if (xmlBookNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataBookId;
                            xmlBookNode.PrependChild(xmlIdElement);
                        }

                        if (xmlBookNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataBookName;
                            xmlBookNode.AppendChild(xmlNameElement);
                        }

                        if (xmlBookNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataBookName;
                            xmlBookNode.AppendChild(xmlTranslateElement);
                        }

                        if (xmlBookNode["altcode"] == null)
                        {
                            XmlNode xmlCodeElement = objDataDoc.CreateElement("altcode");
                            xmlCodeElement.InnerText = xmlDataBookNode["code"].InnerText;
                            xmlBookNode.AppendChild(xmlCodeElement);
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

                        XmlNode xmlCodeElement = objDataDoc.CreateElement("altcode");
                        xmlCodeElement.InnerText = xmlDataBookNode["code"].InnerText;
                        xmlBookNode.AppendChild(xmlCodeElement);

                        xmlBookNodesParent.AppendChild(xmlBookNode);
                    }
                }
            }
            foreach (XmlNode xmlBookNode in xmlBookNodesParent.SelectNodes("book"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlBookNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlBookNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlBookNode.Attributes.RemoveAt(i);
                }
                if (xmlDataBookNodeList?.SelectSingleNode("book[id = \"" + xmlBookNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessComplexForms(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataComplexFormId = xmlDataComplexFormNode["id"].InnerText;
                    string strDataComplexFormName = xmlDataComplexFormNode["name"].InnerText;
                    XmlNode xmlComplexFormNode = xmlComplexFormNodesParent.SelectSingleNode("complexform[id=\"" + strDataComplexFormId + "\"]");
                    if (xmlComplexFormNode != null)
                    {
                        if (xmlComplexFormNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataComplexFormId;
                            xmlComplexFormNode.PrependChild(xmlIdElement);
                        }

                        if (xmlComplexFormNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataComplexFormName;
                            xmlComplexFormNode.AppendChild(xmlNameElement);
                        }

                        if (xmlComplexFormNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataComplexFormName;
                            xmlComplexFormNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlComplexFormNode["page"];
                        if (xmlComplexFormNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataComplexFormNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlComplexFormNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlComplexFormNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataComplexFormNode["page"].InnerText;
                        xmlComplexFormNode.AppendChild(xmlPageElement);

                        xmlComplexFormNodesParent.AppendChild(xmlComplexFormNode);
                    }
                }
            }
            foreach (XmlNode xmlComplexFormNode in xmlComplexFormNodesParent.SelectNodes("complexform"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlComplexFormNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlComplexFormNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlComplexFormNode.Attributes.RemoveAt(i);
                }
                if (xmlDataComplexFormNodeList?.SelectSingleNode("complexform[id = \"" + xmlComplexFormNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessContacts(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "contacts.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootContactFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"contacts.xml\"]");
            if (xmlRootContactFileNode == null)
            {
                xmlRootContactFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "contacts.xml";
                xmlRootContactFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootContactFileNode);
            }

            // Process Contacts

            XmlNode xmlContactNodesParent = xmlRootContactFileNode.SelectSingleNode("contacts");

            if (xmlContactNodesParent == null)
            {
                xmlContactNodesParent = objDataDoc.CreateElement("contacts");
                xmlRootContactFileNode.AppendChild(xmlContactNodesParent);
            }

            XmlNode xmlDataContactNodeList = xmlDataDocument.SelectSingleNode("/chummer/contacts");
            if (xmlDataContactNodeList != null)
            {
                foreach (XmlNode xmlDataContactNode in xmlDataContactNodeList.SelectNodes("contact"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlContactNodesParent.SelectSingleNode("contact[text()=\"" + xmlDataContactNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlContactNode = objDataDoc.CreateElement("contact");
                        xmlContactNode.InnerText = xmlDataContactNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataContactNode.InnerText;
                        xmlContactNode.Attributes.Append(xmlTranslateAttribute);
                        xmlContactNodesParent.AppendChild(xmlContactNode);
                    }
                }
            }
            foreach (XmlNode xmlContactNode in xmlContactNodesParent.SelectNodes("contact"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataContactNodeList?.SelectSingleNode("contact[text() = \"" + xmlContactNode.InnerText + "\"]") == null)
                {
                    xmlContactNodesParent.RemoveChild(xmlContactNode);
                }
            }

            // Process Sexes

            XmlNode xmlSexNodesParent = xmlRootContactFileNode.SelectSingleNode("sexes");

            if (xmlSexNodesParent == null)
            {
                xmlSexNodesParent = objDataDoc.CreateElement("sexes");
                xmlRootContactFileNode.AppendChild(xmlSexNodesParent);
            }

            XmlNode xmlDataSexNodeList = xmlDataDocument.SelectSingleNode("/chummer/sexes");
            if (xmlDataSexNodeList != null)
            {
                foreach (XmlNode xmlDataSexNode in xmlDataSexNodeList.SelectNodes("sex"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlSexNodesParent.SelectSingleNode("sex[text()=\"" + xmlDataSexNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlSexNode = objDataDoc.CreateElement("sex");
                        xmlSexNode.InnerText = xmlDataSexNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataSexNode.InnerText;
                        xmlSexNode.Attributes.Append(xmlTranslateAttribute);
                        xmlSexNodesParent.AppendChild(xmlSexNode);
                    }
                }
            }
            foreach (XmlNode xmlSexNode in xmlSexNodesParent.SelectNodes("sex"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataSexNodeList?.SelectSingleNode("sex[text() = \"" + xmlSexNode.InnerText + "\"]") == null)
                {
                    xmlSexNodesParent.RemoveChild(xmlSexNode);
                }
            }

            // Process Ages

            XmlNode xmlAgeNodesParent = xmlRootContactFileNode.SelectSingleNode("ages");

            if (xmlAgeNodesParent == null)
            {
                xmlAgeNodesParent = objDataDoc.CreateElement("ages");
                xmlRootContactFileNode.AppendChild(xmlAgeNodesParent);
            }

            XmlNode xmlDataAgeNodeList = xmlDataDocument.SelectSingleNode("/chummer/ages");
            if (xmlDataAgeNodeList != null)
            {
                foreach (XmlNode xmlDataAgeNode in xmlDataAgeNodeList.SelectNodes("age"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlAgeNodesParent.SelectSingleNode("age[text()=\"" + xmlDataAgeNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlAgeNode = objDataDoc.CreateElement("age");
                        xmlAgeNode.InnerText = xmlDataAgeNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataAgeNode.InnerText;
                        xmlAgeNode.Attributes.Append(xmlTranslateAttribute);
                        xmlAgeNodesParent.AppendChild(xmlAgeNode);
                    }
                }
            }
            foreach (XmlNode xmlAgeNode in xmlAgeNodesParent.SelectNodes("age"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataAgeNodeList?.SelectSingleNode("age[text() = \"" + xmlAgeNode.InnerText + "\"]") == null)
                {
                    xmlAgeNodesParent.RemoveChild(xmlAgeNode);
                }
            }

            // Process Personal Lives

            XmlNode xmlPersonalLifeNodesParent = xmlRootContactFileNode.SelectSingleNode("personallives");

            if (xmlPersonalLifeNodesParent == null)
            {
                xmlPersonalLifeNodesParent = objDataDoc.CreateElement("personallives");
                xmlRootContactFileNode.AppendChild(xmlPersonalLifeNodesParent);
            }

            XmlNode xmlDataPersonalLifeNodeList = xmlDataDocument.SelectSingleNode("/chummer/personallives");
            if (xmlDataPersonalLifeNodeList != null)
            {
                foreach (XmlNode xmlDataPersonalLifeNode in xmlDataPersonalLifeNodeList.SelectNodes("personallife"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlPersonalLifeNodesParent.SelectSingleNode("personallife[text()=\"" + xmlDataPersonalLifeNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlPersonalLifeNode = objDataDoc.CreateElement("personallife");
                        xmlPersonalLifeNode.InnerText = xmlDataPersonalLifeNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataPersonalLifeNode.InnerText;
                        xmlPersonalLifeNode.Attributes.Append(xmlTranslateAttribute);
                        xmlPersonalLifeNodesParent.AppendChild(xmlPersonalLifeNode);
                    }
                }
            }
            foreach (XmlNode xmlPersonalLifeNode in xmlPersonalLifeNodesParent.SelectNodes("personallife"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataPersonalLifeNodeList?.SelectSingleNode("personallife[text() = \"" + xmlPersonalLifeNode.InnerText + "\"]") == null)
                {
                    xmlPersonalLifeNodesParent.RemoveChild(xmlPersonalLifeNode);
                }
            }

            // Process Types

            XmlNode xmlTypeNodesParent = xmlRootContactFileNode.SelectSingleNode("types");

            if (xmlTypeNodesParent == null)
            {
                xmlTypeNodesParent = objDataDoc.CreateElement("types");
                xmlRootContactFileNode.AppendChild(xmlTypeNodesParent);
            }

            XmlNode xmlDataTypeNodeList = xmlDataDocument.SelectSingleNode("/chummer/types");
            if (xmlDataTypeNodeList != null)
            {
                foreach (XmlNode xmlDataTypeNode in xmlDataTypeNodeList.SelectNodes("type"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlTypeNodesParent.SelectSingleNode("type[text()=\"" + xmlDataTypeNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlTypeNode = objDataDoc.CreateElement("type");
                        xmlTypeNode.InnerText = xmlDataTypeNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataTypeNode.InnerText;
                        xmlTypeNode.Attributes.Append(xmlTranslateAttribute);
                        xmlTypeNodesParent.AppendChild(xmlTypeNode);
                    }
                }
            }
            foreach (XmlNode xmlTypeNode in xmlTypeNodesParent.SelectNodes("type"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataTypeNodeList?.SelectSingleNode("type[text() = \"" + xmlTypeNode.InnerText + "\"]") == null)
                {
                    xmlTypeNodesParent.RemoveChild(xmlTypeNode);
                }
            }

            // Process PreferredPayments

            XmlNode xmlPreferredPaymentNodesParent = xmlRootContactFileNode.SelectSingleNode("preferredpayments");

            if (xmlPreferredPaymentNodesParent == null)
            {
                xmlPreferredPaymentNodesParent = objDataDoc.CreateElement("preferredpayments");
                xmlRootContactFileNode.AppendChild(xmlPreferredPaymentNodesParent);
            }

            XmlNode xmlDataPreferredPaymentNodeList = xmlDataDocument.SelectSingleNode("/chummer/preferredpayments");
            if (xmlDataPreferredPaymentNodeList != null)
            {
                foreach (XmlNode xmlDataPreferredPaymentNode in xmlDataPreferredPaymentNodeList.SelectNodes("preferredpayment"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlPreferredPaymentNodesParent.SelectSingleNode("preferredpayment[text()=\"" + xmlDataPreferredPaymentNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlPreferredPaymentNode = objDataDoc.CreateElement("preferredpayment");
                        xmlPreferredPaymentNode.InnerText = xmlDataPreferredPaymentNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataPreferredPaymentNode.InnerText;
                        xmlPreferredPaymentNode.Attributes.Append(xmlTranslateAttribute);
                        xmlPreferredPaymentNodesParent.AppendChild(xmlPreferredPaymentNode);
                    }
                }
            }
            foreach (XmlNode xmlPreferredPaymentNode in xmlPreferredPaymentNodesParent.SelectNodes("preferredpayment"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataPreferredPaymentNodeList?.SelectSingleNode("preferredpayment[text() = \"" + xmlPreferredPaymentNode.InnerText + "\"]") == null)
                {
                    xmlPreferredPaymentNodesParent.RemoveChild(xmlPreferredPaymentNode);
                }
            }

            // Process Hobbies/Vices

            XmlNode xmlHobbyViceNodesParent = xmlRootContactFileNode.SelectSingleNode("hobbiesvices");

            if (xmlHobbyViceNodesParent == null)
            {
                xmlHobbyViceNodesParent = objDataDoc.CreateElement("hobbiesvices");
                xmlRootContactFileNode.AppendChild(xmlHobbyViceNodesParent);
            }

            XmlNode xmlDataHobbyViceNodeList = xmlDataDocument.SelectSingleNode("/chummer/hobbiesvices");
            if (xmlDataHobbyViceNodeList != null)
            {
                foreach (XmlNode xmlDataHobbyViceNode in xmlDataHobbyViceNodeList.SelectNodes("hobbyvice"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlHobbyViceNodesParent.SelectSingleNode("hobbyvice[text()=\"" + xmlDataHobbyViceNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlHobbyViceNode = objDataDoc.CreateElement("hobbyvice");
                        xmlHobbyViceNode.InnerText = xmlDataHobbyViceNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataHobbyViceNode.InnerText;
                        xmlHobbyViceNode.Attributes.Append(xmlTranslateAttribute);
                        xmlHobbyViceNodesParent.AppendChild(xmlHobbyViceNode);
                    }
                }
            }
            foreach (XmlNode xmlHobbyViceNode in xmlHobbyViceNodesParent.SelectNodes("hobbyvice"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataHobbyViceNodeList?.SelectSingleNode("hobbyvice[text() = \"" + xmlHobbyViceNode.InnerText + "\"]") == null)
                {
                    xmlHobbyViceNodesParent.RemoveChild(xmlHobbyViceNode);
                }
            }
        }

        private static void ProcessCritterPowers(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode["name"].InnerText;
                    string strDataPowerId = xmlDataPowerNode["id"].InnerText;
                    XmlNode xmlPowerNode = xmlPowerNodesParent.SelectSingleNode("power[id=\"" + strDataPowerId + "\"]");
                    if (xmlPowerNode != null)
                    {
                        if (xmlPowerNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }

                        if (xmlPowerNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlNameElement);
                        }

                        if (xmlPowerNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlPowerNode["page"];
                        if (xmlPowerNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlPowerNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlPowerNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataPowerNode["page"].InnerText;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }
            foreach (XmlNode xmlPowerNode in xmlPowerNodesParent.SelectNodes("power"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlPowerNode.Attributes.RemoveAt(i);
                }
                if (xmlDataPowerNodeList?.SelectSingleNode("power[id = \"" + xmlPowerNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessCritters(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode["name"].InnerText;
                    string strDataMetatypeId = xmlDataMetatypeNode["id"].InnerText;
                    XmlNode xmlMetatypeNode = xmlMetatypeNodesParent.SelectSingleNode("metatype[id=\"" + strDataMetatypeId + "\"]");
                    if (xmlMetatypeNode != null)
                    {
                        if (xmlMetatypeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMetatypeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMetatypeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlMetatypeNode["page"];
                        if (xmlMetatypeNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMetatypeNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMetatypeNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataMetatypeNode["page"].InnerText;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }
                }
            }
            foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodesParent.SelectNodes("metatype"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMetatypeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessCyberware(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataCyberwareName = xmlDataCyberwareNode["name"].InnerText;
                    string strDataCyberwareId = xmlDataCyberwareNode["id"].InnerText;
                    XmlNode xmlCyberwareNode = xmlRootCyberwareFileNode.SelectSingleNode("cyberwares/cyberware[id=\"" + strDataCyberwareId + "\"]");
                    if (xmlCyberwareNode != null)
                    {
                        if (xmlCyberwareNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataCyberwareId;
                            xmlCyberwareNode.PrependChild(xmlIdElement);
                        }

                        if (xmlCyberwareNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataCyberwareName;
                            xmlCyberwareNode.AppendChild(xmlNameElement);
                        }

                        if (xmlCyberwareNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataCyberwareName;
                            xmlCyberwareNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlCyberwareNode["page"];
                        if (xmlCyberwareNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataCyberwareNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlCyberwareNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlCyberwareNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataCyberwareNode["page"].InnerText;
                        xmlCyberwareNode.AppendChild(xmlPageElement);

                        xmlCyberwareNodesParent.AppendChild(xmlCyberwareNode);
                    }
                }
            }
            foreach (XmlNode xmlCyberwareNode in xmlCyberwareNodesParent.SelectNodes("cyberware"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlCyberwareNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlCyberwareNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlCyberwareNode.Attributes.RemoveAt(i);
                }
                if (xmlDataCyberwareNodeList?.SelectSingleNode("cyberware[id = \"" + xmlCyberwareNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGradeId = xmlDataGradeNode["id"].InnerText;
                    string strDataGradeName = xmlDataGradeNode["name"].InnerText;
                    XmlNode xmlGradeNode = xmlGradeNodesParent.SelectSingleNode("grade[id=\"" + strDataGradeId + "\"]");
                    if (xmlGradeNode != null)
                    {
                        if (xmlGradeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGradeId;
                            xmlGradeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlGradeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataGradeName;
                            xmlGradeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlGradeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataGradeName;
                            xmlGradeNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlGradeNode["page"];
                        if (xmlGradeNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataGradeNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlGradeNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlGradeNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataGradeNode["page"].InnerText;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }
            foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlGradeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlGradeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlGradeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataGradeNodeList?.SelectSingleNode("grade[id = \"" + xmlGradeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessEchoes(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataEchoId = xmlDataEchoNode["id"].InnerText;
                    string strDataEchoName = xmlDataEchoNode["name"].InnerText;
                    XmlNode xmlEchoNode = xmlEchoNodesParent.SelectSingleNode("echo[id=\"" + strDataEchoId + "\"]");
                    if (xmlEchoNode != null)
                    {
                        if (xmlEchoNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEchoId;
                            xmlEchoNode.PrependChild(xmlIdElement);
                        }

                        if (xmlEchoNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataEchoName;
                            xmlEchoNode.AppendChild(xmlNameElement);
                        }

                        if (xmlEchoNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataEchoName;
                            xmlEchoNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlEchoNode["page"];
                        if (xmlEchoNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataEchoNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlEchoNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlEchoNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataEchoNode["page"].InnerText;
                        xmlEchoNode.AppendChild(xmlPageElement);

                        xmlEchoNodesParent.AppendChild(xmlEchoNode);
                    }
                }
            }
            foreach (XmlNode xmlEchoNode in xmlEchoNodesParent.SelectNodes("echo"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlEchoNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlEchoNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlEchoNode.Attributes.RemoveAt(i);
                }
                if (xmlDataEchoNodeList?.SelectSingleNode("echo[id = \"" + xmlEchoNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessGameplayOptions(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "gameplayoptions.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootGameplayOptionsFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"gameplayoptions.xml\"]");
            if (xmlRootGameplayOptionsFileNode == null)
            {
                xmlRootGameplayOptionsFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "gameplayoptions.xml";
                xmlRootGameplayOptionsFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootGameplayOptionsFileNode);
            }

            // Process GameplayOptions

            XmlNode xmlGameplayOptionNodesParent = xmlRootGameplayOptionsFileNode.SelectSingleNode("gameplayoptions");
            if (xmlGameplayOptionNodesParent == null)
            {
                xmlGameplayOptionNodesParent = objDataDoc.CreateElement("gameplayoptions");
                xmlRootGameplayOptionsFileNode.AppendChild(xmlGameplayOptionNodesParent);
            }

            XmlNode xmlDataGameplayOptionNodeList = xmlDataDocument.SelectSingleNode("/chummer/gameplayoptions");
            if (xmlDataGameplayOptionNodeList != null)
            {
                foreach (XmlNode xmlDataGameplayOptionNode in xmlDataGameplayOptionNodeList.SelectNodes("gameplayoption"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGameplayOptionName = xmlDataGameplayOptionNode["name"].InnerText;
                    string strDataGameplayOptionId = xmlDataGameplayOptionNode["id"].InnerText;
                    XmlNode xmlGameplayOptionNode = xmlGameplayOptionNodesParent.SelectSingleNode("gameplayoption[id=\"" + strDataGameplayOptionId + "\"]");
                    if (xmlGameplayOptionNode != null)
                    {
                        if (xmlGameplayOptionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGameplayOptionId;
                            xmlGameplayOptionNode.PrependChild(xmlIdElement);
                        }

                        if (xmlGameplayOptionNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataGameplayOptionName;
                            xmlGameplayOptionNode.AppendChild(xmlNameElement);
                        }

                        if (xmlGameplayOptionNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataGameplayOptionName;
                            xmlGameplayOptionNode.AppendChild(xmlTranslateElement);
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

                        xmlGameplayOptionNodesParent.AppendChild(xmlGameplayOptionNode);
                    }
                }
            }
            foreach (XmlNode xmlGameplayOptionNode in xmlGameplayOptionNodesParent.SelectNodes("gameplayoption"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlGameplayOptionNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlGameplayOptionNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlGameplayOptionNode.Attributes.RemoveAt(i);
                }
                if (xmlDataGameplayOptionNodeList?.SelectSingleNode("gameplayoption[id = \"" + xmlGameplayOptionNode["id"]?.InnerText + "\"]") == null)
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
        }

        private static void ProcessGear(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "gear.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootGearFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"gear.xml\"]");
            if (xmlRootGearFileNode == null)
            {
                xmlRootGearFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "gear.xml";
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGearName = xmlDataGearNode["name"].InnerText;
                    string strDataGearId = xmlDataGearNode["id"].InnerText;
                    XmlNode xmlGearNode = xmlGearNodesParent.SelectSingleNode("gear[id=\"" + strDataGearId + "\"]");
                    if (xmlGearNode != null)
                    {
                        if (xmlGearNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataGearId;
                            xmlGearNode.PrependChild(xmlIdElement);
                        }

                        if (xmlGearNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataGearName;
                            xmlGearNode.AppendChild(xmlNameElement);
                        }

                        if (xmlGearNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataGearName;
                            xmlGearNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlGearNode["page"];
                        if (xmlGearNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataGearNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlGearNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlGearNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataGearNode["page"].InnerText;
                        xmlGearNode.AppendChild(xmlPageElement);

                        xmlGearNodesParent.AppendChild(xmlGearNode);
                    }
                }
            }
            foreach (XmlNode xmlGearNode in xmlGearNodesParent.SelectNodes("gear"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlGearNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlGearNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlGearNode.Attributes.RemoveAt(i);
                }
                if (xmlDataGearNodeList?.SelectSingleNode("gear[id = \"" + xmlGearNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessImprovements(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataImprovementId = xmlDataImprovementNode["id"].InnerText;
                    string strDataImprovementName = xmlDataImprovementNode["name"].InnerText;
                    XmlNode xmlImprovementNode = xmlImprovementNodesParent.SelectSingleNode("improvement[id=\"" + strDataImprovementId + "\"]");
                    if (xmlImprovementNode != null)
                    {
                        if (xmlImprovementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataImprovementId;
                            xmlImprovementNode.PrependChild(xmlIdElement);
                        }

                        if (xmlImprovementNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataImprovementName;
                            xmlImprovementNode.AppendChild(xmlNameElement);
                        }

                        if (xmlImprovementNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataImprovementName;
                            xmlImprovementNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlImprovementNode["page"];
                        if (xmlImprovementNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataImprovementNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlImprovementNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlImprovementNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataImprovementNode["page"].InnerText;
                        xmlImprovementNode.AppendChild(xmlPageElement);

                        xmlImprovementNodesParent.AppendChild(xmlImprovementNode);
                    }
                }
            }
            foreach (XmlNode xmlImprovementNode in xmlImprovementNodesParent.SelectNodes("improvement"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlImprovementNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlImprovementNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlImprovementNode.Attributes.RemoveAt(i);
                }
                if (xmlDataImprovementNodeList?.SelectSingleNode("improvement[id = \"" + xmlImprovementNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessLicenses(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataLicenseNodeList?.SelectSingleNode("license[text() = \"" + xmlLicenseNode.InnerText + "\"]") == null)
                {
                    xmlLicenseNodesParent.RemoveChild(xmlLicenseNode);
                }
            }
        }

        private static void ProcessLifestyles(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataLifestyleName = xmlDataLifestyleNode["name"].InnerText;
                    string strDataLifestyleId = xmlDataLifestyleNode["id"].InnerText;
                    XmlNode xmlLifestyleNode = xmlLifestyleNodesParent.SelectSingleNode("lifestyle[id=\"" + strDataLifestyleId + "\"]");
                    if (xmlLifestyleNode != null)
                    {
                        if (xmlLifestyleNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataLifestyleId;
                            xmlLifestyleNode.PrependChild(xmlIdElement);
                        }

                        if (xmlLifestyleNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataLifestyleName;
                            xmlLifestyleNode.AppendChild(xmlNameElement);
                        }

                        if (xmlLifestyleNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataLifestyleName;
                            xmlLifestyleNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlLifestyleNode["page"];
                        if (xmlLifestyleNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataLifestyleNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlLifestyleNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlLifestyleNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataLifestyleNode["page"].InnerText;
                        xmlLifestyleNode.AppendChild(xmlPageElement);

                        xmlLifestyleNodesParent.AppendChild(xmlLifestyleNode);
                    }
                }
            }
            foreach (XmlNode xmlLifestyleNode in xmlLifestyleNodesParent.SelectNodes("lifestyle"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlLifestyleNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlLifestyleNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlLifestyleNode.Attributes.RemoveAt(i);
                }
                if (xmlDataLifestyleNodeList?.SelectSingleNode("lifestyle[id = \"" + xmlLifestyleNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataQualityId = xmlDataQualityNode["id"].InnerText;
                    string strDataQualityName = xmlDataQualityNode["name"].InnerText;
                    XmlNode xmlQualityNode = xmlQualityNodesParent.SelectSingleNode("quality[id=\"" + strDataQualityId + "\"]");
                    if (xmlQualityNode != null)
                    {
                        if (xmlQualityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataQualityId;
                            xmlQualityNode.PrependChild(xmlIdElement);
                        }

                        if (xmlQualityNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataQualityName;
                            xmlQualityNode.AppendChild(xmlNameElement);
                        }

                        if (xmlQualityNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataQualityName;
                            xmlQualityNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlQualityNode["page"];
                        if (xmlQualityNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataQualityNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlQualityNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlQualityNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataQualityNode["page"].InnerText;
                        xmlQualityNode.AppendChild(xmlPageElement);

                        xmlQualityNodesParent.AppendChild(xmlQualityNode);
                    }
                }
            }
            foreach (XmlNode xmlQualityNode in xmlQualityNodesParent.SelectNodes("quality"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlQualityNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlQualityNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlQualityNode.Attributes.RemoveAt(i);
                }
                if (xmlDataQualityNodeList?.SelectSingleNode("quality[id = \"" + xmlQualityNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessMartialArts(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMartialArtName = xmlDataMartialArtNode["name"].InnerText;
                    string strDataMartialArtId = xmlDataMartialArtNode["id"].InnerText;
                    XmlNode xmlMartialArtNode = xmlRootMartialArtFileNode.SelectSingleNode("martialarts/martialart[id=\"" + strDataMartialArtId + "\"]");
                    if (xmlMartialArtNode != null)
                    {
                        if (xmlMartialArtNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMartialArtId;
                            xmlMartialArtNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMartialArtNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMartialArtName;
                            xmlMartialArtNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMartialArtNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMartialArtName;
                            xmlMartialArtNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlMartialArtNode["page"];
                        if (xmlMartialArtNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMartialArtNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMartialArtNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMartialArtNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataMartialArtNode["page"].InnerText;
                        xmlMartialArtNode.AppendChild(xmlPageElement);

                        xmlMartialArtNodesParent.AppendChild(xmlMartialArtNode);
                    }
                }
            }
            foreach (XmlNode xmlMartialArtNode in xmlMartialArtNodesParent.SelectNodes("martialart"))
            {
                if (objWorker.CancellationPending)
                    return;
                // Remove Advantages from within MartialArt
                XmlNode xmlRemoveAdvantageNode = xmlMartialArtNode.SelectSingleNode("advantages");
                if (xmlRemoveAdvantageNode != null)
                {
                    xmlMartialArtNode.RemoveChild(xmlRemoveAdvantageNode);
                }

                for (int i = xmlMartialArtNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMartialArtNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMartialArtNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMartialArtNodeList?.SelectSingleNode("martialart[id = \"" + xmlMartialArtNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTechniqueId = xmlDataTechniqueNode["id"].InnerText;
                    string strDataTechniqueName = xmlDataTechniqueNode["name"].InnerText;
                    XmlNode xmlTechniqueNode = xmlTechniqueNodesParent.SelectSingleNode("technique[id=\"" + strDataTechniqueId + "\"]");
                    if (xmlTechniqueNode != null)
                    {
                        if (xmlTechniqueNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTechniqueId;
                            xmlTechniqueNode.PrependChild(xmlIdElement);
                        }

                        if (xmlTechniqueNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataTechniqueName;
                            xmlTechniqueNode.AppendChild(xmlNameElement);
                        }

                        if (xmlTechniqueNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataTechniqueName;
                            xmlTechniqueNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlTechniqueNode["page"];
                        if (xmlTechniqueNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataTechniqueNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlTechniqueNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlTechniqueNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataTechniqueNode["page"].InnerText;
                        xmlTechniqueNode.AppendChild(xmlPageElement);

                        xmlTechniqueNodesParent.AppendChild(xmlTechniqueNode);
                    }
                }
            }
            foreach (XmlNode xmlTechniqueNode in xmlTechniqueNodesParent.SelectNodes("technique"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlTechniqueNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlTechniqueNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlTechniqueNode.Attributes.RemoveAt(i);
                }
                if (xmlDataTechniqueNodeList?.SelectSingleNode("technique[id = \"" + xmlTechniqueNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessMentors(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMentorName = xmlDataMentorNode["name"].InnerText;
                    string strDataMentorId = xmlDataMentorNode["id"].InnerText;
                    string strDataMentorAdvantage = xmlDataMentorNode["advantage"].InnerText;
                    string strDataMentorDisadvantage = xmlDataMentorNode["disadvantage"].InnerText;
                    XmlNode xmlMentorNode = xmlRootMentorFileNode.SelectSingleNode("mentors/mentor[id=\"" + strDataMentorId + "\"]");
                    if (xmlMentorNode != null)
                    {
                        if (xmlMentorNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMentorId;
                            xmlMentorNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMentorNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMentorName;
                            xmlMentorNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMentorNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMentorName;
                            xmlMentorNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlAdvantage = xmlMentorNode["advantage"];
                        if (xmlMentorNode["altadvantage"] == null)
                        {
                            XmlNode xmlAdvantageElement = objDataDoc.CreateElement("altadvantage");
                            xmlAdvantageElement.InnerText = xmlAdvantage?.InnerText ?? strDataMentorAdvantage;
                            xmlMentorNode.AppendChild(xmlAdvantageElement);
                        }
                        if (xmlAdvantage != null)
                        {
                            xmlMentorNode.RemoveChild(xmlAdvantage);
                        }

                        XmlNode xmlDisadvantage = xmlMentorNode["disadvantage"];
                        if (xmlMentorNode["altdisadvantage"] == null)
                        {
                            XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("altdisadvantage");
                            xmlDisadvantageElement.InnerText = xmlDisadvantage?.InnerText ?? strDataMentorDisadvantage;
                            xmlMentorNode.AppendChild(xmlDisadvantageElement);
                        }
                        if (xmlDisadvantage != null)
                        {
                            xmlMentorNode.RemoveChild(xmlDisadvantage);
                        }

                        XmlNode xmlPage = xmlMentorNode["page"];
                        if (xmlMentorNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMentorNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMentorNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMentorNode.RemoveChild(xmlPage);
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

                        XmlNode xmlAdvantageElement = objDataDoc.CreateElement("altadvantage");
                        xmlAdvantageElement.InnerText = strDataMentorAdvantage;
                        xmlMentorNode.AppendChild(xmlAdvantageElement);

                        XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("altdisadvantage");
                        xmlDisadvantageElement.InnerText = strDataMentorDisadvantage;
                        xmlMentorNode.AppendChild(xmlDisadvantageElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
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
                            if (objWorker.CancellationPending)
                                return;
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
                                    if (objWorker.CancellationPending)
                                        return;
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
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlMentorNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMentorNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMentorNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMentorNodeList?.SelectSingleNode("mentor[id = \"" + xmlMentorNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessMetamagic(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "metamagic.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetamagicFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"metamagic.xml\"]");
            if (xmlRootMetamagicFileNode == null)
            {
                xmlRootMetamagicFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "metamagic.xml";
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetamagicName = xmlDataMetamagicNode["name"].InnerText;
                    string strDataMetamagicId = xmlDataMetamagicNode["id"].InnerText;
                    XmlNode xmlMetamagicNode = xmlMetamagicNodesParent.SelectSingleNode("metamagic[id=\"" + strDataMetamagicId + "\"]");
                    if (xmlMetamagicNode != null)
                    {
                        if (xmlMetamagicNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetamagicId;
                            xmlMetamagicNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMetamagicNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMetamagicName;
                            xmlMetamagicNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMetamagicNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMetamagicName;
                            xmlMetamagicNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlMetamagicNode["page"];
                        if (xmlMetamagicNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMetamagicNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMetamagicNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMetamagicNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataMetamagicNode["page"].InnerText;
                        xmlMetamagicNode.AppendChild(xmlPageElement);

                        xmlMetamagicNodesParent.AppendChild(xmlMetamagicNode);
                    }
                }
            }
            foreach (XmlNode xmlMetamagicNode in xmlMetamagicNodesParent.SelectNodes("metamagic"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlMetamagicNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMetamagicNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMetamagicNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMetamagicNodeList?.SelectSingleNode("metamagic[id = \"" + xmlMetamagicNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArtId = xmlDataArtNode["id"].InnerText;
                    string strDataArtName = xmlDataArtNode["name"].InnerText;
                    XmlNode xmlArtNode = xmlArtNodesParent.SelectSingleNode("art[id=\"" + strDataArtId + "\"]");
                    if (xmlArtNode != null)
                    {
                        if (xmlArtNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataArtId;
                            xmlArtNode.PrependChild(xmlIdElement);
                        }

                        if (xmlArtNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataArtName;
                            xmlArtNode.AppendChild(xmlNameElement);
                        }

                        if (xmlArtNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataArtName;
                            xmlArtNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlArtNode["page"];
                        if (xmlArtNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataArtNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlArtNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlArtNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataArtNode["page"].InnerText;
                        xmlArtNode.AppendChild(xmlPageElement);

                        xmlArtNodesParent.AppendChild(xmlArtNode);
                    }
                }
            }
            foreach (XmlNode xmlArtNode in xmlArtNodesParent.SelectNodes("art"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlArtNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlArtNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlArtNode.Attributes.RemoveAt(i);
                }
                if (xmlDataArtNodeList?.SelectSingleNode("art[id = \"" + xmlArtNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessMetatypes(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode["name"].InnerText;
                    string strDataMetatypeId = xmlDataMetatypeNode["id"].InnerText;
                    XmlNode xmlMetatypeNode = xmlMetatypeNodesParent.SelectSingleNode("metatype[id=\"" + strDataMetatypeId + "\"]");
                    if (xmlMetatypeNode != null)
                    {
                        if (xmlMetatypeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMetatypeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMetatypeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlMetatypeNode["page"];
                        if (xmlMetatypeNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMetatypeNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMetatypeNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataMetatypeNode["page"].InnerText;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }

                    // Process Metavariants
                    AuxProcessSubItems(xmlMetatypeNode, xmlDataMetatypeNode, "metavariants", "metavariant", true, objDataDoc, objWorker);

                }
            }
            foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodesParent.SelectNodes("metatype"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMetatypeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessOptions(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "options.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootOptionFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"options.xml\"]");
            if (xmlRootOptionFileNode == null)
            {
                xmlRootOptionFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "options.xml";
                xmlRootOptionFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootOptionFileNode);
            }

            // Process Black Market Pipeline Categories

            XmlNode xmlBlackMarketPipelineCategoryNodesParent = xmlRootOptionFileNode.SelectSingleNode("blackmarketpipelinecategories");

            if (xmlBlackMarketPipelineCategoryNodesParent == null)
            {
                xmlBlackMarketPipelineCategoryNodesParent = objDataDoc.CreateElement("blackmarketpipelinecategories");
                xmlRootOptionFileNode.AppendChild(xmlBlackMarketPipelineCategoryNodesParent);
            }

            XmlNode xmlDataBlackMarketPipelineCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/blackmarketpipelinecategories");
            if (xmlDataBlackMarketPipelineCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataBlackMarketPipelineCategoryNode in xmlDataBlackMarketPipelineCategoryNodeList.SelectNodes("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlBlackMarketPipelineCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataBlackMarketPipelineCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlBlackMarketPipelineCategoryNode = objDataDoc.CreateElement("category");
                        xmlBlackMarketPipelineCategoryNode.InnerText = xmlDataBlackMarketPipelineCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataBlackMarketPipelineCategoryNode.InnerText;
                        xmlBlackMarketPipelineCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlBlackMarketPipelineCategoryNodesParent.AppendChild(xmlBlackMarketPipelineCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlBlackMarketPipelineCategoryNode in xmlBlackMarketPipelineCategoryNodesParent.SelectNodes("category"))
            {
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataBlackMarketPipelineCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlBlackMarketPipelineCategoryNode.InnerText + "\"]") == null)
                {
                    xmlBlackMarketPipelineCategoryNodesParent.RemoveChild(xmlBlackMarketPipelineCategoryNode);
                }
            }

            // Process Limb Counts

            XmlNode xmlLimbCountNodesParent = xmlRootOptionFileNode.SelectSingleNode("limbcounts");
            if (xmlLimbCountNodesParent == null)
            {
                xmlLimbCountNodesParent = objDataDoc.CreateElement("limbcounts");
                xmlRootOptionFileNode.AppendChild(xmlLimbCountNodesParent);
            }

            XmlNode xmlDataLimbCountsNodeList = xmlDataDocument.SelectSingleNode("/chummer/limbcounts");
            if (xmlDataLimbCountsNodeList != null)
            {
                foreach (XmlNode xmlDataLimbOptionNode in xmlDataLimbCountsNodeList.SelectNodes("limb"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataLimbOptionName = xmlDataLimbOptionNode["name"].InnerText;
                    XmlNode xmlLimbOptionNode = xmlLimbCountNodesParent.SelectSingleNode("limb[name=\"" + strDataLimbOptionName + "\"]");
                    if (xmlLimbOptionNode != null)
                    {
                        if (xmlLimbOptionNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataLimbOptionName;
                            xmlLimbOptionNode.AppendChild(xmlNameElement);
                        }

                        if (xmlLimbOptionNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataLimbOptionName;
                            xmlLimbOptionNode.AppendChild(xmlTranslateElement);
                        }
                    }
                    else
                    {
                        xmlLimbOptionNode = objDataDoc.CreateElement("limb");

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataLimbOptionName;
                        xmlLimbOptionNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataLimbOptionName;
                        xmlLimbOptionNode.AppendChild(xmlTranslateElement);

                        xmlLimbCountNodesParent.AppendChild(xmlLimbOptionNode);
                    }
                }
            }
            foreach (XmlNode xmlLimbOptionNode in xmlLimbCountNodesParent.SelectNodes("limb"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlLimbOptionNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlLimbOptionNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlLimbOptionNode.Attributes.RemoveAt(i);
                }
                if (xmlDataLimbCountsNodeList?.SelectSingleNode("limb[name = \"" + xmlLimbOptionNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlLimbOptionNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlLimbCountNodesParent.RemoveChild(xmlLimbOptionNode);
                    }
#endif
                }
            }

            // Process PDF Options

            XmlNode xmlPDFArgumentNodesParent = xmlRootOptionFileNode.SelectSingleNode("pdfarguments");
            if (xmlPDFArgumentNodesParent == null)
            {
                xmlPDFArgumentNodesParent = objDataDoc.CreateElement("pdfarguments");
                xmlRootOptionFileNode.AppendChild(xmlPDFArgumentNodesParent);
            }

            XmlNode xmlDataPDFArgumentsNodeList = xmlDataDocument.SelectSingleNode("/chummer/pdfarguments");
            if (xmlDataPDFArgumentsNodeList != null)
            {
                foreach (XmlNode xmlDataPDFArgumentNode in xmlDataPDFArgumentsNodeList.SelectNodes("pdfargument"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPDFArgumentName = xmlDataPDFArgumentNode["name"].InnerText;
                    XmlNode xmlPDFArgumentNode = xmlPDFArgumentNodesParent.SelectSingleNode("pdfargument[name=\"" + strDataPDFArgumentName + "\"]");
                    if (xmlPDFArgumentNode != null)
                    {
                        if (xmlPDFArgumentNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataPDFArgumentName;
                            xmlPDFArgumentNode.AppendChild(xmlNameElement);
                        }

                        if (xmlPDFArgumentNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataPDFArgumentName;
                            xmlPDFArgumentNode.AppendChild(xmlTranslateElement);
                        }
                    }
                    else
                    {
                        xmlPDFArgumentNode = objDataDoc.CreateElement("pdfargument");

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPDFArgumentName;
                        xmlPDFArgumentNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPDFArgumentName;
                        xmlPDFArgumentNode.AppendChild(xmlTranslateElement);

                        xmlPDFArgumentNodesParent.AppendChild(xmlPDFArgumentNode);
                    }
                }
            }
            foreach (XmlNode xmlPDFArgumentNode in xmlPDFArgumentNodesParent.SelectNodes("pdfargument"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlPDFArgumentNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlPDFArgumentNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlPDFArgumentNode.Attributes.RemoveAt(i);
                }
                if (xmlDataPDFArgumentsNodeList?.SelectSingleNode("pdfargument[name = \"" + xmlPDFArgumentNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPDFArgumentNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlPDFArgumentNodesParent.RemoveChild(xmlPDFArgumentNode);
                    }
#endif
                }
            }
        }

        private static void ProcessParagons(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "paragons.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootParagonFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"paragons.xml\"]");
            if (xmlRootParagonFileNode == null)
            {
                xmlRootParagonFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "paragons.xml";
                xmlRootParagonFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootParagonFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootParagonFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootParagonFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XmlNode xmlDataCategoryNodeList = xmlDataDocument.SelectSingleNode("/chummer/categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XmlNode xmlDataCategoryNode in xmlDataCategoryNodeList.SelectNodes("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
                if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                {
                    xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                }
            }

            // Process Paragons

            XmlNode xmlParagonNodesParent = xmlRootParagonFileNode.SelectSingleNode("mentors");
            if (xmlParagonNodesParent == null)
            {
                xmlParagonNodesParent = objDataDoc.CreateElement("mentors");
                xmlRootParagonFileNode.AppendChild(xmlParagonNodesParent);
            }

            XmlNode xmlDataParagonNodeList = xmlDataDocument.SelectSingleNode("/chummer/mentors");
            if (xmlDataParagonNodeList != null)
            {
                foreach (XmlNode xmlDataParagonNode in xmlDataParagonNodeList.SelectNodes("mentor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataParagonName = xmlDataParagonNode["name"].InnerText;
                    string strDataParagonId = xmlDataParagonNode["id"].InnerText;
                    string strDataParagonAdvantage = xmlDataParagonNode["advantage"].InnerText;
                    string strDataParagonDisadvantage = xmlDataParagonNode["disadvantage"].InnerText;
                    XmlNode xmlParagonNode = xmlRootParagonFileNode.SelectSingleNode("mentors/mentor[id=\"" + strDataParagonId + "\"]");
                    if (xmlParagonNode != null)
                    {
                        if (xmlParagonNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataParagonId;
                            xmlParagonNode.PrependChild(xmlIdElement);
                        }

                        if (xmlParagonNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataParagonName;
                            xmlParagonNode.AppendChild(xmlNameElement);
                        }

                        if (xmlParagonNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataParagonName;
                            xmlParagonNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlAdvantage = xmlParagonNode["advantage"];
                        if (xmlParagonNode["altadvantage"] == null)
                        {
                            XmlNode xmlAdvantageElement = objDataDoc.CreateElement("altadvantage");
                            xmlAdvantageElement.InnerText = xmlAdvantage?.InnerText ?? strDataParagonAdvantage;
                            xmlParagonNode.AppendChild(xmlAdvantageElement);
                        }
                        if (xmlAdvantage != null)
                        {
                            xmlParagonNode.RemoveChild(xmlAdvantage);
                        }

                        XmlNode xmlDisadvantage = xmlParagonNode["disadvantage"];
                        if (xmlParagonNode["altdisadvantage"] == null)
                        {
                            XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("altdisadvantage");
                            xmlDisadvantageElement.InnerText = xmlDisadvantage?.InnerText ?? strDataParagonDisadvantage;
                            xmlParagonNode.AppendChild(xmlDisadvantageElement);
                        }
                        if (xmlDisadvantage != null)
                        {
                            xmlParagonNode.RemoveChild(xmlDisadvantage);
                        }

                        XmlNode xmlPage = xmlParagonNode["page"];
                        if (xmlParagonNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataParagonNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlParagonNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlParagonNode.RemoveChild(xmlPage);
                        }
                    }
                    else
                    {
                        xmlParagonNode = objDataDoc.CreateElement("mentor");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataParagonId;
                        xmlParagonNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataParagonName;
                        xmlParagonNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataParagonName;
                        xmlParagonNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlAdvantageElement = objDataDoc.CreateElement("altadvantage");
                        xmlAdvantageElement.InnerText = strDataParagonAdvantage;
                        xmlParagonNode.AppendChild(xmlAdvantageElement);

                        XmlNode xmlDisadvantageElement = objDataDoc.CreateElement("altdisadvantage");
                        xmlDisadvantageElement.InnerText = strDataParagonDisadvantage;
                        xmlParagonNode.AppendChild(xmlDisadvantageElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataParagonNode["page"].InnerText;
                        xmlParagonNode.AppendChild(xmlPageElement);

                        xmlParagonNodesParent.AppendChild(xmlParagonNode);
                    }

                    XmlNode xmlDataParagonChoicesNode = xmlDataParagonNode["choices"];
                    if (xmlDataParagonChoicesNode != null)
                    {
                        XmlNode xmlParagonChoicesNode = xmlParagonNode["choices"];
                        if (xmlParagonChoicesNode == null)
                        {
                            xmlParagonChoicesNode = objDataDoc.CreateElement("choices");
                            xmlParagonNode.AppendChild(xmlParagonChoicesNode);
                        }

                        foreach (XmlNode xmlDataChoiceNode in xmlDataParagonChoicesNode.SelectNodes("choice"))
                        {
                            if (objWorker.CancellationPending)
                                return;
                            string strDataChoiceName = xmlDataChoiceNode["name"]?.InnerText ?? string.Empty;
                            XmlNode xmlChoiceNode = xmlParagonChoicesNode.SelectSingleNode("choice[name=\"" + strDataChoiceName + "\"]");
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
                                    if (objWorker.CancellationPending)
                                        return;
                                    XmlAttribute xmlChoiceNodeAttribute = objDataDoc.CreateAttribute(xmlDataChoiceNodeAttribute.Name);
                                    xmlChoiceNodeAttribute.Value = xmlDataChoiceNodeAttribute.InnerText;
                                    xmlChoiceNode.Attributes.Append(xmlChoiceNodeAttribute);
                                }

                                xmlParagonChoicesNode.AppendChild(xmlChoiceNode);
                            }
                        }
                    }
                }
            }
            foreach (XmlNode xmlParagonNode in xmlParagonNodesParent.SelectNodes("mentor"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlParagonNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlParagonNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlParagonNode.Attributes.RemoveAt(i);
                }
                if (xmlDataParagonNodeList?.SelectSingleNode("mentor[id = \"" + xmlParagonNode["id"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlParagonNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlParagonNodesParent.RemoveChild(xmlParagonNode);
                    }
#endif
                }
            }

            // Remove Paragon nodes

            XmlNode xmlRemoveNode = xmlRootParagonFileNode.SelectSingleNode("paragons");
            if (xmlRemoveNode != null)
            {
                xmlRootParagonFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessPowers(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode["name"].InnerText;
                    string strDataPowerId = xmlDataPowerNode["id"].InnerText;
                    XmlNode xmlPowerNode = xmlPowerNodesParent.SelectSingleNode("power[id=\"" + strDataPowerId + "\"]");
                    if (xmlPowerNode != null)
                    {
                        if (xmlPowerNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPowerId;
                            xmlPowerNode.PrependChild(xmlIdElement);
                        }

                        if (xmlPowerNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlNameElement);
                        }

                        if (xmlPowerNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlPowerNode["page"];
                        if (xmlPowerNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlPowerNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlPowerNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataPowerNode["page"].InnerText;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }
            foreach (XmlNode xmlPowerNode in xmlPowerNodesParent.SelectNodes("power"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlPowerNode.Attributes.RemoveAt(i);
                }
                if (xmlDataPowerNodeList?.SelectSingleNode("power[id = \"" + xmlPowerNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataEnhancementId = xmlDataEnhancementNode["id"].InnerText;
                    string strDataEnhancementName = xmlDataEnhancementNode["name"].InnerText;
                    XmlNode xmlEnhancementNode = xmlEnhancementNodesParent.SelectSingleNode("enhancement[id=\"" + strDataEnhancementId + "\"]");
                    if (xmlEnhancementNode != null)
                    {
                        if (xmlEnhancementNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataEnhancementId;
                            xmlEnhancementNode.PrependChild(xmlIdElement);
                        }

                        if (xmlEnhancementNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataEnhancementName;
                            xmlEnhancementNode.AppendChild(xmlNameElement);
                        }

                        if (xmlEnhancementNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataEnhancementName;
                            xmlEnhancementNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlEnhancementNode["page"];
                        if (xmlEnhancementNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataEnhancementNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlEnhancementNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlEnhancementNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataEnhancementNode["page"].InnerText;
                        xmlEnhancementNode.AppendChild(xmlPageElement);

                        xmlEnhancementNodesParent.AppendChild(xmlEnhancementNode);
                    }
                }
            }
            foreach (XmlNode xmlEnhancementNode in xmlEnhancementNodesParent.SelectNodes("enhancement"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlEnhancementNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlEnhancementNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlEnhancementNode.Attributes.RemoveAt(i);
                }
                if (xmlDataEnhancementNodeList?.SelectSingleNode("enhancement[id = \"" + xmlEnhancementNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessPriorities(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPriorityName = xmlDataPriorityNode["name"].InnerText;
                    string strDataPriorityId = xmlDataPriorityNode["id"].InnerText;
                    XmlNode xmlPriorityNode = xmlRootPriorityFileNode.SelectSingleNode("priorities/priority[id=\"" + strDataPriorityId + "\"]");
                    if (xmlPriorityNode != null)
                    {
                        if (xmlPriorityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataPriorityId;
                            xmlPriorityNode.PrependChild(xmlIdElement);
                        }

                        if (xmlPriorityNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataPriorityName;
                            xmlPriorityNode.AppendChild(xmlNameElement);
                        }

                        if (xmlPriorityNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataPriorityName;
                            xmlPriorityNode.AppendChild(xmlTranslateElement);
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

                        xmlPriorityNodesParent.AppendChild(xmlPriorityNode);
                    }

                    // Process Talents
                    AuxProcessSubItems(xmlPriorityNode, xmlDataPriorityNode, "talents", "talent", false, objDataDoc, objWorker);

                }
            }
            foreach (XmlNode xmlPriorityNode in xmlPriorityNodesParent.SelectNodes("priority"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlPriorityNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlPriorityNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlPriorityNode.Attributes.RemoveAt(i);
                }
                if (xmlDataPriorityNodeList?.SelectSingleNode("priority[id = \"" + xmlPriorityNode["id"]?.InnerText + "\"]") == null)
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

            // Remove Gameplay Options

            XmlNode xmlRemoveNode = xmlRootPriorityFileNode.SelectSingleNode("gameplayoptions");
            if (xmlRemoveNode != null)
            {
                xmlRootPriorityFileNode.RemoveChild(xmlRemoveNode);
            }

            // Remove Maneuvers

            xmlRemoveNode = xmlRootPriorityFileNode.SelectSingleNode("maneuvers");
            if (xmlRemoveNode != null)
            {
                xmlRootPriorityFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessPrograms(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataProgramName = xmlDataProgramNode["name"].InnerText;
                    string strDataProgramId = xmlDataProgramNode["id"].InnerText;
                    XmlNode xmlProgramNode = xmlProgramNodesParent.SelectSingleNode("program[id=\"" + strDataProgramId + "\"]");
                    if (xmlProgramNode != null)
                    {
                        if (xmlProgramNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataProgramId;
                            xmlProgramNode.PrependChild(xmlIdElement);
                        }

                        if (xmlProgramNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataProgramName;
                            xmlProgramNode.AppendChild(xmlNameElement);
                        }

                        if (xmlProgramNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataProgramName;
                            xmlProgramNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlProgramNode["page"];
                        if (xmlProgramNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataProgramNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlProgramNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlProgramNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataProgramNode["page"].InnerText;
                        xmlProgramNode.AppendChild(xmlPageElement);

                        xmlProgramNodesParent.AppendChild(xmlProgramNode);
                    }
                }
            }
            foreach (XmlNode xmlProgramNode in xmlProgramNodesParent.SelectNodes("program"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlProgramNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlProgramNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlProgramNode.Attributes.RemoveAt(i);
                }
                if (xmlDataProgramNodeList?.SelectSingleNode("program[id = \"" + xmlProgramNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessRanges(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "ranges.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootRangeFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"ranges.xml\"]");
            if (xmlRootRangeFileNode == null)
            {
                xmlRootRangeFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "ranges.xml";
                xmlRootRangeFileNode.Attributes.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootRangeFileNode);
            }

            // Process Ranges

            XmlNode xmlRangeNodesParent = xmlRootRangeFileNode.SelectSingleNode("ranges");
            if (xmlRangeNodesParent == null)
            {
                xmlRangeNodesParent = objDataDoc.CreateElement("ranges");
                xmlRootRangeFileNode.AppendChild(xmlRangeNodesParent);
            }

            XmlNode xmlDataRangeNodeList = xmlDataDocument.SelectSingleNode("/chummer/ranges");
            if (xmlDataRangeNodeList != null)
            {
                foreach (XmlNode xmlDataRangeNode in xmlDataRangeNodeList.SelectNodes("range"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataRangeName = xmlDataRangeNode["name"].InnerText;
                    XmlNode xmlRangeNode = xmlRangeNodesParent.SelectSingleNode("range[name = \"" + strDataRangeName + "\"]");
                    if (xmlRangeNode != null)
                    {
                        if (xmlRangeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataRangeName;
                            xmlRangeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlRangeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataRangeName;
                            xmlRangeNode.AppendChild(xmlTranslateElement);
                        }
                    }
                    else
                    {
                        xmlRangeNode = objDataDoc.CreateElement("range");

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataRangeName;
                        xmlRangeNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataRangeName;
                        xmlRangeNode.AppendChild(xmlTranslateElement);

                        xmlRangeNodesParent.AppendChild(xmlRangeNode);
                    }
                }
            }
            foreach (XmlNode xmlRangeNode in xmlRangeNodesParent.SelectNodes("range"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlRangeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlRangeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlRangeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataRangeNodeList?.SelectSingleNode("range[name = \"" + xmlRangeNode["name"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlRangeNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlRangeNodesParent.RemoveChild(xmlRangeNode);
                    }
#endif
                }
            }
        }

        private static void ProcessQualities(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataQualityName = xmlDataQualityNode["name"].InnerText;
                    string strDataQualityId = xmlDataQualityNode["id"].InnerText;
                    XmlNode xmlQualityNode = xmlQualityNodesParent.SelectSingleNode("quality[id=\"" + strDataQualityId + "\"]");
                    if (xmlQualityNode != null)
                    {
                        if (xmlQualityNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataQualityId;
                            xmlQualityNode.PrependChild(xmlIdElement);
                        }

                        if (xmlQualityNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataQualityName;
                            xmlQualityNode.AppendChild(xmlNameElement);
                        }

                        if (xmlQualityNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataQualityName;
                            xmlQualityNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlQualityNode["page"];
                        if (xmlQualityNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataQualityNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlQualityNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlQualityNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataQualityNode["page"].InnerText;
                        xmlQualityNode.AppendChild(xmlPageElement);

                        xmlQualityNodesParent.AppendChild(xmlQualityNode);
                    }
                }
            }
            foreach (XmlNode xmlQualityNode in xmlQualityNodesParent.SelectNodes("quality"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlQualityNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlQualityNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlQualityNode.Attributes.RemoveAt(i);
                }
                if (xmlDataQualityNodeList?.SelectSingleNode("quality[id = \"" + xmlQualityNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessSkills(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSkillName = xmlDataSkillNode["name"].InnerText;
                    string strDataSkillId = xmlDataSkillNode["id"].InnerText;
                    XmlNode xmlSkillNode = xmlRootSkillFileNode.SelectSingleNode("skills/skill[id=\"" + strDataSkillId + "\"]");
                    if (xmlSkillNode != null)
                    {
                        if (xmlSkillNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSkillId;
                            xmlSkillNode.PrependChild(xmlIdElement);
                        }

                        if (xmlSkillNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataSkillName;
                            xmlSkillNode.AppendChild(xmlNameElement);
                        }

                        if (xmlSkillNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataSkillName;
                            xmlSkillNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlSkillNode["page"];
                        if (xmlSkillNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataSkillNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlSkillNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlSkillNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
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
                        if (objWorker.CancellationPending)
                            return;
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
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlSkillNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlSkillNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlSkillNode.Attributes.RemoveAt(i);
                }
                XmlNode xmlDataSkillNode = xmlDataSkillNodeList?.SelectSingleNode("skill[id = \"" + xmlSkillNode["id"]?.InnerText + "\"]");
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
                        for (int i = xmlSkillNodeSpecsParent.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlSkillNodeSpecsParent.Attributes[i];
                            if (xmlAttribute.Name != "translated")
                                xmlSkillNodeSpecsParent.Attributes.RemoveAt(i);
                        }
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
                                if (objWorker.CancellationPending)
                                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataKnowledgeSkillId = xmlDataKnowledgeSkillNode["id"].InnerText;
                    string strDataKnowledgeSkillName = xmlDataKnowledgeSkillNode["name"].InnerText;
                    XmlNode xmlKnowledgeSkillNode = xmlKnowledgeSkillNodesParent.SelectSingleNode("skill[id=\"" + strDataKnowledgeSkillId + "\"]");
                    if (xmlKnowledgeSkillNode != null)
                    {
                        if (xmlKnowledgeSkillNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataKnowledgeSkillId;
                            xmlKnowledgeSkillNode.PrependChild(xmlIdElement);
                        }

                        if (xmlKnowledgeSkillNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataKnowledgeSkillName;
                            xmlKnowledgeSkillNode.AppendChild(xmlNameElement);
                        }

                        if (xmlKnowledgeSkillNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataKnowledgeSkillName;
                            xmlKnowledgeSkillNode.AppendChild(xmlTranslateElement);
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
                        if (objWorker.CancellationPending)
                            return;
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
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlKnowledgeSkillNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlKnowledgeSkillNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlKnowledgeSkillNode.Attributes.RemoveAt(i);
                }
                XmlNode xmlDataKnowledgeSkillNode = xmlDataKnowledgeSkillNodeList?.SelectSingleNode("skill[id = \"" + xmlKnowledgeSkillNode["id"]?.InnerText + "\"]");
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
                        for (int i = xmlSkillNodeSpecsParent.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlSkillNodeSpecsParent.Attributes[i];
                            if (xmlAttribute.Name != "translated")
                                xmlSkillNodeSpecsParent.Attributes.RemoveAt(i);
                        }
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
                                if (objWorker.CancellationPending)
                                    return;
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

        private static void ProcessSpells(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpellName = xmlDataSpellNode["name"].InnerText;
                    string strDataSpellId = xmlDataSpellNode["id"].InnerText;
                    XmlNode xmlSpellNode = xmlSpellNodesParent.SelectSingleNode("spell[id=\"" + strDataSpellId + "\"]");
                    if (xmlSpellNode != null)
                    {
                        if (xmlSpellNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpellId;
                            xmlSpellNode.PrependChild(xmlIdElement);
                        }

                        if (xmlSpellNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataSpellName;
                            xmlSpellNode.AppendChild(xmlNameElement);
                        }

                        if (xmlSpellNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataSpellName;
                            xmlSpellNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlSpellNode["page"];
                        if (xmlSpellNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataSpellNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlSpellNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlSpellNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataSpellNode["page"].InnerText;
                        xmlSpellNode.AppendChild(xmlPageElement);

                        xmlSpellNodesParent.AppendChild(xmlSpellNode);
                    }
                }
            }
            foreach (XmlNode xmlSpellNode in xmlSpellNodesParent.SelectNodes("spell"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlSpellNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlSpellNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlSpellNode.Attributes.RemoveAt(i);
                }
                if (xmlDataSpellNodeList?.SelectSingleNode("spell[id = \"" + xmlSpellNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessSpiritPowers(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "spiritpowers.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootPowerFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"spiritpowers.xml\"]");
            if (xmlRootPowerFileNode == null)
            {
                xmlRootPowerFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "spiritpowers.xml";
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode["name"].InnerText;
                    XmlNode xmlPowerNode = xmlPowerNodesParent.SelectSingleNode("power[name=\"" + strDataPowerName + "\"]");
                    if (xmlPowerNode != null)
                    {
                        if (xmlPowerNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlNameElement);
                        }

                        if (xmlPowerNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataPowerName;
                            xmlPowerNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlPowerNode["page"];
                        if (xmlPowerNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlPowerNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlPowerNode.RemoveChild(xmlPage);
                        }
                    }
                    else
                    {
                        xmlPowerNode = objDataDoc.CreateElement("power");

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataPowerName;
                        xmlPowerNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataPowerNode["page"].InnerText;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }
            foreach (XmlNode xmlPowerNode in xmlPowerNodesParent.SelectNodes("power"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlPowerNode.Attributes.RemoveAt(i);
                }
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

        private static void ProcessStreams(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTraditionName = xmlDataTraditionNode["name"].InnerText;
                    string strDataTraditionId = xmlDataTraditionNode["id"].InnerText;
                    XmlNode xmlTraditionNode = xmlTraditionNodesParent.SelectSingleNode("tradition[id=\"" + strDataTraditionId + "\"]");
                    if (xmlTraditionNode != null)
                    {
                        if (xmlTraditionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }

                        if (xmlTraditionNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataTraditionName;
                            xmlTraditionNode.AppendChild(xmlNameElement);
                        }

                        if (xmlTraditionNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataTraditionName;
                            xmlTraditionNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlTraditionNode["page"];
                        if (xmlTraditionNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataTraditionNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlTraditionNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlTraditionNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataTraditionNode["page"].InnerText;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }
            foreach (XmlNode xmlTraditionNode in xmlTraditionNodesParent.SelectNodes("tradition"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlTraditionNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlTraditionNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlTraditionNode.Attributes.RemoveAt(i);
                }
                if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[id = \"" + xmlTraditionNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[id=\"" + strDataSpiritId + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }

                        if (xmlSpiritNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataSpiritName;
                            xmlSpiritNode.AppendChild(xmlNameElement);
                        }

                        if (xmlSpiritNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataSpiritName;
                            xmlSpiritNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlSpiritNode["page"];
                        if (xmlSpiritNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataSpiritNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlSpiritNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlSpiritNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataSpiritNode["page"].InnerText;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }
            foreach (XmlNode xmlSpiritNode in xmlSpiritNodesParent.SelectNodes("spirit"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlSpiritNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlSpiritNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlSpiritNode.Attributes.RemoveAt(i);
                }
                if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[id = \"" + xmlSpiritNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessTraditions(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTraditionName = xmlDataTraditionNode["name"].InnerText;
                    string strDataTraditionId = xmlDataTraditionNode["id"].InnerText;
                    XmlNode xmlTraditionNode = xmlTraditionNodesParent.SelectSingleNode("tradition[id=\"" + strDataTraditionId + "\"]");
                    if (xmlTraditionNode != null)
                    {
                        if (xmlTraditionNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTraditionId;
                            xmlTraditionNode.PrependChild(xmlIdElement);
                        }

                        if (xmlTraditionNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataTraditionName;
                            xmlTraditionNode.AppendChild(xmlNameElement);
                        }

                        if (xmlTraditionNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataTraditionName;
                            xmlTraditionNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlTraditionNode["page"];
                        if (xmlTraditionNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataTraditionNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlTraditionNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlTraditionNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataTraditionNode["page"].InnerText;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }
            foreach (XmlNode xmlTraditionNode in xmlTraditionNodesParent.SelectNodes("tradition"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlTraditionNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlTraditionNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlTraditionNode.Attributes.RemoveAt(i);
                }
                if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[id = \"" + xmlTraditionNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpiritId = xmlDataSpiritNode["id"].InnerText;
                    string strDataSpiritName = xmlDataSpiritNode["name"].InnerText;
                    XmlNode xmlSpiritNode = xmlSpiritNodesParent.SelectSingleNode("spirit[id=\"" + strDataSpiritId + "\"]");
                    if (xmlSpiritNode != null)
                    {
                        if (xmlSpiritNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataSpiritId;
                            xmlSpiritNode.PrependChild(xmlIdElement);
                        }

                        if (xmlSpiritNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataSpiritName;
                            xmlSpiritNode.AppendChild(xmlNameElement);
                        }

                        if (xmlSpiritNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataSpiritName;
                            xmlSpiritNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlSpiritNode["page"];
                        if (xmlSpiritNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataSpiritNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlSpiritNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlSpiritNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataSpiritNode["page"].InnerText;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }
            foreach (XmlNode xmlSpiritNode in xmlSpiritNodesParent.SelectNodes("spirit"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlSpiritNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlSpiritNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlSpiritNode.Attributes.RemoveAt(i);
                }
                if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[id = \"" + xmlSpiritNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataDrainAttributeId = xmlDataDrainAttributeNode["id"].InnerText;
                    string strDataDrainAttributeName = xmlDataDrainAttributeNode["name"].InnerText;
                    XmlNode xmlDrainAttributeNode = xmlDrainAttributeNodesParent.SelectSingleNode("drainattribute[id=\"" + strDataDrainAttributeId + "\"]");
                    if (xmlDrainAttributeNode != null)
                    {
                        if (xmlDrainAttributeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataDrainAttributeId;
                            xmlDrainAttributeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlDrainAttributeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataDrainAttributeName;
                            xmlDrainAttributeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlDrainAttributeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataDrainAttributeName;
                            xmlDrainAttributeNode.AppendChild(xmlTranslateElement);
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

                        xmlDrainAttributeNodesParent.AppendChild(xmlDrainAttributeNode);
                    }
                }
            }
            foreach (XmlNode xmlDrainAttributeNode in xmlDrainAttributeNodesParent.SelectNodes("drainattribute"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlDrainAttributeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlDrainAttributeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlDrainAttributeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataDrainAttributeNodeList?.SelectSingleNode("drainattribute[id = \"" + xmlDrainAttributeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessVehicles(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlModCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.InnerText + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.InnerText;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.InnerText;
                        xmlCategoryNode.Attributes.Append(xmlTranslateAttribute);
                        xmlModCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }
            foreach (XmlNode xmlModCategoryNode in xmlModCategoryNodesParent.SelectNodes("category"))
            {
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataVehicleName = xmlDataVehicleNode["name"].InnerText;
                    string strDataVehicleId = xmlDataVehicleNode["id"].InnerText;
                    XmlNode xmlVehicleNode = xmlVehicleNodesParent.SelectSingleNode("vehicle[id=\"" + strDataVehicleId + "\"]");
                    if (xmlVehicleNode != null)
                    {
                        if (xmlVehicleNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataVehicleId;
                            xmlVehicleNode.PrependChild(xmlIdElement);
                        }

                        if (xmlVehicleNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataVehicleName;
                            xmlVehicleNode.AppendChild(xmlNameElement);
                        }

                        if (xmlVehicleNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataVehicleName;
                            xmlVehicleNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlVehicleNode["page"];
                        if (xmlVehicleNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataVehicleNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlVehicleNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlVehicleNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataVehicleNode["page"].InnerText;
                        xmlVehicleNode.AppendChild(xmlPageElement);

                        xmlVehicleNodesParent.AppendChild(xmlVehicleNode);
                    }
                }
            }
            foreach (XmlNode xmlVehicleNode in xmlVehicleNodesParent.SelectNodes("vehicle"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlVehicleNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlVehicleNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlVehicleNode.Attributes.RemoveAt(i);
                }
                if (xmlDataVehicleNodeList?.SelectSingleNode("vehicle[id = \"" + xmlVehicleNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataVehicleModId = xmlDataVehicleModNode["id"].InnerText;
                    string strDataVehicleModName = xmlDataVehicleModNode["name"].InnerText;
                    XmlNode xmlVehicleModNode = xmlVehicleModNodesParent.SelectSingleNode("mod[id=\"" + strDataVehicleModId + "\"]");
                    if (xmlVehicleModNode != null)
                    {
                        if (xmlVehicleModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataVehicleModId;
                            xmlVehicleModNode.PrependChild(xmlIdElement);
                        }

                        if (xmlVehicleModNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataVehicleModName;
                            xmlVehicleModNode.AppendChild(xmlNameElement);
                        }

                        if (xmlVehicleModNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataVehicleModName;
                            xmlVehicleModNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlVehicleModNode["page"];
                        if (xmlVehicleModNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataVehicleModNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlVehicleModNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlVehicleModNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataVehicleModNode["page"].InnerText;
                        xmlVehicleModNode.AppendChild(xmlPageElement);

                        xmlVehicleModNodesParent.AppendChild(xmlVehicleModNode);
                    }
                }
            }
            foreach (XmlNode xmlVehicleModNode in xmlVehicleModNodesParent.SelectNodes("mod"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlVehicleModNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlVehicleModNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlVehicleModNode.Attributes.RemoveAt(i);
                }
                if (xmlDataVehicleModNodeList?.SelectSingleNode("mod[id = \"" + xmlVehicleModNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponMountId = xmlDataWeaponMountNode["id"].InnerText;
                    string strDataWeaponMountName = xmlDataWeaponMountNode["name"].InnerText;
                    XmlNode xmlWeaponMountNode = xmlWeaponMountNodesParent.SelectSingleNode("weaponmount[id=\"" + strDataWeaponMountId + "\"]");
                    if (xmlWeaponMountNode != null)
                    {
                        if (xmlWeaponMountNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponMountId;
                            xmlWeaponMountNode.PrependChild(xmlIdElement);
                        }

                        if (xmlWeaponMountNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataWeaponMountName;
                            xmlWeaponMountNode.AppendChild(xmlNameElement);
                        }

                        if (xmlWeaponMountNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataWeaponMountName;
                            xmlWeaponMountNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlWeaponMountNode["page"];
                        if (xmlWeaponMountNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponMountNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlWeaponMountNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlWeaponMountNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataWeaponMountNode["page"].InnerText;
                        xmlWeaponMountNode.AppendChild(xmlPageElement);

                        xmlWeaponMountNodesParent.AppendChild(xmlWeaponMountNode);
                    }
                }
            }
            foreach (XmlNode xmlWeaponMountNode in xmlWeaponMountNodesParent.SelectNodes("weaponmount"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlWeaponMountNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlWeaponMountNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlWeaponMountNode.Attributes.RemoveAt(i);
                }
                if (xmlDataWeaponMountNodeList?.SelectSingleNode("weaponmount[id = \"" + xmlWeaponMountNode["id"]?.InnerText + "\"]") == null)
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

            // Process Mods for Weapon Mounts

            XmlNode xmlWeaponMountModNodesParent = xmlRootVehicleFileNode.SelectSingleNode("weaponmountmods");
            if (xmlWeaponMountModNodesParent == null)
            {
                xmlWeaponMountModNodesParent = objDataDoc.CreateElement("weaponmountmods");
                xmlRootVehicleFileNode.AppendChild(xmlWeaponMountModNodesParent);
            }

            XmlNode xmlDataWeaponMountModNodeList = xmlDataDocument.SelectSingleNode("/chummer/weaponmountmods");
            if (xmlDataWeaponMountModNodeList != null)
            {
                foreach (XmlNode xmlDataWeaponMountModNode in xmlDataWeaponMountModNodeList.SelectNodes("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponMountModId = xmlDataWeaponMountModNode["id"].InnerText;
                    string strDataWeaponMountModName = xmlDataWeaponMountModNode["name"].InnerText;
                    XmlNode xmlWeaponMountModNode = xmlWeaponMountModNodesParent.SelectSingleNode("mod[id=\"" + strDataWeaponMountModId + "\"]");
                    if (xmlWeaponMountModNode != null)
                    {
                        if (xmlWeaponMountModNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponMountModId;
                            xmlWeaponMountModNode.PrependChild(xmlIdElement);
                        }

                        if (xmlWeaponMountModNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataWeaponMountModName;
                            xmlWeaponMountModNode.AppendChild(xmlNameElement);
                        }

                        if (xmlWeaponMountModNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataWeaponMountModName;
                            xmlWeaponMountModNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlWeaponMountModNode["page"];
                        if (xmlWeaponMountModNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponMountModNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlWeaponMountModNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlWeaponMountModNode.RemoveChild(xmlPage);
                        }
                    }
                    else
                    {
                        xmlWeaponMountModNode = objDataDoc.CreateElement("mod");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataWeaponMountModId;
                        xmlWeaponMountModNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataWeaponMountModName;
                        xmlWeaponMountModNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataWeaponMountModName;
                        xmlWeaponMountModNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataWeaponMountModNode["page"].InnerText;
                        xmlWeaponMountModNode.AppendChild(xmlPageElement);

                        xmlWeaponMountModNodesParent.AppendChild(xmlWeaponMountModNode);
                    }
                }
            }
            foreach (XmlNode xmlWeaponMountModNode in xmlWeaponMountModNodesParent.SelectNodes("mod"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlWeaponMountModNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlWeaponMountModNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlWeaponMountModNode.Attributes.RemoveAt(i);
                }
                if (xmlDataWeaponMountModNodeList?.SelectSingleNode("mod[id = \"" + xmlWeaponMountModNode["id"]?.InnerText + "\"]") == null)
                {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponMountModNode.Attributes.Append(xmlExistsAttribute);
                    }
#else
                    {
                        xmlWeaponMountModNodesParent.RemoveChild(xmlWeaponMountModNode);
                    }
#endif
                }
            }

            // Remove Limits

            XmlNode xmlRemoveNode = xmlRootVehicleFileNode.SelectSingleNode("limits");
            if (xmlRemoveNode != null)
            {
                xmlRootVehicleFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessVessels(XmlDocument objDataDoc, BackgroundWorker objWorker)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "vessels.xml"));

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }
            XmlNode xmlRootMetatypeFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"vessels.xml\"]");
            if (xmlRootMetatypeFileNode == null)
            {
                xmlRootMetatypeFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "vessels.xml";
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode["name"].InnerText;
                    string strDataMetatypeId = xmlDataMetatypeNode["id"].InnerText;
                    XmlNode xmlMetatypeNode = xmlMetatypeNodesParent.SelectSingleNode("metatype[id=\"" + strDataMetatypeId + "\"]");
                    if (xmlMetatypeNode != null)
                    {
                        if (xmlMetatypeNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataMetatypeId;
                            xmlMetatypeNode.PrependChild(xmlIdElement);
                        }

                        if (xmlMetatypeNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlNameElement);
                        }

                        if (xmlMetatypeNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataMetatypeName;
                            xmlMetatypeNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlMetatypeNode["page"];
                        if (xmlMetatypeNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlMetatypeNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlMetatypeNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataMetatypeNode["page"].InnerText;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }
                }
            }
            foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodesParent.SelectNodes("metatype"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlMetatypeNode.Attributes.RemoveAt(i);
                }
                if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
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

        private static void ProcessWeapons(XmlDocument objDataDoc, BackgroundWorker objWorker)
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
                    if (objWorker.CancellationPending)
                        return;
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
                if (objWorker.CancellationPending)
                    return;
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponName = xmlDataWeaponNode["name"].InnerText;
                    string strDataWeaponId = xmlDataWeaponNode["id"].InnerText;
                    XmlNode xmlWeaponNode = xmlWeaponNodesParent.SelectSingleNode("weapon[id=\"" + strDataWeaponId + "\"]");
                    if (xmlWeaponNode != null)
                    {
                        if (xmlWeaponNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataWeaponId;
                            xmlWeaponNode.PrependChild(xmlIdElement);
                        }

                        if (xmlWeaponNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataWeaponName;
                            xmlWeaponNode.AppendChild(xmlNameElement);
                        }

                        if (xmlWeaponNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataWeaponName;
                            xmlWeaponNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlWeaponNode["page"];
                        if (xmlWeaponNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlWeaponNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlWeaponNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataWeaponNode["page"].InnerText;
                        xmlWeaponNode.AppendChild(xmlPageElement);

                        xmlWeaponNodesParent.AppendChild(xmlWeaponNode);
                    }
                }
            }
            foreach (XmlNode xmlWeaponNode in xmlWeaponNodesParent.SelectNodes("weapon"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlWeaponNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlWeaponNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlWeaponNode.Attributes.RemoveAt(i);
                }
                if (xmlDataWeaponNodeList?.SelectSingleNode("weapon[id = \"" + xmlWeaponNode["id"]?.InnerText + "\"]") == null)
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
                    if (objWorker.CancellationPending)
                        return;
                    string strDataAccessoryId = xmlDataAccessoryNode["id"].InnerText;
                    string strDataAccessoryName = xmlDataAccessoryNode["name"].InnerText;
                    XmlNode xmlAccessoryNode = xmlAccessoryNodesParent.SelectSingleNode("accessory[id=\"" + strDataAccessoryId + "\"]");
                    if (xmlAccessoryNode != null)
                    {
                        if (xmlAccessoryNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataAccessoryId;
                            xmlAccessoryNode.PrependChild(xmlIdElement);
                        }

                        if (xmlAccessoryNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataAccessoryName;
                            xmlAccessoryNode.AppendChild(xmlNameElement);
                        }

                        if (xmlAccessoryNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataAccessoryName;
                            xmlAccessoryNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlAccessoryNode["page"];
                        if (xmlAccessoryNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataAccessoryNode["page"].InnerText;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlAccessoryNode.AppendChild(xmlIdElement);
                        }
                        if (xmlPage != null)
                        {
                            xmlAccessoryNode.RemoveChild(xmlPage);
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

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataAccessoryNode["page"].InnerText;
                        xmlAccessoryNode.AppendChild(xmlPageElement);

                        xmlAccessoryNodesParent.AppendChild(xmlAccessoryNode);
                    }
                }
            }
            foreach (XmlNode xmlAccessoryNode in xmlAccessoryNodesParent.SelectNodes("accessory"))
            {
                if (objWorker.CancellationPending)
                    return;
                for (int i = xmlAccessoryNode.Attributes.Count - 1; i >= 0; --i)
                {
                    XmlAttribute xmlAttribute = xmlAccessoryNode.Attributes[i];
                    if (xmlAttribute.Name != "translated")
                        xmlAccessoryNode.Attributes.RemoveAt(i);
                }
                if (xmlDataAccessoryNodeList?.SelectSingleNode("accessory[id = \"" + xmlAccessoryNode["id"]?.InnerText + "\"]") == null)
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

        /// <summary>
        /// Process translation of child elements inside individual item elements 
        /// </summary>
        /// <param name="xmlItemNode">The item on the translation data</param>
        /// <param name="xmlDataItemNode">The item on the source data</param>
        /// <param name="strSubItemParent">Name of the parent tag for the subnodes, ex: "metavariants"</param>
        /// <param name="strSubItem">Name of the subnodes tag, ex: "metavariant"</param>
        /// <param name="blnProcessPages">Does the subnodes have page information?</param>
        /// <param name="objDataDoc">The XmlDocument that holds the translation data</param>
        /// <param name="objWorker">The BackgroundWorker if it used.</param>
        private static void AuxProcessSubItems(XmlNode xmlItemNode, XmlNode xmlDataItemNode, string strSubItemParent, string strSubItem, bool blnProcessPages, XmlDocument objDataDoc, BackgroundWorker objWorker = null)
        {
            XmlNode xmlSubItemsParent = xmlItemNode.SelectSingleNode(strSubItemParent);
            XmlNode xmlDataSubItemsList = xmlDataItemNode.SelectSingleNode(strSubItemParent);

            if (xmlDataSubItemsList != null)
            {
                if (xmlSubItemsParent == null)
                {
                    xmlSubItemsParent = objDataDoc.CreateElement(strSubItemParent);
                    xmlItemNode.AppendChild(xmlSubItemsParent);
                }
                foreach (XmlNode xmlDataSubItem in xmlDataSubItemsList.SelectNodes(strSubItem))
                {
                    if (objWorker?.CancellationPending ?? false)
                        return;
                    string strDataSubItemName = xmlDataSubItem["name"].InnerText;
                    XmlNode xmlSubItem = xmlSubItemsParent.SelectSingleNode(strSubItem + "[name=\"" + strDataSubItemName + "\"]");
                    if (xmlSubItem != null)
                    {
                        if (xmlSubItem["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataSubItemName;
                            xmlSubItem.AppendChild(xmlNameElement);
                        }

                        if (xmlSubItem["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataSubItemName;
                            xmlSubItem.AppendChild(xmlTranslateElement);
                        }
                        // do we process pages?
                        if (blnProcessPages)
                        {
                            XmlNode xmlPage = xmlSubItem["page"];
                            if (xmlSubItem["altpage"] == null)
                            {
                                string strPage = xmlPage?.InnerText ?? xmlDataSubItem["page"].InnerText;
                                XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                                xmlIdElement.InnerText = strPage;
                                xmlSubItem.AppendChild(xmlIdElement);
                            }
                            if (xmlPage != null)
                            {
                                xmlSubItem.RemoveChild(xmlPage);
                            }
                        }
                    }
                    else
                    {
                        xmlSubItem = objDataDoc.CreateElement(strSubItem);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataSubItemName;
                        xmlSubItem.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataSubItemName;
                        xmlSubItem.AppendChild(xmlTranslateElement);

                        // do we process pages?
                        if (blnProcessPages)
                        {
                            XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                            xmlPageElement.InnerText = xmlDataSubItem["page"].InnerText;
                            xmlSubItem.AppendChild(xmlPageElement);
                        }

                        xmlSubItemsParent.AppendChild(xmlSubItem);
                    }
                }
                foreach (XmlNode xmlSubItem in xmlSubItemsParent.SelectNodes(strSubItem))
                {
                    if (objWorker?.CancellationPending ?? false)
                        return;
                    for (int i = xmlSubItem.Attributes.Count - 1; i >= 0; --i)
                    {
                        XmlAttribute xmlAttribute = xmlSubItem.Attributes[i];
                        if (xmlAttribute.Name != "translated")
                            xmlSubItem.Attributes.RemoveAt(i);
                    }
                    if (xmlDataSubItemsList?.SelectSingleNode(strSubItem + "[name = \"" + xmlSubItem["name"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                                {
                                    XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                    xmlExistsAttribute.Value = "False";
                                    xmlSubItem.Attributes.Append(xmlExistsAttribute);
                                }
#else
                        {
                            xmlSubItemsParent.RemoveChild(xmlSubItem);
                        }
#endif
                    }
                }
            }
            else if (xmlSubItemsParent != null)
            {
#if !DELETE
                        {
                            XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                            xmlExistsAttribute.Value = "False";
                            xmlSubItem.Attributes.Append(xmlExistsAttribute);
                        }
#else
                {
                    xmlItemNode.RemoveChild(xmlSubItemsParent);
                }
#endif
            }
        }



        #endregion Data Processing



        #region Properties
        public IList<frmTranslate> OpenTranslateWindows
        {
            get
            {
                return s_LstOpenTranslateWindows;
            }
        }
        #endregion
    }
}
