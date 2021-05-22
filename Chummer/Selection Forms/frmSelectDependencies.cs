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

        public frmSelectDependencies(CharacterOptions objCharacterOptions, CustomDataDirectoryInfo objDirectoryInfoToModify, List<Tuple<object, bool>> lstCharacterCustomDataDirectoryInfos)
        {
            _objCharacterOptions = objCharacterOptions;
            _objDirectoryInfoToModify = objDirectoryInfoToModify;
            _lstCharacterCustomDataDirectoryInfos = lstCharacterCustomDataDirectoryInfos;
            InitializeComponent();
        }

        private void PopulateTreCustomDataDirectories()
        {
            object objOldSelected = treCustomDataFiles.SelectedNode?.Tag;
            treCustomDataFiles.BeginUpdate();
            if (_lstCharacterCustomDataDirectoryInfos.Count != treCustomDataFiles.Nodes.Count)
            {
                treCustomDataFiles.Nodes.Clear();

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
                    treCustomDataFiles.Nodes.Add(objNode);
                }
            }
            else
            {
                for (int i = 0; i < treCustomDataFiles.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataFiles.Nodes[i];
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
                treCustomDataFiles.SelectedNode = treCustomDataFiles.FindNodeByTag(objOldSelected);
            treCustomDataFiles.EndUpdate();
        }

        private void PopulateTreDependencies()
        {
            var objOldSelected = treDependencies.SelectedNode?.Tag;
            treDependencies.BeginUpdate();

            foreach (var dependency in _objDirectoryInfoToModify.DependenciesList)
            {
                string nameAndVersion = string.Format(dependency.Item2 + " (" + dependency.Item3 + ")");
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
                string nameAndVersion = string.Format(exclusivity.Item2 + " (" + exclusivity.Item3 + ")");
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

        private void frmSelectDependencies_Load(object sender, EventArgs e)
        {
            PopulateTreCustomDataDirectories();
            PopulateTreDependencies();
            PopulateTreExclusivities();
            Text = _objDirectoryInfoToModify.Name;
        }

        private void cmdAddDependency_Click(object sender, EventArgs e)
        {
            if (treCustomDataFiles.SelectedNode.Tag is CustomDataDirectoryInfo infoToAdd)
            {
                treDependencies.BeginUpdate();

                Tuple<int, string, string> newDependency = new Tuple<int, string, string>(infoToAdd.Hash, infoToAdd.Name, infoToAdd.Version);

                string nameAndVersion = string.Format(newDependency.Item2 + " (" + newDependency.Item3 + ")");
                TreeNode newNode = new TreeNode
                {
                    Tag = newDependency,
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
            if (treCustomDataFiles.SelectedNode.Tag is CustomDataDirectoryInfo infoToAdd)
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
    }
}
