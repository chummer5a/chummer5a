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
#define DELETE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Translator
{
    public partial class frmTranslatorMain
    {
        private static readonly string PATH = Application.StartupPath;
        private readonly BackgroundWorker _workerDataProcessor = new BackgroundWorker();
        private bool _blnQueueDataProcessorRun;
        private readonly BackgroundWorker _workerStringsProcessor = new BackgroundWorker();
        private bool _blnQueueStringsProcessorRun;
        private string _strLanguageToLoad = string.Empty;
        private readonly string[] _astrArgs = new string[3];
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

        private void RunQueuedWorkers(object sender, EventArgs e)
        {
            if (_blnQueueDataProcessorRun)
            {
                if (!_workerDataProcessor.IsBusy)
                    _workerDataProcessor.RunWorkerAsync();
            }

            if (_blnQueueStringsProcessorRun)
            {
                if (!_workerStringsProcessor.IsBusy)
                    _workerStringsProcessor.RunWorkerAsync();
            }
        }

        #region Control Events

        private void cboLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool blnEnableButtons = cboLanguages.SelectedIndex != -1;
            cmdEdit.Enabled = blnEnableButtons;
            cmdUpdate.Enabled = blnEnableButtons;
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
                    txtLanguageName.Text = objSelectedCulture.TextInfo.ToTitleCase(strName);
                    chkRightToLeft.Checked = objSelectedCulture.TextInfo.IsRightToLeft;
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
                        strName = objSelectedCulture.TextInfo.ToTitleCase(strName.Substring(0, intCountryNameIndex).Trim());
                    if (strName != "Unknown Locale")
                    {
                        txtLanguageName.Text = strName;
                        chkRightToLeft.Checked = objSelectedCulture.TextInfo.IsRightToLeft;
                    }
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
                MessageBox.Show("You must provide a two-character language code.");
                return;
            }

            if (txtRegionCode.TextLength != 2)
            {
                MessageBox.Show("You must provide a two-character region code.");
                return;
            }

            string strLowerCode = txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToLower();

            if (strLowerCode == "en-us")
            {
                MessageBox.Show("You cannot create a localization with this language code, as it is Chummer5's default and fallback localization.");
                return;
            }

            TextInfo objTextInfoForCapitalization = null;
            try
            {
                CultureInfo objCultureInfo = CultureInfo.GetCultureInfo(strLowerCode);
                objTextInfoForCapitalization = objCultureInfo.TextInfo;
            }
            catch (CultureNotFoundException)
            {
                if (MessageBox.Show(
                        "The language code you provided has a language code that does not comply with ISO 639-1 and/or a region code that does not comply with ISO 3166-1. This may cause issues with Chummer." +
                        Environment.NewLine + Environment.NewLine + "Are you sure you wish to use the entered code?",
                        "Language Code Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes)
                    return;
            }

            if (File.Exists(Path.Combine(PATH, "lang", strLowerCode + "_data.xml")) || File.Exists(Path.Combine(PATH, "lang", strLowerCode + ".xml")))
            {
                DialogResult eDialogResult = MessageBox.Show("A translation already exists with the same code as the one you provided." +
                                                             Environment.NewLine + Environment.NewLine + "Do you wish to rebuild the existing translation instead of clearing it and starting anew?",
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

            _strLanguageToLoad = (objTextInfoForCapitalization ?? (new CultureInfo("en-US", false)).TextInfo)
                                 .ToTitleCase(txtLanguageName.Text) + " (" + txtLanguageCode.Text.ToLower() + '-' + txtRegionCode.Text.ToUpper() + ')';
            _astrArgs[0] = strLowerCode;
            _astrArgs[1] = _strLanguageToLoad;
            _astrArgs[2] = bool.TrueString;

            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            cmdCancel.Enabled = true;

            _blnQueueStringsProcessorRun = true;
            _blnQueueDataProcessorRun = true;
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

        private void cmdUpdate_Click(object sender, EventArgs e)
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

            _astrArgs[0] = cboLanguages.Text.Substring(cboLanguages.Text.IndexOf('(') + 1, 5).ToLower();
            _astrArgs[1] = _strLanguageToLoad;
            _astrArgs[2] = bool.FalseString;

            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            cmdCancel.Enabled = true;

            _blnQueueStringsProcessorRun = true;
            _blnQueueDataProcessorRun = true;
        }

        private void cmdRebuild_Click(object sender, EventArgs e)
        {
            if (cboLanguages.SelectedIndex == -1)
                return;

            if (MessageBox.Show("Rebuilding translation files will delete any translation entries that do not have corresponding entries in the base Chummer5a files.\n" +
                                "If your translation files have any entries for custom items, it is recommended that you use \"Update\" instead.\n\n" +
                                "Are you sure you wish to Rebuild your translation files?",
                    "Rebuild Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
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

            _astrArgs[0] = cboLanguages.Text.Substring(cboLanguages.Text.IndexOf('(') + 1, 5).ToLower();
            _astrArgs[1] = _strLanguageToLoad;
            _astrArgs[2] = bool.TrueString;

            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();

            cmdCancel.Enabled = true;

            _blnQueueStringsProcessorRun = true;
            _blnQueueDataProcessorRun = true;
        }

        private void frmTranslatorMain_Load(object sender, EventArgs e)
        {
            LoadLanguageList();

            Application.Idle += RunQueuedWorkers;
        }

        private void frmTranslatorMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Idle -= RunQueuedWorkers;

            if (_workerStringsProcessor.IsBusy)
                _workerStringsProcessor.CancelAsync();
            if (_workerDataProcessor.IsBusy)
                _workerDataProcessor.CancelAsync();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            _blnQueueStringsProcessorRun = false;
            _blnQueueDataProcessorRun = false;
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
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("An error was encountered while trying to restore the original translation files. Cancellation may not have been completely successful.",
                            "Backup Restoration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void chkRightToLeft_CheckedChanged(object sender, EventArgs e)
        {
            txtLanguageName.RightToLeft = chkRightToLeft.Checked ? RightToLeft.Yes : RightToLeft.No;
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

        private Tuple<XmlDocument, string> _objStringsDocWithPath;

        private void DoStringsProcessing(object sender, DoWorkEventArgs e)
        {
            _blnQueueStringsProcessorRun = false;
            string strFilePath = Path.Combine(PATH, "lang", _astrArgs[0] + ".xml");

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
                xmlNameNode.InnerText = _astrArgs[1];
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

            XmlNode xmlRightToLeftNode = xmlRootChummerNode.SelectSingleNode("righttoleft");
            if (xmlRightToLeftNode == null)
            {
                xmlRightToLeftNode = objDoc.CreateElement("righttoleft");
                xmlRightToLeftNode.InnerText = chkRightToLeft.Checked.ToString();
                xmlRootChummerNode.AppendChild(xmlRightToLeftNode);
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
                    using (XmlNodeList xmlStringNodeList = xmlStringsNode.SelectNodes("string"))
                        if (xmlStringNodeList != null)
                            foreach (XmlNode xmlStringNode in xmlStringNodeList)
                            {
                                if (_workerStringsProcessor.CancellationPending)
                                    break;
                                string strKey = xmlStringNode["key"]?.InnerText;
                                XmlNode xmlTranslatedStringNode = xmlTranslatedStringsNode.SelectSingleNode("string[key = \"" + strKey + "\"]");
                                if (xmlTranslatedStringNode == null)
                                {
                                    xmlTranslatedStringsNode.AppendChild(objDoc.ImportNode(xmlStringNode, true));
                                }
                            }

                    using (XmlNodeList xmlTranslatedStringNodeList = xmlTranslatedStringsNode.SelectNodes("string"))
                        if (xmlTranslatedStringNodeList != null)
                            foreach (XmlNode xmlTranslatedStringNode in xmlTranslatedStringNodeList)
                            {
                                if (_workerStringsProcessor.CancellationPending)
                                    break;
                                string strKey = xmlTranslatedStringNode["key"]?.InnerText;
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

        private Tuple<XmlDocument, string> _objDataDocWithPath;

        private void DoDataProcessing(object sender, DoWorkEventArgs e)
        {
            _blnQueueDataProcessorRun = false;
            string strFilePath = Path.Combine(PATH, "lang", _astrArgs[0] + "_data.xml");
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
                    s_LstProcessFunctions[i].Invoke(objDataDoc, _workerDataProcessor, _astrArgs[2] == bool.TrueString);

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

        private static readonly Action<XmlDocument, BackgroundWorker, bool>[] s_LstProcessFunctions =
        {
            ProcessArmor,
            ProcessBioware,
            ProcessBooks,
            ProcessComplexForms,
            ProcessContacts,
            ProcessCritterPowers,
            ProcessCritters,
            ProcessCyberware,
            ProcessDrugs,
            ProcessEchoes,
            ProcessGameplayOptions,
            ProcessGear,
            ProcessImprovements,
            ProcessLicenses,
            ProcessLifestyles,
            ProcessMartialArts,
            ProcessMentors,
            ProcessMetamagic,
            ProcessMetatypes,
            ProcessOptions,
            ProcessParagons,
            ProcessPowers,
            ProcessPriorities,
            ProcessPrograms,
            ProcessQualities,
            ProcessRanges,
            ProcessSkills,
            ProcessSpells,
            ProcessSpiritPowers,
            ProcessStreams,
            ProcessTips,
            ProcessTraditions,
            ProcessVehicles,
            ProcessVessels,
            ProcessWeapons,
        };

        private static void ProcessArmor(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "armor.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootArmorFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootArmorFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootArmorFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootArmorFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Armors

            XmlNode xmlArmorNodesParent = xmlRootArmorFileNode.SelectSingleNode("armors");
            if (xmlArmorNodesParent == null)
            {
                xmlArmorNodesParent = objDataDoc.CreateElement("armors");
                xmlRootArmorFileNode.AppendChild(xmlArmorNodesParent);
            }

            XPathNavigator xmlDataArmorNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("armors");
            if (xmlDataArmorNodeList != null)
            {
                foreach (XPathNavigator xmlDataArmorNode in xmlDataArmorNodeList.Select("armor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArmorName = xmlDataArmorNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataArmorId = xmlDataArmorNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataArmorNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataArmorNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlArmorNode.AppendChild(xmlPageElement);

                        xmlArmorNodesParent.AppendChild(xmlArmorNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlArmorNode in xmlArmorNodesParent.SelectNodes("armor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlArmorNode.Attributes != null)
                        for (int i = xmlArmorNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlArmorNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlArmorNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataArmorNodeList?.SelectSingleNode("armor[id = \"" + xmlArmorNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlArmorNodesParent.RemoveChild(xmlArmorNode);
                        }
#endif
                    }
                }
            }

            // Process Armor Mods

            XmlNode xmlArmorModNodesParent = xmlRootArmorFileNode.SelectSingleNode("mods");
            if (xmlArmorModNodesParent == null)
            {
                xmlArmorModNodesParent = objDataDoc.CreateElement("mods");
                xmlRootArmorFileNode.AppendChild(xmlArmorModNodesParent);
            }

            XPathNavigator xmlDataArmorModNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("mods");
            if (xmlDataArmorModNodeList != null)
            {
                foreach (XPathNavigator xmlDataArmorModNode in xmlDataArmorModNodeList.Select("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArmorModId = xmlDataArmorModNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataArmorModName = xmlDataArmorModNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataArmorModNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataArmorModNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlArmorModNode.AppendChild(xmlPageElement);

                        xmlArmorModNodesParent.AppendChild(xmlArmorModNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlArmorModNode in xmlArmorModNodesParent.SelectNodes("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlArmorModNode.Attributes != null)
                        for (int i = xmlArmorModNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlArmorModNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlArmorModNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataArmorModNodeList?.SelectSingleNode("mod[id = \"" + xmlArmorModNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlArmorModNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlArmorModNodesParent.RemoveChild(xmlArmorModNode);
                        }
#endif
                    }
                }
            }
        }

        private static void ProcessBioware(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "bioware.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootBiowareFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBiowareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootBiowareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootBiowareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Biowares

            XmlNode xmlBiowareNodesParent = xmlRootBiowareFileNode.SelectSingleNode("biowares");
            if (xmlBiowareNodesParent == null)
            {
                xmlBiowareNodesParent = objDataDoc.CreateElement("biowares");
                xmlRootBiowareFileNode.AppendChild(xmlBiowareNodesParent);
            }

            XPathNavigator xmlDataBiowareNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("biowares");
            if (xmlDataBiowareNodeList != null)
            {
                foreach (XPathNavigator xmlDataBiowareNode in xmlDataBiowareNodeList.Select("bioware"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataBiowareName = xmlDataBiowareNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataBiowareId = xmlDataBiowareNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataBiowareNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataBiowareNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlBiowareNode.AppendChild(xmlPageElement);

                        xmlBiowareNodesParent.AppendChild(xmlBiowareNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlBiowareNode in xmlBiowareNodesParent.SelectNodes("bioware"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlBiowareNode.Attributes != null)
                        for (int i = xmlBiowareNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlBiowareNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlBiowareNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataBiowareNodeList?.SelectSingleNode("bioware[id = \"" + xmlBiowareNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlBiowareNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlBiowareNodesParent.RemoveChild(xmlBiowareNode);
                        }
#endif
                    }
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootBiowareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = objDataDoc.CreateElement("grades");
                xmlRootBiowareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XPathNavigator xmlDataGradeNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XPathNavigator xmlDataGradeNode in xmlDataGradeNodeList.Select("grade"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGradeId = xmlDataGradeNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataGradeName = xmlDataGradeNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataGradeNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataGradeNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlGradeNode.Attributes != null)
                        for (int i = xmlGradeNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlGradeNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlGradeNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataGradeNodeList?.SelectSingleNode("grade[id = \"" + xmlGradeNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                        }
#endif
                    }
                }
            }
        }

        private static void ProcessBooks(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "books.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootBooksFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootBooksFileNode);
            }

            XmlNode xmlBookNodesParent = xmlRootBooksFileNode.SelectSingleNode("books");
            if (xmlBookNodesParent == null)
            {
                xmlBookNodesParent = objDataDoc.CreateElement("books");
                xmlRootBooksFileNode.AppendChild(xmlBookNodesParent);
            }

            XPathNavigator xmlDataBookNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("books");
            if (xmlDataBookNodeList != null)
            {
                foreach (XPathNavigator xmlDataBookNode in xmlDataBookNodeList.Select("book"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataBookId = xmlDataBookNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataBookName = xmlDataBookNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            xmlCodeElement.InnerText = xmlDataBookNode.SelectSingleNode("code")?.Value ?? string.Empty;
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
                        xmlCodeElement.InnerText = xmlDataBookNode.SelectSingleNode("code")?.Value ?? string.Empty;
                        xmlBookNode.AppendChild(xmlCodeElement);

                        xmlBookNodesParent.AppendChild(xmlBookNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlBookNodeList = xmlBookNodesParent.SelectNodes("book"))
                {
                    if (xmlBookNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlBookNode in xmlBookNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlBookNode.Attributes != null)
                                for (int i = xmlBookNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlBookNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlBookNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataBookNodeList?.SelectSingleNode("book[id = \"" + xmlBookNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlBookNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlBookNodesParent.RemoveChild(xmlBookNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessComplexForms(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "complexforms.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootComplexFormsFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootComplexFormsFileNode);
            }

            XmlNode xmlComplexFormNodesParent = xmlRootComplexFormsFileNode.SelectSingleNode("complexforms");
            if (xmlComplexFormNodesParent == null)
            {
                xmlComplexFormNodesParent = objDataDoc.CreateElement("complexforms");
                xmlRootComplexFormsFileNode.AppendChild(xmlComplexFormNodesParent);
            }

            XPathNavigator xmlDataComplexFormNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("complexforms");
            if (xmlDataComplexFormNodeList != null)
            {
                foreach (XPathNavigator xmlDataComplexFormNode in xmlDataComplexFormNodeList.Select("complexform"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataComplexFormId = xmlDataComplexFormNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataComplexFormName = xmlDataComplexFormNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataComplexFormNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataComplexFormNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlComplexFormNode.AppendChild(xmlPageElement);

                        xmlComplexFormNodesParent.AppendChild(xmlComplexFormNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlComplexFormNodeList = xmlComplexFormNodesParent.SelectNodes("complexform"))
                {
                    if (xmlComplexFormNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlComplexFormNode in xmlComplexFormNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlComplexFormNode.Attributes != null)
                                for (int i = xmlComplexFormNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlComplexFormNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlComplexFormNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataComplexFormNodeList?.SelectSingleNode("complexform[id = \"" + xmlComplexFormNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlComplexFormNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlComplexFormNodesParent.RemoveChild(xmlComplexFormNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessContacts(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "contacts.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootContactFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootContactFileNode);
            }

            // Process Contacts

            XmlNode xmlContactNodesParent = xmlRootContactFileNode.SelectSingleNode("contacts");

            if (xmlContactNodesParent == null)
            {
                xmlContactNodesParent = objDataDoc.CreateElement("contacts");
                xmlRootContactFileNode.AppendChild(xmlContactNodesParent);
            }

            XPathNavigator xmlDataContactNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("contacts");
            if (xmlDataContactNodeList != null)
            {
                foreach (XPathNavigator xmlDataContactNode in xmlDataContactNodeList.Select("contact"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlContactNodesParent.SelectSingleNode("contact[text()=\"" + xmlDataContactNode.Value + "\"]") == null)
                    {
                        XmlNode xmlContactNode = objDataDoc.CreateElement("contact");
                        xmlContactNode.InnerText = xmlDataContactNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataContactNode.Value;
                        xmlContactNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlContactNodesParent.AppendChild(xmlContactNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlContactNodeList = xmlContactNodesParent.SelectNodes("contact"))
                {
                    if (xmlContactNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlContactNode in xmlContactNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataContactNodeList?.SelectSingleNode("contact[text() = \"" + xmlContactNode.InnerText + "\"]") == null)
                            {
                                xmlContactNodesParent.RemoveChild(xmlContactNode);
                            }
                        }
                    }
                }
            }

            // Process Sexes

            XmlNode xmlSexNodesParent = xmlRootContactFileNode.SelectSingleNode("sexes");

            if (xmlSexNodesParent == null)
            {
                xmlSexNodesParent = objDataDoc.CreateElement("sexes");
                xmlRootContactFileNode.AppendChild(xmlSexNodesParent);
            }

            XPathNavigator xmlDataSexNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("sexes");
            if (xmlDataSexNodeList != null)
            {
                foreach (XPathNavigator xmlDataSexNode in xmlDataSexNodeList.Select("sex"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlSexNodesParent.SelectSingleNode("sex[text()=\"" + xmlDataSexNode.Value + "\"]") == null)
                    {
                        XmlNode xmlSexNode = objDataDoc.CreateElement("sex");
                        xmlSexNode.InnerText = xmlDataSexNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataSexNode.Value;
                        xmlSexNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlSexNodesParent.AppendChild(xmlSexNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlSexNodeList = xmlSexNodesParent.SelectNodes("sex"))
                {
                    if (xmlSexNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlSexNode in xmlSexNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataSexNodeList?.SelectSingleNode("sex[text() = \"" + xmlSexNode.InnerText + "\"]") == null)
                            {
                                xmlSexNodesParent.RemoveChild(xmlSexNode);
                            }
                        }
                    }
                }
            }

            // Process Ages

            XmlNode xmlAgeNodesParent = xmlRootContactFileNode.SelectSingleNode("ages");

            if (xmlAgeNodesParent == null)
            {
                xmlAgeNodesParent = objDataDoc.CreateElement("ages");
                xmlRootContactFileNode.AppendChild(xmlAgeNodesParent);
            }

            XPathNavigator xmlDataAgeNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("ages");
            if (xmlDataAgeNodeList != null)
            {
                foreach (XPathNavigator xmlDataAgeNode in xmlDataAgeNodeList.Select("age"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlAgeNodesParent.SelectSingleNode("age[text()=\"" + xmlDataAgeNode.Value + "\"]") == null)
                    {
                        XmlNode xmlAgeNode = objDataDoc.CreateElement("age");
                        xmlAgeNode.InnerText = xmlDataAgeNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataAgeNode.Value;
                        xmlAgeNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlAgeNodesParent.AppendChild(xmlAgeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlAgeNode in xmlAgeNodesParent.SelectNodes("age"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataAgeNodeList?.SelectSingleNode("age[text() = \"" + xmlAgeNode.InnerText + "\"]") == null)
                    {
                        xmlAgeNodesParent.RemoveChild(xmlAgeNode);
                    }
                }
            }

            // Process Personal Lives

            XmlNode xmlPersonalLifeNodesParent = xmlRootContactFileNode.SelectSingleNode("personallives");

            if (xmlPersonalLifeNodesParent == null)
            {
                xmlPersonalLifeNodesParent = objDataDoc.CreateElement("personallives");
                xmlRootContactFileNode.AppendChild(xmlPersonalLifeNodesParent);
            }

            XPathNavigator xmlDataPersonalLifeNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("personallives");
            if (xmlDataPersonalLifeNodeList != null)
            {
                foreach (XPathNavigator xmlDataPersonalLifeNode in xmlDataPersonalLifeNodeList.Select("personallife"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlPersonalLifeNodesParent.SelectSingleNode("personallife[text()=\"" + xmlDataPersonalLifeNode.Value + "\"]") == null)
                    {
                        XmlNode xmlPersonalLifeNode = objDataDoc.CreateElement("personallife");
                        xmlPersonalLifeNode.InnerText = xmlDataPersonalLifeNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataPersonalLifeNode.Value;
                        xmlPersonalLifeNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlPersonalLifeNodesParent.AppendChild(xmlPersonalLifeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlPersonalLifeNode in xmlPersonalLifeNodesParent.SelectNodes("personallife"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataPersonalLifeNodeList?.SelectSingleNode("personallife[text() = \"" + xmlPersonalLifeNode.InnerText + "\"]") == null)
                    {
                        xmlPersonalLifeNodesParent.RemoveChild(xmlPersonalLifeNode);
                    }
                }
            }

            // Process Types

            XmlNode xmlTypeNodesParent = xmlRootContactFileNode.SelectSingleNode("types");

            if (xmlTypeNodesParent == null)
            {
                xmlTypeNodesParent = objDataDoc.CreateElement("types");
                xmlRootContactFileNode.AppendChild(xmlTypeNodesParent);
            }

            XPathNavigator xmlDataTypeNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("types");
            if (xmlDataTypeNodeList != null)
            {
                foreach (XPathNavigator xmlDataTypeNode in xmlDataTypeNodeList.Select("type"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlTypeNodesParent.SelectSingleNode("type[text()=\"" + xmlDataTypeNode.Value + "\"]") == null)
                    {
                        XmlNode xmlTypeNode = objDataDoc.CreateElement("type");
                        xmlTypeNode.InnerText = xmlDataTypeNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataTypeNode.Value;
                        xmlTypeNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlTypeNodesParent.AppendChild(xmlTypeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlTypeNode in xmlTypeNodesParent.SelectNodes("type"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataTypeNodeList?.SelectSingleNode("type[text() = \"" + xmlTypeNode.InnerText + "\"]") == null)
                    {
                        xmlTypeNodesParent.RemoveChild(xmlTypeNode);
                    }
                }
            }

            // Process PreferredPayments

            XmlNode xmlPreferredPaymentNodesParent = xmlRootContactFileNode.SelectSingleNode("preferredpayments");

            if (xmlPreferredPaymentNodesParent == null)
            {
                xmlPreferredPaymentNodesParent = objDataDoc.CreateElement("preferredpayments");
                xmlRootContactFileNode.AppendChild(xmlPreferredPaymentNodesParent);
            }

            XPathNavigator xmlDataPreferredPaymentNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("preferredpayments");
            if (xmlDataPreferredPaymentNodeList != null)
            {
                foreach (XPathNavigator xmlDataPreferredPaymentNode in xmlDataPreferredPaymentNodeList.Select("preferredpayment"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlPreferredPaymentNodesParent.SelectSingleNode("preferredpayment[text()=\"" + xmlDataPreferredPaymentNode.Value + "\"]") == null)
                    {
                        XmlNode xmlPreferredPaymentNode = objDataDoc.CreateElement("preferredpayment");
                        xmlPreferredPaymentNode.InnerText = xmlDataPreferredPaymentNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataPreferredPaymentNode.Value;
                        xmlPreferredPaymentNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlPreferredPaymentNodesParent.AppendChild(xmlPreferredPaymentNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlPreferredPaymentNode in xmlPreferredPaymentNodesParent.SelectNodes("preferredpayment"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataPreferredPaymentNodeList?.SelectSingleNode("preferredpayment[text() = \"" + xmlPreferredPaymentNode.InnerText + "\"]") == null)
                    {
                        xmlPreferredPaymentNodesParent.RemoveChild(xmlPreferredPaymentNode);
                    }
                }
            }

            // Process Hobbies/Vices

            XmlNode xmlHobbyViceNodesParent = xmlRootContactFileNode.SelectSingleNode("hobbiesvices");

            if (xmlHobbyViceNodesParent == null)
            {
                xmlHobbyViceNodesParent = objDataDoc.CreateElement("hobbiesvices");
                xmlRootContactFileNode.AppendChild(xmlHobbyViceNodesParent);
            }

            XPathNavigator xmlDataHobbyViceNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("hobbiesvices");
            if (xmlDataHobbyViceNodeList != null)
            {
                foreach (XPathNavigator xmlDataHobbyViceNode in xmlDataHobbyViceNodeList.Select("hobbyvice"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlHobbyViceNodesParent.SelectSingleNode("hobbyvice[text()=\"" + xmlDataHobbyViceNode.Value + "\"]") == null)
                    {
                        XmlNode xmlHobbyViceNode = objDataDoc.CreateElement("hobbyvice");
                        xmlHobbyViceNode.InnerText = xmlDataHobbyViceNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataHobbyViceNode.Value;
                        xmlHobbyViceNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlHobbyViceNodesParent.AppendChild(xmlHobbyViceNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
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
        }

        private static void ProcessCritterPowers(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "critterpowers.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootPowerFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootPowerFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootPowerFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = objDataDoc.CreateElement("powers");
                xmlRootPowerFileNode.AppendChild(xmlPowerNodesParent);
            }

            XPathNavigator xmlDataPowerNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("powers");
            if (xmlDataPowerNodeList != null)
            {
                foreach (XPathNavigator xmlDataPowerNode in xmlDataPowerNodeList.Select("power"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataPowerId = xmlDataPowerNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlPowerNodeList = xmlPowerNodesParent.SelectNodes("power"))
                {
                    if (xmlPowerNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlPowerNode in xmlPowerNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlPowerNode.Attributes != null)
                                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlPowerNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataPowerNodeList?.SelectSingleNode("power[id = \"" + xmlPowerNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                        {
                            XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                            xmlExistsAttribute.Value = "False";
                            xmlPowerNode.Attributes?.Append(xmlExistsAttribute);
                        }
#else
                                {
                                    xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessCritters(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "critters.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMetatypeFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Metatypes

            XmlNode xmlMetatypeNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("metatypes");
            if (xmlMetatypeNodesParent == null)
            {
                xmlMetatypeNodesParent = objDataDoc.CreateElement("metatypes");
                xmlRootMetatypeFileNode.AppendChild(xmlMetatypeNodesParent);
            }

            XPathNavigator xmlDataMetatypeNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("metatypes");
            if (xmlDataMetatypeNodeList != null)
            {
                foreach (XPathNavigator xmlDataMetatypeNode in xmlDataMetatypeNodeList.Select("metatype"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMetatypeId = xmlDataMetatypeNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMetatypeNodeList = xmlMetatypeNodesParent.SelectNodes("metatype"))
                {
                    if (xmlMetatypeNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlMetatypeNode.Attributes != null)
                                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMetatypeNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMetatypeNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessCyberware(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "cyberware.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootCyberwareFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootCyberwareFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootCyberwareFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Cyberwares

            XmlNode xmlCyberwareNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("cyberwares");
            if (xmlCyberwareNodesParent == null)
            {
                xmlCyberwareNodesParent = objDataDoc.CreateElement("cyberwares");
                xmlRootCyberwareFileNode.AppendChild(xmlCyberwareNodesParent);
            }

            XPathNavigator xmlDataCyberwareNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("cyberwares");
            if (xmlDataCyberwareNodeList != null)
            {
                foreach (XPathNavigator xmlDataCyberwareNode in xmlDataCyberwareNodeList.Select("cyberware"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataCyberwareName = xmlDataCyberwareNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataCyberwareId = xmlDataCyberwareNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataCyberwareNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataCyberwareNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlCyberwareNode.AppendChild(xmlPageElement);

                        xmlCyberwareNodesParent.AppendChild(xmlCyberwareNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlCyberwareNode in xmlCyberwareNodesParent.SelectNodes("cyberware"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCyberwareNode.Attributes != null)
                        for (int i = xmlCyberwareNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlCyberwareNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlCyberwareNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataCyberwareNodeList?.SelectSingleNode("cyberware[id = \"" + xmlCyberwareNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlCyberwareNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlCyberwareNodesParent.RemoveChild(xmlCyberwareNode);
                        }
#endif
                    }
                }
            }

            // Process Grades

            XmlNode xmlGradeNodesParent = xmlRootCyberwareFileNode.SelectSingleNode("grades");
            if (xmlGradeNodesParent == null)
            {
                xmlGradeNodesParent = objDataDoc.CreateElement("grades");
                xmlRootCyberwareFileNode.AppendChild(xmlGradeNodesParent);
            }

            XPathNavigator xmlDataGradeNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("grades");
            if (xmlDataGradeNodeList != null)
            {
                foreach (XPathNavigator xmlDataGradeNode in xmlDataGradeNodeList.Select("grade"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGradeId = xmlDataGradeNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataGradeName = xmlDataGradeNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataGradeNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataGradeNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlGradeNode.AppendChild(xmlPageElement);

                        xmlGradeNodesParent.AppendChild(xmlGradeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlGradeNode in xmlGradeNodesParent.SelectNodes("grade"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlGradeNode.Attributes != null)
                        for (int i = xmlGradeNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlGradeNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlGradeNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataGradeNodeList?.SelectSingleNode("grade[id = \"" + xmlGradeNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlGradeNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlGradeNodesParent.RemoveChild(xmlGradeNode);
                        }
#endif
                    }
                }
            }

            // Remove Cybersuites

            XmlNode xmlRemoveNode = xmlRootCyberwareFileNode.SelectSingleNode("suites");
            if (xmlRemoveNode != null)
            {
                xmlRootCyberwareFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessDrugs(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "drugcomponents.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }

            XmlNode xmlRootDrugFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"drugcomponents.xml\"]");
            if (xmlRootDrugFileNode == null)
            {
                xmlRootDrugFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "drugcomponents.xml";
                xmlRootDrugFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootDrugFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootDrugFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootDrugFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            #region Process Drug Components

            XmlNode xmlDrugComponentNodesParent = xmlRootDrugFileNode.SelectSingleNode("drugcomponents");
            if (xmlDrugComponentNodesParent == null)
            {
                xmlDrugComponentNodesParent = objDataDoc.CreateElement("drugcomponents");
                xmlRootDrugFileNode.AppendChild(xmlDrugComponentNodesParent);
            }

            XPathNavigator xmlDataDrugComponentNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("drugcomponents");
            if (xmlDataDrugComponentNodeList != null)
            {
                foreach (XPathNavigator xmlDataDrugComponentNode in xmlDataDrugComponentNodeList.Select("drugcomponent"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataDrugComponentName = xmlDataDrugComponentNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataDrugComponentId = xmlDataDrugComponentNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    XmlNode xmlDrugComponentNode = xmlDrugComponentNodesParent.SelectSingleNode("drugcomponent[id=\"" + strDataDrugComponentId + "\"]");
                    if (xmlDrugComponentNode != null)
                    {
                        if (xmlDrugComponentNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataDrugComponentId;
                            xmlDrugComponentNode.PrependChild(xmlIdElement);
                        }

                        if (xmlDrugComponentNode["name"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                            xmlNameElement.InnerText = strDataDrugComponentName;
                            xmlDrugComponentNode.AppendChild(xmlNameElement);
                        }

                        if (xmlDrugComponentNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataDrugComponentName;
                            xmlDrugComponentNode.AppendChild(xmlTranslateElement);
                        }

                        XmlNode xmlPage = xmlDrugComponentNode["page"];
                        if (xmlDrugComponentNode["altpage"] == null)
                        {
                            string strPage = xmlPage?.InnerText ?? xmlDataDrugComponentNode.SelectSingleNode("page")?.Value ?? string.Empty;
                            XmlNode xmlIdElement = objDataDoc.CreateElement("altpage");
                            xmlIdElement.InnerText = strPage;
                            xmlDrugComponentNode.AppendChild(xmlIdElement);
                        }

                        if (xmlPage != null)
                        {
                            xmlDrugComponentNode.RemoveChild(xmlPage);
                        }
                    }
                    else
                    {
                        xmlDrugComponentNode = objDataDoc.CreateElement("drugcomponent");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataDrugComponentId;
                        xmlDrugComponentNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("name");
                        xmlNameElement.InnerText = strDataDrugComponentName;
                        xmlDrugComponentNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataDrugComponentName;
                        xmlDrugComponentNode.AppendChild(xmlTranslateElement);

                        XmlNode xmlPageElement = objDataDoc.CreateElement("altpage");
                        xmlPageElement.InnerText = xmlDataDrugComponentNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlDrugComponentNode.AppendChild(xmlPageElement);

                        xmlDrugComponentNodesParent.AppendChild(xmlDrugComponentNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlDrugComponentNodeList = xmlDrugComponentNodesParent.SelectNodes("drugcomponent"))
                {
                    if (xmlDrugComponentNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlDrugComponentNode in xmlDrugComponentNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDrugComponentNode.Attributes != null)
                                for (int i = xmlDrugComponentNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlDrugComponentNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlDrugComponentNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataDrugComponentNodeList?.SelectSingleNode("drugcomponent[id = \"" + xmlDrugComponentNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlDrugComponentNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlDrugComponentNodesParent.RemoveChild(xmlDrugComponentNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            #endregion
        }

        private static void ProcessEchoes(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "echoes.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootEchoesFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootEchoesFileNode);
            }

            XmlNode xmlEchoNodesParent = xmlRootEchoesFileNode.SelectSingleNode("echoes");
            if (xmlEchoNodesParent == null)
            {
                xmlEchoNodesParent = objDataDoc.CreateElement("echoes");
                xmlRootEchoesFileNode.AppendChild(xmlEchoNodesParent);
            }

            XPathNavigator xmlDataEchoNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("echoes");
            if (xmlDataEchoNodeList != null)
            {
                foreach (XPathNavigator xmlDataEchoNode in xmlDataEchoNodeList.Select("echo"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataEchoId = xmlDataEchoNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataEchoName = xmlDataEchoNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataEchoNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataEchoNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlEchoNode.AppendChild(xmlPageElement);

                        xmlEchoNodesParent.AppendChild(xmlEchoNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlEchoNodeList = xmlEchoNodesParent.SelectNodes("echo"))
                {
                    if (xmlEchoNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlEchoNode in xmlEchoNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlEchoNode.Attributes != null)
                                for (int i = xmlEchoNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlEchoNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlEchoNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataEchoNodeList?.SelectSingleNode("echo[id = \"" + xmlEchoNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlEchoNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlEchoNodesParent.RemoveChild(xmlEchoNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessGameplayOptions(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "gameplayoptions.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootGameplayOptionsFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootGameplayOptionsFileNode);
            }

            // Process GameplayOptions

            XmlNode xmlGameplayOptionNodesParent = xmlRootGameplayOptionsFileNode.SelectSingleNode("gameplayoptions");
            if (xmlGameplayOptionNodesParent == null)
            {
                xmlGameplayOptionNodesParent = objDataDoc.CreateElement("gameplayoptions");
                xmlRootGameplayOptionsFileNode.AppendChild(xmlGameplayOptionNodesParent);
            }

            XPathNavigator xmlDataGameplayOptionNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("gameplayoptions");
            if (xmlDataGameplayOptionNodeList != null)
            {
                foreach (XPathNavigator xmlDataGameplayOptionNode in xmlDataGameplayOptionNodeList.Select("gameplayoption"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGameplayOptionName = xmlDataGameplayOptionNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataGameplayOptionId = xmlDataGameplayOptionNode.SelectSingleNode("id")?.Value ?? string.Empty;
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

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlGameplayOptionNodeList = xmlGameplayOptionNodesParent.SelectNodes("gameplayoption"))
                {
                    if (xmlGameplayOptionNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlGameplayOptionNode in xmlGameplayOptionNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlGameplayOptionNode.Attributes != null)
                                for (int i = xmlGameplayOptionNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlGameplayOptionNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlGameplayOptionNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataGameplayOptionNodeList?.SelectSingleNode("gameplayoption[id = \"" + xmlGameplayOptionNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlGameplayOptionNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlGameplayOptionNodesParent.RemoveChild(xmlGameplayOptionNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessGear(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "gear.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootGearFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootGearFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootGearFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootGearFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Gears

            XmlNode xmlGearNodesParent = xmlRootGearFileNode.SelectSingleNode("gears");
            if (xmlGearNodesParent == null)
            {
                xmlGearNodesParent = objDataDoc.CreateElement("gears");
                xmlRootGearFileNode.AppendChild(xmlGearNodesParent);
            }

            XPathNavigator xmlDataGearNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("gears");
            if (xmlDataGearNodeList != null)
            {
                foreach (XPathNavigator xmlDataGearNode in xmlDataGearNodeList.Select("gear"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataGearName = xmlDataGearNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataGearId = xmlDataGearNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    XmlNode xmlGearNode = xmlGearNodesParent.SelectSingleNode("gear[id=" + strDataGearId.CleanXPath() + "]");
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
                            string strPage = xmlPage?.InnerText ?? xmlDataGearNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataGearNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlGearNode.AppendChild(xmlPageElement);

                        xmlGearNodesParent.AppendChild(xmlGearNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlGearNodeList = xmlGearNodesParent.SelectNodes("gear"))
                {
                    if (xmlGearNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlGearNode in xmlGearNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlGearNode.Attributes != null)
                                for (int i = xmlGearNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlGearNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlGearNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataGearNodeList?.SelectSingleNode("gear[id = " + xmlGearNode["id"]?.InnerText.CleanXPath() + "]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlGearNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlGearNodesParent.RemoveChild(xmlGearNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessImprovements(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "improvements.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootImprovementsFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootImprovementsFileNode);
            }

            XmlNode xmlImprovementNodesParent = xmlRootImprovementsFileNode.SelectSingleNode("improvements");
            if (xmlImprovementNodesParent == null)
            {
                xmlImprovementNodesParent = objDataDoc.CreateElement("improvements");
                xmlRootImprovementsFileNode.AppendChild(xmlImprovementNodesParent);
            }

            XPathNavigator xmlDataImprovementNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("improvements");
            if (xmlDataImprovementNodeList != null)
            {
                foreach (XPathNavigator xmlDataImprovementNode in xmlDataImprovementNodeList.Select("improvement"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataImprovementId = xmlDataImprovementNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataImprovementName = xmlDataImprovementNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataImprovementNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataImprovementNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlImprovementNode.AppendChild(xmlPageElement);

                        xmlImprovementNodesParent.AppendChild(xmlImprovementNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlImprovementNodeList = xmlImprovementNodesParent.SelectNodes("improvement"))
                {
                    if (xmlImprovementNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlImprovementNode in xmlImprovementNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlImprovementNode.Attributes != null)
                                for (int i = xmlImprovementNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlImprovementNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlImprovementNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataImprovementNodeList?.SelectSingleNode("improvement[id = \"" + xmlImprovementNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlImprovementNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlImprovementNodesParent.RemoveChild(xmlImprovementNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessLicenses(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "licenses.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootLicenseFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLicenseFileNode);
            }

            // Process Licenses

            XmlNode xmlLicenseNodesParent = xmlRootLicenseFileNode.SelectSingleNode("licenses");

            if (xmlLicenseNodesParent == null)
            {
                xmlLicenseNodesParent = objDataDoc.CreateElement("licenses");
                xmlRootLicenseFileNode.AppendChild(xmlLicenseNodesParent);
            }

            XPathNavigator xmlDataLicenseNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("licenses");
            if (xmlDataLicenseNodeList != null)
            {
                foreach (XPathNavigator xmlDataLicenseNode in xmlDataLicenseNodeList.Select("license"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlLicenseNodesParent.SelectSingleNode("license[text()=\"" + xmlDataLicenseNode.Value + "\"]") == null)
                    {
                        XmlNode xmlLicenseNode = objDataDoc.CreateElement("license");
                        xmlLicenseNode.InnerText = xmlDataLicenseNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataLicenseNode.Value;
                        xmlLicenseNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlLicenseNodesParent.AppendChild(xmlLicenseNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlLicenseNodeList = xmlLicenseNodesParent.SelectNodes("license"))
                {
                    if (xmlLicenseNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlLicenseNode in xmlLicenseNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataLicenseNodeList?.SelectSingleNode("license[text() = \"" + xmlLicenseNode.InnerText + "\"]") == null)
                            {
                                xmlLicenseNodesParent.RemoveChild(xmlLicenseNode);
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessLifestyles(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "lifestyles.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootLifestyleFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootLifestyleFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootLifestyleFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Lifestyles

            XmlNode xmlLifestyleNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("lifestyles");
            if (xmlLifestyleNodesParent == null)
            {
                xmlLifestyleNodesParent = objDataDoc.CreateElement("lifestyles");
                xmlRootLifestyleFileNode.AppendChild(xmlLifestyleNodesParent);
            }

            XPathNavigator xmlDataLifestyleNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("lifestyles");
            if (xmlDataLifestyleNodeList != null)
            {
                foreach (XPathNavigator xmlDataLifestyleNode in xmlDataLifestyleNodeList.Select("lifestyle"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataLifestyleName = xmlDataLifestyleNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataLifestyleId = xmlDataLifestyleNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataLifestyleNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataLifestyleNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlLifestyleNode.AppendChild(xmlPageElement);

                        xmlLifestyleNodesParent.AppendChild(xmlLifestyleNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlLifestyleNode in xmlLifestyleNodesParent.SelectNodes("lifestyle"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlLifestyleNode.Attributes != null)
                        for (int i = xmlLifestyleNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlLifestyleNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlLifestyleNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataLifestyleNodeList?.SelectSingleNode("lifestyle[id = \"" + xmlLifestyleNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlLifestyleNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlLifestyleNodesParent.RemoveChild(xmlLifestyleNode);
                        }
#endif
                    }
                }
            }

            // Process Lifestyle Qualities

            XmlNode xmlQualityNodesParent = xmlRootLifestyleFileNode.SelectSingleNode("qualities");
            if (xmlQualityNodesParent == null)
            {
                xmlQualityNodesParent = objDataDoc.CreateElement("qualities");
                xmlRootLifestyleFileNode.AppendChild(xmlQualityNodesParent);
            }

            XPathNavigator xmlDataQualityNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("qualities");
            if (xmlDataQualityNodeList != null)
            {
                foreach (XPathNavigator xmlDataQualityNode in xmlDataQualityNodeList.Select("quality"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataQualityId = xmlDataQualityNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataQualityName = xmlDataQualityNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataQualityNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataQualityNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlQualityNode.AppendChild(xmlPageElement);

                        xmlQualityNodesParent.AppendChild(xmlQualityNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlQualityNode in xmlQualityNodesParent.SelectNodes("quality"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlQualityNode.Attributes != null)
                        for (int i = xmlQualityNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlQualityNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlQualityNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataQualityNodeList?.SelectSingleNode("quality[id = \"" + xmlQualityNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlQualityNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlQualityNodesParent.RemoveChild(xmlQualityNode);
                        }
#endif
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

        private static void ProcessMartialArts(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "martialarts.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMartialArtFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMartialArtFileNode);
            }

            // Process Martial Arts

            XmlNode xmlMartialArtNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("martialarts");
            if (xmlMartialArtNodesParent == null)
            {
                xmlMartialArtNodesParent = objDataDoc.CreateElement("martialarts");
                xmlRootMartialArtFileNode.AppendChild(xmlMartialArtNodesParent);
            }

            XPathNavigator xmlDataMartialArtNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("martialarts");
            if (xmlDataMartialArtNodeList != null)
            {
                foreach (XPathNavigator xmlDataMartialArtNode in xmlDataMartialArtNodeList.Select("martialart"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMartialArtName = xmlDataMartialArtNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMartialArtId = xmlDataMartialArtNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMartialArtNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMartialArtNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMartialArtNode.AppendChild(xmlPageElement);

                        xmlMartialArtNodesParent.AppendChild(xmlMartialArtNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMartialArtNodeList = xmlMartialArtNodesParent.SelectNodes("martialart"))
                {
                    if (xmlMartialArtNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMartialArtNode in xmlMartialArtNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            // Remove Advantages from within MartialArt
                            XmlNode xmlRemoveAdvantageNode = xmlMartialArtNode.SelectSingleNode("advantages");
                            if (xmlRemoveAdvantageNode != null)
                            {
                                xmlMartialArtNode.RemoveChild(xmlRemoveAdvantageNode);
                            }

                            if (xmlMartialArtNode.Attributes != null)
                                for (int i = xmlMartialArtNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMartialArtNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMartialArtNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMartialArtNodeList?.SelectSingleNode("martialart[id = \"" + xmlMartialArtNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMartialArtNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMartialArtNodesParent.RemoveChild(xmlMartialArtNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            // Process Techniques

            XmlNode xmlTechniqueNodesParent = xmlRootMartialArtFileNode.SelectSingleNode("techniques");
            if (xmlTechniqueNodesParent == null)
            {
                xmlTechniqueNodesParent = objDataDoc.CreateElement("techniques");
                xmlRootMartialArtFileNode.AppendChild(xmlTechniqueNodesParent);
            }

            XPathNavigator xmlDataTechniqueNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("techniques");
            if (xmlDataTechniqueNodeList != null)
            {
                foreach (XPathNavigator xmlDataTechniqueNode in xmlDataTechniqueNodeList.Select("technique"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTechniqueId = xmlDataTechniqueNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataTechniqueName = xmlDataTechniqueNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataTechniqueNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataTechniqueNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlTechniqueNode.AppendChild(xmlPageElement);

                        xmlTechniqueNodesParent.AppendChild(xmlTechniqueNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlTechniqueNodeList = xmlTechniqueNodesParent.SelectNodes("technique"))
                {
                    if (xmlTechniqueNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlTechniqueNode in xmlTechniqueNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlTechniqueNode.Attributes != null)
                                for (int i = xmlTechniqueNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlTechniqueNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlTechniqueNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataTechniqueNodeList?.SelectSingleNode("technique[id = \"" + xmlTechniqueNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlTechniqueNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlTechniqueNodesParent.RemoveChild(xmlTechniqueNode);
                                }
#endif
                            }
                        }
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

        private static void ProcessMentors(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "mentors.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMentorFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMentorFileNode);
            }

            // Process Mentors

            XmlNode xmlMentorNodesParent = xmlRootMentorFileNode.SelectSingleNode("mentors");
            if (xmlMentorNodesParent == null)
            {
                xmlMentorNodesParent = objDataDoc.CreateElement("mentors");
                xmlRootMentorFileNode.AppendChild(xmlMentorNodesParent);
            }

            XPathNavigator xmlDataMentorNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("mentors");
            if (xmlDataMentorNodeList != null)
            {
                foreach (XPathNavigator xmlDataMentorNode in xmlDataMentorNodeList.Select("mentor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMentorName = xmlDataMentorNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMentorId = xmlDataMentorNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataMentorAdvantage = xmlDataMentorNode.SelectSingleNode("advantage")?.Value ?? string.Empty;
                    string strDataMentorDisadvantage = xmlDataMentorNode.SelectSingleNode("disadvantage")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMentorNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMentorNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMentorNode.AppendChild(xmlPageElement);

                        xmlMentorNodesParent.AppendChild(xmlMentorNode);
                    }

                    XPathNavigator xmlDataMentorChoicesNode = xmlDataMentorNode.SelectSingleNode("choices");
                    if (xmlDataMentorChoicesNode != null)
                    {
                        XmlNode xmlMentorChoicesNode = xmlMentorNode["choices"];
                        if (xmlMentorChoicesNode == null)
                        {
                            xmlMentorChoicesNode = objDataDoc.CreateElement("choices");
                            xmlMentorNode.AppendChild(xmlMentorChoicesNode);
                        }

                        foreach (XPathNavigator xmlDataChoiceNode in xmlDataMentorChoicesNode.Select("choice"))
                        {
                            if (objWorker.CancellationPending)
                                return;
                            string strDataChoiceName = xmlDataChoiceNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

                                foreach (XPathNavigator xmlDataChoiceNodeAttribute in xmlDataChoiceNode.SelectChildren(XPathNodeType.Attribute))
                                {
                                    if (objWorker.CancellationPending)
                                        return;
                                    XmlAttribute xmlChoiceNodeAttribute = objDataDoc.CreateAttribute(xmlDataChoiceNodeAttribute.Name);
                                    xmlChoiceNodeAttribute.Value = xmlDataChoiceNodeAttribute.Value;
                                    xmlChoiceNode.Attributes?.Append(xmlChoiceNodeAttribute);
                                }

                                xmlMentorChoicesNode.AppendChild(xmlChoiceNode);
                            }
                        }
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMentorNodeList = xmlMentorNodesParent.SelectNodes("mentor"))
                {
                    if (xmlMentorNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMentorNode in xmlMentorNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlMentorNode.Attributes != null)
                                for (int i = xmlMentorNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMentorNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMentorNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMentorNodeList?.SelectSingleNode("mentor[id = \"" + xmlMentorNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMentorNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMentorNodesParent.RemoveChild(xmlMentorNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessMetamagic(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "metamagic.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMetamagicFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetamagicFileNode);
            }

            // Process Streams

            XmlNode xmlMetamagicNodesParent = xmlRootMetamagicFileNode.SelectSingleNode("metamagics");
            if (xmlMetamagicNodesParent == null)
            {
                xmlMetamagicNodesParent = objDataDoc.CreateElement("metamagics");
                xmlRootMetamagicFileNode.AppendChild(xmlMetamagicNodesParent);
            }

            XPathNavigator xmlDataMetamagicNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("metamagics");
            if (xmlDataMetamagicNodeList != null)
            {
                foreach (XPathNavigator xmlDataMetamagicNode in xmlDataMetamagicNodeList.Select("metamagic"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetamagicName = xmlDataMetamagicNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMetamagicId = xmlDataMetamagicNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMetamagicNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMetamagicNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMetamagicNode.AppendChild(xmlPageElement);

                        xmlMetamagicNodesParent.AppendChild(xmlMetamagicNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMetamagicNodeList = xmlMetamagicNodesParent.SelectNodes("metamagic"))
                {
                    if (xmlMetamagicNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMetamagicNode in xmlMetamagicNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlMetamagicNode.Attributes != null)
                                for (int i = xmlMetamagicNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMetamagicNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMetamagicNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMetamagicNodeList?.SelectSingleNode("metamagic[id = \"" + xmlMetamagicNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMetamagicNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMetamagicNodesParent.RemoveChild(xmlMetamagicNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            // Process Arts

            XmlNode xmlArtNodesParent = xmlRootMetamagicFileNode.SelectSingleNode("arts");
            if (xmlArtNodesParent == null)
            {
                xmlArtNodesParent = objDataDoc.CreateElement("arts");
                xmlRootMetamagicFileNode.AppendChild(xmlArtNodesParent);
            }

            XPathNavigator xmlDataArtNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("arts");
            if (xmlDataArtNodeList != null)
            {
                foreach (XPathNavigator xmlDataArtNode in xmlDataArtNodeList.Select("art"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataArtId = xmlDataArtNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataArtName = xmlDataArtNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataArtNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataArtNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlArtNode.AppendChild(xmlPageElement);

                        xmlArtNodesParent.AppendChild(xmlArtNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlArtNodeList = xmlArtNodesParent.SelectNodes("art"))
                {
                    if (xmlArtNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlArtNode in xmlArtNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlArtNode.Attributes != null)
                                for (int i = xmlArtNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlArtNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlArtNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataArtNodeList?.SelectSingleNode("art[id = \"" + xmlArtNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlArtNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlArtNodesParent.RemoveChild(xmlArtNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessMetatypes(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "metatypes.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMetatypeFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Metatypes

            XmlNode xmlMetatypeNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("metatypes");
            if (xmlMetatypeNodesParent == null)
            {
                xmlMetatypeNodesParent = objDataDoc.CreateElement("metatypes");
                xmlRootMetatypeFileNode.AppendChild(xmlMetatypeNodesParent);
            }

            XPathNavigator xmlDataMetatypeNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("metatypes");
            if (xmlDataMetatypeNodeList != null)
            {
                foreach (XPathNavigator xmlDataMetatypeNode in xmlDataMetatypeNodeList.Select("metatype"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMetatypeId = xmlDataMetatypeNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }

                    // Process Metavariants
                    AuxProcessSubItems(xmlMetatypeNode, xmlDataMetatypeNode, "metavariants", "metavariant", true, objDataDoc, objWorker, blnRemoveTranslationIfSourceNotFound);
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMetatypeNodeList = xmlMetatypeNodesParent.SelectNodes("metatype"))
                {
                    if (xmlMetatypeNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlMetatypeNode.Attributes != null)
                                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMetatypeNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMetatypeNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessOptions(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "options.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootOptionFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootOptionFileNode);
            }

            // Process Black Market Pipeline Categories

            XmlNode xmlBlackMarketPipelineCategoryNodesParent = xmlRootOptionFileNode.SelectSingleNode("blackmarketpipelinecategories");

            if (xmlBlackMarketPipelineCategoryNodesParent == null)
            {
                xmlBlackMarketPipelineCategoryNodesParent = objDataDoc.CreateElement("blackmarketpipelinecategories");
                xmlRootOptionFileNode.AppendChild(xmlBlackMarketPipelineCategoryNodesParent);
            }

            XPathNavigator xmlDataBlackMarketPipelineCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("blackmarketpipelinecategories");
            if (xmlDataBlackMarketPipelineCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataBlackMarketPipelineCategoryNode in xmlDataBlackMarketPipelineCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlBlackMarketPipelineCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataBlackMarketPipelineCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlBlackMarketPipelineCategoryNode = objDataDoc.CreateElement("category");
                        xmlBlackMarketPipelineCategoryNode.InnerText = xmlDataBlackMarketPipelineCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataBlackMarketPipelineCategoryNode.Value;
                        xmlBlackMarketPipelineCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlBlackMarketPipelineCategoryNodesParent.AppendChild(xmlBlackMarketPipelineCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlBlackMarketPipelineCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlBlackMarketPipelineCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlBlackMarketPipelineCategoryNodesParent?.SelectSingleNode("category[text() = \"" + xmlBlackMarketPipelineCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlBlackMarketPipelineCategoryNodesParent.RemoveChild(xmlBlackMarketPipelineCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Limb Counts

            XmlNode xmlLimbCountNodesParent = xmlRootOptionFileNode.SelectSingleNode("limbcounts");
            if (xmlLimbCountNodesParent == null)
            {
                xmlLimbCountNodesParent = objDataDoc.CreateElement("limbcounts");
                xmlRootOptionFileNode.AppendChild(xmlLimbCountNodesParent);
            }

            XPathNavigator xmlDataLimbCountsNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("limbcounts");
            if (xmlDataLimbCountsNodeList != null)
            {
                foreach (XPathNavigator xmlDataLimbOptionNode in xmlDataLimbCountsNodeList.Select("limb"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataLimbOptionName = xmlDataLimbOptionNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlLimbOptionNode in xmlLimbCountNodesParent.SelectNodes("limb"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlLimbOptionNode.Attributes != null)
                        for (int i = xmlLimbOptionNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlLimbOptionNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlLimbOptionNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataLimbCountsNodeList?.SelectSingleNode("limb[name = " + xmlLimbOptionNode["name"]?.InnerText.CleanXPath() + "]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlLimbOptionNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlLimbCountNodesParent.RemoveChild(xmlLimbOptionNode);
                        }
#endif
                    }
                }
            }

            // Process PDF Options

            XmlNode xmlPDFArgumentNodesParent = xmlRootOptionFileNode.SelectSingleNode("pdfarguments");
            if (xmlPDFArgumentNodesParent == null)
            {
                xmlPDFArgumentNodesParent = objDataDoc.CreateElement("pdfarguments");
                xmlRootOptionFileNode.AppendChild(xmlPDFArgumentNodesParent);
            }

            XPathNavigator xmlDataPDFArgumentsNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("pdfarguments");
            if (xmlDataPDFArgumentsNodeList != null)
            {
                foreach (XPathNavigator xmlDataPDFArgumentNode in xmlDataPDFArgumentsNodeList.Select("pdfargument"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPDFArgumentName = xmlDataPDFArgumentNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlPDFArgumentNode in xmlPDFArgumentNodesParent.SelectNodes("pdfargument"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlPDFArgumentNode.Attributes != null)
                        for (int i = xmlPDFArgumentNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlPDFArgumentNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlPDFArgumentNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataPDFArgumentsNodeList?.SelectSingleNode("pdfargument[name = " + xmlPDFArgumentNode["name"]?.InnerText.CleanXPath() + "]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlPDFArgumentNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlPDFArgumentNodesParent.RemoveChild(xmlPDFArgumentNode);
                        }
#endif
                    }
                }
            }
        }

        private static void ProcessParagons(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "paragons.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootParagonFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootParagonFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootParagonFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootParagonFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Paragons

            XmlNode xmlParagonNodesParent = xmlRootParagonFileNode.SelectSingleNode("mentors");
            if (xmlParagonNodesParent == null)
            {
                xmlParagonNodesParent = objDataDoc.CreateElement("mentors");
                xmlRootParagonFileNode.AppendChild(xmlParagonNodesParent);
            }

            XPathNavigator xmlDataParagonNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("mentors");
            if (xmlDataParagonNodeList != null)
            {
                foreach (XPathNavigator xmlDataParagonNode in xmlDataParagonNodeList.Select("mentor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataParagonName = xmlDataParagonNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataParagonId = xmlDataParagonNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataParagonAdvantage = xmlDataParagonNode.SelectSingleNode("advantage")?.Value ?? string.Empty;
                    string strDataParagonDisadvantage = xmlDataParagonNode.SelectSingleNode("disadvantage")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataParagonNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataParagonNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlParagonNode.AppendChild(xmlPageElement);

                        xmlParagonNodesParent.AppendChild(xmlParagonNode);
                    }

                    XPathNavigator xmlDataParagonChoicesNode = xmlDataParagonNode.SelectSingleNode("choices");
                    if (xmlDataParagonChoicesNode != null)
                    {
                        XmlNode xmlParagonChoicesNode = xmlParagonNode["choices"];
                        if (xmlParagonChoicesNode == null)
                        {
                            xmlParagonChoicesNode = objDataDoc.CreateElement("choices");
                            xmlParagonNode.AppendChild(xmlParagonChoicesNode);
                        }

                        foreach (XPathNavigator xmlDataChoiceNode in xmlDataParagonChoicesNode.Select("choice"))
                        {
                            if (objWorker.CancellationPending)
                                return;
                            string strDataChoiceName = xmlDataChoiceNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

                                foreach (XPathNavigator xmlDataChoiceNodeAttribute in xmlDataChoiceNode.SelectChildren(XPathNodeType.Attribute))
                                {
                                    if (objWorker.CancellationPending)
                                        return;
                                    XmlAttribute xmlChoiceNodeAttribute = objDataDoc.CreateAttribute(xmlDataChoiceNodeAttribute.Name);
                                    xmlChoiceNodeAttribute.Value = xmlDataChoiceNodeAttribute.Value;
                                    xmlChoiceNode.Attributes?.Append(xmlChoiceNodeAttribute);
                                }

                                xmlParagonChoicesNode.AppendChild(xmlChoiceNode);
                            }
                        }
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlParagonNode in xmlParagonNodesParent.SelectNodes("mentor"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlParagonNode.Attributes != null)
                        for (int i = xmlParagonNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlParagonNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlParagonNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataParagonNodeList?.SelectSingleNode("mentor[id = \"" + xmlParagonNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlParagonNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlParagonNodesParent.RemoveChild(xmlParagonNode);
                        }
#endif
                    }
                }
            }

            // Remove Paragon nodes

            XmlNode xmlRemoveNode = xmlRootParagonFileNode.SelectSingleNode("paragons");
            if (xmlRemoveNode != null)
            {
                xmlRootParagonFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessPowers(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "powers.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootPowerFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = objDataDoc.CreateElement("powers");
                xmlRootPowerFileNode.AppendChild(xmlPowerNodesParent);
            }

            XPathNavigator xmlDataPowerNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("powers");
            if (xmlDataPowerNodeList != null)
            {
                foreach (XPathNavigator xmlDataPowerNode in xmlDataPowerNodeList.Select("power"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataPowerId = xmlDataPowerNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlPowerNodeList = xmlPowerNodesParent.SelectNodes("power"))
                {
                    if (xmlPowerNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlPowerNode in xmlPowerNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlPowerNode.Attributes != null)
                                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlPowerNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataPowerNodeList?.SelectSingleNode("power[id = \"" + xmlPowerNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlPowerNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            // Process Enhancements

            XmlNode xmlEnhancementNodesParent = xmlRootPowerFileNode.SelectSingleNode("enhancements");
            if (xmlEnhancementNodesParent == null)
            {
                xmlEnhancementNodesParent = objDataDoc.CreateElement("enhancements");
                xmlRootPowerFileNode.AppendChild(xmlEnhancementNodesParent);
            }

            XPathNavigator xmlDataEnhancementNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("enhancements");
            if (xmlDataEnhancementNodeList != null)
            {
                foreach (XPathNavigator xmlDataEnhancementNode in xmlDataEnhancementNodeList.Select("enhancement"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataEnhancementId = xmlDataEnhancementNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataEnhancementName = xmlDataEnhancementNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataEnhancementNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataEnhancementNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlEnhancementNode.AppendChild(xmlPageElement);

                        xmlEnhancementNodesParent.AppendChild(xmlEnhancementNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlEnhancementNodeList = xmlEnhancementNodesParent.SelectNodes("enhancement"))
                {
                    if (xmlEnhancementNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlEnhancementNode in xmlEnhancementNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlEnhancementNode.Attributes != null)
                                for (int i = xmlEnhancementNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlEnhancementNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlEnhancementNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataEnhancementNodeList?.SelectSingleNode("enhancement[id = \"" + xmlEnhancementNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlEnhancementNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlEnhancementNodesParent.RemoveChild(xmlEnhancementNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessPriorities(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "priorities.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootPriorityFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPriorityFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootPriorityFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootPriorityFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Priorities

            XmlNode xmlPriorityNodesParent = xmlRootPriorityFileNode.SelectSingleNode("priorities");
            if (xmlPriorityNodesParent == null)
            {
                xmlPriorityNodesParent = objDataDoc.CreateElement("priorities");
                xmlRootPriorityFileNode.AppendChild(xmlPriorityNodesParent);
            }

            XPathNavigator xmlDataPriorityNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("priorities");
            if (xmlDataPriorityNodeList != null)
            {
                foreach (XPathNavigator xmlDataPriorityNode in xmlDataPriorityNodeList.Select("priority"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPriorityName = xmlDataPriorityNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataPriorityId = xmlDataPriorityNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                    AuxProcessSubItems(xmlPriorityNode, xmlDataPriorityNode, "talents", "talent", false, objDataDoc, objWorker, blnRemoveTranslationIfSourceNotFound);
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlPriorityNodeList = xmlPriorityNodesParent.SelectNodes("priority"))
                {
                    if (xmlPriorityNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlPriorityNode in xmlPriorityNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlPriorityNode.Attributes != null)
                                for (int i = xmlPriorityNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlPriorityNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlPriorityNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataPriorityNodeList?.SelectSingleNode("priority[id = \"" + xmlPriorityNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlPriorityNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlPriorityNodesParent.RemoveChild(xmlPriorityNode);
                                }
#endif
                            }
                        }
                    }
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

        private static void ProcessPrograms(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "programs.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootProgramFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootProgramFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootProgramFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootProgramFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Programs

            XmlNode xmlProgramNodesParent = xmlRootProgramFileNode.SelectSingleNode("programs");
            if (xmlProgramNodesParent == null)
            {
                xmlProgramNodesParent = objDataDoc.CreateElement("programs");
                xmlRootProgramFileNode.AppendChild(xmlProgramNodesParent);
            }

            XPathNavigator xmlDataProgramNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("programs");
            if (xmlDataProgramNodeList != null)
            {
                foreach (XPathNavigator xmlDataProgramNode in xmlDataProgramNodeList.Select("program"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataProgramName = xmlDataProgramNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataProgramId = xmlDataProgramNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataProgramNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataProgramNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlProgramNode.AppendChild(xmlPageElement);

                        xmlProgramNodesParent.AppendChild(xmlProgramNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlProgramNodeList = xmlProgramNodesParent.SelectNodes("program"))
                {
                    if (xmlProgramNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlProgramNode in xmlProgramNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlProgramNode.Attributes != null)
                                for (int i = xmlProgramNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlProgramNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlProgramNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataProgramNodeList?.SelectSingleNode("program[id = \"" + xmlProgramNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlProgramNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlProgramNodesParent.RemoveChild(xmlProgramNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            // Remove Options

            XmlNode xmlRemoveNode = xmlRootProgramFileNode.SelectSingleNode("options");
            if (xmlRemoveNode != null)
            {
                xmlRootProgramFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessRanges(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "ranges.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootRangeFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootRangeFileNode);
            }

            // Process Ranges

            XmlNode xmlRangeNodesParent = xmlRootRangeFileNode.SelectSingleNode("ranges");
            if (xmlRangeNodesParent == null)
            {
                xmlRangeNodesParent = objDataDoc.CreateElement("ranges");
                xmlRootRangeFileNode.AppendChild(xmlRangeNodesParent);
            }

            XPathNavigator xmlDataRangeNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("ranges");
            if (xmlDataRangeNodeList != null)
            {
                foreach (XPathNavigator xmlDataRangeNode in xmlDataRangeNodeList.Select("range"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataRangeName = xmlDataRangeNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    XmlNode xmlRangeNode = xmlRangeNodesParent.SelectSingleNode("range[name = " + strDataRangeName.CleanXPath() + "]");
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

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlRangeNodeList = xmlRangeNodesParent.SelectNodes("range"))
                {
                    if (xmlRangeNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlRangeNode in xmlRangeNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlRangeNode.Attributes != null)
                                for (int i = xmlRangeNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlRangeNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlRangeNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataRangeNodeList?.SelectSingleNode("range[name = " + xmlRangeNode["name"]?.InnerText.CleanXPath() + "]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlRangeNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlRangeNodesParent.RemoveChild(xmlRangeNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessQualities(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "qualities.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootQualityFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootQualityFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootQualityFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootQualityFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Qualities

            XmlNode xmlQualityNodesParent = xmlRootQualityFileNode.SelectSingleNode("qualities");
            if (xmlQualityNodesParent == null)
            {
                xmlQualityNodesParent = objDataDoc.CreateElement("qualities");
                xmlRootQualityFileNode.AppendChild(xmlQualityNodesParent);
            }

            XPathNavigator xmlDataQualityNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("qualities");
            if (xmlDataQualityNodeList != null)
            {
                foreach (XPathNavigator xmlDataQualityNode in xmlDataQualityNodeList.Select("quality"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataQualityName = xmlDataQualityNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataQualityId = xmlDataQualityNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataQualityNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataQualityNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlQualityNode.AppendChild(xmlPageElement);

                        xmlQualityNodesParent.AppendChild(xmlQualityNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlQualityNodeList = xmlQualityNodesParent.SelectNodes("quality"))
                {
                    if (xmlQualityNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlQualityNode in xmlQualityNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlQualityNode.Attributes != null)
                                for (int i = xmlQualityNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlQualityNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlQualityNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataQualityNodeList?.SelectSingleNode("quality[id = \"" + xmlQualityNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlQualityNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlQualityNodesParent.RemoveChild(xmlQualityNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessSkills(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "skills.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootSkillFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootSkillFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootSkillFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootSkillFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        XmlAttribute xmlTypeAttribute = objDataDoc.CreateAttribute("type");
                        xmlTypeAttribute.Value = xmlDataCategoryNode.SelectSingleNode("@type")?.Value ?? string.Empty;
                        xmlCategoryNode.Attributes?.Append(xmlTypeAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Skill Groups

            XmlNode xmlSkillGroupNodesParent = xmlRootSkillFileNode.SelectSingleNode("skillgroups");

            if (xmlSkillGroupNodesParent == null)
            {
                xmlSkillGroupNodesParent = objDataDoc.CreateElement("skillgroups");
                xmlRootSkillFileNode.AppendChild(xmlSkillGroupNodesParent);
            }

            XPathNavigator xmlDataSkillGroupNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("skillgroups");
            if (xmlDataSkillGroupNodeList != null)
            {
                foreach (XPathNavigator xmlDataSkillGroupNode in xmlDataSkillGroupNodeList.Select("name"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlSkillGroupNodesParent.SelectSingleNode("name[text()=\"" + xmlDataSkillGroupNode.Value + "\"]") == null)
                    {
                        XmlNode xmlSkillGroupNode = objDataDoc.CreateElement("name");
                        xmlSkillGroupNode.InnerText = xmlDataSkillGroupNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataSkillGroupNode.Value;
                        xmlSkillGroupNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlSkillGroupNodesParent.AppendChild(xmlSkillGroupNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlSkillGroupNode in xmlSkillGroupNodesParent.SelectNodes("name"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataSkillGroupNodeList?.SelectSingleNode("name[text() = \"" + xmlSkillGroupNode.InnerText + "\"]") == null)
                    {
                        xmlSkillGroupNodesParent.RemoveChild(xmlSkillGroupNode);
                    }
                }
            }

            // Process Skills

            XmlNode xmlSkillNodesParent = xmlRootSkillFileNode.SelectSingleNode("skills");
            if (xmlSkillNodesParent == null)
            {
                xmlSkillNodesParent = objDataDoc.CreateElement("skills");
                xmlRootSkillFileNode.AppendChild(xmlSkillNodesParent);
            }

            XPathNavigator xmlDataSkillNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("skills");
            if (xmlDataSkillNodeList != null)
            {
                foreach (XPathNavigator xmlDataSkillNode in xmlDataSkillNodeList.Select("skill"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSkillName = xmlDataSkillNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataSkillId = xmlDataSkillNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataSkillNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataSkillNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlSkillNode.AppendChild(xmlPageElement);

                        xmlSkillNodesParent.AppendChild(xmlSkillNode);
                    }

                    XmlNode xmlSkillSpecsNode = xmlSkillNode["specs"];
                    if (xmlSkillSpecsNode == null)
                    {
                        xmlSkillSpecsNode = objDataDoc.CreateElement("specs");
                        xmlSkillNode.AppendChild(xmlSkillSpecsNode);
                    }

                    XPathNavigator xmlDataSkillSpecsNodeList = xmlDataSkillNode.SelectSingleNode("specs");
                    foreach (XPathNavigator xmlDataSpecNode in xmlDataSkillSpecsNodeList.Select("spec"))
                    {
                        if (objWorker.CancellationPending)
                            return;
                        string strSpecName = xmlDataSpecNode.Value;
                        XmlNode xmlSpecNode = xmlSkillSpecsNode.SelectSingleNode("spec[text()=\"" + strSpecName + "\"]");
                        if (xmlSpecNode == null)
                        {
                            xmlSpecNode = objDataDoc.CreateElement("spec");
                            xmlSpecNode.InnerText = strSpecName;
                            XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                            xmlTranslateAttribute.InnerText = strSpecName;
                            xmlSpecNode.Attributes?.Append(xmlTranslateAttribute);
                            xmlSkillSpecsNode.AppendChild(xmlSpecNode);
                        }
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlSkillNode in xmlSkillNodesParent.SelectNodes("skill"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlSkillNode.Attributes != null)
                        for (int i = xmlSkillNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlSkillNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlSkillNode.Attributes.RemoveAt(i);
                        }

                    XPathNavigator xmlDataSkillNode = xmlDataSkillNodeList?.SelectSingleNode("skill[id = \"" + xmlSkillNode["id"]?.InnerText + "\"]");
                    if (xmlDataSkillNode == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSkillNode.Attributes?.Append(xmlExistsAttribute);
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
                            if (xmlSkillNodeSpecsParent.Attributes != null)
                                for (int i = xmlSkillNodeSpecsParent.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlSkillNodeSpecsParent.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlSkillNodeSpecsParent.Attributes.RemoveAt(i);
                                }

                            XPathNavigator xmlDataSkillNodeSpecsParent = xmlDataSkillNode.SelectSingleNode("specs");
                            if (xmlDataSkillNodeSpecsParent == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSkillNodeSpecsParent.Attributes?.Append(xmlExistsAttribute);
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
                                            xmlSpecNode.Attributes?.Append(xmlExistsAttribute);
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

            // Process Knowledge Skills

            XmlNode xmlKnowledgeSkillNodesParent = xmlRootSkillFileNode.SelectSingleNode("knowledgeskills");
            if (xmlKnowledgeSkillNodesParent == null)
            {
                xmlKnowledgeSkillNodesParent = objDataDoc.CreateElement("knowledgeskills");
                xmlRootSkillFileNode.AppendChild(xmlKnowledgeSkillNodesParent);
            }

            XPathNavigator xmlDataKnowledgeSkillNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("knowledgeskills");
            if (xmlDataKnowledgeSkillNodeList != null)
            {
                foreach (XPathNavigator xmlDataKnowledgeSkillNode in xmlDataKnowledgeSkillNodeList.Select("skill"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataKnowledgeSkillId = xmlDataKnowledgeSkillNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataKnowledgeSkillName = xmlDataKnowledgeSkillNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

                    XPathNavigator xmlDataKnowledgeSkillSpecsNodeList = xmlDataKnowledgeSkillNode.SelectSingleNode("specs");
                    foreach (XPathNavigator xmlDataSpecNode in xmlDataKnowledgeSkillSpecsNodeList.Select("spec"))
                    {
                        if (objWorker.CancellationPending)
                            return;
                        string strSpecName = xmlDataSpecNode.Value;
                        XmlNode xmlSpecNode = xmlKnowledgeSkillSpecsNode.SelectSingleNode("spec[text()=\"" + strSpecName + "\"]");
                        if (xmlSpecNode == null)
                        {
                            xmlSpecNode = objDataDoc.CreateElement("spec");
                            xmlSpecNode.InnerText = strSpecName;
                            XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                            xmlTranslateAttribute.InnerText = strSpecName;
                            xmlSpecNode.Attributes?.Append(xmlTranslateAttribute);
                            xmlKnowledgeSkillSpecsNode.AppendChild(xmlSpecNode);
                        }
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlKnowledgeSkillNode in xmlKnowledgeSkillNodesParent.SelectNodes("skill"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlKnowledgeSkillNode.Attributes != null)
                        for (int i = xmlKnowledgeSkillNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlKnowledgeSkillNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlKnowledgeSkillNode.Attributes.RemoveAt(i);
                        }

                    XPathNavigator xmlDataKnowledgeSkillNode = xmlDataKnowledgeSkillNodeList?.SelectSingleNode("skill[id = \"" + xmlKnowledgeSkillNode["id"]?.InnerText + "\"]");
                    if (xmlDataKnowledgeSkillNode == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlKnowledgeSkillNode.Attributes?.Append(xmlExistsAttribute);
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
                            if (xmlSkillNodeSpecsParent.Attributes != null)
                                for (int i = xmlSkillNodeSpecsParent.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlSkillNodeSpecsParent.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlSkillNodeSpecsParent.Attributes.RemoveAt(i);
                                }

                            XPathNavigator xmlDataSkillNodeSpecsParent = xmlDataKnowledgeSkillNode.SelectSingleNode("specs");
                            if (xmlDataSkillNodeSpecsParent == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSkillNodeSpecsParent.Attributes?.Append(xmlExistsAttribute);
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
                                            xmlSpecNode.Attributes?.Append(xmlExistsAttribute);
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
        }

        private static void ProcessSpells(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "spells.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootSpellFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootSpellFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootSpellFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootSpellFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Spells

            XmlNode xmlSpellNodesParent = xmlRootSpellFileNode.SelectSingleNode("spells");
            if (xmlSpellNodesParent == null)
            {
                xmlSpellNodesParent = objDataDoc.CreateElement("spells");
                xmlRootSpellFileNode.AppendChild(xmlSpellNodesParent);
            }

            XPathNavigator xmlDataSpellNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("spells");
            if (xmlDataSpellNodeList != null)
            {
                foreach (XPathNavigator xmlDataSpellNode in xmlDataSpellNodeList.Select("spell"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpellName = xmlDataSpellNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataSpellId = xmlDataSpellNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataSpellNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataSpellNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlSpellNode.AppendChild(xmlPageElement);

                        xmlSpellNodesParent.AppendChild(xmlSpellNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlSpellNodeList = xmlSpellNodesParent.SelectNodes("spell"))
                {
                    if (xmlSpellNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlSpellNode in xmlSpellNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlSpellNode.Attributes != null)
                                for (int i = xmlSpellNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlSpellNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlSpellNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataSpellNodeList?.SelectSingleNode("spell[id = \"" + xmlSpellNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSpellNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlSpellNodesParent.RemoveChild(xmlSpellNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessSpiritPowers(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "spiritpowers.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootPowerFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootPowerFileNode);
            }

            // Process Powers

            XmlNode xmlPowerNodesParent = xmlRootPowerFileNode.SelectSingleNode("powers");
            if (xmlPowerNodesParent == null)
            {
                xmlPowerNodesParent = objDataDoc.CreateElement("powers");
                xmlRootPowerFileNode.AppendChild(xmlPowerNodesParent);
            }

            XPathNavigator xmlDataPowerNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("powers");
            if (xmlDataPowerNodeList != null)
            {
                foreach (XPathNavigator xmlDataPowerNode in xmlDataPowerNodeList.Select("power"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataPowerName = xmlDataPowerNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataPowerNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlPowerNode.AppendChild(xmlPageElement);

                        xmlPowerNodesParent.AppendChild(xmlPowerNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlPowerNodeList = xmlPowerNodesParent.SelectNodes("power"))
                {
                    if (xmlPowerNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlPowerNode in xmlPowerNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlPowerNode.Attributes != null)
                                for (int i = xmlPowerNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlPowerNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlPowerNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataPowerNodeList?.SelectSingleNode("power[name = " + xmlPowerNode["name"]?.InnerText.CleanXPath() + "]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlPowerNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlPowerNodesParent.RemoveChild(xmlPowerNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessStreams(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "streams.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootTraditionFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Streams

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = objDataDoc.CreateElement("traditions");
                xmlRootTraditionFileNode.AppendChild(xmlTraditionNodesParent);
            }

            XPathNavigator xmlDataTraditionNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("traditions");
            if (xmlDataTraditionNodeList != null)
            {
                foreach (XPathNavigator xmlDataTraditionNode in xmlDataTraditionNodeList.Select("tradition"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTraditionName = xmlDataTraditionNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataTraditionId = xmlDataTraditionNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataTraditionNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataTraditionNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlTraditionNodeList = xmlTraditionNodesParent.SelectNodes("tradition"))
                {
                    if (xmlTraditionNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlTraditionNode in xmlTraditionNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlTraditionNode.Attributes != null)
                                for (int i = xmlTraditionNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlTraditionNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlTraditionNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[id = \"" + xmlTraditionNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlTraditionNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                                }
#endif
                            }
                        }
                    }
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XPathNavigator xmlDataSpiritNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XPathNavigator xmlDataSpiritNode in xmlDataSpiritNodeList.Select("spirit"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpiritId = xmlDataSpiritNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataSpiritName = xmlDataSpiritNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataSpiritNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataSpiritNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlSpiritNodeList = xmlSpiritNodesParent.SelectNodes("spirit"))
                {
                    if (xmlSpiritNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlSpiritNode in xmlSpiritNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlSpiritNode.Attributes != null)
                                for (int i = xmlSpiritNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlSpiritNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlSpiritNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[id = \"" + xmlSpiritNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlSpiritNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessTips(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "tips.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

            XmlNode xmlRootNode = objDataDoc.SelectSingleNode("/chummer");
            if (xmlRootNode == null)
            {
                xmlRootNode = objDataDoc.CreateElement("chummer");
                objDataDoc.AppendChild(xmlRootNode);
            }

            XmlNode xmlRootTipFileNode = objDataDoc.SelectSingleNode("/chummer/chummer[@file = \"tips.xml\"]");
            if (xmlRootTipFileNode == null)
            {
                xmlRootTipFileNode = objDataDoc.CreateElement("chummer");
                XmlAttribute xmlAttribute = objDataDoc.CreateAttribute("file");
                xmlAttribute.Value = "tips.xml";
                xmlRootTipFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTipFileNode);
            }

            // Process Tips

            XmlNode xmlTipNodesParent = xmlRootTipFileNode.SelectSingleNode("tips");
            if (xmlTipNodesParent == null)
            {
                xmlTipNodesParent = objDataDoc.CreateElement("tips");
                xmlRootTipFileNode.AppendChild(xmlTipNodesParent);
            }

            XPathNavigator xmlDataTipNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("tips");
            if (xmlDataTipNodeList != null)
            {
                foreach (XPathNavigator xmlDataTipNode in xmlDataTipNodeList.Select("tip"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTipText = xmlDataTipNode.SelectSingleNode("text")?.Value ?? string.Empty;
                    string strDataTipId = xmlDataTipNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    XmlNode xmlTipNode = xmlTipNodesParent.SelectSingleNode("tip[id=\"" + strDataTipId + "\"]");
                    if (xmlTipNode != null)
                    {
                        if (xmlTipNode["id"] == null)
                        {
                            XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                            xmlIdElement.InnerText = strDataTipId;
                            xmlTipNode.PrependChild(xmlIdElement);
                        }

                        if (xmlTipNode["text"] == null)
                        {
                            XmlNode xmlNameElement = objDataDoc.CreateElement("text");
                            xmlNameElement.InnerText = strDataTipText;
                            xmlTipNode.AppendChild(xmlNameElement);
                        }

                        if (xmlTipNode["translate"] == null)
                        {
                            XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                            xmlTranslateElement.InnerText = strDataTipText;
                            xmlTipNode.AppendChild(xmlTranslateElement);
                        }
                    }
                    else
                    {
                        xmlTipNode = objDataDoc.CreateElement("tip");

                        XmlNode xmlIdElement = objDataDoc.CreateElement("id");
                        xmlIdElement.InnerText = strDataTipId;
                        xmlTipNode.AppendChild(xmlIdElement);

                        XmlNode xmlNameElement = objDataDoc.CreateElement("text");
                        xmlNameElement.InnerText = strDataTipText;
                        xmlTipNode.AppendChild(xmlNameElement);

                        XmlNode xmlTranslateElement = objDataDoc.CreateElement("translate");
                        xmlTranslateElement.InnerText = strDataTipText;
                        xmlTipNode.AppendChild(xmlTranslateElement);

                        xmlTipNodesParent.AppendChild(xmlTipNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlTipNodeList = xmlTipNodesParent.SelectNodes("tip"))
                {
                    if (xmlTipNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlTipNode in xmlTipNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlTipNode.Attributes != null)
                                for (int i = xmlTipNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlTipNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlTipNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataTipNodeList?.SelectSingleNode("tip[id = \"" + xmlTipNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlTipNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlTipNodesParent.RemoveChild(xmlTipNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessTraditions(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "traditions.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootTraditionFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootTraditionFileNode);
            }

            // Process Traditions

            XmlNode xmlTraditionNodesParent = xmlRootTraditionFileNode.SelectSingleNode("traditions");
            if (xmlTraditionNodesParent == null)
            {
                xmlTraditionNodesParent = objDataDoc.CreateElement("traditions");
                xmlRootTraditionFileNode.AppendChild(xmlTraditionNodesParent);
            }

            XPathNavigator xmlDataTraditionNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("traditions");
            if (xmlDataTraditionNodeList != null)
            {
                foreach (XPathNavigator xmlDataTraditionNode in xmlDataTraditionNodeList.Select("tradition"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataTraditionName = xmlDataTraditionNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataTraditionId = xmlDataTraditionNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataTraditionNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataTraditionNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlTraditionNode.AppendChild(xmlPageElement);

                        xmlTraditionNodesParent.AppendChild(xmlTraditionNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlTraditionNode in xmlTraditionNodesParent.SelectNodes("tradition"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlTraditionNode.Attributes != null)
                        for (int i = xmlTraditionNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlTraditionNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlTraditionNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataTraditionNodeList?.SelectSingleNode("tradition[id = \"" + xmlTraditionNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlTraditionNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlTraditionNodesParent.RemoveChild(xmlTraditionNode);
                        }
#endif
                    }
                }
            }

            // Process Spirits

            XmlNode xmlSpiritNodesParent = xmlRootTraditionFileNode.SelectSingleNode("spirits");
            if (xmlSpiritNodesParent == null)
            {
                xmlSpiritNodesParent = objDataDoc.CreateElement("spirits");
                xmlRootTraditionFileNode.AppendChild(xmlSpiritNodesParent);
            }

            XPathNavigator xmlDataSpiritNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("spirits");
            if (xmlDataSpiritNodeList != null)
            {
                foreach (XPathNavigator xmlDataSpiritNode in xmlDataSpiritNodeList.Select("spirit"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataSpiritId = xmlDataSpiritNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataSpiritName = xmlDataSpiritNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataSpiritNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataSpiritNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlSpiritNode.AppendChild(xmlPageElement);

                        xmlSpiritNodesParent.AppendChild(xmlSpiritNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlSpiritNode in xmlSpiritNodesParent.SelectNodes("spirit"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlSpiritNode.Attributes != null)
                        for (int i = xmlSpiritNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlSpiritNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlSpiritNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataSpiritNodeList?.SelectSingleNode("spirit[id = \"" + xmlSpiritNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlSpiritNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlSpiritNodesParent.RemoveChild(xmlSpiritNode);
                        }
#endif
                    }
                }
            }

            // Process Drain Attributes

            XmlNode xmlDrainAttributeNodesParent = xmlRootTraditionFileNode.SelectSingleNode("drainattributes");
            if (xmlDrainAttributeNodesParent == null)
            {
                xmlDrainAttributeNodesParent = objDataDoc.CreateElement("drainattributes");
                xmlRootTraditionFileNode.AppendChild(xmlDrainAttributeNodesParent);
            }

            XPathNavigator xmlDataDrainAttributeNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("drainattributes");
            if (xmlDataDrainAttributeNodeList != null)
            {
                foreach (XPathNavigator xmlDataDrainAttributeNode in xmlDataDrainAttributeNodeList.Select("drainattribute"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataDrainAttributeId = xmlDataDrainAttributeNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataDrainAttributeName = xmlDataDrainAttributeNode.SelectSingleNode("name")?.Value ?? string.Empty;
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

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlDrainAttributeNode in xmlDrainAttributeNodesParent.SelectNodes("drainattribute"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDrainAttributeNode.Attributes != null)
                        for (int i = xmlDrainAttributeNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlDrainAttributeNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlDrainAttributeNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataDrainAttributeNodeList?.SelectSingleNode("drainattribute[id = \"" + xmlDrainAttributeNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlDrainAttributeNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlDrainAttributeNodesParent.RemoveChild(xmlDrainAttributeNode);
                        }
#endif
                    }
                }
            }
        }

        private static void ProcessVehicles(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "vehicles.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootVehicleFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootVehicleFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootVehicleFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootVehicleFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Mod Categories

            XmlNode xmlModCategoryNodesParent = xmlRootVehicleFileNode.SelectSingleNode("modcategories");

            if (xmlModCategoryNodesParent == null)
            {
                xmlModCategoryNodesParent = objDataDoc.CreateElement("modcategories");
                xmlRootVehicleFileNode.AppendChild(xmlModCategoryNodesParent);
            }

            XPathNavigator xmlDataModCategoryNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("modcategories");
            if (xmlDataModCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataModCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlModCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlModCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlModCategoryNode in xmlModCategoryNodesParent.SelectNodes("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlDataModCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlModCategoryNode.InnerText + "\"]") == null)
                    {
                        xmlModCategoryNodesParent.RemoveChild(xmlModCategoryNode);
                    }
                }
            }

            // Process Vehicles

            XmlNode xmlVehicleNodesParent = xmlRootVehicleFileNode.SelectSingleNode("vehicles");
            if (xmlVehicleNodesParent == null)
            {
                xmlVehicleNodesParent = objDataDoc.CreateElement("vehicles");
                xmlRootVehicleFileNode.AppendChild(xmlVehicleNodesParent);
            }

            XPathNavigator xmlDataVehicleNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("vehicles");
            if (xmlDataVehicleNodeList != null)
            {
                foreach (XPathNavigator xmlDataVehicleNode in xmlDataVehicleNodeList.Select("vehicle"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataVehicleName = xmlDataVehicleNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataVehicleId = xmlDataVehicleNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataVehicleNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataVehicleNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlVehicleNode.AppendChild(xmlPageElement);

                        xmlVehicleNodesParent.AppendChild(xmlVehicleNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlVehicleNode in xmlVehicleNodesParent.SelectNodes("vehicle"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlVehicleNode.Attributes != null)
                        for (int i = xmlVehicleNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlVehicleNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlVehicleNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataVehicleNodeList?.SelectSingleNode("vehicle[id = \"" + xmlVehicleNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlVehicleNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlVehicleNodesParent.RemoveChild(xmlVehicleNode);
                        }
#endif
                    }
                }
            }

            // Process Vehicle Mods

            XmlNode xmlVehicleModNodesParent = xmlRootVehicleFileNode.SelectSingleNode("mods");
            if (xmlVehicleModNodesParent == null)
            {
                xmlVehicleModNodesParent = objDataDoc.CreateElement("mods");
                xmlRootVehicleFileNode.AppendChild(xmlVehicleModNodesParent);
            }

            XPathNavigator xmlDataVehicleModNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("mods");
            if (xmlDataVehicleModNodeList != null)
            {
                foreach (XPathNavigator xmlDataVehicleModNode in xmlDataVehicleModNodeList.Select("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataVehicleModId = xmlDataVehicleModNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataVehicleModName = xmlDataVehicleModNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataVehicleModNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataVehicleModNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlVehicleModNode.AppendChild(xmlPageElement);

                        xmlVehicleModNodesParent.AppendChild(xmlVehicleModNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlVehicleModNode in xmlVehicleModNodesParent.SelectNodes("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlVehicleModNode.Attributes != null)
                        for (int i = xmlVehicleModNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlVehicleModNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlVehicleModNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataVehicleModNodeList?.SelectSingleNode("mod[id = \"" + xmlVehicleModNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlVehicleModNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlVehicleModNodesParent.RemoveChild(xmlVehicleModNode);
                        }
#endif
                    }
                }
            }

            // Process Weapon Mounts

            XmlNode xmlWeaponMountNodesParent = xmlRootVehicleFileNode.SelectSingleNode("weaponmounts");
            if (xmlWeaponMountNodesParent == null)
            {
                xmlWeaponMountNodesParent = objDataDoc.CreateElement("weaponmounts");
                xmlRootVehicleFileNode.AppendChild(xmlWeaponMountNodesParent);
            }

            XPathNavigator xmlDataWeaponMountNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("weaponmounts");
            if (xmlDataWeaponMountNodeList != null)
            {
                foreach (XPathNavigator xmlDataWeaponMountNode in xmlDataWeaponMountNodeList.Select("weaponmount"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponMountId = xmlDataWeaponMountNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataWeaponMountName = xmlDataWeaponMountNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponMountNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataWeaponMountNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlWeaponMountNode.AppendChild(xmlPageElement);

                        xmlWeaponMountNodesParent.AppendChild(xmlWeaponMountNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlWeaponMountNode in xmlWeaponMountNodesParent.SelectNodes("weaponmount"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlWeaponMountNode.Attributes != null)
                        for (int i = xmlWeaponMountNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlWeaponMountNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlWeaponMountNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataWeaponMountNodeList?.SelectSingleNode("weaponmount[id = \"" + xmlWeaponMountNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponMountNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlWeaponMountNodesParent.RemoveChild(xmlWeaponMountNode);
                        }
#endif
                    }
                }
            }

            // Process Mods for Weapon Mounts

            XmlNode xmlWeaponMountModNodesParent = xmlRootVehicleFileNode.SelectSingleNode("weaponmountmods");
            if (xmlWeaponMountModNodesParent == null)
            {
                xmlWeaponMountModNodesParent = objDataDoc.CreateElement("weaponmountmods");
                xmlRootVehicleFileNode.AppendChild(xmlWeaponMountModNodesParent);
            }

            XPathNavigator xmlDataWeaponMountModNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("weaponmountmods");
            if (xmlDataWeaponMountModNodeList != null)
            {
                foreach (XPathNavigator xmlDataWeaponMountModNode in xmlDataWeaponMountModNodeList.Select("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponMountModId = xmlDataWeaponMountModNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataWeaponMountModName = xmlDataWeaponMountModNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponMountModNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataWeaponMountModNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlWeaponMountModNode.AppendChild(xmlPageElement);

                        xmlWeaponMountModNodesParent.AppendChild(xmlWeaponMountModNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlWeaponMountModNode in xmlWeaponMountModNodesParent.SelectNodes("mod"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlWeaponMountModNode.Attributes != null)
                        for (int i = xmlWeaponMountModNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlWeaponMountModNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlWeaponMountModNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataWeaponMountModNodeList?.SelectSingleNode("mod[id = \"" + xmlWeaponMountModNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponMountModNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlWeaponMountModNodesParent.RemoveChild(xmlWeaponMountModNode);
                        }
#endif
                    }
                }
            }

            // Remove Limits

            XmlNode xmlRemoveNode = xmlRootVehicleFileNode.SelectSingleNode("limits");
            if (xmlRemoveNode != null)
            {
                xmlRootVehicleFileNode.RemoveChild(xmlRemoveNode);
            }
        }

        private static void ProcessVessels(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "vessels.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootMetatypeFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootMetatypeFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootMetatypeFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Metatypes

            XmlNode xmlMetatypeNodesParent = xmlRootMetatypeFileNode.SelectSingleNode("metatypes");
            if (xmlMetatypeNodesParent == null)
            {
                xmlMetatypeNodesParent = objDataDoc.CreateElement("metatypes");
                xmlRootMetatypeFileNode.AppendChild(xmlMetatypeNodesParent);
            }

            XPathNavigator xmlDataMetatypeNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("metatypes");
            if (xmlDataMetatypeNodeList != null)
            {
                foreach (XPathNavigator xmlDataMetatypeNode in xmlDataMetatypeNodeList.Select("metatype"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataMetatypeName = xmlDataMetatypeNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataMetatypeId = xmlDataMetatypeNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataMetatypeNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlMetatypeNode.AppendChild(xmlPageElement);

                        xmlMetatypeNodesParent.AppendChild(xmlMetatypeNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlMetatypeNodeList = xmlMetatypeNodesParent.SelectNodes("metatype"))
                {
                    if (xmlMetatypeNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlMetatypeNode in xmlMetatypeNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlMetatypeNode.Attributes != null)
                                for (int i = xmlMetatypeNode.Attributes.Count - 1; i >= 0; --i)
                                {
                                    XmlAttribute xmlAttribute = xmlMetatypeNode.Attributes[i];
                                    if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                        xmlMetatypeNode.Attributes.RemoveAt(i);
                                }

                            if (xmlDataMetatypeNodeList?.SelectSingleNode("metatype[id = \"" + xmlMetatypeNode["id"]?.InnerText + "\"]") == null)
                            {
#if !DELETE
                            {
                                XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                xmlExistsAttribute.Value = "False";
                                xmlMetatypeNode.Attributes?.Append(xmlExistsAttribute);
                            }
#else
                                {
                                    xmlMetatypeNodesParent.RemoveChild(xmlMetatypeNode);
                                }
#endif
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessWeapons(XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlDocument xmlDataDocument = new XmlDocument();
            xmlDataDocument.Load(Path.Combine(PATH, "data", "weapons.xml"));
            XPathNavigator xmlDataDocumentBaseChummerNode = xmlDataDocument.GetFastNavigator().SelectSingleNode("/chummer");

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
                xmlRootWeaponFileNode.Attributes?.Append(xmlAttribute);
                xmlRootNode.AppendChild(xmlRootWeaponFileNode);
            }

            // Process Categories

            XmlNode xmlCategoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("categories");

            if (xmlCategoryNodesParent == null)
            {
                xmlCategoryNodesParent = objDataDoc.CreateElement("categories");
                xmlRootWeaponFileNode.AppendChild(xmlCategoryNodesParent);
            }

            XPathNavigator xmlDataCategoryNodeList = xmlDataDocumentBaseChummerNode?.SelectSingleNode("categories");
            if (xmlDataCategoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataCategoryNode in xmlDataCategoryNodeList.Select("category"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlCategoryNodesParent.SelectSingleNode("category[text()=\"" + xmlDataCategoryNode.Value + "\"]") == null)
                    {
                        XmlNode xmlCategoryNode = objDataDoc.CreateElement("category");
                        xmlCategoryNode.InnerText = xmlDataCategoryNode.Value;
                        XmlAttribute xmlTranslateAttribute = objDataDoc.CreateAttribute("translate");
                        xmlTranslateAttribute.Value = xmlDataCategoryNode.Value;
                        xmlCategoryNode.Attributes?.Append(xmlTranslateAttribute);
                        xmlCategoryNodesParent.AppendChild(xmlCategoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                using (XmlNodeList xmlCategoryNodeList = xmlCategoryNodesParent.SelectNodes("category"))
                {
                    if (xmlCategoryNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlCategoryNode in xmlCategoryNodeList)
                        {
                            if (objWorker.CancellationPending)
                                return;
                            if (xmlDataCategoryNodeList?.SelectSingleNode("category[text() = \"" + xmlCategoryNode.InnerText + "\"]") == null)
                            {
                                xmlCategoryNodesParent.RemoveChild(xmlCategoryNode);
                            }
                        }
                    }
                }
            }

            // Process Weapons

            XmlNode xmlWeaponNodesParent = xmlRootWeaponFileNode.SelectSingleNode("weapons");
            if (xmlWeaponNodesParent == null)
            {
                xmlWeaponNodesParent = objDataDoc.CreateElement("weapons");
                xmlRootWeaponFileNode.AppendChild(xmlWeaponNodesParent);
            }

            XPathNavigator xmlDataWeaponNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("weapons");
            if (xmlDataWeaponNodeList != null)
            {
                foreach (XPathNavigator xmlDataWeaponNode in xmlDataWeaponNodeList.Select("weapon"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataWeaponName = xmlDataWeaponNode.SelectSingleNode("name")?.Value ?? string.Empty;
                    string strDataWeaponId = xmlDataWeaponNode.SelectSingleNode("id")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataWeaponNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataWeaponNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlWeaponNode.AppendChild(xmlPageElement);

                        xmlWeaponNodesParent.AppendChild(xmlWeaponNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlWeaponNode in xmlWeaponNodesParent.SelectNodes("weapon"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlWeaponNode.Attributes != null)
                        for (int i = xmlWeaponNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlWeaponNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlWeaponNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataWeaponNodeList?.SelectSingleNode("weapon[id = \"" + xmlWeaponNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlWeaponNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlWeaponNodesParent.RemoveChild(xmlWeaponNode);
                        }
#endif
                    }
                }
            }

            // Process Weapon Mods

            XmlNode xmlAccessoryNodesParent = xmlRootWeaponFileNode.SelectSingleNode("accessories");
            if (xmlAccessoryNodesParent == null)
            {
                xmlAccessoryNodesParent = objDataDoc.CreateElement("accessories");
                xmlRootWeaponFileNode.AppendChild(xmlAccessoryNodesParent);
            }

            XPathNavigator xmlDataAccessoryNodeList = xmlDataDocumentBaseChummerNode.SelectSingleNode("accessories");
            if (xmlDataAccessoryNodeList != null)
            {
                foreach (XPathNavigator xmlDataAccessoryNode in xmlDataAccessoryNodeList.Select("accessory"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    string strDataAccessoryId = xmlDataAccessoryNode.SelectSingleNode("id")?.Value ?? string.Empty;
                    string strDataAccessoryName = xmlDataAccessoryNode.SelectSingleNode("name")?.Value ?? string.Empty;
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
                            string strPage = xmlPage?.InnerText ?? xmlDataAccessoryNode.SelectSingleNode("page")?.Value ?? string.Empty;
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
                        xmlPageElement.InnerText = xmlDataAccessoryNode.SelectSingleNode("page")?.Value ?? string.Empty;
                        xmlAccessoryNode.AppendChild(xmlPageElement);

                        xmlAccessoryNodesParent.AppendChild(xmlAccessoryNode);
                    }
                }
            }

            if (blnRemoveTranslationIfSourceNotFound)
            {
                foreach (XmlNode xmlAccessoryNode in xmlAccessoryNodesParent.SelectNodes("accessory"))
                {
                    if (objWorker.CancellationPending)
                        return;
                    if (xmlAccessoryNode.Attributes != null)
                        for (int i = xmlAccessoryNode.Attributes.Count - 1; i >= 0; --i)
                        {
                            XmlAttribute xmlAttribute = xmlAccessoryNode.Attributes[i];
                            if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                xmlAccessoryNode.Attributes.RemoveAt(i);
                        }

                    if (xmlDataAccessoryNodeList?.SelectSingleNode("accessory[id = \"" + xmlAccessoryNode["id"]?.InnerText + "\"]") == null)
                    {
#if !DELETE
                    {
                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                        xmlExistsAttribute.Value = "False";
                        xmlAccessoryNode.Attributes?.Append(xmlExistsAttribute);
                    }
#else
                        {
                            xmlAccessoryNodesParent.RemoveChild(xmlAccessoryNode);
                        }
#endif
                    }
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
        private static void AuxProcessSubItems(XmlNode xmlItemNode, XPathNavigator xmlDataItemNode, string strSubItemParent, string strSubItem, bool blnProcessPages, XmlDocument objDataDoc, BackgroundWorker objWorker, bool blnRemoveTranslationIfSourceNotFound)
        {
            XmlNode xmlSubItemsParent = xmlItemNode.SelectSingleNode(strSubItemParent);
            XPathNavigator xmlDataSubItemsList = xmlDataItemNode.SelectSingleNode(strSubItemParent);

            if (xmlDataSubItemsList != null)
            {
                if (xmlSubItemsParent == null)
                {
                    xmlSubItemsParent = objDataDoc.CreateElement(strSubItemParent);
                    xmlItemNode.AppendChild(xmlSubItemsParent);
                }

                foreach (XPathNavigator xmlDataSubItem in xmlDataSubItemsList.Select(strSubItem))
                {
                    if (objWorker?.CancellationPending ?? false)
                        return;
                    string strDataSubItemName = xmlDataSubItem.SelectSingleNode("name")?.Value ?? string.Empty;
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
                                string strPage = xmlPage?.InnerText ?? xmlDataSubItem.SelectSingleNode("page")?.Value ?? string.Empty;
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
                            xmlPageElement.InnerText = xmlDataSubItem.SelectSingleNode("page")?.Value ?? string.Empty;
                            xmlSubItem.AppendChild(xmlPageElement);
                        }

                        xmlSubItemsParent.AppendChild(xmlSubItem);
                    }
                }

                if (blnRemoveTranslationIfSourceNotFound)
                {
                    using (XmlNodeList xmlSubItemList = xmlSubItemsParent.SelectNodes(strSubItem))
                    {
                        if (xmlSubItemList?.Count > 0)
                        {
                            foreach (XmlNode xmlSubItem in xmlSubItemList)
                            {
                                if (objWorker?.CancellationPending ?? false)
                                    return;
                                if (xmlSubItem.Attributes != null)
                                {
                                    for (int i = xmlSubItem.Attributes.Count - 1; i >= 0; --i)
                                    {
                                        XmlAttribute xmlAttribute = xmlSubItem.Attributes[i];
                                        if (xmlAttribute.Name != "translated" && !xmlAttribute.Name.StartsWith("xml:"))
                                            xmlSubItem.Attributes.RemoveAt(i);
                                    }
                                }

                                if (xmlDataSubItemsList.SelectSingleNode(strSubItem + "[name = " + xmlSubItem["name"]?.InnerText.CleanXPath() + "]") == null)
                                {
#if !DELETE
                                    {
                                        XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                                        xmlExistsAttribute.Value = "False";
                                        xmlSubItem.Attributes?.Append(xmlExistsAttribute);
                                    }
#else
                                    {
                                        xmlSubItemsParent.RemoveChild(xmlSubItem);
                                    }
#endif
                                }
                            }
                        }
                    }
                }
            }
            else if (xmlSubItemsParent != null && blnRemoveTranslationIfSourceNotFound)
            {
#if !DELETE
                        {
                            XmlAttribute xmlExistsAttribute = objDataDoc.CreateAttribute("exists");
                            xmlExistsAttribute.Value = "False";
                            xmlSubItem.Attributes?.Append(xmlExistsAttribute);
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

        public IList<frmTranslate> OpenTranslateWindows => s_LstOpenTranslateWindows;

        #endregion
    }
}
