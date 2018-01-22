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
﻿using System;
using System.Xml;
using Chummer.Backend;
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
        ImproveMartialArt,
        AddMartialArtManeuver,
        AddSpell,
        AddComplexForm,
        ImproveComplexForm,
        AddComplexFormOption,
        ImproveComplexFormOption,
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
    public class ExpenseUndo
    {
        private KarmaExpenseType _objKarmaExpenseType;
        private NuyenExpenseType _objNuyenExpenseType;
        private string _strObjectId;
        private decimal _decQty = 0;
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
            _objKarmaExpenseType = objExpenseType;
            _strObjectId = strObjectId;

            return this;
        }

        /// <summary>
        /// Create the ExpenseUndo Entry.
        /// </summary>
        /// <param name="objExpenseType">Nuyen expense type.</param>
        /// <param name="strObjectId">Object identifier.</param>
        /// <param name="intQty">Amount of Nuyen.</param>
        public ExpenseUndo CreateNuyen(NuyenExpenseType objExpenseType, string strObjectId, decimal decQty = 0)
        {
            _objNuyenExpenseType = objExpenseType;
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
            objWriter.WriteStartElement("undo");
            objWriter.WriteElementString("karmatype", _objKarmaExpenseType.ToString());
            objWriter.WriteElementString("nuyentype", _objNuyenExpenseType.ToString());
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
                _objKarmaExpenseType = ConvertToKarmaExpenseType(objNode["karmatype"].InnerText);
            if (objNode["nuyentype"] != null)
                _objNuyenExpenseType = ConvertToNuyenExpenseType(objNode["nuyentype"].InnerText);
            objNode.TryGetStringFieldQuickly("objectid", ref _strObjectId);
            objNode.TryGetDecFieldQuickly("qty", ref _decQty);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Karma Expense Type.
        /// </summary>
        public KarmaExpenseType KarmaType
        {
            get
            {
                return _objKarmaExpenseType;
            }
            set
            {
                _objKarmaExpenseType = value;
            }
        }

        /// <summary>
        /// Nuyen Expense Type.
        /// </summary>
        public NuyenExpenseType NuyenType
        {
            get
            {
                return _objNuyenExpenseType;
            }
            set
            {
                _objNuyenExpenseType = value;
            }
        }

        /// <summary>
        /// Object InternalId.
        /// </summary>
        public string ObjectId
        {
            get
            {
                return _strObjectId;
            }
            set
            {
                _strObjectId = value;
            }
        }

        /// <summary>
        /// Quantity of items added (Nuyen only).
        /// </summary>
        public decimal Qty
        {
            get
            {
                return _decQty;
            }
            set
            {
                _decQty = value;
            }
        }

        /// <summary>
        /// Extra information.
        /// </summary>
        public string Extra
        {
            get
            {
                return _strExtra;
            }
            set
            {
                _strExtra = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Exense Log Entry.
    /// </summary>
    public class ExpenseLogEntry
    {
        private Guid _guiID = Guid.Empty;
        private readonly Character _objCharacter;
        private DateTime _datDate = new DateTime();
        private decimal _decAmount = 0;
        private string _strReason = string.Empty;
        private ExpenseType _objExpenseType;
        private bool _blnRefund = false;
        private ExpenseUndo _objUndo;

        #region Helper Methods
        /// <summary>
        /// ExpenseLogEntry Comparer.
        /// </summary>
        public static int CompareDate(ExpenseLogEntry x, ExpenseLogEntry y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (y == null)
                    return 1;
                else
                {
                    int intReturn = y.Date.CompareTo(x.Date);
                    return intReturn;
                }
            }
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
        /// <param name="strReason">Reason for the Karma/Nueyn change.</param>
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
            objWriter.WriteStartElement("expense");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("date", _datDate.ToString("s", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("amount", _decAmount.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("reason", _strReason);
            objWriter.WriteElementString("type", _objExpenseType.ToString());
            objWriter.WriteElementString("refund", _blnRefund.ToString());
            if (_objUndo != null)
                _objUndo.Save(objWriter);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the KarmaLogEntry from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _datDate = DateTime.Parse(objNode["date"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            objNode.TryGetDecFieldQuickly("amount", ref _decAmount);
            if (objNode.TryGetStringFieldQuickly("reason", ref _strReason))
                _strReason.TrimEnd(" (" + LanguageManager.GetString("String_Expense_Refund", GlobalOptions.Language) + ')');
            if (objNode["type"] != null)
                _objExpenseType = ConvertToExpenseType(objNode["type"].InnerText);
            objNode.TryGetBoolFieldQuickly("refund", ref _blnRefund);

            if (objNode["undo"] != null)
            {
                _objUndo = new ExpenseUndo();
                _objUndo.Load(objNode["undo"]);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (Amount != 0 || _objCharacter.Options.PrintFreeExpenses)
            {
                objWriter.WriteStartElement("expense");
                objWriter.WriteElementString("date", Date.ToString(objCulture));
                objWriter.WriteElementString("amount", Amount.ToString(Type == ExpenseType.Nuyen ? _objCharacter.Options.NuyenFormat : "#,0.##", objCulture));
                objWriter.WriteElementString("reason", DisplayReason(strLanguageToPrint));
                objWriter.WriteElementString("type", Type.ToString());
                objWriter.WriteElementString("refund", Refund.ToString());
                objWriter.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Expense Log Entry.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString("D");
            }
            set
            {
                _guiID = Guid.Parse(value);
            }
        }

        /// <summary>
        /// Date the Exense Log Entry was made.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return _datDate;
            }
            set
            {
                _datDate = value;
            }
        }

        /// <summary>
        /// Karma/Nuyen amount gained or spent.
        /// </summary>
        public decimal Amount
        {
            get
            {
                return _decAmount;
            }
            set
            {
                _decAmount = value;
            }
        }

        /// <summary>
        /// The Reason for the Entry expense.
        /// </summary>
        public string Reason
        {
            get
            {
                return _strReason;
            }
            set
            {
                _strReason = value;
            }
        }

        /// <summary>
        /// The Reason for the Entry expense.
        /// </summary>
        public string DisplayReason(string strLanguage)
        {
            string strReturn = _strReason;
            if (_blnRefund)
                strReturn += " (" + LanguageManager.GetString("String_Expense_Refund", strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        /// The Expense type.
        /// </summary>
        public ExpenseType Type
        {
            get
            {
                return _objExpenseType;
            }
            set
            {
                _objExpenseType = value;
            }
        }

        /// <summary>
        /// Whether or not the Expense is a Karma refund.
        /// </summary>
        public bool Refund
        {
            get
            {
                return _blnRefund;
            }
            set
            {
                _blnRefund = value;
            }
        }

        /// <summary>
        /// Undo object.
        /// </summary>
        public ExpenseUndo Undo
        {
            get
            {
                return _objUndo;
            }
            set
            {
                _objUndo = value;
            }
        }
        #endregion
    }
}
