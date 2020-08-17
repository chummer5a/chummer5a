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
 using System.Text;
 using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public sealed partial class frmSelectCyberwareSuite : Form
    {
        private string _strSelectedSuite = string.Empty;
        private readonly Improvement.ImprovementSource _eSource;
        private readonly string _strType;
        private readonly Character _objCharacter;
        private decimal _decCost;

        private readonly XmlDocument _objXmlDocument;

        #region Control events
        public frmSelectCyberwareSuite(Character objCharacter, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Cyberware)
        {
            InitializeComponent();
            _eSource = eSource;
            this.TranslateWinForm();

            if (_eSource == Improvement.ImprovementSource.Cyberware)
                _strType = "cyberware";
            else
            {
                _strType = "bioware";
                Text = LanguageManager.GetString("Title_SelectBiowareSuite");
                lblCyberwareLabel.Text = LanguageManager.GetString("Label_SelectBiowareSuite_PartsInSuite");
            }

            _objCharacter = objCharacter;
            _objXmlDocument = XmlManager.Load(_strType + ".xml", string.Empty, true);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstCyberware_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void frmSelectCyberwareSuite_Load(object sender, EventArgs e)
        {
            if (_objCharacter.IsAI)
                return;

            List<Grade> lstGrades = _objCharacter.GetGradeList(_eSource);

            using (XmlNodeList xmlSuiteList = _objXmlDocument.SelectNodes("/chummer/suites/suite"))
                if (xmlSuiteList?.Count > 0)
                    foreach (XmlNode objXmlSuite in xmlSuiteList)
                    {
                        string strName = objXmlSuite["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            string strGrade = objXmlSuite["grade"]?.InnerText ?? string.Empty;
                            if (string.IsNullOrEmpty(strGrade) && (lstGrades.All(x => x.Name != strGrade) ||
                                                                   _objCharacter.Improvements.Any(x =>
                                                                       ((_eSource == Improvement.ImprovementSource.Cyberware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade) ||
                                                                        (_eSource == Improvement.ImprovementSource.Bioware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade))
                                                                       && strGrade.Contains(x.ImprovedName) && x.Enabled)))
                                continue;
                            lstCyberware.Items.Add(new ListItem(objXmlSuite["id"]?.InnerText ?? strName, strName));
                        }
                    }
            lstCyberware.ValueMember = nameof(ListItem.Value);
            lstCyberware.DisplayMember = nameof(ListItem.Name);
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedSuite = lstCyberware.SelectedItem?.ToString();
            XmlNode xmlSuite = null;
            string strGrade;
            Grade objGrade = null;
            if (strSelectedSuite != null)
            {
                xmlSuite = _objXmlDocument.SelectSingleNode("/chummer/suites/suite[id = \"" + strSelectedSuite + "\"]");
                string strSuiteGradeEntry = xmlSuite?["grade"]?.InnerText;
                if (!string.IsNullOrEmpty(strSuiteGradeEntry))
                {
                    strGrade = CyberwareGradeName(strSuiteGradeEntry);
                    if (!string.IsNullOrEmpty(strGrade))
                    {
                        objGrade = _objCharacter.GetGradeList(_eSource).FirstOrDefault(x => x.Name == strGrade);
                    }
                }
            }

            if (objGrade == null)
            {
                lblCyberware.Text = string.Empty;
                lblEssence.Text = string.Empty;
                lblCost.Text = string.Empty;
                lblGrade.Text = string.Empty;
                return;
            }

            decimal decTotalESS = 0.0m;
            decimal decTotalCost = 0;

            List<Cyberware> lstSuiteCyberwares = new List<Cyberware>();
            ParseNode(xmlSuite, objGrade, lstSuiteCyberwares);
            StringBuilder sbdCyberwareLabelString = new StringBuilder();
            foreach (Cyberware objCyberware in lstSuiteCyberwares)
            {
                WriteList(sbdCyberwareLabelString, objCyberware, 0);
                decTotalCost += objCyberware.TotalCost;
                decTotalESS += objCyberware.CalculatedESS;
            }

            lblCyberware.Text = sbdCyberwareLabelString.ToString();
            lblEssence.Text = decTotalESS.ToString(_objCharacter.Options.EssenceFormat, GlobalOptions.CultureInfo);
            lblCost.Text = decTotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            lblGrade.Text = objGrade.CurrentDisplayName;
            _decCost = decTotalCost;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name of Suite that was selected in the dialogue.
        /// </summary>
        public string SelectedSuite => _strSelectedSuite;

        /// <summary>
        /// Total cost of the Cyberware Suite. This is done to make it easier to obtain the actual cost in Career Mode.
        /// </summary>
        public decimal TotalCost => _decCost;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstCyberware.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedSuite = strSelectedId;
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Convert the grade string found in the file to the name of the Grade found in the Cyberware.xml file.
        /// </summary>
        /// <param name="strValue">Grade from the Cyberware Suite.</param>
        private static string CyberwareGradeName(string strValue)
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
        /// <param name="xmlSuite">XmlNode to parse.</param>
        /// <param name="objGrade">Grade that the Cyberware should be created with.</param>
        /// <param name="lstChildren">List for children to which child items should be assigned.</param>
        private void ParseNode(XmlNode xmlSuite, Grade objGrade, IList<Cyberware> lstChildren)
        {
            // Run through all of the items in the Suite list.
            using (XmlNodeList xmlChildrenList = xmlSuite.SelectNodes(_strType + "s/" + _strType))
                if (xmlChildrenList?.Count > 0)
                    foreach (XmlNode xmlChildItem in xmlChildrenList)
                    {
                        int intRating = 0;
                        xmlChildItem.TryGetInt32FieldQuickly("rating", ref intRating);
                        string strName = string.Empty;
                        xmlChildItem.TryGetStringFieldQuickly("name", ref strName);

                        // Retrieve the information for the current piece of Cyberware and add it to the ESS and Cost totals.
                        XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strType + "s/" + _strType + "[name = \"" + strName + "\"]");

                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<Vehicle> lstVehicles = new List<Vehicle>();
                        Cyberware objCyberware = new Cyberware(_objCharacter);
                        objCyberware.Create(objXmlCyberware, objGrade, _eSource, intRating, lstWeapons, lstVehicles, false, false);
                        objCyberware.Suite = true;

                        lstChildren.Add(objCyberware);

                        ParseNode(xmlChildItem, objGrade, objCyberware.Children);
                    }
        }

        /// <summary>
        /// Write out the Cyberware in the list to the label.
        /// </summary>
        /// <param name="objCyberwareLabelString">StringBuilder into which the cyberware list is written.</param>
        /// <param name="objCyberware">Cyberware to iterate through.</param>
        /// <param name="intDepth">Current dept in the list to determine how many spaces to print.</param>
        private void WriteList(StringBuilder objCyberwareLabelString, Cyberware objCyberware, int intDepth)
        {
            for (int i = 0; i <= intDepth; ++i)
                objCyberwareLabelString.Append("   ");

            objCyberwareLabelString.AppendLine(objCyberware.CurrentDisplayName);

            foreach (Cyberware objPlugin in objCyberware.Children)
                WriteList(objCyberwareLabelString, objPlugin, intDepth + 1);
        }
        #endregion
    }
}
