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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameShort(GlobalSettings.DefaultLanguage)}")]
    public class MentorSpirit : IHasInternalId, IHasName, IHasXmlDataNode, IHasSource
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strAdvantage = string.Empty;
        private string _strDisadvantage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodChoice1;
        private XmlNode _nodChoice2;
        private Improvement.ImprovementType _eMentorType;
        private Guid _guiSourceID;
        private readonly Character _objCharacter;
        private bool _blnMentorMask;

        #region Constructor

        public MentorSpirit(Character objCharacter, XmlNode xmlNodeMentor = null)
        {
            // Create the GUID for the new Mentor Spirit.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            XmlNode namenode = xmlNodeMentor?.SelectSingleNode("name");
            if (namenode != null)
                Name = namenode.InnerText;
            XmlNode typenode = xmlNodeMentor?.SelectSingleNode("mentortype");
            if (typenode != null && Enum.TryParse(typenode.InnerText, true, out Improvement.ImprovementType outEnum))
            {
                _eMentorType = outEnum;
            }
        }

        /// <summary>
        /// Create a Mentor Spirit from an XmlNode.
        /// </summary>
        /// <param name="xmlMentor">XmlNode to create the object from.</param>
        /// <param name="eMentorType">Whether this is a Mentor or a Paragon.</param>
        /// <param name="strForceValueChoice1">Name/Text for Choice 1.</param>
        /// <param name="strForceValueChoice2">Name/Text for Choice 2.</param>
        /// <param name="strForceValue">Force a value to be selected for the Mentor Spirit.</param>
        public void Create(XmlNode xmlMentor, Improvement.ImprovementType eMentorType, string strForceValue = "", string strForceValueChoice1 = "", string strForceValueChoice2 = "")
        {
            _eMentorType = eMentorType;
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            xmlMentor.TryGetStringFieldQuickly("name", ref _strName);
            xmlMentor.TryGetStringFieldQuickly("source", ref _strSource);
            xmlMentor.TryGetStringFieldQuickly("page", ref _strPage);
            if (!xmlMentor.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                xmlMentor.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetBookNotes(xmlMentor, Name, DisplayNameShort(GlobalSettings.Language), Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            if (!xmlMentor.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", xmlMentor });
                Utils.BreakIfDebug();
            }

            // Cache the English list of advantages gained through the Mentor Spirit.
            xmlMentor.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
            xmlMentor.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);

            _nodBonus = xmlMentor["bonus"];
            if (_nodBonus != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodBonus, 1, DisplayNameShort(GlobalSettings.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                _strExtra = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (!string.IsNullOrEmpty(strForceValue))
            {
                _strExtra = strForceValue;
            }
            _nodChoice1 = xmlMentor.SelectSingleNode("choices/choice[name = " + strForceValueChoice1.CleanXPath() + "]/bonus");
            if (_nodChoice1 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                //ImprovementManager.ForcedValue = strForceValueChoice1;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodChoice1, 1, DisplayNameShort(GlobalSettings.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (string.IsNullOrEmpty(_strExtra))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice1))
            {
                _strExtra = strForceValueChoice1;
            }
            _nodChoice2 = xmlMentor.SelectSingleNode("choices/choice[name = " + strForceValueChoice2.CleanXPath() + "]/bonus");
            if (_nodChoice2 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                //ImprovementManager.ForcedValue = strForceValueChoice2;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), _nodChoice2, 1, DisplayNameShort(GlobalSettings.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (string.IsNullOrEmpty(_strExtra))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldForce;
                ImprovementManager.SelectedValue = strOldSelected;
            }
            else if (string.IsNullOrEmpty(_strExtra) && !string.IsNullOrEmpty(strForceValueChoice2))
            {
                _strExtra = strForceValueChoice2;
            }

            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetTextFromPdf(_strSource + ' ' + _strPage, _strName);
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalSettings.Language), CurrentDisplayName);
                }
            }
            */
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("mentorspirit");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("mentortype", _eMentorType.ToString());
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("advantage", _strAdvantage);
            objWriter.WriteElementString("disadvantage", _strDisadvantage);
            objWriter.WriteElementString("mentormask", _blnMentorMask.ToString(GlobalSettings.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodChoice1 != null)
                objWriter.WriteRaw("<choice1>" + _nodChoice1.InnerXml + "</choice1>");
            else
                objWriter.WriteElementString("choice1", string.Empty);
            if (_nodChoice2 != null)
                objWriter.WriteRaw("<choice2>" + _nodChoice2.InnerXml + "</choice2>");
            else
                objWriter.WriteElementString("choice2", string.Empty);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));

            if (SourceID != Guid.Empty && !string.IsNullOrEmpty(SourceIDString))
            {
                objWriter.WriteElementString("id", SourceIDString);
            }

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Mentor Spirit from the XmlNode.
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
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            if (objNode["mentortype"] != null)
            {
                _eMentorType = Improvement.ConvertToImprovementType(objNode["mentortype"].InnerText);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
            Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(this.GetNodeXPath);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                if (objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID) == false)
                {
                    _objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode("/chummer/mentors/mentor[name = " + Name.CleanXPath() + "]")?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (_objCharacter.LastSavedVersion <= new Version(5, 217, 31))
            {
                // Cache advantages from data file because localized version used to be cached directly.
                XPathNavigator node = objMyNode.Value;
                if (node != null)
                {
                    if (!node.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage))
                        objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                    if (!node.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage))
                        objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
                }
                else
                {
                    objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                    objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
                }
            }
            else
            {
                objNode.TryGetMultiLineStringFieldQuickly("advantage", ref _strAdvantage);
                objNode.TryGetMultiLineStringFieldQuickly("disadvantage", ref _strDisadvantage);
            }
            objNode.TryGetBoolFieldQuickly("mentormask", ref _blnMentorMask);
            _nodBonus = objNode["bonus"];
            _nodChoice1 = objNode["choice1"];
            _nodChoice2 = objNode["choice2"];
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("mentorspirit");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("mentortype", _eMentorType.ToString());
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("advantage", DisplayAdvantage(strLanguageToPrint));
            objWriter.WriteElementString("disadvantage", DisplayDisadvantage(strLanguageToPrint));
            objWriter.WriteElementString("advantage_english", Advantage);
            objWriter.WriteElementString("disadvantage_english", Disadvantage);
            objWriter.WriteElementString("extra", _objCharacter.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("mentormask", MentorMask.ToString(GlobalSettings.InvariantCultureInfo));
            if (GlobalSettings.PrintNotes)
                objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteEndElement();
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name of the Mentor Spirit or Paragon.
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_strName) && _objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                {
                    _strName = _objCharacter.MentorSpirits[0].Name;
                }
                return _strName;
            }
            set
            {
                if (_strName != value)
                {
                    if (SourceID == Guid.Empty)
                    {
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                    _strName = value;
                    if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayName));
                }
            }
        }

        /// <summary>
        /// Extra string related to improvements selected for the Mentor Spirit or Paragon.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set
            {
                if (_strExtra != value)
                {
                    _strExtra = value;
                    if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayName));
                }
            }
        }

        /// <summary>
        /// Whether the Mentor Spirit is taken with a Mentor Mask.
        /// </summary>
        public bool MentorMask
        {
            get => _blnMentorMask;
            set => _blnMentorMask = value;
        }

        /// <summary>
        /// Advantage of the Mentor Spirit or Paragon (in English).
        /// </summary>
        public string Advantage => _strAdvantage;

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI. Advantage (Extra).
        /// </summary>
        public string DisplayAdvantage(string strLanguage)
        {
            string strReturn = Advantage;
            if (strLanguage != GlobalSettings.DefaultLanguage)
            {
                string strTemp = string.Empty;
                if (GetNodeXPath(strLanguage)?.TryGetMultiLineStringFieldQuickly("altadvantage", ref strTemp) == true)
                    strReturn = strTemp;
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// Disadvantage of the Mentor Spirit or Paragon (in English).
        /// </summary>
        public string Disadvantage => _strDisadvantage;

        /// <summary>
        /// Disadvantage of the mentor as it should be displayed in the UI. Disadvantage (Extra).
        /// </summary>
        public string DisplayDisadvantage(string strLanguage)
        {
            string strReturn = Disadvantage;
            if (strLanguage != GlobalSettings.DefaultLanguage)
            {
                string strTemp = string.Empty;
                if (GetNodeXPath(strLanguage)?.TryGetMultiLineStringFieldQuickly("altdisadvantage", ref strTemp) == true)
                    strReturn = strTemp;
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNodeXPath(strLanguage)?.SelectSingleNode("translate")?.Value ?? Name;
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
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNodeXPath(strLanguage)?.SelectSingleNode("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = _objCharacter
                                  .LoadData(
                                      _eMentorType == Improvement.ImprovementType.MentorSpirit
                                          ? "mentors.xml"
                                          : "paragons.xml", strLanguage)
                                  .SelectSingleNode(SourceID == Guid.Empty
                                                        ? "/chummer/mentors/mentor[name = " + Name.CleanXPath()
                                                        + ']'
                                                        : "/chummer/mentors/mentor[id = "
                                                          + SourceIDString.CleanXPath()
                                                          + " or id = " + SourceIDString.ToUpperInvariant()
                                                              .CleanXPath()
                                                          + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public XPathNavigator GetNodeXPath(string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = _objCharacter
                                    .LoadDataXPath(
                                        _eMentorType == Improvement.ImprovementType.MentorSpirit
                                            ? "mentors.xml"
                                            : "paragons.xml", strLanguage)
                                    .SelectSingleNode(SourceID == Guid.Empty
                                                          ? "/chummer/mentors/mentor[name = " + Name.CleanXPath()
                                                          + ']'
                                                          : "/chummer/mentors/mentor[id = "
                                                            + SourceIDString.CleanXPath()
                                                            + " or id = " + SourceIDString.ToUpperInvariant()
                                                                .CleanXPath()
                                                            + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        #endregion Properties

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
