using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.Backend;
using System.Net;
using SINners.Models;

namespace ChummerHub.Client.UI
{
    public partial class SINnerGroupSearch : UserControl
    {
        public SINner MySinner { get; set; }
        public SINnerGroupSearch()
        {
            InitializeComponent();
        }

        private void bCreateGroup_Click(object sender, EventArgs e)
        {
            CreateGroup(this.tbSearchGroupname.Text);
        }

        private async Task<SINnerGroup> CreateGroup(string groupname)
        {
            try
            {
                if(String.IsNullOrEmpty(this.tbSearchGroupname.Text))
                {
                    MessageBox.Show("Please specify a groupename to create!");
                    this.tbSearchGroupname.Focus();
                    return null;
                }
                if(this.MySinner == null)
                {
                    MessageBox.Show("MySinner not set!");
                    return null;
                }
                var response = await StaticUtils.Client.PostGroupWithHttpMessagesAsync(this.tbSearchGroupname.Text, MySinner.Id);
                var rescontent = await response.Response.Content.ReadAsStringAsync();
                if((response.Response.StatusCode == HttpStatusCode.OK)
                    || (response.Response.StatusCode == HttpStatusCode.Created))
                {

                }
                else
                {
                    MessageBox.Show(rescontent);
                }

            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                MessageBox.Show(ex.Message);
                throw;
            }
            return null;
        }

        private void tlpSearchGroups_Click(object sender, EventArgs e)
        {

        }
    }
}
