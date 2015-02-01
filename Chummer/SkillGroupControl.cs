using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

// GroupRatingChanged Event Handler.
public delegate void GroupRatingChangedHandler(Object sender);
public delegate void GroupKaramClickHandler(Object sender);

namespace Chummer
{
    public partial class SkillGroupControl : UserControl
    {
		private SkillGroup _objSkillGroup;
		private readonly bool _blnCareer = false;
		private bool _blnIsEnabled = true;
        private int _intWorkingRating = 0;

        // GroupRatingChanged Event.
        public event GroupRatingChangedHandler GroupRatingChanged;
		public event GroupKaramClickHandler GroupKarmaClicked;
		
		private readonly CharacterOptions _objOptions;

		#region Control Events
		public SkillGroupControl(CharacterOptions objOptions, bool blnCareer = false)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_blnCareer = blnCareer;
			_objOptions = objOptions;
        }

        private void SkillControl_Load(object sender, EventArgs e)
        {
			nudSkill.Maximum = _objSkillGroup.RatingMaximum;

			if (_blnCareer)
			{
				if (_objSkillGroup.RatingMaximum < 12)
					_objSkillGroup.RatingMaximum = 12;
				nudSkill.Maximum = _objSkillGroup.RatingMaximum;
				nudSkill.Visible = false;
                nudKarma.Visible = false;
				cmdImproveSkill.Visible = true;
				lblGroupRating.Visible = true;
			}

            if (_objSkillGroup.FreeLevels > 0)
                nudSkill.Minimum = _objSkillGroup.FreeLevels;
            else
                nudSkill.Minimum = 0;

			if (_objSkillGroup.Broken)
				IsEnabled = false;
            this.Width = nudKarma.Left + nudKarma.Width;
        }

		private void nudSkill_ValueChanged(object sender, EventArgs e)
        {
            // Raise the GroupRatingChanged Event when the NumericUpDown's Value changes.
            // The entire SkillGroupControl is passed as an argument so the handling event can evaluate its contents.
            if (nudSkill.Value + nudKarma.Value > nudSkill.Maximum)
                nudSkill.Value = nudSkill.Maximum - nudKarma.Value;
            _objSkillGroup.Base = Convert.ToInt32(nudSkill.Value);
			_objSkillGroup.Rating = Convert.ToInt32(nudSkill.Value) + Convert.ToInt32(nudKarma.Value);
            GroupRatingChanged(this);
        }

