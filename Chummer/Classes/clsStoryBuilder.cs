using System;
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
		public StoryBuilder(Character objCharacter)
		{
			_objCharacter = objCharacter;
		}

		public String GetStory()
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
				String stageName = xdoc.SelectSingleNode("chummer/stages/stage[@order = \"" + (i + 1) + "\"]").InnerText;
				int j;
                for (j = i; j < modules.Count; j++)
                {
	                if (modules[j]["stage"].InnerText == stageName)
		                break;
                }
				if (j != i)
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
				Write(story, module["story"].InnerText, 5);
				story.Append(Environment.NewLine);
				story.Append(Environment.NewLine);

			}

			return story.ToString();
		}

		private void Write(StringBuilder story, string innerText, int levels)
		{
			if (levels <= 0) return;

			String[] words;
            if (innerText.StartsWith("$") && innerText.IndexOf(" ") < 0)
            {
	            words = Macro(innerText).Split(" \n\r\t".ToCharArray());
            }
			else
			{
				words = innerText.Split(" \n\r\t".ToCharArray());
			}
			

			foreach (string word in words)
			{
				String trim = word.Trim();
				if (trim.StartsWith("$DOLLAR"))
				{
					story.Append('$');
					story.AppendFormat(trim.Substring(7));
				}
				else if (trim.StartsWith("$"))
				{
					Write(story, trim, --levels);
				}
				else
				{
					story.AppendFormat(trim);
				}
				story.Append(' ');
			}

			story.Length--;
		}

		private string Macro(string innerText)
		{
			if (innerText == "$STREET")
			{
				return _objCharacter.Alias;
			}
			else if(innerText == "$REAL")
			{
				return _objCharacter.Name;
			}
			else if (innerText == "$YEAR")
			{
				int year;
				if (int.TryParse(_objCharacter.Age, out year))
				{
					return (2075 - year).ToString();
				}
				else
				{
					return String.Format("ERROR PARSING \"{0}\"", _objCharacter.Age);
				}
			}

			//Did not meet predefined macros, check user defined
			String endString = innerText.ToLower().Substring(1).TrimEnd(",.".ToCharArray());
			String searchString = "/chummer/storybuilder/macros/" + endString;
			XmlNode userMacro =
				XmlManager.Instance.Load("lifemodules.xml").
				SelectSingleNode(searchString);
			String selected;

			if (userMacro != null)
			{
				if (userMacro.FirstChild.Name == "random")
				{
					XmlNodeList nodes = userMacro.FirstChild.ChildNodes;
					selected = nodes[new Random().Next(0, nodes.Count)].Name;

				}
				else if (userMacro.FirstChild.Name == "persistent")
				{
					if (persistenceDictionary.ContainsKey(userMacro.Name))
					{
						selected = persistenceDictionary[userMacro.Name];
					}
					else
					{
						XmlNodeList nodes = userMacro.FirstChild.ChildNodes;
						selected = nodes[new Random().Next(0, nodes.Count)].Name;
						persistenceDictionary.Add(userMacro.Name, selected);
					}
				}
				else
				{
					return "(Unknown Macro type " + userMacro.FirstChild.Name + ")";
				}
				String removed = innerText.Substring(endString.Length + 1);
				return userMacro.FirstChild[selected].InnerText + removed;
			}


			return String.Format("(Unknown Macro $DOLLAR{0})", innerText.Substring(1));
		}
	}
}
