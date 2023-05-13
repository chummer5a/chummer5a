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

namespace Chummer
{
    public class DiceRollerListViewItem : ListViewItemWithValue
    {
        private int _intResult;
        private int _intTarget;
        private int _intGlitchMin;
        private int _intBubbleDie;

        public DiceRollerListViewItem(int intResult, int intTarget = 5, int intGlitchMin = 1, bool blnBubbleDie = false)
            : base(intResult)
        {
            _intResult = intResult;
            _intTarget = intTarget;
            _intGlitchMin = intGlitchMin;
            _intBubbleDie = blnBubbleDie.ToInt32();

            UpdateText();
            UpdateColor();
        }

        public int Result
        {
            get => _intResult;
            set
            {
                if (Interlocked.Exchange(ref _intResult, value) == value)
                    return;
                Value = value;
                UpdateText();
                UpdateColor();
            }
        }

        public async ValueTask SetResult(int value, CancellationToken token = default)
        {
            if (Interlocked.Exchange(ref _intResult, value) == value)
                return;
            Value = value;
            await UpdateTextAsync(token).ConfigureAwait(false);
            await UpdateColorAsync(token).ConfigureAwait(false);
        }

        public int Target
        {
            get => _intTarget;
            set
            {
                if (Interlocked.Exchange(ref _intTarget, value) == value)
                    return;
                UpdateColor();
            }
        }

        public async ValueTask SetTargetAsync(int value, CancellationToken token = default)
        {
            if (Interlocked.Exchange(ref _intTarget, value) == value)
                return;
            await UpdateColorAsync(token).ConfigureAwait(false);
        }

        public int GlitchMin
        {
            get => _intGlitchMin;
            set
            {
                if (Interlocked.Exchange(ref _intGlitchMin, value) == value)
                    return;
                UpdateColor();
            }
        }

        public async ValueTask SetGlitchMinAsync(int value, CancellationToken token = default)
        {
            if (Interlocked.Exchange(ref _intGlitchMin, value) == value)
                return;
            await UpdateColorAsync(token).ConfigureAwait(false);
        }

        public bool BubbleDie
        {
            get => _intBubbleDie > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intBubbleDie, intNewValue) == intNewValue)
                    return;
                UpdateText();
            }
        }

        public async ValueTask SetBubbleDie(bool value, CancellationToken token = default)
        {
            int intNewValue = value.ToInt32();
            if (Interlocked.Exchange(ref _intBubbleDie, intNewValue) == intNewValue)
                return;
            await UpdateTextAsync(token).ConfigureAwait(false);
        }

        public bool IsHit => Result >= Target && !BubbleDie;

        public bool IsGlitch => Result <= GlitchMin;

        private void UpdateText(CancellationToken token = default)
        {
            string strText = BubbleDie
                ? LanguageManager.GetString("String_BubbleDie", token: token)
                  + LanguageManager.GetString("String_Space", token: token) + '('
                  + Result.ToString(GlobalSettings.CultureInfo) + ')'
                : Result.ToString(GlobalSettings.CultureInfo);
            Utils.RunOnMainThread(() => Text = strText, token);
        }

        private async ValueTask UpdateTextAsync(CancellationToken token = default)
        {
            string strText = BubbleDie
                ? await LanguageManager.GetStringAsync("String_BubbleDie", token: token).ConfigureAwait(false)
                  + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                  + Result.ToString(GlobalSettings.CultureInfo) + ')'
                : Result.ToString(GlobalSettings.CultureInfo);
            await Utils.RunOnMainThreadAsync(() => Text = strText, token).ConfigureAwait(false);
        }

        private void UpdateColor(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Color objForeColor;
            Color objBackColor;
            if (IsHit)
            {
                if (IsGlitch)
                {
                    objForeColor = ColorManager.DieGlitchHitFore;
                    objBackColor = ColorManager.DieGlitchHitBackground;
                }
                else
                {
                    objForeColor = ColorManager.DieHitFore;
                    objBackColor = ColorManager.DieHitBackground;
                }
            }
            else if (IsGlitch)
            {
                objForeColor = ColorManager.DieGlitchFore;
                objBackColor = ColorManager.DieGlitchBackground;
            }
            else
            {
                objForeColor = ColorManager.WindowText;
                objBackColor = ColorManager.Window;
            }

            Utils.RunOnMainThread(() =>
            {
                ForeColor = objForeColor;
                BackColor = objBackColor;
            }, token);
        }

        private async ValueTask UpdateColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Color objForeColor;
            Color objBackColor;
            if (IsHit)
            {
                if (IsGlitch)
                {
                    objForeColor = await ColorManager.GetDieGlitchHitForeAsync(token).ConfigureAwait(false);
                    objBackColor = await ColorManager.GetDieGlitchHitBackgroundAsync(token).ConfigureAwait(false);
                }
                else
                {
                    objForeColor = await ColorManager.GetDieHitForeAsync(token).ConfigureAwait(false);
                    objBackColor = await ColorManager.GetDieHitBackgroundAsync(token).ConfigureAwait(false);
                }
            }
            else if (IsGlitch)
            {
                objForeColor = await ColorManager.GetDieGlitchForeAsync(token).ConfigureAwait(false);
                objBackColor = await ColorManager.GetDieGlitchBackgroundAsync(token).ConfigureAwait(false);
            }
            else
            {
                objForeColor = await ColorManager.GetWindowTextAsync(token).ConfigureAwait(false);
                objBackColor = await ColorManager.GetWindowAsync(token).ConfigureAwait(false);
            }

            await Utils.RunOnMainThreadAsync(() =>
            {
                ForeColor = objForeColor;
                BackColor = objBackColor;
            }, token).ConfigureAwait(false);
        }
    }
}
