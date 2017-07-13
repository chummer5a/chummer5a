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
using System.Windows.Forms;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectNexus : Form
    {
        private Gear _objGear;

        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectNexus(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _objGear = new Gear(objCharacter);
            MoveControls();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void nudProcessor_ValueChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void nudSystem_ValueChanged(object sender, EventArgs e)
        {
            nudPersona.Minimum = nudSystem.Value * 3;
            CalculateNexus();
        }

        private void nudResponse_ValueChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void nudFirewall_ValueChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void frmSelectNexus_Load(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void nudPersona_ValueChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void nudSignal_ValueChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            CalculateNexus();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Nexus Gear that was built using the dialogue.
        /// </summary>
        public Gear SelectedNexus
        {
            get
            {
                return _objGear;
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFreeItem.Checked;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculate the cost and Rating of the Nexus. Assemble the Gear to represent it.
        /// </summary>
        private void CalculateNexus()
        {
            int intCost = 0;

            int intProcessor = Convert.ToInt32(nudProcessor.Value);
            int intSystem = Convert.ToInt32(nudSystem.Value);
            int intResponse = Convert.ToInt32(nudResponse.Value);
            int intFirewall = Convert.ToInt32(nudFirewall.Value);
            int intPersona = Convert.ToInt32(nudPersona.Value);
            int intSignal = Convert.ToInt32(nudSignal.Value);

            // Determine the individual component costs and ratings.
            // Response.
            string strResponseAvail = (intResponse * 4).ToString();
            int intResponseCost = 0;
            if (intResponse <= 3)
                intResponseCost = intResponse * intProcessor * 50;
            else if (intResponse <= 6)
                intResponseCost = intResponse * intProcessor * 100;
            else
            {
                intResponseCost = intResponseCost * intProcessor * 500;
                strResponseAvail += "F";
            }

            // System.
            string strSystemAvail = string.Empty;
            int intSystemCost = 0;
            if (intSystem <= 3)
            {
                strSystemAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intPersona, GlobalOptions.InvariantCultureInfo) / 5.0)).ToString();
                intSystemCost = intSystem * intPersona * 25;
            }
            else if (intSystem <= 6)
            {
                strSystemAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intPersona, GlobalOptions.InvariantCultureInfo) / 2.0)).ToString();
                intSystemCost = intSystem * intPersona * 50;
            }
            else
            {
                strSystemAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intPersona, GlobalOptions.InvariantCultureInfo) / 2.0)).ToString() + "F";
                intSystemCost = intSystem * intPersona * 300;
            }

            // Firewall.
            string strFirewallAvail = string.Empty;
            int intFirewallCost = 0;
            if (intFirewall <= 3)
            {
                strFirewallAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intProcessor, GlobalOptions.InvariantCultureInfo) / 10.0)).ToString();
                intFirewallCost = intFirewall * intProcessor * 25;
            }
            else if (intFirewall <= 6)
            {
                strFirewallAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intProcessor, GlobalOptions.InvariantCultureInfo) / 5.0)).ToString();
                intFirewallCost = intFirewall * intProcessor * 50;
            }
            else
            {
                strFirewallAvail = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intProcessor, GlobalOptions.InvariantCultureInfo) / 5.0)).ToString() + "F";
                intFirewallCost = intFirewall * intProcessor * 250;
            }

            // Signal.
            int intSignalCost = 0;
            switch (intSignal)
            {
                case 2:
                    intSignalCost = 50;
                    break;
                case 3:
                    intSignalCost = 150;
                    break;
                case 4:
                    intSignalCost = 500;
                    break;
                case 5:
                    intSignalCost = 1000;
                    break;
                case 6:
                    intSignalCost = 3000;
                    break;
                case 7:
                    intSignalCost = 6500;
                    break;
                case 8:
                    intSignalCost = 8750;
                    break;
                case 9:
                    intSignalCost = 12250;
                    break;
                case 10:
                    intSignalCost = 17250;
                    break;
                default:
                    intSignalCost = 10;
                    break;
            }

            intCost = intResponseCost + intSystemCost + intFirewallCost + intSignalCost;
            if (chkFreeItem.Checked)
                intCost = 0;

            // Update the labels.
            lblResponseAvail.Text = strResponseAvail.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
            lblSystemAvail.Text = strSystemAvail.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
            lblFirewallAvail.Text = strFirewallAvail.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
            lblCost.Text = String.Format("{0:###,###,##0¥}", Convert.ToInt32(intCost));

            Gear objNexus = new Gear(_objCharacter);
            objNexus.Name = LanguageManager.Instance.GetString("String_SelectNexus_Nexus") + " (" + LanguageManager.Instance.GetString("String_SelectNexus_Processor") + " " + intProcessor.ToString() + ")";
            objNexus.Cost = intCost.ToString();
            objNexus.Avail = "0";
            objNexus.Category = "Nexus";
            objNexus.Source = "UN";
            objNexus.Page = "50";

            Gear objResponse = new Gear(_objCharacter);
            objResponse.Name = LanguageManager.Instance.GetString("String_Response") + " (" + LanguageManager.Instance.GetString("String_Rating") + " " + intResponse.ToString() + ")";
            objResponse.Category = "Nexus Module";
            objResponse.Cost = "0";
            objResponse.Avail = strResponseAvail;
            objResponse.Source = "UN";
            objResponse.Page = "50";
            objNexus.Children.Add(objResponse);

            Gear objSignal = new Gear(_objCharacter);
            objSignal.Name = LanguageManager.Instance.GetString("String_Signal") + " (" + LanguageManager.Instance.GetString("String_Rating") + " " + intSignal.ToString() + ")";
            objSignal.Category = "Nexus Module";
            objSignal.Cost = "0";
            objSignal.Avail = "0";
            objSignal.Source = "UN";
            objSignal.Page = "50";
            objNexus.Children.Add(objSignal);

            Gear objSystem = new Gear(_objCharacter);
            objSystem.Name = LanguageManager.Instance.GetString("String_System") + " (" + LanguageManager.Instance.GetString("String_Rating") + " " + intSystem.ToString() + ")";
            objSystem.Category = "Nexus Module";
            objSystem.Cost = "0";
            objSystem.Avail = strSystemAvail;
            objSystem.Source = "UN";
            objSystem.Page = "50";
            objNexus.Children.Add(objSystem);

            Gear objFirewall = new Gear(_objCharacter);
            objFirewall.Name = LanguageManager.Instance.GetString("String_Firewall") + " (" + LanguageManager.Instance.GetString("String_Rating") + " " + intFirewall.ToString() + ")";
            objFirewall.Category = "Nexus Module";
            objFirewall.Cost = "0";
            objFirewall.Avail = strFirewallAvail;
            objFirewall.Source = "UN";
            objFirewall.Page = "50";
            objNexus.Children.Add(objFirewall);

            Gear objPersona = new Gear(_objCharacter);
            objPersona.Name = LanguageManager.Instance.GetString("String_SelectNexus_PersonaLimit") + " (" + intPersona.ToString() + ")";
            objPersona.Category = "Nexus Module";
            objPersona.Cost = "0";
            objPersona.Avail = "0";
            objPersona.Source = "UN";
            objPersona.Page = "50";
            objNexus.Children.Add(objPersona);

            _objGear = objNexus;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblProcessorLabel.Width, lblSystemLabel.Width);
            intWidth = Math.Max(intWidth, lblResponseLabel.Width);
            intWidth = Math.Max(intWidth, lblFirewallLabel.Width);

            nudProcessor.Left = lblProcessorLabel.Left + intWidth + 6;
            nudSystem.Left = lblSystemLabel.Left + intWidth + 6;
            nudResponse.Left = lblResponseLabel.Left + intWidth + 6;
            nudFirewall.Left = lblFirewallLabel.Left + intWidth + 6;

            lblSystemAvailLabel.Left = nudSystem.Left + 48;
            lblSystemAvail.Left = lblSystemAvailLabel.Left + lblSystemAvailLabel.Width + 6;
            lblResponseAvailLabel.Left = nudResponse.Left + 48;
            lblResponseAvail.Left = lblResponseAvailLabel.Left + lblSystemAvailLabel.Width + 6;
            lblFirewallAvailLabel.Left = nudFirewall.Left + 48;
            lblFirewallAvail.Left = lblFirewallAvailLabel.Left + lblFirewallAvailLabel.Width + 6;

            intWidth = Math.Max(lblSignalLabel.Width, lblPersonaLimitLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);

            lblSignalLabel.Left = lblSystemAvail.Left + 36;
            nudSignal.Left = lblSignalLabel.Left + intWidth + 6;
            lblPersonaLimitLabel.Left = lblSystemAvail.Left + 36;
            nudPersona.Left = lblPersonaLimitLabel.Left + intWidth + 6;
            lblCostLabel.Left = lblSystemAvail.Left + 36;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;
            chkFreeItem.Left = lblSystemAvailLabel.Left + 36;
        }
        #endregion
    }
}