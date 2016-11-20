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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectMetamagic : Form
	{
		private string _strSelectedMetamagic = "";

		private Mode _objMode = Mode.Metamagic;
		private string _strNode = "metamagic";
		private string _strRoot = "metamagics";
		private bool _blnAddAgain = false;

		private readonly Character _objCharacter;

		private XmlDocument _objXmlDocument = new XmlDocument();

		private readonly XmlDocument _objMetatypeDocument = new XmlDocument();
		private readonly XmlDocument _objCritterDocument = new XmlDocument();
		private readonly XmlDocument _objQualityDocument = new XmlDocument();

		public enum Mode
		{
			Metamagic = 0,
			Echo = 1,
		}

		#region Control Events
		public frmSelectMetamagic(Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;

			_objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
			_objCritterDocument = XmlManager.Instance.Load("critters.xml");
			_objQualityDocument = XmlManager.Instance.Load("qualities.xml");
		}

		private void frmSelectMetamagic_Load(object sender, EventArgs e)
		{
			// Update the window title if needed.
			if (_strNode == "echo")
			{
				this.Text = LanguageManager.Instance.GetString("Title_SelectMetamagic_Echo");
				chkLimitList.Text = LanguageManager.Instance.GetString("Checkbox_SelectEcho_LimitList");
			}

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			BuildMetamagicList();
		}

		private void lstMetamagic_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstMetamagic.Text == "")
				return;

			// Retireve the information for the selected piece of Cyberware.
			XmlNode objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/" + _strRoot + "/" + _strNode + "[name = \"" + lstMetamagic.SelectedValue + "\"]");

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlMetamagic["source"].InnerText);
			string strPage = objXmlMetamagic["page"].InnerText;
			if (objXmlMetamagic["altpage"] != null)
				strPage = objXmlMetamagic["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMetamagic["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstMetamagic_DoubleClick(object sender, EventArgs e)
		{
			if (lstMetamagic.Text != "")
				AcceptForm();
		}

		private void chkLimitList_CheckedChanged(object sender, EventArgs e)
		{
			BuildMetamagicList();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not the user wants to add another item after this one.
		/// </summary>
		public bool AddAgain
		{
			get
			{
				return _blnAddAgain;
			}
		}

		/// <summary>
		/// Set the window's Mode to Cyberware or Bioware.
		/// </summary>
		public Mode WindowMode
		{
			get
			{
				return _objMode;
			}
			set
			{
				_objMode = value;
				switch (_objMode)
				{
					case Mode.Metamagic:
						_strNode = "metamagic";
						_strRoot = "metamagics";
						break;
					case Mode.Echo:
						_strNode = "echo";
						_strRoot = "echoes";
						break;
				}
			}
		}

		/// <summary>
		/// Name of Metamagic that was selected in the dialogue.
		/// </summary>
		public string SelectedMetamagic
		{
			get
			{
				return _strSelectedMetamagic;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Build the list of Metamagics.
		/// </summary>
		private void BuildMetamagicList()
		{
			XmlNodeList objXmlMetamagicList;
			List<ListItem> lstMetamagics = new List<ListItem>();

			// Load the Metamagic information.
			switch (_objMode)
			{
				case Mode.Metamagic:
					_objXmlDocument = XmlManager.Instance.Load("metamagic.xml");
					break;
				case Mode.Echo:
					_objXmlDocument = XmlManager.Instance.Load("echoes.xml");
					break;
			}

			// If the character has MAG enabled, filter the list based on Adept/Magician availability.
			if (_objCharacter.MAGEnabled)
			{
				if (_objCharacter.MagicianEnabled && !_objCharacter.AdeptEnabled)
					objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[magician = 'yes' and (" + _objCharacter.Options.BookXPath() + ")]");
				else if (!_objCharacter.MagicianEnabled && _objCharacter.AdeptEnabled)
					objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[adept = 'yes' and (" + _objCharacter.Options.BookXPath() + ")]");
				else
					objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");
			}
			else
				objXmlMetamagicList = _objXmlDocument.SelectNodes("/chummer/" + _strRoot + "/" + _strNode + "[" + _objCharacter.Options.BookXPath() + "]");

			foreach (XmlNode objXmlMetamagic in objXmlMetamagicList)
			{
				if (!chkLimitList.Checked || (chkLimitList.Checked && RequirementMet(objXmlMetamagic, false)))
				{
                    bool blnNew = true;
                    foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
                    {
                        if (objMetamagic.Name == objXmlMetamagic["name"].InnerText)
						{
							blnNew = false;
							blnNew = objXmlMetamagic["limit"]?.InnerText == "no";
						}
                    }

                    if (blnNew)
                    {
					    ListItem objItem = new ListItem();
					    objItem.Value = objXmlMetamagic["name"].InnerText;
						objItem.Name = objXmlMetamagic["translate"]?.InnerText ?? objXmlMetamagic["name"].InnerText;
						lstMetamagics.Add(objItem);
                    }
				}
			}
			SortListItem objSort = new SortListItem();
			lstMetamagics.Sort(objSort.Compare);
			lstMetamagic.DataSource = null;
			lstMetamagic.ValueMember = "Value";
			lstMetamagic.DisplayMember = "Name";
			lstMetamagic.DataSource = lstMetamagics;
		}

		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			if (lstMetamagic.Text == "")
				return;

			_strSelectedMetamagic = lstMetamagic.SelectedValue.ToString();

			// Make sure the selected Metamagic or Echo meets its requirements.
			XmlNode objXmlMetamagic;
			if (_objMode == Mode.Metamagic)
				objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/metamagics/metamagic[name = \"" + lstMetamagic.SelectedValue + "\"]");
			else
				objXmlMetamagic = _objXmlDocument.SelectSingleNode("/chummer/echoes/echo[name = \"" + lstMetamagic.SelectedValue + "\"]");

			if (!RequirementMet(objXmlMetamagic, true))
				return;

			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Check if the Metamagic's requirements/restrictions are being met.
		/// </summary>
		/// <param name="objXmlCheckMetamagic">XmlNode of the Metamagic.</param>
		/// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
		private bool RequirementMet(XmlNode objXmlCheckMetamagic, bool blnShowMessage)
		{
			// Ignore the rules.
			if (_objCharacter.IgnoreRules)
				return true;

			string strParent = "";
			string strChild = "";
			if (_objMode == Mode.Metamagic)
			{
				strParent = "metamagics";
				strChild = "metamagic";
			}
			else
			{
				strParent = "echoes";
				strChild = "echo";
			}

			if (objXmlCheckMetamagic.InnerXml.Contains("<required>"))
			{
                bool blnRequirementMet = true;
                string strRequirement = "";
                if (objXmlCheckMetamagic.InnerXml.Contains("<allof>"))
                {
                    strRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_AllOf");
                }

				// Metamagic requirements.
				foreach (XmlNode objXmlMetamagic in objXmlCheckMetamagic.SelectNodes("required/allof/metamagic"))
				{
					bool blnFound = _objCharacter.Metamagics.Any(objMetamagic => objMetamagic.Name == objXmlMetamagic.InnerText);

					if (!blnFound)
					{
						blnRequirementMet = false;
						XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + strParent + "/" + strChild + "[name = \"" + objXmlMetamagic.InnerText + "\"]");
						if (objNode["translate"] != null)
							strRequirement += "\n\t" + objNode["translate"].InnerText;
						else
							strRequirement += "\n\t" + objXmlMetamagic.InnerText;
					}
				}

                // Power requirements.
                foreach (XmlNode objXmlPower in objXmlCheckMetamagic.SelectNodes("required/allof/power"))
                {
                    bool blnFound = _objCharacter.Powers.Any(objPower => objPower.Name == objXmlPower.InnerText);

	                if (!blnFound)
                    {
                        blnRequirementMet = false;
                        strRequirement += "\n\t" + objXmlPower.InnerText;
                    }
                }

                // Art requirements.
                bool blnStreetGrimoire = (_objCharacter.Options.Books.Contains("SG"));
                if (blnStreetGrimoire && !_objCharacter.Options.IgnoreArt)
                {
                    foreach (XmlNode objXmlArt in objXmlCheckMetamagic.SelectNodes("required/allof/art"))
                    {
                        bool blnFound = _objCharacter.Arts.Any(objArt => objArt.Name == objXmlArt.InnerText);

	                    if (!blnFound)
                        {
                            blnRequirementMet = false;
                            strRequirement += "\n\t" + objXmlArt.InnerText;
                        }
                    }
                }

                // Echo requirements.
				foreach (XmlNode objXmlEcho in objXmlCheckMetamagic.SelectNodes("required/allof/echo"))
				{
					bool blnFound = _objCharacter.Metamagics.Any(objEcho => objEcho.Name == objXmlEcho.InnerText);

					if (!blnFound)
					{
						blnRequirementMet = false;
						XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + strParent + "/" + strChild + "[name = \"" + objXmlEcho.InnerText + "\"]");
						if (objNode["translate"] != null)
							strRequirement += "\n\t" + objNode["translate"].InnerText;
						else
							strRequirement += "\n\t" + objXmlEcho.InnerText;
					}
				}

				// Metatype requirements.
				bool blnMetatypeFound = false;
				string strMetatypeRequirement = "";
				if (objXmlCheckMetamagic.SelectNodes("required/allof/metatype").Count == 0)
					blnMetatypeFound = true;
				else
				{
					foreach (XmlNode objXmlMetatype in objXmlCheckMetamagic.SelectNodes("required/allof/metatype"))
					{
						if (_objCharacter.Metatype == objXmlMetatype.InnerText)
						{
							blnMetatypeFound = true;
							break;
						}

						XmlNode objNode =
							_objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlMetatype.InnerText + "\"]") ??
							_objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlMetatype.InnerText + "\"]");
						strMetatypeRequirement += objNode?["translate"] != null
							? "\n\t" + objNode["translate"].InnerText
							: "\n\t" + objXmlMetatype.InnerText;
					}
					if (!blnMetatypeFound)
					{
						blnRequirementMet = false;
						strRequirement += strMetatypeRequirement;
					}
				}

				// Quality requirements.
				bool blnQualityFound = false;
				string strQualityRequirement = "";
				if (objXmlCheckMetamagic.SelectNodes("required/allof/quality").Count == 0)
					blnQualityFound = true;
				else
				{
					foreach (XmlNode objXmlQuality in objXmlCheckMetamagic.SelectNodes("required/allof/quality"))
					{
						foreach (Quality objQuality in _objCharacter.Qualities)
						{
							if (objQuality.Name == objXmlQuality.InnerText)
							{
								blnQualityFound = true;
								break;
							}

							XmlNode objNode =
								_objQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlQuality.InnerText + "\"]");
							strQualityRequirement += objNode["translate"] != null
								? "\n\t" + objNode["translate"].InnerText
								: "\n\t" + objXmlQuality.InnerText;
						}
					}
					if (!blnQualityFound)
					{
						blnRequirementMet = false;
						strRequirement += strQualityRequirement;
					}
				}

                // Check OneOf requirements
                string strOneOfRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_OneOf");
                bool blnOneOfRequirementMet = !objXmlCheckMetamagic.InnerXml.Contains("<oneof>");

                foreach (XmlNode objXmlQuality in objXmlCheckMetamagic.SelectNodes("required/oneof/quality"))
                {
	                if (_objCharacter.Qualities.Any(objQuality => objQuality.Name == objXmlQuality.InnerText))
	                {
		                blnOneOfRequirementMet = true;
	                }

	                if (!blnOneOfRequirementMet)
                        strOneOfRequirement += "\n\t" + objXmlQuality.InnerText;
                    else
                        break;
                }


				foreach (XmlNode objXmlArt in objXmlCheckMetamagic.SelectNodes("required/oneof/art"))
			    {
			        if (!blnStreetGrimoire || _objCharacter.Options.IgnoreArt)
			        {
                        blnOneOfRequirementMet = true;
                        break;
                    }
                    if (_objCharacter.Arts.Any(objArt => objArt.Name == objXmlArt.InnerText))
                    {
	                    blnOneOfRequirementMet = true;
                    }

			        if (!blnOneOfRequirementMet)
			            strOneOfRequirement += "\n\t" + objXmlArt.InnerText;
			        else
			            break;
			    }



			    if (!blnOneOfRequirementMet)
                {
                    blnRequirementMet = false;
                    strRequirement += strOneOfRequirement;
                }

				if (!blnRequirementMet)
				{
					string strMessage = "";
					string strTitle = "";

					if (_objMode == Mode.Metamagic)
					{
						strMessage = LanguageManager.Instance.GetString("Message_SelectMetamagic_MetamagicRequirement");
						strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectMetamagic_MetamagicRequirement");
					}
					else
					{
						strMessage = LanguageManager.Instance.GetString("Message_SelectMetamagic_EchoRequirement");
						strTitle = LanguageManager.Instance.GetString("MessageTitle_SelectMetamagic_EchoRequirement");
					}
					strMessage += strRequirement;

					if (blnShowMessage)
						MessageBox.Show(strMessage, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;
				}
			}

			return true;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}