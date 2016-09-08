﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmCharacterRoster : Form
	{
		List<CharacterCache> lstCharacterCache = new List<CharacterCache>();
		
		public frmCharacterRoster()
		{
			InitializeComponent();
			LoadCharacters();
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
			lblEssenceLabel.Left = tabCharacterText.Left;

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
			if (lblEssenceLabel.Right > intWidth)
			{
				intWidth = lblEssenceLabel.Right;
			}
			lblEssence.Left = intWidth + 12;
			lblPlayerName.Left = intWidth + 12;
			lblCareerKarma.Left = intWidth + 12;
			lblCharacterAlias.Left = intWidth + 12;
			lblMetatype.Left = intWidth + 12;
			lblCharacterName.Left = intWidth + 12;
		}

		private void LoadCharacters()
		{
			foreach (string strFile in GlobalOptions.Instance.ReadStickyMRUList().Where(System.IO.File.Exists))
			{
				CacheCharacter(strFile);
			}
			foreach (string strFile in GlobalOptions.Instance.ReadMRUList().Where(System.IO.File.Exists))
			{
				CacheCharacter(strFile);
			}
			if (GlobalOptions.Instance.CharacterRosterPath != null)
			{
				string[] objFiles = Directory.GetFiles(GlobalOptions.Instance.CharacterRosterPath);
				//Make sure we're not loading a character that was already loaded by the MRU list.
				foreach (string strFile in objFiles.Where(strFile => strFile.EndsWith(".chum5")))
				{
					bool blnAdd = lstCharacterCache.All(objCache => objCache.FilePath != strFile);
					if (blnAdd)
					{
						CacheCharacter(strFile);
					}
				}
			}
		}
		/// <summary>
		/// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
		/// </summary>
		/// <param name="strFile"></param>
		private void CacheCharacter(string strFile)
		{
			TreeNode objNode = new TreeNode();
			XmlDocument objXmlSource = new XmlDocument();
			objXmlSource.Load(strFile);
			CharacterCache objCache = new CharacterCache();
			XmlNode objXmlSourceNode = objXmlSource.SelectSingleNode("/character");
			if (objXmlSourceNode != null)
			{
				objCache.Description = objXmlSourceNode["description"]?.InnerText;
				objCache.BuildMethod = objXmlSourceNode["buildmethod"]?.InnerText;
				objCache.Background = objXmlSourceNode["background"]?.InnerText;
				objCache.Notes = objXmlSourceNode["gamenotes"]?.InnerText;
				objCache.Concept = objXmlSourceNode["concept"]?.InnerText;
				objCache.Karma = objXmlSourceNode["totalkarma"]?.InnerText;
				objCache.Metatype = objXmlSourceNode["metatype"]?.InnerText;
				objCache.PlayerName = objXmlSourceNode["player"]?.InnerText;
				objCache.CharacterName = objXmlSourceNode["name"]?.InnerText;
				objCache.CharacterAlias = objXmlSourceNode["alias"]?.InnerText;
				objCache.Created = Convert.ToBoolean(objXmlSourceNode["created"]?.InnerText);
				objCache.Essence = objXmlSourceNode["totaless"]?.InnerText;
				if (!string.IsNullOrEmpty(objXmlSourceNode["mugshot"]?.InnerText))
				{
					byte[] bytImage = Convert.FromBase64String(objXmlSourceNode["mugshot"]?.InnerText);
					MemoryStream objStream = new MemoryStream(bytImage, 0, bytImage.Length);
					objStream.Write(bytImage, 0, bytImage.Length);
					Image imgMugshot = Image.FromStream(objStream, true);
					objCache.Mugshot = imgMugshot;
				}
				else
				{
					objCache.Mugshot = null;
				}
			}
			objCache.FilePath = strFile;
			lstCharacterCache.Add(objCache);
			objNode.Tag = lstCharacterCache.IndexOf(objCache);
			
			objNode.Text = CalculatedName(objCache);
			treCharacterList.Nodes.Add(objNode);
		}

		/// <summary>
		/// Generates a name for the treenode based on values contained in the CharacterCache object. 
		/// </summary>
		/// <param name="objCache"></param>
		/// <returns></returns>
		private static string CalculatedName(CharacterCache objCache)
		{
			string strName = objCache.CharacterName;
			if (string.IsNullOrEmpty(strName))
			{
				strName = "Unnamed Character";
			}
			string strBuildMethod = objCache.BuildMethod ?? "Unknown build method";
			bool blnCreated = objCache.Created;
			string strCreated = "";
			strCreated = LanguageManager.Instance.GetString(blnCreated ? "Title_CareerMode" : "Title_CreateMode");
			string strReturn = $"{strName} ({strBuildMethod} - {strCreated})";
			return strReturn;
		}

		/// <summary>
		/// Update the labels and images based on the selected treenode.
		/// </summary>
		/// <param name="objCache"></param>
		private void UpdateCharacter(CharacterCache objCache)
		{
			txtCharacterBio.Text = objCache.Description;
			txtCharacterBackground.Text = objCache.Background;
			txtCharacterNotes.Text = objCache.Notes;
			txtCharacterConcept.Text = objCache.Concept;
			lblCareerKarma.Text = objCache.Karma;
			lblMetatype.Text = objCache.Metatype;
			lblPlayerName.Text = objCache.PlayerName;
			lblCharacterName.Text = objCache.CharacterName;
			lblCharacterAlias.Text = objCache.CharacterAlias;
			lblEssence.Text = objCache.Essence;
			picMugshot.Image = objCache.Mugshot;
		}

		private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
		{
			CharacterCache objCache = lstCharacterCache[Convert.ToInt32(treCharacterList.SelectedNode.Tag)];
			UpdateCharacter(objCache);
		}

		private void treCharacterList_DoubleClick(object sender, EventArgs e)
		{
			CharacterCache objCache = lstCharacterCache[Convert.ToInt32(treCharacterList.SelectedNode.Tag)];
			GlobalOptions.Instance.MainForm.LoadCharacter(objCache.FilePath);
		}

		/// <summary>
		/// Caches a subset of a full character's properties for loading purposes. 
		/// </summary>
		private class CharacterCache
		{
			internal string FilePath { get; set; }
			internal string Description { get; set; }
			internal string Background { get; set; }
			internal string Notes { get; set; }
			internal string Concept { get; set; }
			internal string Karma { get; set; }
			internal string Metatype { get; set; }
			internal string PlayerName { get; set; }
			internal string CharacterName { get; set; }
			internal string CharacterAlias { get; set; }
			internal Image Mugshot { get; set; }
			public string BuildMethod { get; internal set; }
			public bool Created { get; internal set; }
			public string Essence { get; internal set; }
		}
	}
}
