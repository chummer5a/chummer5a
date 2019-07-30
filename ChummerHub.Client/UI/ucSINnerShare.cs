using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Model;
using Microsoft.Rest;
using NLog;
using SINners;
using SINners.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnerShare : UserControl
    {
        private NLog.Logger Log = LogManager.GetCurrentClassLogger();

        public frmCharacterRoster.CharacterCache MyCharacterCache { get; set; }

        public BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        public ucSINnerShare()
        {
            InitializeComponent();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;  //Tell the user how the process went
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true; //Allow for the process to be cancelled
        }

        public class MyUserState
        {
            public string LinkText { get; set; }
            public string StatusText { get; set; }
            public BackgroundWorker myWorker { get; set; }
            public int CurrentProgress { get; internal set; }
            /// <summary>
            /// 5 Steps are made in total!
            /// </summary>
            public int ProgressSteps { get; internal set; }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                MyUserState myState = new MyUserState();
                myState.myWorker = backgroundWorker1;
                CharacterExtended ce = null;
                var client = StaticUtils.GetClient();
                string sinnerid = "";
                if (MyCharacterCache.MyPluginDataDic.TryGetValue("SINnerId", out Object sinneridobj))
                {
                    sinnerid = sinneridobj?.ToString();
                }
                Character c = new Character()
                {
                    FileName = MyCharacterCache.FilePath
                };
                var foundchar = (from a in PluginHandler.MainForm.OpenCharacters
                    where a.FileName == MyCharacterCache.FilePath
                    select a).ToList();
                if (foundchar?.Any() == true)
                    c = foundchar?.FirstOrDefault();
                else
                {
                    using (frmLoading frmLoadingForm = new frmLoading {CharacterFile = MyCharacterCache.FilePath})
                    {
                        frmLoadingForm.Reset(36);
                        frmLoadingForm.Show();
                        myState.StatusText = "Loading chummer to find/generate SINner Id.";
                        myState.CurrentProgress = 10;
                        backgroundWorker1.ReportProgress(myState.CurrentProgress, myState);
                        c.Load(frmLoadingForm, false).ConfigureAwait(false);
                    }
                }
                if (c == null)
                    throw new ArgumentNullException("Could not load Character file " + MyCharacterCache.FilePath + ".");
                ce = new CharacterExtended(c, null);
                if (ce?.MySINnerFile?.Id != null)
                    sinnerid = ce.MySINnerFile.Id.ToString();

                Guid SINid = Guid.Empty;
                if ((String.IsNullOrEmpty(sinnerid)
                    || (!Guid.TryParse(sinnerid, out SINid))))
                {
                    myState.StatusText = "SINner Id is unknown or not issued!";
                    backgroundWorker1.ReportProgress(30, myState);
                }
                else
                {
                    myState.StatusText = "SINner Id is " + SINid + ".";
                    myState.CurrentProgress = 30;
                    backgroundWorker1.ReportProgress(myState.CurrentProgress, myState);
                }

                myState.StatusText = "Checking SINner availability or Uploading SINer";
                myState.CurrentProgress = 35;
                backgroundWorker1.ReportProgress(myState.CurrentProgress, myState);
                myState.ProgressSteps = 10;
                var uploadtask = ce.UploadInBackground(myState);
                uploadtask.Wait(TimeSpan.FromMinutes(2));
                if ((uploadtask.Result == false || (ce.MySINnerFile.Id == null)))
                    throw new ArgumentException("Could not access SINnerId after upload!");
                SINid = ce.MySINnerFile.Id.Value;
                ResultSinnerGetSINById result = client.GetSINById(SINid);
                if (result == null)
                    throw new ArgumentException("Could not parse result from SINners Webservice!");
                if (result.CallSuccess != true)
                {
                    if (result.MyException is Exception myException)
                        throw new ArgumentException("Error from SINners Webservice: " + result.ErrorText, myException);
                    else
                        throw new ArgumentException("Error from SINners Webservice: " + result.ErrorText);
                }

                myState.StatusText = "SINner is online available.";
                myState.CurrentProgress = 90;
                backgroundWorker1.ReportProgress(myState.CurrentProgress, myState);

                string url = "chummer://plugin:SINners:Load:" + sinnerid;
                myState.LinkText = url;
                backgroundWorker1.ReportProgress(100, myState);
                e.Result = myState;
            }
            catch (Exception exception)
            {
                Log.Warn(exception);
                throw;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            pgbStatus.Value = e.ProgressPercentage;
            if (e.UserState is MyUserState us)
            {
                if (!tbStatus.Text.Contains(us.StatusText))
                    tbStatus.Text += us.StatusText + Environment.NewLine;
                if (!String.IsNullOrEmpty(us.LinkText) && (us.LinkText != tbLink.Text))
                {
                    tbLink.Text = us.LinkText;
                }

            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                tbStatus.Text += "Process was cancelled" + Environment.NewLine;
            }
            else if (e.Error != null)
            {
                tbStatus.Text += "There was an error while trying to share this chummer: " + Environment.NewLine + e.Error;
            }
            else
            {
                
                if (e.Result is MyUserState us)
                {
                    tbLink.Text = us.LinkText;
                    tbStatus.Text += "Link copied to clipboard." + Environment.NewLine;
                    Clipboard.SetText(us.LinkText);
                }
                tbStatus.Text += "Process was completed" + Environment.NewLine;
            }
        }
    }
}
