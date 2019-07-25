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
using ChummerHub.Client.Backend;
using ChummerHub.Client.Model;
using Microsoft.Rest;
using NLog;
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
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {


                MyUserState myState = new MyUserState();
                string sinnerid = "";
                if (MyCharacterCache.MyPluginDataDic.TryGetValue("SINnerId", out Object sinneridobj))
                {
                    sinnerid = sinneridobj?.ToString();

                }
                else
                {
                    Character c = new Character()
                    {
                        FileName = MyCharacterCache.FilePath
                    };
                    using (frmLoading frmLoadingForm = new frmLoading {CharacterFile = MyCharacterCache.FilePath})
                    {
                        frmLoadingForm.Reset(36);
                        frmLoadingForm.Show();
                        if (c.Load(frmLoadingForm, false).Result)
                        {
                            CharacterExtended ce = new CharacterExtended(c, null);
                            sinnerid = ce.MySINnerFile.Id.ToString();
                        }
                    }
                }

                if (!Guid.TryParse(sinnerid, out Guid SINid))
                {
                    throw new ArgumentException("Could not parse sinnerid: " + sinnerid);
                }

                myState.StatusText = "SINner Id is " + SINid + ".";
                backgroundWorker1.ReportProgress(20, myState);

                var client = StaticUtils.GetClient();
                HttpOperationResponse<ResultSinnerGetSINById> response =
                    (client.GetSINByIdWithHttpMessagesAsync(SINid).Result);
                ResultSinnerGetSINById result =
                    ChummerHub.Client.Backend.Utils.HandleError(response, response.Body).Result as
                        ResultSinnerGetSINById;
                if (result == null)
                    throw new ArgumentException("Could not parse result from SINners Webservice!");
                if (result.CallSuccess != true)
                {
                    if (result.MyException is Exception myException)
                        throw new ArgumentException("Error from SINners Webservice: " + result.ErrorText, myException);
                    else
                        throw new ArgumentException("Error from SINners Webservice: " + result.ErrorText);
                }

                myState.StatusText = "SINner found online.";
                backgroundWorker1.ReportProgress(40, myState);

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
