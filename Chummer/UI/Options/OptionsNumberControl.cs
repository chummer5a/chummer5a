using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Classes;

namespace Chummer.UI.Options
{
	public partial class OptionsNumberControl : UserControl
	{
		public OptionsNumberControl(Option thisOption = null)
		{
			InitializeComponent();
			LinkedOption = thisOption;
			if (LinkedOption != null)
			{
				lblRuleDescriptionLabel.Text = LanguageManager.Instance.GetString(LinkedOption.DescriptionTag);
				if (!string.IsNullOrWhiteSpace(LinkedOption.ModifierTag))
				{
					lblRuleMultiplierLabel.Text = LanguageManager.Instance.GetString(LinkedOption.ModifierTag);
				}
				if (LinkedOption.Value != null)
				{
					nudRule.Value = Convert.ToDecimal(LinkedOption.Value);
				}
				MoveControls();
			}
		}

		public Option LinkedOption { get; set; }

		/// <summary>
		/// Moves the controls
		/// </summary>
		private void MoveControls()
		{
			string strDescription = lblRuleDescriptionLabel.Text;
			Font fntSelectedFont = lblRuleDescriptionLabel.Font;
			int intSize = TextRenderer.MeasureText(strDescription, fntSelectedFont).Width;

			strDescription = lblRuleMultiplierLabel.Text;
			fntSelectedFont = lblRuleMultiplierLabel.Font;
			intSize = Math.Max(intSize, TextRenderer.MeasureText(strDescription, fntSelectedFont).Width);

			lblRuleDescriptionLabel.Left = 2;
			lblRuleDescriptionLabel.Width = intSize;
			nudRule.Left = lblRuleDescriptionLabel.Right + 2;
			lblRuleMultiplierLabel.Left = nudRule.Right + 2;
			lblRuleMultiplierLabel.Width = intSize;
		}
	}
}
