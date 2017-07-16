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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectCyberwareSuite : Form
    {
        private string _strSelectedSuite = string.Empty;
        private double _dblCharacterESSModifier = 1.0;
        private Improvement.ImprovementSource _objSource = Improvement.ImprovementSource.Cyberware;
        private string _strType = "cyberware";
        private Character _objCharacter;
        private int _intCost = 0;

        List<Cyberware> _lstCyberware = new List<Cyberware>();

        private XmlDocument _objXmlDocument = new XmlDocument();

        #region Control events
        public frmSelectCyberwareSuite(Improvement.ImprovementSource objSource, Character objCharacter)
        {
            InitializeComponent();
            _objSource = objSource;
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            if (_objSource == Improvement.ImprovementSource.Cyberware)
                _strType = "cyberware";
            else
            {
                _strType = "bioware";
                Text = LanguageManager.Instance.GetString("Title_SelectBiowareSuite");
                lblCyberwareLabel.Text = LanguageManager.Instance.GetString("Label_SelectBiowareSuite_PartsInSuite");
            }

            _objCharacter = objCharacter;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstCyberware.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstCyberware_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstCyberware.Text))
                AcceptForm();
        }

        private void frmSelectCyberwareSuite_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            _objXmlDocument = XmlManager.Instance.Load(_strType + ".xml");

            if (_objCharacter.DEPEnabled)
                return;

            XmlNodeList objXmlSuiteList = _objXmlDocument.SelectNodes("/chummer/suites/suite");

            foreach (XmlNode objXmlSuite in objXmlSuiteList)
            {
                lstCyberware.Items.Add(objXmlSuite["name"].InnerText);
            }
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstCyberware.Text))
                return;

            _lstCyberware.Clear();

            XmlNode objXmlSuite = _objXmlDocument.SelectSingleNode("/chummer/suites/suite[name = \"" + lstCyberware.Text + "\"]");
            lblGrade.Text = objXmlSuite["grade"].InnerText;

            decimal decTotalESS = 0.0m;
            int intTotalCost = 0;

            // Retrieve the information for the selected Grade.
            XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = \"" + CyberwareGradeName(objXmlSuite["grade"].InnerText) + "\"]");

            XPathNavigator nav = _objXmlDocument.CreateNavigator();
            lblCyberware.Text = string.Empty;

            Grade objGrade = new Cyberware(_objCharacter).ConvertToCyberwareGrade(objXmlGrade["name"].InnerText, _objSource);
            ParseNode(objXmlSuite, objGrade, null);
            foreach (Cyberware objCyberware in _lstCyberware)
            {
                WriteList(objCyberware, 0);
                intTotalCost += objCyberware.TotalCost;
                decTotalESS += objCyberware.CalculatedESS();
            }

            lblEssence.Text = Math.Round(decTotalESS, _objCharacter.Options.EssenceDecimals).ToString(GlobalOptions.CultureInfo);
            lblCost.Text = $"{intTotalCost:###,###,##0¥}";
            _intCost = intTotalCost;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Essence cost multiplier from the character.
        /// </summary>
        public double CharacterESSMultiplier
        {
            set
            {
                _dblCharacterESSModifier = value;
            }
        }

        /// <summary>
        /// Name of Suite that was selected in the dialogue.
        /// </summary>
        public string SelectedSuite
        {
            get
            {
                return _strSelectedSuite;
            }
        }

        /// <summary>
        /// Total cost of the Cyberware Suite. This is done to make it easier to obtain the actual cost in Career Mode.
        /// </summary>
        public int TotalCost
        {
            get
            {
                return _intCost;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedSuite = lstCyberware.Text;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Convert the grade string found in the file to the name of the Grade found in the Cyberware.xml file.
        /// </summary>
        /// <param name="strValue">Grade from the Cyberware Suite.</param>
        private string CyberwareGradeName(string strValue)
        {
            switch (strValue)
            {
                case "Alphaware":
                    return "Alphaware";
                case "Betaware":
                    return "Betaware";
                case "Deltaware":
                    return "Deltaware";
                case "Standard (Used)":
                case "StandardSecondHand":
                    return "Standard (Used)";
                case "Alphaware (Used)":
                case "AlphawareSecondHand":
                    return "Alphaware (Used)";
                case "StandardAdapsin":
                    return "Standard (Adapsin)";
                case "AlphawareAdapsin":
                    return "Alphaware (Adapsin)";
                case "BetawareAdapsin":
                    return "Betaware (Adapsin)";
                case "DeltawareAdapsin":
                    return "Deltaware (Adapsin)";
                case "Standard (Used) (Adapsin)":
                case "StandardSecondHandAdapsin":
                    return "Standard (Used) (Adapsin)";
                case "Alphaware (Used) (Adapsin)":
                case "AlphawareSecondHandAdapsin":
                    return "Alphaware (Used) (Adapsin)";
                default:
                    return "Standard";
            }
        }

        /// <summary>
        /// Parse an XmlNode and create the Cyberware for it and its children, adding them to the list of Cyberware in the suite.
        /// </summary>
        /// <param name="objXmlSuite">XmlNode to parse.</param>
        /// <param name="objGrade">Grade that the Cyberware should be created with.</param>
        /// <param name="objParent">Parent that child items should be assigned to.</param>
        private void ParseNode(XmlNode objXmlSuite, Grade objGrade, Cyberware objParent)
        {
            // Run through all of the items in the Suite list.
            foreach (XmlNode objXmlItem in objXmlSuite.SelectNodes(_strType + "s/" + _strType))
            {
                int intRating = 0;
                if (objXmlItem["rating"] != null)
                {
                    intRating = Convert.ToInt32(objXmlItem["rating"].InnerText);
                }

                // Retrieve the information for the current piece of Cyberware and add it to the ESS and Cost totals.
                XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strType + "s/" + _strType + "[name = \"" + objXmlItem["name"].InnerText + "\"]");

                TreeNode objTreeNode = new TreeNode();
                List<Weapon> lstWeapons = new List<Weapon>();
                List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                List<Vehicle> objVehicles = new List<Vehicle>();
                List<TreeNode> objVehicleNodes = new List<TreeNode>();
                Cyberware objCyberware = new Cyberware(_objCharacter);
                objCyberware.Create(objXmlCyberware, _objCharacter, objGrade, _objSource, intRating, objTreeNode, lstWeapons, lstWeaponNodes, objVehicles, objVehicleNodes, false, false);
                objCyberware.Suite = true;

                if (objParent == null)
                    _lstCyberware.Add(objCyberware);
                else
                    objParent.Children.Add(objCyberware);

                ParseNode(objXmlItem, objGrade, objCyberware);
            }
        }

        /// <summary>
        /// Write out the Cyberware in the list to the label.
        /// </summary>
        /// <param name="objCyberware">Cyberware to iterate through.</param>
        /// <param name="intDepth">Current dept in the list to determine how many spaces to print.</param>
        private void WriteList(Cyberware objCyberware, int intDepth)
        {
            string strSpace = string.Empty;
            for (int i = 0; i <= intDepth; i++)
                strSpace += "   ";
                
            lblCyberware.Text += strSpace + objCyberware.DisplayName + "\n";

            foreach (Cyberware objPlugin in objCyberware.Children)
                WriteList(objPlugin, intDepth + 1);
        }
        #endregion
    }
}