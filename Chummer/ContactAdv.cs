using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class ContactAdv : Form
    {
        private HoverDisplayCordinator _hoverDisplay;
        private ContactControl _host;
        private Contact _objContact;
        private Character _objCharacter;
        private ContractControlCallBackObject _cb;

        public ContactAdv(HoverDisplayCordinator hoverDisplay, ContactControl host, Contact contact, Character character, ContractControlCallBackObject cb)
        {
            //Accualy create the form, add components and all that poo
            InitializeComponent();


            //Save those for later date
            _cb = cb;
            _host = host;
            _objContact = contact;
            _objCharacter = character;
            _hoverDisplay = hoverDisplay;
            //and suscribe to the moment this+usercontrol leaves focus
            _hoverDisplay.OnAllLeave += _hoverDisplay_OnNoneActive;

            //Limit Connection based on created and friends in high places
            if (!_objCharacter.Created)
            {
                if (_objCharacter.FriendsInHighPlaces)
                {
                    nudConnection.Maximum = 12;
                }
                else
                {
                    nudConnection.Maximum = 6;
                }
            }
            else
            {
                nudConnection.Maximum = 12;
            }

            nudConnection.Value = _objContact.Connection;
            nudLoyalty.Value = _objContact.Loyalty;
            chkGroup.Checked = _objContact.IsGroup;

            //We don't acctualy pay for contacts in play so everyone is free
            //Don't present a useless field
            if (_objCharacter.Created)
            {
                lblFree.Visible = false;
                chkFree.Visible = false;
            }
            
            

            LoadLanguage();
        }

        void _hoverDisplay_OnNoneActive(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadLanguage()
        {
            

            //setup the right tooltips depending on enemy/contact
            bool blnEnemy = _objContact.EntityType == ContactType.Enemy;
            
            if (_objContact.FileName != "")
            {
                tipTooltip.SetToolTip(imgLink,
                    LanguageManager.Instance.GetString(blnEnemy
                        ? "Tip_Enemy_OpenLinkedEnemy"
                        : "Tip_Contact_OpenLinkedContact"));
            }
            else
            {
                tipTooltip.SetToolTip(imgLink,
                    LanguageManager.Instance.GetString(blnEnemy
                        ? "Tip_Enemy_LinkEnemy"
                        : "Tip_Contact_LinkContact"));
            }
            string strTooltip = LanguageManager.Instance.GetString(blnEnemy
                    ? "Tip_Enemy_EditNotes"
                    : "Tip_Contact_EditNotes");
            if (_objContact.Notes != string.Empty)
                strTooltip += "\n\n" + _objContact.Notes;
            tipTooltip.SetToolTip(imgNotes, strTooltip);

            //Set Loyality to Incidence in case of Enemies
            if (blnEnemy)
            {
                lblLoyalty.Text = "Incidence:";
                lblLoyalty.Tag = "Label_Enemy_Incidence"; //Tag for translation
            }


            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void nudConnection_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ConnectionGroupRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            if (!_objCharacter.Created)
            {
                if (_objCharacter.FriendsInHighPlaces)
                {
                    nudConnection.Maximum = 12;
                }
                else
                {
                    nudConnection.Maximum = 6;
                }
            }
            else
            {
                nudConnection.Maximum = 12;
            }
            _objContact.Connection = Convert.ToInt32(nudConnection.Value);
            _cb.OnConnectionRatingChanged(this);
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the LoyaltyRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            _objContact.Loyalty = Convert.ToInt32(nudLoyalty.Value);
            _cb.OnLoyaltyRatingChanged(this);
        }

        private void imgLink_Click(object sender, EventArgs e)
        {
            // Determine which options should be shown based on the FileName value.
            if (_objContact.FileName != "")
            {
                tsAttachCharacter.Visible = false;
                tsContactOpen.Visible = true;
                tsRemoveCharacter.Visible = true;
            }
            else
            {
                tsAttachCharacter.Visible = true;
                tsContactOpen.Visible = false;
                tsRemoveCharacter.Visible = false;
            }
            cmsContact.Show(imgLink, imgLink.Left - 110, imgLink.Top);
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmContactNotes = new frmNotes();
            frmContactNotes.Notes = _objContact.Notes;
            frmContactNotes.ShowDialog(this);

            if (frmContactNotes.DialogResult == DialogResult.OK)
                _objContact.Notes = frmContactNotes.Notes;

            string strTooltip = "";
            if (_objContact.EntityType == ContactType.Enemy)
                strTooltip = LanguageManager.Instance.GetString("Tip_Enemy_EditNotes");
            else
                strTooltip = LanguageManager.Instance.GetString("Tip_Contact_EditNotes");
            if (_objContact.Notes != string.Empty)
                strTooltip += "\n\n" + _objContact.Notes;
            tipTooltip.SetToolTip(imgNotes, strTooltip);
        }

        private void cmsContact_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem objItem in ((ContextMenuStrip)sender).Items)
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
                }
            }
        }

        private void tsContactOpen_Click(object sender, EventArgs e)
        {
            bool blnError = false;
            bool blnUseRelative = false;

            // Make sure the file still exists before attempting to load it.
            if (!File.Exists(_objContact.FileName))
            {
                // If the file doesn't exist, use the relative path if one is available.
                if (_objContact.RelativeFileName == "")
                    blnError = true;
                else
                {
                    MessageBox.Show(Path.GetFullPath(_objContact.RelativeFileName));
                    if (!File.Exists(Path.GetFullPath(_objContact.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;
                }

                if (blnError)
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_FileNotFound").Replace("{0}", _objContact.FileName), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (!blnUseRelative)
                GlobalOptions.Instance.MainForm.LoadCharacter(_objContact.FileName, false);
            else
            {
                string strFile = Path.GetFullPath(_objContact.RelativeFileName);
                GlobalOptions.Instance.MainForm.LoadCharacter(strFile, false);
            }
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_RemoveCharacterAssociation"), LanguageManager.Instance.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = "";
                _objContact.RelativeFileName = "";
                if (_objContact.EntityType == ContactType.Enemy)
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_LinkFile"));
                else
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_LinkFile"));
                _cb.OnFileNameChanged(this);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objContact.FileName = openFileDialog.FileName;
                if (_objContact.EntityType == ContactType.Enemy)
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_OpenFile"));
                else
                    tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_OpenFile"));

                // Set the relative path.
                Uri uriApplication = new Uri(@Application.StartupPath);
                Uri uriFile = new Uri(@_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative.ToString();

                _cb.OnFileNameChanged(this);
            }
        }

        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            _objContact.IsGroup = chkGroup.Checked;
            chkGroup.Enabled = !_objContact.MadeMan;

            _cb.OnGroupStatusChanged(this);

            //Loyality can be changed by event above
            nudLoyalty.Enabled = !_objContact.IsGroup;
            nudLoyalty.Value = _objContact.Loyalty;
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            _objContact.Free = chkFree.Checked;
            _cb.OnOtherCostChanged(this);
        }
    }
}
