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
 using System.Xml;
 using System.Globalization;

namespace Chummer
{
    public enum ExpenseType
    {
        Karma = 0,
        Nuyen = 1,
    }

    public enum KarmaExpenseType
    {
        ImproveAttribute = 0,
        AddQuality,
        ImproveSkillGroup,
        AddSkill,
        ImproveSkill,
        SkillSpec,
        AddMartialArt,
        AddSpell,
        AddComplexForm,
        AddMetamagic,
        ImproveInitiateGrade,
        RemoveQuality,
        ManualAdd,
        ManualSubtract,
        BindFocus,
        JoinGroup,
        LeaveGroup,
        QuickeningMetamagic,
        AddPowerPoint,
        AddSpecialization,
        AddAIProgram,
        AddAIAdvancedProgram,
        AddCritterPower,
        SpiritFettering,
        AddMartialArtTechnique
    }

    public enum NuyenExpenseType
    {
        AddCyberware = 0,
        IncreaseLifestyle,
        AddArmor,
        AddArmorMod,
        AddWeapon,
        AddWeaponMod,
        AddWeaponAccessory,
        AddGear,
        AddVehicle,
        AddVehicleMod,
        AddVehicleGear,
        AddVehicleWeapon,
        AddVehicleWeaponMod,
        AddVehicleWeaponAccessory,
        AddVehicleWeaponMount,
        ManualAdd,
        ManualSubtract,
        AddArmorGear,
        AddVehicleModCyberware,
        AddCyberwareGear,
        AddWeaponGear,
        ImproveInitiateGrade,
        AddVehicleWeaponMountMod,
    }

    /// <summary>
    /// Undo information for an Expense Log Entry.
    /// </summary>
    [DebuggerDisplay("{ObjectId}: {Qty.ToString()}, {Extra}")]
    public class ExpenseUndo
    {
        private string _strObjectId;
        private decimal _decQty;
        private string _strExtra = string.Empty;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a KarmaExpenseType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static KarmaExpenseType ConvertToKarmaExpenseType(string strValue)
        {
            return Enum.TryParse(strValue, out KarmaExpenseType result) ? result : KarmaExpenseType.ManualAdd;
        }

        /// <summary>
        /// Convert a string to a NuyenExpenseType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static NuyenExpenseType ConvertToNuyenExpenseType(string strValue)
        {
            return Enum.TryParse(strValue, out NuyenExpenseType result) ? result : NuyenExpenseType.ManualAdd;
        }
        #endregion

        #region Constructor, Create, Save, and Load Methods
        /// <summary>
        /// Create the ExpenseUndo Entry.
        /// </summary>
        /// <param name="objExpenseType">Karma expense type.</param>
        /// <param name="strObjectId">Object identifier.</param>
        public ExpenseUndo CreateKarma(KarmaExpenseType objExpenseType, string strObjectId)
        {
            KarmaType = objExpenseType;
            _strObjectId = strObjectId;

            return this;
        }

