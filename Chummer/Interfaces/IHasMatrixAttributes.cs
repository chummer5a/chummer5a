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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public interface IHasMatrixAttributes
    {
        int GetBaseMatrixAttribute(string strAttributeName);

        int GetBonusMatrixAttribute(string strAttributeName);

        bool IsProgram { get; }
        string Attack { get; set; }
        string Sleaze { get; set; }
        string DataProcessing { get; set; }
        string Firewall { get; set; }
        string ModAttack { get; set; }
        string ModSleaze { get; set; }
        string ModDataProcessing { get; set; }
        string ModFirewall { get; set; }
        string Overclocked { get; set; }

        string CanFormPersona { get; set; }
        bool IsCommlink { get; }

        string DeviceRating { get; set; }
        int BaseMatrixBoxes { get; }
        int BonusMatrixBoxes { get; set; }
        int TotalBonusMatrixBoxes { get; }
        int MatrixCM { get; }
        int MatrixCMFilled { get; set; }
        string ProgramLimit { get; set; }

        bool CanSwapAttributes { get; set; }
        string AttributeArray { get; set; }
        string ModAttributeArray { get; set; }

        IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes { get; }
    }

    public static class MatrixAttributes
    {
        public static readonly ReadOnlyCollection<string> MatrixAttributeStrings = Array.AsReadOnly(new[]
            { "Attack", "Sleaze", "Data Processing", "Firewall", "Device Rating", "Program Limit" });

        /// <summary>
        /// Get the total value of a Matrix attribute of this gear after children and Overclocker
        /// </summary>
        public static int GetTotalMatrixAttribute(this IHasMatrixAttributes objThis, string strAttributeName)
        {
            if (objThis == null)
                return 0;
            return objThis.GetBaseMatrixAttribute(strAttributeName) + objThis.GetBonusMatrixAttribute(strAttributeName);
        }

        /// <summary>
        /// Whether or not this Commlink is active and counting towards the character's Matrix Initiative.
        /// </summary>
        public static bool IsActiveCommlink(this IHasMatrixAttributes objThis, Character objCharacter)
        {
            if (objThis == null || objCharacter == null || !objThis.IsCommlink)
                return false;
            return objCharacter.ActiveCommlink == objThis;
        }

        /// <summary>
        /// Set a Commlink to be active and count towards the character's Matrix Initiative.
        /// </summary>
        public static void SetActiveCommlink(this IHasMatrixAttributes objThis, Character objCharacter, bool blnValue)
        {
            if (objThis == null || objCharacter == null)
                return;

            if (blnValue && objThis.IsCommlink)
            {
                objCharacter.ActiveCommlink = objThis;
            }
            else if (objCharacter.ActiveCommlink == objThis)
            {
                objCharacter.ActiveCommlink = null;
            }
        }

        /// <summary>
        /// Whether or not this is an A.I.'s Home Node.
        /// </summary>
        public static bool IsHomeNode(this IHasMatrixAttributes objThis, Character objCharacter)
        {
            if (objThis == null || objCharacter == null)
                return false;
            return objCharacter.HomeNode == objThis;
        }

        /// <summary>
        /// Set whether or not this is an A.I.'s Home Node.
        /// </summary>
        public static void SetHomeNode(this IHasMatrixAttributes objThis, Character objCharacter, bool blnValue)
        {
            if (objThis == null || objCharacter == null)
                return;

            if (blnValue)
                objCharacter.HomeNode = objThis;
            else if (objCharacter.ActiveCommlink == objThis)
                objCharacter.ActiveCommlink = null;
        }

        /// <summary>
        /// Gets a Matrix Attribute string based on its name
        /// </summary>
        /// <param name="strAttributeName">Name of the Matrix Attribute</param>
        /// <param name="objThis">Object whose Matrix Attribute to get.</param>
        /// <returns></returns>
        public static string GetMatrixAttributeString(this IHasMatrixAttributes objThis, string strAttributeName)
        {
            if (objThis == null)
                return string.Empty;
            switch (strAttributeName)
            {
                case "Attack":
                    return objThis.Attack;

                case "Sleaze":
                    return objThis.Sleaze;

                case "Data Processing":
                    return objThis.DataProcessing;

                case "Firewall":
                    return objThis.Firewall;

                case "Mod Attack":
                    return objThis.ModAttack;

                case "Mod Sleaze":
                    return objThis.ModSleaze;

                case "Mod Data Processing":
                    return objThis.ModDataProcessing;

                case "Mod Firewall":
                    return objThis.ModFirewall;

                case "Device Rating":
                    return objThis.DeviceRating;

                case "Program Limit":
                    return objThis.ProgramLimit;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Refreshes a set of ComboBoxes corresponding to Matrix attributes
        /// </summary>
        public static void RefreshMatrixAttributeComboBoxes(this IHasMatrixAttributes objThis, ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDataProcessing, ComboBox cboFirewall)
        {
            if (objThis == null)
                return;
            if (cboAttack == null)
                throw new ArgumentNullException(nameof(cboAttack));
            if (cboSleaze == null)
                throw new ArgumentNullException(nameof(cboSleaze));
            if (cboDataProcessing == null)
                throw new ArgumentNullException(nameof(cboDataProcessing));
            if (cboFirewall == null)
                throw new ArgumentNullException(nameof(cboFirewall));

            int intBaseAttack = objThis.GetBaseMatrixAttribute("Attack");
            int intBaseSleaze = objThis.GetBaseMatrixAttribute("Sleaze");
            int intBaseDataProcessing = objThis.GetBaseMatrixAttribute("Data Processing");
            int intBaseFirewall = objThis.GetBaseMatrixAttribute("Firewall");
            int intBonusAttack = objThis.GetBonusMatrixAttribute("Attack");
            int intBonusSleaze = objThis.GetBonusMatrixAttribute("Sleaze");
            int intBonusDataProcessing = objThis.GetBonusMatrixAttribute("Data Processing");
            int intBonusFirewall = objThis.GetBonusMatrixAttribute("Firewall");

            cboAttack.SuspendLayout();
            cboSleaze.SuspendLayout();
            cboDataProcessing.SuspendLayout();
            cboFirewall.SuspendLayout();
            cboAttack.BeginUpdate();
            cboSleaze.BeginUpdate();
            cboDataProcessing.BeginUpdate();
            cboFirewall.BeginUpdate();

            cboAttack.Enabled = false;
            cboAttack.BindingContext = new BindingContext();
            cboAttack.DataSource = new List<string>(4) { (intBaseAttack + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo), (intBaseSleaze + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo), (intBaseDataProcessing + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo), (intBaseFirewall + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo) };
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = objThis.CanSwapAttributes;

            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.DataSource = new List<string>(4) { (intBaseAttack + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo), (intBaseSleaze + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo), (intBaseDataProcessing + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo), (intBaseFirewall + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo) };
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = objThis.CanSwapAttributes;

            cboDataProcessing.Enabled = false;
            cboDataProcessing.BindingContext = new BindingContext();
            cboDataProcessing.DataSource = new List<string>(4) { (intBaseAttack + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo), (intBaseSleaze + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo), (intBaseDataProcessing + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo), (intBaseFirewall + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo) };
            cboDataProcessing.SelectedIndex = 2;
            cboDataProcessing.Visible = true;
            cboDataProcessing.Enabled = objThis.CanSwapAttributes;

            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.DataSource = new List<string>(4) { (intBaseAttack + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo), (intBaseSleaze + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo), (intBaseDataProcessing + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo), (intBaseFirewall + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo) };
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = objThis.CanSwapAttributes;

            cboAttack.EndUpdate();
            cboSleaze.EndUpdate();
            cboDataProcessing.EndUpdate();
            cboFirewall.EndUpdate();
            cboAttack.ResumeLayout();
            cboSleaze.ResumeLayout();
            cboDataProcessing.ResumeLayout();
            cboFirewall.ResumeLayout();
        }

        public static bool ProcessMatrixAttributeComboBoxChange(this IHasMatrixAttributes objThis, Character objCharacter, ComboBox cboChangedAttribute, ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDataProcessing, ComboBox cboFirewall)
        {
            if (objThis == null)
                return false;
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (cboChangedAttribute == null)
                throw new ArgumentNullException(nameof(cboChangedAttribute));
            if (cboAttack == null)
                throw new ArgumentNullException(nameof(cboAttack));
            if (cboSleaze == null)
                throw new ArgumentNullException(nameof(cboSleaze));
            if (cboDataProcessing == null)
                throw new ArgumentNullException(nameof(cboDataProcessing));
            if (cboFirewall == null)
                throw new ArgumentNullException(nameof(cboFirewall));

            string strTemp;
            Action<string> funcAttributePropertySetter;

            if (cboChangedAttribute == cboAttack)
            {
                strTemp = objThis.Attack;
                funcAttributePropertySetter = (x => objThis.Attack = x);
            }
            else if (cboChangedAttribute == cboSleaze)
            {
                strTemp = objThis.Sleaze;
                funcAttributePropertySetter = (x => objThis.Sleaze = x);
            }
            else if (cboChangedAttribute == cboDataProcessing)
            {
                strTemp = objThis.DataProcessing;
                funcAttributePropertySetter = (x => objThis.DataProcessing = x);
            }
            else if (cboChangedAttribute == cboFirewall)
            {
                strTemp = objThis.Firewall;
                funcAttributePropertySetter = (x => objThis.Firewall = x);
            }
            else
                return false;

            int intCurrentIndex = cboChangedAttribute.SelectedIndex;
            bool blnRefreshCharacter = false;
            // Find the combo with the same value as this one and change it to the missing value.
            if (cboChangedAttribute != cboAttack && cboAttack.SelectedIndex == intCurrentIndex)
            {
                funcAttributePropertySetter.Invoke(objThis.Attack);
                objThis.Attack = strTemp;
                blnRefreshCharacter = true;
            }
            else if (cboChangedAttribute != cboSleaze && cboSleaze.SelectedIndex == intCurrentIndex)
            {
                funcAttributePropertySetter.Invoke(objThis.Sleaze);
                objThis.Sleaze = strTemp;
                blnRefreshCharacter = true;
            }
            else if (cboChangedAttribute != cboDataProcessing && cboDataProcessing.SelectedIndex == intCurrentIndex)
            {
                funcAttributePropertySetter.Invoke(objThis.DataProcessing);
                objThis.DataProcessing = strTemp;
                blnRefreshCharacter = true;
            }
            else if (cboChangedAttribute != cboFirewall && cboFirewall.SelectedIndex == intCurrentIndex)
            {
                funcAttributePropertySetter.Invoke(objThis.Firewall);
                objThis.Firewall = strTemp;
                blnRefreshCharacter = true;
            }

            if (blnRefreshCharacter)
            {
                objThis.RefreshMatrixAttributeComboBoxes(cboAttack, cboSleaze, cboDataProcessing, cboFirewall);
            }

            return blnRefreshCharacter && (objThis.IsActiveCommlink(objCharacter) || objThis.IsHomeNode(objCharacter));
        }

        /// <summary>
        /// If this item has an attribute array, refresh it.
        /// </summary>
        /// <param name="objThis"></param>
        public static void RefreshMatrixAttributeArray(this IHasMatrixAttributes objThis)
        {
            if (objThis == null)
                return;
            if (!objThis.CanSwapAttributes)
                return;
            int intBaseAttack = objThis.GetBaseMatrixAttribute("Attack");
            int intBaseSleaze = objThis.GetBaseMatrixAttribute("Sleaze");
            int intBaseDataProcessing = objThis.GetBaseMatrixAttribute("Data Processing");
            int intBaseFirewall = objThis.GetBaseMatrixAttribute("Firewall");
            List<int> lstStatsArray = new List<int>(4)
            {
                intBaseAttack,
                intBaseSleaze,
                intBaseDataProcessing,
                intBaseFirewall
            };
            lstStatsArray.Sort();
            lstStatsArray.Reverse();

            string[] strCyberdeckArray = objThis.AttributeArray.Split(',');
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCyberdeckArray0))
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCyberdeckArray1))
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCyberdeckArray2))
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdCyberdeckArray3))
            {
                sbdCyberdeckArray0.Append(strCyberdeckArray[0]);
                sbdCyberdeckArray1.Append(strCyberdeckArray[1]);
                sbdCyberdeckArray2.Append(strCyberdeckArray[2]);
                sbdCyberdeckArray3.Append(strCyberdeckArray[3]);
                StringBuilder[] asbdCyberdeckArray =
                {
                    sbdCyberdeckArray0,
                    sbdCyberdeckArray1,
                    sbdCyberdeckArray2,
                    sbdCyberdeckArray3
                };
                foreach (string strLoopArrayText in objThis.ChildrenWithMatrixAttributes.Select(
                             x => x.ModAttributeArray))
                {
                    if (string.IsNullOrEmpty(strLoopArrayText))
                        continue;
                    string[] strLoopArray = strLoopArrayText.Split(',');
                    for (int i = 0; i < 4; ++i)
                    {
                        asbdCyberdeckArray[i].Append("+(").Append(strLoopArray[i]).Append(')');
                    }
                }

                for (int i = 0; i < 4; ++i)
                {
                    if (intBaseAttack == lstStatsArray[i])
                    {
                        objThis.Attack = asbdCyberdeckArray[i].ToString();
                        lstStatsArray[i] = int.MinValue;
                        break;
                    }
                }

                for (int i = 0; i < 4; ++i)
                {
                    if (intBaseSleaze == lstStatsArray[i])
                    {
                        objThis.Sleaze = asbdCyberdeckArray[i].ToString();
                        lstStatsArray[i] = int.MinValue;
                        break;
                    }
                }

                for (int i = 0; i < 4; ++i)
                {
                    if (intBaseDataProcessing == lstStatsArray[i])
                    {
                        objThis.DataProcessing = asbdCyberdeckArray[i].ToString();
                        lstStatsArray[i] = int.MinValue;
                        break;
                    }
                }

                for (int i = 0; i < 4; ++i)
                {
                    if (intBaseFirewall == lstStatsArray[i])
                    {
                        objThis.Firewall = asbdCyberdeckArray[i].ToString();
                        break;
                    }
                }
            }
        }
    }
}
