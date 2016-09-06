using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmCharacterRoster : Form
	{
		List<XmlDocument> objCharacterCache = new List<XmlDocument>();
		
		public frmCharacterRoster()
		{
			InitializeComponent();
			foreach (string strFile in GlobalOptions.Instance.ReadStickyMRUList().Where(System.IO.File.Exists))
			{
				LoadCharacter(strFile);
			}
			foreach (string strFile in GlobalOptions.Instance.ReadMRUList().Where(System.IO.File.Exists))
			{
				LoadCharacter(strFile);
			}
			MoveControls();
		}

		private void MoveControls()
		{
			int intWidth = (from TreeNode objNode in treCharacterList.Nodes select objNode.Bounds.Left*2 + objNode.Bounds.Width).Concat(new[] {treCharacterList.Width}).Max();
			int intDifference = intWidth - treCharacterList.Width;
			treCharacterList.Width = intWidth;
			tabCharacterText.Left = treCharacterList.Width + 12;
			tabCharacterText.Width -= intDifference;

			lblPlayerNameLabel.Left = tabCharacterText.Left;
			lblCharacterNameLabel.Left = tabCharacterText.Left;
			lblCareerKarmaLabel.Left = tabCharacterText.Left;
			lblMetatypeLabel.Left = tabCharacterText.Left;
			lblCharacterAliasLabel.Left = tabCharacterText.Left;

			intWidth = lblPlayerNameLabel.Right;
			if (lblCareerKarmaLabel.Right > intWidth)
			{
				intWidth = lblCareerKarmaLabel.Right;
			}
			if (lblCareerKarmaLabel.Right > intWidth)
			{
				intWidth = lblCareerKarmaLabel.Right;
			}
			if (lblMetatypeLabel.Right > intWidth)
			{
				intWidth = lblMetatypeLabel.Right;
			}
			if (lblCharacterAliasLabel.Right > intWidth)
			{
				intWidth = lblCharacterAliasLabel.Right;
			}
			lblPlayerName.Left = intWidth + 12;
			lblCareerKarma.Left = intWidth + 12;
			lblCharacterAlias.Left = intWidth + 12;
			lblMetatype.Left = intWidth + 12;
			lblCharacterName.Left = intWidth + 12;
		}

		private void LoadCharacter(string strFile)
		{
			TreeNode objNode = new TreeNode();
			XmlDocument objXmlDocument = new XmlDocument();
			objXmlDocument.Load(strFile);
			objCharacterCache.Add(objXmlDocument);
			objNode.Tag = objCharacterCache.IndexOf(objXmlDocument);
			XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/character");
			objNode.Text = CalculatedName(objXmlNode);
			treCharacterList.Nodes.Add(objNode);
		}

		private static string CalculatedName(XmlNode objDocument)
		{
			string strName = objDocument["name"]?.InnerText;
			if (String.IsNullOrEmpty(strName))
			{
				strName = "Unnamed Character";
			}
			string strBuildMethod = objDocument["buildmethod"]?.InnerText ?? "Unknown build method";
			bool blnCreated = Convert.ToBoolean(objDocument["created"]?.InnerText);
			string strCreated = "";
			strCreated = LanguageManager.Instance.GetString(blnCreated ? "Title_CareerMode" : "Title_CreateMode");
			string strReturn = $"{strName} ({strBuildMethod} {strCreated})";
			return strReturn;
		}

		void ScrapeCharacter(XmlNode objSource)
		{
			txtCharacterBio.Text = objSource["description"]?.InnerText;
			txtCharacterBackground.Text = objSource["background"]?.InnerText;
			txtCharacterNotes.Text = objSource["gamenotes"]?.InnerText;
			txtCharacterConcept.Text = objSource["concept"]?.InnerText;
			lblCareerKarma.Text = objSource["totalkarma"]?.InnerText;
			lblMetatype.Text = objSource["metatype"]?.InnerText;
			lblPlayerName.Text = objSource["player"]?.InnerText;
			lblCharacterName.Text = objSource["name"]?.InnerText;
			lblCharacterAlias.Text = objSource["alias"]?.InnerText;
			if (!string.IsNullOrEmpty(objSource["mugshot"]?.InnerText))
			{
				byte[] bytImage = Convert.FromBase64String(objSource["mugshot"]?.InnerText);
				MemoryStream objStream = new MemoryStream(bytImage, 0, bytImage.Length);
				objStream.Write(bytImage, 0, bytImage.Length);
				Image imgMugshot = Image.FromStream(objStream, true);
				picMugshot.Image = imgMugshot;
			}
			else
			{
				picMugshot.Image = null;
			}
		}

		private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
		{
			XmlDocument objXmlDocument = objCharacterCache[Convert.ToInt32(treCharacterList.SelectedNode.Tag)];
			XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/character");
			ScrapeCharacter(objXmlNode);
		}
	}
}
