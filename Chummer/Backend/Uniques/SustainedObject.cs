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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Annotations;
using NLog;

namespace Chummer
{
    public class SustainedObject : IHasInternalId, INotifyPropertyChanged
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private readonly Character _objCharacter;
        private bool _blnSelfSustained = true;
        private int _intForce;
        private int _intNetHits;
        private IHasInternalId _objLinkedObject;
        private Improvement.ImprovementSource _eLinkedObjectType;

        #region Constructor, Create, Save, Load, and Print Methods

        public SustainedObject(Character objCharacter)
        {
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Creates a sustained object from a thing that can be sustained (usually a Spell, Complex Form, or Critter Power)
        /// </summary>
        /// <param name="objLinkedObject">The liked object that is meant to be sustained.</param>
        /// <returns></returns>
        public void Create(IHasInternalId objLinkedObject)
        {
            _objLinkedObject = objLinkedObject;
            switch (objLinkedObject)
            {
                case Spell _:
                    _eLinkedObjectType = Improvement.ImprovementSource.Spell;
                    break;

                case ComplexForm _:
                    _eLinkedObjectType = Improvement.ImprovementSource.ComplexForm;
                    break;

                case CritterPower _:
                    _eLinkedObjectType = Improvement.ImprovementSource.CritterPower;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(objLinkedObject));
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("sustainedobject");
            objWriter.WriteElementString("linkedobject", _objLinkedObject.InternalId);
            objWriter.WriteElementString("linkedobjecttype", _eLinkedObjectType.ToString());
            objWriter.WriteElementString("force", _intForce.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("nethits", _intNetHits.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("self", _blnSelfSustained.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Sustained Object from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            string strLinkedId = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("linkedobject", ref strLinkedId))
            {
                _guiID = Guid.Empty;
                return;
            }
            if (objNode["linkedobjecttype"] != null)
            {
                _eLinkedObjectType = Improvement.ConvertToImprovementSource(objNode["linkedobjecttype"].InnerText);
            }
            else
            {
                _guiID = Guid.Empty;
                return;
            }
            IEnumerable<IHasInternalId> lstToSearch;
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    lstToSearch = _objCharacter.Spells;
                    break;

                case Improvement.ImprovementSource.ComplexForm:
                    lstToSearch = _objCharacter.ComplexForms;
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    lstToSearch = _objCharacter.CritterPowers;
                    break;

                default:
                    _guiID = Guid.Empty;
                    return;
            }
            _objLinkedObject = lstToSearch.FirstOrDefault(x => x.InternalId == strLinkedId);
            if (_objLinkedObject == null)
            {
                Utils.BreakIfDebug();
                _guiID = Guid.Empty;
                return;
            }
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
            objNode.TryGetInt32FieldQuickly("nethits", ref _intNetHits);
            objNode.TryGetBoolFieldQuickly("self", ref _blnSelfSustained);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            // <sustainedobject>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("sustainedobject");
            try
            {
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("name_english", Name);
                await objWriter.WriteElementStringAsync("force", Force.ToString(objCulture));
                await objWriter.WriteElementStringAsync("nethits", NetHits.ToString(objCulture));
                await objWriter.WriteElementStringAsync("self", SelfSustained.ToString(objCulture));
            }
            finally
            {
                // </sustainedobject>
                await objBaseElement.DisposeAsync();
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Is the spell sustained by yourself?
        /// </summary>
        public bool SelfSustained
        {
            get => _blnSelfSustained;
            set
            {
                if (_blnSelfSustained == value)
                    return;
                _blnSelfSustained = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Force of the sustained spell
        /// </summary>
        public int Force
        {
            get => _intForce;
            set
            {
                if (_intForce == value)
                    return;
                _intForce = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The Net Hits the Sustained Spell has
        /// </summary>
        public int NetHits
        {
            get => _intNetHits;
            set
            {
                if (_intNetHits == value)
                    return;
                _intNetHits = value;
                OnPropertyChanged();
            }
        }

        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public IHasInternalId LinkedObject => _objLinkedObject;

        public Improvement.ImprovementSource LinkedObjectType => _eLinkedObjectType;

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    return (_objLinkedObject as Spell)?.DisplayNameShort(strLanguage);

                case Improvement.ImprovementSource.ComplexForm:
                    return (_objLinkedObject as ComplexForm)?.DisplayNameShort(strLanguage);

                case Improvement.ImprovementSource.CritterPower:
                    return (_objLinkedObject as CritterPower)?.DisplayNameShort(strLanguage);
            }
            return LanguageManager.GetString("String_Unknown", strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public Task<string> DisplayNameShortAsync(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    if (_objLinkedObject is Spell objSpell)
                        return objSpell.DisplayNameShortAsync(strLanguage).AsTask();
                    break;
                case Improvement.ImprovementSource.ComplexForm:
                    if (_objLinkedObject is ComplexForm objComplexForm)
                        return objComplexForm.DisplayNameShortAsync(strLanguage).AsTask();
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    if (_objLinkedObject is CritterPower objCritterPower)
                        return objCritterPower.DisplayNameShortAsync(strLanguage).AsTask();
                    break;
            }
            return LanguageManager.GetStringAsync("String_Unknown", strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    return (_objLinkedObject as Spell)?.DisplayName(strLanguage);

                case Improvement.ImprovementSource.ComplexForm:
                    return (_objLinkedObject as ComplexForm)?.DisplayName(strLanguage);

                case Improvement.ImprovementSource.CritterPower:
                    return (_objLinkedObject as CritterPower)?.DisplayName(strLanguage);
            }
            return LanguageManager.GetString("String_Unknown", strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public Task<string> DisplayNameAsync(string strLanguage)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_eLinkedObjectType)
            {
                case Improvement.ImprovementSource.Spell:
                    if (_objLinkedObject is Spell objSpell)
                        return objSpell.DisplayNameAsync(strLanguage).AsTask();
                    break;
                case Improvement.ImprovementSource.ComplexForm:
                    if (_objLinkedObject is ComplexForm objComplexForm)
                        return objComplexForm.DisplayNameAsync(strLanguage).AsTask();
                    break;

                case Improvement.ImprovementSource.CritterPower:
                    if (_objLinkedObject is CritterPower objCritterPower)
                        return objCritterPower.DisplayNameAsync(strLanguage).AsTask();
                    break;
            }
            return LanguageManager.GetStringAsync("String_Unknown", strLanguage);
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string Name
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (_eLinkedObjectType)
                {
                    case Improvement.ImprovementSource.Spell:
                        return (_objLinkedObject as Spell)?.Name;

                    case Improvement.ImprovementSource.ComplexForm:
                        return (_objLinkedObject as ComplexForm)?.Name;

                    case Improvement.ImprovementSource.CritterPower:
                        return (_objLinkedObject as CritterPower)?.Name;
                }
                return LanguageManager.GetString("String_Unknown", GlobalSettings.DefaultLanguage);
            }
        }

        public bool HasSustainingPenalty => SelfSustained && LinkedObjectType != Improvement.ImprovementSource.CritterPower;

        #endregion Properties

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
            if (strPropertyName == nameof(SelfSustained) || strPropertyName == nameof(LinkedObjectType))
                _objCharacter.RefreshSustainingPenalties();
        }
    }
}
