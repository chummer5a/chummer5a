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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Special wrapper around forms that is meant to make sure `using` blocks' disposals always happen in a thread-safe manner
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ThreadSafeForm<T> : IDisposable where T : Form

    {
        public T MyForm { get; }

        private ThreadSafeForm(T frmInner)
        {
            MyForm = frmInner;
        }

        public static ThreadSafeForm<T> Get(Func<T> funcFormConstructor)
        {
            return new ThreadSafeForm<T>(Program.MainForm.DoThreadSafeFunc(funcFormConstructor));
        }

        public static async ValueTask<ThreadSafeForm<T>> GetAsync(Func<T> funcFormConstructor, CancellationToken token = default)
        {
            return new ThreadSafeForm<T>(await Program.MainForm.DoThreadSafeFuncAsync(funcFormConstructor, token));
        }

        public static async ValueTask<ThreadSafeForm<T>> GetAsync(Func<CancellationToken, T> funcFormConstructor, CancellationToken token = default)
        {
            return new ThreadSafeForm<T>(await Program.MainForm.DoThreadSafeFuncAsync(funcFormConstructor, token));
        }

        public void Dispose()
        {
            MyForm?.DoThreadSafe(x => x.Dispose());
        }

        public DialogResult ShowDialog()
        {
            return MyForm.ShowDialog();
        }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            return MyForm.ShowDialog(owner);
        }

        public DialogResult ShowDialogSafe(IWin32Window owner = null, CancellationToken token = default)
        {
            return MyForm.ShowDialogSafe(owner, token);
        }

        public DialogResult ShowDialogSafe(Character objCharacter, CancellationToken token = default)
        {
            return MyForm.ShowDialogSafe(objCharacter, token);
        }

        public Task<DialogResult> ShowDialogSafeAsync(IWin32Window owner = null, CancellationToken token = default)
        {
            return MyForm.ShowDialogSafeAsync(owner, token);
        }

        public Task<DialogResult> ShowDialogSafeAsync(Character objCharacter, CancellationToken token = default)
        {
            return MyForm.ShowDialogSafeAsync(objCharacter, token);
        }

        public Task<DialogResult> ShowDialogNonBlockingAsync(IWin32Window owner = null, CancellationToken token = default)
        {
            return MyForm.ShowDialogNonBlockingAsync(owner, token);
        }

        public Task<DialogResult> ShowDialogNonBlockingSafeAsync(IWin32Window owner = null, CancellationToken token = default)
        {
            return MyForm.ShowDialogNonBlockingSafeAsync(owner, token);
        }

        public Task<DialogResult> ShowDialogNonBlockingSafeAsync(Character objCharacter, CancellationToken token = default)
        {
            return MyForm.ShowDialogNonBlockingSafeAsync(objCharacter, token);
        }
    }
}
