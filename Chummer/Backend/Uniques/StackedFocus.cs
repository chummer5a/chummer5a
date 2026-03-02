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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// A Stacked Focus.
    /// </summary>
    [DebuggerDisplay("{Name(\"en-us\")}")]
    public sealed class StackedFocus : IHasLockObject, IHasCharacterObject
    {
        private Guid _guiID;
        private bool _blnBonded;
        private Guid _guiGearId;
        private readonly ThreadSafeList<Gear> _lstGear;
        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, and Load Methods

        public StackedFocus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstGear = new ThreadSafeList<Gear>(2, LockObject);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("stackedfocus");
                objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("gearid", _guiGearId.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("bonded", _blnBonded.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                    objGear.Save(objWriter);
                objWriter.WriteEndElement();
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Stacked Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            using (LockObject.EnterWriteLock())
            {
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                objNode.TryGetField("gearid", Guid.TryParse, out _guiGearId);
                _blnBonded = objNode["bonded"]?.InnerTextIsTrueString() == true;
                using (XmlNodeList nodGearList = objNode.SelectNodes("gears/gear"))
                {
                    if (nodGearList == null)
                        return;
                    foreach (XmlNode nodGear in nodGearList)
                    {
                        Gear objGear = new Gear(_objCharacter);
                        try
                        {
                            objGear.Load(nodGear);
                            _lstGear.Add(objGear);
                        }
                        catch
                        {
                            objGear.DeleteGear();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load the Stacked Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token);
            try
            {
                token.ThrowIfCancellationRequested();
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                objNode.TryGetField("gearid", Guid.TryParse, out _guiGearId);
                _blnBonded = objNode["bonded"]?.InnerTextIsTrueString() == true;
                using (XmlNodeList nodGearList = objNode.SelectNodes("gears/gear"))
                {
                    if (nodGearList != null)
                    {
                        foreach (XmlNode nodGear in nodGearList)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            try
                            {
                                await objGear.LoadAsync(nodGear, token: token).ConfigureAwait(false);
                                await _lstGear.AddAsync(objGear, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objGear.DeleteGearAsync(token: CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, and Load Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Stacked Focus in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// GUID of the linked Gear.
        /// </summary>
        public string GearId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiGearId.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                {
                    using (LockObject.EnterReadLock())
                    {
                        if (_guiGearId == guiTemp)
                            return;
                    }

                    using (LockObject.EnterUpgradeableReadLock())
                    {
                        if (_guiGearId == guiTemp)
                            return;
                        using (LockObject.EnterWriteLock())
                            _guiGearId = guiTemp;
                    }
                }
            }
        }

        /// <summary>
        /// Whether the Stacked Focus in Bonded.
        /// </summary>
        public bool Bonded
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnBonded;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnBonded == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnBonded == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnBonded = value;
                }
            }
        }

        /// <summary>
        /// The Stacked Focus' total Force.
        /// </summary>
        public int TotalForce
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Gear.Sum(x => x.Rating);
            }
        }

        /// <summary>
        /// The Stacked Focus' total Force.
        /// </summary>
        public async Task<int> GetTotalForceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await Gear.SumAsync(x => x.GetRatingAsync(token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <summary>
        /// The cost in Karma to bind this Stacked Focus.
        /// </summary>
        public int BindingCost
        {
            get
            {
                decimal decCost = 0;
                using (LockObject.EnterReadLock())
                {
                    foreach (Gear objFocus in Gear)
                    {
                        // Each Focus costs an amount of Karma equal to their Force x specific Karma cost.
                        string strFocusName = objFocus.Name;
                        string strFocusExtra = objFocus.Extra;
                        int intPosition = strFocusName.IndexOf('(');
                        if (intPosition > -1)
                            strFocusName = strFocusName.Substring(0, intPosition - 1);
                        intPosition = strFocusName.IndexOf(',');
                        if (intPosition > -1)
                            strFocusName = strFocusName.Substring(0, intPosition);
                        decimal decExtraKarmaCost = 0;
                        if (strFocusName.EndsWith(", Individualized, Complete", StringComparison.Ordinal))
                        {
                            decExtraKarmaCost = -2;
                            strFocusName = strFocusName.Replace(", Individualized, Complete", string.Empty);
                        }
                        else if (strFocusName.EndsWith(", Individualized, Partial", StringComparison.Ordinal))
                        {
                            decExtraKarmaCost = -1;
                            strFocusName = strFocusName.Replace(", Individualized, Partial", string.Empty);
                        }

                        decimal decKarmaMultiplier;
                        switch (strFocusName)
                        {
                            case "Qi Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaQiFocus;
                                break;

                            case "Sustaining Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaSustainingFocus;
                                break;

                            case "Counterspelling Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaCounterspellingFocus;
                                break;

                            case "Banishing Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaBanishingFocus;
                                break;

                            case "Binding Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaBindingFocus;
                                break;

                            case "Weapon Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaWeaponFocus;
                                break;

                            case "Spellcasting Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaSpellcastingFocus;
                                break;

                            case "Summoning Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaSummoningFocus;
                                break;

                            case "Alchemical Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaAlchemicalFocus;
                                break;

                            case "Centering Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaCenteringFocus;
                                break;

                            case "Masking Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaMaskingFocus;
                                break;

                            case "Disenchanting Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaDisenchantingFocus;
                                break;

                            case "Power Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaPowerFocus;
                                break;

                            case "Flexible Signature Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaFlexibleSignatureFocus;
                                break;

                            case "Ritual Spellcasting Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaRitualSpellcastingFocus;
                                break;

                            case "Spell Shaping Focus":
                                decKarmaMultiplier = _objCharacter.Settings.KarmaSpellShapingFocus;
                                break;

                            default:
                                decKarmaMultiplier = 1;
                                break;
                        }

                        _objCharacter.Improvements.ForEach(objLoopImprovement =>
                        {
                            if (objLoopImprovement.ImprovedName != strFocusName
                                || (!string.IsNullOrEmpty(objLoopImprovement.Target)
                                    && !strFocusExtra.Contains(objLoopImprovement.Target))
                                || !objLoopImprovement.Enabled)
                                return;

                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.FocusBindingKarmaCost:
                                    decExtraKarmaCost += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                                    decKarmaMultiplier += objLoopImprovement.Value;
                                    break;
                            }
                        });

                        decCost += objFocus.Rating * decKarmaMultiplier + decExtraKarmaCost;
                    }
                }

                return decCost.StandardRound();
            }
        }

        /// <summary>
        /// The cost in Karma to bind this Stacked Focus.
        /// </summary>
        public async Task<int> GetBindingCostAsync(CancellationToken token = default)
        {
            decimal decCost = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                decCost += await Gear.SumAsync(async objFocus =>
                {
                    // Each Focus costs an amount of Karma equal to their Force x specific Karma cost.
                    string strFocusName = objFocus.Name;
                    string strFocusExtra = objFocus.Extra;
                    int intPosition = strFocusName.IndexOf('(');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition - 1);
                    intPosition = strFocusName.IndexOf(',');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition);
                    decimal decExtraKarmaCost = 0;
                    if (strFocusName.EndsWith(", Individualized, Complete", StringComparison.Ordinal))
                    {
                        decExtraKarmaCost = -2;
                        strFocusName = strFocusName.Replace(", Individualized, Complete", string.Empty);
                    }
                    else if (strFocusName.EndsWith(", Individualized, Partial", StringComparison.Ordinal))
                    {
                        decExtraKarmaCost = -1;
                        strFocusName = strFocusName.Replace(", Individualized, Partial", string.Empty);
                    }

                    decimal decKarmaMultiplier;
                    CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                    switch (strFocusName)
                    {
                        case "Qi Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaQiFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Sustaining Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaSustainingFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Counterspelling Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaCounterspellingFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        case "Banishing Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaBanishingFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Binding Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaBindingFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Weapon Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaWeaponFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Spellcasting Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaSpellcastingFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        case "Summoning Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaSummoningFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Alchemical Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaAlchemicalFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Centering Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaCenteringFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Masking Focus":
                            decKarmaMultiplier
                                = await objSettings.GetKarmaMaskingFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Disenchanting Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaDisenchantingFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        case "Power Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaPowerFocusAsync(token).ConfigureAwait(false);
                            break;

                        case "Flexible Signature Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaFlexibleSignatureFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        case "Ritual Spellcasting Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaRitualSpellcastingFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        case "Spell Shaping Focus":
                            decKarmaMultiplier = await objSettings.GetKarmaSpellShapingFocusAsync(token)
                                .ConfigureAwait(false);
                            break;

                        default:
                            decKarmaMultiplier = 1;
                            break;
                    }

                    await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                        objLoopImprovement =>
                        {
                            if (objLoopImprovement.ImprovedName != strFocusName
                                || (!string.IsNullOrEmpty(objLoopImprovement.Target)
                                    && !strFocusExtra.Contains(objLoopImprovement.Target))
                                || !objLoopImprovement.Enabled)
                                return;

                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.FocusBindingKarmaCost:
                                    decExtraKarmaCost += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                                    decKarmaMultiplier += objLoopImprovement.Value;
                                    break;
                            }
                        }, token: token).ConfigureAwait(false);

                    return await objFocus.GetRatingAsync(token).ConfigureAwait(false) * decKarmaMultiplier + decExtraKarmaCost;
                }, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return decCost.StandardRound();
        }

        /// <summary>
        /// Stacked Focus Name.
        /// </summary>
        public string Name(CultureInfo objCulture, string strLanguage)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                using (LockObject.EnterReadLock())
                {
                    foreach (Gear objGear in Gear)
                    {
                        sbdReturn.Append(objGear.DisplayName(objCulture, strLanguage), ',', strSpace);
                    }
                }

                // Remove the trailing comma.
                if (sbdReturn.Length > 0)
                    sbdReturn.Length -= 2;

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Stacked Focus Name.
        /// </summary>
        public async Task<string> NameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await Gear.ForEachAsync(async objGear =>
                    {
                        sbdReturn.Append(await objGear.DisplayNameAsync(objCulture, strLanguage, token: token)
                                                      .ConfigureAwait(false), ',', strSpace);
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                // Remove the trailing comma.
                if (sbdReturn.Length > 0)
                    sbdReturn.Length -= 2;

                return sbdReturn.ToString();
            }
        }

        public string CurrentDisplayName => Name(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => NameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// List of Gear that make up the Stacked Focus.
        /// </summary>
        public ThreadSafeList<Gear> Gear
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstGear;
            }
        }

        #endregion Properties

        #region Methods

        public async Task<TreeNode> CreateTreeNode(Gear objGear, ContextMenuStrip cmsStackedFocus, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objGear == null)
                throw new ArgumentNullException(nameof(objGear));
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                TreeNode objNode = await objGear.CreateTreeNode(cmsStackedFocus, null, token).ConfigureAwait(false);

                objNode.Name = InternalId;
                objNode.Text = await LanguageManager.GetStringAsync("String_StackedFocus", token: token).ConfigureAwait(false)
                               + await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false)
                               + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false)
                               + await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                objNode.Tag = this;
                objNode.Checked = Bonded;

                return objNode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Methods

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
                _lstGear.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await _lstGear.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
