using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.Properties;
using NLog;


namespace ChummerHub.Client.UI
{
    public partial class ucSINnerShare : UserControl
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public frmSINnerShare MyFrmSINnerShare { get; set; }

        public CharacterCache MyCharacterCache { get; set; }
        public SINnerSearchGroup MySINnerSearchGroup { get; set; }
        public Func<Task<MyUserState>> DoWork { get; }
        public Action<MyUserState> RunWorkerCompleted { get; }
        public Action<int, MyUserState> ReportProgress { get; }

        public ucSINnerShare()
        {
            InitializeComponent();
            DoWork += ShareChummer_DoWork;
            ReportProgress += ShareChummer_ProgressChanged;
            RunWorkerCompleted += ShareChummer_RunWorkerCompleted;  //Tell the user how the process went
        }

        public class MyUserState
        {
            public string LinkText { get; set; }
            public string StatusText { get; set; }
            public ucSINnerShare myWorker { get; set; }

            public MyUserState(ucSINnerShare worker)
            {
                myWorker = worker;
            }
            public int CurrentProgress { get; internal set; }
            /// <summary>
            /// 5 Steps are made in total!
            /// </summary>
            public int ProgressSteps { get; internal set; }
        }

        private Task<MyUserState> ShareChummer_DoWork()
        {
            if (MySINnerSearchGroup != null)
            {
                return ShareChummerGroup();
            }

            if (MyCharacterCache != null)
            {
                return ShareSingleChummer();
            }

            throw new ArgumentException("Either MySINnerSearchGroup or MyCharacterCache must be set!");
        }

        private async Task<MyUserState> ShareChummerGroup()
        {
            try
            {
                string hash = string.Empty;
                using (CustomActivity op_shareChummer = Timekeeper.StartSyncron("Share Group", null,
                    CustomActivity.OperationType.DependencyOperation, MySINnerSearchGroup?.Groupname))
                {
                    MyUserState myState = new MyUserState(this);
                    SinnersClient client = StaticUtils.GetClient();

                    if (string.IsNullOrEmpty(MySINnerSearchGroup?.Id?.ToString()))
                    {
                        myState.StatusText = "Group Id is unknown or not issued!";
                        ReportProgress(30, myState);
                    }
                    else
                    {
                        myState.StatusText = "Group Id is " + MySINnerSearchGroup?.Id + ".";
                        myState.CurrentProgress = 30;
                        ReportProgress(myState.CurrentProgress, myState);
                    }

                    //check if char is already online and updated
                    using (_ = Timekeeper.StartSyncron(
                        "check if online", op_shareChummer,
                        CustomActivity.OperationType.DependencyOperation, MySINnerSearchGroup?.Groupname))
                    {
                        ResultGroupGetGroupById checkresult = await client.GetGroupByIdAsync(MySINnerSearchGroup?.Id).ConfigureAwait(false);
                        if (checkresult == null)
                            throw new ArgumentException("Could not parse result from SINners Webservice!");
                        if (!checkresult.CallSuccess)
                        {
                            if (checkresult.MyException != null)
                                throw new ArgumentException(
                                    "Error from SINners Webservice: " + checkresult.ErrorText,
                                    checkresult.MyException.ToString());
                            throw new ArgumentException("Error from SINners Webservice: " +
                                                        checkresult.ErrorText);
                        }

                        hash = checkresult.MyGroup.MyHash;
                    }


                    myState.StatusText = "Group is online available.";
                    myState.CurrentProgress = 90;
                    ReportProgress(myState.CurrentProgress, myState);

                    string url = client.BaseUrl + "G";
                    url += "/" + hash;
                    if (Settings.Default.OpenChummerFromSharedLinks)
                    {
                        url += "?open=true";
                    }

                    myState.LinkText = url;
                    ReportProgress(100, myState);
                    RunWorkerCompleted(myState);
                    return myState;
                }
            }
            catch (Exception exception)
            {
                Log.Warn(exception);
                throw;
            }
        }

