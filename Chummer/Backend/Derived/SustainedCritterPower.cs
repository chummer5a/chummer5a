using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Sustained Critterpower
    /// </summary>

    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    #region Constructor, Create, Save, Load, and Print Methods

    public class SustainedCritterPower :CritterPower, ISustainable
    {
        private bool _blnSelfSustained;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;

        public SustainedCritterPower(Character objCharacter) : base(objCharacter)
        {
            //Create the GUID for new sustained Complex Forms
            guiID = Guid.NewGuid();
        }

        public void Create(CritterPower refCritterPower)
        {
            guiSourceID = refCritterPower.SourceID;
            Name = refCritterPower.Name;
        }

        public override void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("type", nameof(SustainedCritterPower));
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", strName);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Loads an SustainedCritterPower from an given XML Node
        /// </summary>
        /// <param name="objNode"></param>
        public override void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out guiID))
            {
                guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref guiSourceID);
            }
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
        }


        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            //Do not change the Start Element name!
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("type", nameof(SustainedCritterPower));
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));

            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        public int NetHits
        {
            get => _intNetHits;
            set => _intNetHits = value;
        }

        public int Force
        {
            get => _intForce;
            set => _intForce = value;
        }


        /// <summary>
        /// Is the Critter power self sustained, is never true to prevent misscalculation!
        /// </summary>
        public bool SelfSustained
        {
            get => _blnSelfSustained;
            //Get be read only to satisfy the Interface implementation, but this should prevent any ways to set this to true.
            // ReSharper disable once ValueParameterNotUsed
            set => _blnSelfSustained = false;
        }
        #endregion

    }
}
