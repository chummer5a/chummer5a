using System;
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
        private string[] _attAbbrevs;
        private string[] _attValues;
		public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var lines = textBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
	        if (lines.Length <= 0) return;
	        var doc = new XmlDocument();
	        // write the root chummer node.
	        XmlNode objHeader = doc.CreateElement("critter");
	        doc.AppendChild(objHeader);

	        _attAbbrevs = ReplaceAttributeAbbrevs(lines[0]).Split(' ');
	        _attValues = lines[1].Split(' ');
	        if (_attAbbrevs.Length != _attValues.Length)
	        {
		        MessageBox.Show("Mismatched attribute lengths!");
		        return;
	        }
	        for (var i = 0; i < _attAbbrevs.Length; i++)
	        {
		        var nodes = new[] {"min", "max", "aug"};
		        foreach (var node in nodes)
		        {
			        XmlNode xmlNode = doc.CreateElement($"{_attAbbrevs[i]}{node}".ToLower());
			        xmlNode.InnerText = _attValues[i];
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
			        var walk = new[] {"0", "0", "0"};
			        var run = new[] { "0", "0", "0" };
			        var sprint = new[] { "0", "0", "0" };
			        foreach (var movement in movements.Where(movement => movement != "Movement"))
			        {
				        var current = movement.Replace("x", "").Replace("+", "").TrimEnd();
				        var position = 0; 
				        if (current.ToLower().Contains("swim"))
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
			        }

			        XmlNode xmlNode = doc.CreateElement("walk");
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
				        XmlNode xmlNode = doc.CreateElement("skill"); 
				        var attr = doc.CreateAttribute("rating");
				        var index = s.LastIndexOf(' ');
				        xmlNode.InnerText = s.Substring(0, index).Trim();
				        attr.Value = s.Substring(index + 1).Trim();
				        xmlNode.Attributes?.Append(attr);
				        xmlParentNode.AppendChild(xmlNode);
			        }
			        objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Complex Forms"))
		        {
			        var line = lines[i].Replace("Complex Forms ", "");
			        var skills = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("complexforms");
			        foreach (var s in skills)
			        {
				        XmlNode xmlNode = doc.CreateElement("complexform");
				        xmlNode.InnerText = s.Trim();
				        xmlParentNode.AppendChild(xmlNode);
			        }
			        objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Powers"))
		        {
			        var line = lines[i].Replace("Powers ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("critterpowers");
			        foreach (var s in split)
			        {
				        XmlNode xmlNode = doc.CreateElement("critterpower");
				        xmlNode.InnerText = s.Trim();
				        xmlParentNode.AppendChild(xmlNode);
			        }
			        objHeader.AppendChild(xmlParentNode);
		        }
		        else if (lines[i].StartsWith("Programs"))
		        {
			        var line = lines[i].Replace("Programs ", "");
			        var split = line.Split(',');
			        XmlNode xmlParentNode = doc.CreateElement("gears");
			        foreach (var s in split)
			        {
				        XmlNode xmlNode = doc.CreateElement("gear");
				        xmlNode.InnerText = s.Trim();
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
				        XmlNode xmlNode = doc.CreateElement("quality");
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
					XmlNode xmlNode = doc.CreateElement("armor");
					xmlNode.InnerText = line.Trim();
					doc.AppendChild(xmlNode);

				}
	        }

	        textBox.Text = System.Xml.Linq.XElement.Parse(doc.InnerXml).ToString();
        }

		private static string ReplaceAttributeAbbrevs(string input)
        {
            input = ReplaceWholeWord(input, "B", "bod");
            input = ReplaceWholeWord(input, "A", "agi");
            input = ReplaceWholeWord(input, "R", "rea");
            input = ReplaceWholeWord(input, "S", "str");
            input = ReplaceWholeWord(input, "W", "wil");
            input = ReplaceWholeWord(input, "L", "log");
            input = ReplaceWholeWord(input, "I", "int");
            input = ReplaceWholeWord(input, "C", "cha");
			input = ReplaceWholeWord(input, "DEPTH", "dep");
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
    }
}
