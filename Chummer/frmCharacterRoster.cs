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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmCharacterRoster : Form
    {
        private readonly ConcurrentDictionary<string, CharacterCache> _lstCharacterCache = new ConcurrentDictionary<string, CharacterCache>();

        private readonly FileSystemWatcher watcherCharacterRosterFolder;
        private bool _blnSkipUpdate;

        public frmCharacterRoster()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            ContextMenuStrip[] lstCMSToTranslate =
            {
                cmsRoster
            };

            foreach (ContextMenuStrip objCMS in lstCMSToTranslate)
            {
                if (objCMS != null)
                {
                    foreach (ToolStripMenuItem objItem in objCMS.Items.OfType<ToolStripMenuItem>())
                    {
                        LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
                    }
                }
            }

            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                watcherCharacterRosterFolder = new FileSystemWatcher(GlobalOptions.CharacterRosterPath, "*.chum5");
            }
        }
        private void frmCharacterRoster_Load(object sender, EventArgs e)
        {
            GlobalOptions.MRUChanged += PopulateCharacterList;
            treCharacterList.ItemDrag += treCharacterList_ItemDrag;
            treCharacterList.DragEnter += treCharacterList_DragEnter;
            treCharacterList.DragDrop += treCharacterList_DragDrop;
            treCharacterList.DragOver += treCharacterList_DragOver;

            if (watcherCharacterRosterFolder != null)
            {
                watcherCharacterRosterFolder.Changed += RefreshWatchListOnly;
                watcherCharacterRosterFolder.Created += RefreshWatchListOnly;
                watcherCharacterRosterFolder.Deleted += RefreshWatchListOnly;
                watcherCharacterRosterFolder.Renamed += RefreshWatchListOnly;
            }

            LoadCharacters();
            UpdateCharacter(null);
        }

        private void frmCharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {

            GlobalOptions.MRUChanged -= PopulateCharacterList;
            treCharacterList.ItemDrag -= treCharacterList_ItemDrag;
            treCharacterList.DragEnter -= treCharacterList_DragEnter;
            treCharacterList.DragDrop -= treCharacterList_DragDrop;
            treCharacterList.DragOver -= treCharacterList_DragOver;

            if (watcherCharacterRosterFolder != null)
            {
                watcherCharacterRosterFolder.Changed -= RefreshWatchListOnly;
                watcherCharacterRosterFolder.Created -= RefreshWatchListOnly;
                watcherCharacterRosterFolder.Deleted -= RefreshWatchListOnly;
                watcherCharacterRosterFolder.Renamed -= RefreshWatchListOnly;
            }
        }

        public void RefreshWatchListOnly(object sender, EventArgs e)
        {
            if (_blnSkipUpdate)
                return;

            SuspendLayout();
            LoadCharacters(false, false);
            ResumeLayout();
        }

        public void PopulateCharacterList(object sender, TextEventArgs e)
        {
            if (_blnSkipUpdate)
                return;

            SuspendLayout();
            if (e?.Text != "mru")
            {
                treCharacterList.Nodes.Clear();
                _lstCharacterCache.Clear();
                LoadCharacters();
                GC.Collect();
            }
            else
            {
                LoadCharacters(false);
            }
            ResumeLayout();
        }

        public void RefreshNodes()
        {
            foreach (TreeNode objTypeNode in treCharacterList.Nodes)
            {
                foreach (TreeNode objCharacterNode in objTypeNode.Nodes)
                {
                    string strFile = objCharacterNode.Tag.ToString();
                    if (_lstCharacterCache.TryGetValue(strFile, out CharacterCache objCache) && objCache != null)
                    {
                        objCharacterNode.Text = CalculatedName(objCache);
                        objCharacterNode.ToolTipText = objCache.FilePath.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>');
                        if (!string.IsNullOrEmpty(objCache.ErrorText))
                        {
                            objCharacterNode.ForeColor = Color.Red;
                            objCharacterNode.ToolTipText += Environment.NewLine + Environment.NewLine
                                                                                + LanguageManager.GetString("String_Error", GlobalOptions.Language) + LanguageManager.GetString("String_Colon", GlobalOptions.Language)
                                                                                + Environment.NewLine + objCache.ErrorText;
                        }
                        else
                            objCharacterNode.ForeColor = SystemColors.WindowText;
                    }
                    else
                    {
                        objCharacterNode.Text = Path.GetFileNameWithoutExtension(strFile) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + LanguageManager.GetString("String_Error", GlobalOptions.Language) + ')';
                        objCharacterNode.ToolTipText = strFile.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>') + Environment.NewLine + Environment.NewLine
                                                       + LanguageManager.GetString("String_Error", GlobalOptions.Language) + LanguageManager.GetString("String_Colon", GlobalOptions.Language)
                                                       + Environment.NewLine + LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language);
                        objCharacterNode.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void LoadCharacters(bool blnRefreshFavorites = true, bool blnRefreshRecents = true, bool blnRefreshWatch = true)
        {
            ReadOnlyObservableCollection<string> lstFavorites = new ReadOnlyObservableCollection<string>(GlobalOptions.FavoritedCharacters);
            bool blnAddFavouriteNode = false;
            TreeNode objFavouriteNode = null;
            TreeNode[] lstFavoritesNodes = null;
            if (blnRefreshFavorites)
            {
                objFavouriteNode = treCharacterList.FindNode("Favourite", false);
                if (objFavouriteNode == null)
                {
                    objFavouriteNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_FavouriteCharacters", GlobalOptions.Language)) {Tag = "Favourite"};
                    blnAddFavouriteNode = true;
                }

                lstFavoritesNodes = new TreeNode[lstFavorites.Count];
            }

            IList<string> lstRecents = new List<string>(GlobalOptions.MostRecentlyUsedCharacters);

            List<string> lstWatch = new List<string>();
            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                string[] objFiles = Directory.GetFiles(GlobalOptions.CharacterRosterPath, "*.chum5");
                for (int i = 0; i < objFiles.Length; ++i)
                {
                    string strFile = objFiles[i];
                    // Make sure we're not loading a character that was already loaded by the MRU list.
                    if (lstFavorites.Contains(strFile) ||
                        lstRecents.Contains(strFile))
                        continue;

                    lstWatch.Add(strFile);
                }
            }

            bool blnAddWatchNode = false;
            TreeNode objWatchNode = null;
            TreeNode[] lstWatchNodes = null;
            if (blnRefreshWatch)
            {
                objWatchNode = treCharacterList.FindNode("Watch", false);
                if (objWatchNode == null && lstWatch.Count > 0)
                {
                    objWatchNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_WatchFolder", GlobalOptions.Language)) {Tag = "Watch"};
                    blnAddWatchNode = true;
                }

                lstWatchNodes = new TreeNode[lstWatch.Count];
            }

            bool blnAddRecentNode = false;
            TreeNode objRecentNode = null;
            TreeNode[] lstRecentsNodes = null;
            if (blnRefreshRecents)
            {
                // Add any characters that are open to the displayed list so we can have more than 10 characters listed
                foreach (CharacterShared objCharacterForm in Program.MainForm.OpenCharacterForms)
                {
                    string strFile = objCharacterForm.CharacterObject.FileName;
                    // Make sure we're not loading a character that was already loaded by the MRU list.
                    if (lstFavorites.Contains(strFile) ||
                        lstRecents.Contains(strFile) ||
                        lstWatch.Contains(strFile))
                        continue;

                    lstRecents.Add(strFile);
                }

                foreach (string strFavorite in lstFavorites)
                    lstRecents.Remove(strFavorite);

                objRecentNode = treCharacterList.FindNode("Recent", false);
                if (objRecentNode == null && lstRecents.Count > 0)
                {
                    objRecentNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_RecentCharacters", GlobalOptions.Language)) { Tag = "Recent" };
                    blnAddRecentNode = true;
                }

                lstRecentsNodes = new TreeNode[lstRecents.Count];
            }
            Parallel.Invoke(
                () => {
                    if (objFavouriteNode != null && lstFavoritesNodes != null)
                    {
                        object lstFavoritesNodesLock = new object();

                        Parallel.For(0, lstFavorites.Count, i =>
                        {
                            string strFile = lstFavorites[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstFavoritesNodesLock)
                                lstFavoritesNodes[i] = objNode;
                        });

                        if (blnAddFavouriteNode)
                        {
                            for (int i = 0; i < lstFavoritesNodes.Length; i++)
                            {
                                TreeNode objNode = lstFavoritesNodes[i];
                                if (objNode != null)
                                    objFavouriteNode.Nodes.Add(objNode);
                            }
                        }
                    }
                },
                () => {
                    if (objRecentNode != null && lstRecentsNodes != null)
                    {
                        object lstRecentsNodesLock = new object();

                        Parallel.For(0, lstRecents.Count, i =>
                        {
                            string strFile = lstRecents[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstRecentsNodesLock)
                                lstRecentsNodes[i] = objNode;
                        });

                        if (blnAddRecentNode)
                        {
                            for (int i = 0; i < lstRecentsNodes.Length; i++)
                            {
                                TreeNode objNode = lstRecentsNodes[i];
                                if (objNode != null)
                                    objRecentNode.Nodes.Add(objNode);
                            }
                        }
                    }
                },
                () =>
                {
                    if (objWatchNode != null && lstWatchNodes != null)
                    {
                        object lstWatchNodesLock = new object();

                        Parallel.For(0, lstWatch.Count, i =>
                        {
                            string strFile = lstWatch[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstWatchNodesLock)
                                lstWatchNodes[i] = objNode;
                        });

                        if (blnAddWatchNode)
                        {
                            for (int i = 0; i < lstWatchNodes.Length; i++)
                            {
                                TreeNode objNode = lstWatchNodes[i];
                                if (objNode != null)
                                    objWatchNode.Nodes.Add(objNode);
                            }
                        }
                    }
                });
            if (objFavouriteNode != null)
            {
                if (blnAddFavouriteNode)
                {
                    treCharacterList.Nodes.Add(objFavouriteNode);
                    objFavouriteNode.Expand();
                }
                else
                {
                    objFavouriteNode.Nodes.Clear();
                    for (int i = 0; i < lstFavoritesNodes.Length; i++)
                    {
                        TreeNode objNode = lstFavoritesNodes[i];
                        if (objNode != null)
                            objFavouriteNode.Nodes.Add(objNode);
                    }
                }
            }

            if (objRecentNode != null)
            {
                if (blnAddRecentNode)
                {
                    treCharacterList.Nodes.Add(objRecentNode);
                    objRecentNode.Expand();
                }
                else
                {
                    objRecentNode.Nodes.Clear();
                    for (int i = 0; i < lstRecentsNodes.Length; i++)
                    {
                        TreeNode objNode = lstRecentsNodes[i];
                        if (objNode != null)
                            objRecentNode.Nodes.Add(objNode);
                    }
                }
            }
            if (objWatchNode != null)
            {
                if (blnAddWatchNode)
                {
                    treCharacterList.Nodes.Add(objWatchNode);
                    objWatchNode.Expand();
                }
                else
                {
                    objWatchNode.Nodes.Clear();
                    for (int i = 0; i < lstWatchNodes.Length; i++)
                    {
                        TreeNode objNode = lstWatchNodes[i];
                        if (objNode != null)
                            objWatchNode.Nodes.Add(objNode);
                    }
                }
            }
            treCharacterList.ExpandAll();
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        private TreeNode CacheCharacter(string strFile)
        {
            CharacterCache objCache = new CharacterCache();
            string strErrorText = string.Empty;
            XPathNavigator xmlSourceNode;
            if (!File.Exists(strFile))
            {
                xmlSourceNode = null;
                strErrorText = LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language);
            }
            else
            {
                // If we run into any problems loading the character cache, fail out early.
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(objStreamReader);
                        xmlSourceNode = xmlDoc.CreateNavigator().SelectSingleNode("/character");
                    }
                }
                catch (Exception ex)
                {
                    xmlSourceNode = null;
                    strErrorText = ex.ToString();
                }
            }

            if (xmlSourceNode != null)
            {
                objCache.Description = xmlSourceNode.SelectSingleNode("description")?.Value;
                objCache.BuildMethod = xmlSourceNode.SelectSingleNode("buildmethod")?.Value;
                objCache.Background = xmlSourceNode.SelectSingleNode("background")?.Value;
                objCache.CharacterNotes = xmlSourceNode.SelectSingleNode("notes")?.Value;
                objCache.GameNotes = xmlSourceNode.SelectSingleNode("gamenotes")?.Value;
                objCache.Concept = xmlSourceNode.SelectSingleNode("concept")?.Value;
                objCache.Karma = xmlSourceNode.SelectSingleNode("totalkarma")?.Value;
                objCache.Metatype = xmlSourceNode.SelectSingleNode("metatype")?.Value;
                objCache.Metavariant = xmlSourceNode.SelectSingleNode("metavariant")?.Value;
                objCache.PlayerName = xmlSourceNode.SelectSingleNode("playername")?.Value;
                objCache.CharacterName = xmlSourceNode.SelectSingleNode("name")?.Value;
                objCache.CharacterAlias = xmlSourceNode.SelectSingleNode("alias")?.Value;
                objCache.Created = xmlSourceNode.SelectSingleNode("created")?.Value == bool.TrueString;
                objCache.Essence = xmlSourceNode.SelectSingleNode("totaless")?.Value;
                string strSettings = xmlSourceNode.SelectSingleNode("settings")?.Value ?? string.Empty;
                objCache.SettingsFile = !File.Exists(Path.Combine(Application.StartupPath, "settings", strSettings)) ? LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language) : strSettings;
                string strMugshotBase64 = xmlSourceNode.SelectSingleNode("mugshot")?.Value;
                if (!string.IsNullOrEmpty(strMugshotBase64))
                {
                    objCache.Mugshot = strMugshotBase64.ToImage();
                }
                else
                {
                    XPathNavigator xmlMainMugshotIndex = xmlSourceNode.SelectSingleNode("mainmugshotindex");
                    if (xmlMainMugshotIndex != null && int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) && intMainMugshotIndex >= 0)
                    {
                        XPathNodeIterator xmlMugshotList = xmlSourceNode.Select("mugshots/mugshot");
                        if (xmlMugshotList.Count > intMainMugshotIndex)
                        {
                            int intIndex = 0;
                            foreach (XPathNavigator xmlMugshot in xmlMugshotList)
                            {
                                if (intMainMugshotIndex == intIndex)
                                {
                                    objCache.Mugshot = xmlMugshot.Value.ToImage();
                                    break;
                                }

                                intIndex += 1;
                            }
                        }
                    }
                }
            }
            else
            {
                objCache.ErrorText = strErrorText;
            }

            objCache.FilePath = strFile;
            objCache.FileName = strFile.Substring(strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            if (!_lstCharacterCache.TryAdd(strFile, objCache))
                _lstCharacterCache[strFile] = objCache;

            TreeNode objNode = new TreeNode
            {
                ContextMenuStrip = cmsRoster,
                Text = CalculatedName(objCache),
                ToolTipText = objCache.FilePath.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>'),
                Tag = strFile
            };
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                objNode.ForeColor = Color.Red;
                objNode.ToolTipText += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Error", GlobalOptions.Language)
                                       + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + Environment.NewLine + objCache.ErrorText;
            }

            return objNode;
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="objCache">Cache from which to generate name.</param>
        /// <param name="blnAddMarkerIfOpen">Whether to add an asterisk to the beginning of the name if the character is open.</param>
        /// <returns></returns>
        private static string CalculatedName(CharacterCache objCache, bool blnAddMarkerIfOpen = true)
        {
            string strReturn;
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                strReturn = Path.GetFileNameWithoutExtension(objCache.FileName) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + LanguageManager.GetString("String_Error", GlobalOptions.Language) + ')';
            }
            else
            {
                strReturn = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(strReturn))
                {
                    strReturn = objCache.CharacterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language);
                }

                string strBuildMethod = LanguageManager.GetString("String_" + objCache.BuildMethod, GlobalOptions.Language, false);
                if (string.IsNullOrEmpty(strBuildMethod))
                    strBuildMethod = LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strCreated = LanguageManager.GetString(objCache.Created ? "Title_CareerMode" : "Title_CreateMode", GlobalOptions.Language);
                strReturn += $" ({strBuildMethod} - {strCreated})";
            }
            if (blnAddMarkerIfOpen && Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject.FileName == objCache.FilePath))
                strReturn = "* " + strReturn;
            return strReturn;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        private void UpdateCharacter(CharacterCache objCache)
        {
            if (objCache != null)
            {
                string strUnknown = LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strNone = LanguageManager.GetString("String_None", GlobalOptions.Language);
                txtCharacterBio.Text = objCache.Description;
                txtCharacterBackground.Text = objCache.Background;
                txtCharacterNotes.Text = objCache.CharacterNotes;
                txtGameNotes.Text = objCache.GameNotes;
                txtCharacterConcept.Text = objCache.Concept;
                lblCareerKarma.Text = objCache.Karma;
                if (string.IsNullOrEmpty(lblCareerKarma.Text) || lblCareerKarma.Text == "0")
                    lblCareerKarma.Text = strNone;
                lblPlayerName.Text = objCache.PlayerName;
                if (string.IsNullOrEmpty(lblPlayerName.Text))
                    lblPlayerName.Text = strUnknown;
                lblCharacterName.Text = objCache.CharacterName;
                if (string.IsNullOrEmpty(lblCharacterName.Text))
                    lblCharacterName.Text = strUnknown;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(lblCharacterAlias.Text))
                    lblCharacterAlias.Text = strUnknown;
                lblEssence.Text = objCache.Essence;
                if (string.IsNullOrEmpty(lblEssence.Text))
                    lblEssence.Text = strUnknown;
                lblFilePath.Text = objCache.FileName;
                if (string.IsNullOrEmpty(lblFilePath.Text))
                    lblFilePath.Text = LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language);
                lblSettings.Text = objCache.SettingsFile;
                if (string.IsNullOrEmpty(lblSettings.Text))
                    lblSettings.Text = strUnknown;
                lblFilePath.SetToolTip(objCache.FilePath.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>'));
                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml");
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + "]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml");
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + "]");
                }

                string strMetatype = objMetatypeNode?["translate"]?.InnerText ?? objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + "]");

                    strMetatype += LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + (objMetatypeNode?["translate"]?.InnerText ?? objCache.Metavariant) + ')';
                }
                lblMetatype.Text = strMetatype;
                tabCharacterText.Visible = true;
            }
            else
            {
                tabCharacterText.Visible = false;
                txtCharacterBio.Text = string.Empty;
                txtCharacterBackground.Text = string.Empty;
                txtCharacterNotes.Text = string.Empty;
                txtGameNotes.Text = string.Empty;
                txtCharacterConcept.Text = string.Empty;
                lblCareerKarma.Text = string.Empty;
                lblMetatype.Text = string.Empty;
                lblPlayerName.Text = string.Empty;
                lblCharacterName.Text = string.Empty;
                lblCharacterAlias.Text = string.Empty;
                lblEssence.Text = string.Empty;
                lblFilePath.Text = string.Empty;
                lblFilePath.SetToolTip(string.Empty);
                lblSettings.Text = string.Empty;
                picMugshot.Image = null;
            }
            lblCareerKarmaLabel.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text);
            lblMetatypeLabel.Visible = !string.IsNullOrEmpty(lblMetatype.Text);
            lblPlayerNameLabel.Visible = !string.IsNullOrEmpty(lblPlayerName.Text);
            lblCharacterNameLabel.Visible = !string.IsNullOrEmpty(lblCharacterName.Text);
            lblCharacterAliasLabel.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text);
            lblEssenceLabel.Visible = !string.IsNullOrEmpty(lblEssence.Text);
            lblFilePathLabel.Visible = !string.IsNullOrEmpty(lblFilePath.Text);
            lblSettingsLabel.Visible = !string.IsNullOrEmpty(lblSettings.Text);
            ProcessMugshotSizeMode();
        }

        #region Form Methods
        
        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CharacterCache objCache = null;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                _lstCharacterCache.TryGetValue(objSelectedNode.Tag.ToString(), out objCache);
            }
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                string strFile = objSelectedNode.Tag.ToString();
                if (!string.IsNullOrEmpty(strFile) && _lstCharacterCache.TryGetValue(strFile, out CharacterCache objCache) && string.IsNullOrEmpty(objCache.ErrorText))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    Cursor = Cursors.WaitCursor;
                    if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                    {
                        objOpenCharacter = Program.MainForm.LoadCharacter(strFile);
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                        objSelectedNode.Text = CalculatedName(objCache);
                    }
                    Cursor = Cursors.Default;
                }
            }
        }
        private void treCharacterList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                TreeNode t = treCharacterList.SelectedNode;

                if (t != null && _lstCharacterCache.TryGetValue(t.Tag.ToString(), out CharacterCache objCache) && objCache != null)
                {
                    switch (t.Parent.Tag.ToString())
                    {
                        case "Recent":
                            GlobalOptions.MostRecentlyUsedCharacters.Remove(objCache.FilePath);
                            break;
                        case "Favourite":
                            GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                            break;
                    }
                }
            }
        }

        private void treCharacterList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treCharacterList_DragOver(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView treSenderView))
                return;
            Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = treSenderView.GetNodeAt(pt);
            if (objNode != null)
            {
                if (objNode.Parent != null)
                    objNode = objNode.Parent;
                if (objNode.Tag.ToString() != "Watch")
                {
                    objNode.BackColor = SystemColors.ControlDark;
                }
            }

            // Clear the background colour for all other Nodes.
            treCharacterList.ClearNodeBackground(objNode);
        }
        private void treCharacterList_DragDrop(object sender, DragEventArgs e)
        {
            // Do not allow the root element to be moved.
            if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 || treCharacterList.SelectedNode.Parent.Tag.ToString() == "Watch")
                return;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                if (!(sender is TreeView treSenderView))
                    return;
                Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                if (nodDestinationNode.Level > 0)
                    nodDestinationNode = nodDestinationNode.Parent;
                string strDestinationNode = nodDestinationNode.Tag.ToString();
                if (strDestinationNode != "Watch")
                {
                    if (!(e.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                        return;

                    if (nodNewNode.Level == 0 || nodNewNode.Parent == nodDestinationNode)
                        return;
                    if (_lstCharacterCache.TryGetValue(nodNewNode.Tag.ToString(), out CharacterCache objCache) && objCache != null)
                    {
                        switch (strDestinationNode)
                        {
                            case "Recent":
                                GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                                GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                                break;
                            case "Favourite":
                                GlobalOptions.FavoritedCharacters.Add(objCache.FilePath);
                                break;
                        }
                    }
                }
            }
        }

        private void treCharacterList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            ProcessMugshotSizeMode();
        }

        private void ProcessMugshotSizeMode()
        {
            picMugshot.SizeMode = picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width
                ? PictureBoxSizeMode.CenterImage
                : PictureBoxSizeMode.Zoom;
        }
        #endregion
        #region Classes
        /// <summary>
        /// Caches a subset of a full character's properties for loading purposes.
        /// </summary>
        private sealed class CharacterCache
        {
            internal string FilePath { get; set; }
            internal string FileName { get; set; }
            internal string ErrorText { get; set; }
            internal string Description { get; set; }
            internal string Background { get; set; }
            internal string GameNotes { get; set; }
            internal string CharacterNotes { get; set; }
            internal string Concept { get; set; }
            internal string Karma { get; set; }
            internal string Metatype { get; set; }
            internal string Metavariant { get; set; }
            internal string PlayerName { get; set; }
            internal string CharacterName { get; set; }
            internal string CharacterAlias { get; set; }
            internal string BuildMethod { get; set; }
            internal string Essence { get; set; }
            internal Image Mugshot { get; set; }
            internal bool Created { get; set; }
            public string SettingsFile { get; set; }
        }
        #endregion

        private void tsDelete_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t != null && _lstCharacterCache.TryGetValue(t.Tag.ToString(), out CharacterCache objCache) && objCache != null)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.MostRecentlyUsedCharacters.Remove(objCache.FilePath);
                        break;
                    case "Favourite":
                        GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                        break;
                }
            }
        }

        private void tsSort_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t != null)
            {
                string strSelectedTag = t.Tag.ToString();
                if (_lstCharacterCache.TryGetValue(t.Tag.ToString(), out CharacterCache objCache) && objCache != null)
                {
                    switch (t.Parent.Tag.ToString())
                    {
                        case "Recent":
                        {
                            _blnSkipUpdate = true;
                            SuspendLayout();

                                List<Tuple<string, string>> lstSorted = new List<Tuple<string, string>>();
                            for (int i = 0; i < GlobalOptions.MostRecentlyUsedCharacters.Count; ++i)
                            {
                                string strLoopFile = GlobalOptions.MostRecentlyUsedCharacters[i];
                                if (_lstCharacterCache.TryGetValue(strLoopFile, out CharacterCache objLoopCache) && objLoopCache != null)
                                    lstSorted.Add(new Tuple<string, string>(CalculatedName(objLoopCache, false), strLoopFile));
                                else
                                    lstSorted.Add(new Tuple<string, string>(Path.GetFileNameWithoutExtension(strLoopFile), strLoopFile));
                            }

                            lstSorted.Sort();
                            for (int i = 0; i < lstSorted.Count; ++i)
                                GlobalOptions.MostRecentlyUsedCharacters.Move(GlobalOptions.MostRecentlyUsedCharacters.IndexOf(lstSorted[i].Item2), i);

                            LoadCharacters(false, true, false);
                            ResumeLayout();
                            treCharacterList.SelectedNode = treCharacterList.FindNode(strSelectedTag);
                            break;
                        }
                        case "Favourite":
                        {
                            _blnSkipUpdate = true;
                            SuspendLayout();

                                List<Tuple<string, string>> lstSorted = new List<Tuple<string, string>>();
                            for (int i = 0; i < GlobalOptions.FavoritedCharacters.Count; ++i)
                            {
                                string strLoopFile = GlobalOptions.FavoritedCharacters[i];
                                if (_lstCharacterCache.TryGetValue(strLoopFile, out CharacterCache objLoopCache) && objLoopCache != null)
                                    lstSorted.Add(new Tuple<string, string>(CalculatedName(objLoopCache, false), strLoopFile));
                                else
                                    lstSorted.Add(new Tuple<string, string>(Path.GetFileNameWithoutExtension(strLoopFile), strLoopFile));
                            }

                            lstSorted.Sort();
                            for (int i = 0; i < lstSorted.Count; ++i)
                                GlobalOptions.FavoritedCharacters.Move(GlobalOptions.FavoritedCharacters.IndexOf(lstSorted[i].Item2), i);

                            _blnSkipUpdate = false;

                            LoadCharacters(true, false, false);
                            ResumeLayout();
                            treCharacterList.SelectedNode = treCharacterList.FindNode(strSelectedTag);
                            break;
                        }
                    }
                }
            }
        }

        private void tsToggleFav_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t != null && _lstCharacterCache.TryGetValue(t.Tag.ToString(), out CharacterCache objCache) && objCache != null)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.FavoritedCharacters.Add(objCache.FilePath);
                        break;
                    case "Favourite":
                        GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                        GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                        break;
                    case "Watch":
                        GlobalOptions.FavoritedCharacters.Add(objCache.FilePath);
                        break;
                }
                treCharacterList.SelectedNode = t;
            }
        }

        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            // Generic event for all TreeViews to allow right-clicking to select a TreeNode so the proper ContextMenu is shown.
            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //{
            TreeView objTree = (TreeView)sender;
            objTree.SelectedNode = objTree.HitTest(e.X, e.Y).Node;
            //}
            if (ModifierKeys == Keys.Control)
            {
                if (!objTree.SelectedNode.IsExpanded)
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.ExpandAll();
                    }
                }
                else
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.Collapse();
                    }
                }
            }
        }

        private void tsCloseOpenCharacter_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            string strFile = objSelectedNode.Tag.ToString();
            if (string.IsNullOrEmpty(strFile))
                return;
            Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
            Cursor = Cursors.WaitCursor;
            if (objOpenCharacter != null)
            {
                Program.MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objOpenCharacter)?.Close();
            }
            Cursor = Cursors.Default;
        }
    }
}
