using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Skills;

namespace Chummer.Classes
{
	class AddImprovementCollection
	{
		private Character _objCharacter;
		private readonly ImprovementManager _manager;

		public AddImprovementCollection(Character character, ImprovementManager manager, Improvement.ImprovementSource objImprovementSource, string sourceName, string strUnique, string forcedValue, string limitSelection, string selectedValue, bool blnConcatSelectedValue, string strFriendlyName, int intRating, Func<string, int, int> valueToInt, Action rollback)
		{
			_objCharacter = character;
			_manager = manager;
			this._objImprovementSource = objImprovementSource;
			SourceName = sourceName;
			_strUnique = strUnique;
			ForcedValue = forcedValue;
			LimitSelection = limitSelection;
			SelectedValue = selectedValue;
			this._blnConcatSelectedValue = blnConcatSelectedValue;
			this._strFriendlyName = strFriendlyName;
			this._intRating = intRating;
			ValueToInt = valueToInt;
			Rollback = rollback;
			Commit = manager.Commit;
		}

		public string SourceName;
		public string ForcedValue;
		public string LimitSelection;
		public string SelectedValue;

		private readonly Improvement.ImprovementSource _objImprovementSource;
		private readonly string _strUnique;
		private readonly bool _blnConcatSelectedValue;
		private readonly string _strFriendlyName;
		private readonly int _intRating;


		//Transplanted functions, delegate values to make reflection grabbing all methods less fool proof...
		private readonly Func<string, int, int> ValueToInt;
		private readonly Action Commit;
		private readonly Action Rollback;

