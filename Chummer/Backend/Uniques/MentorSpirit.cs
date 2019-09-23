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
using NLog;

namespace Chummer
{
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameShort(GlobalOptions.DefaultLanguage)}")]
    public class MentorSpirit : IHasInternalId, IHasName, IHasXmlNode, IHasSource
    {

        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
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
            if(namenode != null)
                Name = namenode.InnerText;
            XmlNode typenode = xmlNodeMentor?.SelectSingleNode("mentortype");
            if (typenode != null)
            {
                if(Enum.TryParse(typenode.InnerText, true, out Improvement.ImprovementType outEnum))
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
            xmlMentor.TryGetStringFieldQuickly("name", ref _strName);
            xmlMentor.TryGetStringFieldQuickly("source", ref _strSource);
            xmlMentor.TryGetStringFieldQuickly("page", ref _strPage);
            if (!xmlMentor.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                xmlMentor.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (!xmlMentor.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", xmlMentor });
                Utils.BreakIfDebug();
            }

            // Build the list of advantages gained through the Mentor Spirit.
            if (!xmlMentor.TryGetStringFieldQuickly("altadvantage", ref _strAdvantage))
            {
                xmlMentor.TryGetStringFieldQuickly("advantage", ref _strAdvantage);
            }
            if (!xmlMentor.TryGetStringFieldQuickly("altdisadvantage", ref _strDisadvantage))
            {
                xmlMentor.TryGetStringFieldQuickly("disadvantage", ref _strDisadvantage);
            }

            _nodBonus = xmlMentor["bonus"];
            if (_nodBonus != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForceValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
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
            _nodChoice1 = xmlMentor.SelectSingleNode("choices/choice[name = \"" + strForceValueChoice1 + "\"]/bonus");
            if (_nodChoice1 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                //ImprovementManager.ForcedValue = strForceValueChoice1;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodChoice1, false, 1, DisplayNameShort(GlobalOptions.Language)))
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
            _nodChoice2 = xmlMentor.SelectSingleNode("choices/choice[name = \"" + strForceValueChoice2 + "\"]/bonus");
            if (_nodChoice2 != null)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                //ImprovementManager.ForcedValue = strForceValueChoice2;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.MentorSpirit, _guiID.ToString("D"), _nodChoice2, false, 1, DisplayNameShort(GlobalOptions.Language)))
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
            objWriter.WriteElementString("mentormask", _blnMentorMask.ToString());
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
            objWriter.WriteElementString("notes", _strNotes);

            if (!string.IsNullOrEmpty(strSourceID))
            {
                objWriter.WriteElementString("id", strSourceID);
            }

            objWriter.WriteEndElement();

            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Mentor Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                if (node?.TryGetGuidFieldQuickly("id", ref _guiSourceID) == false)
                {
                    XmlNode objNewNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/mentors/mentor[name = \"" + Name + "\"]");
                    objNewNode?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (objNode["mentortype"] != null)
            {
                _eMentorType = Improvement.ConvertToImprovementType(objNode["mentortype"].InnerText);
                _objCachedMyXmlNode = null;
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("advantage", ref _strAdvantage);
            objNode.TryGetStringFieldQuickly("disadvantage", ref _strDisadvantage);
            objNode.TryGetBoolFieldQuickly("mentormask", ref _blnMentorMask);
            _nodBonus = objNode["bonus"];
            _nodChoice1 = objNode["choice1"];
            _nodChoice2 = objNode["choice2"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("mentorspirit");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("mentortype", _eMentorType.ToString());
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("advantage", Advantage);
            objWriter.WriteElementString("disadvantage", Disadvantage);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("mentormask", MentorMask.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties


        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D");

        /// <summary>
        /// Name of the Mentor Spirit or Paragon.
        /// </summary>
        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_strName))
                {
                    if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                        _strName = _objCharacter.MentorSpirits[0].Name;
                }
                return _strName;
            }
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
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
        /// Advantage of the Mentor Spirit or Paragon.
        /// </summary>
        public string Advantage
        {
            get => _strAdvantage;
            set
            {
                if (_strAdvantage != value)
                {
                    _strAdvantage = value;
                    if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                }
            }
        }

        /// <summary>
        /// Advantage of the mentor as it should be displayed in the UI. Advantage (Extra).
        /// </summary>
        public string DisplayAdvantage(string strLanguage)
        {
            string strReturn = Advantage;

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// Disadvantage of the Mentor Spirit or Paragon.
        /// </summary>
        public string Disadvantage
        {
            get => _strDisadvantage;
            set
            {
                if (_strDisadvantage != value)
                {
                    _strDisadvantage = value;
                    if (_objCharacter.MentorSpirits.Count > 0 && _objCharacter.MentorSpirits[0] == this)
                        _objCharacter.OnPropertyChanged(nameof(Character.FirstMentorSpiritDisplayInformation));
                }
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Guid of the Xml Node containing data on this Mentor Spirit or Paragon.
        /// </summary>
        public string strSourceID => _guiSourceID.Equals(Guid.Empty) ? string.Empty : _guiSourceID.ToString("D");

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
                var xdoc = XmlManager.Load(
                    _eMentorType == Improvement.ImprovementType.MentorSpirit ? "mentors.xml" : "paragons.xml",
                    strLanguage);
                _objCachedMyXmlNode = xdoc.SelectSingleNode(SourceID == Guid.Empty
                        ? $"/chummer/mentors/mentor[name = \"{Name}\"]"
                        : $"/chummer/mentors/mentor[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public string InternalId => _guiID.ToString("D");

        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
