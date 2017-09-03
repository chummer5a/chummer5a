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
        public event PropertyChangedEventHandler ChildPropertyChanged; 

        private BindingListDisplay<Power> _powers;
        public PowersTabUserControl()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        public void MissingDatabindingsWorkaround()
        {
            //TODO: Databind this
            CalculatePowerPoints();
        }

        private bool _loadCalled;
        private bool _initialized;
        private Character _character;
        private List<Tuple<string, Predicate<Power>>> _dropDownList;
        private List<Tuple<string, IComparer<Power>>>  _sortList;
        private bool _searchMode;

        public Character ObjCharacter
        {
            set
            {
                _character = value;
                RealLoad();
            }
            get { return _character; }
        }

        private void PowersTabUserControl_Load(object sender, EventArgs e)
        {
            _loadCalled = true;
            RealLoad();
        }

        private void RealLoad() //Cannot be called before both Loaded are called and it have a character object
        {
            if (_initialized) return;

            if (!(_character != null && _loadCalled)) return;

            _initialized = true;  //Only do once
            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release 
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This 
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            //this.SuspendLayout();
            MakePowerDisplays();

            parts.TaskEnd("MakePowerDisplay()");

            _dropDownList = GenerateDropdownFilter();

            parts.TaskEnd("GenerateDropDown()");

            _sortList = GenerateSortList();

            parts.TaskEnd("GenerateSortList()");

            
            cboDisplayFilter.DataSource = _dropDownList;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.SelectedIndex = 0;
            cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

            parts.TaskEnd("_ddl databind");

            cboSort.DataSource = _sortList;
            cboSort.ValueMember = "Item2";
            cboSort.DisplayMember = "Item1";
            cboSort.SelectedIndex = 0;
            cboSort.MaxDropDownItems = _sortList.Count;

            parts.TaskEnd("_sort databind");

            _powers.ChildPropertyChanged += ChildPropertyChanged;

            //Visible = true;
            //this.ResumeLayout(false);
            //this.PerformLayout();
            parts.TaskEnd("visible");
            Panel1_Resize();
            parts.TaskEnd("resize");
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
            //this.Update();
            //this.ResumeLayout(true);
            //this.PerformLayout();
        }

        private List<Tuple<string, IComparer<Power>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Power>>> ret = new List<Tuple<string, IComparer<Power>>>()
            {
                new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
                    new PowerSorter((x, y) => x.DisplayName.CompareTo(y.DisplayName))),
                new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortRating"),
                    new PowerSorter((x, y) => y.Rating.CompareTo(x.Rating))),
                new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Power_SortAction"),
                    new PowerSorter((x, y) => y.DisplayAction.CompareTo(x.DisplayAction))),
                //new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
                //    new PowerSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),
            };

            return ret;
        }
        
        private List<Tuple<string, Predicate<Power>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Power>>> ret = new List<Tuple<string, Predicate<Power>>>
            {
                new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_Search"), null),
                new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_PowerFilterAll"), power => true),
                new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_PowerFilterRatingAboveZero"),
                    power => power.Rating > 0),
                new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_PowerFilterRatingZero"),
                    power => power.Rating == 0)
            };
            //TODO: TRANSLATIONS

            ret.AddRange(
                from XmlNode objNode 
                in XmlManager.Instance.Load("powers.xml").SelectNodes("/chummer/categories/category")
                let displayName = objNode.Attributes?["translate"]?.InnerText ?? objNode.InnerText
                select new Tuple<string, Predicate<Power>>(
                    $"{LanguageManager.Instance.GetString("Label_Category")} {displayName}", 
                    power => power.Category == objNode.InnerText));

            return ret;
        }

        private void MakePowerDisplays()
        {
            Stopwatch sw = Stopwatch.StartNew();

            _powers = new BindingListDisplay<Power>(_character.Powers,
                power => new PowerControl(power))
            {
                Location = new Point(3, 3),
            };
            pnlPowers.Controls.Add(_powers);

            sw.TaskEnd("_powers add");

        }

        private void Panel1_Resize()
        {
            int height = pnlPowers.Height;
            const int intWidth = 255;
            if (_powers == null) return;
            _powers.Height = height - _powers.Top;
            _powers.Size = new Size(pnlPowers.Width - (intWidth+10), pnlPowers.Height - 39);
        }

        private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox csender = (ComboBox) sender;
            Tuple<string, Predicate<Power>> selectedItem = (Tuple<string, Predicate<Power>>)csender.SelectedItem;

            if (selectedItem.Item2 == null)
            {
                csender.DropDownStyle = ComboBoxStyle.DropDown;
                _searchMode = true;
                
            }
            else
            {
                csender.DropDownStyle = ComboBoxStyle.DropDownList;
                _searchMode = false;
                _powers.Filter(selectedItem.Item2);
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox csender = (ComboBox)sender;
            Tuple<string, IComparer<Power>> selectedItem = (Tuple<string, IComparer<Power>>)csender.SelectedItem;

            _powers.Sort(selectedItem.Item2);
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_searchMode)
            {
                _powers.Filter(skill => CultureInfo.InvariantCulture.CompareInfo.IndexOf(skill.DisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }

        private void cmdAddPower_Click(object sender, EventArgs e)
        {
            frmSelectPower frmPickPower = new frmSelectPower(ObjCharacter);
            frmPickPower.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickPower.DialogResult == DialogResult.Cancel)
                return;

            Power objPower = new Power(ObjCharacter);

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");

            XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + frmPickPower.SelectedPower + "\"]");
            objPower.Create(objXmlPower, ObjCharacter.ObjImprovementManager);
            ObjCharacter.Powers.Add(objPower);
            MissingDatabindingsWorkaround();
            if (frmPickPower.AddAgain)
                cmdAddPower_Click(sender, e);
        }

        /// <summary>
        /// Calculate the number of Adept Power Points used.
        /// </summary>
        public void CalculatePowerPoints()
        {
            lblPowerPoints.Text = String.Format("{1} ({0} " + LanguageManager.Instance.GetString("String_Remaining") + ")", PowerPointsRemaining, PowerPointsTotal);
            ValidateVisibility();
        }

        private int PowerPointsTotal
        {
            get
            {
                int intMAG;
                if (ObjCharacter.AdeptEnabled && ObjCharacter.MagicianEnabled)
                {
                    // If both Adept and Magician are enabled, this is a Mystic Adept, so use the MAG amount assigned to this portion.
                    intMAG = ObjCharacter.MysticAdeptPowerPoints;
                }
                else
                {
                    // The character is just an Adept, so use the full value.
                    intMAG = ObjCharacter.MAG.TotalValue;
                }

                // Add any Power Point Improvements to MAG.
                intMAG += ObjCharacter.ObjImprovementManager.ValueOf(Improvement.ImprovementType.AdeptPowerPoints);

                return intMAG;
            }
        }

        private decimal PowerPointsRemaining
        {
            get
            {
                return PowerPointsTotal - ObjCharacter.Powers.Sum(objPower => objPower.PowerPoints);
            }
        }

        private void ValidateVisibility()
        {
            lblDiscountLabel.Visible = _character.Powers.Any(objPower => objPower.AdeptWayDiscountEnabled);
        }
    }
}
