using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<string> _attAbbrevs = new List<string>();
        private List<string> _attValues = new List<string>();
        private readonly List<string> _attList = new List<string> { "bod", "agi", "rea", "str", "cha", "int", "log", "wil", "ini", "edg", "mag", "res", "dep", "ess" };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConvertString(bool critter = true)
        {
            if (!Guid.TryParse(txtGUID.Text, out Guid g))
            {
                txtGUID.Text = Guid.NewGuid().ToString("D");
            }
            string[] lines = txtRaw.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length <= 1) return;
            XmlDocument doc = new XmlDocument();
            // write the root chummer node.

            XmlNode objHeader = doc.CreateElement(critter ? "critter" : "spirit");
            doc.AppendChild(objHeader);

            XmlNode xmlNode = doc.CreateElement("id");
            xmlNode.InnerText = txtGUID.Text.Length > 0 ? txtGUID.Text : g.ToString("D");
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

            string[] nodes = { "min", "max", "aug" };
            foreach (string str in _attList)
            {
                if (_attAbbrevs.Contains(str))
                {
                    if (critter)
                    {
                        int i = _attAbbrevs.IndexOf(str);
                        foreach (string node in nodes)
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
            for (int i = 2; i < lines.Length; i++)
            {
                string strLine = lines[i];
                if (strLine.StartsWith("Initiative", StringComparison.Ordinal))
                {
                    //Should probably be a handler for initiative dice here.
                }
                else if (strLine.StartsWith("Movement", StringComparison.Ordinal))
                {
                    string rawMove = strLine.Replace("Movement ", string.Empty);
                    if (rawMove.Contains('('))
                    {
                        rawMove = rawMove.Replace('(', ',').Replace(")", string.Empty);
                    }
                    string[] movements = rawMove.Split(',');
                    string[] walk = { "0", "1", "0" };
                    string[] run = { "0", "0", "0" };
                    string[] sprint = { "0", "1", "0" };
                    foreach (string movement in movements.Where(movement => movement != "Movement"))
                    {
                        string current = movement.Replace("x", string.Empty).Replace("+", string.Empty).TrimEnd();
                        int position = 0;
                        if (current.IndexOf("swimming", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            position = 1;
                            current = current.Split(' ')[0];
                        }
                        else if (current.IndexOf("ﬂight", StringComparison.OrdinalIgnoreCase) >= 0 || current.IndexOf("flight", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            position = 2;
                            current = current.Split(' ')[0];
                        }
                        string[] currentMovement = current.Split('/');
                        walk[position] = currentMovement[0];
                        run[position] = currentMovement[1];
                        sprint[position] = currentMovement[2];

                        // Set default swimming values if not present; based on metahuman values, may be incorrect.
                        if (position == 1)
                        {
                            int.TryParse(walk[position], NumberStyles.Any, CultureInfo.InvariantCulture,
                                out int intWalk);
                            walk[position] = Math.Max(intWalk, 1).ToString();
                            int.TryParse(sprint[position], NumberStyles.Any, CultureInfo.InvariantCulture,
                                out int intSprint);
                            sprint[position] = Math.Max(intSprint, 1).ToString();
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
                else if (strLine.StartsWith("Physical Skills", StringComparison.Ordinal)
                         || strLine.StartsWith("Matrix Skills", StringComparison.Ordinal)
                         || strLine.StartsWith("Skills", StringComparison.Ordinal)
                         || strLine.StartsWith("Magic Skills", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Physical Skills ", string.Empty).Replace("Matrix Skills ", string.Empty)
                                     .Replace("Magic Skills ", string.Empty).Replace("Skills ", string.Empty);
                    string[] split = strLine.Split(',');
                    XmlNode xmlParentNode = doc.CreateElement("skills");
                    foreach (string s in split.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        xmlNode = doc.CreateElement("skill");
                        int index = s.LastIndexOf(" R", StringComparison.Ordinal);
                        if (index != -1)
                        {
                            XmlAttribute attr = doc.CreateAttribute("rating");
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
                else if (strLine.StartsWith("Complex Forms", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Complex Forms ", string.Empty);
                    string[] split = strLine.Split(',');
                    XmlNode xmlParentNode = doc.CreateElement("complexforms");
                    foreach (string s in split.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        xmlNode = doc.CreateElement("complexform");
                        if (s.Contains('('))
                        {
                            XmlAttribute attr = doc.CreateAttribute("select");
                            attr.Value = s.Split('(', ')')[1];
                            xmlNode.Attributes?.Append(attr);
                        }

                        xmlNode.InnerText = s.Split('(', ')')[0].Trim();
                        xmlParentNode.AppendChild(xmlNode);
                    }
                    objHeader.AppendChild(xmlParentNode);
                }
                else if (strLine.StartsWith("Powers", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Powers ", string.Empty);
                    string[] split = strLine.Split(',');
                    XmlNode xmlParentNode = doc.CreateElement("powers");
                    foreach (string s in split.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        xmlNode = doc.CreateElement("power");
                        if (s.Contains(':'))
                        {
                            XmlAttribute attr = doc.CreateAttribute("select");
                            attr.Value = s.Split(':')[1].Trim();
                            xmlNode.Attributes?.Append(attr);
                            xmlNode.InnerText = s.Split(':')[0].Trim();
                        }
                        else if (s.Contains('('))
                        {
                            XmlAttribute attr = doc.CreateAttribute("select");
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
                else if (strLine.StartsWith("Programs", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Programs ", string.Empty);
                    string[] split = strLine.Split(',');
                    XmlNode xmlParentNode = doc.CreateElement("gears");
                    foreach (string s in split.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        xmlNode = doc.CreateElement("quality");
                        if (s.Contains('('))
                        {
                            XmlAttribute attr = doc.CreateAttribute("rating");
                            attr.Value = s.Split('(', ')')[1];
                            xmlNode.Attributes?.Append(attr);
                        }

                        xmlNode.InnerText = s.Split('(', ')')[0].Trim();
                        xmlParentNode.AppendChild(xmlNode);
                    }
                    objHeader.AppendChild(xmlParentNode);
                }
                else if (strLine.StartsWith("Qualities", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Qualities ", string.Empty);
                    string[] split = strLine.Split(',');
                    XmlNode xmlParentNode = doc.CreateElement("qualities");
                    foreach (string s in split.Where(s => !string.IsNullOrEmpty(s)))
                    {
                        xmlNode = doc.CreateElement("quality");
                        if (s.Contains('('))
                        {
                            XmlAttribute attr = doc.CreateAttribute("select");
                            attr.Value = s.Split('(', ')')[1];
                            xmlNode.Attributes?.Append(attr);
                        }

                        xmlNode.InnerText = s.Split('(', ')')[0].Trim();
                        xmlParentNode.AppendChild(xmlNode);
                    }
                    objHeader.AppendChild(xmlParentNode);
                }
                else if (strLine.StartsWith("Armor", StringComparison.Ordinal))
                {
                    strLine = strLine.Replace("Armor ", string.Empty);
                    xmlNode = doc.CreateElement("armor");
                    xmlNode.InnerText = strLine.Trim();
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

            txtRaw.Text = XElement.Parse(doc.InnerXml).ToString();
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
            string ret = Regex.Replace(original, pattern, replacement, regexOptions);
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
