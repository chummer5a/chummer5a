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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public interface IHasMatrixAttributes : IHasCharacterObject
    {
        int GetBaseMatrixAttribute(string strAttributeName);

        Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default);

        int GetBonusMatrixAttribute(string strAttributeName);

        Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default);

        /// <summary>
        /// Whether the Gear qualifies as a Program in the printout XML.
        /// </summary>
        bool IsProgram { get; }
        string Attack { get; set; }
        string Sleaze { get; set; }
        string DataProcessing { get; set; }
        string Firewall { get; set; }
        string ModAttack { get; set; }
        string ModSleaze { get; set; }
        string ModDataProcessing { get; set; }
        string ModFirewall { get; set; }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        string Overclocked { get; set; }
        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        Task<string> GetOverclockedAsync(CancellationToken token = default);
        /// <summary>
        /// String to determine if the device can form persona or grants persona forming to its parent.
        /// </summary>
        string CanFormPersona { get; set; }
        /// <summary>
        /// String to determine if the device can form persona or grants persona forming to its parent.
        /// </summary>
        Task<string> GetCanFormPersonaAsync(CancellationToken token = default);
        /// <summary>
        /// Is this device one that can form a persona?
        /// </summary>
        bool IsCommlink { get; }
        /// <summary>
        /// Is this device one that can form a persona?
        /// </summary>
        Task<bool> GetIsCommlinkAsync(CancellationToken token = default);

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
        /// Get the total value of a Matrix attribute of this gear after children and Overclocker
        /// </summary>
        public static async Task<int> GetTotalMatrixAttributeAsync(this IHasMatrixAttributes objThis,
            string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null)
                return 0;
            return await objThis.GetBaseMatrixAttributeAsync(strAttributeName, token).ConfigureAwait(false) +
                   await objThis.GetBonusMatrixAttributeAsync(strAttributeName, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether this Commlink is active and counting towards the character's Matrix Initiative.
        /// </summary>
        public static bool IsActiveCommlink(this IHasMatrixAttributes objThis, Character objCharacter)
        {
            if (objThis == null || objCharacter == null)
                return false;
            return objCharacter.ActiveCommlink == objThis;
        }

        /// <summary>
        /// Whether this Commlink is active and counting towards the character's Matrix Initiative.
        /// </summary>
        public static async Task<bool> IsActiveCommlinkAsync(this IHasMatrixAttributes objThis,
            Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null || objCharacter == null)
                return false;
            return await objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) == objThis;
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
        /// Set a Commlink to be active and count towards the character's Matrix Initiative.
        /// </summary>
        public static async Task SetActiveCommlinkAsync(this IHasMatrixAttributes objThis, Character objCharacter,
            bool blnValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null || objCharacter == null)
                return;

            if (blnValue && await objThis.GetIsCommlinkAsync(token).ConfigureAwait(false))
            {
                await objCharacter.SetActiveCommlinkAsync(objThis, token).ConfigureAwait(false);
            }
            else if (await objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) == objThis)
            {
                await objCharacter.SetActiveCommlinkAsync(null, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this is an A.I.'s Home Node.
        /// </summary>
        public static bool IsHomeNode(this IHasMatrixAttributes objThis, Character objCharacter)
        {
            if (objThis == null || objCharacter == null)
                return false;
            return objCharacter.HomeNode == objThis;
        }

        /// <summary>
        /// Whether this is an A.I.'s Home Node.
        /// </summary>
        public static async Task<bool> IsHomeNodeAsync(this IHasMatrixAttributes objThis, Character objCharacter,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null || objCharacter == null)
                return false;
            return await objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false) == objThis;
        }

        /// <summary>
        /// Set whether this is an A.I.'s Home Node.
        /// </summary>
        public static void SetHomeNode(this IHasMatrixAttributes objThis, Character objCharacter, bool blnValue)
        {
            if (objThis == null || objCharacter == null)
                return;

            if (blnValue)
                objCharacter.HomeNode = objThis;
            else if (objCharacter.HomeNode == objThis)
                objCharacter.HomeNode = null;
        }

        public static async Task SetHomeNodeAsync(this IHasMatrixAttributes objThis, Character objCharacter, bool blnValue,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null || objCharacter == null)
                return;
            if (blnValue)
                await objCharacter.SetHomeNodeAsync(objThis, token).ConfigureAwait(false);
            else if (await objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false) == objThis)
                await objCharacter.SetHomeNodeAsync(null, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a Matrix Attribute string based on its name
        /// </summary>
        /// <param name="strAttributeName">Name of the Matrix Attribute</param>
        /// <param name="objThis">Object whose Matrix Attribute to get.</param>
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
        public static void RefreshMatrixAttributeComboBoxes(this IHasMatrixAttributes objThis, ElasticComboBox cboAttack, ElasticComboBox cboSleaze, ElasticComboBox cboDataProcessing, ElasticComboBox cboFirewall)
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

            IDisposable objThisLocker = null;
            if (objThis is IHasLockObject objHasLock)
                objThisLocker = objHasLock.LockObject.EnterReadLock();
            try
            {
                int intBaseAttack = objThis.GetBaseMatrixAttribute("Attack");
                int intBaseSleaze = objThis.GetBaseMatrixAttribute("Sleaze");
                int intBaseDataProcessing = objThis.GetBaseMatrixAttribute("Data Processing");
                int intBaseFirewall = objThis.GetBaseMatrixAttribute("Firewall");
                int intBonusAttack = objThis.GetBonusMatrixAttribute("Attack");
                int intBonusSleaze = objThis.GetBonusMatrixAttribute("Sleaze");
                int intBonusDataProcessing = objThis.GetBonusMatrixAttribute("Data Processing");
                int intBonusFirewall = objThis.GetBonusMatrixAttribute("Firewall");
                bool blnCanSwapAttributes = objThis.CanSwapAttributes;

                cboAttack.DoThreadSafe(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                });
                cboSleaze.DoThreadSafe(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                });
                cboDataProcessing.DoThreadSafe(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                });
                cboFirewall.DoThreadSafe(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                });

                try
                {
                    cboAttack.DoThreadSafe(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 0;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    });

                    cboSleaze.DoThreadSafe(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 1;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    });

                    cboDataProcessing.DoThreadSafe(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusDataProcessing).ToString(GlobalSettings
                                .InvariantCultureInfo),
                            (intBaseFirewall + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 2;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    });

                    cboFirewall.DoThreadSafe(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 3;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    });
                }
                finally
                {
                    cboAttack.DoThreadSafe(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    });
                    cboSleaze.DoThreadSafe(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    });
                    cboDataProcessing.DoThreadSafe(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    });
                    cboFirewall.DoThreadSafe(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    });
                }
            }
            finally
            {
                objThisLocker?.Dispose();
            }
        }

        /// <summary>
        /// Refreshes a set of ComboBoxes corresponding to Matrix attributes
        /// </summary>
        public static async Task RefreshMatrixAttributeComboBoxesAsync(this IHasMatrixAttributes objThis,
            ElasticComboBox cboAttack, ElasticComboBox cboSleaze, ElasticComboBox cboDataProcessing,
            ElasticComboBox cboFirewall, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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

            IAsyncDisposable objThisLocker = null;
            if (objThis is IHasLockObject objHasLock)
                objThisLocker = await objHasLock.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBaseAttack = await objThis.GetBaseMatrixAttributeAsync("Attack", token).ConfigureAwait(false);
                int intBaseSleaze = await objThis.GetBaseMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false);
                int intBaseDataProcessing =
                    await objThis.GetBaseMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false);
                int intBaseFirewall =
                    await objThis.GetBaseMatrixAttributeAsync("Firewall", token).ConfigureAwait(false);
                int intBonusAttack = await objThis.GetBonusMatrixAttributeAsync("Attack", token).ConfigureAwait(false);
                int intBonusSleaze = await objThis.GetBonusMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false);
                int intBonusDataProcessing = await objThis.GetBonusMatrixAttributeAsync("Data Processing", token)
                    .ConfigureAwait(false);
                int intBonusFirewall =
                    await objThis.GetBonusMatrixAttributeAsync("Firewall", token).ConfigureAwait(false);
                bool blnCanSwapAttributes = objThis.CanSwapAttributes;

                await cboAttack.DoThreadSafeAsync(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                }, token).ConfigureAwait(false);
                await cboSleaze.DoThreadSafeAsync(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                }, token).ConfigureAwait(false);
                await cboDataProcessing.DoThreadSafeAsync(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                }, token).ConfigureAwait(false);
                await cboFirewall.DoThreadSafeAsync(x =>
                {
                    x.SuspendLayout();
                    x.BeginUpdate();
                }, token).ConfigureAwait(false);

                try
                {
                    await cboAttack.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusAttack).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 0;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    }, token).ConfigureAwait(false);

                    await cboSleaze.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusSleaze).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 1;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    }, token).ConfigureAwait(false);

                    await cboDataProcessing.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusDataProcessing).ToString(GlobalSettings
                                .InvariantCultureInfo),
                            (intBaseFirewall + intBonusDataProcessing).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 2;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    }, token).ConfigureAwait(false);

                    await cboFirewall.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        x.BindingContext = new BindingContext();
                        x.DataSource = new List<string>(4)
                        {
                            (intBaseAttack + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseSleaze + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseDataProcessing + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo),
                            (intBaseFirewall + intBonusFirewall).ToString(GlobalSettings.InvariantCultureInfo)
                        };
                        x.SelectedIndex = 3;
                        x.Visible = true;
                        x.Enabled = blnCanSwapAttributes;
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    await cboAttack.DoThreadSafeAsync(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    }, token).ConfigureAwait(false);
                    await cboSleaze.DoThreadSafeAsync(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    }, token).ConfigureAwait(false);
                    await cboDataProcessing.DoThreadSafeAsync(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    }, token).ConfigureAwait(false);
                    await cboFirewall.DoThreadSafeAsync(x =>
                    {
                        x.EndUpdate();
                        x.ResumeLayout();
                    }, token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objThisLocker != null)
                    await objThisLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static bool ProcessMatrixAttributeComboBoxChange(this IHasMatrixAttributes objThis, Character objCharacter, ElasticComboBox cboChangedAttribute, ElasticComboBox cboAttack, ElasticComboBox cboSleaze, ElasticComboBox cboDataProcessing, ElasticComboBox cboFirewall)
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

            IDisposable objThisLocker = null;
            if (objThis is IHasLockObject objHasLock)
                objThisLocker = objHasLock.LockObject.EnterUpgradeableReadLock();
            try
            {
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

                int intCurrentIndex = cboChangedAttribute.DoThreadSafeFunc(x => x.SelectedIndex);
                bool blnRefreshCharacter = false;
                bool blnDPChanged = cboChangedAttribute == cboDataProcessing;
                // Find the combo with the same value as this one and change it to the missing value.
                if (cboChangedAttribute != cboAttack &&
                    cboAttack.DoThreadSafeFunc(x => x.SelectedIndex) == intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Attack);
                    objThis.Attack = strTemp;
                    blnRefreshCharacter = true;
                }
                else if (cboChangedAttribute != cboSleaze &&
                         cboSleaze.DoThreadSafeFunc(x => x.SelectedIndex) == intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Sleaze);
                    objThis.Sleaze = strTemp;
                    blnRefreshCharacter = true;
                }
                else if (!blnDPChanged && cboDataProcessing.DoThreadSafeFunc(x => x.SelectedIndex) == intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.DataProcessing);
                    objThis.DataProcessing = strTemp;
                    blnRefreshCharacter = true;
                    blnDPChanged = true;
                }
                else if (cboChangedAttribute != cboFirewall &&
                         cboFirewall.DoThreadSafeFunc(x => x.SelectedIndex) == intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Firewall);
                    objThis.Firewall = strTemp;
                    blnRefreshCharacter = true;
                }

                if (blnRefreshCharacter)
                {
                    objThis.RefreshMatrixAttributeComboBoxes(cboAttack, cboSleaze, cboDataProcessing, cboFirewall);
                    if (objThis.IsActiveCommlink(objCharacter) || objThis.IsHomeNode(objCharacter))
                    {
                        if (blnDPChanged)
                        {
                            if (objThis.IsActiveCommlink(objCharacter))
                            {
                                if (objThis.IsHomeNode(objCharacter))
                                    objCharacter.OnMultiplePropertyChanged(nameof(Character.MatrixInitiativeValue),
                                        nameof(Character.MatrixInitiativeColdValue),
                                        nameof(Character.MatrixInitiativeHotValue));
                                else
                                    objCharacter.OnMultiplePropertyChanged(nameof(Character.MatrixInitiativeColdValue),
                                        nameof(Character.MatrixInitiativeHotValue));
                            }
                            else
                                objCharacter.OnPropertyChanged(nameof(Character.MatrixInitiativeValue));
                        }

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                objThisLocker?.Dispose();
            }
        }

        public static async Task<bool> ProcessMatrixAttributeComboBoxChangeAsync(this IHasMatrixAttributes objThis,
            Character objCharacter, ElasticComboBox cboChangedAttribute, ElasticComboBox cboAttack,
            ElasticComboBox cboSleaze, ElasticComboBox cboDataProcessing, ElasticComboBox cboFirewall,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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

            IAsyncDisposable objThisLocker = null;
            if (objThis is IHasLockObject objHasLock)
                objThisLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
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

                int intCurrentIndex = await cboChangedAttribute.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                    .ConfigureAwait(false);
                bool blnRefreshCharacter = false;
                bool blnDPChanged = cboChangedAttribute == cboDataProcessing;
                // Find the combo with the same value as this one and change it to the missing value.
                if (cboChangedAttribute != cboAttack &&
                    await cboAttack.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) ==
                    intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Attack);
                    objThis.Attack = strTemp;
                    blnRefreshCharacter = true;
                }
                else if (cboChangedAttribute != cboSleaze &&
                         await cboSleaze.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) ==
                         intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Sleaze);
                    objThis.Sleaze = strTemp;
                    blnRefreshCharacter = true;
                }
                else if (!blnDPChanged &&
                         await cboDataProcessing.DoThreadSafeFuncAsync(x => x.SelectedIndex, token)
                             .ConfigureAwait(false) ==
                         intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.DataProcessing);
                    objThis.DataProcessing = strTemp;
                    blnRefreshCharacter = true;
                    blnDPChanged = true;
                }
                else if (cboChangedAttribute != cboFirewall &&
                         await cboFirewall.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) ==
                         intCurrentIndex)
                {
                    funcAttributePropertySetter.Invoke(objThis.Firewall);
                    objThis.Firewall = strTemp;
                    blnRefreshCharacter = true;
                }

                if (blnRefreshCharacter)
                {
                    await objThis
                        .RefreshMatrixAttributeComboBoxesAsync(cboAttack, cboSleaze, cboDataProcessing, cboFirewall,
                            token)
                        .ConfigureAwait(false);
                    if (await objThis.IsActiveCommlinkAsync(objCharacter, token).ConfigureAwait(false) ||
                        await objThis.IsHomeNodeAsync(objCharacter, token).ConfigureAwait(false))
                    {
                        if (blnDPChanged)
                        {
                            if (await objThis.IsActiveCommlinkAsync(objCharacter, token).ConfigureAwait(false))
                            {
                                if (await objThis.IsHomeNodeAsync(objCharacter, token).ConfigureAwait(false))
                                    await objCharacter.OnMultiplePropertyChangedAsync(token,
                                        nameof(Character.MatrixInitiativeValue),
                                        nameof(Character.MatrixInitiativeColdValue),
                                        nameof(Character.MatrixInitiativeHotValue)).ConfigureAwait(false);
                                else
                                    await objCharacter.OnMultiplePropertyChangedAsync(token,
                                        nameof(Character.MatrixInitiativeColdValue),
                                        nameof(Character.MatrixInitiativeHotValue)).ConfigureAwait(false);
                            }
                            else
                                await objCharacter
                                    .OnPropertyChangedAsync(nameof(Character.MatrixInitiativeValue), token)
                                    .ConfigureAwait(false);
                        }

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                if (objThisLocker != null)
                    await objThisLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If this item has an attribute array, refresh it.
        /// </summary>
        public static void RefreshMatrixAttributeArray(this IHasMatrixAttributes objThis, Character objCharacter)
        {
            if (objThis == null)
                return;
            if (!(objThis is IHasLockObject objHasLock))
                objHasLock = objThis.CharacterObject;
            IDisposable objLocker = objHasLock?.LockObject.EnterUpgradeableReadLock();
            try
            {
                if (objThis.CanSwapAttributes)
                {
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
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCyberdeckArray0))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCyberdeckArray1))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCyberdeckArray2))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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

                        IDisposable objLocker2 = objHasLock?.LockObject.EnterWriteLock();
                        try
                        {
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
                        finally
                        {
                            objLocker2?.Dispose();
                        }
                    }
                }

                if (objCharacter != null)
                {
                    if (objThis.IsActiveCommlink(objCharacter))
                    {
                        if (objThis.IsHomeNode(objCharacter))
                            objCharacter.OnMultiplePropertyChanged(nameof(Character.MatrixInitiativeValue),
                                                                   nameof(Character.MatrixInitiativeColdValue),
                                                                   nameof(Character.MatrixInitiativeHotValue));
                        else
                            objCharacter.OnMultiplePropertyChanged(nameof(Character.MatrixInitiativeColdValue),
                                                                   nameof(Character.MatrixInitiativeHotValue));
                    }
                    else
                        objCharacter.OnPropertyChanged(nameof(Character.MatrixInitiativeValue));
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        /// <summary>
        /// If this item has an attribute array, refresh it.
        /// </summary>
        public static async Task RefreshMatrixAttributeArrayAsync(this IHasMatrixAttributes objThis,
            Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objThis == null)
                return;
            if (!(objThis is IHasLockObject objHasLock))
                objHasLock = objThis.CharacterObject;
            IAsyncDisposable objLocker = null;
            if (objHasLock != null)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (objThis.CanSwapAttributes)
                {
                    int intBaseAttack = await objThis.GetBaseMatrixAttributeAsync("Attack", token).ConfigureAwait(false);
                    int intBaseSleaze = await objThis.GetBaseMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false);
                    int intBaseDataProcessing = await objThis.GetBaseMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false);
                    int intBaseFirewall = await objThis.GetBaseMatrixAttributeAsync("Firewall", token).ConfigureAwait(false);
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
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdCyberdeckArray0))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdCyberdeckArray1))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdCyberdeckArray2))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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

                        IAsyncDisposable objLocker2 = null;
                        if (objHasLock != null)
                            objLocker2 = await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
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
                        finally
                        {
                            if (objLocker2 != null)
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                if (objCharacter != null)
                {
                    if (await objThis.IsActiveCommlinkAsync(objCharacter, token).ConfigureAwait(false))
                    {
                        if (await objThis.IsHomeNodeAsync(objCharacter, token).ConfigureAwait(false))
                            await objCharacter.OnMultiplePropertyChangedAsync(token,
                                nameof(Character.MatrixInitiativeValue),
                                nameof(Character.MatrixInitiativeColdValue),
                                nameof(Character.MatrixInitiativeHotValue)).ConfigureAwait(false);
                        else
                            await objCharacter.OnMultiplePropertyChangedAsync(token,
                                nameof(Character.MatrixInitiativeColdValue),
                                nameof(Character.MatrixInitiativeHotValue)).ConfigureAwait(false);
                    }
                    else
                        await objCharacter.OnPropertyChangedAsync(nameof(Character.MatrixInitiativeValue), token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
