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
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public sealed class Story : IDisposable
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
            _lstStoryModules.CollectionChanged += LstStoryModulesOnCollectionChanged;
        }

        private void LstStoryModulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.NewItems)
                            objModule.ParentStory = this;
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.OldItems)
                            objModule.ParentStory = null;
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        _blnNeedToRegeneratePersistents = true;
                        foreach (StoryModule objModule in e.OldItems)
                            objModule.ParentStory = null;
                        foreach (StoryModule objModule in e.NewItems)
                            objModule.ParentStory = this;
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        _blnNeedToRegeneratePersistents = true;
                        break;
                    }
            }
        }

        public ThreadSafeObservableCollection<StoryModule> Modules
        {
            get
            {
                using (EnterReadLock.Enter(_objCharacter.LockObject))
                    return _lstStoryModules;
            }
        }

        public LockingDictionary<string, StoryModule> PersistentModules
        {
            get
            {
                using (EnterReadLock.Enter(_objCharacter.LockObject))
                    return _dicPersistentModules;
            }
        }

        public StoryModule GeneratePersistentModule(string strFunction)
        {
            XPathNavigator xmlStoryPool = _xmlStoryDocumentBaseNode.SelectSingleNode("storypools/storypool[name = " + strFunction.CleanXPath() + ']');
            if (xmlStoryPool != null)
            {
                XPathNodeIterator xmlPossibleStoryList = xmlStoryPool.SelectAndCacheExpression("story");
                Dictionary<string, int> dicStoriesListWithWeights = new Dictionary<string, int>(xmlPossibleStoryList.Count);
                int intTotalWeight = 0;
                foreach (XPathNavigator xmlStory in xmlPossibleStoryList)
                {
                    string strStoryId = xmlStory.SelectSingleNodeAndCacheExpression("id")?.Value;
                    if (!string.IsNullOrEmpty(strStoryId))
                    {
                        if (!int.TryParse(xmlStory.SelectSingleNodeAndCacheExpression("weight")?.Value ?? "1", out int intWeight))
                            intWeight = 1;
                        intTotalWeight += intWeight;
                        if (dicStoriesListWithWeights.TryGetValue(strStoryId, out int intExistingWeight))
                            dicStoriesListWithWeights[strStoryId] = intExistingWeight + intWeight;
                        else
                            dicStoriesListWithWeights.Add(strStoryId, intWeight);
                    }
                }

                int intRandomResult = GlobalSettings.RandomGenerator.NextModuloBiasRemoved(intTotalWeight);
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
                    XPathNavigator xmlNewPersistentNode = _xmlStoryDocumentBaseNode.SelectSingleNode("stories/story[id = " + strSelectedId.CleanXPath() + ']');
                    if (xmlNewPersistentNode != null)
                    {
                        StoryModule objPersistentStoryModule = new StoryModule(_objCharacter)
                        {
                            ParentStory = this,
                            IsRandomlyGenerated = true
                        };
                        objPersistentStoryModule.Create(xmlNewPersistentNode);
                        PersistentModules.TryAdd(strFunction, objPersistentStoryModule);
                        return objPersistentStoryModule;
                    }
                }
            }

            return null;
        }

        public async ValueTask GeneratePersistentsAsync(CultureInfo objCulture, string strLanguage)
        {
            List<string> lstPersistentKeysToRemove = new List<string>(_dicPersistentModules.Count);
            foreach (KeyValuePair<string, StoryModule> objPersistentModule in _dicPersistentModules)
            {
                if (objPersistentModule.Value.IsRandomlyGenerated)
                    lstPersistentKeysToRemove.Add(objPersistentModule.Key);
            }

            foreach (string strKey in lstPersistentKeysToRemove)
                await _dicPersistentModules.RemoveAsync(strKey);

            foreach (StoryModule objModule in Modules)
                await objModule.TestRunToGeneratePersistents(objCulture, strLanguage);
            _blnNeedToRegeneratePersistents = false;
        }

        public async ValueTask<string> PrintStory(CultureInfo objCulture, string strLanguage)
        {
            if (_blnNeedToRegeneratePersistents)
                await GeneratePersistentsAsync(objCulture, strLanguage);
            string[] strModuleOutputStrings = new string[Modules.Count];
            for (int i = 0; i < Modules.Count; ++i)
            {
                strModuleOutputStrings[i] = await Modules[i].PrintModule(objCulture, strLanguage);
            }
            return string.Concat(strModuleOutputStrings);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _dicPersistentModules.Dispose();
            _lstStoryModules.Dispose();
        }
    }
}