		private void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
			string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
			int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
			int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false)
		{
			_manager.CreateImprovement(strImprovedName, objImprovementSource,
				strSourceName, objImprovementType, strUnique,
				intValue, intRating, intMinimum, intMaximum, intAugmented,
				intAugmentedMaximum, strExclude, blnAddToRating);
		}
		private bool CreateImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName,
			XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "")
		{
			return _manager.CreateImprovements(objImprovementSource, strSourceName, nodBonus, blnConcatSelectedValue, intRating,
				strFriendlyName);
		}


		#region

		public void qualitylevel(XmlNode bonusNode)
		{
			//List of qualities to work with
			Guid[] all =
			{
				Guid.Parse("9ac85feb-ae1e-4996-8514-3570d411e1d5"), //national
				Guid.Parse("d9479e5c-d44a-45b9-8fb4-d1e08a9487b2"), //dirty criminal
				Guid.Parse("318d2edd-833b-48c5-a3e1-343bf03848a5"), //Limited
				Guid.Parse("e00623e1-54b0-4a91-b234-3c7e141deef4") //Corp
			};

			//Add to list
			//retrive list
			//sort list
			//find active instance
			//if active = list[top]
			//	return
			//else
			//	remove active
			//  add list[top]
			//	set list[top] active
		}

		public void enableattribute(XmlNode bonusNode)
		{
			Log.Info("enableattribute");
			if (bonusNode["name"].InnerText == "MAG")
			{
				_objCharacter.MAGEnabled = true;
				Log.Info("Calling CreateImprovement for MAG");
				CreateImprovement("MAG", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
					"enableattribute", 0, 0);
			}
			else if (bonusNode["name"].InnerText == "RES")
			{
				_objCharacter.RESEnabled = true;
				Log.Info("Calling CreateImprovement for RES");
				CreateImprovement("RES", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
					"enableattribute", 0, 0);
			}
		}

		// Add an Attribute Replacement.
		public void replaceattributes(XmlNode bonusNode)
		{
			XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute");
			if (objXmlAttributes != null)
				foreach (XmlNode objXmlAttribute in objXmlAttributes)
				{
					Log.Info("replaceattribute");
					Log.Info("replaceattribute = " + bonusNode.OuterXml.ToString());
					// Record the improvement.
					int intMin = 0;
					int intMax = 0;

					// Extract the modifiers.
					if (objXmlAttribute.InnerXml.Contains("min"))
						intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
					if (objXmlAttribute.InnerXml.Contains("max"))
						intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
					string strAttribute = objXmlAttribute["name"].InnerText;

					Log.Info("Calling CreateImprovement");
					CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.ReplaceAttribute,
						_strUnique,
						0, 1, intMin, intMax, 0, 0);
				}
		}

		// Enable a special tab.
		public void enabletab(XmlNode bonusNode)
		{
			Log.Info("enabletab");
			foreach (XmlNode objXmlEnable in bonusNode.ChildNodes)
			{
				switch (objXmlEnable.InnerText)
				{
					case "magician":
						_objCharacter.MagicianEnabled = true;
						Log.Info("magician");
						CreateImprovement("Magician", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
							"enabletab", 0, 0);
						break;
					case "adept":
						_objCharacter.AdeptEnabled = true;
                        Log.Info("adept");
						CreateImprovement("Adept", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
							"enabletab",
							0, 0);
						break;
					case "technomancer":
						_objCharacter.TechnomancerEnabled = true;
						Log.Info("technomancer");
						CreateImprovement("Technomancer", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
							"enabletab", 0, 0);
						break;
					case "critter":
						_objCharacter.CritterEnabled = true;
						Log.Info("critter");
						CreateImprovement("Critter", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
							"enabletab", 0, 0);
						break;
					case "initiation":
						_objCharacter.InitiationEnabled = true;
						Log.Info("initiation");
						CreateImprovement("Initiation", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
							"enabletab", 0, 0);
						break;
				}
			}
		}

		// Select Restricted (select Restricted items for Fake Licenses).
		public void selectrestricted(XmlNode bonusNode)
		{
			Log.Info("selectrestricted");
			frmSelectItem frmPickItem = new frmSelectItem();
			frmPickItem.Character = _objCharacter;
			if (ForcedValue != string.Empty)
				frmPickItem.ForceItem = ForcedValue;
			frmPickItem.AllowAutoSelect = false;
			frmPickItem.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickItem.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickItem.SelectedItem;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			// Create the Improvement.
			Log.Info("Calling CreateImprovement");
			CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
				Improvement.ImprovementType.Restricted, _strUnique);
		}

		public void cyberseeker(XmlNode bonusNode)
		{
			//Check if valid attrib
			if (new string[] { "BOD", "AGI", "STR", "REA", "LOG", "CHA", "INT", "WIL", "BOX" }.Any(x => x == bonusNode.InnerText))
			{
				CreateImprovement(bonusNode.InnerText, _objImprovementSource, SourceName, Improvement.ImprovementType.Seeker,
					_strUnique, 0, 0, 0, 0, 0, 0);

			}
			else
			{
				Utils.BreakIfDebug();
			}

		}

		// Select a Skill.
		public void selectskill(XmlNode bonusNode)
		{
			//TODO this don't work
			Log.Info("selectskill");
			if (ForcedValue == "+2 to a Combat Skill")
				ForcedValue = "";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("_strForcedValue = " + ForcedValue);

			// Display the Select Skill window and record which Skill was selected.
			frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
			if (_strFriendlyName != "")
				frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
					.Replace("{0}", _strFriendlyName);
			else
				frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

			Log.Info("selectskill = " + bonusNode.OuterXml.ToString());
			if (bonusNode.OuterXml.Contains("skillgroup"))
				frmPickSkill.OnlySkillGroup = bonusNode.Attributes["skillgroup"].InnerText;
			else if (bonusNode.OuterXml.Contains("skillcategory"))
				frmPickSkill.OnlyCategory = bonusNode.Attributes["skillcategory"].InnerText;
			else if (bonusNode.OuterXml.Contains("excludecategory"))
				frmPickSkill.ExcludeCategory = bonusNode.Attributes["excludecategory"].InnerText;
			else if (bonusNode.OuterXml.Contains("limittoskill"))
				frmPickSkill.LimitToSkill = bonusNode.Attributes["limittoskill"].InnerText;
			else if (bonusNode.OuterXml.Contains("limittoattribute"))
				frmPickSkill.LinkedAttribute = bonusNode.Attributes["limittoattribute"].InnerText;

			if (ForcedValue != "")
			{
				frmPickSkill.OnlySkill = ForcedValue;
				frmPickSkill.Opacity = 0;
			}
			frmPickSkill.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickSkill.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}

			SelectedValue = frmPickSkill.SelectedSkill;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			// Find the selected Skill.
			foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
			{
				if (frmPickSkill.SelectedSkill.Contains("Exotic Melee Weapon") ||
					frmPickSkill.SelectedSkill.Contains("Exotic Ranged Weapon") ||
					frmPickSkill.SelectedSkill.Contains("Pilot Exotic Vehicle"))
				{
					if (objSkill.Name + " (" + objSkill.Specialization + ")" == frmPickSkill.SelectedSkill)
					{
						// We've found the selected Skill.
						if (bonusNode.InnerXml.Contains("val"))
						{
							Log.Info("Calling CreateImprovement");
							CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", _objImprovementSource, SourceName,
								Improvement.ImprovementType.Skill, _strUnique, ValueToInt(bonusNode["val"].InnerText, _intRating), 1,
								0, 0, 0, 0, "", blnAddToRating);
						}

						if (bonusNode.InnerXml.Contains("max"))
						{
							Log.Info("Calling CreateImprovement");
							CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", _objImprovementSource, SourceName,
								Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
								ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, "", blnAddToRating);
						}
					}
				}
				else
				{
					if (objSkill.Name == frmPickSkill.SelectedSkill)
					{
						// We've found the selected Skill.
						if (bonusNode.InnerXml.Contains("val"))
						{
							Log.Info("Calling CreateImprovement");
							CreateImprovement(objSkill.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.Skill,
								_strUnique,
								ValueToInt(bonusNode["val"].InnerText, _intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
						}

						if (bonusNode.InnerXml.Contains("max"))
						{
							Log.Info("Calling CreateImprovement");
							CreateImprovement(objSkill.Name, _objImprovementSource, SourceName, Improvement.ImprovementType.Skill,
								_strUnique,
								0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0, 0, "", blnAddToRating);
						}
					}
				}
			}
		}

		// Select a Skill Group.
		public void selectskillgroup(XmlNode bonusNode)
		{
			Log.Info("selectskillgroup");
			string strExclude = "";
			if (bonusNode.Attributes["excludecategory"] != null)
				strExclude = bonusNode.Attributes["excludecategory"].InnerText;

			frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
			if (_strFriendlyName != "")
				frmPickSkillGroup.Description =
					LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", _strFriendlyName);
			else
				frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (ForcedValue != "")
			{
				frmPickSkillGroup.OnlyGroup = ForcedValue;
				frmPickSkillGroup.Opacity = 0;
			}

			if (strExclude != string.Empty)
				frmPickSkillGroup.ExcludeCategory = strExclude;

			frmPickSkillGroup.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickSkillGroup.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}

			SelectedValue = frmPickSkillGroup.SelectedSkillGroup;

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			if (bonusNode.SelectSingleNode("bonus") != null)
			{
				Log.Info("Calling CreateImprovement");
				CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
					_strUnique, ValueToInt(bonusNode["bonus"].InnerText, _intRating), 1, 0, 0, 0, 0, strExclude,
					blnAddToRating);
			}
			else
			{
				Log.Info("Calling CreateImprovement");
				CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
					_strUnique, 0, 0, 0, 1, 0, 0, strExclude,
					blnAddToRating);
			}
		}

		public void selectattributes(XmlNode bonusNode)
		{
			foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("selectattribute"))
			{
				Log.Info("selectattribute");
				// Display the Select Attribute window and record which Skill was selected.
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				if (_strFriendlyName != "")
					frmPickAttribute.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
				else
					frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

				// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
				if (_objCharacter.MAGEnabled)
					frmPickAttribute.AddMAG();
				if (_objCharacter.RESEnabled)
					frmPickAttribute.AddRES();

				Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

				if (objXmlAttribute.InnerXml.Contains("<attribute>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("attribute"))
						strValue.Add(objSubNode.InnerText);
					frmPickAttribute.LimitToList(strValue);
				}

				if (bonusNode.InnerXml.Contains("<excludeattribute>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("excludeattribute"))
						strValue.Add(objSubNode.InnerText);
					frmPickAttribute.RemoveFromList(strValue);
				}

				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (ForcedValue != "")
					LimitSelection = ForcedValue;

				Log.Info("_strForcedValue = " + ForcedValue);
				Log.Info("_strLimitSelection = " + LimitSelection);

				if (LimitSelection != "")
				{
					frmPickAttribute.SingleAttribute(LimitSelection);
					frmPickAttribute.Opacity = 0;
				}

				frmPickAttribute.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickAttribute.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				/*SelectedValue = frmPickAttribute.SelectedAttribute;
				if (_blnConcatSelectedValue)
					SourceName += " (" + SelectedValue + ")";
				*/
				Log.Info("_strSelectedValue = " + frmPickAttribute.SelectedAttribute);
				Log.Info("SourceName = " + SourceName);

				// Record the improvement.
				int intMin = 0;
				int intAug = 0;
				int intMax = 0;
				int intAugMax = 0;

				// Extract the modifiers.
				if (objXmlAttribute.InnerXml.Contains("min"))
					intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
				if (objXmlAttribute.InnerXml.Contains("val"))
					intAug = Convert.ToInt32(objXmlAttribute["val"].InnerText);
				if (objXmlAttribute.InnerXml.Contains("max"))
					intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
				if (objXmlAttribute.InnerXml.Contains("aug"))
					intAugMax = Convert.ToInt32(objXmlAttribute["aug"].InnerText);

				string strAttribute = frmPickAttribute.SelectedAttribute;

				if (objXmlAttribute["affectbase"] != null)
					strAttribute += "Base";

				Log.Info("Calling CreateImprovement");
				CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
					_strUnique,
					0, 1, intMin, intMax, intAug, intAugMax);
			}
		}

		// Select an CharacterAttribute.
		public void selectattribute(XmlNode bonusNode)
		{
			Log.Info("selectattribute");
			// Display the Select Attribute window and record which Skill was selected.
			frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
			if (_strFriendlyName != "")
				frmPickAttribute.Description =
					LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
			else
				frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

			// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
			if (_objCharacter.MAGEnabled)
				frmPickAttribute.AddMAG();
			if (_objCharacter.RESEnabled)
				frmPickAttribute.AddRES();

			Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

			if (bonusNode.InnerXml.Contains("<attribute>"))
			{
				List<string> strValue = new List<string>();
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
					strValue.Add(objXmlAttribute.InnerText);
				frmPickAttribute.LimitToList(strValue);
			}

			if (bonusNode.InnerXml.Contains("<excludeattribute>"))
			{
				List<string> strValue = new List<string>();
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludeattribute"))
					strValue.Add(objXmlAttribute.InnerText);
				frmPickAttribute.RemoveFromList(strValue);
			}

			// Check to see if there is only one possible selection because of _strLimitSelection.
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (LimitSelection != "")
			{
				frmPickAttribute.SingleAttribute(LimitSelection);
				frmPickAttribute.Opacity = 0;
			}

			frmPickAttribute.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickAttribute.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickAttribute.SelectedAttribute;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			// Record the improvement.
			int intMin = 0;
			int intAug = 0;
			int intMax = 0;
			int intAugMax = 0;

			// Extract the modifiers.
			if (bonusNode.InnerXml.Contains("min"))
				intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("val"))
				intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("max"))
				intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("aug"))
				intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

			string strAttribute = frmPickAttribute.SelectedAttribute;

			if (bonusNode["affectbase"] != null)
				strAttribute += "Base";

			Log.Info("Calling CreateImprovement");
			CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
				_strUnique,
				0, 1, intMin, intMax, intAug, intAugMax);
		}

		// Select a Limit.
		public void selectlimit(XmlNode bonusNode)
		{
			Log.Info("selectlimit");
			// Display the Select Limit window and record which Limit was selected.
			frmSelectLimit frmPickLimit = new frmSelectLimit();
			if (_strFriendlyName != "")
				frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
					.Replace("{0}", _strFriendlyName);
			else
				frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

			Log.Info("selectlimit = " + bonusNode.OuterXml.ToString());

			if (bonusNode.InnerXml.Contains("<limit>"))
			{
				List<string> strValue = new List<string>();
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("limit"))
					strValue.Add(objXmlAttribute.InnerText);
				frmPickLimit.LimitToList(strValue);
			}

			if (bonusNode.InnerXml.Contains("<excludelimit>"))
			{
				List<string> strValue = new List<string>();
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludelimit"))
					strValue.Add(objXmlAttribute.InnerText);
				frmPickLimit.RemoveFromList(strValue);
			}

			// Check to see if there is only one possible selection because of _strLimitSelection.
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (LimitSelection != "")
			{
				frmPickLimit.SingleLimit(LimitSelection);
				frmPickLimit.Opacity = 0;
			}

			frmPickLimit.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickLimit.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickLimit.SelectedLimit;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			// Record the improvement.
			int intMin = 0;
			int intAug = 0;
			int intMax = 0;
			int intAugMax = 0;

			// Extract the modifiers.
			if (bonusNode.InnerXml.Contains("min"))
				intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("val"))
				intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("max"))
				intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
			if (bonusNode.InnerXml.Contains("aug"))
				intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

			string strLimit = frmPickLimit.SelectedLimit;

			if (bonusNode["affectbase"] != null)
				strLimit += "Base";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			LimitModifier objLimitMod = new LimitModifier(_objCharacter);
			// string strBonus = bonusNode["value"].InnerText;
			int intBonus = intAug;
			string strName = _strFriendlyName;
			TreeNode nodTemp = new TreeNode();
			Improvement.ImprovementType objType = Improvement.ImprovementType.PhysicalLimit;

			switch (strLimit)
			{
				case "Mental":
					{
						objType = Improvement.ImprovementType.MentalLimit;
						break;
					}
				case "Social":
					{
						objType = Improvement.ImprovementType.SocialLimit;
						break;
					}
				default:
					{
						objType = Improvement.ImprovementType.PhysicalLimit;
						break;
					}
			}

			Log.Info("Calling CreateImprovement");
			CreateImprovement(strLimit, _objImprovementSource, SourceName, objType, _strFriendlyName, intBonus, 0, intMin,
				intMax,
				intAug, intAugMax);
		}

		// Select an CharacterAttribute to use instead of the default on a skill.
		public void swapskillattribute(XmlNode bonusNode)
		{
			Log.Info("swapskillattribute");
			// Display the Select Attribute window and record which Skill was selected.
			frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
			if (_strFriendlyName != "")
				frmPickAttribute.Description =
					LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
			else
				frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

			List<string> strValue = new List<string>();
			strValue.Add("LOG");
			strValue.Add("WIL");
			strValue.Add("INT");
			strValue.Add("CHA");
			strValue.Add("EDG");
			strValue.Add("MAG");
			strValue.Add("RES");
			frmPickAttribute.RemoveFromList(strValue);

			Log.Info("swapskillattribute = " + bonusNode.OuterXml.ToString());

			if (bonusNode.InnerXml.Contains("<attribute>"))
			{
				List<string> strLimitValue = new List<string>();
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
					strLimitValue.Add(objXmlAttribute.InnerText);
				frmPickAttribute.LimitToList(strLimitValue);
			}

			// Check to see if there is only one possible selection because of _strLimitSelection.
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (LimitSelection != "")
			{
				frmPickAttribute.SingleAttribute(LimitSelection);
				frmPickAttribute.Opacity = 0;
			}

			frmPickAttribute.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickAttribute.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickAttribute.SelectedAttribute;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			Log.Info("Calling CreateImprovement");
			CreateImprovement(frmPickAttribute.SelectedAttribute, _objImprovementSource, SourceName,
				Improvement.ImprovementType.SwapSkillAttribute, _strUnique);
		}

		// Select a Spell.
		public void selectspell(XmlNode bonusNode)
		{
			Log.Info("selectspell");
			// Display the Select Spell window.
			frmSelectSpell frmPickSpell = new frmSelectSpell(_objCharacter);

			if (bonusNode.Attributes["category"] != null)
				frmPickSpell.LimitCategory = bonusNode.Attributes["category"].InnerText;

			Log.Info("selectspell = " + bonusNode.OuterXml.ToString());
			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (ForcedValue != "")
			{
				frmPickSpell.ForceSpellName = ForcedValue;
				frmPickSpell.Opacity = 0;
			}

			if (bonusNode.Attributes["ignorerequirements"] != null)
			{
				frmPickSpell.IgnoreRequirements = Convert.ToBoolean(bonusNode.Attributes["ignorerequirements"].InnerText);
			}

			frmPickSpell.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickSpell.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickSpell.SelectedSpell;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			Log.Info("Calling CreateImprovement");
			CreateImprovement(frmPickSpell.SelectedSpell, _objImprovementSource, SourceName, Improvement.ImprovementType.Text,
				_strUnique);
		}

		// Select a Contact
		public void selectcontact(XmlNode bonusNode)
		{
			Log.Info("selectcontact");
			XmlNode nodSelect = bonusNode;

			frmSelectItem frmSelect = new frmSelectItem();

			String strMode = nodSelect["type"]?.InnerText ?? "all";

			List<Contact> selectedContactsList;
			if (strMode == "all")
			{
				selectedContactsList = new List<Contact>(_objCharacter.Contacts);
			}
			else if (strMode == "group" || strMode == "nongroup")
			{
				bool blnGroup = strMode == "group";


				//Select any contact where IsGroup equals blnGroup
				//and add to a list
				selectedContactsList =
					new List<Contact>(from contact in _objCharacter.Contacts
									  where contact.IsGroup == blnGroup
									  select contact);
			}
			else
			{
				throw new AbortedException();
			}

			if (selectedContactsList.Count == 0)
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_NoContactFound"),
					LanguageManager.Instance.GetString("MessageTitle_NoContactFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new AbortedException();
			}

			int count = 0;
			//Black magic LINQ to cast content of list to another type
			List<ListItem> contacts = new List<ListItem>(from x in selectedContactsList
														 select new ListItem() { Name = x.Name, Value = (count++).ToString() });

			String strPrice = nodSelect?.InnerText ?? "";

			frmSelect.GeneralItems = contacts;
			frmSelect.ShowDialog();

			int index = int.Parse(frmSelect.SelectedItem);
			if (frmSelect.DialogResult != DialogResult.Cancel)
			{
				Contact selectedContact = selectedContactsList[index];

				if (nodSelect["mademan"] != null)
				{
					selectedContact.MadeMan = true;
					CreateImprovement(selectedContact.GUID, Improvement.ImprovementSource.Quality, SourceName,
						Improvement.ImprovementType.ContactMadeMan, selectedContact.GUID);
				}

				if (String.IsNullOrWhiteSpace(SelectedValue))
				{
					SelectedValue = selectedContact.Name;
				}
				else
				{
					SelectedValue += (", " + selectedContact.Name);
				}
			}
			else
			{
				throw new AbortedException();
			}
		}

		public void addcontact(XmlNode bonusNode)
		{
			Log.Info("addcontact");

			int loyalty, connection;

			bonusNode.TryGetField("loyalty", out loyalty, 1);
			bonusNode.TryGetField("connection", out connection, 1);
			bool group = bonusNode["group"] != null;
			bool free = bonusNode["free"] != null;

			Contact contact = new Contact(_objCharacter);
			contact.Free = free;
			contact.IsGroup = group;
			contact.Loyalty = loyalty;
			contact.Connection = connection;
			contact.ReadOnly = true;
			_objCharacter.Contacts.Add(contact);

			CreateImprovement(contact.GUID, Improvement.ImprovementSource.Quality, SourceName,
				Improvement.ImprovementType.AddContact, contact.GUID);
		}

		// Affect a Specific CharacterAttribute.
		public void specificattribute(XmlNode bonusNode)
		{
			Log.Info("specificattribute");

			if (bonusNode["name"].InnerText != "ESS")
			{
				// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
				// Record the improvement.
				int intMin = 0;
				int intAug = 0;
				int intMax = 0;
				int intAugMax = 0;

				// Extract the modifiers.
				if (bonusNode.InnerXml.Contains("min"))
					intMin = ValueToInt(bonusNode["min"].InnerXml, _intRating);
				if (bonusNode.InnerXml.Contains("val"))
					intAug = ValueToInt(bonusNode["val"].InnerXml, _intRating);
				if (bonusNode.InnerXml.Contains("max"))
				{
					if (bonusNode["max"].InnerText.Contains("-natural"))
					{
						intMax = Convert.ToInt32(bonusNode["max"].InnerText.Replace("-natural", string.Empty)) -
								 _objCharacter.GetAttribute(bonusNode["name"].InnerText).MetatypeMaximum;
					}
					else
						intMax = ValueToInt(bonusNode["max"].InnerXml, _intRating);
				}
				if (bonusNode.InnerXml.Contains("aug"))
					intAugMax = ValueToInt(bonusNode["aug"].InnerXml, _intRating);

				string strUseUnique = _strUnique;
				if (bonusNode["name"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

				string strAttribute = bonusNode["name"].InnerText;

				if (bonusNode["affectbase"] != null)
					strAttribute += "Base";

				CreateImprovement(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
					strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
			}
			else
			{
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Essence, "",
					Convert.ToInt32(bonusNode["val"].InnerText));
			}
		}

		// Add a paid increase to an attribute
		public void attributelevel(XmlNode bonusNode)
		{
			Log.Info(new object[] { "attributelevel", bonusNode.OuterXml });
			String strAttrib;
			int value;
			bonusNode.TryGetField("val", out value, 1);

			if (bonusNode.TryGetField("name", out strAttrib))
			{
				CreateImprovement(strAttrib, _objImprovementSource, SourceName,
					Improvement.ImprovementType.Attributelevel, "", value);
			}
			else
			{
				Log.Error(new object[] { "attributelevel", bonusNode.OuterXml });
			}
		}

		public void skilllevel(XmlNode bonusNode)
		{
			Log.Info(new object[] { "skilllevel", bonusNode.OuterXml });
			String strSkill;
			int value;
			bonusNode.TryGetField("val", out value, 1);
			if (bonusNode.TryGetField("name", out strSkill))
			{
				CreateImprovement(strSkill, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillLevel, "", value);


			}
			else
			{
				Log.Error(new object[] { "skilllevel", bonusNode.OuterXml });
			}
		}

		public void pushtext(XmlNode bonusNode)
		{

			String push = bonusNode.InnerText;
			if (!String.IsNullOrWhiteSpace(push))
			{
				_objCharacter.Pushtext.Push(push);
			}
		}

		public void knowsoft(XmlNode bonusNode)
		{
			int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, _intRating) : 1;

			string name;
			if (!string.IsNullOrWhiteSpace(ForcedValue))
			{
				name = ForcedValue;
			}
			else if (bonusNode["pick"] != null)
			{
				List<ListItem> types;
				if (bonusNode["group"] != null)
				{
					var v = bonusNode.SelectNodes($"./group");
					types =
						KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"group[. = '{x.Value}']").Count > 0).ToList();

				}
				else if (bonusNode["notgroup"] != null)
				{
					types =
						KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"notgroup[. = '{x.Value}']").Count == 0).ToList();
				}
				else
				{
					types = KnowledgeSkill.KnowledgeTypes;
				}

				frmSelectItem select = new frmSelectItem();
				select.DropdownItems = KnowledgeSkill.KnowledgeSkillsWithCategory(types.Select(x => x.Value).ToArray());

				select.ShowDialog();
				if (select.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				name = select.SelectedItem;
			}
			else if (bonusNode["name"] != null)
			{
				name = bonusNode["name"].InnerText;
			}
			else
			{
				//TODO some kind of error handling
				Log.Error(new[] { bonusNode.OuterXml, "Missing pick or name" });
				throw new AbortedException();
			}
			SelectedValue = name;


			KnowledgeSkill skill = new KnowledgeSkill(_objCharacter, name);

			bool knowsoft = bonusNode.TryCheckValue("require", "skilljack");

			if (knowsoft)
			{
				_objCharacter.SkillsSection.KnowsoftSkills.Add(skill);
				if (_objCharacter.SkillsoftAccess)
				{
					_objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
				}
			}
			else
			{
				_objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
			}

			CreateImprovement(name, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillBase, _strUnique, val);
			CreateImprovement(skill.Id.ToString(), _objImprovementSource, SourceName,
				Improvement.ImprovementType.SkillKnowledgeForced, _strUnique);

		}

		public void knowledgeskilllevel(XmlNode bonusNode)
		{
			//Theoretically life modules, right now we just give out free points and let people sort it out themselves.
			//Going to be fun to do the real way, from a computer science perspective, but i don't feel like using 2 weeks on that now

			int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, _intRating) : 1;
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, "", val);
		}

		public void knowledgeskillpoints(XmlNode bonusNode)
		{
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, "",
				ValueToInt(bonusNode.InnerText, Convert.ToInt32(bonusNode.Value)));
		}

		public void skillgrouplevel(XmlNode bonusNode)
		{
			Log.Info(new object[] { "skillgrouplevel", bonusNode.OuterXml });
			String strSkillGroup;
			int value;
			if (bonusNode.TryGetField("name", out strSkillGroup) &&
				bonusNode.TryGetField("val", out value))
			{
				CreateImprovement(strSkillGroup, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillGroupLevel, "", value);
			}
			else
			{
				Log.Error(new object[] { "skillgrouplevel", bonusNode.OuterXml });
			}
		}

		// Change the maximum number of BP that can be spent on Nuyen.
		public void nuyenmaxbp(XmlNode bonusNode)
		{
			Log.Info("nuyenmaxbp");
			Log.Info("nuyenmaxbp = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.NuyenMaxBP, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Apply a bonus/penalty to physical limit.
		public void physicallimit(XmlNode bonusNode)
		{
			Log.Info("physicallimit");
			Log.Info("physicallimit = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("Physical", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalLimit,
				"",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Apply a bonus/penalty to mental limit.
		public void mentallimit(XmlNode bonusNode)
		{
			Log.Info("mentallimit");
			Log.Info("mentallimit = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("Mental", _objImprovementSource, SourceName, Improvement.ImprovementType.MentalLimit,
				"",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Apply a bonus/penalty to social limit.
		public void sociallimit(XmlNode bonusNode)
		{
			Log.Info("sociallimit");
			Log.Info("sociallimit = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("Social", _objImprovementSource, SourceName, Improvement.ImprovementType.SocialLimit,
				"",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
		public void nuyenamt(XmlNode bonusNode)
		{
			Log.Info("nuyenamt");
			Log.Info("nuyenamt = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Nuyen, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Improve Condition Monitors.
		public void conditionmonitor(XmlNode bonusNode)
		{
			Log.Info("conditionmonitor");
			Log.Info("conditionmonitor = " + bonusNode.OuterXml.ToString());
			// Physical Condition.
			if (bonusNode.InnerXml.Contains("physical"))
			{
				Log.Info("Calling CreateImprovement for Physical");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCM, _strUnique,
					ValueToInt(bonusNode["physical"].InnerText, _intRating));
			}

			// Stun Condition.
			if (bonusNode.InnerXml.Contains("stun"))
			{
				Log.Info("Calling CreateImprovement for Stun");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.StunCM, _strUnique,
					ValueToInt(bonusNode["stun"].InnerText, _intRating));
			}

			// Condition Monitor Threshold.
			if (bonusNode["threshold"] != null)
			{
				string strUseUnique = _strUnique;
				if (bonusNode["threshold"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["threshold"].Attributes["precedence"].InnerText;

				Log.Info("Calling CreateImprovement for Threshold");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
					ValueToInt(bonusNode["threshold"].InnerText, _intRating));
			}

			// Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
			if (bonusNode["thresholdoffset"] != null)
			{
				string strUseUnique = _strUnique;
				if (bonusNode["thresholdoffset"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["thresholdoffset"].Attributes["precedence"].InnerText;

				Log.Info("Calling CreateImprovement for Threshold Offset");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMThresholdOffset,
					strUseUnique, ValueToInt(bonusNode["thresholdoffset"].InnerText, _intRating));
			}

			// Condition Monitor Overflow.
			if (bonusNode["overflow"] != null)
			{
				Log.Info("Calling CreateImprovement for Overflow");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CMOverflow, _strUnique,
					ValueToInt(bonusNode["overflow"].InnerText, _intRating));
			}
		}

		// Improve Living Personal Attributes.
		public void livingpersona(XmlNode bonusNode)
		{
			Log.Info("livingpersona");
			Log.Info("livingpersona = " + bonusNode.OuterXml.ToString());
			// Response.
			if (bonusNode.InnerXml.Contains("response"))
			{
				Log.Info("Calling CreateImprovement for response");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaResponse,
					_strUnique, ValueToInt(bonusNode["response"].InnerText, _intRating));
			}

			// Signal.
			if (bonusNode.InnerXml.Contains("signal"))
			{
				Log.Info("Calling CreateImprovement for signal");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSignal,
					_strUnique,
					ValueToInt(bonusNode["signal"].InnerText, _intRating));
			}

			// Firewall.
			if (bonusNode.InnerXml.Contains("firewall"))
			{
				Log.Info("Calling CreateImprovement for firewall");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaFirewall,
					_strUnique, ValueToInt(bonusNode["firewall"].InnerText, _intRating));
			}

			// System.
			if (bonusNode.InnerXml.Contains("system"))
			{
				Log.Info("Calling CreateImprovement for system");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSystem,
					_strUnique,
					ValueToInt(bonusNode["system"].InnerText, _intRating));
			}

			// Biofeedback Filter.
			if (bonusNode.InnerXml.Contains("biofeedback"))
			{
				Log.Info("Calling CreateImprovement for biofeedback");
				CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaBiofeedback,
					_strUnique, ValueToInt(bonusNode["biofeedback"].InnerText, _intRating));
			}
		}

		// The Improvement adjusts a specific Skill.
		public void specificskill(XmlNode bonusNode)
		{
			Log.Info("specificskill");
			Log.Info("specificskill = " + bonusNode.OuterXml.ToString());
			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}

			string strUseUnique = _strUnique;
			if (bonusNode.Attributes["precedence"] != null)
				strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;

			// Record the improvement.
			if (bonusNode["bonus"] != null)
			{
				Log.Info("Calling CreateImprovement for bonus");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.Skill, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
					0, "", blnAddToRating);
			}
			if (bonusNode["max"] != null)
			{
				Log.Info("Calling CreateImprovement for max");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ValueToInt(bonusNode["max"].InnerText, _intRating), 0,
					0,
					"", blnAddToRating);
			}
		}

		public void reflexrecorderoptimization(XmlNode bonusNode)
		{
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ReflexRecorderOptimization,
				_strUnique);
		}

		// The Improvement adds a martial art
		public void martialart(XmlNode bonusNode)
		{
			Log.Info("martialart");
			Log.Info("martialart = " + bonusNode.OuterXml.ToString());
			XmlDocument _objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
			XmlNode objXmlArt =
				_objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + bonusNode.InnerText +
												 "\"]");

			TreeNode objNode = new TreeNode();
			MartialArt objMartialArt = new MartialArt(_objCharacter);
			objMartialArt.Create(objXmlArt, objNode, _objCharacter);
			objMartialArt.IsQuality = true;
			_objCharacter.MartialArts.Add(objMartialArt);
		}

		// The Improvement adds a limit modifier
		public void limitmodifier(XmlNode bonusNode)
		{
			Log.Info("limitmodifier");
			Log.Info("limitmodifier = " + bonusNode.OuterXml.ToString());
			LimitModifier objLimitMod = new LimitModifier(_objCharacter);
			string strLimit = bonusNode["limit"].InnerText;
			string strBonus = bonusNode["value"].InnerText;
			if (strBonus == "Rating")
			{
				strBonus = _intRating.ToString();
			}
			string strCondition = "";
			try
			{
				strCondition = bonusNode["condition"].InnerText;
			}
			catch
			{
			}
			int intBonus = 0;
			if (strBonus == "Rating")
				intBonus = _intRating;
			else
				intBonus = Convert.ToInt32(strBonus);
			string strName = _strFriendlyName;
			TreeNode nodTemp = new TreeNode();
			Log.Info("Calling CreateImprovement");
			CreateImprovement(strLimit, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitModifier,
				_strFriendlyName, intBonus, 0, 0, 0, 0, 0, strCondition);
		}

		// The Improvement adjusts a Skill Category.
		public void skillcategory(XmlNode bonusNode)
		{
			Log.Info("skillcategory");
			Log.Info("skillcategory = " + bonusNode.OuterXml.ToString());

			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}
			if (bonusNode.InnerXml.Contains("exclude"))
			{
				Log.Info("Calling CreateImprovement - exclude");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillCategory, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0,
					0,
					0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
			}
			else
			{
				Log.Info("Calling CreateImprovement");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillCategory, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0,
					0,
					0, 0, "", blnAddToRating);
			}
		}

		// The Improvement adjusts a Skill Group.
		public void skillgroup(XmlNode bonusNode)
		{
			Log.Info("skillgroup");
			Log.Info("skillgroup = " + bonusNode.OuterXml.ToString());

			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}
			if (bonusNode.InnerXml.Contains("exclude"))
			{
				Log.Info("Calling CreateImprovement - exclude");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillGroup, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
					0, bonusNode["exclude"].InnerText, blnAddToRating);
			}
			else
			{
				Log.Info("Calling CreateImprovement");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillGroup, _strUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1, 0, 0, 0,
					0, "", blnAddToRating);
			}
		}

		// The Improvement adjust Skills with the given CharacterAttribute.
		public void skillattribute(XmlNode bonusNode)
		{
			Log.Info("skillattribute");
			Log.Info("skillattribute = " + bonusNode.OuterXml.ToString());

			string strUseUnique = _strUnique;
			if (bonusNode["name"].Attributes["precedence"] != null)
				strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

			bool blnAddToRating = false;
			if (bonusNode["applytorating"] != null)
			{
				if (bonusNode["applytorating"].InnerText == "yes")
					blnAddToRating = true;
			}
			if (bonusNode.InnerXml.Contains("exclude"))
			{
				Log.Info("Calling CreateImprovement - exclude");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1,
					0, 0, 0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
			}
			else
			{
				Log.Info("Calling CreateImprovement");
				CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
					Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, _intRating), 1,
					0, 0, 0, 0, "", blnAddToRating);
			}
		}

		// The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
		public void skillarticulation(XmlNode bonusNode)
		{
			Log.Info("skillarticulation");
			Log.Info("skillarticulation = " + bonusNode.OuterXml.ToString());

			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.EnhancedArticulation,
				_strUnique,
				ValueToInt(bonusNode["bonus"].InnerText, _intRating));
		}

		// Check for Armor modifiers.
		public void armor(XmlNode bonusNode)
		{
			Log.Info("armor");
			Log.Info("armor = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			string strUseUnique = _strUnique;
			if (bonusNode.Attributes["precedence"] != null)
			{
				strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;
			}
			else if (bonusNode.Attributes["group"] != null)
			{
				strUseUnique = "group" + bonusNode.Attributes["group"].InnerText;
			}
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Armor, strUseUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Reach modifiers.
		public void reach(XmlNode bonusNode)
		{
			Log.Info("reach");
			Log.Info("reach = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Reach, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Unarmed Damage Value modifiers.
		public void unarmeddv(XmlNode bonusNode)
		{
			Log.Info("unarmeddv");
			Log.Info("unarmeddv = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDV, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Unarmed Damage Value Physical.
		public void unarmeddvphysical(XmlNode bonusNode)
		{
			Log.Info("unarmeddvphysical");
			Log.Info("unarmeddvphysical = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDVPhysical, "");
		}

		// Check for Unarmed Armor Penetration.
		public void unarmedap(XmlNode bonusNode)
		{
			Log.Info("unarmedap");
			Log.Info("unarmedap = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedAP, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Initiative modifiers.
		public void initiative(XmlNode bonusNode)
		{
			Log.Info("initiative");
			Log.Info("initiative = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Initiative, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Initiative Pass modifiers. Only the highest one ever applies.
		public void initiativepass(XmlNode bonusNode)
		{
			Log.Info("initiativepass");
			Log.Info("initiativepass = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");

			string strUseUnique = bonusNode.Name;
			if (bonusNode.Attributes["precedence"] != null)
				strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;

			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativePass,
				strUseUnique, ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Initiative Pass modifiers. Only the highest one ever applies.
		public void initiativepassadd(XmlNode bonusNode)
		{
			Log.Info("initiativepassadd");
			Log.Info("initiativepassadd = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativePassAdd, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Matrix Initiative modifiers.
		public void matrixinitiative(XmlNode bonusNode)
		{
			Log.Info("matrixinitiative");
			Log.Info("matrixinitiative = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiative, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Matrix Initiative Pass modifiers.
		public void matrixinitiativepass(XmlNode bonusNode)
		{
			Log.Info("matrixinitiativepass");
			Log.Info("matrixinitiativepass = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativePass,
				"matrixinitiativepass", ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Matrix Initiative Pass modifiers.
		public void matrixinitiativepassadd(XmlNode bonusNode)
		{
			Log.Info("matrixinitiativepassadd");
			Log.Info("matrixinitiativepassadd = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativePass,
				_strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Lifestyle cost modifiers.
		public void lifestylecost(XmlNode bonusNode)
		{
			Log.Info("lifestylecost");
			Log.Info("lifestylecost = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LifestyleCost, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for basic Lifestyle cost modifiers.
		public void basiclifestylecost(XmlNode bonusNode)
		{
			Log.Info("basiclifestylecost");
			Log.Info("basiclifestylecost = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BasicLifestyleCost, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Genetech Cost modifiers.
		public void genetechcostmultiplier(XmlNode bonusNode)
		{
			Log.Info("genetechcostmultiplier");
			Log.Info("genetechcostmultiplier = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechCostMultiplier,
				_strUnique, ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Genetech: Transgenics Cost modifiers.
		public void transgenicsgenetechcost(XmlNode bonusNode)
		{
			Log.Info("transgenicsgenetechcost");
			Log.Info("transgenicsgenetechcost = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TransgenicsBiowareCost,
				_strUnique, ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Basic Bioware Essence Cost modifiers.
		public void basicbiowareessmultiplier(XmlNode bonusNode)
		{
			Log.Info("basicbiowareessmultiplier");
			Log.Info("basicbiowareessmultiplier = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BasicBiowareEssCost,
				_strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Bioware Essence Cost modifiers.
		public void biowareessmultiplier(XmlNode bonusNode)
		{
			Log.Info("biowareessmultiplier");
			Log.Info("biowareessmultiplier = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Cybeware Essence Cost modifiers.
		public void cyberwareessmultiplier(XmlNode bonusNode)
		{
			Log.Info("cyberwareessmultiplier");
			Log.Info("cyberwareessmultiplier = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Uneducated modifiers.
		public void uneducated(XmlNode bonusNode)
		{
			Log.Info("uneducated");
			Log.Info("uneducated = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Uneducated, _strUnique);
			_objCharacter.SkillsSection.Uneducated = true;
		}

		// Check for College Education modifiers.
		public void collegeeducation(XmlNode bonusNode)
		{
			Log.Info("collegeeducation");
			Log.Info("collegeeducation = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CollegeEducation, _strUnique);
			_objCharacter.SkillsSection.CollegeEducation = true;
		}

		// Check for Jack Of All Trades modifiers.
		public void jackofalltrades(XmlNode bonusNode)
		{
			Log.Info("jackofalltrades");
			Log.Info("jackofalltrades = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.JackOfAllTrades, _strUnique);
			_objCharacter.SkillsSection.JackOfAllTrades = true;
		}

		// Check for Prototype Transhuman modifiers.
		public void prototypetranshuman(XmlNode bonusNode)
		{
			Log.Info("prototypetranshuman");
			Log.Info("prototypetranshuman = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");

			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PrototypeTranshuman, _strUnique);
			_objCharacter.PrototypeTranshuman = Convert.ToDecimal(bonusNode.InnerText);

		}

		// Check for Uncouth modifiers.
		public void uncouth(XmlNode bonusNode)
		{
			Log.Info("uncouth");
			Log.Info("uncouth = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Uncouth, _strUnique);
			_objCharacter.SkillsSection.Uncouth = true;
		}

		// Check for Friends In High Places modifiers.
		public void friendsinhighplaces(XmlNode bonusNode)
		{
			Log.Info("friendsinhighplaces");
			Log.Info("friendsinhighplaces = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FriendsInHighPlaces,
				_strUnique);
			_objCharacter.FriendsInHighPlaces = true;
		}

		// Check for School of Hard Knocks modifiers.
		public void schoolofhardknocks(XmlNode bonusNode)
		{
			Log.Info("schoolofhardknocks");
			Log.Info("schoolofhardknocks = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SchoolOfHardKnocks, _strUnique);
			_objCharacter.SkillsSection.SchoolOfHardKnocks = true;
		}

		// Check for ExCon modifiers.
		public void excon(XmlNode bonusNode)
		{
			Log.Info("ExCon");
			Log.Info("ExCon = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ExCon, _strUnique);
			_objCharacter.ExCon = true;
		}

		// Check for TrustFund modifiers.
		public void trustfund(XmlNode bonusNode)
		{
			Log.Info("TrustFund");
			Log.Info("TrustFund = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TrustFund,
				_strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
			_objCharacter.TrustFund = ValueToInt(bonusNode.InnerText, _intRating);
		}

		// Check for Tech School modifiers.
		public void techschool(XmlNode bonusNode)
		{
			Log.Info("techschool");
			Log.Info("techschool = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.TechSchool, _strUnique);
			_objCharacter.SkillsSection.TechSchool = true;
		}

		// Check for MadeMan modifiers.
		public void mademan(XmlNode bonusNode)
		{
			Log.Info("MadeMan");
			Log.Info("MadeMan = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MadeMan, _strUnique);
			_objCharacter.MadeMan = true;
		}

		// Check for Linguist modifiers.
		public void linguist(XmlNode bonusNode)
		{
			Log.Info("Linguist");
			Log.Info("Linguist = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Linguist, _strUnique);
			_objCharacter.SkillsSection.Linguist = true;
		}

		// Check for LightningReflexes modifiers.
		public void lightningreflexes(XmlNode bonusNode)
		{
			Log.Info("LightningReflexes");
			Log.Info("LightningReflexes = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LightningReflexes, _strUnique);
			_objCharacter.LightningReflexes = true;
		}

		// Check for Fame modifiers.
		public void fame(XmlNode bonusNode)
		{
			Log.Info("Fame");
			Log.Info("Fame = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Fame, _strUnique);
			_objCharacter.Fame = true;
		}

		// Check for BornRich modifiers.
		public void bornrich(XmlNode bonusNode)
		{
			Log.Info("BornRich");
			Log.Info("BornRich = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BornRich, _strUnique);
			_objCharacter.BornRich = true;
		}

		// Check for Erased modifiers.
		public void erased(XmlNode bonusNode)
		{
			Log.Info("Erased");
			Log.Info("Erased = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Erased, _strUnique);
			_objCharacter.Erased = true;
		}

		// Check for Erased modifiers.
		public void overclocker(XmlNode bonusNode)
		{
			Log.Info("OverClocker");
			Log.Info("Overclocker = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Overclocker, _strUnique);
			_objCharacter.Overclocker = true;
		}

		// Check for Restricted Gear modifiers.
		public void restrictedgear(XmlNode bonusNode)
		{
			Log.Info("restrictedgear");
			Log.Info("restrictedgear = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedGear, _strUnique);
			_objCharacter.RestrictedGear = true;
		}

		// Check for Adept Linguistics.
		public void adeptlinguistics(XmlNode bonusNode)
		{
			Log.Info("adeptlinguistics");
			Log.Info("adeptlinguistics = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptLinguistics, _strUnique,
				1);
		}

		// Check for Weapon Category DV modifiers.
		public void weaponcategorydv(XmlNode bonusNode)
		{
			//TODO: FIX THIS
			/*
			 * I feel like talking a little bit about improvementmanager at
			 * this point. It is an intresting class. First of all, it 
			 * manages to throw out everything we ever learned about OOP
			 * and create a class based on functional programming.
			 * 
			 * That is true, it is a class, based on manipulating a single
			 * list on another class.
			 * 
			 * But atleast there is a reference to it somewhere right?
			 * 
			 * No, you create one wherever you need it, meaning there are
			 * tens of instances of this class, all operating on the same 
			 * list
			 * 
			 * After that, it is just plain stupid.
			 * If you have an list of xmlNodes and some might be the same
			 * it checks if a specific node exists (sometimes even by text
			 * comparison on .OuterXml) and then runs specific code for 
			 * each. If it is there multiple times either of those 2 things
			 * happen.
			 * 
			 * 1. Sad, nothing we can do, guess you have to survive
			 * 2. Lets create a foreach in that specific part of the code
			 * 
			 * Fuck ImprovementManager, kill it with fire, burn the ashes
			 * and feed what remains to a dragon that eats unholy 
			 * abominations
			 */


			Log.Info("weaponcategorydv");
			Log.Info("weaponcategorydv = " + bonusNode.OuterXml.ToString());
			XmlNodeList objXmlCategoryList = bonusNode.SelectNodes("weaponcategorydv");
			XmlNode nodWeapon = bonusNode;

			if (nodWeapon["selectskill"] != null)
			{
				// Display the Select Skill window and record which Skill was selected.
				frmSelectItem frmPickCategory = new frmSelectItem();
				List<ListItem> lstGeneralItems = new List<ListItem>();

				ListItem liBlades = new ListItem();
				liBlades.Name = "Blades";
				liBlades.Value = "Blades";

				ListItem liClubs = new ListItem();
				liClubs.Name = "Clubs";
				liClubs.Value = "Clubs";

				ListItem liUnarmed = new ListItem();
				liUnarmed.Name = "Unarmed";
				liUnarmed.Value = "Unarmed";

				ListItem liAstral = new ListItem();
				liAstral.Name = "Astral Combat";
				liAstral.Value = "Astral Combat";

				ListItem liExotic = new ListItem();
				liExotic.Name = "Exotic Melee Weapons";
				liExotic.Value = "Exotic Melee Weapons";

				lstGeneralItems.Add(liAstral);
				lstGeneralItems.Add(liBlades);
				lstGeneralItems.Add(liClubs);
				lstGeneralItems.Add(liExotic);
				lstGeneralItems.Add(liUnarmed);
				frmPickCategory.GeneralItems = lstGeneralItems;

				if (_strFriendlyName != "")
					frmPickCategory.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", _strFriendlyName);
				else
					frmPickCategory.Description = LanguageManager.Instance.GetString("Title_SelectWeaponCategory");

				Log.Info("_strForcedValue = " + ForcedValue);

				if (ForcedValue.StartsWith("Adept:") || ForcedValue.StartsWith("Magician:"))
					ForcedValue = "";

				if (ForcedValue != "")
				{
					frmPickCategory.Opacity = 0;
				}
				frmPickCategory.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickCategory.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				SelectedValue = frmPickCategory.SelectedItem;

				Log.Info("strSelected = " + SelectedValue);

				foreach (Power objPower in _objCharacter.Powers)
				{
					if (objPower.InternalId == SourceName)
					{
						objPower.Extra = SelectedValue;
					}
				}

				Log.Info("Calling CreateImprovement");
				CreateImprovement(SelectedValue, _objImprovementSource, SourceName,
					Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ValueToInt(nodWeapon["bonus"].InnerXml, _intRating));
			}
			else
			{
				// Run through each of the Skill Groups since there may be more than one affected.
				foreach (XmlNode objXmlCategory in objXmlCategoryList)
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(objXmlCategory["name"].InnerText, _objImprovementSource, SourceName,
						Improvement.ImprovementType.WeaponCategoryDV, _strUnique, ValueToInt(objXmlCategory["bonus"].InnerXml, _intRating));
				}
			}
		}

		// Check for Mentor Spirit bonuses.
		public void selectmentorspirit(XmlNode bonusNode)
		{
			Log.Info("selectmentorspirit");
			Log.Info("selectmentorspirit = " + bonusNode.OuterXml.ToString());
			frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
			frmPickMentorSpirit.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickMentorSpirit.SelectedMentor;

			string strHoldValue = SelectedValue;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			if (frmPickMentorSpirit.BonusNode != null)
			{
				if (!CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName))
				{
					throw new AbortedException();
				}
			}

			if (frmPickMentorSpirit.Choice1BonusNode != null)
			{
				Log.Info("frmPickMentorSpirit.Choice1BonusNode = " + frmPickMentorSpirit.Choice1BonusNode.OuterXml.ToString());
				string strForce = ForcedValue;
				if (!frmPickMentorSpirit.Choice1.StartsWith("Adept:") && !frmPickMentorSpirit.Choice1.StartsWith("Magician:"))
					ForcedValue = frmPickMentorSpirit.Choice1;
				else
					ForcedValue = "";
				Log.Info("Calling CreateImprovement");
				bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice1BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName);
				if (!blnSuccess)
				{
					throw new AbortedException();
				}
				ForcedValue = strForce;
				_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
			}

			if (frmPickMentorSpirit.Choice2BonusNode != null)
			{
				Log.Info("frmPickMentorSpirit.Choice2BonusNode = " + frmPickMentorSpirit.Choice2BonusNode.OuterXml.ToString());
				string strForce = ForcedValue;
				if (!frmPickMentorSpirit.Choice2.StartsWith("Adept:") && !frmPickMentorSpirit.Choice2.StartsWith("Magician:"))
					ForcedValue = frmPickMentorSpirit.Choice2;
				else
					ForcedValue = "";
				Log.Info("Calling CreateImprovement");
				bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice2BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName);
				if (!blnSuccess)
				{
					throw new AbortedException();
				}
				ForcedValue = strForce;
				_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
			}

			SelectedValue = strHoldValue;
			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("_strForcedValue = " + ForcedValue);
		}

		// Check for Paragon bonuses.
		public void selectparagon(XmlNode bonusNode)
		{
			Log.Info("selectparagon");
			Log.Info("selectparagon = " + bonusNode.OuterXml.ToString());
			frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
			frmPickMentorSpirit.XmlFile = "paragons.xml";
			frmPickMentorSpirit.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickMentorSpirit.SelectedMentor;
			string strHoldValue = SelectedValue;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			if (frmPickMentorSpirit.BonusNode != null)
			{
				bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName);
				if (!blnSuccess)
				{
					throw new AbortedException();
				}
			}

			if (frmPickMentorSpirit.Choice1BonusNode != null)
			{
				string strForce = ForcedValue;
				ForcedValue = frmPickMentorSpirit.Choice1;
				bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice1BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName);
				if (!blnSuccess)
				{
					throw new AbortedException();
				}
				ForcedValue = strForce;
				_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
			}

			if (frmPickMentorSpirit.Choice2BonusNode != null)
			{
				string strForce = ForcedValue;
				ForcedValue = frmPickMentorSpirit.Choice2;
				bool blnSuccess = CreateImprovements(_objImprovementSource, SourceName, frmPickMentorSpirit.Choice2BonusNode,
					_blnConcatSelectedValue, _intRating, _strFriendlyName);
				if (!blnSuccess)
				{
					throw new AbortedException();
				}
				ForcedValue = strForce;
				_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
			}

			SelectedValue = strHoldValue;
		}

		// Check for Smartlink bonus.
		public void smartlink(XmlNode bonusNode)
		{
			Log.Info("smartlink");
			Log.Info("smartlink = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Smartlink, "smartlink");
		}

		// Check for Adapsin bonus.
		public void adapsin(XmlNode bonusNode)
		{
			Log.Info("adapsin");
			Log.Info("adapsin = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Adapsin, "adapsin");
		}

		// Check for SoftWeave bonus.
		public void softweave(XmlNode bonusNode)
		{
			Log.Info("softweave");
			Log.Info("softweave = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SoftWeave, "softweave");
		}

		// Check for Sensitive System.
		public void sensitivesystem(XmlNode bonusNode)
		{
			Log.Info("sensitivesystem");
			Log.Info("sensitivesystem = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SensitiveSystem,
				"sensitivesystem");
		}

		// Check for Movement Percent.
		public void movementmultiplier(XmlNode bonusNode)
		{
			Log.Info("movementmultiplier");
			Log.Info("movementmultiplier = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MovementMultiplier, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Movement Percent.
		public void movementpercent(XmlNode bonusNode)
		{
			Log.Info("movementpercent");
			Log.Info("movementpercent = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.MovementPercent, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Swim Percent.
		public void swimpercent(XmlNode bonusNode)
		{
			Log.Info("swimpercent");
			Log.Info("swimpercent = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SwimPercent, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Fly Percent.
		public void flypercent(XmlNode bonusNode)
		{
			Log.Info("flypercent");
			Log.Info("flypercent = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FlyPercent, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Fly Speed.
		public void flyspeed(XmlNode bonusNode)
		{
			Log.Info("flyspeed");
			Log.Info("flyspeed = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FlySpeed, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for free Positive Qualities.
		public void freepositivequalities(XmlNode bonusNode)
		{
			Log.Info("freepositivequalities");
			Log.Info("freepositivequalities = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreePositiveQualities, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for free Negative Qualities.
		public void freenegativequalities(XmlNode bonusNode)
		{
			Log.Info("freenegativequalities");
			Log.Info("freenegativequalities = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeNegativeQualities, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Select Side.
		public void selectside(XmlNode bonusNode)
		{
			Log.Info("selectside");
			Log.Info("selectside = " + bonusNode.OuterXml.ToString());
			frmSelectSide frmPickSide = new frmSelectSide();
			frmPickSide.Description = LanguageManager.Instance.GetString("Label_SelectSide").Replace("{0}", _strFriendlyName);
			if (ForcedValue != "")
				frmPickSide.ForceValue(ForcedValue);
			else
				frmPickSide.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickSide.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickSide.SelectedSide;
			Log.Info("_strSelectedValue = " + SelectedValue);
		}

		// Check for Free Spirit Power Points.
		public void freespiritpowerpoints(XmlNode bonusNode)
		{
			Log.Info("freespiritpowerpoints");
			Log.Info("freespiritpowerpoints = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Adept Power Points.
		public void adeptpowerpoints(XmlNode bonusNode)
		{
			Log.Info("adeptpowerpoints");
			Log.Info("adeptpowerpoints = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Adept Powers
		public void specificpower(XmlNode bonusNode)
		{
			//TODO: Probably broken
			Log.Info("specificpower");
			Log.Info("specificpower = " + bonusNode.OuterXml.ToString());
			// If the character isn't an adept or mystic adept, skip the rest of this.
			if (_objCharacter.AdeptEnabled)
			{
				string strSelection = "";
				ForcedValue = "";


				Log.Info("objXmlSpecificPower = " + bonusNode.OuterXml.ToString());

				string strPowerName = bonusNode["name"].InnerText;
				int intLevels = 0;
				if (bonusNode["val"] != null)
					intLevels = Convert.ToInt32(bonusNode["val"].InnerText);
				bool blnFree = false;
				if (bonusNode["free"] != null)
					blnFree = (bonusNode["free"].InnerText == "yes");

				string strPowerNameLimit = strPowerName;
				if (bonusNode["selectlimit"] != null)
				{
					Log.Info("selectlimit = " + bonusNode["selectlimit"].OuterXml.ToString());
					ForcedValue = "";
					// Display the Select Limit window and record which Limit was selected.
					frmSelectLimit frmPickLimit = new frmSelectLimit();
					if (_strFriendlyName != "")
						frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
							.Replace("{0}", _strFriendlyName);
					else
						frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

					if (bonusNode["selectlimit"].InnerXml.Contains("<limit>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objXmlAttribute in bonusNode["selectlimit"].SelectNodes("limit"))
							strValue.Add(objXmlAttribute.InnerText);
						frmPickLimit.LimitToList(strValue);
					}

					if (bonusNode["selectlimit"].InnerXml.Contains("<excludelimit>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objXmlAttribute in bonusNode["selectlimit"].SelectNodes("excludelimit"))
							strValue.Add(objXmlAttribute.InnerText);
						frmPickLimit.RemoveFromList(strValue);
					}

					// Check to see if there is only one possible selection because of _strLimitSelection.
					if (ForcedValue != "")
						LimitSelection = ForcedValue;

					Log.Info("_strForcedValue = " + ForcedValue);
					Log.Info("_strLimitSelection = " + LimitSelection);

					if (LimitSelection != "")
					{
						frmPickLimit.SingleLimit(LimitSelection);
						frmPickLimit.Opacity = 0;
					}

					frmPickLimit.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickLimit.DialogResult == DialogResult.Cancel)
					{
						throw new AbortedException();
					}

					SelectedValue = frmPickLimit.SelectedLimit;
					strSelection = SelectedValue;
					ForcedValue = SelectedValue;

					Log.Info("_strForcedValue = " + ForcedValue);
					Log.Info("_strLimitSelection = " + LimitSelection);
				}

				if (bonusNode["selectskill"] != null)
				{
					Log.Info("selectskill = " + bonusNode["selectskill"].OuterXml.ToString());
					XmlNode nodSkill = bonusNode;
					// Display the Select Skill window and record which Skill was selected.
					frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
					if (_strFriendlyName != "")
						frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
							.Replace("{0}", _strFriendlyName);
					else
						frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

					if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillgroup"))
						frmPickSkill.OnlySkillGroup = nodSkill.SelectSingleNode("selectskill").Attributes["skillgroup"].InnerText;
					else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillcategory"))
						frmPickSkill.OnlyCategory = nodSkill.SelectSingleNode("selectskill").Attributes["skillcategory"].InnerText;
					else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("excludecategory"))
						frmPickSkill.ExcludeCategory = nodSkill.SelectSingleNode("selectskill").Attributes["excludecategory"].InnerText;
					else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("limittoskill"))
						frmPickSkill.LimitToSkill = nodSkill.SelectSingleNode("selectskill").Attributes["limittoskill"].InnerText;

					if (ForcedValue.StartsWith("Adept:") || ForcedValue.StartsWith("Magician:"))
						ForcedValue = "";

					Log.Info("_strForcedValue = " + ForcedValue);
					Log.Info("_strLimitSelection = " + LimitSelection);

					if (ForcedValue != "")
					{
						frmPickSkill.OnlySkill = ForcedValue;
						frmPickSkill.Opacity = 0;
					}
					frmPickSkill.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickSkill.DialogResult == DialogResult.Cancel)
					{
						throw new AbortedException();
					}

					SelectedValue = frmPickSkill.SelectedSkill;
					ForcedValue = SelectedValue;
					strSelection = SelectedValue;

					Log.Info("_strForcedValue = " + ForcedValue);
					Log.Info("_strSelectedValue = " + SelectedValue);
					Log.Info("strSelection = " + strSelection);
				}

				if (bonusNode["selecttext"] != null)
				{
					Log.Info("selecttext = " + bonusNode["selecttext"].OuterXml.ToString());
					frmSelectText frmPickText = new frmSelectText();


					if (_objCharacter.Pushtext.Count > 0)
					{
						strSelection = _objCharacter.Pushtext.Pop();
					}
					else
					{
						frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
							.Replace("{0}", _strFriendlyName);

						Log.Info("_strForcedValue = " + ForcedValue);
						Log.Info("_strLimitSelection = " + LimitSelection);

						if (LimitSelection != "")
						{
							frmPickText.SelectedValue = LimitSelection;
							frmPickText.Opacity = 0;
						}

						frmPickText.ShowDialog();

						// Make sure the dialogue window was not canceled.
						if (frmPickText.DialogResult == DialogResult.Cancel)
						{
							throw new AbortedException();
						}

						strSelection = frmPickText.SelectedValue;
						LimitSelection = strSelection;
					}
					Log.Info("_strLimitSelection = " + LimitSelection);
					Log.Info("strSelection = " + strSelection);
				}

				if (bonusNode["specificattribute"] != null)
				{
					Log.Info("specificattribute = " + bonusNode["specificattribute"].OuterXml.ToString());
					strSelection = bonusNode["specificattribute"]["name"].InnerText.ToString();
					Log.Info(
						"strSelection = " + strSelection);
				}

				if (bonusNode["selectattribute"] != null)
				{
					Log.Info("selectattribute = " + bonusNode["selectattribute"].OuterXml.ToString());
					XmlNode nodSkill = bonusNode;
					if (ForcedValue.StartsWith("Adept"))
						ForcedValue = "";

					// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
					frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
					if (_strFriendlyName != "")
						frmPickAttribute.Description =
							LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
					else
						frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

					// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
					if (_objCharacter.MAGEnabled)
						frmPickAttribute.AddMAG();
					if (_objCharacter.RESEnabled)
						frmPickAttribute.AddRES();

					if (nodSkill["selectattribute"].InnerXml.Contains("<attribute>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("attribute"))
							strValue.Add(objXmlAttribute.InnerText);
						frmPickAttribute.LimitToList(strValue);
					}

					if (nodSkill["selectattribute"].InnerXml.Contains("<excludeattribute>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("excludeattribute"))
							strValue.Add(objXmlAttribute.InnerText);
						frmPickAttribute.RemoveFromList(strValue);
					}

					// Check to see if there is only one possible selection because of _strLimitSelection.
					if (ForcedValue != "")
						LimitSelection = ForcedValue;

					Log.Info("_strForcedValue = " + ForcedValue);
					Log.Info("_strLimitSelection = " + LimitSelection);

					if (LimitSelection != "")
					{
						frmPickAttribute.SingleAttribute(LimitSelection);
						frmPickAttribute.Opacity = 0;
					}

					frmPickAttribute.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickAttribute.DialogResult == DialogResult.Cancel)
					{
						throw new AbortedException();
					}

					SelectedValue = frmPickAttribute.SelectedAttribute;
					if (_blnConcatSelectedValue)
						SourceName += " (" + SelectedValue + ")";
					strSelection = SelectedValue;
					ForcedValue = SelectedValue;

					Log.Info("_strSelectedValue = " + SelectedValue);
					Log.Info("SourceName = " + SourceName);
					Log.Info("_strForcedValue = " + ForcedValue);
				}

				// Check if the character already has this power
				Log.Info("strSelection = " + strSelection);
				bool blnHasPower = false;
				Power objPower = new Power(_objCharacter);
				foreach (Power power in _objCharacter.Powers)
				{
					if (power.Name == strPowerNameLimit)
					{
						if (power.Extra != "" && power.Extra == strSelection)
						{
							blnHasPower = true;
							objPower = power;
						}
						else if (power.Extra == "")
						{
							blnHasPower = true;
							objPower = power;
						}
					}
				}

				Log.Info("blnHasPower = " + blnHasPower);

				if (blnHasPower)
				{
					// If yes, mark it free or give it free levels
					if (blnFree)
					{
						objPower.Free = true;
					}
					else
					{
						objPower.FreeLevels += intLevels;
						if (objPower.Rating < objPower.FreeLevels)
							objPower.Rating = objPower.FreeLevels;
					}
				}
				else
				{
					Log.Info("Adding Power " + strPowerName);
					// If no, add the power and mark it free or give it free levels
					objPower = new Power(_objCharacter);
					_objCharacter.Powers.Add(objPower);

					// Get the Power information
					XmlDocument objXmlDocument = new XmlDocument();
					objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]");
					Log.Info("objXmlPower = " + objXmlPower.OuterXml.ToString());

					bool blnLevels = false;
					if (objXmlPower["levels"] != null)
						blnLevels = (objXmlPower["levels"].InnerText != "no");
					objPower.LevelsEnabled = blnLevels;
					objPower.Name = strPowerNameLimit;
					if (strSelection != string.Empty)
						objPower.Extra = strSelection;
					if (objXmlPower["doublecost"] != null)
						objPower.DoubleCost = false;
					objPower.PointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
					objPower.Source = objXmlPower["source"].InnerText;
					objPower.Page = objXmlPower["page"].InnerText;

					if (objPower.LevelsEnabled)
					{
						if (objPower.Name == "Improved Ability (skill)")
						{
							foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
							{
								if (objPower.Extra == objSkill.Name ||
									(objSkill.IsExoticSkill &&
									 objPower.Extra == (objSkill.DisplayName + " (" + (objSkill as ExoticSkill).Specific + ")")))
								{
									int intImprovedAbilityMaximum = objSkill.Rating + (objSkill.Rating / 2);
									if (intImprovedAbilityMaximum == 0)
									{
										intImprovedAbilityMaximum = 1;
									}
									objPower.MaxLevels = intImprovedAbilityMaximum;
								}
							}
						}
						else if (objXmlPower["levels"].InnerText != "yes")
						{
							objPower.MaxLevels = Convert.ToInt32(objXmlPower["levels"].InnerText);
						}
						else
						{
							objPower.MaxLevels = _objCharacter.MAG.TotalValue;
						}
					}

					if (blnFree && objPower.MaxLevels == 0)
					{
						objPower.Free = true;
					}
					else
					{
						objPower.FreeLevels += intLevels;
						if (objPower.Rating < intLevels)
							objPower.Rating = objPower.FreeLevels;
					}

					if (objXmlPower.InnerXml.Contains("bonus"))
					{
						objPower.Bonus = objXmlPower["bonus"];
						Log.Info("Calling CreateImprovements");
						if (
							!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false,
								Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
						{
							_objCharacter.Powers.Remove(objPower);
						}
					}
				}
				SelectedValue = "";
				ForcedValue = "";
				strSelection = "";
			}
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPower, "");
		}

		// Select a Power.
		public void selectpowers(XmlNode bonusNode)
		{
			XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower");
			foreach (XmlNode objNode in objXmlPowerList)
			{
				Log.Info("selectpower");
				Log.Info("_strSelectedValue = " + SelectedValue);
				Log.Info("_strForcedValue = " + ForcedValue);

				//Gerry: These unfortunately did not work in any case of multiple bonuses
				// Switched the setting of powerpoints and levels to ADDING them
				// Remove resetting powerpoints.
				bool blnExistingPower = false;
				foreach (Power objExistingPower in _objCharacter.Powers)
				{
					if (objExistingPower.Name.StartsWith("Improved Reflexes"))
					{
						if (objExistingPower.Name.EndsWith("1"))
						{
							if (objExistingPower.Name.EndsWith("1"))
							{
								if (_intRating >= 6)
									objExistingPower.FreePoints += 1.5M;
								//else
								//	objExistingPower.FreePoints = 0;
							}
							else if (objExistingPower.Name.EndsWith("2"))
							{
								if (_intRating >= 10)
									objExistingPower.FreePoints += 2.5M;
								else if (_intRating >= 4)
									objExistingPower.FreePoints += 1.0M;
								//else
								//	objExistingPower.FreePoints = 0;
							}
							else
							{
								if (_intRating >= 14)
									objExistingPower.FreePoints += 3.5M;
								else if (_intRating >= 8)
									objExistingPower.FreePoints += 2.0M;
								else if (_intRating >= 4)
									objExistingPower.FreePoints += 1.0M;
								//else
								//	objExistingPower.FreePoints = 0;
							}
						}
						else
						{
							// we have to adjust the number of free levels.
							decimal decLevels = Convert.ToDecimal(_intRating) / 4;
							decLevels = Math.Floor(decLevels / objExistingPower.PointsPerLevel);
							objExistingPower.FreeLevels += Convert.ToInt32(decLevels);
							if (objExistingPower.Rating < _intRating)
								objExistingPower.Rating = objExistingPower.FreeLevels;
							break;
						}
					}
					else
					{
						// we have to adjust the number of free levels.
						decimal decLevels = Convert.ToDecimal(_intRating) / 4;
						decLevels = Math.Floor(decLevels / objExistingPower.PointsPerLevel);
						objExistingPower.FreeLevels = Convert.ToInt32(decLevels);
						if (objExistingPower.Rating < _intRating)
							objExistingPower.Rating = objExistingPower.FreeLevels;
						break;
					}
					//}
				}

				if (!blnExistingPower)
				{
					// Display the Select Skill window and record which Skill was selected.
					frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
					Log.Info("selectpower = " + objNode.OuterXml.ToString());

					if (objNode.OuterXml.Contains("limittopowers"))
						frmPickPower.LimitToPowers = objNode.Attributes["limittopowers"].InnerText;
					frmPickPower.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickPower.DialogResult == DialogResult.Cancel)
					{
						throw new AbortedException();
					}

					SelectedValue = frmPickPower.SelectedPower;
					if (_blnConcatSelectedValue)
						SourceName += " (" + SelectedValue + ")";

					XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objXmlPower =
						objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + SelectedValue + "\"]");
					string strSelection = "";

					Log.Info("_strSelectedValue = " + SelectedValue);
					Log.Info("SourceName = " + SourceName);

					XmlNode objBonus = objXmlPower["bonus"];

					string strPowerNameLimit = SelectedValue;
					if (objBonus != null)
					{
						if (objBonus["selectlimit"] != null)
						{
							Log.Info("selectlimit = " + objBonus["selectlimit"].OuterXml.ToString());
							ForcedValue = "";
							// Display the Select Limit window and record which Limit was selected.
							frmSelectLimit frmPickLimit = new frmSelectLimit();
							if (_strFriendlyName != "")
								frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
									.Replace("{0}", _strFriendlyName);
							else
								frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

							if (objBonus["selectlimit"].InnerXml.Contains("<limit>"))
							{
								List<string> strValue = new List<string>();
								foreach (XmlNode objXmlAttribute in objBonus["selectlimit"].SelectNodes("limit"))
									strValue.Add(objXmlAttribute.InnerText);
								frmPickLimit.LimitToList(strValue);
							}

							if (objBonus["selectlimit"].InnerXml.Contains("<excludelimit>"))
							{
								List<string> strValue = new List<string>();
								foreach (XmlNode objXmlAttribute in objBonus["selectlimit"].SelectNodes("excludelimit"))
									strValue.Add(objXmlAttribute.InnerText);
								frmPickLimit.RemoveFromList(strValue);
							}

							// Check to see if there is only one possible selection because of _strLimitSelection.
							if (ForcedValue != "")
								LimitSelection = ForcedValue;

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strLimitSelection = " + LimitSelection);

							if (LimitSelection != "")
							{
								frmPickLimit.SingleLimit(LimitSelection);
								frmPickLimit.Opacity = 0;
							}

							frmPickLimit.ShowDialog();

							// Make sure the dialogue window was not canceled.
							if (frmPickLimit.DialogResult == DialogResult.Cancel)
							{
								throw new AbortedException();
							}

							SelectedValue = frmPickLimit.SelectedLimit;
							strSelection = SelectedValue;
							ForcedValue = SelectedValue;

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strLimitSelection = " + LimitSelection);
						}

						if (objBonus["selectskill"] != null)
						{
							Log.Info("selectskill = " + objBonus["selectskill"].OuterXml.ToString());
							XmlNode nodSkill = objBonus;
							// Display the Select Skill window and record which Skill was selected.
							frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
							if (_strFriendlyName != "")
								frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
									.Replace("{0}", _strFriendlyName);
							else
								frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

							if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillgroup"))
								frmPickSkill.OnlySkillGroup = nodSkill.SelectSingleNode("selectskill").Attributes["skillgroup"].InnerText;
							else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillcategory"))
								frmPickSkill.OnlyCategory = nodSkill.SelectSingleNode("selectskill").Attributes["skillcategory"].InnerText;
							else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("excludecategory"))
								frmPickSkill.ExcludeCategory = nodSkill.SelectSingleNode("selectskill").Attributes["excludecategory"].InnerText;
							else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("limittoskill"))
								frmPickSkill.LimitToSkill = nodSkill.SelectSingleNode("selectskill").Attributes["limittoskill"].InnerText;

							if (ForcedValue.StartsWith("Adept:") || ForcedValue.StartsWith("Magician:"))
								ForcedValue = "";

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strLimitSelection = " + LimitSelection);

							if (ForcedValue != "")
							{
								frmPickSkill.OnlySkill = ForcedValue;
								frmPickSkill.Opacity = 0;
							}
							frmPickSkill.ShowDialog();

							// Make sure the dialogue window was not canceled.
							if (frmPickSkill.DialogResult == DialogResult.Cancel)
							{
								throw new AbortedException();
							}

							SelectedValue = frmPickSkill.SelectedSkill;
							ForcedValue = SelectedValue;
							strSelection = SelectedValue;

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strSelectedValue = " + SelectedValue);
							Log.Info("strSelection = " + strSelection);
						}

						if (objBonus["selecttext"] != null)
						{
							Log.Info("selecttext = " + objBonus["selecttext"].OuterXml.ToString());
							frmSelectText frmPickText = new frmSelectText();
							frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
								.Replace("{0}", _strFriendlyName);

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strLimitSelection = " + LimitSelection);

							if (LimitSelection != "")
							{
								frmPickText.SelectedValue = LimitSelection;
								frmPickText.Opacity = 0;
							}

							frmPickText.ShowDialog();

							// Make sure the dialogue window was not canceled.
							if (frmPickText.DialogResult == DialogResult.Cancel)
							{
								throw new AbortedException();
							}

							strSelection = frmPickText.SelectedValue;
							LimitSelection = strSelection;

							Log.Info("_strLimitSelection = " + LimitSelection);
							Log.Info("strSelection = " + strSelection);
						}

						if (objBonus["specificattribute"] != null)
						{
							Log.Info("specificattribute = " + objBonus["specificattribute"].OuterXml.ToString());
							strSelection = objBonus["specificattribute"]["name"].InnerText.ToString();
							Log.Info("strSelection = " + strSelection);
						}

						if (objBonus["selectattribute"] != null)
						{
							Log.Info("selectattribute = " + objBonus["selectattribute"].OuterXml.ToString());
							XmlNode nodSkill = objBonus;
							if (ForcedValue.StartsWith("Adept"))
								ForcedValue = "";

							// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
							frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
							if (_strFriendlyName != "")
								frmPickAttribute.Description =
									LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", _strFriendlyName);
							else
								frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

							// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
							if (_objCharacter.MAGEnabled)
								frmPickAttribute.AddMAG();
							if (_objCharacter.RESEnabled)
								frmPickAttribute.AddRES();

							if (nodSkill["selectattribute"].InnerXml.Contains("<attribute>"))
							{
								List<string> strValue = new List<string>();
								foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("attribute"))
									strValue.Add(objXmlAttribute.InnerText);
								frmPickAttribute.LimitToList(strValue);
							}

							if (nodSkill["selectattribute"].InnerXml.Contains("<excludeattribute>"))
							{
								List<string> strValue = new List<string>();
								foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("excludeattribute"))
									strValue.Add(objXmlAttribute.InnerText);
								frmPickAttribute.RemoveFromList(strValue);
							}

							// Check to see if there is only one possible selection because of _strLimitSelection.
							if (ForcedValue != "")
								LimitSelection = ForcedValue;

							Log.Info("_strForcedValue = " + ForcedValue);
							Log.Info("_strLimitSelection = " + LimitSelection);

							if (LimitSelection != "")
							{
								frmPickAttribute.SingleAttribute(LimitSelection);
								frmPickAttribute.Opacity = 0;
							}

							frmPickAttribute.ShowDialog();

							// Make sure the dialogue window was not canceled.
							if (frmPickAttribute.DialogResult == DialogResult.Cancel)
							{
								throw new AbortedException();
							}

							SelectedValue = frmPickAttribute.SelectedAttribute;
							if (_blnConcatSelectedValue)
								SourceName += " (" + SelectedValue + ")";
							strSelection = SelectedValue;
							ForcedValue = SelectedValue;

							Log.Info("_strSelectedValue = " + SelectedValue);
							Log.Info("SourceName = " + SourceName);
							Log.Info("_strForcedValue = " + ForcedValue);
						}
					}

					// If no, add the power and mark it free or give it free levels
					Power objPower = new Power(_objCharacter);
					bool blnHasPower = false;

					foreach (Power power in _objCharacter.Powers)
					{
						if (power.Name == objXmlPower["name"].InnerText)
						{
							if (power.Extra != "" && power.Extra == strSelection)
							{
								blnHasPower = true;
								objPower = power;
							}
							else if (power.Extra == "")
							{
								blnHasPower = true;
								objPower = power;
							}
						}
					}

					Log.Info("blnHasPower = " + blnHasPower);

					if (blnHasPower)
					{
						// If yes, mark it free or give it free levels
						if (objXmlPower["levels"].InnerText == "no")
						{
							if (objPower.Name.StartsWith("Improved Reflexes"))
							{
								if (objPower.Name.EndsWith("1"))
								{
									if (_intRating >= 6)
										objPower.FreePoints = 1.5M;
									else
										objPower.FreePoints = 0;
								}
								else if (objPower.Name.EndsWith("2"))
								{
									if (_intRating >= 10)
										objPower.FreePoints = 2.5M;
									else if (_intRating >= 4)
										objPower.FreePoints = 1.0M;
									else
										objPower.FreePoints = 0;
								}
								else
								{
									if (_intRating >= 14)
										objPower.FreePoints = 3.5M;
									else if (_intRating >= 8)
										objPower.FreePoints = 2.0M;
									else if (_intRating >= 4)
										objPower.FreePoints = 1.0M;
									else
										objPower.FreePoints = 0;
								}
							}
							else
							{
								objPower.Free = true;
							}
						}
						else
						{
							decimal decLevels = Convert.ToDecimal(_intRating) / 4;
							decLevels = Math.Floor(decLevels / objPower.PointsPerLevel);
							objPower.FreeLevels += Convert.ToInt32(decLevels);
							objPower.Rating += Convert.ToInt32(decLevels);
						}
						objPower.BonusSource = SourceName;
					}
					else
					{
						Log.Info("Adding Power " + SelectedValue);
						// Get the Power information
						_objCharacter.Powers.Add(objPower);
						Log.Info("objXmlPower = " + objXmlPower.OuterXml.ToString());

						bool blnLevels = false;
						if (objXmlPower["levels"] != null)
							blnLevels = (objXmlPower["levels"].InnerText == "yes");
						objPower.LevelsEnabled = blnLevels;
						objPower.Name = objXmlPower["name"].InnerText;
						objPower.PointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
						objPower.Source = objXmlPower["source"].InnerText;
						objPower.Page = objXmlPower["page"].InnerText;
						objPower.BonusSource = SourceName;
						if (strSelection != string.Empty)
							objPower.Extra = strSelection;
						if (objXmlPower["doublecost"] != null)
							objPower.DoubleCost = false;

						if (objXmlPower["levels"].InnerText == "no")
						{
							if (objPower.Name.StartsWith("Improved Reflexes"))
							{
								if (objPower.Name.EndsWith("1"))
								{
									if (_intRating >= 6)
										objPower.FreePoints = 1.5M;
									else
										objPower.FreePoints = 0;
								}
								else if (objPower.Name.EndsWith("2"))
								{
									if (_intRating >= 10)
										objPower.FreePoints = 2.5M;
									else if (_intRating >= 4)
										objPower.FreePoints = 1.0M;
									else
										objPower.FreePoints = 0;
								}
								else
								{
									if (_intRating >= 14)
										objPower.FreePoints = 3.5M;
									else if (_intRating >= 8)
										objPower.FreePoints = 2.0M;
									else if (_intRating >= 4)
										objPower.FreePoints = 1.0M;
									else
										objPower.FreePoints = 0;
								}
							}
							else
							{
								objPower.Free = true;
							}
						}
						else
						{
							decimal decLevels = Convert.ToDecimal(_intRating) / 4;
							decLevels = Math.Floor(decLevels / objPower.PointsPerLevel);
							objPower.FreeLevels += Convert.ToInt32(decLevels);
							if (objPower.Rating < _intRating)
								objPower.Rating = objPower.FreeLevels;
						}

						if (objXmlPower.InnerXml.Contains("bonus"))
						{
							objPower.Bonus = objXmlPower["bonus"];
							Log.Info("Calling CreateImprovements");
							if (
								!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false,
									Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
							{
								_objCharacter.Powers.Remove(objPower);
							}
						}
					}
				}
			}
		}

		// Check for Armor Encumbrance Penalty.
		public void armorencumbrancepenalty(XmlNode bonusNode)
		{
			Log.Info("armorencumbrancepenalty");
			Log.Info("armorencumbrancepenalty = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Initiation.
		public void initiation(XmlNode bonusNode)
		{
			Log.Info("initiation");
			Log.Info("initiation = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Initiation, "",
				ValueToInt(bonusNode.InnerText, _intRating));
			_objCharacter.InitiateGrade += ValueToInt(bonusNode.InnerText, _intRating);
		}

		// Check for Submersion.
		public void submersion(XmlNode bonusNode)
		{
			Log.Info("submersion");
			Log.Info("submersion = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Submersion, "",
				ValueToInt(bonusNode.InnerText, _intRating));
			_objCharacter.SubmersionGrade += ValueToInt(bonusNode.InnerText, _intRating);
		}

		// Check for Skillwires.
		public void skillwire(XmlNode bonusNode)
		{
			Log.Info("skillwire");
			Log.Info("skillwire = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Skillwire, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Hardwires.
		public void hardwires(XmlNode bonusNode)
		{
			Log.Info("hardwire");
			Log.Info("hardwire = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			Cyberware objCyberware = new Cyberware(_objCharacter);
			CommonFunctions _objFunctions = new CommonFunctions();
			objCyberware = _objFunctions.FindCyberware(SourceName, _objCharacter.Cyberware);
			if (objCyberware == null)
			{
				Log.Info("_strSelectedValue = " + SelectedValue);
				Log.Info("_strForcedValue = " + ForcedValue);

				// Display the Select Skill window and record which Skill was selected.
				frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
				if (_strFriendlyName != "")
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
						.Replace("{0}", _strFriendlyName);
				else
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

				Log.Info("selectskill = " + bonusNode.OuterXml.ToString());
				if (bonusNode.OuterXml.Contains("skillgroup"))
					frmPickSkill.OnlySkillGroup = bonusNode.Attributes["skillgroup"].InnerText;
				else if (bonusNode.OuterXml.Contains("skillcategory"))
					frmPickSkill.OnlyCategory = bonusNode.Attributes["skillcategory"].InnerText;
				else if (bonusNode.OuterXml.Contains("excludecategory"))
					frmPickSkill.ExcludeCategory = bonusNode.Attributes["excludecategory"].InnerText;
				else if (bonusNode.OuterXml.Contains("limittoskill"))
					frmPickSkill.LimitToSkill = bonusNode.Attributes["limittoskill"].InnerText;
				else if (bonusNode.OuterXml.Contains("limittoattribute"))
					frmPickSkill.LinkedAttribute = bonusNode.Attributes["limittoattribute"].InnerText;

				if (ForcedValue != "")
				{
					frmPickSkill.OnlySkill = ForcedValue;
					frmPickSkill.Opacity = 0;
				}
				frmPickSkill.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickSkill.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				SelectedValue = frmPickSkill.SelectedSkill;
			}
			else
			{
				SelectedValue = objCyberware.Location;
			}
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);
			CreateImprovement(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Hardwire,
				SelectedValue,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Damage Resistance.
		public void damageresistance(XmlNode bonusNode)
		{
			Log.Info("damageresistance");
			Log.Info("damageresistance = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.DamageResistance, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Restricted Item Count.
		public void restricteditemcount(XmlNode bonusNode)
		{
			Log.Info("restricteditemcount");
			Log.Info("restricteditemcount = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.RestrictedItemCount, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Judge Intentions.
		public void judgeintentions(XmlNode bonusNode)
		{
			Log.Info("judgeintentions");
			Log.Info("judgeintentions = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentions, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Composure.
		public void composure(XmlNode bonusNode)
		{
			Log.Info("composure");
			Log.Info("composure = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Composure, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Lift and Carry.
		public void liftandcarry(XmlNode bonusNode)
		{
			Log.Info("liftandcarry");
			Log.Info("liftandcarry = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.LiftAndCarry, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Memory.
		public void memory(XmlNode bonusNode)
		{
			Log.Info("memory");
			Log.Info("memory = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Memory, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Concealability.
		public void concealability(XmlNode bonusNode)
		{
			Log.Info("concealability");
			Log.Info("concealability = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Concealability, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Drain Resistance.
		public void drainresist(XmlNode bonusNode)
		{
			Log.Info("drainresist");
			Log.Info("drainresist = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.DrainResistance, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Fading Resistance.
		public void fadingresist(XmlNode bonusNode)
		{
			Log.Info("fadingresist");
			Log.Info("fadingresist = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.FadingResistance, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Notoriety.
		public void notoriety(XmlNode bonusNode)
		{
			Log.Info("notoriety");
			Log.Info("notoriety = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.Notoriety, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Complex Form Limit.
		public void complexformlimit(XmlNode bonusNode)
		{
			Log.Info("complexformlimit");
			Log.Info("complexformlimit = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ComplexFormLimit, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Spell Limit.
		public void spelllimit(XmlNode bonusNode)
		{
			Log.Info("spelllimit");
			Log.Info("spelllimit = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SpellLimit, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Spell Category bonuses.
		public void spellcategory(XmlNode bonusNode)
		{
			Log.Info("spellcategory");
			Log.Info("spellcategory = " + bonusNode.OuterXml.ToString());

			string strUseUnique = _strUnique;
			if (bonusNode["name"].Attributes["precedence"] != null)
				strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

			Log.Info("Calling CreateImprovement");
			CreateImprovement(bonusNode["name"].InnerText, _objImprovementSource, SourceName,
				Improvement.ImprovementType.SpellCategory, strUseUnique, ValueToInt(bonusNode["val"].InnerText, _intRating));
		}

		// Check for Throwing Range bonuses.
		public void throwrange(XmlNode bonusNode)
		{
			Log.Info("throwrange");
			Log.Info("throwrange = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRange, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Throwing STR bonuses.
		public void throwstr(XmlNode bonusNode)
		{
			Log.Info("throwstr");
			Log.Info("throwstr = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowSTR, _strUnique,
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Skillsoft access.
		public void skillsoftaccess(XmlNode bonusNode)
		{
			Log.Info("skillsoftaccess");
			Log.Info("skillsoftaccess = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.SkillsoftAccess, "");
			_objCharacter.SkillsSection.KnowledgeSkills.AddRange(_objCharacter.SkillsSection.KnowsoftSkills);
		}

		// Check for Quickening Metamagic.
		public void quickeningmetamagic(XmlNode bonusNode)
		{
			Log.Info("quickeningmetamagic");
			Log.Info("quickeningmetamagic = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.QuickeningMetamagic, "");
		}

		// Check for ignore Stun CM Penalty.
		public void ignorecmpenaltystun(XmlNode bonusNode)
		{
			Log.Info("ignorecmpenaltystun");
			Log.Info("ignorecmpenaltystun = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, "");
		}

		// Check for ignore Physical CM Penalty.
		public void ignorecmpenaltyphysical(XmlNode bonusNode)
		{
			Log.Info("ignorecmpenaltyphysical");
			Log.Info("ignorecmpenaltyphysical = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, "");
		}

		// Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
		public void cyborgessence(XmlNode bonusNode)
		{
			Log.Info("cyborgessence");
			Log.Info("cyborgessence = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.CyborgEssence, "");
		}

		// Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
		public void essencemax(XmlNode bonusNode)
		{
			Log.Info("essencemax");
			Log.Info("essencemax = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.EssenceMax, "",
				ValueToInt(bonusNode.InnerText, _intRating));
		}

		// Check for Select Sprite.
		public void selectsprite(XmlNode bonusNode)
		{
			Log.Info("selectsprite");
			Log.Info("selectsprite = " + bonusNode.OuterXml.ToString());
			XmlDocument objXmlDocument = XmlManager.Instance.Load("critters.xml");
			XmlNodeList objXmlNodeList =
				objXmlDocument.SelectNodes("/chummer/metatypes/metatype[contains(category, \"Sprites\")]");
			List<ListItem> lstCritters = new List<ListItem>();
			foreach (XmlNode objXmlNode in objXmlNodeList)
			{
				ListItem objItem = new ListItem();
				if (objXmlNode["translate"] != null)
					objItem.Name = objXmlNode["translate"].InnerText;
				else
					objItem.Name = objXmlNode["name"].InnerText;
				objItem.Value = objItem.Name;
				lstCritters.Add(objItem);
			}

			frmSelectItem frmPickItem = new frmSelectItem();
			frmPickItem.GeneralItems = lstCritters;
			frmPickItem.ShowDialog();

			if (frmPickItem.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickItem.SelectedItem;

			Log.Info("Calling CreateImprovement");
			CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
				Improvement.ImprovementType.AddSprite,
				"");
		}

		// Check for Black Market Discount.
		public void blackmarketdiscount(XmlNode bonusNode)
		{
			Log.Info("blackmarketdiscount");
			Log.Info("blackmarketdiscount = " + bonusNode.OuterXml.ToString());
			Log.Info("Calling CreateImprovement");
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.BlackMarketDiscount,
				_strUnique);
			_objCharacter.BlackMarketDiscount = true;
		}

		// Select Armor (Mostly used for Custom Fit (Stack)).
		public void selectarmor(XmlNode bonusNode)
		{
			Log.Info("selectarmor");
			Log.Info("selectarmor = " + bonusNode.OuterXml.ToString());
			string strSelectedValue = "";
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			// Display the Select Item window and record the value that was entered.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("armor.xml");
			XmlNodeList objXmlNodeList;
			
			if (!string.IsNullOrEmpty(bonusNode.InnerText))
			{
				objXmlNodeList = objXmlDocument.SelectNodes("/chummer/armors/armor[name starts-with " + bonusNode.InnerText + "(" + _objCharacter.Options.BookXPath() +
															") and category = 'High-Fashion Armor Clothing' and mods[name = 'Custom Fit']]");
			}
			else
			{
				objXmlNodeList =
					objXmlDocument.SelectNodes("/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() +
											   ") and category = 'High-Fashion Armor Clothing' and mods[name = 'Custom Fit']]");
			}

			//.SelectNodes("/chummer/skills/skill[not(exotic) and (" + _objCharacter.Options.BookXPath() + ")" + SkillFilter(filter) + "]");

			List<ListItem> lstArmors = new List<ListItem>();
			foreach (XmlNode objNode in objXmlNodeList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objNode["name"].InnerText;
				objItem.Name = objNode.Attributes["translate"]?.InnerText ?? objNode["name"].InnerText;
				lstArmors.Add(objItem);
			}

			if (lstArmors.Count > 0)
			{

				frmSelectItem frmPickItem = new frmSelectItem();
				frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
					.Replace("{0}", _strFriendlyName);
				frmPickItem.GeneralItems = lstArmors;

				Log.Info("_strLimitSelection = " + LimitSelection);
				Log.Info("_strForcedValue = " + ForcedValue);

				if (LimitSelection != "")
				{
					frmPickItem.ForceItem = LimitSelection;
					frmPickItem.Opacity = 0;
				}

				frmPickItem.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickItem.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				SelectedValue = frmPickItem.SelectedItem;
				if (_blnConcatSelectedValue)
					SourceName += " (" + SelectedValue + ")";

				strSelectedValue = frmPickItem.SelectedItem;
				Log.Info("_strSelectedValue = " + SelectedValue);
				Log.Info("SelectedValue = " + strSelectedValue);
			}

		}

		// Select Weapon (custom entry for things like Spare Clip).
		public void selectweapon(XmlNode bonusNode)
		{
			Log.Info("selectweapon");
			Log.Info("selectweapon = " + bonusNode.OuterXml.ToString());
			string strSelectedValue = "";
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			if (_objCharacter == null)
			{
				// If the character is null (this is a Vehicle), the user must enter their own string.
				// Display the Select Item window and record the value that was entered.
				frmSelectText frmPickText = new frmSelectText();
				frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
					.Replace("{0}", _strFriendlyName);

				Log.Info("_strLimitSelection = " + LimitSelection);
				Log.Info("_strForcedValue = " + ForcedValue);

				if (LimitSelection != "")
				{
					frmPickText.SelectedValue = LimitSelection;
					frmPickText.Opacity = 0;
				}

				frmPickText.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickText.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				SelectedValue = frmPickText.SelectedValue;
				if (_blnConcatSelectedValue)
					SourceName += " (" + SelectedValue + ")";

				strSelectedValue = frmPickText.SelectedValue;
				Log.Info("_strSelectedValue = " + SelectedValue);
				Log.Info("SelectedValue = " + strSelectedValue);
			}
			else
			{
				string strExclude = "";
				List <ListItem> lstWeapons = new List<ListItem>();
				strExclude = bonusNode.Attributes["excludecategory"]?.InnerText;
				foreach (Weapon objWeapon in _objCharacter.Weapons)
				{
					bool blnAdd = !(strExclude != "" && objWeapon.WeaponType == strExclude);
					if (blnAdd)
					{
						ListItem objItem = new ListItem();
						objItem.Value = objWeapon.Name;
						objItem.Name = objWeapon.DisplayName;
						lstWeapons.Add(objItem);
					}
				}

				frmSelectItem frmPickItem = new frmSelectItem();
				frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
					.Replace("{0}", _strFriendlyName);
				frmPickItem.GeneralItems = lstWeapons;

				Log.Info("_strLimitSelection = " + LimitSelection);
				Log.Info("_strForcedValue = " + ForcedValue);

				if (LimitSelection != "")
				{
					frmPickItem.ForceItem = LimitSelection;
					frmPickItem.Opacity = 0;
				}

				frmPickItem.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickItem.DialogResult == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				SelectedValue = frmPickItem.SelectedItem;
				if (_blnConcatSelectedValue)
					SourceName += " (" + SelectedValue + ")";

				strSelectedValue = frmPickItem.SelectedItem;
				Log.Info("_strSelectedValue = " + SelectedValue);
				Log.Info("SelectedValue = " + strSelectedValue);
			}

			// Create the Improvement.
			Log.Info("Calling CreateImprovement");
			CreateImprovement(strSelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique);
		}

		// Select an Optional Power.
		public void optionalpowers(XmlNode bonusNode)
		{
			XmlNodeList objXmlPowerList = bonusNode.SelectNodes("optionalpower");
			Log.Info("selectoptionalpower");
			// Display the Select Attribute window and record which Skill was selected.
			frmSelectOptionalPower frmPickPower = new frmSelectOptionalPower();
			frmPickPower.Description = LanguageManager.Instance.GetString("String_Improvement_SelectOptionalPower");
			string strForcedValue = "";

			List<KeyValuePair<string, string>> lstValue = new List<KeyValuePair<string, string>>();
			foreach (XmlNode objXmlOptionalPower in objXmlPowerList)
			{
				string strQuality = objXmlOptionalPower.InnerText;
				if (objXmlOptionalPower.Attributes["select"] != null)
				{
					strForcedValue = objXmlOptionalPower.Attributes["select"].InnerText;
				}
				lstValue.Add(new KeyValuePair<string, string>(strQuality, strForcedValue));
			}
			frmPickPower.LimitToList(lstValue);


			// Check to see if there is only one possible selection because of _strLimitSelection.
			if (ForcedValue != "")
				LimitSelection = ForcedValue;

			Log.Info("_strForcedValue = " + ForcedValue);
			Log.Info("_strLimitSelection = " + LimitSelection);

			if (LimitSelection != "")
			{
				frmPickPower.SinglePower(LimitSelection);
				frmPickPower.Opacity = 0;
			}

			frmPickPower.ShowDialog();

			// Make sure the dialogue window was not canceled.
			if (frmPickPower.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			strForcedValue = frmPickPower.SelectedPowerExtra;
			// Record the improvement.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
			XmlNode objXmlPowerNode =
				objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + frmPickPower.SelectedPower + "\"]");
			TreeNode objPowerNode = new TreeNode();
			CritterPower objPower = new CritterPower(_objCharacter);

			objPower.Create(objXmlPowerNode, _objCharacter, objPowerNode, 0, strForcedValue);
			_objCharacter.CritterPowers.Add(objPower);
		}

		public void critterpowers(XmlNode bonusNode)
		{
			XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
			foreach (XmlNode objXmlPower in bonusNode.SelectNodes("power"))
			{
				XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
				TreeNode objPowerNode = new TreeNode();
				CritterPower objPower = new CritterPower(_objCharacter);
				string strForcedValue = "";
				int intRating = 0;
				if (objXmlPower.Attributes != null && objXmlPower.Attributes.Count > 0)
				{
					intRating = Convert.ToInt32(objXmlPower.Attributes["rating"]?.InnerText);
					strForcedValue = objXmlPower.Attributes["select"]?.InnerText;
				}

				objPower.Create(objXmlCritterPower, _objCharacter, objPowerNode, intRating, strForcedValue);
				_objCharacter.CritterPowers.Add(objPower);
			}
		}

		public void publicawareness(XmlNode bonusNode)
		{
			CreateImprovement("", _objImprovementSource, SourceName, Improvement.ImprovementType.PublicAwareness, _strUnique,
				ValueToInt(bonusNode.InnerText, 1));
		}

		public void dealerconnection(XmlNode bonusNode)
		{
			Log.Info("dealerconnection");
			frmSelectItem frmPickItem = new frmSelectItem();
			List<ListItem> lstItems = new List<ListItem>();
			XmlNodeList objXmlList = bonusNode.SelectNodes("category");
			foreach (XmlNode objNode in objXmlList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objNode.InnerText;
				objItem.Name = objNode.InnerText;
				lstItems.Add(objItem);
			}
			frmPickItem.GeneralItems = lstItems;
			frmPickItem.AllowAutoSelect = false;
			frmPickItem.ShowDialog();
			// Make sure the dialogue window was not canceled.
			if (frmPickItem.DialogResult == DialogResult.Cancel)
			{
				throw new AbortedException();
			}

			SelectedValue = frmPickItem.SelectedItem;
			if (_blnConcatSelectedValue)
				SourceName += " (" + SelectedValue + ")";

			Log.Info("_strSelectedValue = " + SelectedValue);
			Log.Info("SourceName = " + SourceName);

			// Create the Improvement.
			Log.Info("Calling CreateImprovement");
			CreateImprovement(frmPickItem.SelectedItem, _objImprovementSource, SourceName,
				Improvement.ImprovementType.DealerConnection, _strUnique);
		}

		public void unlockskills(XmlNode bonusNode)
		{
			List<string> options = bonusNode.InnerText.Split(',').Select(x => x.Trim()).ToList();
			string final;
			if (options.Count == 0)
			{
				Utils.BreakIfDebug();
				throw new AbortedException();
			}
			else if (options.Count == 1)
			{
				final = options[0];
			}
			else
			{
				frmSelectItem frmSelect = new frmSelectItem
				{
					AllowAutoSelect = true,
					GeneralItems = options.Select(x => new ListItem(x, x)).ToList()
				};

				if (_objCharacter.Pushtext.Count > 0)
				{
					frmSelect.ForceItem = _objCharacter.Pushtext.Pop();
				}

				if (frmSelect.ShowDialog() == DialogResult.Cancel)
				{
					throw new AbortedException();
				}

				final = frmSelect.SelectedItem;
			}

			SkillsSection.FilterOptions skills;
			if (Enum.TryParse(final, out skills))
			{
				_objCharacter.SkillsSection.AddSkills(skills);
				CreateImprovement(skills.ToString(), Improvement.ImprovementSource.Quality, SourceName,
					Improvement.ImprovementType.SpecialSkills, _strUnique);
			}
			else
			{
				Utils.BreakIfDebug();
				Log.Info(new[] { "Failed to parse", "specialskills", bonusNode.OuterXml });
			}
		}

		public void addqualities(XmlNode bonusNode)
		{
			XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
			foreach (XmlNode objXmlAddQuality in bonusNode.SelectNodes("addquality"))
			{
				XmlNode objXmlSelectedQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlAddQuality.InnerText + "\"]");
				string strForceValue = "";
				if (objXmlAddQuality.Attributes["select"] != null)
					strForceValue = objXmlAddQuality.Attributes["select"].InnerText;
				bool blnAddQuality = _objCharacter.Qualities.All(objCharacterQuality => objCharacterQuality.Name != objXmlAddQuality.InnerText || objCharacterQuality.Extra != strForceValue);

				// Make sure the character does not yet have this Quality.

				if (blnAddQuality)
				{
					TreeNode objAddQualityNode = new TreeNode();
					Quality objAddQuality = new Quality(_objCharacter);
					objAddQuality.Create(objXmlSelectedQuality, _objCharacter, QualitySource.Selected, objAddQualityNode, null, null, strForceValue);

					bool blnFree = (objXmlAddQuality.Attributes["contributetobp"] == null ||
					                (objXmlAddQuality.Attributes["contributetobp"]?.InnerText.ToLower() != "true"));
					if (blnFree)
					{
						objAddQuality.BP = 0;
						objAddQuality.ContributeToLimit = false;
					}
					_objCharacter.Qualities.Add(objAddQuality);
					CreateImprovement(objAddQuality.InternalId, Improvement.ImprovementSource.Quality, SourceName,
					Improvement.ImprovementType.SpecificQuality, _strUnique);
				}
			}
		}

		#endregion
	}

	internal class AbortedException : Exception
	{
	}
}
