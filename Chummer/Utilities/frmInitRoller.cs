using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmInitRoller : Form
    {
        private int dice = 0;

        #region Control Events
        public frmInitRoller()
        {
            InitializeComponent();
            //LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            this.CenterToParent();
        }

        private void frmInitRoller_Load(object sender, EventArgs e)
        {
            lblDice.Text += " " + dice.ToString() + "D6: ";
            nudDiceResult.Maximum = dice * 6;
            nudDiceResult.Minimum = dice;
            MoveControls();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Number of dice that are rolled for the lifestyle.
        /// </summary>
        public int Dice
        {
            get
            {
                return dice;
            }
            set
            {
                dice = value;
            }
        }

        /// <summary>
        /// Window title.
        /// </summary>
        public string Title
        {
            set
            {
                this.Text = value;
            }
        }

        /// <summary>
        /// Description text.
        /// </summary>
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

        /// <summary>
        /// Dice roll result.
        /// </summary>
        public int Result
        {
            get
            {
                return Convert.ToInt32(nudDiceResult.Value);
            }
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            nudDiceResult.Left = lblDice.Left + lblDice.Width + 6;
            lblResult.Left = nudDiceResult.Left + nudDiceResult.Width + 6;
        }
        #endregion
    }
}
