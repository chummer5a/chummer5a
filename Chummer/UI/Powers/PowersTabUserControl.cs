using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.UI.Shared;

namespace Chummer.UI.Skills
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
			CalculatePowerPoints();
		}

		private bool _loadCalled = false;
		private bool _initialized = false;
		private Character _character;
		private List<Tuple<string, Predicate<Power>>> _dropDownList;
		private List<Tuple<string, IComparer<Power>>>  _sortList;
		private List<PowerControl> controls = new List<PowerControl>();
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

		private void SkillsTabUserControl_Load(object sender, EventArgs e)
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
			MakeSkillDisplays();

			parts.TaskEnd("MakeSkillDisplay()");

			_dropDownList = GenerateDropdownFilter();

			parts.TaskEnd("GenerateDropDown()");

			_sortList = GenerateSortList();

			parts.TaskEnd("GenerateSortList()");

			/*
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

			parts.TaskEnd("_sort databind");*/

			_powers.ChildPropertyChanged += ChildPropertyChanged;

			//Visible = true;
			//this.ResumeLayout(false);
			//this.PerformLayout();
			parts.TaskEnd("visible");
			Panel1_Resize(null, null);
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
				/*new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
					new SkillSorter(SkillsSection.CompareSkills)),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortRating"),
					new SkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortDicepool"),
					new SkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortLowerDicepool"),
					new SkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortAttributeValue"),
					new SkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortAttributeName"),
					new SkillSorter((x, y) => x.Attribute.CompareTo(y.Attribute))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortGroupName"),
					new SkillSorter((x, y) => y.SkillGroup.CompareTo(x.SkillGroup))),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortGroupRating"),
					new SkillSortBySkillGroup()),
				new Tuple<string, IComparer<Power>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
					new SkillSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),*/
			};

			return ret;
		}
		
		private List<Tuple<string, Predicate<Power>>> GenerateDropdownFilter()
		{
			List<Tuple<string, Predicate<Power>>> ret = new List<Tuple<string, Predicate<Power>>>
			{
				new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_Search"), null),
				new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_SkillFilterAll"), skill => true),
				new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_SkillFilterRatingAboveZero"),
					skill => skill.Rating > 0),
				new Tuple<string, Predicate<Power>>(LanguageManager.Instance.GetString("String_SkillFilterRatingZero"),
					skill => skill.Rating == 0)
			};
			//TODO: TRANSLATIONS

			ret.AddRange(
				from XmlNode objNode 
				in XmlManager.Instance.Load("powers.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]")
				let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
				select new Tuple<string, Predicate<Power>>(
					$"{LanguageManager.Instance.GetString("Label_Category")} {displayName}", 
					power => power.Category == objNode.InnerText));

			return ret;
		}

		private void MakeSkillDisplays()
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

		private void Panel1_Resize(object sender, EventArgs e)
		{
			int height = pnlPowers.Height;
			int intWidth = 255;
			if (_powers != null)
			{
				_powers.Height = height - _powers.Top;
				_powers.Size = new Size(pnlPowers.Width - (intWidth+10), pnlPowers.Height - 39);
			}
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
			CalculatePowerPoints();
			if (frmPickPower.AddAgain)
				cmdAddPower_Click(sender, e);
		}



		/// <summary>
		/// Calculate the number of Adept Power Points used.
		/// </summary>
		private void CalculatePowerPoints()
		{
			decimal decPowerPoints = 0;

			foreach (Power objPower in ObjCharacter.Powers)
			{
				decPowerPoints += objPower.PowerPoints;
			}

			int intMAG = 0;
			if (ObjCharacter.AdeptEnabled && ObjCharacter.MagicianEnabled)
			{
				// If both Adept and Magician are enabled, this is a Mystic Adept, so use the MAG amount assigned to this portion.
				intMAG = ObjCharacter.MAGAdept;
			}
			else
			{
				// The character is just an Adept, so use the full value.
				intMAG = ObjCharacter.MAG.TotalValue;
			}

			// Add any Power Point Improvements to MAG.
			intMAG += ObjCharacter.ObjImprovementManager.ValueOf(Improvement.ImprovementType.AdeptPowerPoints);

			string strRemain = (intMAG - decPowerPoints).ToString();
			while (strRemain.EndsWith("0") && strRemain.Length > 4)
				strRemain = strRemain.Substring(0, strRemain.Length - 1);

			lblPowerPoints.Text = String.Format("{1} ({0} " + LanguageManager.Instance.GetString("String_Remaining") + ")", strRemain, intMAG);
		}
	}
}
