using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Backend;
using Chummer.Plugins;
using NLog;
using ChummerHub.Client.Sinners;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersBasic : UserControl
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ucSINnersUserControl myUC { get; set; }

        public ucSINnersBasic()
        {
            SINnersBasicConstructor(null);
        }

        public ucSINnersBasic(ucSINnersUserControl parent)
        {
            if (parent != null)
                SINnersBasicConstructor(parent);
        }

        private bool _inConstructor;

        private void SINnersBasicConstructor(ucSINnersUserControl parent)
        {
            _inConstructor = true;
            InitializeComponent();

            TagValueArchetype.DataSource = Contact.ContactArchetypes(parent.CharacterObject);
            Name = "SINnersBasic";
            bGroupSearch.Enabled = false;
            AutoSize = true;
            myUC = parent;
            myUC.MyCE = parent.MyCE;
            if (myUC.MyCE?.MySINnerFile?.Id != null)
                tbID.Text = myUC.MyCE?.MySINnerFile?.Id?.ToString();
            string tip =
                "Assigning this SINner a new Id enables you to save multiple versions of this chummer on SINnersHub." +
                Environment.NewLine;
            bGenerateNewId.SetToolTip(tip);
            Task.Run(() => CheckSINnerStatus().ContinueWith(a =>
            {
                if (!a.Result)
                {
                    Log.Error("somehow I couldn't check the onlinestatus of " +
                                                        myUC.MyCE.MySINnerFile.Id);
                }
            }));
            foreach (object cb in gpTags.Controls)
            {
                if (cb is Control cont)
                    cont.Click += OnGroupBoxTagsClick;
                switch (cb)
                {
                    case CheckBox cccb:
                        cccb.CheckedChanged += OnGroupBoxTagsClick;
                        cccb.CheckStateChanged += OnGroupBoxTagsClick;
                        break;
                    case ComboBox ccb:
                        ccb.TextChanged += OnGroupBoxTagsClick;
                        break;
                    case TextBox ctb:
                        ctb.TextChanged += OnGroupBoxTagsClick;
                        break;
                }
            }

            _inConstructor = false;
        }

        public async Task<bool> CheckSINnerStatus()
        {
            try
            {
                if (myUC?.MyCE?.MySINnerFile?.Id == null || myUC.MyCE.MySINnerFile.Id == Guid.Empty)
                {
                    await bUpload.DoThreadSafeAsync(x => x.Text = "SINless Character/Error" );
                    return false;
                }

                using (CursorWait.New(this, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    ResultSinnerGetSINnerGroupFromSINerById response = await client.GetSINnerGroupFromSINerByIdAsync(myUC.MyCE.MySINnerFile.Id.Value);
                    
                    SINnerGroup objMySiNnerGroup = response.MySINnerGroup;
                    
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        if (objMySiNnerGroup != null)
                        {
                            myUC.MyCE.MySINnerFile.MyGroup = objMySiNnerGroup;
                            bUpload.Text = "Remove from SINners";
                            bGroupSearch.Enabled = true;
                            lUploadStatus.Text = "Online";
                            bUpload.Enabled = true;
                        }
                        else
                        {
                            myUC.MyCE.MySINnerFile.MyGroup = null;
                            lUploadStatus.Text = "Not Online";
                            bGroupSearch.Enabled = false;
                            bGroupSearch.SetToolTip(
                                "SINner needs to be uploaded first, before he/she can join a group.");
                            bUpload.Enabled = true;
                            bUpload.Text = "Upload";
                        }
                        //else if (eResponseStatus == HttpStatusCode.NoContent)
                        //{
                        //    myUC.MyCE.MySINnerFile.MyGroup = null;
                        //    lUploadStatus.Text = "Status Code: " + eResponseStatus;
                        //    bGroupSearch.Enabled = true;
                        //    bGroupSearch.SetToolTip(
                        //        "SINner does not belong to a group.");
                        //    bUpload.Text = "Remove from SINners";
                        //    lUploadStatus.Text = "Online";
                        //    bUpload.Enabled = true;
                        //}
                        cbTagCustom.Enabled = false;
                        TagValueCustomName.Enabled = false;
                    });
                    await this.DoThreadSafeAsync(UpdateTags);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                await bUpload.DoThreadSafeAsync(x => x.Text = "Unknown Status");
                return false;
            }
            return true;
        }

        private void UpdateTags()
        {
            if (StaticUtils.UserRoles.Any(a => string.Equals(a, "ArchetypeAdmin", StringComparison.InvariantCultureIgnoreCase)))
            {
                cbTagCustom.Enabled = true;
                TagValueCustomName.Enabled = true;
            }
            if (myUC?.MyCE?.MySINnerFile?.MyGroup != null)
                lGourpForSinner.Text = myUC.MyCE.MySINnerFile.MyGroup.Groupname;

            List<Control> gpControlSeq = GetAllControls(gpTags).Where(x => x.Name.Contains("cbTag")).ToList();
            List<Control> gpControlValueSeq = GetAllControls(gpTags).Where(x => x.Name.Contains("TagValue")).ToList();

            foreach (Control cb in gpControlSeq)
            {
                if (cb is CheckBox cbTag)
                    cbTag.CheckState = CheckState.Unchecked;
            }

            if (myUC.MyCE?.MySINnerFile != null)
            {
                foreach (Tag tag in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags)
                {
                    if (tag == null)
                        continue;
                    //search for a CheckBox that is named like the tag
                    string checkBoxKey = "cbTag" + tag.TagName;
                    Control objMatchingCheckBox = gpControlSeq.Find(x => x.Name == checkBoxKey);
                    if (objMatchingCheckBox == null)
                        continue;
                    if (!(objMatchingCheckBox is CheckBox cbTag))
                        throw new ArgumentNullException("Control " + checkBoxKey + " is NOT a Checkbox!");

                    cbTag.Checked = bool.TryParse(tag.TagValue, out bool value) && value;
                    //search for the value-control (whatever that may be)
                    Control objMatchingControl = gpControlValueSeq.Find(x => x.Name == "TagValue" + tag.TagName);
                    if (objMatchingControl == null)
                        continue;

                    cbTag.Checked = true;
                    switch (objMatchingControl)
                    {
                        case TextBox tbTagValue:
                            tbTagValue.Text = tag.TagValue;
                            break;
                        case ComboBox comboTagValue:
                        {
                            if (tag.TagValue != null)
                            {
                                if (!comboTagValue.Items.Contains(tag.TagValue))
                                    comboTagValue.Items.Add(tag.TagValue);
                                comboTagValue.SelectedItem = tag.TagValue;
                            }

                            break;
                        }
                        case NumericUpDown upDownTagValue when decimal.TryParse(tag.TagValue, out decimal val):
                            upDownTagValue.Value = val;
                            break;
                    }
                }
            }
        }

        private void OnGroupBoxTagsClick(object sender, object eventargs)
        {
            // Call the base class
            //base.OnClick(eventargs);
            SaveTagsToSinner();
        }

        private IEnumerable<Control> GetAllControls(Control container)
        {
            foreach (Control objLoopChild in container.Controls)
            {
                yield return objLoopChild;
                foreach (Control objLoopSubchild in GetAllControls(objLoopChild))
                    yield return objLoopSubchild;
            }
        }

        private void SaveTagsToSinner()
        {
            if (_inConstructor)
                return;
            if (myUC == null)
                return;
            List<Control> gpControlValueSeq = GetAllControls(gpTags).Where(x => x.Name.Contains("TagValue")).ToList();

            myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags = myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Where(a => a != null).ToList();

            foreach (Control cb in GetAllControls(gpTags).Where(x => x.Name.Contains("cbTag")))
            {
                if (!(cb is CheckBox cbTag))
                    continue;

                string tagName = cbTag.Name.Substring("cbTag".Length);
                Tag tag;
                if (myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.All(a => a != null && a.TagName != tagName))
                {
                    tag = new Tag(true)
                    {
                        SiNnerId = myUC.MyCE.MySINnerFile.Id,
                        TagName = tagName
                    };
                }
                else
                {
                    tag = myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.FirstOrDefault(a => a != null &&  a.TagName == tagName);
                }
                if (tag == null)
                    continue;
                if (myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Contains(tag))
                    myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Remove(tag);
                if (!cbTag.Checked)
                    continue;
                if (cbTag.CheckState == CheckState.Checked)
                    tag.TagValue = bool.TrueString;
                myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Add(tag);
                //search for the value
                Control objMatchingControl = gpControlValueSeq.Find(x => x.Name == "TagValue" + tag.TagName);
                switch (objMatchingControl)
                {
                    case TextBox tbTagValue:
                        tag.TagValue = tbTagValue.Text;
                        break;
                    case ComboBox comboTagValue:
                        tag.TagValue = comboTagValue.SelectedItem?.ToString();
                        break;
                    case NumericUpDown upDownTagValue:
                        tag.TagValue = upDownTagValue.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                }
            }
        }


        private async void bUpload_Click(object sender, EventArgs e)
        {
            using (CursorWait.New(this, true))
            {
                try
                {
                    if (bUpload.Text.Contains("Upload"))
                    {
                        lUploadStatus.Text = "Uploading";
                        await myUC.MyCE.Upload();
                    }
                    else
                    {
                        lUploadStatus.Text = "Removing";
                        await myUC.RemoveSINnerAsync();
                    }

                }
                catch (Exception exception)
                {
                    Program.ShowMessageBox(exception.Message);
                }
            }
            await CheckSINnerStatus();
        }

        private void bGroupSearch_Click(object sender, EventArgs e)
        {
            using (frmSINnerGroupSearch gs = new frmSINnerGroupSearch(myUC.MyCE, this))
            {
                async void OnGroupJoinCallback(object o, SINnerGroup group)
                {
                    await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodes(PluginHandler.MyPluginHandlerInstance);
                }

                gs.MySINnerGroupSearch.OnGroupJoinCallback += OnGroupJoinCallback;
                gs.ShowDialog(Program.MainForm);
            }
        }

        private async void BVisibility_Click(object sender, EventArgs e)
        {
            using (frmSINnerVisibility visfrm = new frmSINnerVisibility())
            {
                using (CursorWait.New(this, true))
                {
                    if (myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility.UserRights.Count == 0)
                    {
                        SinnersClient client = StaticUtils.GetClient();
                        if (myUC.MyCE.MySINnerFile.Id != null)
                        {
                            ResultSinnerGetSINnerVisibilityById res =
                                await client.GetSINnerVisibilityByIdAsync(
                                    myUC.MyCE.MySINnerFile.Id.Value);
                            await Backend.Utils.ShowErrorResponseFormAsync(res);
                            if (res.CallSuccess)
                            {
                                myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility.UserRights = res.UserRights;
                            }

                            
                        }
                    }
                }

                visfrm.MyVisibility = myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility;
                DialogResult result = visfrm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility = visfrm.MyVisibility;
                }
            }
        }

        private async void BGenerateNewId_Click(object sender, EventArgs e)
        {
            Guid? oldId = myUC.MyCE.MySINnerFile.Id;
            myUC.MyCE.MySINnerFile.Id = Guid.NewGuid();
            myUC.MyCE.MySINnerFile.SiNnerMetaData.Id = Guid.NewGuid();
            myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            foreach (SINnerUserRight user in myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility.UserRights)
            {
                user.Id = Guid.NewGuid();
            }
            myUC.MyCE.PopulateTags();
            //this.myUC.MyCE.MySINnerFile.MyExtendedAttributes.Id = Guid.NewGuid();
            if (oldId != null)
            {
                myUC.CharacterObject.FileName =  myUC.CharacterObject.FileName.Replace(oldId.ToString(), myUC.MyCE.MySINnerFile.Id.ToString());
            }
            await myUC.CharacterObject.SaveAsync(myUC.MyCE.MySINnerFile.Id + ".chum5", false);
            tbID.Text = myUC.MyCE.MySINnerFile.Id.ToString();
        }

        private void TlpTags_MouseLeave(object sender, EventArgs e)
        {
            SaveTagsToSinner();
        }
    }
}
