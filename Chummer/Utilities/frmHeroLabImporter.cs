using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmHeroLabImporter : Form
    {
        List<HeroLabCharacterCache> _lstCharacterCache = new List<HeroLabCharacterCache>();
        private object _lstCharacterCacheLock = new object();
        Dictionary<string, Bitmap> _dicImages = new Dictionary<string, Bitmap>();

        public frmHeroLabImporter()
        {
            InitializeComponent();
            MoveControls();
        }

        private void cmdSelectFile_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to possess.
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Hero Lab Files (*.por)|*.por|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                string strSelectedFile = openFileDialog.FileName;
                TreeNode objNode = CacheCharacters(strSelectedFile);
                if (objNode != null)
                {
                    treCharacterList.Nodes.Clear();
                    treCharacterList.Nodes.Add(objNode);
                    if (objNode.Nodes.Count > 0)
                        treCharacterList.SelectedNode = objNode.Nodes[0];
                    else
                        treCharacterList.SelectedNode = objNode;
                }
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="objParentNode"></param>
        private TreeNode CacheCharacters(string strFile)
        {
            if (!File.Exists(strFile))
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + "\n\n" + strFile);
                return null;
            }

            List<XmlDocument> lstCharacterXmlStatblocks = new List<XmlDocument>();
            try
            {
                using (ZipArchive zipArchive = ZipFile.Open(strFile, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        string strEntryFullName = entry.FullName;
                        if (strEntryFullName.EndsWith(".xml") && strEntryFullName.StartsWith("statblocks_xml"))
                        {
                            XmlDocument xmlSourceDoc = new XmlDocument();
                            // If we run into any problems loading the character cache, fail out early.
                            try
                            {
                                using (StreamReader sr = new StreamReader(entry.Open(), true))
                                {
                                    xmlSourceDoc.Load(sr);
                                    lstCharacterXmlStatblocks.Add(xmlSourceDoc);
                                }
                            }
                            // If we run into any problems loading the character cache, fail out early.
                            catch (IOException)
                            {
                                continue;
                            }
                            catch (XmlException)
                            {
                                continue;
                            }
                        }
                        else if (strEntryFullName.StartsWith("images") && strEntryFullName.Contains('.'))
                        {
                            string strKey = Path.GetFileName(strEntryFullName);
                            Bitmap imgMugshot = (new Bitmap(entry.Open(), true)).ConvertPixelFormat(PixelFormat.Format32bppPArgb);
                            if (_dicImages.ContainsKey(strKey))
                                _dicImages[strKey] = imgMugshot;
                            else
                                _dicImages.Add(strKey, imgMugshot);
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + "\n\n" + strFile);
                return null;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed", GlobalOptions.Language) + "\n\n" + strFile);
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                return null;
            }

            string strFileText = strFile.CheapReplace(Application.StartupPath, () => "<" + Application.ProductName + ">");
            TreeNode nodRootNode = new TreeNode
            {
                Text = strFileText,
                ToolTipText = strFileText
            };

            XmlDocument xmlMetatypesDocument = XmlManager.Load("metatypes.xml");
            foreach (XmlDocument xmlCharacterDocument in lstCharacterXmlStatblocks)
            {
                XmlNode xmlBaseCharacterNode = xmlCharacterDocument.SelectSingleNode("/document/public/character");
                if (xmlBaseCharacterNode != null)
                {
                    HeroLabCharacterCache objCache = new HeroLabCharacterCache
                    {
                        PlayerName = xmlBaseCharacterNode.Attributes["playername"]?.InnerText
                    };
                    string strNameString = xmlBaseCharacterNode.Attributes["name"]?.InnerText;
                    objCache.CharacterId = strNameString;
                    if (!string.IsNullOrEmpty(strNameString))
                    {
                        int intAsIndex = strNameString.IndexOf(" as ");
                        if (intAsIndex != -1)
                        {
                            objCache.CharacterName = strNameString.Substring(0, intAsIndex);
                            objCache.CharacterAlias = strNameString.Substring(intAsIndex).TrimStart(" as ").Trim('\'');
                        }
                        else
                        {
                            objCache.CharacterName = strNameString;
                        }
                    }

                    string strRaceString = xmlBaseCharacterNode.SelectSingleNode("race/@name")?.InnerText;
                    if (strRaceString == "Metasapient")
                        strRaceString = "A.I.";
                    if (!string.IsNullOrEmpty(strRaceString))
                    {
                        foreach (XmlNode xmlMetatype in xmlMetatypesDocument.SelectNodes("/chummer/metatypes/metatype"))
                        {
                            string strMetatypeName = xmlMetatype["name"].InnerText;
                            if (strMetatypeName == strRaceString)
                            {
                                objCache.Metatype = strMetatypeName;
                                objCache.Metavariant = "None";
                                break;
                            }
                            foreach (XmlNode xmlMetavariant in xmlMetatype.SelectNodes("metavariants/metavariant"))
                            {
                                string strMetavariantName = xmlMetavariant["name"].InnerText;
                                if (strMetavariantName == strRaceString)
                                {
                                    objCache.Metatype = strMetatypeName;
                                    objCache.Metavariant = strMetavariantName;
                                    break;
                                }
                            }
                        }
                    }

                    objCache.Description = xmlBaseCharacterNode.SelectSingleNode("personal/description")?.InnerText;
                    objCache.Karma = xmlBaseCharacterNode.SelectSingleNode("karma/@total")?.InnerText ?? "0";
                    objCache.Essence = xmlBaseCharacterNode.SelectSingleNode("attributes/attribute[@name = \"Essence\"]/@text")?.InnerText;
                    objCache.BuildMethod = xmlBaseCharacterNode.SelectSingleNode("creation/bp/@total")?.InnerText == "25" ?
                        CharacterBuildMethod.Priority.ToString() :
                        CharacterBuildMethod.Karma.ToString();
                    
                    objCache.Created = objCache.Karma != "0";
                    if (!objCache.Created)
                    {
                        XmlNodeList xmlJournalEntries = xmlBaseCharacterNode.SelectNodes("journals/journal");
                        if (xmlJournalEntries.Count > 1)
                        {
                            objCache.Created = true;
                        }
                        else if (xmlJournalEntries.Count == 1 && xmlJournalEntries[0].Attributes["name"]?.InnerText != "Title")
                        {
                            objCache.Created = true;
                        }
                    }
                    string strImageString = xmlBaseCharacterNode.SelectSingleNode("images/image/@filename")?.InnerText;
                    if (!string.IsNullOrEmpty(strImageString) && _dicImages.TryGetValue(strImageString, out Bitmap objTemp))
                    {
                        objCache.Mugshot = objTemp;
                    }

                    objCache.FilePath = strFile;
                    TreeNode objNode = new TreeNode
                    {
                        Text = CalculatedName(objCache),
                        ToolTipText = strFile.CheapReplace(Application.StartupPath, () => "<" + Application.ProductName + ">")
                    };
                    nodRootNode.Nodes.Add(objNode);

                    lock (_lstCharacterCacheLock)
                    {
                        _lstCharacterCache.Add(objCache);
                        objNode.Tag = _lstCharacterCache.IndexOf(objCache);
                    }
                }
            }
            nodRootNode.Expand();
            return nodRootNode;
        }

        #region Classes
        /// <summary>
        /// Caches a subset of a full character's properties for loading purposes. 
        /// </summary>
        private sealed class HeroLabCharacterCache
        {
            internal string FilePath { get; set; }
            internal string Description { get; set; }
            internal string Karma { get; set; }
            internal string Metatype { get; set; }
            internal string Metavariant { get; set; }
            internal string PlayerName { get; set; }
            internal string CharacterId { get; set; }
            internal string CharacterName { get; set; }
            internal string CharacterAlias { get; set; }
            internal string BuildMethod { get; set; }
            internal string Essence { get; set; }
            internal Image Mugshot { get; set; }
            internal bool Created { get; set; }
        }
        #endregion

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object. 
        /// </summary>
        /// <param name="objCache"></param>
        /// <returns></returns>
        private static string CalculatedName(HeroLabCharacterCache objCache)
        {
            string strName = objCache.CharacterAlias;
            if (string.IsNullOrEmpty(strName))
            {
                strName = objCache.CharacterName;
                if (string.IsNullOrEmpty(strName))
                    strName = LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language);
            }
            string strBuildMethod = LanguageManager.GetString("String_" + objCache.BuildMethod, GlobalOptions.Language, false);
            if (string.IsNullOrEmpty(strBuildMethod))
                strBuildMethod = "Unknown build method";
            string strCreated = LanguageManager.GetString(objCache.Created ? "Title_CareerMode" : "Title_CreateMode", GlobalOptions.Language);
            string strReturn = $"{strName} ({strBuildMethod} - {strCreated})";
            return strReturn;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        private void UpdateCharacter(HeroLabCharacterCache objCache)
        {
            if (objCache != null)
            {
                txtCharacterBio.Text = objCache.Description;
                lblCareerKarma.Text = objCache.Karma;
                lblPlayerName.Text = objCache.PlayerName;
                lblCharacterName.Text = objCache.CharacterName;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                lblEssence.Text = objCache.Essence;
                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml");
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCache.Metatype + "\"]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml");
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCache.Metatype + "\"]");
                }

                string strMetatype = objMetatypeNode?["translate"]?.InnerText ?? objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + objCache.Metavariant + "\"]");

                    strMetatype += " (" + (objMetatypeNode?["translate"]?.InnerText ?? objCache.Metavariant) + ')';
                }
                lblMetatype.Text = strMetatype;
                cmdImport.Enabled = true;
            }
            else
            {
                txtCharacterBio.Text = string.Empty;
                lblCareerKarma.Text = string.Empty;
                lblMetatype.Text = string.Empty;
                lblPlayerName.Text = string.Empty;
                lblCharacterName.Text = string.Empty;
                lblCharacterAlias.Text = string.Empty;
                lblEssence.Text = string.Empty;
                picMugshot.Image = null;
                cmdImport.Enabled = false;
            }
            picMugshot_SizeChanged(null, EventArgs.Empty);
        }

        #region Form Methods

        private void MoveControls()
        {
            lblPlayerNameLabel.Left = tabCharacterText.Left;
            lblCharacterNameLabel.Left = tabCharacterText.Left;
            lblCareerKarmaLabel.Left = tabCharacterText.Left;
            lblMetatypeLabel.Left = tabCharacterText.Left;
            lblCharacterAliasLabel.Left = tabCharacterText.Left;
            lblEssenceLabel.Left = tabCharacterText.Left;

            int intWidth = lblPlayerNameLabel.Right;
            if (lblCareerKarmaLabel.Right > intWidth)
            {
                intWidth = lblCareerKarmaLabel.Right;
            }
            if (lblCareerKarmaLabel.Right > intWidth)
            {
                intWidth = lblCareerKarmaLabel.Right;
            }
            if (lblMetatypeLabel.Right > intWidth)
            {
                intWidth = lblMetatypeLabel.Right;
            }
            if (lblCharacterAliasLabel.Right > intWidth)
            {
                intWidth = lblCharacterAliasLabel.Right;
            }
            if (lblEssenceLabel.Right > intWidth)
            {
                intWidth = lblEssenceLabel.Right;
            }
            intWidth += 12;

            lblEssence.Left = intWidth;
            lblPlayerName.Left = intWidth;
            lblCareerKarma.Left = intWidth;
            lblCharacterAlias.Left = intWidth;
            lblMetatype.Left = intWidth;
            lblCharacterName.Left = intWidth;
        }

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            HeroLabCharacterCache objCache = null;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                    objCache = _lstCharacterCache[intIndex];
            }
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            DoImport();
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            DoImport();
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
        }
        #endregion

        private void DoImport()
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                {
                    string strFile = _lstCharacterCache[intIndex]?.FilePath;
                    if (!string.IsNullOrEmpty(strFile))
                    {
                        ;
                    }
                }
            }
        }
    }
}
