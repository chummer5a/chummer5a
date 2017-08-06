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
using System.Linq;
 using System.Runtime.Serialization;
 using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectProgram : Form
    {
        private string _strSelectedProgram = string.Empty;

        private bool _blnAddAgain = false;
        private string _strLimitCategory = string.Empty;
        private readonly Character _objCharacter;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private bool _blnBiowireEnabled = false;

        #region Control Events
        public frmSelectProgram(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectProgram_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Programs information.
            _objXmlDocument = XmlManager.Instance.Load("complexforms.xml");

            // Populate the Program list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/complexforms/complexform[" + _objCharacter.Options.BookXPath() + "]");

            bool blnCheckForOptional = false;
            XmlNode objXmlCritter = null;
            XmlDocument objXmlCritterDocument = new XmlDocument();
            if (_objCharacter.IsCritter)
            {
                objXmlCritterDocument = XmlManager.Instance.Load("critters.xml");
                objXmlCritter = objXmlCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (objXmlCritter.InnerXml.Contains("<optionalcomplexforms>"))
                    blnCheckForOptional = true;
            }

            // Check to see if the character has the Biowire Echo.
            foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
            {
                if (objMetamagic.Name == "Biowire")
                    _blnBiowireEnabled = true;
            }

            trePrograms.TreeViewNodeSorter = new SortByName();
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                bool blnAdd = true;
                TreeNode nodProgram = new TreeNode();
                nodProgram.Text = objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"].InnerText;
                nodProgram.Tag = objXmlProgram["id"].InnerText;

                // If this is a Sprite with Optional Complex Forms, see if this Complex Form is allowed.
                if (blnCheckForOptional)
                {
                    blnAdd = false;
                    foreach (XmlNode objXmlForm in objXmlCritter.SelectNodes("optionalcomplexforms/complexform"))
                    {
                        if (objXmlForm.InnerText == objXmlProgram["name"].InnerText)
                            blnAdd = true;
                    }
                }

                // Add the Program to the Category node.
                if (blnAdd)
                    trePrograms.Nodes.Add(nodProgram);
            }
            trePrograms.Nodes[0].Expand();
        }

        private void trePrograms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Display the Program information.
            XmlNode objXmlProgram = _objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[id = \"" + trePrograms.SelectedNode.Tag + "\"]");
            if (objXmlProgram != null)
            {
                string strDuration = objXmlProgram["duration"].InnerText;
                string strTarget = objXmlProgram["target"].InnerText;
                string strFV = objXmlProgram["fv"].InnerText;

                lblDuration.Text = strDuration;
                lblTarget.Text = strTarget;
                lblFV.Text = strFV;

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlProgram["source"].InnerText);
                string strPage = objXmlProgram["page"].InnerText;
                if (objXmlProgram["altpage"] != null)
                    strPage = objXmlProgram["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;

                tipTooltip.SetToolTip(lblSource,
                    _objCharacter.Options.LanguageBookLong(objXmlProgram["source"].InnerText) + " " +
                    LanguageManager.Instance.GetString("String_Page") + " " + strPage);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (trePrograms.SelectedNode != null)
            {
                AcceptForm();
            }
        }

        private void trePrograms_DoubleClick(object sender, EventArgs e)
        {
            if (trePrograms.SelectedNode != null)
            {
                AcceptForm();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/complexforms/complexform[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

            trePrograms.Nodes.Clear();

            // Populate the Program list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes(strSearch);
            trePrograms.TreeViewNodeSorter = new SortByName();
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                TreeNode nodProgram = new TreeNode();
                nodProgram.Text = objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"].InnerText;
                nodProgram.Tag = objXmlProgram["id"].InnerText;
                trePrograms.Nodes.Add(nodProgram);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (trePrograms.SelectedNode == null)
            {
                if (trePrograms.Nodes.Count > 0)
                    trePrograms.SelectedNode = trePrograms.Nodes[0];
            }
            if (e.KeyCode == Keys.Down)
            {
                if (trePrograms.SelectedNode != null)
                {
                    trePrograms.SelectedNode = trePrograms.SelectedNode.NextVisibleNode;
                    if (trePrograms.SelectedNode == null)
                        trePrograms.SelectedNode = trePrograms.Nodes[0];
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (trePrograms.SelectedNode != null)
                {
                    trePrograms.SelectedNode = trePrograms.SelectedNode.PrevVisibleNode;
                    if (trePrograms.SelectedNode == null && trePrograms.Nodes.Count > 0)
                        trePrograms.SelectedNode = trePrograms.Nodes[trePrograms.Nodes.Count - 1].LastNode;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Program that was selected in the dialogue.
        /// </summary>
        public string SelectedProgram
        {
            get
            {
                return _strSelectedProgram;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedProgram = trePrograms.SelectedNode.Tag.ToString();
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intLeft = lblDurationLabel.Width;
            intLeft = Math.Max(intLeft, lblTargetLabel.Width);
            intLeft = Math.Max(intLeft, lblFV.Width);
            intLeft = Math.Max(intLeft, lblSourceLabel.Width);

            lblTarget.Left = lblTargetLabel.Left + intLeft + 6;
            lblDuration.Left = lblDurationLabel.Left + intLeft + 6;
            lblFV.Left = lblFVLabel.Left + intLeft + 6;
            lblSource.Left = lblSourceLabel.Left + intLeft + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
