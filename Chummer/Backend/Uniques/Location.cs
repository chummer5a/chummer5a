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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Location.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Location : IHasInternalId, IHasName, IHasNotes, ICanRemove
    {
        private Guid _guiID;
        private string _strName;
        private string _strNotes = string.Empty;
        private readonly Character _objCharacter;
        #region Constructor, Create, Save, Load, and Print Methods
        public Location(Character objCharacter, ObservableCollection<Location> parent, string name = "", bool addToList = true)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _strName = name;
            Parent = parent;
            if (addToList)
                Parent.Add(this);
            Children.CollectionChanged += ChildrenOnCollectionChanged;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("location");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = new Guid();
                _strName = objNode.InnerText;
            }
            else
            {
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            }
            if (Parent == null || Parent.Contains(this)) return;
                Parent.Add(this);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("location");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            return Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            return strReturn;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public TaggedObservableCollection<IHasLocation> Children { get; } = new TaggedObservableCollection<IHasLocation>();

        public ObservableCollection<Location> Parent { get; set; }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsLocation, bool blnAddCategory = false)
        {
            string strText = DisplayName(GlobalOptions.Language);
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsLocation,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion


        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IHasLocation objNewItem in e.NewItems)
                        objNewItem.Location = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IHasLocation objOldItem in e.OldItems)
                        objOldItem.Location = null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (IHasLocation objOldItem in e.OldItems)
                        objOldItem.Location = null;
                    foreach (IHasLocation objNewItem in e.NewItems)
                        objNewItem.Location = this;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (IHasLocation objItem in Children)
                    {
                        objItem.Location = null;
                    }
                    break;
            }
        }

        public bool Remove(Character character)
        {
            foreach (IHasLocation item in Children)
            {
                item.Location = null;
            }
            string strMessage = LanguageManager.GetString("Message_DeleteGearLocation", GlobalOptions.Language);
            return character.ConfirmDelete(strMessage) && Parent.Remove(this);
        }
    }
}
