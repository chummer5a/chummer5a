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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Chummer.Backend.Equipment;
using Chummer.Skills;
using System.Xml;

namespace Chummer
{
	/// <summary>
	/// Contains functionality shared between frmCreate and frmCareer
	/// </summary>
	[System.ComponentModel.DesignerCategory("")]
	public class CharacterShared : Form
	{
		protected Character _objCharacter;
		protected MainController _objController;
		protected CharacterOptions _objOptions;
		protected CommonFunctions _objFunctions;

		public CharacterShared()
		{
			_gunneryCached = new Lazy<Skill>(() => _objCharacter.SkillsSection.Skills.First(x => x.Name == "Gunnery"));
		}


		/// <summary>
		/// Wrapper for relocating contact forms. 
		/// </summary>
		public class TransportWrapper
		{
			private Control control;

			public TransportWrapper(Control control)
			{
				this.control = control;
			}

			public Control Control
			{
				get { return control; }
			}
		}

		protected void RedlinerCheck()
		{
			string strSeekerImprovPrefix = "SEEKER";
			//Get attributes affected by redliner/cyber singularity seeker
			var attributes = new List<string>(
				from improvement in _objCharacter.Improvements
				where improvement.ImproveType == Improvement.ImprovementType.Seeker
				select improvement.ImprovedName);

			//And the improvements comming from there
			var impr = new List<Improvement>(
				from improvement in _objCharacter.Improvements
				where (improvement.ImproveType == Improvement.ImprovementType.Attribute ||
				       improvement.ImproveType == Improvement.ImprovementType.PhysicalCM) &&
				      improvement.SourceName.Contains(strSeekerImprovPrefix)
				//for backwards compability
				select improvement);

			//if neither contains anything, it is safe to exit
			if (impr.Count == 0 && attributes.Count == 0)
			{
				_objCharacter.RedlinerBonus = 0;
				return;
			}

			//Calculate bonus from cyberlimbs
			int count = Math.Min(_objCharacter.Cyberware.Count(c => c.LimbSlot != "" && c.Name.Contains("Full"))/2, 2);
			if (impr.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI"))
			{
				_objCharacter.RedlinerBonus = count;
			}
			else
			{
				_objCharacter.RedlinerBonus = 0;
			}

			for (int i = 0; i < attributes.Count; i++)
			{
				Improvement objImprove =
					impr.FirstOrDefault(
						x =>
							x.SourceName == strSeekerImprovPrefix + "_" + attributes[i] &&
							x.Value == (attributes[i] == "BOX" ? count*-3 : count));
				if (objImprove != null)
				{
					attributes.RemoveAt(i);
					impr.Remove(objImprove);
				}
			}
			//Improvement manager defines the functions we need to manipulate improvements
			//When the locals (someday) gets moved to this class, this can be removed and use
			//the local
			Lazy<ImprovementManager> manager = new Lazy<ImprovementManager>(() => new ImprovementManager(_objCharacter));

			// Remove which qualites have been removed or which values have changed
			foreach (Improvement improvement in impr)
			{
				manager.Value.RemoveImprovements(improvement.ImproveSource, improvement.SourceName);
			}

			// Add new improvements or old improvements with new values
			foreach (string attribute in attributes)
			{
				if (attribute == "BOX")
				{
					manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
						strSeekerImprovPrefix + "_" + attribute, Improvement.ImprovementType.PhysicalCM,
						Guid.NewGuid().ToString(), count*-3);
				}
				else
				{
					manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
						strSeekerImprovPrefix + "_" + attribute, Improvement.ImprovementType.Attribute,
						Guid.NewGuid().ToString(), count, 1, 0, 0, count);
				}
			}
			if (manager.IsValueCreated)
			{
				manager.Value.Commit(); //REFACTOR! WHEN MOVING MANAGER, change this to bool
			}
		}

