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
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public sealed class Story : IHasLockObject
    {
        private readonly LockingDictionary<string, StoryModule> _dicPersistentModules = new LockingDictionary<string, StoryModule>();
        private readonly Character _objCharacter;
        private readonly ThreadSafeObservableCollection<StoryModule> _lstStoryModules = new ThreadSafeObservableCollection<StoryModule>();
        private bool _blnNeedToRegeneratePersistents = true;

        // Note: as long as this is only used to generate language-agnostic information, it can be cached once when the object is created and left that way.
        // If this is used to generate some language-specific information, then it will need to be re-built every time the user changes the language in which their story is generated.
        private readonly XPathNavigator _xmlStoryDocumentBaseNode;

        public Story(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _xmlStoryDocumentBaseNode = objCharacter.LoadDataXPath("stories.xml").SelectSingleNodeAndCacheExpression("/chummer");
            _lstStoryModules.CollectionChanged += StoryModulesOnCollectionChanged;
        }

        private void StoryModulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        using (LockObject.EnterReadLock())
                        {
                            _blnNeedToRegeneratePersistents = true;
                            foreach (StoryModule objModule in e.NewItems)
                                objModule.ParentStory = this;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        using (LockObject.EnterReadLock())
                        {
                            _blnNeedToRegeneratePersistents = true;
                            foreach (StoryModule objModule in e.OldItems)
                            {
                                if (objModule.ParentStory == this)
                                {
                                    objModule.ParentStory = null;
                                    objModule.Dispose();
                                }
                            }
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        using (LockObject.EnterReadLock())
                        {
                            _blnNeedToRegeneratePersistents = true;
                            foreach (StoryModule objModule in e.OldItems)
                            {
                                if (objModule.ParentStory == this && !e.NewItems.Contains(objModule))
                                {
                                    objModule.ParentStory = null;
                                    objModule.Dispose();
                                }
                            }

                            foreach (StoryModule objModule in e.NewItems)
                                objModule.ParentStory = this;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        using (LockObject.EnterReadLock())
                            _blnNeedToRegeneratePersistents = true;
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

        public LockingDictionary<string, StoryModule> PersistentModules
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicPersistentModules;
            }
        }

        public async ValueTask<StoryModule> GeneratePersistentModule(string strFunction, CancellationToken token = default)
        {
            XPathNavigator xmlStoryPool = _xmlStoryDocumentBaseNode.TryGetNodeByNameOrId("storypools/storypool", strFunction);
            if (xmlStoryPool != null)
            {
                XPathNodeIterator xmlPossibleStoryList = await xmlStoryPool.SelectAndCacheExpressionAsync("story", token: token).ConfigureAwait(false);
                Dictionary<string, int> dicStoriesListWithWeights = new Dictionary<string, int>(xmlPossibleStoryList.Count);
                int intTotalWeight = 0;
                foreach (XPathNavigator xmlStory in xmlPossibleStoryList)
                {
                    string strStoryId = (await xmlStory.SelectSingleNodeAndCacheExpressionAsync("id", token: token).ConfigureAwait(false))?.Value;
                    if (!string.IsNullOrEmpty(strStoryId))
                    {
                        if (!int.TryParse((await xmlStory.SelectSingleNodeAndCacheExpressionAsync("weight", token: token).ConfigureAwait(false))?.Value ?? "1", out int intWeight))
                            intWeight = 1;
                        intTotalWeight += intWeight;
                        if (dicStoriesListWithWeights.TryGetValue(strStoryId, out int intExistingWeight))
                            dicStoriesListWithWeights[strStoryId] = intExistingWeight + intWeight;
                        else
                            dicStoriesListWithWeights.Add(strStoryId, intWeight);
                    }
                }

                int intRandomResult = await GlobalSettings.RandomGenerator.NextModuloBiasRemovedAsync(intTotalWeight, token: token).ConfigureAwait(false);
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
                        StoryModule objPersistentStoryModule = new StoryModule(_objCharacter)
                        {
                            ParentStory = this,
                            IsRandomlyGenerated = true
                        };
                        try
                        {
                            await objPersistentStoryModule.CreateAsync(xmlNewPersistentNode, token).ConfigureAwait(false);
                            await _dicPersistentModules.TryAddAsync(strFunction, objPersistentStoryModule, token).ConfigureAwait(false);
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

        public async ValueTask GeneratePersistentsAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                List<string> lstPersistentKeysToRemove
                    = new List<string>(await _dicPersistentModules.GetCountAsync(token).ConfigureAwait(false));
                await _dicPersistentModules.ForEachAsync(x =>
                {
                    if (x.Value.IsRandomlyGenerated)
                        lstPersistentKeysToRemove.Add(x.Key);
                }, token).ConfigureAwait(false);

                foreach (string strKey in lstPersistentKeysToRemove)
                    await _dicPersistentModules.RemoveAsync(strKey, token).ConfigureAwait(false);

                await Modules.ForEachAsync(x => x.TestRunToGeneratePersistents(objCulture, strLanguage, token).AsTask(), token)
                             .ConfigureAwait(false);
                _blnNeedToRegeneratePersistents = false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask<string> PrintStory(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_blnNeedToRegeneratePersistents)
                    await GeneratePersistentsAsync(objCulture, strLanguage, token).ConfigureAwait(false);
                string[] strModuleOutputStrings;
                using (await Modules.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                {
                    int intCount = await Modules.GetCountAsync(token).ConfigureAwait(false);
                    strModuleOutputStrings = new string[intCount];
                    for (int i = 0; i < intCount; ++i)
                    {
                        strModuleOutputStrings[i]
                            = await (await Modules.GetValueAtAsync(i, token).ConfigureAwait(false))
                                    .PrintModule(objCulture, strLanguage, token)
                                    .ConfigureAwait(false);
                    }
                }

                return string.Concat(strModuleOutputStrings);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                _dicPersistentModules.Dispose();
                _lstStoryModules.Dispose();
            }
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await _dicPersistentModules.DisposeAsync().ConfigureAwait(false);
                await _lstStoryModules.DisposeAsync().ConfigureAwait(false);
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
