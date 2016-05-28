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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public sealed class LanguageManager
	{
		/// <summary>
		/// An individual language string.
		/// </summary>
		private class LanguageString
		{
			private string _strKey = "";
			private string _strText = "";

			/// <summary>
			/// String's unique Key.
			/// </summary>
			public string Key
			{
				get
				{
					return _strKey;
				}
				set
				{
					_strKey = value;
				}
			}

			/// <summary>
			/// String's text.
			/// </summary>
			public string Text
			{
				get
				{
					return _strText;
				}
				set
				{
					_strText = value;
				}
			}
		}

		static private readonly bool _blnDebug = false;
		static private string _strLanguage = "";
		static readonly LanguageManager _objInstance = new LanguageManager();
		static private readonly Dictionary<string, string> _objDictionary = new Dictionary<string, string>();
		static bool _blnLoaded = false;
		static readonly XmlDocument _objXmlDocument = new XmlDocument();
		static XmlDocument _objXmlDataDocument;

		#region Constructor and Instance
		static LanguageManager()
		{
			string[] strArgs = Environment.GetCommandLineArgs();
			if (strArgs.GetUpperBound(0) > 0)
			{
				if (strArgs[1] == "/debug")
					_blnDebug = true;
			}
			RefreshStrings();
		}

		LanguageManager()
		{
		}

		/// <summary>
		/// Global instance of the LanguageManager.
		/// </summary>
		public static LanguageManager Instance
		{
			get
			{
				return _objInstance;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not the LanguageManager loaded the default language successfully.
		/// </summary>
		public bool Loaded
		{
			get
			{
				return _blnLoaded;
			}
		}

		/// <summary>
		/// XmlDocument that holds UI translations.
		/// </summary>
		public XmlDocument XmlDoc
		{
			get
			{
				return _objXmlDocument;
			}
		}

		/// <summary>
		/// XmlDocument that holds item name translations.
		/// </summary>
		public XmlDocument DataDoc
		{
			get
			{
				return _objXmlDataDocument;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Load the English document and cache it in the List of LanguageStrings so it only needs to be read in once.
		/// </summary>
		private static void RefreshStrings()
		{
			if (Utils.IsRunningInVisualStudio()) return;

			try
			{
				_objDictionary.Clear();
				XmlDocument objEnglishDocument = new XmlDocument();
				string strFilePath = Path.Combine(Application.StartupPath, "lang", "en-us.xml");
				objEnglishDocument.Load(strFilePath);
				foreach (XmlNode objNode in objEnglishDocument.SelectNodes("/chummer/strings/string"))
				{
					LanguageString objString = new LanguageString();
					objString.Key = objNode["key"].InnerText;
					objString.Text = objNode["text"].InnerText;
					_objDictionary.Add(objNode["key"].InnerText, objNode["text"].InnerText);
				}
				_blnLoaded = true;
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
				//TODO this might fuck stuff up, remove before release, or fix?
				//Had obscure bug where this closed visual studio
				MessageBox.Show("Could not load default language file!" + Path.Combine(Application.StartupPath, "lang", "en-us.xml"), "Default Language Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//Application.Exit();
			}
		}

		/// <summary>
		/// Load the selected XML file and its associated custom file.
		/// </summary>
		/// <param name="strLanguage">Language to Load.</param>
		/// <param name="objObject">Object to translate after loading the data.</param>
		public void Load(string strLanguage, object objObject)
		{
			// _strLanguage is populated when the language is read for the first time, meaning this is only triggered once (and language is only read in once since it shouldn't change).
			string strFilePath = "";
			if (strLanguage != "en-us" && _strLanguage == "")
			{
				try
				{
					_strLanguage = strLanguage;
					XmlDocument objLanguageDocument = new XmlDocument();
					strFilePath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
					objLanguageDocument.Load(strFilePath);
					_objXmlDocument.Load(strFilePath);
					foreach (XmlNode objNode in objLanguageDocument.SelectNodes("/chummer/strings/string"))
					{
						// Look for the English version of the found string. If it has been found, replace the English contents with the contents from this file.
						// If the string was not found, then someone has inserted a Key that should not exist and is ignored.
						try
						{
							if (_objDictionary[objNode["key"].InnerText] != null)
								_objDictionary[objNode["key"].InnerText] = objNode["text"].InnerText;
						}
						catch
						{
						}
					}
				}
				catch (Exception)
				{
					_strLanguage = strLanguage;
					MessageBox.Show("Language file " + strFilePath + " could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Check to see if the data translation file for the selected language exists.
				string strDataPath = Path.Combine(Application.StartupPath, "lang", strLanguage + "_data.xml");
				if (File.Exists(strDataPath))
				{
					try
					{
						_objXmlDataDocument = new XmlDocument();
						_objXmlDataDocument.Load(strDataPath);
					}
					catch
					{
						// Failing to load the data translation file should not render the application unusable.
					}
				}
			}

			// If the object is a Form, call the UpdateForm method to provide its translations.
			if (objObject is Form)
				UpdateForm((Form)objObject);

			// If the object is a UserControl, call the UpdateUserControl method to provide its translations.
			if (objObject is UserControl)
				UpdateUserControl((UserControl)objObject);
		}

		/// <summary>
		/// Recursive method to translate all of the controls in a Form or UserControl.
		/// </summary>
		/// <param name="objParent">Control container to translate.</param>
		private void UpdateControls(Control objParent)
		{
			// Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.

			foreach (Label lblLabel in objParent.Controls.OfType<Label>())
			{
				if (lblLabel.Tag != null)
				{
					try
					{
						lblLabel.Text = GetString(lblLabel.Tag.ToString());
					}
					catch
					{
						if (_blnDebug)
							throw;
						else
							lblLabel.Text = lblLabel.Tag.ToString();
					}
				}
			}
			foreach (Button cmdButton in objParent.Controls.OfType<Button>())
			{
				if (cmdButton.Tag != null)
				{
					try
					{
						cmdButton.Text = GetString(cmdButton.Tag.ToString());
					}
					catch
					{
						if (_blnDebug)
							throw;
						else
							cmdButton.Text = cmdButton.Tag.ToString();
					}
				}
			}
			foreach (CheckBox chkCheckbox in objParent.Controls.OfType<CheckBox>())
			{
				if (chkCheckbox.Tag != null)
				{
					try
					{
						if (chkCheckbox.Tag.ToString().Contains("_"))
							chkCheckbox.Text = GetString(chkCheckbox.Tag.ToString());
					}
					catch
					{
						if (_blnDebug)
							throw;
						else
							chkCheckbox.Text = chkCheckbox.Tag.ToString();
					}
				}
			}
			foreach (ListView lstList in objParent.Controls.OfType<ListView>())
			{
				foreach (ColumnHeader objHeader in lstList.Columns)
				{
					if (objHeader.Tag != null)
					{
						try
						{
							objHeader.Text = GetString(objHeader.Tag.ToString());
						}
						catch
						{
							if (_blnDebug)
								throw;
							else
								objHeader.Text = objHeader.Tag.ToString();
						}
					}
				}
			}

			// Run through any Panels on the container.
			foreach (Panel objPanel in objParent.Controls.OfType<Panel>())
			{
				UpdateControls(objPanel);
			}

			// Run through any Tabs on the container.
			foreach (TabControl objTabControl in objParent.Controls.OfType<TabControl>())
			{
				foreach (TabPage tabPage in objTabControl.TabPages)
				{
					if (tabPage.Tag != null)
					{
						try
						{
							tabPage.Text = GetString(tabPage.Tag.ToString());
						}
						catch
						{
							if (_blnDebug)
								throw;
							else
								tabPage.Text = tabPage.Tag.ToString();
						}
					}

					UpdateControls(tabPage);
				}
			}

			// Run through everything in any SplitContainers.
			foreach (SplitContainer objSplitControl in objParent.Controls.OfType<SplitContainer>())
			{
				for (int i = 1; i <= 2; i++)
				{
					SplitterPanel objSplitPanel;
					if (i == 1)
						objSplitPanel = objSplitControl.Panel1;
					else
						objSplitPanel = objSplitControl.Panel2;

					UpdateControls(objSplitPanel);
				}
			}

			// Run through any FlowLayoutPanels on the container.
			foreach (FlowLayoutPanel objContainer in objParent.Controls.OfType<FlowLayoutPanel>())
			{
				UpdateControls(objContainer);
			}

			foreach (TreeView treTree in objParent.Controls.OfType<TreeView>())
			{
				foreach (TreeNode objNode in treTree.Nodes)
				{
					if (objNode.Level == 0)
					{
						if (objNode.Tag != null)
						{
							if (objNode.Tag.ToString().StartsWith("Node_"))
							{
								try
								{
									objNode.Text = GetString(objNode.Tag.ToString());
								}
								catch
								{
									if (_blnDebug)
										throw;
									else
										objNode.Text = objNode.Tag.ToString();
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Translate the contents of a UserControl.
		/// </summary>
		/// <param name="objControl">UserControl to translate.</param>
		private void UpdateUserControl(UserControl objControl)
		{
			UpdateControls(objControl);
		}

		/// <summary>
		/// Translate the contents of a Form.
		/// </summary>
		/// <param name="objForm">Form to translate.</param>
		private void UpdateForm(Form objForm)
		{
			// Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
			// Update the Form itself.
			if (objForm.Tag != null)
			{
				try
				{
					objForm.Text = GetString(objForm.Tag.ToString());
				}
				catch
				{
					if (_blnDebug)
						throw;
					else
						objForm.Text = objForm.Tag.ToString();
				}
			}

            // update any menu strip items that have tags
            if (objForm.MainMenuStrip != null)
            {
                foreach (ToolStripMenuItem objItem in objForm.MainMenuStrip.Items)
                    SetMenuItemsRecursively(objItem);
            }

			// Run through any StatusStrips.
			foreach (StatusStrip objStrip in objForm.Controls.OfType<StatusStrip>())
			{
				foreach (ToolStripStatusLabel tssLabel in objStrip.Items.OfType<ToolStripStatusLabel>())
				{
					if (tssLabel.Tag != null)
						try
						{
							tssLabel.Text = GetString(tssLabel.Tag.ToString());
						}
						catch
						{
							if (_blnDebug)
								throw;
							else
								tssLabel.Text = tssLabel.Tag.ToString();
						}
				}
			}

			// Handle control over to the method that handles translating all of the other Controls.
			UpdateControls(objForm);
		}

        /// <summary>
        /// Loads the proper language from the language file for every menu item recursively
        /// </summary>
        /// <param name="objItem"></param>
        private void SetMenuItemsRecursively(ToolStripMenuItem objItem)
        {
            if (objItem.Tag != null)
                try
                {
                    objItem.Text = GetString(objItem.Tag.ToString());
                }
                catch
                {
                    if (_blnDebug)
                        throw;
                    else
                        objItem.Text = objItem.Tag.ToString();
                }

            if (objItem.DropDownItems == null || objItem.DropDownItems.Count == 0)
                return; // we have no more drop down items to pull

            foreach (ToolStripMenuItem objRecursiveItem in objItem.DropDownItems.OfType<ToolStripMenuItem>())
            {
                SetMenuItemsRecursively(objRecursiveItem);
                if (objItem.Tag != null)
                    try
                    {
                        objItem.Text = GetString(objItem.Tag.ToString());
                    }
                    catch
                    {
                        if (_blnDebug)
                            throw;
                        else
                            objItem.Text = objItem.Tag.ToString();
                    }
            }
        }
       

		/// <summary>
		/// Retrieve a string from the language file.
		/// </summary>
		/// <param name="strKey">Key to retrieve.</param>
		public string GetString(string strKey)
		{
            try
            {
                string strReturn = "";
                strReturn = _objDictionary[strKey].Replace("\\n", "\n");
                return strReturn;
            }
            catch
            {		//TODO THIS IS RETARDED. Doctor it hurts if i do this. Thats why i try again to see if it stops hurting
                string strReturn = "Error in string return - " + _objDictionary[strKey].ToString();
                return strReturn;
            }
		}

		/// <summary>
		/// Check the Keys in the selected language file against the English version. 
		/// </summary>
		/// <param name="strLanguage">Language to check.</param>
		public void VerifyStrings(string strLanguage)
		{
			// Load the English version.
			List<LanguageString> lstEnglish = new List<LanguageString>();
			XmlDocument objEnglishDocument = new XmlDocument();
			string strFilePath = Path.Combine(Application.StartupPath, "lang", "en-us.xml");
			objEnglishDocument.Load(strFilePath);
			foreach (XmlNode objNode in objEnglishDocument.SelectNodes("/chummer/strings/string"))
			{
				LanguageString objString = new LanguageString();
				objString.Key = objNode["key"].InnerText;
				objString.Text = objNode["text"].InnerText;
				lstEnglish.Add(objString);
			}

			// Load the selected language version.
			List<LanguageString> lstLanguage = new List<LanguageString>();
			XmlDocument objLanguageDocument = new XmlDocument();
			string strLangPath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
			objLanguageDocument.Load(strLangPath);
			foreach (XmlNode objNode in objLanguageDocument.SelectNodes("/chummer/strings/string"))
			{
				LanguageString objString = new LanguageString();
				objString.Key = objNode["key"].InnerText;
				objString.Text = objNode["text"].InnerText;
				lstLanguage.Add(objString);
			}

			string strMessage = "";
			// Check for strings that are in the English file but not in the selected language file.
			foreach (LanguageString objString in lstEnglish)
			{
				LanguageString objFindString = lstLanguage.Find(objItem => objItem.Key == objString.Key);
				if (objFindString == null)
					strMessage += "\nMissing String: " + objString.Key;
			}
			// Check for strings that are not in the English file but are in the selected language file (someone has put in Keys that they shouldn't have which are ignored).
			foreach (LanguageString objString in lstLanguage)
			{
				LanguageString objFindString = lstEnglish.Find(objItem => objItem.Key == objString.Key);
				if (objFindString == null)
					strMessage += "\nUnused String: " + objString.Key;
			}

			// Display the message.
			if (strMessage != "")
				MessageBox.Show(strMessage, "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
			else
				MessageBox.Show("Language file is OK.", "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Attempt to translate any Extra text for an item.
		/// </summary>
		/// <param name="strExtra">Extra string to translate.</param>
		public string TranslateExtra(string strExtra)
		{
			string strReturn = "";

			// Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
			if (_strLanguage != "en-us" && strExtra.Trim() != "")
			{
				XmlDocument objXmlDocument = new XmlDocument();

				// Look in Weapon Categories.
				objXmlDocument = XmlManager.Instance.Load("weapons.xml");
				XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode.Attributes["translate"] != null)
					{
						strReturn = objNode.Attributes["translate"].InnerText;
						return strReturn;
					}
				}

				// Look in Weapons.
				objNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}

				// Look in Skills.
				objXmlDocument = XmlManager.Instance.Load("skills.xml");
				objNode = objXmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}

				XmlNodeList objNodelist = objXmlDocument.SelectNodes("/chummer/skills/skill/specs/spec");
				foreach (XmlNode objXMLNode in objNodelist)
				{
					if (objXMLNode.InnerText == strExtra)
					{ 
						if (objXMLNode.Attributes["translate"] != null)
						{
							strReturn = objXMLNode.Attributes["translate"].InnerText;
							return strReturn;
						}
					}
				}
				// Look in Skill Groups.
				objNode = objXmlDocument.SelectSingleNode("/chummer/skillgroups/name[. = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode.Attributes["translate"] != null)
					{
						strReturn = objNode.Attributes["translate"].InnerText;
						return strReturn;
					}
				}

				// Look in Licences.
				objXmlDocument = XmlManager.Instance.Load("licenses.xml");
				objNode = objXmlDocument.SelectSingleNode("/chummer/licenses/license[. = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode.Attributes["translate"] != null)
					{
						strReturn = objNode.Attributes["translate"].InnerText;
						return strReturn;
					}
				}

				// Look in Mentors.
				objXmlDocument = XmlManager.Instance.Load("mentors.xml");
				objNode = objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}
				objNode = objXmlDocument.SelectSingleNode("/chummer/mentors/mentor/choices/choice[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}

				// Look in Paragons.
				objXmlDocument = XmlManager.Instance.Load("paragons.xml");
				objNode = objXmlDocument.SelectSingleNode("/chummer/mentors/mentor[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}
				objNode = objXmlDocument.SelectSingleNode("/chummer/mentors/mentor/choices/choice[name = \"" + strExtra.Replace("\"", string.Empty) + "\"]");
				if (objNode != null)
				{
					if (objNode["translate"] != null)
					{
						strReturn = objNode["translate"].InnerText;
						return strReturn;
					}
				}

				// Attempt to translate CharacterAttribute names.
				switch (strExtra)
				{
					case "BOD":
						strReturn = GetString("String_AttributeBODShort");
						break;
					case "AGI":
						strReturn = GetString("String_AttributeAGIShort");
						break;
					case "REA":
						strReturn = GetString("String_AttributeREAShort");
						break;
					case "STR":
						strReturn = GetString("String_AttributeSTRShort");
						break;
					case "CHA":
						strReturn = GetString("String_AttributeCHAShort");
						break;
					case "INT":
						strReturn = GetString("String_AttributeINTShort");
						break;
					case "LOG":
						strReturn = GetString("String_AttributeLOGShort");
						break;
					case "WIL":
						strReturn = GetString("String_AttributeWILShort");
						break;
					case "EDG":
						strReturn = GetString("String_AttributeEDGShort");
						break;
					case "MAG":
						strReturn = GetString("String_AttributeMAGShort");
						break;
					case "RES":
						strReturn = GetString("String_AttributeRESShort");
						break;
				}
			}

			// If no translation could be found, just use whatever we were passed.
			if (strReturn == "")
				strReturn = strExtra;

			return strReturn;
		}
		#endregion
	}
}