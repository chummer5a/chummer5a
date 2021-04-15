using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Sustained Magician Spell
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]

    public class SustainedSpell : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strExtra = string.Empty;
        private bool _blnSelfSustained = true;
        private bool _blnLimited;
        private bool _blnExtended;
        private bool _blnAlchemical;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public SustainedSpell(Character objCharacter)
        {
            //Create the GUID for new sustained spells
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        public void Create(Spell spellRef)
        {
            _strName = spellRef.Name;
            _blnLimited = spellRef.Limited;
            _blnExtended = spellRef.Extended;
            _blnAlchemical = spellRef.Alchemical;
            _guiSourceID = spellRef.SourceID;
        }



        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter"></param>
        /// <param name="objCulture"></param>
        /// <param name="strLanguageToPrint"></param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedspell");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            if (Limited)
                objWriter.WriteElementString("name", string.Format(objCulture, "{0}{1}({2})",
                    DisplayNameShort(strLanguageToPrint), LanguageManager.GetString("String_Space", strLanguageToPrint), LanguageManager.GetString("String_SpellLimited", strLanguageToPrint)));
            else if (Alchemical)
                objWriter.WriteElementString("name", string.Format(objCulture, "{0}{1}({2})",
                    DisplayNameShort(strLanguageToPrint), LanguageManager.GetString("String_Space", strLanguageToPrint), LanguageManager.GetString("String_SpellAlchemical", strLanguageToPrint)));
            else
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Spell from the XmlNode.
        /// </summary>
        /// <param name="objNode"></param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
            objNode.TryGetBoolFieldQuickly("self", ref _blnSelfSustained);
        }

        /// <summary>
        /// Save the objects xml to the XmlWriter, used for Sustained spells only!
        /// </summary>
        /// <param name="objWriter"></param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedspell");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
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
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Spell's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                }
            }
        }

        /// <summary>
        /// Extra information from Improvement
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Is the spell sustained by yourself?
        /// </summary>
        public bool SelfSustained
        {
            get => _blnSelfSustained;
            set
            {
                _blnSelfSustained = value;
                _objCharacter.OnPropertyChanged("SustainingPenalty");
            }
            
        }

        /// <summary>
        /// Force of the sustained spell
        /// </summary>
        public int Force
        {
            get => _intForce;
            set => _intForce = value;
        }

        /// <summary>
        /// The Net Hits the Sustained Spell has
        /// </summary>
        public int NetHits
        {
            get => _intNetHits;
            set => _intNetHits = value;
        }

        /// <summary>
        /// Whether or not the spell is limited
        /// </summary>
        public bool Limited
        {
            get => _blnLimited;
            set => _blnLimited = value;
        }

        /// <summary>
        /// Whether or not the spell is self sustained
        /// </summary>
        public bool Extended
        {
            get => _blnExtended;
            set => _blnLimited = value;
        }

        /// <summary>
        /// Wether or not the Sustained Spell is Alchemical
        /// </summary>
        public bool Alchemical
        {
            get => _blnAlchemical;
            set => _blnAlchemical = value;
        }
        #endregion

        #region Helper Methods

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
                _objCachedMyXmlNode = _objCharacter.LoadData("spells.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/spells/spell[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/spells/spell[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = strLanguage != GlobalOptions.DefaultLanguage ? GetNode(strLanguage)?["translate"]?.InnerText ?? Name : Name;
            if (Extended && !Name.EndsWith("Extended", StringComparison.Ordinal))
                strReturn += ',' + LanguageManager.GetString("String_Space", strLanguage) + LanguageManager.GetString("String_SpellExtended", strLanguage);

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (Limited)
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.GetString("String_SpellLimited", strLanguage) + ')';
            if (Alchemical)
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.GetString("String_SpellAlchemical", strLanguage) + ')';
            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            }
            return strReturn;
        }

        #endregion
    }
}
