using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private bool _blnSelfSustained = true;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;

        public SustainedCritterPower(Character objCharacter) : base(objCharacter)
        {
            //Create the GUID for new sustained Complex Forms
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Maps all the writable properties of an reference CritterPower unto a SustainedCritterPower
        /// </summary>
        /// <param name="critterPowerRef">The CritterPower to use as an reference</param>
        /// <param name="susCritterPowerTarget">The SustainedCritterPower that is copied upon</param>
        /// <returns></returns>
        public SustainedCritterPower CreateByMapping(CritterPower critterPowerRef, SustainedCritterPower susCritterPowerTarget)
        {
            Type t = typeof(CritterPower);
            PropertyInfo[] objPropertyInfo = t.GetProperties();

            foreach (var propInfo in objPropertyInfo)
            {
                if (propInfo.CanWrite)
                {
                    var value = propInfo.GetValue(critterPowerRef);
                    propInfo.SetValue(susCritterPowerTarget, value, null);
                }
            }
            _guiSourceID = critterPowerRef.SourceID;
            return susCritterPowerTarget;
        }

        /// <summary>
        ///  Saves all additional information needed for sustained derived objects xml to the XmlWriter.
        /// </summary>
        /// <param name="objWriter"></param>
        public override void SaveDerived(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("force", _intForce.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Loads all the additional information of an Sustained Object from an given XMLNode
        /// </summary>
        /// <param name="objBaseNode"></param>
        public override void LoadDerived(XmlNode objBaseNode)
        {
            XmlNode objNode = objBaseNode.SelectSingleNode("sustainedobject");
            if (objNode == null)
                return;

            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }

            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
            objNode.TryGetBoolFieldQuickly("self", ref _blnSelfSustained);
        }


        /// <summary>
        /// Prints all additional information needed for sustained derived objects xml to the XmlWriter.
        /// </summary>
        /// <param name="objWriter"></param>
        public override void PrintDerived(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;

            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("guid", InternalId);
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
        public new string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        #endregion

    }
}
