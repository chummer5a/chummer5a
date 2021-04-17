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

    public class SustainedSpell : Spell, IHasInternalId, IHasName, IHasXmlNode, ISustainable
    {
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private bool _blnSelfSustained = true;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;
        private bool _blnLimited;
        private bool _blnExtended;
        private bool _blnAlchemical;

        #region Constructor, Create, Save, Load, and Print Methods

        public SustainedSpell(Character objCharacter) : base(objCharacter)
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
        public override void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
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
        public override void Load(XmlNode objNode)
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
        public override void Save(XmlTextWriter objWriter)
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
        /// Is the spell sustained by yourself?
        /// </summary>
        public bool SelfSustained
        {
            get => _blnSelfSustained;
            set
            {
                if (_blnSelfSustained != value)
                {
                    _blnSelfSustained = value;
                    _objCharacter.OnPropertyChanged("SustainingPenalty");
                }

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
        #endregion

        #region Helper Methods
        #endregion
    }
}