		/// <summary>
		/// Update the label and tooltip for the character's Condition Monitors.
		/// </summary>
		/// <param name="lblPhysical"></param>
		/// <param name="lblStun"></param>
		/// <param name="tipTooltip"></param>
		/// <param name="_objImprovementManager"></param>
		protected void UpdateConditionMonitor(Label lblPhysical, Label lblStun, ToolTip tipTooltip,
			ImprovementManager _objImprovementManager)
		{
			// Condition Monitor.
			double dblBOD = _objCharacter.BOD.TotalValue;
			double dblWIL = _objCharacter.WIL.TotalValue;
			int intCMPhysical = _objCharacter.PhysicalCM;
			int intCMStun = _objCharacter.StunCM;

			// Update the Condition Monitor labels.
			lblPhysical.Text = intCMPhysical.ToString();
			lblStun.Text = intCMStun.ToString();
			string strCM = "8 + (BOD/2)(" + ((int) Math.Ceiling(dblBOD/2)).ToString() + ")";
			if (_objImprovementManager.ValueOf(Improvement.ImprovementType.PhysicalCM) != 0)
				strCM += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
				         _objImprovementManager.ValueOf(Improvement.ImprovementType.PhysicalCM).ToString() + ")";
			tipTooltip.SetToolTip(lblPhysical, strCM);
			strCM = "8 + (WIL/2)(" + ((int) Math.Ceiling(dblWIL/2)).ToString() + ")";
			if (_objImprovementManager.ValueOf(Improvement.ImprovementType.StunCM) != 0)
				strCM += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
				         _objImprovementManager.ValueOf(Improvement.ImprovementType.StunCM).ToString() + ")";
			tipTooltip.SetToolTip(lblStun, strCM);
		}

		/// <summary>
		/// Update the label and tooltip for the character's Armor Rating.
		/// </summary>
		/// <param name="lblArmor"></param>
		/// <param name="tipTooltip"></param>
		/// <param name="_objImprovementManager"></param>
		/// <param name="lblCMArmor"></param>
		protected void UpdateArmorRating(Label lblArmor, ToolTip tipTooltip, ImprovementManager _objImprovementManager,
			Label lblCMArmor = null)
		{
			// Armor Ratings.
			lblArmor.Text = _objCharacter.TotalArmorRating.ToString();
			string strArmorToolTip = "";
			strArmorToolTip = LanguageManager.Instance.GetString("Tip_Armor") + " (" + _objCharacter.ArmorRating.ToString() + ")";
			if (_objCharacter.ArmorRating != _objCharacter.TotalArmorRating)
				strArmorToolTip += " + " + LanguageManager.Instance.GetString("Tip_Modifiers") + " (" +
				                   (_objCharacter.TotalArmorRating - _objCharacter.ArmorRating).ToString() + ")";
			tipTooltip.SetToolTip(lblArmor, strArmorToolTip);
			if (lblCMArmor != null)
			{
				lblCMArmor.Text = _objCharacter.TotalArmorRating.ToString();
				tipTooltip.SetToolTip(lblCMArmor, strArmorToolTip);
			}

			// Remove any Improvements from Armor Encumbrance.
			_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance");
			// Create the Armor Encumbrance Improvements.
			if (_objCharacter.ArmorEncumbrance < 0)
			{
				_objImprovementManager.CreateImprovement("AGI", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
					Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, _objCharacter.ArmorEncumbrance);
				_objImprovementManager.CreateImprovement("REA", Improvement.ImprovementSource.ArmorEncumbrance, "Armor Encumbrance",
					Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, _objCharacter.ArmorEncumbrance);
			}
		}

		/// <summary>
		/// Update the labels and tooltips for the character's Attributes.
		/// </summary>
		/// <param name="objAttribute"></param>
		/// <param name="lblATTMetatype"></param>
		/// <param name="lblATTAug"></param>
		/// <param name="tipTooltip"></param>
		/// <param name="nudATT"></param>
		protected void UpdateCharacterAttribute(CharacterAttrib objAttribute, Label lblATTMetatype, Label lblATTAug,
			ToolTip tipTooltip, [Optional] NumericUpDown nudATT, [Optional] NumericUpDown nudKATT)
		{
			if (nudATT != null)
			{
				nudATT.Minimum = objAttribute.TotalMinimum;
				nudATT.Maximum = objAttribute.TotalMaximum;
				nudATT.Value = Math.Max(objAttribute.Value - objAttribute.Karma, objAttribute.Base);
			}
			if (nudKATT != null)
			{
				nudKATT.Minimum = 0;
				nudKATT.Maximum = objAttribute.TotalMaximum;
				nudKATT.Value = objAttribute.Karma;
			}
			lblATTMetatype.Text = string.Format("{0} / {1} ({2})", objAttribute.TotalMinimum, objAttribute.TotalMaximum,
				objAttribute.TotalAugmentedMaximum);
			if (objAttribute.HasModifiers)
			{
				lblATTAug.Text = string.Format("{0} ({1})", objAttribute.Value, objAttribute.TotalValue);
				tipTooltip.SetToolTip(lblATTAug, objAttribute.ToolTip());
			}
			else
			{
				lblATTAug.Text = string.Format("{0}", objAttribute.Value);
				tipTooltip.SetToolTip(lblATTAug, "");
			}
		}

