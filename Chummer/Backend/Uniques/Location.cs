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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Location.
    /// </summary>
    [DebuggerDisplay("{nameof(Name)}")]
    public sealed class Location : IHasInternalId, IHasName, IHasNotes, ICanRemove, ICanSort, IDisposable
    {
        private Guid _guiID;
        private string _strName;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private int _intSortOrder;
        private readonly Character _objCharacter;
        private readonly ThreadSafeObservableCollection<IHasLocation> _lstChildren = new ThreadSafeObservableCollection<IHasLocation>();

        #region Constructor, Create, Save, Load, and Print Methods

        public Location(Character objCharacter, ICollection<Location> objParent, string strName = "")
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _strName = strName;
            Parent = objParent;
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            Children.BeforeClearCollectionChanged += ChildrenOnBeforeClearCollectionChanged;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("location");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
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
                _guiID = Guid.NewGuid();
                _strName = objNode.InnerText;
            }
            else
            {
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }

            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

            if (Parent?.Contains(this) == false)
                Parent.Add(this);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlWriter objWriter, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("location");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            if (GlobalSettings.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

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
        public string DisplayNameShort(string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalSettings.Language)
                return Name;
            return _objCharacter.TranslateExtra(
                !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                    ? LanguageManager.ReverseTranslateExtra(Name, GlobalSettings.Language, _objCharacter)
                    : Name, strLanguage);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return DisplayNameShort(strLanguage);
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        public ThreadSafeObservableCollection<IHasLocation> Children
        {
            get
            {
                using (EnterReadLock.Enter(_objCharacter.LockObject))
                    return _lstChildren;
            }
        }

        public ICollection<Location> Parent { get; }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsLocation)
        {
            string strText = DisplayName(GlobalSettings.Language);
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsLocation,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            return objNode;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
                : ColorManager.WindowText;

        #endregion UI Methods

        private void ChildrenOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (IHasLocation objOldItem in e.OldItems)
                objOldItem.Location = null;
        }

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
                    HashSet<IHasLocation> setNewItems = e.NewItems.OfType<IHasLocation>().ToHashSet();
                    foreach (IHasLocation objOldItem in e.OldItems)
                    {
                        if (!setNewItems.Contains(objOldItem))
                            objOldItem.Location = null;
                    }
                    foreach (IHasLocation objNewItem in setNewItems)
                        objNewItem.Location = this;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (IHasLocation objItem in Children)
                        objItem.Location = this;
                    break;
            }
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteGearLocation")))
                return false;

            foreach (IHasLocation item in Children)
                item.Location = null;

            bool blnReturn = Parent?.Contains(this) != true || Parent.Remove(this);

            Dispose();

            return blnReturn;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _lstChildren.Dispose();
        }
    }
}
