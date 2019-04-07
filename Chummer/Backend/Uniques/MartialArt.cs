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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Martial Art.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class MartialArt : IHasChildren<MartialArtTechnique>, IHasName, IHasInternalId, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intKarmaCost = 7;
        private int _intRating = 1;
        private readonly TaggedObservableCollection<MartialArtTechnique> _lstTechniques = new TaggedObservableCollection<MartialArtTechnique>();
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        private bool _blnIsQuality;

        #region Create, Save, Load, and Print Methods
        public MartialArt(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _guiID = Guid.NewGuid();
        }

        /// Create a Martial Art from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        public void Create(XmlNode objXmlArtNode)
        {
            if (!objXmlArtNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warning(new object[] { "Missing id field for xmlnode", objXmlArtNode });
                Utils.BreakIfDebug();
            }
            if (objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlArtNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            if (!objXmlArtNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArtNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _blnIsQuality = objXmlArtNode["isquality"]?.InnerText == bool.TrueString;

            if (objXmlArtNode["bonus"] != null)
            {
                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MartialArt, InternalId,
                    objXmlArtNode["bonus"], false, 1, DisplayNameShort(GlobalOptions.Language));
            }

            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetTextFromPDF($"{_strSource} {_strPage}", _strName);
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPDF($"{Source} {Page(GlobalOptions.Language)}", DisplayName(GlobalOptions.Language));
                }
            }*/
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, Page(GlobalOptions.Language), GlobalOptions.Language));

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("cost", _intKarmaCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isquality", _blnIsQuality.ToString());
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objTechnique in _lstTechniques)
            {
                objTechnique.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
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
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode["sourceid"] == null || !objNode.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetField("id", Guid.TryParse, out _guiSourceID);
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("cost", ref _intKarmaCost);
            objNode.TryGetBoolFieldQuickly("isquality", ref _blnIsQuality);

            using (XmlNodeList xmlLegacyTechniqueList = objNode.SelectNodes("martialartadvantages/martialartadvantage"))
                if (xmlLegacyTechniqueList != null)
                    foreach (XmlNode nodTechnique in xmlLegacyTechniqueList)
                    {
                        MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                        objTechnique.Load(nodTechnique);
                        _lstTechniques.Add(objTechnique);
                    }

            using (XmlNodeList xmlTechniqueList = objNode.SelectNodes("martialarttechniques/martialarttechnique"))
                if (xmlTechniqueList != null)
                    foreach (XmlNode nodTechnique in xmlTechniqueList)
                    {
                        MartialArtTechnique objTechnique = new MartialArtTechnique(_objCharacter);
                        objTechnique.Load(nodTechnique);
                        _lstTechniques.Add(objTechnique);
                    }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("martialart");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("cost", Cost.ToString(objCulture));
            objWriter.WriteStartElement("martialarttechniques");
            foreach (MartialArtTechnique objAdvantage in Techniques)
            {
                objAdvantage.Print(objWriter, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
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
        public string SourceIDString => _guiSourceID.ToString("D");

        public string InternalId => _guiID.ToString("D");

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

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
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
        /// Selected Martial Arts Advantages.
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
                _objCachedMyXmlNode = XmlManager.Load("martialarts.xml", strLanguage).SelectSingleNode("/chummer/martialarts/martialart[id = \"" + SourceIDString + "\"]");
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
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsMartialArt,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
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
            bool blnAddAgain;
            do
            {
                frmSelectMartialArt frmPickMartialArt = new frmSelectMartialArt(objCharacter);
                frmPickMartialArt.ShowDialog();

                if (frmPickMartialArt.DialogResult == DialogResult.Cancel)
                    return false;

                blnAddAgain = frmPickMartialArt.AddAgain;
                // Open the Martial Arts XML file and locate the selected piece.
                XmlDocument objXmlDocument = XmlManager.Load("martialarts.xml");

                XmlNode objXmlArt = objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[id = \"" + frmPickMartialArt.SelectedMartialArt + "\"]");

                MartialArt objMartialArt = new MartialArt(objCharacter);
                objMartialArt.Create(objXmlArt);
                
                if (objCharacter.Created)
                {
                    int intKarmaCost = objMartialArt.Rating * objMartialArt.Cost * objCharacter.Options.KarmaQuality;
                    if (intKarmaCost > objCharacter.Karma)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        ImprovementManager.RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);
                        return false;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(objCharacter);
                    objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseLearnMartialArt", GlobalOptions.Language) + ' ' + objMartialArt.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                    objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    objCharacter.Karma -= intKarmaCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddMartialArt, objMartialArt.InternalId);
                    objExpense.Undo = objUndo;
                }
                objCharacter.MartialArts.Add(objMartialArt);
            } while (blnAddAgain);

            return true;
        }

        public bool Remove(Character objCharacter, bool blnConfirmDelete = true)
        {
            // Delete the selected Martial Art.
            if (IsQuality) return false;
            if (blnConfirmDelete)
            {
                if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt",
                    GlobalOptions.Language)))
                    return false;
            }

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.MartialArt,
                InternalId);
            // Remove the Improvements for any Advantages for the Martial Art that is being removed.
            foreach (MartialArtTechnique objAdvantage in Techniques)
            {
                ImprovementManager.RemoveImprovements(_objCharacter,
                    Improvement.ImprovementSource.MartialArtTechnique, objAdvantage.InternalId);
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
                    return Color.SaddleBrown;
                }
                if (IsQuality)
                {
                    return SystemColors.GrayText;
                }
                return SystemColors.WindowText;
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
