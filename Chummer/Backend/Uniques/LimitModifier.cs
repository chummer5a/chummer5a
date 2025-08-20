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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Skill Limit Modifier.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class LimitModifier : IHasInternalId, IHasName, ICanRemove, IHasCharacterObject, IHasNotes
    {
        private Guid _guiID;
        private bool _blnCanDelete = true;
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strLimit = string.Empty;
        private string _strCondition = string.Empty;
        private int _intBonus;
        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, Load, and Print Methods

        public LimitModifier(Character objCharacter, string strGuid = "")
        {
            // Create the GUID for the new Skill Limit Modifier.
            _guiID = strGuid.IsGuid() ? new Guid(strGuid) : Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Skill Limit Modifier from properties.
        /// </summary>
        /// <param name="strName">The name of the modifier.</param>
        /// <param name="intBonus">The bonus amount.</param>
        /// <param name="strLimit">The limit this modifies.</param>
        /// <param name="strCondition">Condition when the limit modifier is to be activated.</param>
        /// <param name="blnCanDelete">Can this limit modifier be deleted.</param>
        public void Create(string strName, int intBonus, string strLimit, string strCondition, bool blnCanDelete)
        {
            _strName = strName;
            _strLimit = strLimit;
            _intBonus = intBonus;
            _strCondition = strCondition;
            _blnCanDelete = blnCanDelete;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("limitmodifier");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("bonus", _intBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("candelete", _blnCanDelete.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Limit Modifier from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode));
        }

        /// <summary>
        /// Load the Limit Modifier from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetInt32FieldQuickly("bonus", ref _intBonus);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            if (!objNode.TryGetBoolFieldQuickly("candelete", ref _blnCanDelete))
            {
                _blnCanDelete = blnSync
                    ? _objCharacter.Improvements.All(x => x.ImproveType != Improvement.ImprovementType.LimitModifier || x.ImprovedName != InternalId, token)
                    : await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false))
                        .AllAsync(x => x.ImproveType != Improvement.ImprovementType.LimitModifier || x.ImprovedName != InternalId, token).ConfigureAwait(false);
            }
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with</param>
        /// <param name="objCulture">Culture in which to print</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            // <limitmodifier>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("limitmodifier", token: token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "fullname", await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                          token: token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                          token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname_english", await DisplayNameAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync(
                          "name_english", Name,
                          token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("bonus", Bonus.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("limit", Limit, token: token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync("condition", await DisplayConditionAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter
                      .WriteElementStringAsync("condition_english", await DisplayConditionAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);
            }
            finally
            {
                // </limitmodifier>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Skill Limit Modifier in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public Task<string> GetNotesAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return Task.FromResult(_strNotes);
        }

        public Task SetNotesAsync(string value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _strNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        public Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<Color>(token);
            return Task.FromResult(_colNotes);
        }

        public Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _colNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Limit.
        /// </summary>
        public string Limit
        {
            get => _strLimit;
            set => _strLimit = value;
        }

        private string _strCachedCondition = string.Empty;
        private string _strCachedDisplayCondition = string.Empty;
        private string _strCachedDisplayConditionLanguage = string.Empty;

        /// <summary>
        /// Condition.
        /// </summary>
        public string Condition
        {
            get
            {
                // If we've already cached a value for this, just return it.
                if (!string.IsNullOrWhiteSpace(_strCachedCondition))
                {
                    return _strCachedCondition;
                }

                string strReturn = _strCondition;
                // Assume that if the original string contains underscores it's a valid language key. Otherwise, spare checking it against the dictionary.
                if (strReturn.Contains('_'))
                {
                    string strTemp = LanguageManager.GetString(strReturn, GlobalSettings.DefaultLanguage, false);
                    if (!string.IsNullOrWhiteSpace(strTemp))
                        strReturn = strTemp;
                }
                return _strCachedCondition = strReturn;
            }
            set
            {
                if (Interlocked.Exchange(ref _strCondition, value) == value)
                    return;
                _strCachedCondition = string.Empty;
                _strCachedDisplayCondition = string.Empty;
                _strCachedDisplayConditionLanguage = string.Empty;
            }
        }

        public async Task<string> GetConditionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // If we've already cached a value for this, just return it.
            if (!string.IsNullOrWhiteSpace(_strCachedCondition))
            {
                return _strCachedCondition;
            }

            string strReturn = _strCondition;
            // Assume that if the original string contains underscores it's a valid language key. Otherwise, spare checking it against the dictionary.
            if (strReturn.Contains('_'))
            {
                string strTemp = await LanguageManager.GetStringAsync(_strCondition, GlobalSettings.DefaultLanguage, false, token).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(strTemp))
                    strReturn = strTemp;
            }
            return _strCachedCondition = strReturn;
        }

        public string CurrentDisplayCondition => DisplayCondition(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayConditionAsync(CancellationToken token = default) => DisplayConditionAsync(GlobalSettings.Language, token);

        public string DisplayCondition(string strLanguage)
        {
            if (strLanguage == GlobalSettings.DefaultLanguage)
                return Condition;
            // If we've already cached a value for this, just return it.
            // (Ghetto fix cache culture tag and compare to current?)
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayCondition) && strLanguage == _strCachedDisplayConditionLanguage)
            {
                return _strCachedDisplayCondition;
            }

            string strCondition = Condition;
            string strReturn = LanguageManager.TranslateExtra(strCondition, strLanguage, _objCharacter);
            if (string.IsNullOrWhiteSpace(strReturn))
                strReturn = strCondition;
            _strCachedDisplayConditionLanguage = strLanguage;
            return _strCachedDisplayCondition = strReturn;
        }

        public async Task<string> DisplayConditionAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage == GlobalSettings.DefaultLanguage)
                return await GetConditionAsync(token).ConfigureAwait(false);
            // If we've already cached a value for this, just return it.
            // (Ghetto fix cache culture tag and compare to current?)
            if (!string.IsNullOrWhiteSpace(_strCachedDisplayCondition) && strLanguage == _strCachedDisplayConditionLanguage)
            {
                return _strCachedDisplayCondition;
            }

            string strCondition = await GetConditionAsync(token).ConfigureAwait(false);
            string strReturn = await LanguageManager.TranslateExtraAsync(strCondition, strLanguage, _objCharacter, token: token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(strReturn))
                strReturn = strCondition;
            _strCachedDisplayConditionLanguage = strLanguage;
            return _strCachedDisplayCondition = strReturn;
        }

        /// <summary>
        /// Bonus.
        /// </summary>
        public int Bonus
        {
            get => _intBonus;
            set => _intBonus = value;
        }

        /// <summary>
        /// Whether this limit can be modified and/or deleted
        /// </summary>
        public bool CanDelete => _blnCanDelete;

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            return strLanguage == GlobalSettings.DefaultLanguage
                ? Name
                : LanguageManager.TranslateExtra(Name, strLanguage, _objCharacter);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return strLanguage == GlobalSettings.DefaultLanguage
                ? Task.FromResult(Name)
                : LanguageManager.TranslateExtraAsync(Name, strLanguage, _objCharacter, token: token);
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            string strReturn = DisplayNameShort(strLanguage);
            if (_intBonus >= 0)
                strReturn += strSpace + "[+" + _intBonus.ToString(objCulture) + ']';
            else
                strReturn += strSpace + '[' + _intBonus.ToString(objCulture) + ']';
            string strCondition = DisplayCondition(strLanguage);
            if (!string.IsNullOrEmpty(strCondition))
                strReturn += strSpace + '(' + strCondition + ')';
            return strReturn;
        }

        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            if (_intBonus >= 0)
                strReturn += strSpace + "[+" + _intBonus.ToString(objCulture) + ']';
            else
                strReturn += strSpace + '[' + _intBonus.ToString(objCulture) + ']';
            string strCondition = await DisplayConditionAsync(strLanguage, token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strCondition))
                strReturn += strSpace + '(' + strCondition + ')';
            return strReturn;
        }

        #endregion Properties

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsLimitModifier, CancellationToken token = default)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                ContextMenuStrip = cmsLimitModifier,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };
            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return !CanDelete
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return !CanDelete
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
            {
                return !CanDelete
                    ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
            }
            return !CanDelete
                ? ColorManager.GrayText
                : ColorManager.WindowText;
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (_objCharacter.LimitModifiers.Contains(this) && blnConfirmDelete)
            {
                return CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteLimitModifier"))
                       && _objCharacter.LimitModifiers.Remove(this);
            }

            // No character-created limits found, which means it comes from an improvement.
            // TODO: ImprovementSource exists for a reason.
            Program.ShowScrollableMessageBox(LanguageManager.GetString("Message_CannotDeleteLimitModifier"),
                                             LanguageManager.GetString("MessageTitle_CannotDeleteLimitModifier"),
                                             MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (await _objCharacter.LimitModifiers.ContainsAsync(this, token).ConfigureAwait(false) && blnConfirmDelete)
            {
                return await CommonFunctions.ConfirmDeleteAsync(
                           await LanguageManager.GetStringAsync("Message_DeleteLimitModifier", token: token).ConfigureAwait(false), token).ConfigureAwait(false)
                       && await _objCharacter.LimitModifiers.RemoveAsync(this, token).ConfigureAwait(false);
            }

            // No character-created limits found, which means it comes from an improvement.
            // TODO: ImprovementSource exists for a reason.
            await Program.ShowScrollableMessageBoxAsync(
                await LanguageManager.GetStringAsync("Message_CannotDeleteLimitModifier", token: token).ConfigureAwait(false),
                await LanguageManager.GetStringAsync("MessageTitle_CannotDeleteLimitModifier", token: token).ConfigureAwait(false),
                MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
            return false;
        }
    }
}
