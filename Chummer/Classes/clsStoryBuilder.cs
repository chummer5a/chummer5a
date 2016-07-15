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
using System.Drawing;
using System.Text;
using System.Xml;

namespace Chummer
{
	class StoryBuilder
	{
		private Dictionary<String, String> persistenceDictionary = new Dictionary<String, String>(); 
		private Character _objCharacter;
		Random random = new Random();
		public StoryBuilder(Character objCharacter)
		{
			_objCharacter = objCharacter;
			persistenceDictionary.Add("metatype", _objCharacter.Metatype.ToLower());
			persistenceDictionary.Add("metavariant", _objCharacter.Metavariant.ToLower());
		}

		public String GetStory()
		{
			try
			{
				//Generate list of all life modules (xml, we don't save required data to quality) this character has
				List<XmlNode> modules = new List<XmlNode>();

				foreach (Quality quality in _objCharacter.Qualities)
				{
					if (quality.Type == QualityType.LifeModule)
					{
						modules.Add(Quality.GetNodeOverrideable(quality.QualityId));
					}
				}

				//Little bit of data required for following steps
				XmlDocument xdoc = XmlManager.Instance.Load("lifemodules.xml");

				//Sort the list (Crude way, but have to do)
				for (int i = 0; i < modules.Count; i++)
				{
					String stageName = "";
                    if (i <= 4)
					{
						stageName = xdoc.SelectSingleNode("chummer/stages/stage[@order = \"" + (i + 1) + "\"]").InnerText;
					}
					else
					{
						stageName = xdoc.SelectSingleNode("chummer/stages/stage[@order = \"" + 5 + "\"]").InnerText;
					}
					int j;
					for (j = i; j < modules.Count; j++)
					{
						if (modules[j]["stage"].InnerText == stageName)
							break;
					}
					if (j != i && j < modules.Count)
					{
						XmlNode tmp = modules[i];
						modules[i] = modules[j];
						modules[j] = tmp;
					}
				}

				StringBuilder story = new StringBuilder();
				//Acctualy "write" the story
				foreach (XmlNode module in modules)
				{
					try
					{
						Write(story, module["story"].InnerText, 5);
						story.Append(Environment.NewLine);
						story.Append(Environment.NewLine);
					}
					catch (Exception e)
					{
						return 
							"Failed trying to write " + module.Name + 
							Environment.NewLine + e.Message + 
							Environment.NewLine + Environment.NewLine + 
							e.StackTrace.ToString();
					}

				}

				return story.ToString();
			}
			catch (Exception e)
			{
				return e.Message + Environment.NewLine + Environment.NewLine + e.StackTrace.ToString();
			}
			
		}

		private void Write(StringBuilder story, string innerText, int levels)
		{
			if (levels <= 0) return;

			int startingLenght = story.Length;

			String[] words;
            if (innerText.StartsWith("$") && innerText.IndexOf(" ") < 0)
            {
	            words = Macro(innerText).Split(" \n\r\t".ToCharArray());
            }
			else
			{
				words = innerText.Split(" \n\r\t".ToCharArray());
			}

			bool mfix = false;
			foreach (string word in words)
			{
				String trim = word.Trim();
				if (trim.StartsWith("$DOLLAR"))
				{
					story.Append('$');
					mfix = true;
				}
				else if (trim.StartsWith("$"))
				{
					//if (story.Length > 0 && story[story.Length - 1] == ' ') story.Length--;
					Write(story, trim, --levels);
					mfix = true;
				}
				else
				{
					if (story.Length != startingLenght && !mfix)
					{
						story.Append(' ');
					}
					else
					{
						mfix = false;
					}
					int slenght = story.Length;
					story.AppendFormat(trim);
					if (story.Length != slenght)
					{
						
					}
				}
				
			}

		}

		public string Macro(string innerText)
		{
			String endString = innerText.ToLower().Substring(1).TrimEnd(",.".ToCharArray());
			String macroName, macroPool;
			if (endString.Contains("_"))
			{
				String[] split = endString.Split('_');
				macroName = split[0];
				macroPool = split[1];
			}
			else
			{
				macroName = macroPool = endString;
			}

			//$DOLLAR is defined elsewhere to prevent recursive calling
			if (macroName == "street")
			{
				if (_objCharacter.Alias != "")
				{
					return _objCharacter.Alias;
				}
				else
				{
					return "Alias ";
				}
			}
			else if(macroName == "real")
			{
				if (_objCharacter.Name != "")
				{
					return _objCharacter.Name;
				}
				else
				{
					return "Unnamed John Doe ";
				}
			}
			else if (macroName == "year")
			{
				int year;
				if (int.TryParse(_objCharacter.Age, out year))
				{
					int age;
					if (int.TryParse(macroPool, out age))
					{
						return (2075 + age - year).ToString();
					}
					else
					{
						return (2075 - year).ToString();
					}
				}
				else
				{
					return String.Format("(ERROR PARSING \"{0}\")", _objCharacter.Age);
				}
			}

			//Did not meet predefined macros, check user defined
			
			String searchString = "/chummer/storybuilder/macros/" + macroName;
			XmlNode userMacro =
				XmlManager.Instance.Load("lifemodules.xml")
				.SelectSingleNode(searchString);
			
			if (userMacro != null)
			{
				if (userMacro.FirstChild != null)
				{
					String selected;
					//Allready defined, no need to do anything fancy
					if (persistenceDictionary.ContainsKey(macroPool))
					{
						selected = persistenceDictionary[macroPool];
					}
					else if (userMacro.FirstChild.Name == "random")
					{
						//Any node not named 
						XmlNodeList possible = userMacro.FirstChild.SelectNodes("./*[not(self::default)]");
						selected = possible[random.Next(possible.Count)].Name;

					}
					else if (userMacro.FirstChild.Name == "persistent")
					{
						//Any node not named 
						XmlNodeList possible = userMacro.FirstChild.SelectNodes("./*[not(self::default)]");
						selected = possible[random.Next(possible.Count)].Name;
						persistenceDictionary.Add(macroPool, selected);
					}
					else
					{
						return String.Format("(Formating error in  $DOLLAR{0} )", macroName);
					}

					if (userMacro.FirstChild[selected] != null)
					{
						return userMacro.FirstChild[selected].InnerText;
					}
					else if (userMacro.FirstChild["default"] != null)
					{
						return userMacro.FirstChild["default"].InnerText;
					}
					else
					{
						return String.Format("(Unknown key {0} in  $DOLLAR{1} )", macroPool, macroName);
					}
				}
				else
				{
					return userMacro.InnerText;
				}
			}
			return String.Format("(Unknown Macro  $DOLLAR{0} )", innerText.Substring(1));
		}
	}
}
