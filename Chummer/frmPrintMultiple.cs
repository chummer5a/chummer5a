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
                    TreeNode objNode = new TreeNode();
                    objNode.Text = Path.GetFileName(strFileName);
                    objNode.Tag = strFileName;
                    treCharacters.Nodes.Add(objNode);
                }
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (treCharacters.SelectedNode != null)
            {
                treCharacters.SelectedNode.Remove();
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
            Width = cmdPrint.Left + cmdPrint.Width + 19;

            prgProgress.Width = Width - prgProgress.Left - 19;
        }
        #endregion
    }
}