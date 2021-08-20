using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Sinners;
using NLog;


namespace ChummerHub.Client.UI
{
    public partial class ucSINnerVisibility : UserControl
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private SINnerVisibility _mySINnerVisibility;
        public SINnerVisibility MyVisibility
        {
            get => _mySINnerVisibility;
            set
            {
                _mySINnerVisibility = value;
                FillVisibilityListBox();
            }
        }

        public CheckedListBox MyCheckBoxList => clbVisibilityToUsers;

        public CheckBox MyCheckBoxGroupVisible => cbVisibleInGroups;

        public ucSINnerVisibility()
        {
            InitializeComponent();
            cbVisibleInGroups.Checked = true;
            MyVisibility = new SINnerVisibility()
            {
                UserRights = new List<SINnerUserRight>()
            };
            string tooltip = "not checked: Character is only visible to users in the list above." + Environment.NewLine;
            tooltip += "checked: Character is visible to users, who can see/own other characters that are members of the same group." + Environment.NewLine + Environment.NewLine;
            tooltip += "This is ment to be used by GMs managing multiple groups and want to put NPCs into groups mixing them with player-characters.";                
            clbVisibilityToUsers.SetToolTip(tooltip);
            clbVisibilityToUsers.ItemCheck += clbVisibilityToUsers_ItemCheck;
        }

        public ucSINnerVisibility(SINnerVisibility vis)
        {
            MyVisibility = vis;
            InitializeComponent();
            cbVisibleInGroups.Checked = MyVisibility?.IsGroupVisible != null && MyVisibility.IsGroupVisible;
            clbVisibilityToUsers.ItemCheck += clbVisibilityToUsers_ItemCheck;
        }

        private void FillVisibilityListBox()
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                try
                {
                    //clbVisibilityToUsers.DataSource = null;
                    if (MyVisibility != null)
                    {
                        clbVisibilityToUsers.DataSource = MyVisibility.UserRightsObservable;
                    }
                    clbVisibilityToUsers.DisplayMember = "EMail";
                    clbVisibilityToUsers.ValueMember = "CanEdit";
                    for (int i = 0; i < clbVisibilityToUsers.Items.Count; i++)
                    {
                        SINnerUserRight obj = (SINnerUserRight)clbVisibilityToUsers.Items[i];
                        clbVisibilityToUsers.SetItemChecked(i, obj.CanEdit);
                    }
                    clbVisibilityToUsers.Refresh();
                    cbVisibleInGroups.Checked = MyVisibility?.IsGroupVisible != null && MyVisibility.IsGroupVisible;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    throw;
                }
            });
        }

        private void bVisibilityAddEmail_Click(object sender, EventArgs e)
        {
            string email = tbVisibilityAddEmail.Text;
            MyVisibility.AddVisibilityForEmail(email);
            FillVisibilityListBox();
        }

        private void clbVisibilityToUsers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (clbVisibilityToUsers.SelectedIndex != -1)
            {
                ListBox.SelectedObjectCollection selectedItems = clbVisibilityToUsers.SelectedItems;
                for (int i = selectedItems.Count - 1; i >= 0; --i)
                {
                    if (selectedItems[i] is SINnerUserRight userright)
                    {
                        userright.CanEdit = e.NewValue == CheckState.Checked;
                    }
                }
            }
            else
                Program.MainForm.ShowMessageBox("No email selected!");
        }

        private void bVisibilityRemove_Click(object sender, EventArgs e)
        {
            if (clbVisibilityToUsers.SelectedIndex != -1)
            {
                ListBox.SelectedObjectCollection selectedItems = clbVisibilityToUsers.SelectedItems;
                for (int i = selectedItems.Count - 1; i >= 0; --i)
                {
                    if (selectedItems[i] is SINnerUserRight userright)
                    {
                        MyVisibility.UserRights.Remove(userright);
                        MyVisibility.UserRightsObservable = null;
                    }
                }
                FillVisibilityListBox();
            }
            else
                Program.MainForm.ShowMessageBox("No email selected!");
        }

        private void CbVisibleInGroups_Click(object sender, EventArgs e)
        {
            MyVisibility.IsGroupVisible = cbVisibleInGroups.Checked;
        }
    }
}
