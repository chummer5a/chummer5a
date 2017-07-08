using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
	    private List<string> _attAbbrevs = new List<string>();
	    private List<string> _attValues = new List<string>();
	    private readonly List<string> _attList = new List<string> { "bod","agi","rea","str", "cha", "int","log","wil", "ini", "edg", "mag","res","dep","ess"};
		public MainWindow()
        {
            InitializeComponent();
        }


        private void ConvertString(bool critter = true)
        {
	        Guid g;
	        if (!Guid.TryParse(txtGUID.Text, out g))
            {
	            txtGUID.Text = Guid.NewGuid().ToString();
            }
            var lines = txtRaw.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
	        if (lines.Length <= 1) return;
	        var doc = new XmlDocument();
	        // write the root chummer node.
			
	        XmlNode objHeader = doc.CreateElement(critter ? "critter" : "spirit");
	        doc.AppendChild(objHeader);

			XmlNode xmlNode = doc.CreateElement("id");
			xmlNode.InnerText = txtGUID.Text.Length > 0 ? txtGUID.Text : g.ToString();
			objHeader.AppendChild(xmlNode);

			xmlNode = doc.CreateElement("name");
			xmlNode.InnerText = txtName.Text;
			objHeader.AppendChild(xmlNode);

			_attAbbrevs = new List<string>(ReplaceAttributeAbbrevs(lines[0]).Split(' '));
	        _attValues = new List<string>(lines[1].Split(' '));
	        if (_attAbbrevs.Count != _attValues.Count)
	        {
		        MessageBox.Show("Mismatched attribute lengths!");
		        return;
	        }

			var nodes = new[] { "min", "max", "aug" };
	        foreach (string str in _attList)
	        {
		        if (_attAbbrevs.Contains(str))
		        {
			        if (critter)
			        {
				        int i = _attAbbrevs.IndexOf(str);
				        foreach (var node in nodes)
				        {
					        xmlNode = doc.CreateElement($"{str}{node}".ToLower());
					        if (node == "aug" && int.TryParse(_attValues[i], out int result))
					        {
						        xmlNode.InnerText = node == "aug" ? (result + 4).ToString() : _attValues[i];
					        }
					        else
					        {
						        xmlNode.InnerText = _attValues[i];
					        }

					        objHeader.AppendChild(xmlNode);
				        }
			        }
			        else
			        {
						int i = _attAbbrevs.IndexOf(str);
						xmlNode = doc.CreateElement($"{str}".ToLower());
						xmlNode.InnerText = _attValues[i];
						objHeader.AppendChild(xmlNode);
					}
		        }
		        else if (!critter)
				{
					xmlNode = doc.CreateElement($"{str}".ToLower());
					xmlNode.InnerText = "0";
					objHeader.AppendChild(xmlNode);
				}
	        }


	        //Skip the first two lines, since we know they're attributes. 
	        for (var i = 2; i < lines.Length; i++)
	        {
		        if (lines[i].StartsWith("Initiative"))
		        {
			        //Should probably be a handler for initiative dice here. 
		        }
		        else if (lines[i].StartsWith("Movement"))
		        {
			        var rawMove = lines[i].Replace("Movement ","");
			        if (rawMove.Contains('('))
			        {
				        rawMove = rawMove.Replace('(', ',').Replace(")",string.Empty);
			        }
			        var movements = rawMove.Split(',').ToArray();
			        var walk = new[] {"0", "1", "0"};
			        var run = new[] { "0", "0", "0" };
			        var sprint = new[] { "0", "1", "0" };
			        foreach (var movement in movements.Where(movement => movement != "Movement"))
			        {
				        var current = movement.Replace("x", "").Replace("+", "").TrimEnd();
				        var position = 0; 
				        if (current.ToLower().Contains("swimming"))
				        {
					        position = 1;
					        current = current.Split(' ')[0];
				        }
				        else if (current.ToLower().Contains("ﬂight") || current.ToLower().Contains("flight"))
				        {
					        position = 2;
					        current = current.Split(' ')[0];
				        }
				        var currentMovement = current.Split('/');
				        walk[position] = currentMovement[0];
				        run[position] = currentMovement[1];
				        sprint[position] = currentMovement[2];

						// Set default swimming values if not present; based on metahuman values, may be incorrect. 
				        if (position == 1)
				        {
					        walk[position] = Math.Max(Convert.ToInt32(walk[position]), 1).ToString();
							sprint[position] = Math.Max(Convert.ToInt32(sprint[position]), 1).ToString();
						}
			        }

			        xmlNode = doc.CreateElement("walk");
			        xmlNode.InnerText = string.Join("/", walk);
			        objHeader.AppendChild(xmlNode);

			        xmlNode = doc.CreateElement("run");
			        xmlNode.InnerText = string.Join("/", run);
			        objHeader.AppendChild(xmlNode);

			        xmlNode = doc.CreateElement("sprint");
			        xmlNode.InnerText = string.Join("/", sprint);
			        objHeader.AppendChild(xmlNode);
		        }
		        else if (lines[i].StartsWith("Physical Skills") || (lines[i].StartsWith("Matrix Skills")) || (lines[i].StartsWith("Skills")) || (lines[i].StartsWith("Magic Skills")))
		        {
			        var line = lines[i].Replace("Physical Skills ", "").Replace("Matrix Skills ", "").Replace("Magic Skills ", "").Replace("Skills ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("skills");
			        foreach (var s in split.Where(s => s != string.Empty))
			        {
				        xmlNode = doc.CreateElement("skill"); 
				        var index = s.LastIndexOf(" R");
				        if (index != -1)
						{
							var attr = doc.CreateAttribute("rating");
							xmlNode.InnerText = s.Substring(0, index).Trim();
							attr.Value = s.Substring(index + 1).Trim();
							xmlNode.Attributes?.Append(attr);
						}
				        else
				        {
					        xmlNode.InnerText = s.Trim();
				        }
				        xmlParentNode.AppendChild(xmlNode);
			        }
			        objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Complex Forms"))
		        {
			        var line = lines[i].Replace("Complex Forms ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("complexforms");
					foreach (var s in split.Where(s => s != string.Empty))
					{
						xmlNode = doc.CreateElement("complexform");
						if (s.Contains('('))
						{
							var attr = doc.CreateAttribute("select");
							attr.Value = s.Split('(', ')')[1];
							xmlNode.Attributes?.Append(attr);
						}

						xmlNode.InnerText = s.Split('(', ')')[0].Trim();
						xmlParentNode.AppendChild(xmlNode);
					}
					objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Powers"))
		        {
			        var line = lines[i].Replace("Powers ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("powers");
					foreach (var s in split.Where(s => s != string.Empty))
					{
						xmlNode = doc.CreateElement("power");
						if (s.Contains(':'))
						{
							var attr = doc.CreateAttribute("select");
							attr.Value = s.Split(':')[1].Trim();
							xmlNode.Attributes?.Append(attr);
							xmlNode.InnerText = s.Split(':')[0].Trim();
						}
						else if (s.Contains('('))
						{
							var attr = doc.CreateAttribute("select");
							attr.Value = s.Split('(', ')')[1];
							xmlNode.Attributes?.Append(attr);
							xmlNode.InnerText = s.Split('(', ')')[0].Trim();
						}
						else
						{
							xmlNode.InnerText = s.Trim();
						}

						xmlParentNode.AppendChild(xmlNode);
					}
					objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Programs"))
		        {
			        var line = lines[i].Replace("Programs ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("gears");
					foreach (var s in split.Where(s => s != string.Empty))
					{
						xmlNode = doc.CreateElement("quality");
						if (s.Contains('('))
						{
							var attr = doc.CreateAttribute("rating");
							attr.Value = s.Split('(', ')')[1];
							xmlNode.Attributes?.Append(attr);
						}

						xmlNode.InnerText = s.Split('(', ')')[0].Trim();
						xmlParentNode.AppendChild(xmlNode);
					}
					objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Qualities"))
		        {
			        var line = lines[i].Replace("Qualities ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("qualities");
			        foreach (var s in split.Where(s => s != string.Empty))
			        {
				        xmlNode = doc.CreateElement("quality");
				        if (s.Contains('('))
				        {
					        var attr = doc.CreateAttribute("select");
					        attr.Value = s.Split('(', ')')[1];
					        xmlNode.Attributes?.Append(attr);
				        }
							
				        xmlNode.InnerText = s.Split('(', ')')[0].Trim();
				        xmlParentNode.AppendChild(xmlNode);
			        }
			        objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Armor"))
				{
					var line = lines[i].Replace("Armor ", "");
					xmlNode = doc.CreateElement("armor");
					xmlNode.InnerText = line.Trim();
					objHeader.AppendChild(xmlNode);
				}
	        }

			// Create the default critter tab bonus. 
			XmlNode xmlBonusNode = doc.CreateElement("bonus");
			XmlNode xmlChildNode = doc.CreateElement("enabletab");
			XmlNode xmlTabNode = doc.CreateElement("name");
	        xmlTabNode.InnerText = "critter";
	        xmlChildNode.AppendChild(xmlTabNode);
	        xmlBonusNode.AppendChild(xmlChildNode);
			objHeader.AppendChild(xmlBonusNode);

			xmlNode = doc.CreateElement("source");
	        xmlNode.InnerText = txtSource.Text.Length > 0 ? txtSource.Text : "EDITME";
	        objHeader.AppendChild(xmlNode);
			xmlNode = doc.CreateElement("page");
			xmlNode.InnerText = txtPage.Text.Length > 0 ? txtPage.Text : "EDITME";
			objHeader.AppendChild(xmlNode);

			txtRaw.Text = System.Xml.Linq.XElement.Parse(doc.InnerXml).ToString();
        }

		private static string ReplaceAttributeAbbrevs(string input)
		{
			input = input.ToLower();
			input = ReplaceWholeWord(input, "b", "bod");
			input = ReplaceWholeWord(input, "a", "agi");
			input = ReplaceWholeWord(input, "r", "rea");
			input = ReplaceWholeWord(input, "s", "str");
			input = ReplaceWholeWord(input, "w", "wil");
			input = ReplaceWholeWord(input, "l", "log");
			input = ReplaceWholeWord(input, "i", "int");
			input = ReplaceWholeWord(input, "c", "cha");
			input = ReplaceWholeWord(input, "edg", "edg");
			input = ReplaceWholeWord(input, "m", "mag");
			input = ReplaceWholeWord(input, "r", "res");
			input = ReplaceWholeWord(input, "depth", "dep");
			return input;
        }

        /// <summary>
        /// Regex wrapper to replace whole words in a string only. 
        /// </summary>
        /// <param name="original">Default string.</param>
        /// <param name="wordToFind">Text to replace.</param>
        /// <param name="replacement">Text that will be inserted.</param>
        /// <param name="regexOptions">Extra regex options. Default is none.</param>
        /// <returns></returns>
        public static string ReplaceWholeWord(string original, string wordToFind,
     string replacement, RegexOptions regexOptions = RegexOptions.None)
        {

            string pattern = $@"\b{wordToFind}\b";
            var ret = Regex.Replace(original, pattern, replacement, regexOptions);
            return ret;
        }

		private void CreateSpirit(object sender, RoutedEventArgs e)
		{
			ConvertString(false);
		}

		private void CreateCritter(object sender, RoutedEventArgs e)
		{
			ConvertString();
		}
	}
}