        /// <summary>
        /// Create the ExpenseUndo Entry.
        /// </summary>
        /// <param name="objExpenseType">Nuyen expense type.</param>
        /// <param name="strObjectId">Object identifier.</param>
        /// <param name="decQty">Amount of Nuyen.</param>
        public ExpenseUndo CreateNuyen(NuyenExpenseType objExpenseType, string strObjectId, decimal decQty = 0)
        {
            NuyenType = objExpenseType;
            _strObjectId = strObjectId;
            _decQty = decQty;

            return this;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("undo");
            objWriter.WriteElementString("karmatype", KarmaType.ToString());
            objWriter.WriteElementString("nuyentype", NuyenType.ToString());
            objWriter.WriteElementString("objectid", _strObjectId);
            objWriter.WriteElementString("qty", _decQty.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the KarmaLogEntry from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (objNode["karmatype"] != null)
                KarmaType = ConvertToKarmaExpenseType(objNode["karmatype"].InnerText);
            if (objNode["nuyentype"] != null)
                NuyenType = ConvertToNuyenExpenseType(objNode["nuyentype"].InnerText);
            objNode.TryGetStringFieldQuickly("objectid", ref _strObjectId);
            objNode.TryGetDecFieldQuickly("qty", ref _decQty);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Karma Expense Type.
        /// </summary>
        public KarmaExpenseType KarmaType { get; set; }

        /// <summary>
        /// Nuyen Expense Type.
        /// </summary>
        public NuyenExpenseType NuyenType { get; set; }

        /// <summary>
        /// Object InternalId.
        /// </summary>
        public string ObjectId
        {
            get => _strObjectId;
            set => _strObjectId = value;
        }

        /// <summary>
        /// Quantity of items added (Nuyen only).
        /// </summary>
        public decimal Qty
        {
            get => _decQty;
            set => _decQty = value;
        }

        /// <summary>
        /// Extra information.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }
        #endregion
    }

    /// <summary>
    /// Expense Log Entry.
    /// </summary>
    [DebuggerDisplay("{Date.ToString()}: {Amount.ToString()}")]
    public class ExpenseLogEntry : IHasInternalId, IComparable, IEquatable<ExpenseLogEntry>
    {
        private Guid _guiID;
        private readonly Character _objCharacter;
        private DateTime _datDate;
        private decimal _decAmount;
        private string _strReason = string.Empty;
        private ExpenseType _objExpenseType;
        private bool _blnRefund;
        private bool _blnForceCareerVisible;

        #region Helper Methods
        public int CompareTo(object obj)
        {
            if (obj is ExpenseLogEntry objEntry)
            {
                if (Equals(_objCharacter, objEntry._objCharacter))
                {
                    int intReturn = Date.CompareTo(objEntry.Date);
                    if (intReturn == 0)
                        intReturn = _objExpenseType.CompareTo(objEntry._objExpenseType);
                    if (intReturn == 0)
                        intReturn = string.Compare(Reason, objEntry.Reason, StringComparison.Ordinal);
                    if (intReturn == 0)
                        intReturn = Refund.CompareTo(objEntry.Refund);
                    if (intReturn == 0)
                        intReturn = Amount.CompareTo(objEntry.Amount);
                    if (intReturn == 0)
                        intReturn = ForceCareerVisible.CompareTo(objEntry.ForceCareerVisible);
                    return -intReturn;
                }

                int intBackupReturn = string.Compare(_objCharacter?.FileName ?? string.Empty, objEntry._objCharacter?.FileName ?? string.Empty, StringComparison.Ordinal);
                if (intBackupReturn == 0)
                    intBackupReturn = string.Compare(_objCharacter?.CharacterName ?? string.Empty, objEntry._objCharacter?.CharacterName ?? string.Empty, StringComparison.Ordinal);
                if (intBackupReturn == 0)
                    intBackupReturn = Date.CompareTo(objEntry.Date);
                if (intBackupReturn == 0)
                    intBackupReturn = _objExpenseType.CompareTo(objEntry._objExpenseType);
                if (intBackupReturn == 0)
                    intBackupReturn = string.Compare(Reason, objEntry.Reason, StringComparison.Ordinal);
                if (intBackupReturn == 0)
                    intBackupReturn = Refund.CompareTo(objEntry.Refund);
                if (intBackupReturn == 0)
                    intBackupReturn = Amount.CompareTo(objEntry.Amount);
                if (intBackupReturn == 0)
                    intBackupReturn = ForceCareerVisible.CompareTo(objEntry.ForceCareerVisible);
                return -intBackupReturn;
            }
            return 1;
        }

        /// <summary>
        /// Convert a string to an ExpenseType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ExpenseType ConvertToExpenseType(string strValue)
        {
            switch (strValue)
            {
                case "Nuyen":
                    return ExpenseType.Nuyen;
                default:
                    return ExpenseType.Karma;
            }
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public ExpenseLogEntry(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _guiID = Guid.NewGuid();
        }

        /// <summary>
        /// Create a new Expense Log Entry.
        /// </summary>
        /// <param name="decAmount">Amount of the Karma/Nuyen expense.</param>
        /// <param name="strReason">Reason for the Karma/Nuyen change.</param>
        /// <param name="objExpenseType">Type of expense, either Karma or Nuyen.</param>
        /// <param name="datDate">Date and time of the Expense.</param>
        /// <param name="blnRefund">Whether or not this expense is a Karma refund.</param>
        public ExpenseLogEntry Create(decimal decAmount, string strReason, ExpenseType objExpenseType, DateTime datDate, bool blnRefund = false)
        {
            _decAmount = decAmount;
            _strReason = strReason;
            _datDate = datDate;
            _objExpenseType = objExpenseType;
            _blnRefund = blnRefund;

            return this;  //Allow chaining
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("expense");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("date", _datDate.ToString("s", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("amount", _decAmount.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("reason", _strReason);
            objWriter.WriteElementString("type", _objExpenseType.ToString());
            objWriter.WriteElementString("refund", _blnRefund.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("forcecareervisible", _blnForceCareerVisible.ToString(GlobalOptions.InvariantCultureInfo));
            Undo?.Save(objWriter);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the KarmaLogEntry from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            DateTime.TryParse(objNode["date"]?.InnerText, GlobalOptions.InvariantCultureInfo, DateTimeStyles.None, out _datDate);
            objNode.TryGetDecFieldQuickly("amount", ref _decAmount);
            if (objNode.TryGetStringFieldQuickly("reason", ref _strReason))
                _strReason = _strReason.TrimEndOnce(" (" + LanguageManager.GetString("String_Expense_Refund") + ')').Replace("🡒", "->");
            if (objNode["type"] != null)
                _objExpenseType = ConvertToExpenseType(objNode["type"].InnerText);
            objNode.TryGetBoolFieldQuickly("refund", ref _blnRefund);
            objNode.TryGetBoolFieldQuickly("forcecareervisible", ref _blnForceCareerVisible);

            if (objNode["undo"] != null)
            {
                Undo = new ExpenseUndo();
                Undo.Load(objNode["undo"]);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            if (Amount != 0 || GlobalOptions.PrintFreeExpenses)
            {
                objWriter.WriteStartElement("expense");
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("date", Date.ToString(objCulture));
                objWriter.WriteElementString("amount", Amount.ToString(Type == ExpenseType.Nuyen ? _objCharacter.Options.NuyenFormat : "#,0.##", objCulture));
                objWriter.WriteElementString("reason", DisplayReason(strLanguageToPrint));
                objWriter.WriteElementString("type", Type.ToString());
                objWriter.WriteElementString("refund", Refund.ToString(GlobalOptions.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Expense Log Entry.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Date the Expense Log Entry was made.
        /// </summary>
        public DateTime Date
        {
            get => _datDate;
            set => _datDate = value;
        }

        /// <summary>
        /// Karma/Nuyen amount gained or spent.
        /// </summary>
        public decimal Amount
        {
            get => _decAmount;
            set
            {
                if (_decAmount != value)
                {
                    _decAmount = value;
                    if (!Refund)
                        _objCharacter?.OnPropertyChanged(Type == ExpenseType.Nuyen ? nameof(Character.CareerNuyen) : nameof(Character.CareerKarma));
                }
            }
        }

        /// <summary>
        /// The Reason for the Entry expense.
        /// </summary>
        public string Reason
        {
            get => _strReason;
            set => _strReason = value;
        }

        /// <summary>
        /// The Reason for the Entry expense.
        /// </summary>
        public string DisplayReason(string strLanguage)
        {
            if (Refund)
                return Reason + LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.GetString("String_Expense_Refund", strLanguage) + ')';
            return Reason;
        }

        /// <summary>
        /// The Expense type.
        /// </summary>
        public ExpenseType Type
        {
            get => _objExpenseType;
            set
            {
                if (_objExpenseType != value)
                {
                    _objExpenseType = value;
                    if (Amount > 0 && !Refund)
                        _objCharacter?.OnMultiplePropertyChanged(nameof(Character.CareerNuyen), nameof(Character.CareerKarma));
                }
            }
        }

        /// <summary>
        /// Whether or not the Expense is a Karma refund.
        /// </summary>
        public bool Refund
        {
            get => _blnRefund;
            set
            {
                if (_blnRefund != value)
                {
                    _blnRefund = value;
                    if (Amount > 0)
                        _objCharacter?.OnPropertyChanged(Type == ExpenseType.Nuyen ? nameof(Character.CareerNuyen) : nameof(Character.CareerKarma));
                }
            }
        }

        /// <summary>
        /// Should this Expense be presented to the Total Career Karma and Nuyen values?
        /// </summary>
        public bool ForceCareerVisible
        {
            get => _blnForceCareerVisible;
            set => _blnForceCareerVisible = value;
        }

        /// <summary>
        /// Undo object.
        /// </summary>
        public ExpenseUndo Undo { get; set; }

        #endregion

        public bool Equals(ExpenseLogEntry other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return CompareTo(other) == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ExpenseLogEntry) obj);
        }

        public override int GetHashCode()
        {
            return new {_objCharacter, Date, Amount, Reason, Refund, ForceCareerVisible}.GetHashCode();
        }

        public static bool operator ==(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            return !(left == right);
        }

        public static bool operator <(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ExpenseLogEntry left, ExpenseLogEntry right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
