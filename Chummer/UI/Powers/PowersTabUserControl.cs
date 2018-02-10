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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Powers;
using Chummer.UI.Shared;

// ReSharper disable StringCompareToIsCultureSpecific

namespace Chummer.UI.Powers
{
    public partial class PowersTabUserControl : UserControl
    {
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate; 

        private BindingListDisplay<Power> _powers;
        public PowersTabUserControl()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _dropDownList = GenerateDropdownFilter();
            _sortList = GenerateSortList();
        }

        public void MissingDatabindingsWorkaround()
        {
            //TODO: Databind this
            CalculatePowerPoints();
        }
        
        private Character _objCharacter;
        private readonly IList<Tuple<string, Predicate<Power>>> _dropDownList;
        private readonly IList<Tuple<string, IComparer<Power>>>  _sortList;
        private bool _blnSearchMode;
        
        private void PowersTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter == null)
            {
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.WaitCursor;
                RealLoad();
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.Default;
            }
        }
        
        public void RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                Utils.BreakIfDebug();
                _objCharacter = new Character();
            }

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release 
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This 
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            SuspendLayout();
            DoubleBuffered = true;

            CalculatePowerPoints();

            _powers = new BindingListDisplay<Power>(_objCharacter.Powers, power => new PowerControl(power))
            {
                Location = new Point(3, 3),
            };
            pnlPowers.Controls.Add(_powers);

            parts.TaskEnd("MakePowerDisplay()");

            cboDisplayFilter.DataSource = _dropDownList;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.SelectedIndex = 1;
            cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

            parts.TaskEnd("_ddl databind");

            cboSort.DataSource = _sortList;
            cboSort.ValueMember = "Item2";
            cboSort.DisplayMember = "Item1";
            cboSort.SelectedIndex = 0;
            cboSort.MaxDropDownItems = _sortList.Count;

            parts.TaskEnd("_sort databind");

            _powers.ChildPropertyChanged += RefreshPowerInfo;
            _powers.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;

            //Visible = true;
            //this.ResumeLayout(false);
            //this.PerformLayout();
            parts.TaskEnd("visible");

            _powers.Height = pnlPowers.Height - _powers.Top;
            _powers.Width = pnlPowers.Width - _powers.Left;

            parts.TaskEnd("resize");
            //this.Update();
            ResumeLayout(true);
            //this.PerformLayout();
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        private static IList<Tuple<string, IComparer<Power>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Power>>> ret = new List<Tuple<string, IComparer<Power>>>()
            {
                new Tuple<string, IComparer<Power>>(LanguageManager.GetString("Skill_SortAlphabetical", GlobalOptions.Language),
                    new PowerSorter((x, y) => x.DisplayName.CompareTo(y.DisplayName))),
                new Tuple<string, IComparer<Power>>(LanguageManager.GetString("Skill_SortRating", GlobalOptions.Language),
                    new PowerSorter((x, y) => y.TotalRating.CompareTo(x.TotalRating))),
                new Tuple<string, IComparer<Power>>(LanguageManager.GetString("Power_SortAction", GlobalOptions.Language),
                    new PowerSorter((x, y) => y.DisplayAction.CompareTo(x.DisplayAction))),
                //new Tuple<string, IComparer<Power>>(LanguageManager.GetString("Skill_SortCategory"),
                //    new PowerSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),
            };

            return ret;
        }
        
        private static IList<Tuple<string, Predicate<Power>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Power>>> ret = new List<Tuple<string, Predicate<Power>>>
            {
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_Search", GlobalOptions.Language), null),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterAll", GlobalOptions.Language), power => true),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingAboveZero", GlobalOptions.Language), power => power.Rating > 0),
                new Tuple<string, Predicate<Power>>(LanguageManager.GetString("String_PowerFilterRatingZero", GlobalOptions.Language), power => power.Rating == 0)
            };
            //TODO: TRANSLATIONS

            using (XmlNodeList xmlPowerCategoryList = XmlManager.Load("powers.xml").SelectNodes("/chummer/categories/category"))
                if (xmlPowerCategoryList != null)
                    foreach (XmlNode xmlCategoryNode in xmlPowerCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<Power>>(
                            $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName}",
                            power => power.Category == strName));
                    }

            return ret;
        }
        
        private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDisplayFilter.SelectedItem is Tuple<string, Predicate<Power>> selectedItem)
            {
                if (selectedItem.Item2 == null)
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDown;
                    _blnSearchMode = true;
                    cboDisplayFilter.Text = string.Empty;
                }
                else
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                    _blnSearchMode = false;
                    _powers.Filter(selectedItem.Item2);
                }
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSort.SelectedItem is Tuple<string, IComparer<Power>> selectedItem)
                _powers.Sort(selectedItem.Item2);
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnSearchMode)
            {
                _powers.Filter(skill => GlobalOptions.InvariantCultureInfo.CompareInfo.IndexOf(skill.DisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }

        private void cmdAddPower_Click(object sender, EventArgs e)
        {
            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
            bool blnAddAgain;

            do
            {
                frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
                frmPickPower.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickPower.DialogResult == DialogResult.Cancel)
                {
                    frmPickPower.Dispose();
                    break;
                }
                blnAddAgain = frmPickPower.AddAgain;

                Power objPower = new Power(_objCharacter);

                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + frmPickPower.SelectedPower + "\"]");
                frmPickPower.Dispose();
                if (objPower.Create(objXmlPower))
                {
                    _objCharacter.Powers.Add(objPower);
                    MissingDatabindingsWorkaround();
                }
            }
            while (blnAddAgain);
        }

        public void RefreshPowerInfo(object sender, EventArgs e)
        {
            CalculatePowerPoints();
        }

        /// <summary>
        /// Calculate the number of Adept Power Points used.
        /// </summary>
        public void CalculatePowerPoints()
        {
            int intPowerPointsTotal = PowerPointsTotal;
            decimal decPowerPointsRemaining = intPowerPointsTotal - _objCharacter.Powers.AsParallel().Sum(objPower => objPower.PowerPoints);
            lblPowerPoints.Text = string.Format("{0} ({1} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')', intPowerPointsTotal, decPowerPointsRemaining);
            lblDiscountLabel.Visible = _objCharacter.Powers.Any(objPower => objPower.AdeptWayDiscountEnabled);
        }

        private int PowerPointsTotal
        {
            get
            {
                int intMAG;
                if (_objCharacter.IsMysticAdept)
                {
                    // If both Adept and Magician are enabled, this is a Mystic Adept, so use the MAG amount assigned to this portion.
                    intMAG = _objCharacter.Options.MysAdeptSecondMAGAttribute ? _objCharacter.MAGAdept.TotalValue : _objCharacter.MysticAdeptPowerPoints;
                }
                else
                {
                    // The character is just an Adept, so use the full value.
                    intMAG = _objCharacter.MAG.TotalValue;
                }

                // Add any Power Point Improvements to MAG.
                intMAG += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.AdeptPowerPoints);

                return Math.Max(intMAG, 0);
            }
        }
    }
}
