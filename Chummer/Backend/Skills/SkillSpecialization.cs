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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Skills
{
    /// <summary>
    /// Type of Specialization
    /// </summary>
    public sealed class SkillSpecialization : IHasName, IHasXmlDataNode, IHasLockObject
    {
        private Guid _guiID;
        private int _intNameLoaded;
        private Task<string> _tskNameLoader;
        private CancellationTokenSource _objNameLoaderCancellationTokenSource;
        private string _strName;
        private readonly bool _blnFree;
        private readonly bool _blnExpertise;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public SkillSpecialization(Character objCharacter, string strName, bool blnFree = false, bool blnExpertise = false)
        {
            _objCharacter = objCharacter;
            _strName = strName; // Shouldn't create tasks in constructors because of potential unexpected behavior
            _guiID = Guid.NewGuid();
            _blnFree = blnFree;
            _blnExpertise = blnExpertise;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (EnterReadLock.Enter(LockObject))
            {
                objWriter.WriteStartElement("spec");
                objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("name", Name);
                objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("expertise", _blnExpertise.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Re-create a saved SkillSpecialization from an XmlNode;
        /// </summary>
        /// <param name="objCharacter">Character to load for.</param>
        /// <param name="xmlNode">XmlNode to load.</param>
        public static SkillSpecialization Load(Character objCharacter, XmlNode xmlNode)
        {
            string strName = string.Empty;
            if (!xmlNode.TryGetStringFieldQuickly("name", ref strName) || string.IsNullOrEmpty(strName))
                return null;
            if (!xmlNode.TryGetField("guid", Guid.TryParse, out Guid guiTemp))
                guiTemp = Guid.NewGuid();

            return new SkillSpecialization(objCharacter, strName, xmlNode["free"]?.InnerText == bool.TrueString, xmlNode["expertise"]?.InnerText == bool.TrueString)
            {
                _guiID = guiTemp
            };
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // <skillspecialization>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("skillspecialization", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync(
                              "name", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                              token: token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync("free", Free.ToString(GlobalSettings.InvariantCultureInfo),
                                                   token: token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync("expertise", Expertise.ToString(GlobalSettings.InvariantCultureInfo),
                                                   token: token).ConfigureAwait(false);
                    await objWriter
                          .WriteElementStringAsync("specbonus",
                                                   (await GetSpecializationBonusAsync(token).ConfigureAwait(false))
                                                   .ToString(objCulture), token: token).ConfigureAwait(false);
                }
                finally
                {
                    // </skillspecialization>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (EnterReadLock.Enter(LockObject))
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? Name;
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("@translate", token).ConfigureAwait(false))
                    ?.Value ?? Name
                    : Name;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists in the program's current language.
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The Skill to which this specialization belongs
        /// </summary>
        public Skill Parent
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _objParent;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    _objParent = value;
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                if (Parent == null)
                    objReturn = null;
                else
                    objReturn = (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? Parent.GetNode(strLanguage, token: token)
                            : await Parent.GetNodeAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.SelectSingleNode("specs/spec[. = " + Name.CleanXPath() + ']');
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;
        private Skill _objParent;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                if (Parent == null)
                    objReturn = null;
                else
                    objReturn = (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? Parent.GetNodeXPath(strLanguage, token: token)
                            : await Parent.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.SelectSingleNode("specs/spec[. = " + Name.CleanXPath() + ']');
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string Name
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intNameLoaded <= 1)
                    {
                        int intOld = Interlocked.CompareExchange(ref _intNameLoaded, 2, 1);
                        while (intOld < 3) // Need this in case we reset name while in the process of fetching it
                        {
                            if (intOld == 0)
                            {
                                CancellationTokenSource objNewSource = new CancellationTokenSource();
                                CancellationToken objToken = objNewSource.Token;
                                CancellationTokenSource objOldSource
                                    = Interlocked.CompareExchange(ref _objNameLoaderCancellationTokenSource,
                                                                  objNewSource,
                                                                  null);
                                // Cancellation token source is only null if it's our first time running
                                if (objOldSource == null && Interlocked.CompareExchange(ref _intNameLoaded, 2, 0) == 0)
                                {
                                    Task<string> tskNewTask
                                        = Task.Run(
                                            () => _objCharacter.ReverseTranslateExtraAsync(
                                                _strName, GlobalSettings.Language, "skills.xml", objToken), objToken);
                                    Task<string> tskOld
                                        = Interlocked.CompareExchange(ref _tskNameLoader, tskNewTask, null);
                                    if (tskOld != null)
                                    {
                                        // This should never happen, throw an exception immediately
                                        Utils.BreakIfDebug();
                                        throw new InvalidOperationException();
                                    }

                                    _strName = Utils.SafelyRunSynchronously(() => _tskNameLoader);
                                    intOld = Interlocked.CompareExchange(ref _intNameLoaded, 3, 2);
                                }
                                else
                                    objNewSource.Dispose();
                            }

                            while (intOld == 0 || intOld == 2)
                            {
                                Utils.SafeSleep();
                                intOld = Interlocked.CompareExchange(ref _intNameLoaded, 2, 1);
                            }
                        }
                    }

                    return _strName;
                }
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Name == value)
                        return;
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationToken objToken = objNewSource.Token;
                    using (LockObject.EnterWriteLock())
                    {
                        _intNameLoaded = 0;
                        CancellationTokenSource objOldSource
                            = Interlocked.Exchange(ref _objNameLoaderCancellationTokenSource, objNewSource);
                        if (objOldSource != null)
                        {
                            objOldSource.Cancel(false);
                            objOldSource.Dispose();
                        }

                        Task<string> tskOld = Interlocked.Exchange(ref _tskNameLoader, Task.Run(
                                                                       () => _objCharacter.ReverseTranslateExtraAsync(
                                                                           value, GlobalSettings.Language, "skills.xml",
                                                                           objToken), objToken));
                        if (tskOld != null)
                            Utils.SafelyRunSynchronously(() => tskOld);
                        Interlocked.CompareExchange(ref _intNameLoaded, 1, 0);
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                }
            }
        }

        /// <summary>
        /// Is this a forced specialization (true) or player entered (false)
        /// </summary>
        public bool Free
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnFree;
            }
        }

        /// <summary>
        /// Does this specialization give an extra bonus on top of the normal bonus that specializations give (used by SASS' Inspired and by 6e)
        /// </summary>
        public bool Expertise
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnExpertise;
            }
        }

        /// <summary>
        /// The bonus this specialization gives to relevant dicepools
        /// </summary>
        public int SpecializationBonus
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intReturn = 0;
                    if (ImprovementManager
                        .GetCachedImprovementListForValueOf(_objCharacter,
                                                            Improvement.ImprovementType.DisableSpecializationEffects,
                                                            Parent.DictionaryKey)
                        .Count == 0)
                    {
                        if (Expertise)
                            intReturn += _objCharacter.Settings.ExpertiseBonus;
                        else
                            intReturn += _objCharacter.Settings.SpecializationBonus;
                    }

                    decimal decBonus = 0;
                    foreach (Improvement objImprovement in Parent.RelevantImprovements(
                                 x => x.Condition == Name && !x.AddToRating, blnIncludeConditionals: true))
                    {
                        switch (objImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.Skill:
                            case Improvement.ImprovementType.SkillBase:
                            case Improvement.ImprovementType.SkillCategory:
                            case Improvement.ImprovementType.SkillGroup:
                            case Improvement.ImprovementType.SkillGroupBase:
                                decBonus += objImprovement.Rating;
                                break;
                        }
                    }

                    return intReturn + decBonus.StandardRound();
                }
            }
        }

        /// <summary>
        /// The bonus this specialization gives to relevant dicepools
        /// </summary>
        public async ValueTask<int> GetSpecializationBonusAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intReturn = 0;
                if ((await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                                                    Improvement.ImprovementType
                                                                               .DisableSpecializationEffects,
                                                                    await Parent.GetDictionaryKeyAsync(token)
                                                                        .ConfigureAwait(false), token: token)
                           .ConfigureAwait(false))
                    .Count == 0)
                {
                    if (Expertise)
                        intReturn += _objCharacter.Settings.ExpertiseBonus;
                    else
                        intReturn += _objCharacter.Settings.SpecializationBonus;
                }

                decimal decBonus = 0;
                foreach (Improvement objImprovement in await Parent.RelevantImprovementsAsync(
                                                                       x => x.Condition == Name && !x.AddToRating,
                                                                       blnIncludeConditionals: true, token: token)
                                                                   .ConfigureAwait(false))
                {
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.Skill:
                        case Improvement.ImprovementType.SkillBase:
                        case Improvement.ImprovementType.SkillCategory:
                        case Improvement.ImprovementType.SkillGroup:
                        case Improvement.ImprovementType.SkillGroupBase:
                            decBonus += objImprovement.Rating;
                            break;
                    }
                }

                return intReturn + decBonus.StandardRound();
            }
        }

        #endregion Properties

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                CancellationTokenSource objSource
                    = Interlocked.Exchange(ref _objNameLoaderCancellationTokenSource, null);
                if (objSource != null)
                {
                    objSource.Cancel(false);
                    objSource.Dispose();
                }

                Task<string> tskOld = Interlocked.Exchange(ref _tskNameLoader, null);
                if (tskOld != null)
                    Utils.SafelyRunSynchronously(() => tskOld);
            }
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                CancellationTokenSource objSource
                    = Interlocked.Exchange(ref _objNameLoaderCancellationTokenSource, null);
                if (objSource != null)
                {
                    objSource.Cancel(false);
                    objSource.Dispose();
                }

                Task<string> tskOld = Interlocked.Exchange(ref _tskNameLoader, null);
                if (tskOld != null)
                    await tskOld.ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
