using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectDependencies : Form
    {
        private readonly CharacterOptions _objCharacterOptions;
        private CustomDataDirectoryInfo _objDirectoryInfoToModify;
        private readonly List<Tuple<object, bool>> _lstCharacterCustomDataDirectoryInfos;
        private readonly List<Tuple<int, string, string>> _lstFormDependencies;
        private readonly List<Tuple<int, string, string>> _lstFormExclusivities;


        public frmSelectDependencies(CharacterOptions objCharacterOptions, CustomDataDirectoryInfo objDirectoryInfoToModify, List<Tuple<object, bool>> lstCharacterCustomDataDirectoryInfos)
        {
            _objCharacterOptions = objCharacterOptions;
            _objDirectoryInfoToModify = objDirectoryInfoToModify;
            _lstCharacterCustomDataDirectoryInfos = lstCharacterCustomDataDirectoryInfos;
            InitializeComponent();
        }

        private void PopulateTreCustomDataDirectories()
        {
            object objOldSelected = treCustomDataDirectories.SelectedNode?.Tag;
            treCustomDataDirectories.BeginUpdate();
            if (_lstCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (Tuple<object, bool> objCustomDataDirectory in _lstCharacterCustomDataDirectoryInfos)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Tag = objCustomDataDirectory.Item1,
                    };
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for (int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataDirectories.Nodes[i];
                    Tuple<object, bool> objCustomDataDirectory = _lstCharacterCustomDataDirectoryInfos[i];
                    if (objCustomDataDirectory.Item1 != objNode.Tag)
                        objNode.Tag = objCustomDataDirectory.Item1;
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                }
            }

            if (objOldSelected != null)
                treCustomDataDirectories.SelectedNode = treCustomDataDirectories.FindNodeByTag(objOldSelected);
            treCustomDataDirectories.EndUpdate();
        }

        private void PopulateTreDependencies()
        {
            var objOldSelected = treDependencies.SelectedNode?.Tag;
            treDependencies.BeginUpdate();

            foreach (var dependency in _objDirectoryInfoToModify.DependenciesList)
            {
                string nameAndVersion = string.Format(dependency.Item2 + LanguageManager.GetString("AddVersion"), dependency.Item3);
                TreeNode newNode = new TreeNode
                {
                    Tag = dependency,
                    Text = nameAndVersion
                };
                treDependencies.Nodes.Add(newNode);
            }

            if (objOldSelected != null)
                treDependencies.SelectedNode = treDependencies.FindNodeByTag(objOldSelected);
            treDependencies.EndUpdate();
        }

        private void PopulateTreExclusivities()
        {
            var objOldSelected = treExclusivities.SelectedNode?.Tag;
            treExclusivities.BeginUpdate();

            foreach (var exclusivity in _objDirectoryInfoToModify.ExclusivitiesList)
            {
                string nameAndVersion = string.Format(exclusivity.Item2 + LanguageManager.GetString("AddVersion"), exclusivity.Item3);
                TreeNode newNode = new TreeNode()
                {
                    Tag = exclusivity,
                    Text = nameAndVersion
                };
                treExclusivities.Nodes.Add(newNode);
            }

            if (objOldSelected != null)
                treExclusivities.SelectedNode = treExclusivities.FindNodeByTag(objOldSelected);
            treExclusivities.EndUpdate();
        }

        private void RefreshRtboxDescription()
        {
            
        }

        private void AcceptForm()
        {
            foreach (TreeNode dependencyNode in treDependencies.Nodes)
            {
                if (dependencyNode.Tag is Tuple<int, string, string> dependency)
                    _lstFormDependencies.Add(dependency);
            }
            foreach (TreeNode exclusivityNode in treExclusivities.Nodes)
            {
                if (exclusivityNode.Tag is Tuple<int, string, string> exclusivity)
                    _lstFormExclusivities.Add(exclusivity);
            }
            foreach (TreeNode authorNode in treAuthors.Nodes)
            {
                if (authorNode.Tag is KeyValuePair<string, bool> author)
                    FrmAuthorDictionary.Add(author);
            }


            DialogResult = DialogResult.OK;
        }


        #region ControlEvents
        private void frmSelectDependencies_Load(object sender, EventArgs e)
        {
            PopulateTreCustomDataDirectories();
            PopulateTreDependencies();
            PopulateTreExclusivities();
            Text = _objDirectoryInfoToModify.Name;
        }

        private void cmdAddDependency_Click(object sender, EventArgs e)
        {
            if (treCustomDataDirectories.SelectedNode.Tag is CustomDataDirectoryInfo infoToAdd)
            {
                treDependencies.BeginUpdate();

                Tuple<int, string, string> newExclusivity = new Tuple<int, string, string>(infoToAdd.Hash, infoToAdd.Name, infoToAdd.Version);

                string nameAndVersion = string.Format(newExclusivity.Item2 + LanguageManager.GetString("AddVersion"), newExclusivity.Item3);
                TreeNode newNode = new TreeNode
                {
                    Tag = newExclusivity,
                    Text = nameAndVersion
                };

                treDependencies.Nodes.Add(newNode);
                treDependencies.EndUpdate();
            }
        }

        private void cmdRemoveDependency_Click(object sender, EventArgs e)
        {
            treDependencies.SelectedNode.Remove();
        }

        private void cmdAddExclusivity_Click(object sender, EventArgs e)
        {
            if (treCustomDataDirectories.SelectedNode.Tag is CustomDataDirectoryInfo infoToAdd)
            {
                treExclusivities.BeginUpdate();

                Tuple<int, string, string> newExclusivity = new Tuple<int, string, string>(infoToAdd.Hash, infoToAdd.Name, infoToAdd.Version);

                string nameAndVersion = string.Format(newExclusivity.Item2 + " (" + newExclusivity.Item3 + ")");
                TreeNode newNode = new TreeNode
                {
                    Tag = newExclusivity,
                    Text = nameAndVersion
                };

                treExclusivities.Nodes.Add(newNode);
                treExclusivities.EndUpdate();
            }
        }

        private void cmdRemoveExclusivity_Click(object sender, EventArgs e)
        {
            treExclusivities.SelectedNode.Remove();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Properties

        /// <summary>
        /// A Dictionary, that uses the author name as key and provides a bool if he is the main author
        /// </summary>
        public IDictionary<string, bool> FrmAuthorDictionary { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// A Dictionary containing all Descriptions, which uses the language code as key
        /// </summary>
        public IDictionary<string, string> FrmDescriptionDictionary { get; } = new Dictionary<string, string>();

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(int Hash, str name, str version).
        /// </summary>
        public IReadOnlyList<Tuple<int, string, string>> DependenciesList => _lstFormDependencies;

        /// <summary>
        /// A list of all exclusivities each formatted as Tuple(int Hash, str name, str version).
        /// </summary>
        public IReadOnlyList<Tuple<int, string, string>> ExclusivitiesList => _lstFormExclusivities;

        #endregion

    }
}