		/// <summary>
		/// Update the labels and tooltips for the character's Limits.
		/// </summary>
		/// <param name="lblPhysical"></param>
		/// <param name="lblMental"></param>
		/// <param name="lblSocial"></param>
		/// <param name="lblAstral"></param>
		/// <param name="tipTooltip"></param>
		protected void RefreshLimits(Label lblPhysical, Label lblMental, Label lblSocial, Label lblAstral, ToolTip tipTooltip)
		{
			lblPhysical.Text = _objCharacter.LimitPhysical.ToString();
			string strPhysical = string.Format("({0} [{1}] * 2) + {2} [{3}] + {4} [{5}] / 3", LanguageManager.Instance.GetString("String_AttributeSTRShort"), _objCharacter.STR.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeBODShort"), _objCharacter.BOD.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeREAShort"), _objCharacter.REA.TotalValue.ToString());
			strPhysical = _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled && objImprovement.ImproveType == Improvement.ImprovementType.PhysicalLimit).Aggregate(strPhysical, (current, objImprovement) => current + (" + " + _objCharacter.GetObjectName(objImprovement) + " (" + (objImprovement.Value) + ")"));
			tipTooltip.SetToolTip(lblPhysical, strPhysical);

			lblMental.Text = _objCharacter.LimitMental.ToString();
			string strMental = string.Format("({0} [{1}] * 2) + {2} [{3}] + {4} [{5}] / 3", LanguageManager.Instance.GetString("String_AttributeLOGShort"), _objCharacter.LOG.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeINTShort"), _objCharacter.INT.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeWILShort"), _objCharacter.WIL.TotalValue.ToString());
			strMental = _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled && objImprovement.ImproveType == Improvement.ImprovementType.MentalLimit).Aggregate(strMental, (current, objImprovement) => current + (" + " + _objCharacter.GetObjectName(objImprovement) + " (" + (objImprovement.Value) + ")"));
			tipTooltip.SetToolTip(lblMental, strMental);

			lblSocial.Text = _objCharacter.LimitSocial.ToString();
			string strSocial = string.Format("({0} [{1}] * 2) + {2} [{3}] + {4} [{5}] / 3", LanguageManager.Instance.GetString("String_AttributeCHAShort"), _objCharacter.CHA.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeWILShort"), _objCharacter.WIL.TotalValue.ToString(), LanguageManager.Instance.GetString("String_AttributeESSShort"), _objCharacter.Essence.ToString());
			strSocial = _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled && objImprovement.ImproveType == Improvement.ImprovementType.SocialLimit).Aggregate(strSocial, (current, objImprovement) => current + (" + " + _objCharacter.GetObjectName(objImprovement) + " (" + (objImprovement.Value) + ")"));
			tipTooltip.SetToolTip(lblSocial, strSocial);

			lblAstral.Text = _objCharacter.LimitAstral.ToString();
		}

		private Lazy<Skill> _gunneryCached;

		protected int MountedGunManualOperationDicePool(Weapon weapon)
		{
			return _gunneryCached.Value.Pool;
		}

		protected int MountedGunCommandDeviceDicePool(Weapon weapon)
		{
			return _gunneryCached.Value.PoolOtherAttribute(_objCharacter.LOG.TotalValue);
		}

		protected int MountedGunDogBrainDicePool(Weapon weapon, Vehicle vehicle)
		{
			int pilotRating = vehicle.Pilot;

			Gear maybeAutoSoft =
				vehicle.Gear.SelectMany(x => x.ThisAndAllChildren())
					.FirstOrDefault(x => x.Name == "Autosoft" && (x.Extra == weapon.Name || x.Extra == weapon.DisplayName));

			if (maybeAutoSoft != null)
			{
				return maybeAutoSoft.Rating + pilotRating;
			}

			return 0;
		}

		/// <summary>
		/// Edit and update a Limit Modifier.
		/// </summary>
		/// <param name="treLimit"></param>
		/// <param name="cmsLimitModifier"></param>
		protected void UpdateLimitModifier(TreeView treLimit, ContextMenuStrip cmsLimitModifier)
		{
			TreeNode objSelectedNode = treLimit.SelectedNode;
			LimitModifier objLimitModifier = _objFunctions.FindLimitModifier(treLimit.SelectedNode.Tag.ToString(),
				_objCharacter.LimitModifiers);
			//If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
			if (objLimitModifier == null)
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Warning_NoLimitFound"));
				return;
			}
			frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(objLimitModifier);
			frmPickLimitModifier.ShowDialog(this);

			if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
				return;

			//Remove the old LimitModifier to ensure we don't double up.
			_objCharacter.LimitModifiers.Remove(objLimitModifier);
			// Create the new limit modifier.
			TreeNode objNode = new TreeNode();
			objLimitModifier = new LimitModifier(_objCharacter);
			string strLimit = treLimit.SelectedNode.Parent.Text;
			string strCondition = frmPickLimitModifier.SelectedCondition;
			objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, strLimit,
				strCondition, _objCharacter, objNode);
			objLimitModifier.Guid = new Guid(objSelectedNode.Tag.ToString());
			if (objLimitModifier.InternalId == Guid.Empty.ToString())
				return;

			_objCharacter.LimitModifiers.Add(objLimitModifier);

			//Add the new treeview node for the LimitModifier.
			objNode.ContextMenuStrip = cmsLimitModifier;
			objNode.Text = objLimitModifier.DisplayName;
			objNode.Tag = objLimitModifier.InternalId;
			objSelectedNode.Parent.Nodes.Add(objNode);
			objSelectedNode.Remove();
		}

		/// <summary>
		/// Clears and updates the treeview for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
		/// </summary>
		/// <param name="treCritterPowers">Treenode that will be cleared and populated.</param>
		/// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
		protected void RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers)
		{
			//Clear the default nodes of entries.
			foreach (TreeNode objNode in treCritterPowers.Nodes)
			{
				objNode.Nodes.Clear();
			}
			//Add the Critter Powers that exist.
			foreach (CritterPower objPower in _objCharacter.CritterPowers)
			{
				TreeNode objNode = new TreeNode();
				objNode.Text = objPower.DisplayName;
				objNode.Tag = objPower.InternalId;
				objNode.ContextMenuStrip = cmsCritterPowers;
				if (objPower.Notes != string.Empty)
					objNode.ForeColor = Color.SaddleBrown;
				objNode.ToolTipText = CommonFunctions.WordWrap(objPower.Notes, 100);

				if (objPower.Category != "Weakness")
				{
					treCritterPowers.Nodes[0].Nodes.Add(objNode);
					treCritterPowers.Nodes[0].Expand();
				}
				else
				{
					treCritterPowers.Nodes[1].Nodes.Add(objNode);
					treCritterPowers.Nodes[1].Expand();
				}
			}
		}

		/// <summary>
		/// Refreshes the list of qualities into the selected TreeNode. If the same number of 
		/// </summary>
		/// <param name="treQualities">Treeview to insert the qualities into.</param>
		/// <param name="cmsQuality">ContextMenuStrip to add to each Quality node.</param>
		/// <param name="blnForce">Forces a refresh of the TreeNode despite a match.</param>
		protected void RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, bool blnForce = false)
		{
			//Count the child nodes in each treenode.
			int intQualityCount = 0;
			foreach (TreeNode objTreeNode in treQualities.Nodes)
			{
				intQualityCount += objTreeNode.Nodes.Count;
			}

			//If the node count is the same as the quality count, there's no need to do anything.
			if (intQualityCount != _objCharacter.Qualities.Count || blnForce)
			{
				foreach (TreeNode objTreeNode in treQualities.Nodes)
				{
					objTreeNode.Nodes.Clear();
				}
				// Populate the Qualities list.
				foreach (Quality objQuality in _objCharacter.Qualities)
				{
					TreeNode objNode = new TreeNode();
					objNode.Text = objQuality.DisplayName;
					objNode.Tag = objQuality.InternalId;
					objNode.ContextMenuStrip = cmsQuality;

					if (objQuality.Notes != string.Empty)
						objNode.ForeColor = Color.SaddleBrown;
					else
					{
						if (objQuality.OriginSource == QualitySource.Metatype ||
						    objQuality.OriginSource == QualitySource.MetatypeRemovable)
							objNode.ForeColor = SystemColors.GrayText;
					}
					objNode.ToolTipText = CommonFunctions.WordWrap(objQuality.Notes, 100);

					switch (objQuality.Type)
					{
						case QualityType.Positive:
							treQualities.Nodes[0].Nodes.Add(objNode);
							treQualities.Nodes[0].Expand();
							break;
						case QualityType.Negative:
							treQualities.Nodes[1].Nodes.Add(objNode);
							treQualities.Nodes[1].Expand();
							break;
						case QualityType.LifeModule:
							treQualities.Nodes[2].Nodes.Add(objNode);
							treQualities.Nodes[2].Expand();
							break;
					}
				}
			}
		}

		/// <summary>
		/// Method for removing old <addqualities /> nodes from existing characters.
		/// </summary>
		/// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
		/// <param name="treQualities"></param>
		/// <param name="_objImprovementManager"></param>
		protected void RemoveAddedQualities(XmlNodeList objNodeList, TreeView treQualities,
			ImprovementManager _objImprovementManager)
		{
			foreach (XmlNode objNode in objNodeList)
			{
				foreach (Quality objQuality in _objCharacter.Qualities.Where(objQuality => objQuality.Name == objNode.InnerText))
				{
					switch (objQuality.Type)
					{
						case QualityType.Positive:
							foreach (
								TreeNode nodQuality in
									treQualities.Nodes[0].Nodes.Cast<TreeNode>().Where(nodQuality => nodQuality.Text == objQuality.Name))
							{
								nodQuality.Remove();
							}
							break;
						case QualityType.Negative:
							foreach (
								TreeNode nodQuality in
									treQualities.Nodes[1].Nodes.Cast<TreeNode>().Where(nodQuality => nodQuality.Text == objQuality.Name))
							{
								nodQuality.Remove();
							}
							break;
					}
					_objCharacter.Qualities.Remove(objQuality);
					_objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
					break;
				}
			}
		}

		/// <summary>
		/// Add a mugshot to the character.
		/// </summary>
		/// <param name="picMugshot"></param>
		protected bool AddMugshot(PictureBox picMugshot)
		{
			bool blnSuccess = true;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (!string.IsNullOrWhiteSpace(_objOptions.RecentImageFolder) && Directory.Exists(_objOptions.RecentImageFolder))
			{
				openFileDialog.InitialDirectory = _objOptions.RecentImageFolder;
			}
			// Prompt the user to select an image to associate with this character.

			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
			openFileDialog.Filter = string.Format("All image files ({1})|{1}|{0}|All files|*",
				string.Join("|",
					codecs.Select(codec => string.Format("{0} ({1})|{1}", codec.CodecName, codec.FilenameExtension)).ToArray()),
				string.Join(";", codecs.Select(codec => codec.FilenameExtension).ToArray()));

			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				MemoryStream objStream = new MemoryStream();
				// Convert the image to a string usinb Base64.
				try
				{
					Image imgMugshot = new Bitmap(openFileDialog.FileName);
					imgMugshot.Save(objStream, imgMugshot.RawFormat);
					string strResult = Convert.ToBase64String(objStream.ToArray());

					_objCharacter.Mugshot = strResult;
					picMugshot.Image = imgMugshot;

					objStream.Close();

					_objOptions.RecentImageFolder = Path.GetDirectoryName(openFileDialog.FileName);
				}
				catch
				{
					blnSuccess = false;
				}
			}
			return blnSuccess;
		}
	}
}