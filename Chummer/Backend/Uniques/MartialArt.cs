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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Martial Art.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class MartialArt : IHasChildren<MartialArtTechnique>, IHasName, IHasInternalId, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intKarmaCost = 7;
        private readonly TaggedObservableCollection<MartialArtTechnique> _lstTechniques = new TaggedObservableCollection<MartialArtTechnique>();
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private bool _blnIsQuality;

        #region Create, Save, Load, and Print Methods
        public MartialArt(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _guiID = Guid.NewGuid();

            _lstTechniques.CollectionChanged += TechniquesOnCollectionChanged;
        }

        private void TechniquesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _objCharacter?.IsLoading == false)
            {
                Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties =
                    new Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>();
                foreach (MartialArtTechnique objNewItem in e.NewItems)
                {
                    // Needed in order to properly process named sources where
                    // the tooltip was built before the object was added to the character
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.SourceName == objNewItem.InternalId && objImprovement.Enabled)
                        {
                            foreach (Tuple<INotifyMultiplePropertyChanged, string> tuplePropertyChanged in objImprovement.GetRelevantPropertyChangers())
                            {
                                if (dicChangedProperties.TryGetValue(tuplePropertyChanged.Item1, out HashSet<string> setChangedProperties))
                                    setChangedProperties.Add(tuplePropertyChanged.Item2);
                                else
                                    dicChangedProperties.Add(tuplePropertyChanged.Item1, new HashSet<string> { tuplePropertyChanged.Item2 });
                            }
                        }
                    }
                }
                foreach (INotifyMultiplePropertyChanged objToProcess in dicChangedProperties.Keys)
                {
                    objToProcess.OnMultiplePropertyChanged(dicChangedProperties[objToProcess].ToArray());
                }
            }
        }

        /// Create a Martial Art from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        public void Create(XmlNode objXmlArtNode)
        {
            if (!objXmlArtNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlArtNode });
                Utils.BreakIfDebug();
            }
            if (objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlArtNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            if (!objXmlArtNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArtNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
            _blnIsQuality = objXmlArtNode["isquality"]?.InnerText == bool.TrueString;

            if (objXmlArtNode["bonus"] != null)
            {
                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArt, InternalId,
                    objXmlArtNode["bonus"], 1, DisplayNameShort(GlobalOptions.Language));
            }
            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (objXmlArtNode.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                    !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strQualityNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + Page, strEnglishNameOnPage, _objCharacter);

                if (string.IsNullOrEmpty(strQualityNotes) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    string strTranslatedNameOnPage = CurrentDisplayName;

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (objXmlArtNode.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalOptions.Language),
                            strTranslatedNameOnPage, _objCharacter);
                    }
                }
                else
                    Notes = strQualityNotes;
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo, _objCharacter);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("cost", _intKarmaCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isquality", _blnIsQuality.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objTechnique in _lstTechniques)
            {
                objTechnique.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteEndElement();

            if (!IsQuality)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Martial Art from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            objNode.TryGetBoolFieldQuickly("isquality", ref _blnIsQuality);

            using (XmlNodeList xmlLegacyTechniqueList = objNode.SelectNodes("martialartadvantages/martialartadvantage"))
            {
                if (xmlLegacyTechniqueList != null)
                {
                    foreach (XmlNode nodTechnique in xmlLegacyTechniqueList)
                    {
                        MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                        objTechnique.Load(nodTechnique);
                        _lstTechniques.Add(objTechnique);
                    }
                }
            }

            using (XmlNodeList xmlTechniqueList = objNode.SelectNodes("martialarttechniques/martialarttechnique"))
            {
                if (xmlTechniqueList != null)
                {
                    foreach (XmlNode nodTechnique in xmlTechniqueList)
                    {
                        MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                        objTechnique.Load(nodTechnique);
                        _lstTechniques.Add(objTechnique);
                    }
                }
            }

            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public async void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", await _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("cost", Cost.ToString(objCulture));
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objTechnique in Techniques)
            {
                objTechnique.Print(objWriter, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }


        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Karma Cost (usually 7).
        /// </summary>
        public int Cost
        {
            get => _intKarmaCost;
            set => _intKarmaCost = value;
        }

        /// <summary>
        /// Is from a quality.
        /// </summary>
        public bool IsQuality
        {
            get => _blnIsQuality;
            set => _blnIsQuality = value;
        }

        /// <summary>
        /// Selected Martial Arts Techniques.
        /// </summary>
        public TaggedObservableCollection<MartialArtTechnique> Techniques => _lstTechniques;
        public TaggedObservableCollection<MartialArtTechnique> Children => Techniques;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("martialarts.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/martialarts/martialart[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/martialarts/martialart[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsMartialArt, ContextMenuStrip cmsMartialArtTechnique)
        {
            if (IsQuality && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsMartialArt,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            foreach (MartialArtTechnique objTechnique in Techniques)
            {
                TreeNode objLoopNode = objTechnique.CreateTreeNode(cmsMartialArtTechnique);
                if (objLoopNode != null)
                {
                    lstChildNodes.Add(objLoopNode);
                    objNode.Expand();
                }
            }

            return objNode;
        }

        public static bool Purchase(Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException    (nameof(objCharacter));
            bool blnAddAgain;
            do
            {
                using (frmSelectMartialArt frmPickMartialArt = new frmSelectMartialArt(objCharacter))
                {
                    frmPickMartialArt.ShowDialog(Program.MainForm);

                    if (frmPickMartialArt.DialogResult == DialogResult.Cancel)
                        return false;

                    blnAddAgain = frmPickMartialArt.AddAgain;
                    // Open the Martial Arts XML file and locate the selected piece.
                    XmlNode objXmlArt = objCharacter.LoadData("martialarts.xml").SelectSingleNode("/chummer/martialarts/martialart[id = " + frmPickMartialArt.SelectedMartialArt.CleanXPath() + "]");

                    MartialArt objMartialArt = new MartialArt(objCharacter);
                    objMartialArt.Create(objXmlArt);

                    if (objCharacter.Created)
                    {
                        int intKarmaCost = objMartialArt.Cost;
                        if (intKarmaCost > objCharacter.Karma)
                        {
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);
                            return false;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
                        objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseLearnMartialArt") + ' ' + objMartialArt.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma,
                            DateTime.Now);
                        objCharacter.ExpenseEntries.AddWithSort(objExpense);
                        objCharacter.Karma -= intKarmaCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddMartialArt, objMartialArt.InternalId);
                        objExpense.Undo = objUndo;
                    }

                    objCharacter.MartialArts.Add(objMartialArt);
                }
            } while (blnAddAgain);

            return true;
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            // Delete the selected Martial Art.
            if (IsQuality) return false;
            if (blnConfirmDelete)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt")))
                    return false;
            }

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.MartialArt,
                InternalId);
            // Remove the Improvements for any Techniques for the Martial Art that is being removed.
            foreach (MartialArtTechnique objTechnique in Techniques)
            {
                ImprovementManager.RemoveImprovements(_objCharacter,
                    Improvement.ImprovementSource.MartialArtTechnique, objTechnique.InternalId);
            }

            _objCharacter.MartialArts.Remove(this);
            return true;
        }
        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return IsQuality
                        ? ColorManager.GrayHasNotesColor
                        : ColorManager.HasNotesColor;
                }
                return IsQuality
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
