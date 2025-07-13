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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Chummer
{
    public readonly struct AvailabilityValue : IComparable, IEquatable<AvailabilityValue>
    {
        public bool AddToParent { get; }
        public bool IncludedInParent { get; }
        public char Suffix { get; }

        public int Value => _objValueInitializer.Value;
        public Task<int> GetValueAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            return _objValueInitializer.IsValueCreated
                ? Task.FromResult(_objValueInitializer.Value)
                : _objAsyncValueInitializer.GetValueAsync(token);
        }
        private readonly Lazy<int> _objValueInitializer;
        private readonly AsyncLazy<int> _objAsyncValueInitializer;

        public AvailabilityValue(int intValue, char chrSuffix, bool blnAddToParent, bool blnIncludedInParent = false)
        {
            AddToParent = blnAddToParent;
            IncludedInParent = blnIncludedInParent;
            switch (chrSuffix)
            {
                case 'F':
                case 'R':
                    Suffix = chrSuffix;
                    break;

                default:
                    Suffix = 'Z';
                    break;
            }
            if (intValue < 0)
                intValue = 0;
            _objValueInitializer = new Lazy<int>(() => intValue);
            _objAsyncValueInitializer = new AsyncLazy<int>(() => Task.FromResult(intValue), Utils.JoinableTaskFactory);
        }

        public AvailabilityValue(int intRating, string strInput, int intBonus = 0, bool blnIncludedInParent = false)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                if (decimal.TryParse(strInput, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decInput))
                {
                    Suffix = 'Z';
                    AddToParent = strInput.StartsWith('+') || strInput.StartsWith('-');
                    int intValue = decInput.StandardRound() + intBonus;
                    if (intValue < 0)
                        intValue = 0;
                    _objValueInitializer = new Lazy<int>(() => intValue);
                    _objAsyncValueInitializer = new AsyncLazy<int>(() => Task.FromResult(intValue), Utils.JoinableTaskFactory);
                }
                else
                {
                    string strAvailExpr = strInput;
                    if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strAvailExpr = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                    }

                    Suffix = strAvailExpr[strAvailExpr.Length - 1];
                    if (Suffix == 'F' || Suffix == 'R')
                    {
                        if (strAvailExpr.StartsWith('+') || strAvailExpr.StartsWith('-'))
                        {
                            AddToParent = true;
                            strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 2);
                        }
                        else
                        {
                            AddToParent = false;
                            strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        }
                    }
                    else if (strAvailExpr.StartsWith('+') || strAvailExpr.StartsWith('-'))
                    {
                        AddToParent = true;
                        strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                    }
                    else
                        AddToParent = false;
                    if (strAvailExpr.Contains("Rating"))
                    {
                        string strRating = intRating.ToString(GlobalSettings.InvariantCultureInfo);
                        strAvailExpr = strAvailExpr.Replace("Rating", strRating).Replace("{Rating}", strRating);
                    }
                    strAvailExpr = strAvailExpr.Replace("/", " div ");
                    if (decimal.TryParse(strAvailExpr, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decProcessedInput))
                    {
                        int intValue = decProcessedInput.StandardRound() + intBonus;
                        if (intValue < 0)
                            intValue = 0;
                        _objValueInitializer = new Lazy<int>(() => intValue);
                        _objAsyncValueInitializer = new AsyncLazy<int>(() => Task.FromResult(intValue), Utils.JoinableTaskFactory);
                    }
                    else
                    {
                        _objValueInitializer = new Lazy<int>(() =>
                        {
                            (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strAvailExpr);
                            int intValue = blnIsSuccess ? ((double)objProcess).StandardRound() + intBonus : intBonus;
                            return Math.Max(intValue, 0);
                        });
                        _objAsyncValueInitializer = new AsyncLazy<int>(async () =>
                        {
                            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strAvailExpr);
                            int intValue = blnIsSuccess ? ((double)objProcess).StandardRound() + intBonus : intBonus;
                            return Math.Max(intValue, 0);
                        }, Utils.JoinableTaskFactory);
                    }
                }
            }
            else
            {
                Suffix = 'Z';
                AddToParent = false;
                if (intBonus < 0)
                    intBonus = 0;
                _objValueInitializer = new Lazy<int>(() => intBonus);
                _objAsyncValueInitializer = new AsyncLazy<int>(() => Task.FromResult(intBonus), Utils.JoinableTaskFactory);
            }
            IncludedInParent = blnIncludedInParent;
        }

        public override string ToString()
        {
            return ToString(GlobalSettings.CultureInfo, GlobalSettings.Language);
        }

        public string ToString(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            int intValue = Value;
            string strBaseAvail = intValue.ToString(objCulture);
            if (AddToParent && intValue >= 0)
                strBaseAvail = '+' + strBaseAvail;
            switch (Suffix)
            {
                case 'F':
                    return strBaseAvail + LanguageManager.GetString("String_AvailForbidden", strLanguage, token: token);

                case 'R':
                    return strBaseAvail + LanguageManager.GetString("String_AvailRestricted", strLanguage, token: token);
            }
            return strBaseAvail;
        }

        public Task<string> ToStringAsync(CancellationToken token = default)
        {
            return ToStringAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);
        }

        public async Task<string> ToStringAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intValue = await GetValueAsync(token).ConfigureAwait(false);
            string strBaseAvail = intValue.ToString(objCulture);
            if (AddToParent && intValue >= 0)
                strBaseAvail = '+' + strBaseAvail;
            switch (Suffix)
            {
                case 'F':
                    return strBaseAvail + await LanguageManager.GetStringAsync("String_AvailForbidden", strLanguage, token: token).ConfigureAwait(false);

                case 'R':
                    return strBaseAvail + await LanguageManager.GetStringAsync("String_AvailRestricted", strLanguage, token: token).ConfigureAwait(false);
            }
            return strBaseAvail;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((AvailabilityValue)obj);
        }

        public int CompareTo(AvailabilityValue objOther)
        {
            int intCompareResult = Value.CompareTo(objOther.Value);
            if (intCompareResult == 0)
            {
                intCompareResult = Suffix.CompareTo(objOther.Suffix);
                if (intCompareResult == 0)
                {
                    intCompareResult = AddToParent.CompareTo(objOther.AddToParent);
                }
            }
            return intCompareResult;
        }

        public Task<int> CompareToAsync(object obj, CancellationToken token = default)
        {
            return CompareToAsync((AvailabilityValue)obj, token);
        }

        public async Task<int> CompareToAsync(AvailabilityValue objOther, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intCompareResult = (await GetValueAsync(token).ConfigureAwait(false)).CompareTo(await objOther.GetValueAsync(token).ConfigureAwait(false));
            if (intCompareResult == 0)
            {
                intCompareResult = Suffix.CompareTo(objOther.Suffix);
                if (intCompareResult == 0)
                {
                    intCompareResult = AddToParent.CompareTo(objOther.AddToParent);
                }
            }
            return intCompareResult;
        }

        public bool Equals(AvailabilityValue other)
        {
            return Value.Equals(other.Value) && Suffix.Equals(other.Suffix) && AddToParent.Equals(other.AddToParent);
        }

        public override bool Equals(object obj)
        {
            if (obj is AvailabilityValue objOther)
            {
                return Equals(objOther);
            }
            return false;
        }

        public async Task<bool> EqualsAsync(AvailabilityValue other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await GetValueAsync(token).ConfigureAwait(false)).Equals(await other.GetValueAsync(token).ConfigureAwait(false))
                && Suffix.Equals(other.Suffix) && AddToParent.Equals(other.AddToParent);
        }

        public Task<bool> EqualsAsync(object obj, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<bool>(token);
            if (obj is AvailabilityValue objOther)
                return EqualsAsync(objOther, token);
            return Task.FromResult(false);
        }

        public override int GetHashCode()
        {
            return (Value, Suffix, AddToParent, IncludedInParent).GetHashCode();
        }

        public static bool operator ==(AvailabilityValue left, AvailabilityValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AvailabilityValue left, AvailabilityValue right)
        {
            return !(left == right);
        }

        public static bool operator <(AvailabilityValue left, AvailabilityValue right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(AvailabilityValue left, AvailabilityValue right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(AvailabilityValue left, AvailabilityValue right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(AvailabilityValue left, AvailabilityValue right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
