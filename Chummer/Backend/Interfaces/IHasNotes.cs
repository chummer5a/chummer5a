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

using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public interface IHasNotes
    {
        string Notes { get; set; }
        Task<string> GetNotesAsync(CancellationToken token = default);
        Task SetNotesAsync(string value, CancellationToken token = default);
        Color NotesColor { get; set; }
        Task<Color> GetNotesColorAsync(CancellationToken token = default);
        Task SetNotesColorAsync(Color value, CancellationToken token = default);
        Color PreferredColor { get; }
        Task<Color> GetPreferredColorAsync(CancellationToken token = default);
    }

    public static class Notes
    {
        /// <summary>
        /// Writes notes to an IHasNotes object, returns True if notes were changed and False otherwise.
        /// </summary>
        public static async Task<bool> WriteNotes(this IHasNotes objNotes, TreeNode treNode, CancellationToken token = default)
        {
            if (objNotes == null || treNode == null)
                return false;
            TreeView objTreeView = treNode.TreeView;
            Form frmToUse = objTreeView != null
                ? await objTreeView.DoThreadSafeFuncAsync(x => x.FindForm(), token: token).ConfigureAwait(false) ?? Program.MainForm
                : Program.MainForm;
            string strNotes = await objNotes.GetNotesAsync(token).ConfigureAwait(false);
            Color objColor = await objNotes.GetNotesColorAsync(token).ConfigureAwait(false);
            using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(strNotes, objColor, token), token).ConfigureAwait(false))
            {
                if (await frmItemNotes.ShowDialogSafeAsync(frmToUse, token).ConfigureAwait(false) != DialogResult.OK)
                    return false;

                await objNotes.SetNotesAsync(frmItemNotes.MyForm.Notes, token).ConfigureAwait(false);
                await objNotes.SetNotesColorAsync(frmItemNotes.MyForm.NotesColor, token).ConfigureAwait(false);
            }

            strNotes = (await objNotes.GetNotesAsync(token).ConfigureAwait(false)).WordWrap();
            objColor = await objNotes.GetPreferredColorAsync(token).ConfigureAwait(false);
            if (objTreeView != null)
            {
                await objTreeView.DoThreadSafeAsync(() =>
                {
                    treNode.ForeColor = objColor;
                    treNode.ToolTipText = strNotes;
                }, token: token).ConfigureAwait(false);
            }
            else
            {
                treNode.ForeColor = objColor;
                treNode.ToolTipText = strNotes;
            }

            return true;
        }
    }
}