        private void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            if (nudSkill.Value + nudKarma.Value > nudSkill.Maximum)
                nudKarma.Value = nudSkill.Maximum - nudSkill.Value;
            _objSkillGroup.Karma = Convert.ToInt32(nudKarma.Value);
            _objSkillGroup.Rating = Convert.ToInt32(nudSkill.Value) + Convert.ToInt32(nudKarma.Value);
            GroupRatingChanged(this);
        }

        private void txtSkillName_Enter(object sender, EventArgs e)
        {
            // If the SkillName TextBox is ReadOnly, jump to the Rating Control.
            //if (txtGroupName.ReadOnly)
            //    nudSkill.Focus();
            this.Focus();
        }

		private void cmdImproveSkill_Click(object sender, EventArgs e)
		{
			// Raise the GroupKarmaClicked Even when the Improve button is clicked.
			// The entire SkillGroupControl is passed as an argument so the handling event can evaluate its contents.
			GroupKarmaClicked(this);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Skill Group object that this control is linked to.
		/// </summary>
		public SkillGroup SkillGroupObject
		{
			get
			{
				return _objSkillGroup;
			}
			set
			{
				_objSkillGroup = value;
			}
		}

        /// <summary>
        /// Skill Group name.
        /// </summary>
        public string GroupName
        {
            get
            {
				return _objSkillGroup.Name;
            }
            set
            {
				_objSkillGroup.Name = value;
				txtGroupName.Text = _objSkillGroup.DisplayName;

				// Add a Tooltip that lists all of the skills that belong to this Skill Group.
				XmlDocument objXmlDocument = new XmlDocument();
				objXmlDocument = XmlManager.Instance.Load("skills.xml");

				XmlNodeList objXmlGroupList = objXmlDocument.SelectNodes("/chummer/skills/skill[skillgroup = \"" + value + "\"]");
				string strTooltip = LanguageManager.Instance.GetString("Tip_SkillGroup_Skills") + " ";
				foreach (XmlNode objXmlSkill in objXmlGroupList)
				{
					if (objXmlSkill["translate"] != null)
						strTooltip += objXmlSkill["translate"].InnerText + ", ";
					else
						strTooltip += objXmlSkill["name"].InnerText + ", ";
				}
				strTooltip = strTooltip.Substring(0, strTooltip.Length - 2);
				tipTooltip.SetToolTip(txtGroupName, strTooltip);
            }
        }

        /// <summary>
        /// Minimum Group Rating from Free Levels granted by Priority selection
        /// </summary>
        public int GroupRatingMinimum
        {
            get
            {
                return _objSkillGroup.FreeLevels;
            }
        }

        /// <summary>
        /// Working rating used when evaluating skill group costs
        /// </summary>
        public int GroupWorkingRating
        {
            get
            {
                return _intWorkingRating;
            }
            set
            {
                _intWorkingRating = value;
            }
        }

        /// <summary>
        /// Group Rating.
        /// </summary>
        public int GroupRating
        {
            get
            {
				return _objSkillGroup.Rating;
            }
            set
            {
                nudSkill.Value = value;
				lblGroupRating.Text = value.ToString();
				_objSkillGroup.Rating = value;

				if (value < _objSkillGroup.RatingMaximum)
				{
					string strTooltip = "";
					if (value == 0)
						strTooltip += LanguageManager.Instance.GetString("Tip_ImproveItem").Replace("{0}", (value + 1).ToString()).Replace("{1}", _objOptions.KarmaNewSkillGroup.ToString());
					else
						strTooltip += LanguageManager.Instance.GetString("Tip_ImproveItem").Replace("{0}", (value + 1).ToString()).Replace("{1}", ((value + 1) * _objOptions.KarmaImproveSkillGroup).ToString());
					tipTooltip.SetToolTip(cmdImproveSkill, strTooltip);
					if (_blnIsEnabled)
						cmdImproveSkill.Enabled = true;
				}
				else
					cmdImproveSkill.Enabled = false;
            }
        }

        /// <summary>
        /// Base Group Rating.
        /// </summary>
        public int BaseRating
        {
            get
            {
                return _objSkillGroup.Base;
            }
            set
            {
                nudSkill.Value = value;
                lblGroupRating.Text = (value + nudKarma.Value).ToString();
                _objSkillGroup.Base = value;
            }
        }

        /// <summary>
        /// Karma Group Rating.
        /// </summary>
        public int KarmaRating
        {
            get
            {
                return _objSkillGroup.Karma;
            }
            set
            {
                nudKarma.Value = value;
                lblGroupRating.Text = (value + nudSkill.Value).ToString();
                _objSkillGroup.Karma = value;
            }
        }

        /// <summary>
        /// Maximum Group Rating.
        /// </summary>
        public int GroupRatingMaximum
        {
            get
            {
				return _objSkillGroup.RatingMaximum;
            }
            set
            {
                nudSkill.Maximum = value;
				_objSkillGroup.RatingMaximum = value;
            }

        }

		/// <summary>
		/// Whether or not the Skill Group has been broken by improving an individual Active Skill.
		/// </summary>
		public bool Broken
		{
			get
			{
				return _objSkillGroup.Broken;
			}
			set
			{
				_objSkillGroup.Broken = value;

				if (value)
				{
					// GroupRating = 0;
					IsEnabled = false;
				}
				else
				{
					IsEnabled = true;
				}
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Technical Active Skills.
		/// </summary>
		public bool HasTechnicalSkills
		{
			get
			{
				return _objSkillGroup.HasTechnicalSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Social Active Skills.
		/// </summary>
		public bool HasSocialSkills
		{
			get
			{
				return _objSkillGroup.HasSocialSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Physical Active Skills.
		/// </summary>
		public bool HasPhysicalSkills
		{
			get
			{
				return _objSkillGroup.HasPhysicalSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Magical Active Skills.
		/// </summary>
		public bool HasMagicalSkills
		{
			get
			{
				return _objSkillGroup.HasMagicalSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Resonance Active Skills.
		/// </summary>
		public bool HasResonanceSkills
		{
			get
			{
				return _objSkillGroup.HasResonanceSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Combat Active Skills.
		/// </summary>
		public bool HasCombatSkills
		{
			get
			{
				return _objSkillGroup.HasCombatSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group has any Vehicle Active Skills.
		/// </summary>
		public bool HasVehicleSkills
		{
			get
			{
				return _objSkillGroup.HasVehicleSkills;
			}
		}

		/// <summary>
		/// Whether or not the Skill Group is enabled.
		/// </summary>
		public bool IsEnabled
		{
			get
			{
				return _blnIsEnabled;
			}
			set
			{
				_blnIsEnabled = value;

				if (_blnIsEnabled)
				{
					txtGroupName.ForeColor = SystemColors.ControlText;
					lblGroupRating.ForeColor = SystemColors.ControlText;
					if (_objSkillGroup.Rating < _objSkillGroup.RatingMaximum)
						cmdImproveSkill.Enabled = true;
					nudSkill.Enabled = true;
                    nudKarma.Enabled = true;
				}
				else
				{
					txtGroupName.ForeColor = SystemColors.GrayText;
					lblGroupRating.ForeColor = SystemColors.GrayText;
					cmdImproveSkill.Enabled = false;
					nudSkill.Enabled = false;
                    nudKarma.Enabled = false;
                }
			}
		}
		#endregion

        #region Methods
        /// <summary>
        /// Resets the working rating used when calculating karma costs of skill groups
        /// </summary>
        public void ResetWorkingRating()
        {
            _intWorkingRating = _objSkillGroup.Rating;
        }
        #endregion

        private void txtGroupName_Click(object sender, System.EventArgs e)
        {
            this.Focus();
        }
    }
}