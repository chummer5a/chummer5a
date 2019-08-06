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

        private readonly CharacterOptions _objOptions;

        #region Constructor, Create, Save, and Load Methods
        public InitiationGrade(Character objCharacter)
        {
            // Create the GUID for the new InitiationGrade.
            _guiID = Guid.NewGuid();
            _objOptions = objCharacter.Options;
        }

        /// Create an Intiation Grade from an XmlNode.
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
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("initiationgrade");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("res", _blnTechnomancer.ToString());
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.Instance.InvariantCultureInfo));
            objWriter.WriteElementString("group", _blnGroup.ToString());
            objWriter.WriteElementString("ordeal", _blnOrdeal.ToString());
            objWriter.WriteElementString("schooling", _blnSchooling.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Initiation Grade from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
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
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("initiationgrade");
            objWriter.WriteElementString("grade", Grade.ToString());
            objWriter.WriteElementString("group", Group.ToString());
            objWriter.WriteElementString("ordeal", Ordeal.ToString());
            objWriter.WriteElementString("schooling", Schooling.ToString());
            objWriter.WriteElementString("technomancer", Technomancer.ToString());
            if (_objOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Initiation Grade in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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
                decimal decCost = _objOptions.KarmaInititationFlat + (Grade * _objOptions.KarmaInitiation);
                decimal decMultiplier = 1.0m;

                // Discount for Group.
                if (Group)
                    decMultiplier -= 0.1m;

                // Discount for Ordeal.
                if (Ordeal)
                    decMultiplier -= Technomancer ? 0.2m : 0.1m;

                // Discount for Schooling.
                if (Schooling)
                    decMultiplier -= 0.1m;

                return decimal.ToInt32(decimal.Ceiling(decCost * decMultiplier));
            }
        }

        /// <summary>
        /// Text to display in the Initiation Grade list.
        /// </summary>
        public string Text(string strLanguage)
        {
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            StringBuilder strReturn = new StringBuilder(LanguageManager.GetString("String_Grade", strLanguage));
            strReturn.Append(strSpaceCharacter);
            strReturn.Append(Grade.ToString());
            if (Group || Ordeal)
            {
                strReturn.Append(strSpaceCharacter + '(');
                if (Group)
                {
                    strReturn.Append(Technomancer ? LanguageManager.GetString("String_Network", strLanguage) : LanguageManager.GetString("String_Group", strLanguage));
                    if (Ordeal || Schooling)
                        strReturn.Append(',' + strSpaceCharacter);
                }
                if (Ordeal)
                {
                    strReturn.Append(Technomancer ? LanguageManager.GetString("String_Task", strLanguage) : LanguageManager.GetString("String_Ordeal", strLanguage));
                    if (Schooling)
                        strReturn.Append(',' + strSpaceCharacter);
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

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsInitiationGrade)
        {
            TreeNode objNode = new TreeNode
            {
                ContextMenuStrip = cmsInitiationGrade,
                Name = InternalId,
                Text = Text(GlobalOptions.Instance.Language),
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };
            return objNode;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((InitiationGrade)obj);
        }

        public int CompareTo(InitiationGrade obj)
        {
            return Grade.CompareTo(obj.Grade);
        }
        #endregion

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            // Stop if this isn't the highest grade
            if (characterObject.MAGEnabled)
            {
                if (Grade != characterObject.InitiateGrade)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_DeleteGrade", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_DeleteGrade", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (blnConfirmDelete)
                {
                    if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteInitiateGrade",
                        GlobalOptions.Instance.Language)))
                        return false;
                }

                characterObject.InitiationGrades.Remove(this);
            }
            else if (characterObject.RESEnabled)
            {
                if (Grade != characterObject.SubmersionGrade)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_DeleteGrade", GlobalOptions.Instance.Language), LanguageManager.GetString("MessageTitle_DeleteGrade", GlobalOptions.Instance.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (blnConfirmDelete)
                {
                    if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteSubmersionGrade",
                        GlobalOptions.Instance.Language)))
                        return false;
                }

                characterObject.InitiationGrades.Remove(this);
            }
            else
                return false;
            return true;
        }
    }
}
