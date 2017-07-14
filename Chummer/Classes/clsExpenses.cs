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
        AddQuality = 1,
        ImproveSkillGroup = 2,
        AddSkill = 3,
        ImproveSkill = 4,
        SkillSpec = 5,
        AddMartialArt = 6,
        ImproveMartialArt = 7,
        AddMartialArtManeuver = 8,
        AddSpell = 9,
        AddComplexForm = 10,
        ImproveComplexForm = 11,
        AddComplexFormOption = 12,
        ImproveComplexFormOption = 13,
        AddMetamagic = 14,
        ImproveInitiateGrade = 15,
        RemoveQuality = 16,
        ManualAdd = 17,
        ManualSubtract = 18,
        BindFocus = 19,
        JoinGroup = 20,
        LeaveGroup = 21,
        QuickeningMetamagic = 22,
        AddPowerPoint = 23,
        AddSpecialization = 24,
        AddAIProgram = 25,
        AddAIAdvancedProgram = 26,
        AddCritterPower = 27,
        SpiritFettering = 28
    }

    public enum NuyenExpenseType
    {
        AddCyberware = 0,
        IncreaseLifestyle = 1,
        AddArmor = 2,
        AddArmorMod = 3,
        AddWeapon = 4,
        AddWeaponMod = 5,
        AddWeaponAccessory = 6,
        AddGear = 7,
        AddVehicle = 8,
        AddVehicleMod = 9,
        AddVehicleGear = 10,
        AddVehicleWeapon = 11,
        AddVehicleWeaponMod = 12,
        AddVehicleWeaponAccessory = 13,
        ManualAdd = 14,
        ManualSubtract = 15,
        AddArmorGear = 16,
        AddVehicleModCyberware = 17,
        AddCyberwareGear = 18,
        AddWeaponGear = 19,
        ImproveInitiateGrade = 20,
    }

    /// <summary>
    /// Undo information for an Expense Log Entry.
    /// </summary>
    public class ExpenseUndo
    {
        private KarmaExpenseType _objKarmaExpenseType;
        private NuyenExpenseType _objNuyenExpenseType;
        private string _strObjectId;
        private int _intQty = 0;
        private string _strExtra = string.Empty;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a KarmaExpenseType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public KarmaExpenseType ConvertToKarmaExpenseType(string strValue)
        {
            KarmaExpenseType result;
            return Enum.TryParse(strValue, out result) ? result : KarmaExpenseType.ManualAdd;
        }

        /// <summary>
        /// Convert a string to a NuyenExpenseType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public NuyenExpenseType ConvertToNuyenExpenseType(string strValue)
        {
            NuyenExpenseType result;
            return Enum.TryParse(strValue, out result) ? result : NuyenExpenseType.ManualAdd;
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
        public ExpenseUndo CreateNuyen(NuyenExpenseType objExpenseType, string strObjectId, int intQty = 0)
        {
            _objNuyenExpenseType = objExpenseType;
            _strObjectId = strObjectId;
            _intQty = intQty;

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
            objWriter.WriteElementString("qty", _intQty.ToString());
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
            objNode.TryGetInt32FieldQuickly("qty", ref _intQty);
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
        public int Qty
        {
            get
            {
                return _intQty;
            }
            set
            {
                _intQty = value;
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
        private Guid _guiID = new Guid();
        private DateTime _datDate = new DateTime();
        private int _intAmount = 0;
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
        public ExpenseType ConvertToExpenseType(string strValue)
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
        public ExpenseLogEntry()
        {
            _guiID = Guid.NewGuid();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
        }

        /// <summary>
        /// Create a new Expense Log Entry.
        /// </summary>
        /// <param name="intKarma">Amount of the Karma/Nuyen expense.</param>
        /// <param name="strReason">Reason for the Karma/Nueyn change.</param>
        /// <param name="objExpenseType">Type of expense, either Karma or Nuyen.</param>
        /// <param name="datDate">Date and time of the Expense.</param>
        /// <param name="blnRefund">Whether or not this expense is a Karma refund.</param>
        public ExpenseLogEntry Create(int intKarma, string strReason, ExpenseType objExpenseType, DateTime datDate, bool blnRefund = false)
        {
            if (blnRefund)
                strReason += " (" + LanguageManager.Instance.GetString("String_Expense_Refund") + ")";
            _intAmount = intKarma;
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
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("date", _datDate.ToString("s"));
            objWriter.WriteElementString("amount", _intAmount.ToString());
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
            objNode.TryGetInt32FieldQuickly("amount", ref _intAmount);
            objNode.TryGetStringFieldQuickly("reason", ref _strReason);
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
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("expense");
            objWriter.WriteElementString("date", _datDate.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("amount", _intAmount.ToString());
            objWriter.WriteElementString("reason", _strReason);
            objWriter.WriteElementString("type", _objExpenseType.ToString());
            objWriter.WriteElementString("refund", _blnRefund.ToString());
            objWriter.WriteEndElement();
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
                return _guiID.ToString();
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
        public int Amount
        {
            get
            {
                return _intAmount;
            }
            set
            {
                _intAmount = value;
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