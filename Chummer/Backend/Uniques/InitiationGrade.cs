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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// An Initiation Grade.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Grade) + "}")]
    public class InitiationGrade : IHasInternalId, IComparable, ICanRemove
    {
        private Guid _guiID;
        private bool _blnGroup;
        private bool _blnOrdeal;
        private bool _blnSchooling;
        private bool _blnTechnomancer;
        private int _intGrade;
        private string _strNotes = string.Empty;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, and Load Methods
        public InitiationGrade(Character objCharacter)
        {
            // Create the GUID for the new InitiationGrade.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Initiation Grade from an XmlNode.
        /// <param name="intGrade">Grade number.</param>
        /// <param name="blnTechnomancer">Whether or not the character is a Technomancer.</param>
        /// <param name="blnGroup">Whether or not a Group was used.</param>
        /// <param name="blnOrdeal">Whether or not an Ordeal was used.</param>
        /// <param name="blnSchooling">Whether or not Schooling was used.</param>
        public void Create(int intGrade, bool blnTechnomancer, bool blnGroup, bool blnOrdeal, bool blnSchooling)
        {
            _intGrade = intGrade;
            _blnTechnomancer = blnTechnomancer;
            _blnGroup = blnGroup;
            _blnOrdeal = blnOrdeal;
            _blnSchooling = blnSchooling;
            //TODO: I'm not happy with this.
            //KC 90: a Cyberadept who has Submerged may restore Resonance that has been lost to cyberware (and only cyberware) by an amount equal to half their Submersion Grade(rounded up).
            //To handle this, we ceiling the CyberwareEssence value up, as a non-zero loss of Essence removes a point of Resonance, and cut the submersion grade in half.
            //Whichever value is lower becomes the value of the improvement.
            if (intGrade > 0 && blnTechnomancer && _objCharacter.TechnomancerEnabled && !_objCharacter.Options.SpecialKarmaCostBasedOnShownValue
                && _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.CyberadeptDaemon && x.Enabled))
            {
                decimal decNonCyberwareEssence = _objCharacter.BiowareEssence + _objCharacter.EssenceHole;
                int intResonanceRecovered = (int)Math.Min(Math.Ceiling(0.5m * intGrade),
                     Math.Ceiling(decNonCyberwareEssence) == Math.Floor(decNonCyberwareEssence)
                        ? Math.Ceiling(_objCharacter.CyberwareEssence)
                        : Math.Floor(_objCharacter.CyberwareEssence));
                // Cannot increase RES to be more than what it would be without any Essence loss.
                intResonanceRecovered = _objCharacter.Options.ESSLossReducesMaximumOnly
                    ? Math.Min(intResonanceRecovered, _objCharacter.RES.MaximumNoEssenceLoss() - _objCharacter.RES.TotalMaximum)
                    // +1 compared to normal because this Grade's effect has not been processed yet.
                    : Math.Min(intResonanceRecovered, _objCharacter.RES.MaximumNoEssenceLoss() + 1 - _objCharacter.RES.Value);
                ImprovementManager.CreateImprovement(_objCharacter, "RESBase", Improvement.ImprovementSource.CyberadeptDaemon,
                    _guiID.ToString("D", GlobalOptions.InvariantCultureInfo),
                    Improvement.ImprovementType.Attribute, string.Empty, intResonanceRecovered, 1, 0, 0, intResonanceRecovered);
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
            objWriter.WriteStartElement("initiationgrade");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("res", _blnTechnomancer.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("group", _blnGroup.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("ordeal", _blnOrdeal.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("schooling", _blnSchooling.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Initiation Grade from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetBoolFieldQuickly("res", ref _blnTechnomancer);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetBoolFieldQuickly("group", ref _blnGroup);
            objNode.TryGetBoolFieldQuickly("ordeal", ref _blnOrdeal);
            objNode.TryGetBoolFieldQuickly("schooling", ref _blnSchooling);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("initiationgrade");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("grade", Grade.ToString(objCulture));
            objWriter.WriteElementString("group", Group.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("ordeal", Ordeal.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("schooling", Schooling.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("technomancer", Technomancer.ToString(GlobalOptions.InvariantCultureInfo));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Initiation Grade in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Initiate Grade.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Whether or not a Group was used.
        /// </summary>
        public bool Group
        {
            get => _blnGroup;
            set => _blnGroup = value;
        }

        /// <summary>
        /// Whether or not an Ordeal was used.
        /// </summary>
        public bool Ordeal
        {
            get => _blnOrdeal;
            set => _blnOrdeal = value;
        }

        /// <summary>
        /// Whether or not Schooling was used.
        /// </summary>
        public bool Schooling
        {
            get => _blnSchooling;
            set => _blnSchooling = value;
        }

        /// <summary>
        /// Whether or not the Initiation Grade is for a Technomancer.
        /// </summary>
        public bool Technomancer
        {
            get => _blnTechnomancer;
            set => _blnTechnomancer = value;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// The Initiation Grade's Karma cost.
        /// </summary>
        public int KarmaCost
        {
            get
            {
                CharacterOptions objOptions = _objCharacter.Options;
                decimal decCost = objOptions.KarmaInitiationFlat + (Grade * objOptions.KarmaInitiation);
                decimal decMultiplier = 1.0m;

                // Discount for Group.
                if (Group)
                    decMultiplier -= Technomancer
                        ? objOptions.KarmaRESInitiationGroupPercent
                        : objOptions.KarmaMAGInitiationGroupPercent;

                // Discount for Ordeal.
                if (Ordeal)
                    decMultiplier -= Technomancer
                        ? objOptions.KarmaRESInitiationOrdealPercent
                        : objOptions.KarmaMAGInitiationOrdealPercent;

                // Discount for Schooling.
                if (Schooling)
                    decMultiplier -= Technomancer
                        ? objOptions.KarmaRESInitiationSchoolingPercent
                        : objOptions.KarmaMAGInitiationSchoolingPercent;

                return (decCost * decMultiplier).StandardRound();
            }
        }

        /// <summary>
        /// Text to display in the Initiation Grade list.
        /// </summary>
        public string Text(string strLanguage)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            StringBuilder strReturn = new StringBuilder(LanguageManager.GetString("String_Grade", strLanguage));
            strReturn.Append(strSpace);
            strReturn.Append(Grade.ToString(GlobalOptions.CultureInfo));
            if (Group || Ordeal)
            {
                strReturn.Append(strSpace + '(');
                if (Group)
                {
                    strReturn.Append(Technomancer ? LanguageManager.GetString("String_Network", strLanguage) : LanguageManager.GetString("String_Group", strLanguage));
                    if (Ordeal || Schooling)
                        strReturn.Append(',' + strSpace);
                }
                if (Ordeal)
                {
                    strReturn.Append(Technomancer ? LanguageManager.GetString("String_Task", strLanguage) : LanguageManager.GetString("String_Ordeal", strLanguage));
                    if (Schooling)
                        strReturn.Append(',' + strSpace);
                }
                if (Schooling)
                {
                    strReturn.Append(LanguageManager.GetString("String_Schooling", strLanguage));
                }
                strReturn.Append(')');
            }

            return strReturn.ToString();
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.HasNotesColor
                : ColorManager.WindowText;
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsInitiationGrade)
        {
            TreeNode objNode = new TreeNode
            {
                ContextMenuStrip = cmsInitiationGrade,
                Name = InternalId,
                Text = Text(GlobalOptions.Language),
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };
            return objNode;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((InitiationGrade)obj);
        }

        public int CompareTo(InitiationGrade objGrade)
        {
            return objGrade == null ? 1 : Grade.CompareTo(objGrade.Grade);
        }
        #endregion

        public bool Remove(bool blnConfirmDelete = true)
        {
            // Stop if this isn't the highest grade
            if (_objCharacter.MAGEnabled)
            {
                if (Grade != _objCharacter.InitiateGrade)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_DeleteGrade"), LanguageManager.GetString("MessageTitle_DeleteGrade"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (blnConfirmDelete)
                {
                    if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteInitiateGrade")))
                        return false;
                }
            }
            else if (_objCharacter.RESEnabled)
            {
                if (Grade != _objCharacter.SubmersionGrade)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_DeleteGrade"), LanguageManager.GetString("MessageTitle_DeleteGrade"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (blnConfirmDelete)
                {
                    if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_DeleteSubmersionGrade")))
                        return false;
                }

                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.CyberadeptDaemon, _guiID.ToString("D", GlobalOptions.InvariantCultureInfo));
            }
            else
                return false;

            _objCharacter.InitiationGrades.Remove(this);
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is InitiationGrade objGrade)
            {
                return Grade.Equals(objGrade.Grade);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return new {InternalId, Grade, Group, Ordeal, Schooling, Technomancer, Notes}.GetHashCode();
        }

        public static bool operator ==(InitiationGrade left, InitiationGrade right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(InitiationGrade left, InitiationGrade right)
        {
            return !(left == right);
        }

        public static bool operator <(InitiationGrade left, InitiationGrade right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(InitiationGrade left, InitiationGrade right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(InitiationGrade left, InitiationGrade right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(InitiationGrade left, InitiationGrade right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