        private async Task<MyUserState> ShareSingleChummer()
        {
            if (MyCharacterCache == null)
                throw new ArgumentNullException(nameof(MyCharacterCache));
            string hash = string.Empty;
            try
            {
                using (CustomActivity op_shareChummer = Timekeeper.StartSyncron("Share Chummer", null,
                    CustomActivity.OperationType.DependencyOperation, MyCharacterCache.FilePath))
                {
                    MyUserState myState = new MyUserState(this);
                    CharacterExtended ce = null;
                    SinnersClient client = StaticUtils.GetClient();
                    string sinnerid = string.Empty;
                    Guid SINid = Guid.Empty;

                    try
                    {
                        async Task<CharacterExtended> GetCharacterExtended(CustomActivity parentActivity)
                        {
                            using (_ = Timekeeper.StartSyncron("Loading Chummerfile", parentActivity,
                                CustomActivity.OperationType.DependencyOperation, MyCharacterCache.FilePath))
                            {
                                Character c = PluginHandler.MainForm.OpenCharacters.FirstOrDefault(a => a.FileName == MyCharacterCache.FilePath);
                                bool blnSuccess = true;
                                if (c == null)
                                {
                                    c = new Character { FileName = MyCharacterCache.FilePath };
                                    using (LoadingBar frmLoadingForm = new LoadingBar { CharacterFile = MyCharacterCache.FilePath })
                                    {
                                        await frmLoadingForm.ResetAsync(36);
                                        frmLoadingForm.Show();
                                        myState.StatusText = "Loading chummer file...";
                                        myState.CurrentProgress += 10;
                                        ReportProgress(myState.CurrentProgress, myState);
                                        blnSuccess = await c.LoadAsync(frmLoadingForm, false);
                                    }
                                }

                                if (!blnSuccess)
                                    throw new ArgumentNullException("Could not load Character file " +
                                                                    MyCharacterCache.FilePath +
                                                                    ".");
                                ce = new CharacterExtended(c, null, MyCharacterCache);
                                if (ce?.MySINnerFile?.Id != null)
                                    sinnerid = ce.MySINnerFile.Id.ToString();
                                hash = ce?.MySINnerFile?.MyHash;
                                return ce;
                            }
                        }

                        if (MyCharacterCache.MyPluginDataDic.TryGetValue("SINnerId", out object sinneridobj))
                        {
                            sinnerid = sinneridobj?.ToString() ?? string.Empty;
                        }
                        else
                        {
                            ce = await GetCharacterExtended(op_shareChummer);
                            sinnerid = ce.MySINnerFile.Id.ToString();
                            hash = ce?.MySINnerFile?.MyHash ?? string.Empty;
                        }


                        if (string.IsNullOrEmpty(sinnerid) || !Guid.TryParse(sinnerid, out SINid))
                        {
                            myState.StatusText = "SINner Id is unknown or not issued!";
                            ReportProgress(30, myState);
                        }
                        else
                        {
                            myState.StatusText = "SINner Id is " + SINid + ".";
                            myState.CurrentProgress = 30;
                            ReportProgress(myState.CurrentProgress, myState);
                        }


                        try
                        {
                            //check if char is already online and updated
                            ResultSinnerGetSINById checkresult;
                            using (_ = Timekeeper.StartSyncron(
                                "check if online", op_shareChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacterCache?.FilePath))
                            {
                                checkresult = await client.GetSINByIdAsync(SINid);
                                if (checkresult == null)
                                    throw new ArgumentException("Could not parse result from SINners Webservice!");
                                if (!checkresult.CallSuccess)
                                {
                                    if (checkresult.MyException != null)
                                        throw new ArgumentException(
                                            "Error from SINners Webservice: " + checkresult.ErrorText,
                                            checkresult.MyException.ToString());
                                    throw new ArgumentException("Error from SINners Webservice: " +
                                                                checkresult.ErrorText);
                                }

                                hash = checkresult.MySINner.MyHash;
                            }


                            DateTime lastWriteTimeUtc = MyCharacterCache != null ? File.GetLastWriteTimeUtc(MyCharacterCache.FilePath) : DateTime.MinValue;
                            if (checkresult.MySINner.LastChange < lastWriteTimeUtc)
                            {
                                if (ce == null)
                                {
                                    myState.StatusText = "The Chummer is newer and has to be uploaded again.";
                                    myState.CurrentProgress = 30;
                                    ReportProgress(myState.CurrentProgress, myState);
                                    ce = await GetCharacterExtended(op_shareChummer);
                                }

                                if (ce != null)
                                {
                                    using (CustomActivity op_uploadChummer = Timekeeper.StartSyncron(
                                        "Uploading Chummer", op_shareChummer,
                                        CustomActivity.OperationType.DependencyOperation, MyCharacterCache?.FilePath))
                                    {
                                        myState.StatusText = "Checking SINner availability (and if necessary upload it).";
                                        myState.CurrentProgress = 35;
                                        ReportProgress(myState.CurrentProgress, myState);
                                        myState.ProgressSteps = 10;
                                        await ce.Upload(myState, op_uploadChummer);
                                        if (ce.MySINnerFile.Id != null)
                                            SINid = ce.MySINnerFile.Id.Value;
                                        ResultSinnerGetSINById result = await client.GetSINByIdAsync(SINid);
                                        if (result == null)
                                            throw new ArgumentException(
                                                "Could not parse result from SINners Webservice!");
                                        if (!result.CallSuccess)
                                        {
                                            if (result.MyException != null)
                                                throw new ArgumentException(
                                                    "Error from SINners Webservice: " + result.ErrorText,
                                                    result.MyException.ToString());
                                            throw new ArgumentException(
                                                "Error from SINners Webservice: " + result.ErrorText);
                                        }

                                        hash = result.MySINner.MyHash;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            //checkresult?.Dispose();
                        }
                    }
                    finally
                    {
                        ce?.Dispose();
                    }

                    myState.StatusText = "SINner is online available.";
                    myState.CurrentProgress = 90;
                    ReportProgress(myState.CurrentProgress, myState);

                    string url = client.BaseUrl + "O";
                    url += "/" + hash;
                    if (Settings.Default.OpenChummerFromSharedLinks)
                    {
                        url += "?open=true";
                    }

                    myState.LinkText = url;
                    ReportProgress(100, myState);
                    RunWorkerCompleted(myState);
                    return myState;
                }
            }
            catch (Exception exception)
            {
                Log.Warn(exception);
                throw;
            }
        }

        private void ShareChummer_ProgressChanged(int progress, MyUserState e)
        {
            pgbStatus.DoThreadSafe(() =>
            {
                pgbStatus.Value = e.CurrentProgress;
            });

            if (e is MyUserState us)
            {
                tbStatus.DoThreadSafe(() =>
                {
                    if (!tbStatus.Text.Contains(us.StatusText))
                        tbStatus.Text += us.StatusText + Environment.NewLine;
                });
                tbLink.DoThreadSafe(() =>
                {
                    if (!string.IsNullOrEmpty(us.LinkText) && (us.LinkText != tbLink.Text))
                    {
                        tbLink.Text = us.LinkText;
                    }
                });
            }
        }
        private void ShareChummer_RunWorkerCompleted(MyUserState us)
        {
            pgbStatus.DoThreadSafe(() => { pgbStatus.Value = 100; });
            tbLink.DoThreadSafe(() =>
            {
                tbLink.Text = us.LinkText;
            });
            tbStatus.DoThreadSafe(() =>
            {
                tbStatus.Text += "Link copied to clipboard." + Environment.NewLine;
                Clipboard.SetText(us.LinkText);
                tbStatus.Text += "Process was completed" + Environment.NewLine;
            });
        }

        private void BOk_Click(object sender, EventArgs e)
        {
            MyFrmSINnerShare.Close();
        }
    }
}
