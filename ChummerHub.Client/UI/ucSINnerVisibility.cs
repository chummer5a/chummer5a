using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using NLog;
using SINners.Models;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnerVisibility : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
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
            clbVisibilityToUsers.ItemCheck += clbVisibilityToUsers_ItemCheck;
        }

        public ucSINnerVisibility(SINnerVisibility vis)
        {
            MyVisibility = vis;
            InitializeComponent();
            cbVisibleInGroups.Checked = MyVisibility?.IsGroupVisible.HasValue != true || MyVisibility.IsGroupVisible.Value;
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
                        clbVisibilityToUsers.SetItemChecked(i, obj.CanEdit != null && obj.CanEdit.Value);
                    }
                    clbVisibilityToUsers.Refresh();
                    cbVisibleInGroups.Checked = MyVisibility?.IsGroupVisible.HasValue != true || MyVisibility.IsGroupVisible.Value;
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
                        MyVisibility.UserRightsObservable.Remove(userright);
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
