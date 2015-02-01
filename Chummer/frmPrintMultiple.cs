using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmPrintMultiple : Form
	{
		#region Control Events
		public frmPrintMultiple()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
		}

		private void cmdSelectCharacter_Click(object sender, EventArgs e)
		{
			// Add the selected Files to the list of characters to print.
			if (dlgOpenFile.ShowDialog(this) == DialogResult.OK)
			{
				foreach (string strFileName in dlgOpenFile.FileNames)
				{
					string[] strLongName = strFileName.Split(Path.DirectorySeparatorChar);
					TreeNode objNode = new TreeNode();
					objNode.Text = strLongName[strLongName.Length - 1];
					objNode.Tag = strFileName;
					treCharacters.Nodes.Add(objNode);
				}
			}
		}

		private void cmdDelete_Click(object sender, EventArgs e)
		{
			try
			{
				treCharacters.SelectedNode.Remove();
			}
			catch
			{
			}
		}

		private void cmdPrint_Click(object sender, EventArgs e)
		{
			prgProgress.Value = 0;
			prgProgress.Maximum = treCharacters.Nodes.Count;

			// Write the Character information to a MemoryStream so we don't need to create any files.
			MemoryStream objStream = new MemoryStream();
			XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

			// Being the document.
			objWriter.WriteStartDocument();
			// <characters>
			objWriter.WriteStartElement("characters");

			// Fire the PrintToStream method for all of the characters in the list.
			foreach (TreeNode objNode in treCharacters.Nodes)
			{
				Character objCharacter = new Character();
				objCharacter.FileName = objNode.Tag.ToString();
				objCharacter.Load();

				objCharacter.PrintToStream(objStream, objWriter);
				prgProgress.Value++;
				Application.DoEvents();
			}

			// Finish the document and flush the Writer and Stream.
			// </characters>
			objWriter.WriteEndElement();
			objWriter.WriteEndDocument();
			objWriter.Flush();
			objStream.Flush();

			// Read the stream.
			StreamReader objReader = new StreamReader(objStream);
			objStream.Position = 0;
			XmlDocument objCharacterXML = new XmlDocument();

			// Put the stream into an XmlDocument and send it off to the Viewer.
			string strXML = objReader.ReadToEnd();
			objCharacterXML.LoadXml(strXML);

			objWriter.Close();

			// Set the ProgressBar back to 0.
			prgProgress.Value = 0;

			frmViewer frmViewCharacter = new frmViewer();
			frmViewCharacter.CharacterXML = objCharacterXML;
			frmViewCharacter.SelectedSheet = "Game Master Summary";
			frmViewCharacter.ShowDialog();
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			int intWidth = Math.Max(cmdSelectCharacter.Width, cmdPrint.Width);
			intWidth = Math.Max(intWidth, cmdDelete.Width);
			cmdSelectCharacter.AutoSize = false;
			cmdPrint.AutoSize = false;
			cmdDelete.AutoSize = false;

			cmdSelectCharacter.Width = intWidth;
			cmdPrint.Width = intWidth;
			cmdDelete.Width = intWidth;
			this.Width = cmdPrint.Left + cmdPrint.Width + 19;

			prgProgress.Width = this.Width - prgProgress.Left - 19;
		}
		#endregion
	}
}