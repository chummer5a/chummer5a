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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public sealed class Story : IHasLockObject, IHasCharacterObject
    {
        private readonly ConcurrentDictionary<string, StoryModule> _dicPersistentModules = new ConcurrentDictionary<string, StoryModule>();
        private readonly Character _objCharacter;
        private readonly ThreadSafeObservableCollection<StoryModule> _lstStoryModules;
        private bool _blnNeedToRegeneratePersistents = true;

        // Note: as long as this is only used to generate language-agnostic information, it can be cached once when the object is created and left that way.
        // If this is used to generate some language-specific information, then it will need to be re-built every time the user changes the language in which their story is generated.
        private readonly XPathNavigator _xmlStoryDocumentBaseNode;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        public Story(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstStoryModules = new ThreadSafeObservableCollection<StoryModule>(LockObject);
            _xmlStoryDocumentBaseNode = objCharacter.LoadDataXPath("stories.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _lstStoryModules.CollectionChangedAsync += StoryModulesOnCollectionChanged;
        }

        private async Task StoryModulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    await using (await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.NewItems)
                            objModule.ParentStory = this;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    await using (await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.OldItems)
                        {
                            if (objModule.ParentStory == this)
                            {
                                objModule.ParentStory = null;
                                await objModule.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    await using (await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.OldItems)
                        {
                            if (objModule.ParentStory == this && !e.NewItems.Contains(objModule))
                            {
                                objModule.ParentStory = null;
                                await objModule.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        foreach (StoryModule objModule in e.NewItems)
                            objModule.ParentStory = this;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    await using (await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false))
                    {
                        _blnNeedToRegeneratePersistents = true;
                    }

                    break;
                }
            }
        }

        public ThreadSafeObservableCollection<StoryModule> Modules
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstStoryModules;
            }
        }

        public ConcurrentDictionary<string, StoryModule> PersistentModules
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicPersistentModules;
            }
        }

        public async Task<StoryModule> GeneratePersistentModule(string strFunction, CancellationToken token = default)
        {
            XPathNavigator xmlStoryPool = _xmlStoryDocumentBaseNode.TryGetNodeByNameOrId("storypools/storypool", strFunction);
            if (xmlStoryPool != null)
            {
                XPathNodeIterator xmlPossibleStoryList = xmlStoryPool.SelectAndCacheExpression("story", token: token);
                Dictionary<string, int> dicStoriesListWithWeights = new Dictionary<string, int>(xmlPossibleStoryList.Count);
                int intTotalWeight = 0;
                foreach (XPathNavigator xmlStory in xmlPossibleStoryList)
                {
                    string strStoryId = xmlStory.SelectSingleNodeAndCacheExpression("id", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strStoryId))
                    {
                        if (!int.TryParse(xmlStory.SelectSingleNodeAndCacheExpression("weight", token: token)?.Value ?? "1", out int intWeight))
                            intWeight = 1;
                        intTotalWeight += intWeight;
                        if (dicStoriesListWithWeights.TryGetValue(strStoryId, out int intExistingWeight))
                            dicStoriesListWithWeights[strStoryId] = intExistingWeight + intWeight;
                        else
                            dicStoriesListWithWeights.Add(strStoryId, intWeight);
                    }
                }

                int intRandomResult = await Utils.GlobalRandom.NextModuloBiasRemovedAsync(intTotalWeight, token: token).ConfigureAwait(false);
                string strSelectedId = string.Empty;
                foreach (KeyValuePair<string, int> objStoryId in dicStoriesListWithWeights)
                {
                    intRandomResult -= objStoryId.Value;
                    if (intRandomResult <= 0)
                    {
                        strSelectedId = objStoryId.Key;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XPathNavigator xmlNewPersistentNode = _xmlStoryDocumentBaseNode.TryGetNodeByNameOrId("stories/story", strSelectedId);
                    if (xmlNewPersistentNode != null)
                    {
                        StoryModule objPersistentStoryModule = new StoryModule(_objCharacter);
                        try
                        {
                            objPersistentStoryModule.ParentStory = this;
                            objPersistentStoryModule.IsRandomlyGenerated = true;
                            await objPersistentStoryModule.CreateAsync(xmlNewPersistentNode, token).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            _dicPersistentModules.TryAdd(strFunction, objPersistentStoryModule);
                            return objPersistentStoryModule;
                        }
                        catch
                        {
                            await objPersistentStoryModule.DisposeAsync().ConfigureAwait(false);
                            throw;
                        }
                    }
                }
            }

            return null;
        }

        public async Task GeneratePersistentsAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            await using (await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                List<string> lstPersistentKeysToRemove
                    = new List<string>(_dicPersistentModules.Count);
                foreach (KeyValuePair<string, StoryModule> kvpModule in _dicPersistentModules)
                {
                    token.ThrowIfCancellationRequested();
                    if (kvpModule.Value.IsRandomlyGenerated)
                        lstPersistentKeysToRemove.Add(kvpModule.Key);
                }

                foreach (string strKey in lstPersistentKeysToRemove)
                {
                    token.ThrowIfCancellationRequested();
                    _dicPersistentModules.TryRemove(strKey, out _);
                }

                await Modules.ForEachAsync(x => x.TestRunToGeneratePersistents(objCulture, strLanguage, token), token)
                             .ConfigureAwait(false);
                _blnNeedToRegeneratePersistents = false;
            }
        }

        public async Task<string> PrintStory(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            await using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (_blnNeedToRegeneratePersistents)
                    await GeneratePersistentsAsync(objCulture, strLanguage, token).ConfigureAwait(false);
                await using (await Modules.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    int intCount = await Modules.GetCountAsync(token).ConfigureAwait(false);
                    string[] astrModuleOutputStrings = ArrayPool<string>.Shared.Rent(intCount);
                    try
                    {
                        for (int i = 0; i < intCount; ++i)
                        {
                            astrModuleOutputStrings[i]
                                = await (await Modules.GetValueAtAsync(i, token).ConfigureAwait(false))
                                    .PrintModule(objCulture, strLanguage, token)
                                    .ConfigureAwait(false);
                        }

                        return StringExtensions.ConcatFast(astrModuleOutputStrings, 0, intCount);
                    }
                    finally
                    {
                        if (astrModuleOutputStrings != null)
                            ArrayPool<string>.Shared.Return(astrModuleOutputStrings);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                _lstStoryModules.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await using (await LockObject.EnterWriteLockAsync().ConfigureAwait(false))
            {
                await _lstStoryModules.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
