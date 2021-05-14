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

using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Sustained Complex Form
    /// </summary>
    
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class SustainedComplexForm : ComplexForm, ISustainable, INotifyPropertyChanged
    {
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private bool _blnSelfSustained = true;
        private int _intForce = 0;
        private int _intNetHits = 0;
        private readonly Character _objCharacter;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Constructor, Create, Save, Load, and Print Methods
        public SustainedComplexForm(Character objCharacter) : base(objCharacter)
        {
            //Create the GUID for new sustained Complex Forms
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            PropertyChanged += OnSustainedChanged;
        }

        /// <summary>
        /// Maps all the writable properties of an reference ComplexForm unto a SustainedComplexForm
        /// </summary>
        /// <param name="formRef">The reference ComplexForm</param>
        /// <param name="susFormTarget">The SustainedComplexForm that is copied upon</param>
        /// <returns></returns>
        public SustainedComplexForm CreateByMapping(ComplexForm formRef, SustainedComplexForm susFormTarget)
        {
            Type t = typeof(ComplexForm);
            PropertyInfo[] objPropertyInfo = t.GetProperties();

            foreach (var propInfo in objPropertyInfo)
            {
                if (propInfo.CanWrite)
                {
                    var value = propInfo.GetValue(formRef);
                    propInfo.SetValue(susFormTarget, value, null);
                }
            }
            _guiSourceID = formRef.SourceID;
            return susFormTarget;
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

        public bool SelfSustained
        {
            get => _blnSelfSustained;
            set
            {
                if (_blnSelfSustained != value)
                {
                    _blnSelfSustained = value;
                    OnPropertyChanged();
                }

            }

        }
        public new string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);
        #endregion

        #region Property Changed
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnSustainedChanged(object sender, PropertyChangedEventArgs e)
        {
            _objCharacter.RefreshSustainingPenalties();
        }
        #endregion
    }
}
