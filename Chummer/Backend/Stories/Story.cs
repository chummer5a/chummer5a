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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public class Story
    {
        private readonly ConcurrentDictionary<string, StoryModule> _dicPersistentModules = new ConcurrentDictionary<string, StoryModule>();
        private readonly Character _objCharacter;
        private readonly ObservableCollection<StoryModule> _lstStoryModules = new ObservableCollection<StoryModule>();
        private bool _blnNeedToRegeneratePersistents = true;

        // Note: as long as this is only used to generate language-agnostic information, it can be cached once when the object is created and left that way.
        // If this is used to generate some language-specific information, then it will need to be re-built every time the user changes the language in which their story is generated.
        private readonly XPathNavigator _xmlStoryDocumentBaseNode;

        public Story(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _xmlStoryDocumentBaseNode = XmlManager.Load("stories.xml", objCharacter.Options.CustomDataDictionary).GetFastNavigator().SelectSingleNode("/chummer");
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

        public ObservableCollection<StoryModule> Modules => _lstStoryModules;

        public ConcurrentDictionary<string, StoryModule> PersistentModules => _dicPersistentModules;

        public StoryModule GeneratePersistentModule(string strFunction)
        {
            XPathNavigator xmlStoryPool = _xmlStoryDocumentBaseNode.SelectSingleNode("storypools/storypool[name = \"" + strFunction + "\"]");
            if (xmlStoryPool != null)
            {
                XPathNodeIterator xmlPossibleStoryList = xmlStoryPool.Select("story");
                Dictionary<string, int> dicStoriesListWithWeights = new Dictionary<string, int>(xmlPossibleStoryList.Count);
                int intTotalWeight = 0;
                foreach (XPathNavigator xmlStory in xmlPossibleStoryList)
                {
                    string strStoryId = xmlStory.SelectSingleNode("id")?.Value;
                    if (!string.IsNullOrEmpty(strStoryId))
                    {
                        if (!int.TryParse(xmlStory.SelectSingleNode("weight")?.Value ?? "1", out int intWeight))
                            intWeight = 1;
                        intTotalWeight += intWeight;
                        if (dicStoriesListWithWeights.ContainsKey(strStoryId))
                            dicStoriesListWithWeights[strStoryId] += intWeight;
                        else
                            dicStoriesListWithWeights.Add(strStoryId, intWeight);
                    }
                }

                int intRandomResult = GlobalOptions.RandomGenerator.NextModuloBiasRemoved(intTotalWeight);
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
                    XPathNavigator xmlNewPersistentNode = _xmlStoryDocumentBaseNode.SelectSingleNode("stories/story[id = \"" + strSelectedId + "\"]");
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

        public void GeneratePersistents(CultureInfo objCulture, string strLanguage)
        {
            List<string> lstPersistentKeysToRemove = new List<string>();
            foreach (KeyValuePair<string, StoryModule> objPersistentModule in _dicPersistentModules)
            {
                if (objPersistentModule.Value.IsRandomlyGenerated)
                    lstPersistentKeysToRemove.Add(objPersistentModule.Key);
            }

            foreach (string strKey in lstPersistentKeysToRemove)
                _dicPersistentModules.TryRemove(strKey, out StoryModule _);

            Parallel.ForEach(Modules, x =>
            {
                x.TestRunToGeneratePersistents(objCulture, strLanguage);
            });
            _blnNeedToRegeneratePersistents = false;
        }

        public string PrintStory(CultureInfo objCulture, string strLanguage)
        {
            if (_blnNeedToRegeneratePersistents)
                GeneratePersistents(objCulture, strLanguage);

            object objOutputLock = new object();
            string[] strModuleOutputStrings = new string[Modules.Count];
            Parallel.For(0, strModuleOutputStrings.Length, i =>
            {
                string strModuleOutput = Modules[i].PrintModule(objCulture, strLanguage);
                lock (objOutputLock)
                    strModuleOutputStrings[i] = strModuleOutput;
            });
            return string.Concat(strModuleOutputStrings);
        }
    }
}
