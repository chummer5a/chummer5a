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

namespace Chummer
{
    public class DiceRollerListViewItem : ListViewItemWithValue
    {
        private int _intResult;
        private int _intTarget;
        private int _intGlitchMin;
        private bool _blnBubbleDie;

        public DiceRollerListViewItem(int intResult, int intTarget = 5, int intGlitchMin = 1, bool blnBubbleDie = false) : base(intResult)
        {
            _intResult = intResult;
            _intTarget = intTarget;
            _intGlitchMin = intGlitchMin;
            _blnBubbleDie = blnBubbleDie;

            UpdateText();
            UpdateColor();
        }

        public int Result
        {
            get => _intResult;
            set
            {
                if (_intResult == value)
                    return;
                _intResult = value;
                Value = value;
                UpdateText();
                UpdateColor();
            }
        }

        public int Target
        {
            get => _intTarget;
            set
            {
                if (_intTarget == value)
                    return;
                _intTarget = value;
                UpdateColor();
            }
        }

        public int GlitchMin
        {
            get => _intGlitchMin;
            set
            {
                if (_intGlitchMin == value)
                    return;
                _intGlitchMin = value;
                UpdateColor();
            }
        }

        public bool BubbleDie
        {
            get => _blnBubbleDie;
            set
            {
                if (_blnBubbleDie == value)
                    return;
                _blnBubbleDie = value;
                UpdateText();
            }
        }

        public bool IsHit => Result >= Target && !BubbleDie;

        public bool IsGlitch => Result <= GlitchMin;

        private void UpdateText()
        {
            Text = BubbleDie
                ? LanguageManager.GetString("String_BubbleDie") + LanguageManager.GetString("String_Space") + '(' + Result.ToString(GlobalOptions.CultureInfo) + ')'
                : Result.ToString(GlobalOptions.CultureInfo);
        }

        private void UpdateColor()
        {
            if (IsHit)
            {
                if (IsGlitch)
                {
                    ForeColor = ColorManager.DieGlitchHitFore;
                    BackColor = ColorManager.DieGlitchHitBackground;
                }
                else
                {
                    ForeColor = ColorManager.DieHitFore;
                    BackColor = ColorManager.DieHitBackground;
                }
            }
            else if (IsGlitch)
            {
                ForeColor = ColorManager.DieGlitchFore;
                BackColor = ColorManager.DieGlitchBackground;
            }
            else
            {
                ForeColor = ColorManager.WindowText;
                BackColor = ColorManager.Window;
            }
        }
    }
}
