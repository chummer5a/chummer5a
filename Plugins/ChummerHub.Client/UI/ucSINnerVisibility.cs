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
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Sinners;
using NLog;


namespace ChummerHub.Client.UI
{
    public partial class ucSINnerVisibility : UserControl
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
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
                Program.ShowMessageBox("No email selected!");
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
                Program.ShowMessageBox("No email selected!");
        }

        private void CbVisibleInGroups_Click(object sender, EventArgs e)
        {
            MyVisibility.IsGroupVisible = cbVisibleInGroups.Checked;
        }
    }
}
