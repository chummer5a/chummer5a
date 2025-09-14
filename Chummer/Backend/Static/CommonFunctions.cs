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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Equipment;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.IO;

namespace Chummer
{
    public static class CommonFunctions
    {
        #region XPath Evaluators

        // TODO: implement a sane expression evaluator
        // A single instance of an XmlDocument and its corresponding XPathNavigator helps reduce overhead of evaluating XPaths that just contain mathematical operations
        private static readonly XmlDocument s_ObjXPathNavigatorDocument = new XmlDocument { XmlResolver = null };

        private static readonly DebuggableSemaphoreSlim s_ObjXPathNavigatorDocumentLock = new DebuggableSemaphoreSlim();

        private static readonly ConcurrentStack<XPathNavigator> s_StkXPathNavigatorPool
            = new ConcurrentStack<XPathNavigator>();

        private static readonly ConcurrentDictionary<string, Tuple<bool, object>> s_DicCompiledEvaluations =
            new ConcurrentDictionary<string, Tuple<bool, object>>();

        private static readonly ReadOnlyCollection<char> s_LstInvariantXPathLegalChars = Array.AsReadOnly("1234567890+-*abcdefghilmnorstuvw()[]{}!=<>&;,. ".ToCharArray());

        // Treat as ReadOnlyCollection please, it's only not that because string.IndexOfAny() cannot use a ReadOnlyCollection as its argument
        private static readonly char[] s_LstCharsMarkingNeedOfProcessing = "abcdfghijklmnopqrstuvwxyzABCDFGHIJKLMNOPQRSTUVWXYZ()[]{}!=<>&;+*/\\÷×∙".ToCharArray();

        /// <summary>
        /// Check if a string needs to be processed by an invariant XPath processor to be converted into a number.
        /// If it doesn't, also returns the numerical value of the expression (as a decimal type).
        /// </summary>
        /// <param name="strExpression">String to check.</param>
        /// <param name="decExpressionAsNumber">Numerical value of the expression if it can be evaluated without further XPath processing.</param>
        public static bool DoesNeedXPathProcessingToBeConvertedToNumber(this string strExpression, out decimal decExpressionAsNumber)
        {
            decExpressionAsNumber = 0;
            if (string.IsNullOrWhiteSpace(strExpression))
                return false;
            if (strExpression.IndexOfAny(s_LstCharsMarkingNeedOfProcessing) != -1)
                return true;
            string strTrimmedExpression = strExpression.Trim();
            // If there is a minus sign anywhere except at the very front of the string with a digit following it, return true
            int intLastMinusIndex = strTrimmedExpression.LastIndexOf('-');
            if (intLastMinusIndex >= 1)
                return true;
            if (intLastMinusIndex == 0 && (strTrimmedExpression.Length <= 1 || !char.IsDigit(strTrimmedExpression[1])))
                return true;
            return !decimal.TryParse(strTrimmedExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decExpressionAsNumber);
        }

        /// <summary>
        /// Checks if a string that is meant to hold an expression that is to be processed by an invariant XPath processor has any values that need substitution.
        /// Useful as a sort of initial check to see if we can jump straight to the evaluator or not.
        /// </summary>
        /// <param name="strExpression">String to check.</param>
        /// <param name="blnIncludeLetters">If true, check for uppercase latin letters and opening curly brackets. Otherwise, only check for opening curly brackets.</param>
        public static bool HasValuesNeedingReplacementForXPathProcessing(this string strExpression, bool blnIncludeLetters = true)
        {
            if (string.IsNullOrEmpty(strExpression))
                return false;
            if (blnIncludeLetters)
                // We do not want to fire on lowercase letters because XPath functions are all-lowercase, while every single value we use for replacements has at least one uppercase letter
                return strExpression.IndexOfAny(s_achrUppercaseLatinCharsAndOpenCurlyBracket) >= 0;
            return strExpression.IndexOf('{') >= 0;
        }

        /// <summary>
        /// Checks if a string that is meant to hold an expression that is to be processed by an invariant XPath processor has any values that need substitution.
        /// Useful as a sort of initial check to see if we can jump straight to the evaluator or not.
        /// </summary>
        /// <param name="strExpression">String to check.</param>
        /// <param name="blnIncludeLetters">If true, check for uppercase latin letters and opening curly brackets. Otherwise, only check for opening curly brackets.</param>
        public static bool HasValuesNeedingReplacementForXPathProcessing([NotNull] this StringBuilder sbdExpression, bool blnIncludeLetters = true)
        {
            if (sbdExpression.Length == 0)
                return false;
            if (blnIncludeLetters)
            {
                // We do not want to fire on lowercase letters because XPath functions are all-lowercase, while every single value we use for replacements has at least one uppercase letter
                if (sbdExpression.Length <= Utils.MaxParallelBatchSize)
                    return sbdExpression.Enumerate().Any(x => s_setUppercaseLatinCharsAndOpenCurlyBracket.Contains(x));
                return sbdExpression.Enumerate().AsParallel().Any(x => s_setUppercaseLatinCharsAndOpenCurlyBracket.Contains(x));
            }
            if (sbdExpression.Length <= Utils.MaxParallelBatchSize)
                return sbdExpression.Enumerate().Contains('{');
            return sbdExpression.Enumerate().AsParallel().Contains('{');
        }

        // Treat as ReadOnlyCollection please, it's only not that because key string methods cannot use a ReadOnlyCollection as their argument
        private static readonly char[] s_achrUppercaseLatinCharsAndOpenCurlyBracket = new[]
        {
            '{', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q',
            'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        private static readonly HashSet<char> s_setUppercaseLatinCharsAndOpenCurlyBracket = new HashSet<char>(s_achrUppercaseLatinCharsAndOpenCurlyBracket);

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<Tuple<bool, object>> EvaluateInvariantXPathAsync(string strXPath, CancellationToken token = default)
        {
            return EvaluateInvariantXPathAsync(strXPath, true, token);
        }

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate.</param>
        /// <param name="blnIsMathExpression">Whether the expression is a purely mathematical string, therefore allowing us to replace all possible symbols for mathematical operations with the XPath-appropriate ones.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<Tuple<bool, object>> EvaluateInvariantXPathAsync(string strXPath, bool blnIsMathExpression, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strXPath))
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(false, null);
                s_DicCompiledEvaluations.TryAdd(string.Empty, tupReturn);
                return Task.FromResult(tupReturn);
            }
            if (blnIsMathExpression)
                strXPath = strXPath.Replace("/", " div ").Replace("\\", " div ").Replace("÷", " div ").Replace(" x ", " * ").Replace('∙', '*').Replace('×', '*').Replace('[', '(').Replace(']', ')').TrimStart('+');
            if (!strXPath.IsLegalCharsOnly(true, s_LstInvariantXPathLegalChars))
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(false, strXPath);
                s_DicCompiledEvaluations.TryAdd(strXPath, tupReturn);
                return Task.FromResult(tupReturn);
            }
            if (strXPath == "-")
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(true, 0.0);
                s_DicCompiledEvaluations.TryAdd(strXPath, tupReturn);
                return Task.FromResult(tupReturn);
            }
            return s_DicCompiledEvaluations.GetOrAddAsync(strXPath, async x =>
            {
                bool blnIsSuccess;
                object objReturn;
                try
                {
                    if (!s_StkXPathNavigatorPool.TryPop(out XPathNavigator objEvaluator))
                    {
                        await s_ObjXPathNavigatorDocumentLock.WaitAsync(token).ConfigureAwait(false);
                        try
                        {
                            objEvaluator = s_ObjXPathNavigatorDocument.CreateNavigator();
                        }
                        finally
                        {
                            s_ObjXPathNavigatorDocumentLock.Release();
                        }
                    }

                    try
                    {
                        objReturn = objEvaluator?.Evaluate(x.TrimStart('+'));
                    }
                    finally
                    {
                        s_StkXPathNavigatorPool.Push(objEvaluator);
                    }

                    blnIsSuccess = objReturn != null;
                }
                catch (Exception ex) when ((ex is ArgumentException) || (ex is FormatException) || (ex is XPathException) || (ex is OverflowException))
                {
                    Utils.BreakIfDebug();
                    objReturn = x;
                    blnIsSuccess = false;
                }

                return new Tuple<bool, object>(blnIsSuccess, objReturn); // don't want to store managed objects, only primitives
            }, token: token);
        }

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<bool, object> EvaluateInvariantXPath(string strXPath, CancellationToken token = default)
        {
            return EvaluateInvariantXPath(strXPath, true, token);
        }

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="strXPath">String as XPath Expression to evaluate.</param>
        /// <param name="blnIsMathExpression">Whether the expression is a purely mathematical string, therefore allowing us to replace all division symbols with "div".</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<bool, object> EvaluateInvariantXPath(string strXPath, bool blnIsMathExpression, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(strXPath))
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(false, null);
                s_DicCompiledEvaluations.TryAdd(string.Empty, tupReturn);
                return tupReturn;
            }
            if (blnIsMathExpression)
                strXPath = strXPath.Replace("/", " div ").Replace("\\", " div ").Replace("÷", " div ").Replace(" x ", " * ").Replace('∙', '*').Replace('×', '*').Replace('[', '(').Replace(']', ')').TrimStart('+');
            if (!strXPath.IsLegalCharsOnly(true, s_LstInvariantXPathLegalChars))
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(false, strXPath);
                s_DicCompiledEvaluations.TryAdd(strXPath, tupReturn);
                return tupReturn;
            }
            if (strXPath == "-")
            {
                Tuple<bool, object> tupReturn = new Tuple<bool, object>(true, 0.0);
                s_DicCompiledEvaluations.TryAdd(strXPath, tupReturn);
                return tupReturn;
            }
            token.ThrowIfCancellationRequested();
            return s_DicCompiledEvaluations.GetOrAdd(strXPath, x =>
            {
                bool blnIsSuccess;
                object objReturn;
                try
                {
                    if (!s_StkXPathNavigatorPool.TryPop(out XPathNavigator objEvaluator))
                    {
                        s_ObjXPathNavigatorDocumentLock.SafeWait(token);
                        try
                        {
                            objEvaluator = s_ObjXPathNavigatorDocument.CreateNavigator();
                        }
                        finally
                        {
                            s_ObjXPathNavigatorDocumentLock.Release();
                        }
                    }

                    try
                    {
                        objReturn = objEvaluator?.Evaluate(x.TrimStart('+'));
                    }
                    finally
                    {
                        s_StkXPathNavigatorPool.Push(objEvaluator);
                    }

                    blnIsSuccess = objReturn != null;
                }
                catch (Exception ex) when ((ex is ArgumentException) || (ex is FormatException) || (ex is XPathException) || (ex is OverflowException))
                {
                    Utils.BreakIfDebug();
                    objReturn = x;
                    blnIsSuccess = false;
                }

                return new Tuple<bool, object>(blnIsSuccess, objReturn); // don't want to store managed objects, only primitives
            });
        }

        /// <summary>
        /// Evaluate a string consisting of an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="objXPath">XPath Expression to evaluate</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<Tuple<bool, object>> EvaluateInvariantXPathAsync(XPathExpression objXPath, CancellationToken token = default)
        {
            string strExpression = objXPath.Expression;
            return s_DicCompiledEvaluations.GetOrAddAsync(strExpression, async x =>
            {
                bool blnIsSuccess;
                object objReturn;
                try
                {
                    if (!s_StkXPathNavigatorPool.TryPop(out XPathNavigator objEvaluator))
                    {
                        await s_ObjXPathNavigatorDocumentLock.WaitAsync(token).ConfigureAwait(false);
                        try
                        {
                            objEvaluator = s_ObjXPathNavigatorDocument.CreateNavigator();
                        }
                        finally
                        {
                            s_ObjXPathNavigatorDocumentLock.Release();
                        }
                    }

                    try
                    {
                        objReturn = objEvaluator?.Evaluate(x.TrimStart('+'));
                    }
                    finally
                    {
                        s_StkXPathNavigatorPool.Push(objEvaluator);
                    }

                    blnIsSuccess = objReturn != null;
                }
                catch (Exception ex) when ((ex is ArgumentException) || (ex is FormatException) || (ex is XPathException) || (ex is OverflowException))
                {
                    Utils.BreakIfDebug();
                    objReturn = x;
                    blnIsSuccess = false;
                }

                return
                    new Tuple<bool, object>(blnIsSuccess,
                        objReturn); // don't want to store managed objects, only primitives
            }, token);
        }

        /// <summary>
        /// Evaluate an XPath Expression that could be evaluated on an empty document.
        /// </summary>
        /// <param name="objXPath">XPath Expression to evaluate</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A tuple where the first element is if the calculation was successful and the second element is a System.Boolean, System.Double, System.String, or System.Xml.XPath.XPathNodeIterator depending on the result type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<bool, object> EvaluateInvariantXPath(XPathExpression objXPath, CancellationToken token = default)
        {
            string strExpression = objXPath.Expression;
            return s_DicCompiledEvaluations.GetOrAdd(strExpression, x =>
            {
                bool blnIsSuccess;
                object objReturn;
                try
                {
                    if (!s_StkXPathNavigatorPool.TryPop(out XPathNavigator objEvaluator))
                    {
                        s_ObjXPathNavigatorDocumentLock.SafeWait(token);
                        try
                        {
                            objEvaluator = s_ObjXPathNavigatorDocument.CreateNavigator();
                        }
                        finally
                        {
                            s_ObjXPathNavigatorDocumentLock.Release();
                        }
                    }
                    try
                    {
                        objReturn = objEvaluator?.Evaluate(objXPath);
                    }
                    finally
                    {
                        s_StkXPathNavigatorPool.Push(objEvaluator);
                    }
                    blnIsSuccess = objReturn != null;
                }
                catch (Exception ex) when ((ex is ArgumentException) || (ex is FormatException) || (ex is XPathException) || (ex is OverflowException))
                {
                    Utils.BreakIfDebug();
                    objReturn = x;
                    blnIsSuccess = false;
                }
                
                return new Tuple<bool, object>(blnIsSuccess, objReturn); // don't want to store managed objects, only primitives
            });
        }

        /// <summary>
        /// Parse an XPath for whether it is valid XPath.
        /// </summary>
        /// <param name="strXPathExpression" >XPath Expression to evaluate</param>
        /// <param name="blnIsNullSuccess"   >Should a null or empty result be treated as success?</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCharacterAttributeXPathValidOrNull(string strXPathExpression, bool blnIsNullSuccess = true, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strXPathExpression))
                return blnIsNullSuccess;
            foreach (string strCharAttributeName in Backend.Attributes.AttributeSection.AttributeStrings)
            {
                if (!string.IsNullOrEmpty(strXPathExpression))
                    strXPathExpression = strXPathExpression
                                         .Replace('{' + strCharAttributeName + '}', "1")
                                         .Replace('{' + strCharAttributeName + "Unaug}", "1")
                                         .Replace('{' + strCharAttributeName + "Base}", "1");
            }

            if (string.IsNullOrEmpty(strXPathExpression))
                return true;
            (bool blnIsSuccess, _) = EvaluateInvariantXPath(strXPathExpression, token);
            return blnIsSuccess;
        }

        /// <summary>
        /// Parse an XPath for whether it is valid XPath.
        /// </summary>
        /// <param name="strXPathExpression" >XPath Expression to evaluate</param>
        /// <param name="blnIsNullSuccess"   >Should a null or empty result be treated as success?</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<bool> IsCharacterAttributeXPathValidOrNullAsync(string strXPathExpression, bool blnIsNullSuccess = true, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strXPathExpression))
                return blnIsNullSuccess;
            foreach (string strCharAttributeName in Backend.Attributes.AttributeSection.AttributeStrings)
            {
                if (!string.IsNullOrEmpty(strXPathExpression))
                    strXPathExpression = strXPathExpression
                                         .Replace('{' + strCharAttributeName + '}', "1")
                                         .Replace('{' + strCharAttributeName + "Unaug}", "1")
                                         .Replace('{' + strCharAttributeName + "Base}", "1");
            }

            if (string.IsNullOrEmpty(strXPathExpression))
                return true;
            (bool blnSuccess, object _) = await EvaluateInvariantXPathAsync(strXPathExpression, token).ConfigureAwait(false);
            return blnSuccess;
        }

        #endregion XPath Evaluators

        #region Find Functions

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Gear FindVehicleGear(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            return lstVehicles.FindVehicleGear(strGuid, out Vehicle _, out WeaponAccessory _, out Cyberware _);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Gear was found in.</param>
        /// <param name="objFoundWeaponAccessory">Weapon Accessory that the Gear was found in.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindVehicleGear(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrEmpty(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    Gear objReturn =
                        objVehicle.FindVehicleGear(strGuid, out objFoundWeaponAccessory, out objFoundCyberware, token);
                    if (objReturn != null)
                    {
                        objFoundVehicle = objVehicle;
                        return objReturn;
                    }
                }
            }

            objFoundVehicle = null;
            objFoundWeaponAccessory = null;
            objFoundCyberware = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Gear, Vehicle, WeaponAccessory, Cyberware>> FindVehicleGearAsync(this IAsyncEnumerable<Vehicle> lstVehicles, string strGuid,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            Gear objReturn = null;
            WeaponAccessory objReturnAccessory = null;
            Cyberware objReturnCyberware = null;
            Vehicle objReturnVehicle = null;
            if (!string.IsNullOrEmpty(strGuid) && !strGuid.IsEmptyGuid())
            {
                await lstVehicles.ForEachWithBreakAsync(async objVehicle =>
                {
                    (objReturn, objReturnAccessory, objReturnCyberware) = await objVehicle.FindVehicleGearAsync(strGuid, token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        objReturnVehicle = objVehicle;
                        return false;
                    }
                    return true;
                }, token).ConfigureAwait(false);
            }

            return new Tuple<Gear, Vehicle, WeaponAccessory, Cyberware>(objReturn, objReturnVehicle, objReturnAccessory, objReturnCyberware);
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the VehicleMod.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static VehicleMod FindVehicleMod([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<VehicleMod, bool> funcPredicate, CancellationToken token = default)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            return lstVehicles.FindVehicleMod(funcPredicate, out Vehicle _, out WeaponMount _, token);
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the VehicleMod.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the VehicleMod was found in.</param>
        /// <param name="objFoundWeaponMount">Weapon Mount that the VehicleMod was found in.</param>
        public static VehicleMod FindVehicleMod([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<VehicleMod, bool> funcPredicate, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            foreach (Vehicle objVehicle in lstVehicles)
            {
                VehicleMod objMod = objVehicle.FindVehicleMod(funcPredicate, out objFoundWeaponMount, token);
                if (objMod != null)
                {
                    objFoundVehicle = objVehicle;
                    return objMod;
                }
            }

            objFoundVehicle = null;
            objFoundWeaponMount = null;
            return null;
        }

        /// <summary>
        /// Locate a VehicleMod within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the VehicleMod.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<VehicleMod, Vehicle, WeaponMount>> FindVehicleModAsync([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<VehicleMod, bool> funcPredicate, CancellationToken token = default)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            foreach (Vehicle objVehicle in lstVehicles)
            {
                (VehicleMod objMod, WeaponMount objFoundWeaponMount) = await objVehicle.FindVehicleModAsync(funcPredicate, token).ConfigureAwait(false);
                if (objMod != null)
                {
                    return new Tuple<VehicleMod, Vehicle, WeaponMount>(objMod, objVehicle, objFoundWeaponMount);
                }
            }

            return new Tuple<VehicleMod, Vehicle, WeaponMount>(null, null, null);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Weapon FindVehicleWeapon(this IEnumerable<Vehicle> lstVehicles, string strGuid, CancellationToken token = default)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            return lstVehicles.FindVehicleWeapon(strGuid, out Vehicle _, out WeaponMount _, out VehicleMod _, token);
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle that the Weapon was found in.</param>
        /// <param name="objFoundVehicleMod">Vehicle mod that the Weapon was found in.</param>
        /// <param name="objFoundWeaponMount">Weapon Mount that the Weapon was found in.</param>
        public static Weapon FindVehicleWeapon(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle, out WeaponMount objFoundWeaponMount, out VehicleMod objFoundVehicleMod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    Weapon objReturn = objVehicle.Weapons.DeepFindById(strGuid, token);
                    if (objReturn != null)
                    {
                        objFoundVehicle = objVehicle;
                        objFoundWeaponMount = null;
                        objFoundVehicleMod = null;
                        return objReturn;
                    }

                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        objReturn = objWeaponMount.Weapons.DeepFindById(strGuid, token);
                        if (objReturn != null)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundWeaponMount = objWeaponMount;
                            objFoundVehicleMod = null;
                            return objReturn;
                        }

                        foreach (VehicleMod objMod in objWeaponMount.Mods)
                        {
                            objReturn = objMod.Weapons.DeepFindById(strGuid, token);
                            if (objReturn != null)
                            {
                                objFoundVehicle = objVehicle;
                                objFoundVehicleMod = objMod;
                                objFoundWeaponMount = objWeaponMount;
                                return objReturn;
                            }
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = objMod.Weapons.DeepFindById(strGuid, token);
                        if (objReturn != null)
                        {
                            objFoundVehicle = objVehicle;
                            objFoundVehicleMod = objMod;
                            objFoundWeaponMount = null;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundVehicle = null;
            objFoundWeaponMount = null;
            objFoundVehicleMod = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Weapon, Vehicle, WeaponMount, VehicleMod>> FindVehicleWeaponAsync(this IAsyncEnumerable<Vehicle> lstVehicles, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            Weapon objReturn = null;
            Vehicle objReturnVehicle = null;
            WeaponMount objReturnMount = null;
            VehicleMod objReturnMod = null;
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                await lstVehicles.ForEachWithBreakAsync(async objVehicle =>
                {
                    objReturn = await objVehicle.Weapons.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        objReturnVehicle = objVehicle;
                        return false;
                    }

                    await objVehicle.WeaponMounts.ForEachWithBreakAsync(async objWeaponMount =>
                    {
                        objReturn = await objWeaponMount.Weapons.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                        if (objReturn != null)
                        {
                            objReturnMount = objWeaponMount;
                            return false;
                        }

                        await objWeaponMount.Mods.ForEachWithBreakAsync(async objMod =>
                        {
                            objReturn = await objMod.Weapons.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                            if (objReturn == null)
                                return true;
                            objReturnMod = objMod;
                            return false;
                        }, token).ConfigureAwait(false);

                        if (objReturn == null)
                            return true;
                        objReturnMount = objWeaponMount;
                        return false;
                    }, token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        objReturnVehicle = objVehicle;
                        return false;
                    }

                    await objVehicle.Mods.ForEachWithBreakAsync(async objMod =>
                    {
                        objReturn = await objMod.Weapons.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                        if (objReturn == null)
                            return true;
                        objReturnMod = objMod;
                        return false;
                    }, token).ConfigureAwait(false);

                    if (objReturn == null)
                        return true;
                    objReturnVehicle = objVehicle;
                    return false;
                }, token).ConfigureAwait(false);
            }

            return new Tuple<Weapon, Vehicle, WeaponMount, VehicleMod>(objReturn, objReturnVehicle, objReturnMount, objReturnMod);
        }

        /// <summary>
        /// Locate a Weapon Mount within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        /// <returns></returns>
        public static WeaponMount FindVehicleWeaponMount(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return FindVehicleWeaponMount(lstVehicles, strGuid, out Vehicle _);
        }

        /// <summary>
        /// Locate a Weapon Mount within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        /// <param name="objFoundVehicle">Vehicle in which the Weapon Mount was found.</param>
        public static WeaponMount FindVehicleWeaponMount(this IEnumerable<Vehicle> lstVehicles, string strGuid, out Vehicle objFoundVehicle)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        if (objMod.InternalId == strGuid)
                        {
                            objFoundVehicle = objVehicle;
                            return objMod;
                        }
                    }
                }
            }

            objFoundVehicle = null;
            return null;
        }

        /// <summary>
        /// Locate a Weapon Mount within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<WeaponMount, Vehicle>> FindVehicleWeaponMountAsync(this IEnumerable<Vehicle> lstVehicles, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    WeaponMount objMod =
                        await objVehicle.WeaponMounts.FirstOrDefaultAsync(x => x.InternalId == strGuid, token: token).ConfigureAwait(false);
                    if (objMod != null)
                        return new Tuple<WeaponMount, Vehicle>(objMod, objVehicle);
                }
            }

            return new Tuple<WeaponMount, Vehicle>(null, null);
        }

        /// <summary>
        /// Locate a Vehicle Mod within the character's Vehicles' weapon mounts.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        public static VehicleMod FindVehicleWeaponMountMod(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            return FindVehicleWeaponMountMod(lstVehicles, strGuid, out WeaponMount _);
        }

        /// <summary>
        /// Locate a Vehicle Mod within the character's Vehicles' weapon mounts.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        /// <param name="outMount">Weapon Mount in which the Vehicle Mod was found.</param>
        public static VehicleMod FindVehicleWeaponMountMod(this IEnumerable<Vehicle> lstVehicles, string strGuid, out WeaponMount outMount)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        foreach (VehicleMod objVehicleMod in objWeaponMount.Mods)
                        {
                            if (objVehicleMod.InternalId == strGuid)
                            {
                                outMount = objWeaponMount;
                                return objVehicleMod;
                            }
                        }
                    }
                }
            }

            outMount = null;
            return null;
        }

        /// <summary>
        /// Locate a Vehicle Mod within the character's Vehicles' weapon mounts.
        /// </summary>
        /// <param name="strGuid">Internal Id with which to look for the vehicle mod.</param>
        /// <param name="lstVehicles">List of root vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<VehicleMod, WeaponMount>> FindVehicleWeaponMountModAsync(this IEnumerable<Vehicle> lstVehicles, string strGuid, CancellationToken token = default)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    VehicleMod objReturnMod = null;
                    WeaponMount objReturnMount = null;
                    await objVehicle.WeaponMounts.ForEachWithBreakAsync(async objWeaponMount =>
                    {
                        objReturnMod =
                            await objWeaponMount.Mods.FirstOrDefaultAsync(x => x.InternalId == strGuid, token).ConfigureAwait(false);
                        if (objReturnMod != null)
                        {
                            objReturnMount = objWeaponMount;
                            return false;
                        }

                        return true;
                    }, token).ConfigureAwait(false);
                    if (objReturnMod != null)
                        return new Tuple<VehicleMod, WeaponMount>(objReturnMod, objReturnMount);
                }
            }

            return new Tuple<VehicleMod, WeaponMount>(null, null);
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static WeaponAccessory FindVehicleWeaponAccessory(this IEnumerable<Vehicle> lstVehicles, string strGuid)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    WeaponAccessory objReturn = objVehicle.Weapons.FindWeaponAccessory(strGuid);
                    if (objReturn != null)
                    {
                        return objReturn;
                    }

                    foreach (WeaponMount objMod in objVehicle.WeaponMounts)
                    {
                        objReturn = objMod.Weapons.FindWeaponAccessory(strGuid);
                        if (objReturn != null)
                        {
                            return objReturn;
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objReturn = objMod.Weapons.FindWeaponAccessory(strGuid);
                        if (objReturn != null)
                        {
                            return objReturn;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a Weapon Accessory within the character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Weapon Accessory to find.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<WeaponAccessory> FindVehicleWeaponAccessoryAsync(this IEnumerable<Vehicle> lstVehicles, string strGuid, CancellationToken token = default)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    WeaponAccessory objReturn = await objVehicle.Weapons.FindWeaponAccessoryAsync(strGuid, token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        return objReturn;
                    }

                    await objVehicle.WeaponMounts.ForEachWithBreakAsync(async objMod =>
                    {
                        objReturn = await objMod.Weapons.FindWeaponAccessoryAsync(strGuid, token).ConfigureAwait(false);
                        return objReturn == null;
                    }, token: token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        return objReturn;
                    }

                    await objVehicle.Mods.ForEachWithBreakAsync(async objMod =>
                    {
                        objReturn = await objMod.Weapons.FindWeaponAccessoryAsync(strGuid, token).ConfigureAwait(false);
                        return objReturn == null;
                    }, token: token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        return objReturn;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        public static Cyberware FindVehicleCyberware([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<Cyberware, bool> funcPredicate)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            return lstVehicles.FindVehicleCyberware(funcPredicate, out VehicleMod _);
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public static Cyberware FindVehicleCyberware([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<Cyberware, bool> funcPredicate, out VehicleMod objFoundVehicleMod)
        {
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            foreach (Vehicle objVehicle in lstVehicles)
            {
                Cyberware objReturn = objVehicle.FindVehicleCyberware(funcPredicate, out objFoundVehicleMod);
                if (objReturn != null)
                {
                    return objReturn;
                }
            }

            objFoundVehicleMod = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Cyberware within the character's Vehicles.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="lstVehicles">List of Vehicles to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Cyberware, VehicleMod>> FindVehicleCyberwareAsync([NotNull] this IEnumerable<Vehicle> lstVehicles, [NotNull] Func<Cyberware, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstVehicles == null)
                throw new ArgumentNullException(nameof(lstVehicles));
            foreach (Vehicle objVehicle in lstVehicles)
            {
                (Cyberware objReturn, VehicleMod objReturnMod) = await objVehicle.FindVehicleCyberwareAsync(funcPredicate, token).ConfigureAwait(false);
                if (objReturn != null)
                {
                    return new Tuple<Cyberware, VehicleMod>(objReturn, objReturnMod);
                }
            }

            return new Tuple<Cyberware, VehicleMod>(null, null);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        public static Gear FindArmorGear(this IEnumerable<Armor> lstArmors, string strGuid, CancellationToken token = default)
        {
            if (lstArmors == null)
                throw new ArgumentNullException(nameof(lstArmors));
            return lstArmors.FindArmorGear(strGuid, out Armor _, out ArmorMod _, token);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="objFoundArmor">Armor that the Gear was found in.</param>
        /// <param name="objFoundArmorMod">Armor mod that the Gear was found in.</param>
        public static Gear FindArmorGear(this IEnumerable<Armor> lstArmors, string strGuid, out Armor objFoundArmor, out ArmorMod objFoundArmorMod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstArmors == null)
                throw new ArgumentNullException(nameof(lstArmors));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Armor objArmor in lstArmors)
                {
                    Gear objReturn = objArmor.GearChildren.DeepFindById(strGuid, token);
                    if (objReturn != null)
                    {
                        objFoundArmor = objArmor;
                        objFoundArmorMod = null;
                        return objReturn;
                    }

                    foreach (ArmorMod objMod in objArmor.ArmorMods)
                    {
                        objReturn = objMod.GearChildren.DeepFindById(strGuid, token);
                        if (objReturn != null)
                        {
                            objFoundArmor = objArmor;
                            objFoundArmorMod = objMod;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundArmor = null;
            objFoundArmorMod = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Gear, Armor, ArmorMod>> FindArmorGearAsync(this IAsyncEnumerable<Armor> lstArmors, string strGuid, CancellationToken token = default)
        {
            if (lstArmors == null)
                throw new ArgumentNullException(nameof(lstArmors));
            Gear objReturn = null;
            Armor objReturnArmor = null;
            ArmorMod objReturnMod = null;
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                await lstArmors.ForEachWithBreakAsync(async objArmor =>
                {
                    objReturn = await objArmor.GearChildren.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                    if (objReturn != null)
                    {
                        objReturnArmor = objArmor;
                        return false;
                    }

                    await objArmor.ArmorMods.ForEachWithBreakAsync(async objMod =>
                    {
                        objReturn = await objMod.GearChildren.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
                        if (objReturn == null)
                            return true;

                        objReturnMod = objMod;
                        return false;
                    }, token).ConfigureAwait(false);

                    if (objReturn == null)
                        return true;

                    objReturnArmor = objArmor;
                    return false;
                }, token).ConfigureAwait(false);
            }

            return new Tuple<Gear, Armor, ArmorMod>(objReturn, objReturnArmor, objReturnMod);
        }

        /// <summary>
        /// Locate an Armor Mod within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the ArmorMod to Find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        public static ArmorMod FindArmorMod(this IEnumerable<Armor> lstArmors, string strGuid)
        {
            if (lstArmors == null)
                throw new ArgumentNullException(nameof(lstArmors));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                return lstArmors.SelectMany(objArmor => objArmor.ArmorMods).FirstOrDefault(objMod => objMod.InternalId == strGuid);
            }

            return null;
        }

        /// <summary>
        /// Locate an Armor Mod within the character's Armors.
        /// </summary>
        /// <param name="strGuid">InternalId of the ArmorMod to Find.</param>
        /// <param name="lstArmors">List of Armors to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<ArmorMod> FindArmorModAsync(this IEnumerable<Armor> lstArmors, string strGuid, CancellationToken token = default)
        {
            if (lstArmors == null)
                throw new ArgumentNullException(nameof(lstArmors));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Armor objArmor in lstArmors)
                {
                    ArmorMod objReturn =
                        await objArmor.ArmorMods.FirstOrDefaultAsync(objMod => objMod.InternalId == strGuid, token).ConfigureAwait(false);
                    if (objReturn != null)
                        return objReturn;
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        public static Gear FindCyberwareGear(this IEnumerable<Cyberware> lstCyberware, string strGuid, CancellationToken token = default)
        {
            if (lstCyberware == null)
                throw new ArgumentNullException(nameof(lstCyberware));
            return lstCyberware.FindCyberwareGear(strGuid, out Cyberware _, token);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public static Gear FindCyberwareGear(this IEnumerable<Cyberware> lstCyberware, string strGuid, out Cyberware objFoundCyberware, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCyberware == null)
                throw new ArgumentNullException(nameof(lstCyberware));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Cyberware objCyberware in lstCyberware.DeepWhere(x => x.Children, x => x.GearChildren.Count > 0))
                {
                    Gear objReturn = objCyberware.GearChildren.DeepFindById(strGuid, token);

                    if (objReturn != null)
                    {
                        objFoundCyberware = objCyberware;
                        return objReturn;
                    }
                }
            }

            objFoundCyberware = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Cyberware.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstCyberware">List of Cyberware to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Gear, Cyberware>> FindCyberwareGearAsync(this IAsyncEnumerable<Cyberware> lstCyberware, string strGuid, CancellationToken token = default)
        {
            if (lstCyberware == null)
                throw new ArgumentNullException(nameof(lstCyberware));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Cyberware objCyberware in await lstCyberware.DeepWhereAsync(x => x.GetChildrenAsync(token),
                             async x => await (await x.GetGearChildrenAsync(token).ConfigureAwait(false)).GetCountAsync(token).ConfigureAwait(false) > 0,
                             token: token).ConfigureAwait(false))
                {
                    Gear objReturn = await (await objCyberware.GetGearChildrenAsync(token).ConfigureAwait(false)).DeepFindByIdAsync(strGuid, token: token)
                        .ConfigureAwait(false);

                    if (objReturn != null)
                    {
                        return new Tuple<Gear, Cyberware>(objReturn, objCyberware);
                    }
                }
            }

            return new Tuple<Gear, Cyberware>(null, null);
        }

        /// <summary>
        /// Locate a WeaponAccessory within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        public static WeaponAccessory FindWeaponAccessory(this IEnumerable<Weapon> lstWeapons, string strGuid)
        {
            if (lstWeapons == null)
                throw new ArgumentNullException(nameof(lstWeapons));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                return lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Count > 0)
                                 .SelectMany(objWeapon => objWeapon.WeaponAccessories)
                                 .FirstOrDefault(objAccessory => objAccessory.InternalId == strGuid);
            }

            return null;
        }

        /// <summary>
        /// Locate a WeaponAccessory within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the WeaponAccessory to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<WeaponAccessory> FindWeaponAccessoryAsync(this IEnumerable<Weapon> lstWeapons, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstWeapons == null)
                throw new ArgumentNullException(nameof(lstWeapons));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                if (lstWeapons is IAsyncEnumerable<Weapon> lstWeaponsAsync)
                {
                    foreach (Weapon objWeapon in await lstWeaponsAsync.DeepWhereAsync(x => x.Children,
                                 async x => await x.WeaponAccessories.GetCountAsync(token).ConfigureAwait(false) > 0, token: token).ConfigureAwait(false))
                    {
                        WeaponAccessory objReturn =
                            await objWeapon.WeaponAccessories.FirstOrDefaultAsync(x => x.InternalId == strGuid, token).ConfigureAwait(false);
                        if (objReturn != null)
                            return objReturn;
                    }
                }
                else
                {
                    foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children,
                                 x => x.WeaponAccessories.Count > 0))
                    {
                        WeaponAccessory objReturn =
                            await objWeapon.WeaponAccessories.FirstOrDefaultAsync(x => x.InternalId == strGuid, token).ConfigureAwait(false);
                        if (objReturn != null)
                            return objReturn;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Gear FindWeaponGear(this IEnumerable<Weapon> lstWeapons, string strGuid, CancellationToken token = default)
        {
            if (lstWeapons == null)
                throw new ArgumentNullException(nameof(lstWeapons));
            return lstWeapons.FindWeaponGear(strGuid, out WeaponAccessory _, token);
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="objFoundAccessory">WeaponAccessory that the Gear was found in.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Gear FindWeaponGear(this IEnumerable<Weapon> lstWeapons, string strGuid, out WeaponAccessory objFoundAccessory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstWeapons == null)
                throw new ArgumentNullException(nameof(lstWeapons));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                foreach (Weapon objWeapon in lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.GearChildren.Count > 0, token)))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        Gear objReturn = objAccessory.GearChildren.DeepFindById(strGuid, token);

                        if (objReturn != null)
                        {
                            objFoundAccessory = objAccessory;
                            return objReturn;
                        }
                    }
                }
            }

            objFoundAccessory = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within the character's Weapons.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="lstWeapons">List of Weapons to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<Gear, WeaponAccessory>> FindWeaponGearAsync(this IAsyncEnumerable<Weapon> lstWeapons, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstWeapons == null)
                throw new ArgumentNullException(nameof(lstWeapons));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                Gear objReturn = null;
                WeaponAccessory objReturnAccessory = null;
                foreach (Weapon objWeapon in await lstWeapons.DeepWhereAsync(x => x.Children,
                             x => x.WeaponAccessories.AnyAsync(
                                 async y => await y.GearChildren.GetCountAsync(token).ConfigureAwait(false) > 0, token),
                             token: token).ConfigureAwait(false))
                {
                    await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                    {
                        objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(strGuid, token)
                            .ConfigureAwait(false);
                        if (objReturn != null)
                        {
                            objReturnAccessory = objAccessory;
                            return false;
                        }
                        return true;
                    }, token).ConfigureAwait(false);
                    if (objReturn != null)
                        return new Tuple<Gear, WeaponAccessory>(objReturn, objReturnAccessory);
                }
            }

            return new Tuple<Gear, WeaponAccessory>(null, null);
        }

        /// <summary>
        /// Locate an Enhancement within the character's Enhancements.
        /// </summary>
        /// <param name="strGuid">InternalId of the Art to find.</param>
        /// <param name="objCharacter">The character to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Enhancement FindEnhancement(this Character objCharacter, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                using (objCharacter.LockObject.EnterReadLock(token))
                {
                    foreach (Enhancement objEnhancement in objCharacter.Enhancements)
                    {
                        if (objEnhancement.InternalId == strGuid)
                            return objEnhancement;
                    }

                    return objCharacter.Powers.SelectMany(objPower => objPower.Enhancements)
                                       .FirstOrDefault(objEnhancement => objEnhancement.InternalId == strGuid);
                }
            }

            return null;
        }

        /// <summary>
        /// Locate an Enhancement within the character's Enhancements.
        /// </summary>
        /// <param name="strGuid">InternalId of the Art to find.</param>
        /// <param name="objCharacter">The character to search.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Enhancement> FindEnhancementAsync(this Character objCharacter, string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (!string.IsNullOrWhiteSpace(strGuid) && !strGuid.IsEmptyGuid())
            {
                IAsyncDisposable objLocker = await objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    Enhancement objReturn =
                        await objCharacter.Enhancements.FirstOrDefaultAsync(x => x.InternalId == strGuid, token).ConfigureAwait(false);
                    if (objReturn != null)
                        return objReturn;

                    await objCharacter.Powers.ForEachWithBreakAsync(async objPower =>
                    {
                        objReturn = await objPower.Enhancements.FirstOrDefaultAsync(x => x.InternalId == strGuid, token).ConfigureAwait(false);
                        return objReturn == null;
                    }, token: token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }

            return null;
        }

        #endregion Find Functions

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strAltCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static string LanguageBookCodeFromAltCode(string strAltCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strAltCode))
                return string.Empty;
            XPathNavigator xmlOriginalCode = objSettings != null
                ? objSettings.LoadDataXPath("books.xml", strLanguage, token: token)
                : XmlManager.LoadXPath("books.xml", null, strLanguage, token: token);
            xmlOriginalCode = xmlOriginalCode?.SelectSingleNodeAndCacheExpression("/chummer/books/book[altcode = " + strAltCode.CleanXPath() + "]/code", token);
            return xmlOriginalCode?.Value ?? strAltCode;
        }

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strAltCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<string> LanguageBookCodeFromAltCodeAsync(string strAltCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strAltCode))
                return string.Empty;
            XPathNavigator xmlOriginalCode = objSettings != null
                ? await objSettings.LoadDataXPathAsync("books.xml", strLanguage, token: token).ConfigureAwait(false)
                : await XmlManager.LoadXPathAsync("books.xml", null, strLanguage, token: token).ConfigureAwait(false);
            xmlOriginalCode = xmlOriginalCode?.SelectSingleNodeAndCacheExpression(
                "/chummer/books/book[altcode = " + strAltCode.CleanXPath() + "]/code", token: token);
            return xmlOriginalCode?.Value ?? strAltCode;
        }

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static string LanguageBookShort(string strCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (GlobalSettings.Language == GlobalSettings.DefaultLanguage)
                return strCode;
            if (string.IsNullOrWhiteSpace(strCode))
                return string.Empty;
            XPathNavigator xmlAltCode = objSettings != null
                ? objSettings.LoadDataXPath("books.xml", strLanguage, token: token)
                : XmlManager.LoadXPath("books.xml", null, strLanguage, token: token);
            xmlAltCode = xmlAltCode?.SelectSingleNodeAndCacheExpression("/chummer/books/book[code = " + strCode.CleanXPath() + "]/altcode", token);
            return xmlAltCode?.Value ?? strCode;
        }

        /// <summary>
        /// Book code (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<string> LanguageBookShortAsync(string strCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (GlobalSettings.Language == GlobalSettings.DefaultLanguage)
                return strCode;
            if (string.IsNullOrWhiteSpace(strCode))
                return string.Empty;
            XPathNavigator xmlAltCode = objSettings != null
                ? await objSettings.LoadDataXPathAsync("books.xml", strLanguage, token: token).ConfigureAwait(false)
                : await XmlManager.LoadXPathAsync("books.xml", null, strLanguage, token: token).ConfigureAwait(false);
            xmlAltCode = xmlAltCode?.SelectSingleNodeAndCacheExpression(
                "/chummer/books/book[code = " + strCode.CleanXPath() + "]/altcode", token: token);
            return xmlAltCode?.Value ?? strCode;
        }

        /// <summary>
        /// Book name (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static string LanguageBookLong(string strCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strCode))
                return string.Empty;
            XPathNavigator xmlBook = objSettings != null
                ? objSettings.LoadDataXPath("books.xml", strLanguage, token: token)
                : XmlManager.LoadXPath("books.xml", null, strLanguage, token: token);
            xmlBook = xmlBook?.SelectSingleNodeAndCacheExpression("/chummer/books/book[code = " + strCode.CleanXPath() + ']', token);
            if (xmlBook != null)
            {
                string strReturn = xmlBook.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                   ?? xmlBook.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                if (!string.IsNullOrWhiteSpace(strReturn))
                    return strReturn;
            }

            return string.Empty;
        }

        /// <summary>
        /// Book name (using the translated version if applicable).
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<string> LanguageBookLongAsync(string strCode, string strLanguage = "", CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strCode))
                return string.Empty;
            XPathNavigator xmlBook = objSettings != null
                ? await objSettings.LoadDataXPathAsync("books.xml", strLanguage, token: token).ConfigureAwait(false)
                : await XmlManager.LoadXPathAsync("books.xml", null, strLanguage, token: token).ConfigureAwait(false);
            if (xmlBook != null)
            {
                xmlBook = xmlBook.SelectSingleNodeAndCacheExpression(
                    "/chummer/books/book[code = " + strCode.CleanXPath() + ']', token: token);
                if (xmlBook != null)
                {
                    string strReturn = xmlBook.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                       ?? xmlBook.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                    if (!string.IsNullOrWhiteSpace(strReturn))
                        return strReturn;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Fetch the in-book description of a given object.
        /// </summary>
        public static string GetBookNotes(XmlNode objNode, string strName, string strDisplayName, string strSource, string strPage, string strDisplayPage, Character objCharacter, CancellationToken token = default)
        {
            return GetBookNotes(objNode, strName, strDisplayName, strSource, strPage, strDisplayPage, objCharacter?.Settings, token);
        }

        /// <summary>
        /// Fetch the in-book description of a given object.
        /// </summary>
        public static string GetBookNotes(XmlNode objNode, string strName, string strDisplayName, string strSource, string strPage, string strDisplayPage, CharacterSettings objSettings, CancellationToken token = default)
        {
            string strEnglishNameOnPage = strName;
            string strNameOnPage = string.Empty;
            // make sure we have something and not just an empty tag
            if (objNode.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                !string.IsNullOrEmpty(strNameOnPage))
                strEnglishNameOnPage = strNameOnPage;

            using (objSettings.LockObject.EnterReadLock(token))
            {
                string strNotes = GetTextFromPdf(strSource + ' ' + strPage, strEnglishNameOnPage, objSettings, token);

                if (!string.IsNullOrEmpty(strNotes)
                    || GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                      StringComparison.OrdinalIgnoreCase))
                    return strNotes;
                string strTranslatedNameOnPage = strDisplayName;

                // don't check again it is not translated
                if (strTranslatedNameOnPage == strName)
                    return strNotes;

                // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                if (objNode.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                    && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                    strTranslatedNameOnPage = strNameOnPage;

                return GetTextFromPdf(strSource + ' ' + strDisplayPage,
                                      strTranslatedNameOnPage, objSettings, token);
            }
        }

        /// <summary>
        /// Fetch the in-book description of a given object.
        /// </summary>
        public static async Task<string> GetBookNotesAsync(XmlNode objNode, string strName, string strDisplayName, string strSource, string strPage, string strDisplayPage, Character objCharacter, CancellationToken token = default)
        {
            return await GetBookNotesAsync(objNode, strName, strDisplayName, strSource, strPage, strDisplayPage, objCharacter != null ? await objCharacter.GetSettingsAsync(token).ConfigureAwait(false) : null, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the in-book description of a given object.
        /// </summary>
        public static async Task<string> GetBookNotesAsync(XmlNode objNode, string strName, string strDisplayName, string strSource, string strPage, string strDisplayPage, CharacterSettings objSettings, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strEnglishNameOnPage = strName;
            string strNameOnPage = string.Empty;
            // make sure we have something and not just an empty tag
            if (objNode.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                !string.IsNullOrEmpty(strNameOnPage))
                strEnglishNameOnPage = strNameOnPage;

            IAsyncDisposable objLocker = await objSettings.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strNotes = await GetTextFromPdfAsync(strSource + ' ' + strPage, strEnglishNameOnPage, objSettings, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(strNotes)
                    || GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                        StringComparison.OrdinalIgnoreCase))
                    return strNotes;
                string strTranslatedNameOnPage = strDisplayName;

                // don't check again it is not translated
                if (strTranslatedNameOnPage == strName)
                    return strNotes;

                // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                if (objNode.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                    && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                    strTranslatedNameOnPage = strNameOnPage;

                return await GetTextFromPdfAsync(strSource + ' ' + strDisplayPage,
                                                 strTranslatedNameOnPage, objSettings, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns an XPath Expression's string that searches an item's name for a string.
        /// </summary>
        /// <param name="strNeedle">String to look for</param>
        /// <param name="strNameElement">Name of the element that corresponds to the item's untranslated name.</param>
        /// <param name="strTranslateElement">Name of the element that corresponds to the item's translated name.</param>
        /// <returns></returns>
        public static string GenerateSearchXPath(string strNeedle, string strNameElement = "name", string strTranslateElement = "translate")
        {
            if (string.IsNullOrEmpty(strNeedle))
                return string.Empty;
            string strSearchText = strNeedle.CleanXPath().ToUpperInvariant();
            // Construct a second needle for French where we have zero-width spaces between a starting consonant and an apostrophe in order to fix ListView's weird way of alphabetically sorting names
            string strSearchText2 = string.Empty;
            if (GlobalSettings.Language.StartsWith("FR", StringComparison.OrdinalIgnoreCase) && strSearchText.Contains('\''))
            {
                strSearchText2 = strSearchText
                                 .Replace("D\'A", "D\u200B\'A")
                                 .Replace("D\'À", "D\u200B\'À")
                                 .Replace("D\'Â", "D\u200B\'Â")
                                 .Replace("D\'E", "D\u200B\'E")
                                 .Replace("D\'É", "D\u200B\'É")
                                 .Replace("D\'È", "D\u200B\'È")
                                 .Replace("D\'Ê", "D\u200B\'Ê")
                                 .Replace("D\'I", "D\u200B\'I")
                                 .Replace("D\'Î", "D\u200B\'Î")
                                 .Replace("D\'Ï", "D\u200B\'Ï")
                                 .Replace("D\'O", "D\u200B\'O")
                                 .Replace("D\'Ô", "D\u200B\'Ô")
                                 .Replace("D\'Œ", "D\u200B\'Œ")
                                 .Replace("D\'U", "D\u200B\'U")
                                 .Replace("D\'Û", "D\u200B\'Û")
                                 .Replace("L\'A", "L\u200B\'A")
                                 .Replace("L\'À", "L\u200B\'À")
                                 .Replace("L\'Â", "L\u200B\'Â")
                                 .Replace("L\'E", "L\u200B\'E")
                                 .Replace("L\'É", "L\u200B\'É")
                                 .Replace("L\'È", "L\u200B\'È")
                                 .Replace("L\'Ê", "L\u200B\'Ê")
                                 .Replace("L\'I", "L\u200B\'I")
                                 .Replace("L\'Î", "L\u200B\'Î")
                                 .Replace("L\'Ï", "L\u200B\'Ï")
                                 .Replace("L\'O", "L\u200B\'O")
                                 .Replace("L\'Ô", "L\u200B\'Ô")
                                 .Replace("L\'Œ", "L\u200B\'Œ")
                                 .Replace("L\'U", "L\u200B\'U")
                                 .Replace("L\'Û", "L\u200B\'Û");
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strReturn = "((not(" + strTranslateElement + ") and contains(translate(" + strNameElement
                               // ReSharper disable once StringLiteralTypo
                               + ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøœřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżßａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŒŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻßABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'), "
                               + strSearchText + ")) " +
                               "or contains(translate(" + strTranslateElement
                               // ReSharper disable once StringLiteralTypo
                               + ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøœřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżßａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŒŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻßABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'), "
                               + strSearchText + "))";
            if (!string.IsNullOrEmpty(strSearchText2))
            {
                strReturn = '(' + strReturn + " or ((not(" + strTranslateElement + ") and contains(translate("
                            + strNameElement
                            // ReSharper disable once StringLiteralTypo
                            + ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøœřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżßａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŒŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻßABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'), "
                            + strSearchText2 + ")) " +
                            "or contains(translate(" + strTranslateElement
                            // ReSharper disable once StringLiteralTypo
                            + ",'abcdefghijklmnopqrstuvwxyzàáâãäåæăąāçčćđďèéêëěęēėģğıìíîïīįķłĺļñňńņòóôõöőøœřŕšśşțťùúûüűůūųẃẁŵẅýỳŷÿžźżßａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÆĂĄĀÇČĆĐĎÈÉÊËĚĘĒĖĢĞIÌÍÎÏĪĮĶŁĹĻÑŇŃŅÒÓÔÕÖŐØŒŘŔŠŚŞȚŤÙÚÛÜŰŮŪŲẂẀŴẄÝỲŶŸŽŹŻßABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'), "
                            + strSearchText2 + ")))";
            }

            return strReturn;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <param name="intMinValueFromForce">Minimum value to return if Force is present (greater than 0).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static int ExpressionToInt(string strIn, int intForce = 0, int intOffset = 0, int intMinValueFromForce = 1, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset;
            int intValue = 1;
            string strForce = intForce.ToString(GlobalSettings.InvariantCultureInfo);
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = EvaluateInvariantXPath(
                    strIn.Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce), token);
                if (blnIsSuccess)
                    intValue = ((double)objProcess).StandardRound();
            }
            catch (OverflowException)
            {
                // Result is text and not a double
            }
            catch (InvalidCastException)
            {
                // swallow this
            }

            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < intMinValueFromForce)
                    return intMinValueFromForce;
            }
            else if (intValue < 0)
                return 0;

            return intValue;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="intOffset">Dice offset.</param>
        /// <param name="intMinValueFromForce">Minimum value to return if Force is present (greater than 0).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<int> ExpressionToIntAsync(string strIn, int intForce = 0, int intOffset = 0, int intMinValueFromForce = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(strIn))
                return intOffset;
            int intValue = 1;
            string strForce = intForce.ToString(GlobalSettings.InvariantCultureInfo);
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = await EvaluateInvariantXPathAsync(
                    strIn.Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intValue = ((double)objProcess).StandardRound();
            }
            catch (OverflowException)
            {
                // Result is text and not a double
            }
            catch (InvalidCastException)
            {
                // swallow this
            }

            intValue += intOffset;
            if (intForce > 0)
            {
                if (intValue < intMinValueFromForce)
                    return intMinValueFromForce;
            }
            else if (intValue < 0)
                return 0;

            return intValue;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="decOffset">Dice offset.</param>
        /// <param name="decMinValueFromForce">Minimum value to return if Force is present (greater than 0).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal ExpressionToDecimal(string strIn, int intForce = 0, decimal decOffset = 0, decimal decMinValueFromForce = 1.0m, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(strIn))
                return decOffset;
            decimal decValue = 1;
            string strForce = intForce.ToString(GlobalSettings.InvariantCultureInfo);
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = EvaluateInvariantXPath(
                    strIn.Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce), token);
                if (blnIsSuccess)
                    decValue = Convert.ToDecimal((double)objProcess);
            }
            catch (OverflowException)
            {
                // Result is text and not a double
            }
            catch (InvalidCastException)
            {
                // swallow this
            }

            decValue += decOffset;
            if (intForce > 0)
            {
                if (decValue < decMinValueFromForce)
                    return decMinValueFromForce;
            }
            else if (decValue < 0)
                return 0;

            return decValue;
        }

        /// <summary>
        /// Convert Force, 1D6, or 2D6 into a usable value.
        /// </summary>
        /// <param name="strIn">Expression to convert.</param>
        /// <param name="intForce">Force value to use.</param>
        /// <param name="decOffset">Dice offset.</param>
        /// <param name="decMinValueFromForce">Minimum value to return if Force is present (greater than 0).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public static async Task<decimal> ExpressionToDecimalAsync(string strIn, int intForce = 0, decimal decOffset = 0, decimal decMinValueFromForce = 1.0m, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(strIn))
                return decOffset;
            decimal decValue = 1;
            string strForce = intForce.ToString(GlobalSettings.InvariantCultureInfo);
            // This statement is wrapped in a try/catch since trying 1 div 2 results in an error with XSLT.
            try
            {
                (bool blnIsSuccess, object objProcess) = await EvaluateInvariantXPathAsync(
                    strIn.Replace("F", strForce).Replace("1D6", strForce).Replace("2D6", strForce), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decValue = Convert.ToDecimal((double)objProcess);
            }
            catch (OverflowException)
            {
                // Result is text and not a double
            }
            catch (InvalidCastException)
            {
                // swallow this
            }

            decValue += decOffset;
            if (intForce > 0)
            {
                if (decValue < decMinValueFromForce)
                    return decMinValueFromForce;
            }
            else if (decValue < 0)
                return 0;

            return decValue;
        }

        public static void ShiftTabsOnMouseScroll(object sender, MouseEventArgs e)
        {
            if (!GlobalSettings.SwitchTabsOnHoverScroll || e == null)
                return;
            if (sender is TabControl tabControl && tabControl.DisplayRectangle.Contains(e.Location))
            {
                tabControl.SelectedIndex = (tabControl.SelectedIndex + e.Delta) % tabControl.TabCount;
            }
        }

        /// <summary>
        /// Verify that the user wants to delete an item.
        /// </summary>
        public static bool ConfirmDelete(string strMessage, CancellationToken token = default)
        {
            return !GlobalSettings.ConfirmDelete ||
                   Program.ShowScrollableMessageBox(strMessage, LanguageManager.GetString("MessageTitle_Delete", token: token),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Verify that the user wants to delete an item.
        /// </summary>
        public static async Task<bool> ConfirmDeleteAsync(string strMessage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return !GlobalSettings.ConfirmDelete ||
                   await Program.ShowScrollableMessageBoxAsync(strMessage, await LanguageManager.GetStringAsync("MessageTitle_Delete", token: token).ConfigureAwait(false),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) == DialogResult.Yes;
        }

        /// <summary>
        /// Verify that the user wants to spend their Karma and did not accidentally click the button.
        /// </summary>
        public static bool ConfirmKarmaExpense(string strMessage, CancellationToken token = default)
        {
            return !GlobalSettings.ConfirmKarmaExpense ||
                   Program.ShowScrollableMessageBox(strMessage, LanguageManager.GetString("MessageTitle_ConfirmKarmaExpense", token: token),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Verify that the user wants to spend their Karma and did not accidentally click the button.
        /// </summary>
        public static async Task<bool> ConfirmKarmaExpenseAsync(string strMessage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return !GlobalSettings.ConfirmKarmaExpense ||
                   await Program.ShowScrollableMessageBoxAsync(strMessage, await LanguageManager.GetStringAsync("MessageTitle_ConfirmKarmaExpense", token: token).ConfigureAwait(false),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) == DialogResult.Yes;
        }

        public static Task<XmlDocument> GenerateCharactersExportXml(CultureInfo objCultureInfo, string strLanguage, params Character[] lstCharacters)
        {
            return GenerateCharactersExportXml(objCultureInfo, strLanguage, CancellationToken.None, lstCharacters);
        }

        public static async Task<XmlDocument> GenerateCharactersExportXml(CultureInfo objCultureInfo, string strLanguage, CancellationToken objToken, params Character[] lstCharacters)
        {
            objToken.ThrowIfCancellationRequested();
            XmlDocument objReturn = new XmlDocument { XmlResolver = null };
            // Write the Character information to a RecyclableMemoryStream so we don't need to create any files.
            using (RecyclableMemoryStream objStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
            {
                bool blnWriterError = false;
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    // Begin the document.
                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);
                    try
                    {
                        // </characters>
                        XmlElementWriteHelper objCharactersElement = await objWriter.StartElementAsync("characters", token: objToken).ConfigureAwait(false);
                        try
                        {
                            foreach (Character objCharacter in lstCharacters)
                            {
                                if (objCharacter.IsDisposed)
                                    continue;
                                await objCharacter.PrintToXmlTextWriter(objWriter, objCultureInfo, strLanguage, objToken).ConfigureAwait(false);
                                if (objWriter.WriteState == WriteState.Error)
                                {
                                    Utils.BreakIfDebug();
                                    throw new InvalidOperationException(nameof(objWriter));
                                }
                            }
                        }
                        finally
                        {
                            // </characters>
                            await objCharactersElement.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // Finish the document and flush the Writer and Stream.
                        if (objWriter.WriteState == WriteState.Error)
                        {
                            objWriter.Close();
                            blnWriterError = true;
                        }
                        else
                        {
                            await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                            await objWriter.FlushAsync().ConfigureAwait(false);
                        }
                    }
                }
                if (blnWriterError)
                    throw new InvalidOperationException();

                objToken.ThrowIfCancellationRequested();

                // Read the stream.
                objStream.Position = 0;
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                using (XmlReader objXmlReader = XmlReader.Create(objReader, GlobalSettings.UnSafeXmlReaderSettings))
                    objReturn.Load(objXmlReader);
            }

            return objReturn;
        }

        public static bool DictionaryValuesEqual<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (ReferenceEquals(first, second))
                return true;
            if (first == null || second == null)
                return false;
            if (first.Count != second.Count)
                return false;
            foreach (var kvp in first)
            {
                if (!second.TryGetValue(kvp.Key, out var value))
                    return false;
                if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                    return false;
            }
            return true;
        }

        #region PDF Functions

        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="sender">Control from which this method was called.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task OpenPdfFromControl(object sender, CancellationToken token = default)
        {
            if (!(sender is Control objControl))
                return;
            Control objLoopControl = objControl;
            CharacterSettings objSettings = null;
            while (objLoopControl != null)
            {
                if (objLoopControl is CharacterShared objShared)
                {
                    Character objCharacter = objShared.CharacterObject;
                    if (objCharacter != null)
                        objSettings = await objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                    else
                        objSettings = null;
                    break;
                }

                objLoopControl = await objLoopControl.DoThreadSafeFuncAsync(x => x.Parent, token: token).ConfigureAwait(false);
            }
            await OpenPdfFromControl(sender, objSettings, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="sender">Control from which this method was called.</param>
        /// <param name="objSettings">Settings to use for custom data. If null, no custom data will be used.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task OpenPdfFromControl(object sender, CharacterSettings objSettings, CancellationToken token = default)
        {
            if (!(sender is Control objControl))
                return;
            CursorWait objCursorWait
                = await CursorWait.NewAsync(await objControl.DoThreadSafeFuncAsync(x => x.FindForm(), token: token).ConfigureAwait(false) ?? objControl, token: token).ConfigureAwait(false);
            try
            {
                await OpenPdf(await objControl.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false), objSettings, string.Empty,
                              string.Empty, true, token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Opens a PDF file using the provided source information.
        /// </summary>
        /// <param name="strSource">Book code and page number to open.</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strPdfParameters">PDF parameters to use. If empty, use GlobalSettings.PdfParameters.</param>
        /// <param name="strPdfAppPath">PDF parameters to use. If empty, use GlobalSettings.PdfAppPath.</param>
        /// <param name="blnOpenOptions">If set to True, the user will be prompted whether they wish to link a PDF if no PDF is found.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task OpenPdf(string strSource, CharacterSettings objSettings = null, string strPdfParameters = "", string strPdfAppPath = "", bool blnOpenOptions = false, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strSource))
                return;
            if (string.IsNullOrEmpty(strPdfParameters))
                strPdfParameters = GlobalSettings.PdfParameters;
            if (string.IsNullOrEmpty(strPdfAppPath))
                strPdfAppPath = GlobalSettings.PdfAppPath;
            // The user must have specified the arguments of their PDF application in order to use this functionality.
            while (string.IsNullOrWhiteSpace(strPdfParameters) || string.IsNullOrWhiteSpace(strPdfAppPath) || !File.Exists(strPdfAppPath))
            {
                if (!blnOpenOptions || await Program.ShowScrollableMessageBoxAsync(await LanguageManager.GetStringAsync("Message_NoPDFProgramSet", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_NoPDFProgramSet", token: token).ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) != DialogResult.Yes)
                    return;
                CursorWait objCursorWait = await CursorWait.NewAsync(Program.MainForm, token: token).ConfigureAwait(false);
                try
                {
                    using (ThreadSafeForm<EditGlobalSettings> frmOptions
                           = await ThreadSafeForm<EditGlobalSettings>.GetAsync(() => new EditGlobalSettings(), token).ConfigureAwait(false))
                    {
                        if (string.IsNullOrWhiteSpace(strPdfAppPath) || !File.Exists(strPdfAppPath))
                            // ReSharper disable once AccessToDisposedClosure
                            await frmOptions.MyForm.DoLinkPdfReader(token).ConfigureAwait(false);
                        if (await frmOptions.ShowDialogSafeAsync(Program.MainForm, token).ConfigureAwait(false) != DialogResult.OK)
                            return;
                        strPdfParameters = GlobalSettings.PdfParameters;
                        strPdfAppPath = GlobalSettings.PdfAppPath;
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }

            string strBook;
            int intPage;
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string[] astrSourceParts = null;
            try
            {
                if (!string.IsNullOrEmpty(strSpace))
                    astrSourceParts = strSource.SplitFixedSizePooledArray(strSpace, 2, StringSplitOptions.RemoveEmptyEntries);
                else
                {
                    astrSourceParts = ArrayPool<string>.Shared.Rent(2);
                    if (strSource.StartsWith("SR5", StringComparison.Ordinal))
                    {
                        astrSourceParts[0] = "SR5";
                        astrSourceParts[1] = strSource.Substring(3);
                    }
                    else if (strSource.StartsWith("R5", StringComparison.Ordinal))
                    {
                        astrSourceParts[0] = "R5";
                        astrSourceParts[1] = strSource.Substring(2);
                    }
                    else
                    {
                        int i = strSource.Length - 1;
                        for (; i >= 0; --i)
                        {
                            if (!char.IsNumber(strSource, i))
                            {
                                break;
                            }
                        }

                        astrSourceParts[0] = strSource.Substring(0, i);
                        astrSourceParts[1] = strSource.Substring(i);
                    }
                }

                if (string.IsNullOrEmpty(astrSourceParts[1]) || !int.TryParse(astrSourceParts[1], out intPage))
                    return;

                // Make sure the page is actually a number that we can use as well as being 1 or higher.
                if (intPage < 1)
                    return;

                // Revert the sourcebook code to the one from the XML file if necessary.
                strBook = await LanguageBookCodeFromAltCodeAsync(astrSourceParts[0], string.Empty, objSettings, token).ConfigureAwait(false);
            }
            finally
            {
                if (astrSourceParts != null)
                    ArrayPool<string>.Shared.Return(astrSourceParts);
            }
            // Retrieve the sourcebook information including page offset and PDF application name.
            if (!(await GlobalSettings.GetSourcebookInfosAsync(token).ConfigureAwait(false))
                    .TryGetValue(strBook, out SourcebookInfo objBookInfo) || objBookInfo == null)
                // If the sourcebook was not found, we can't open anything.
                return;
            Uri uriPath = null;
            try
            {
                uriPath = new Uri(objBookInfo.Path);
            }
            catch (UriFormatException)
            {
                // Silently swallow the error because PDF fetching is usually done in the background
                objBookInfo.Path = string.Empty;
            }

            // Check if the file actually exists.
            while (uriPath == null || !File.Exists(uriPath.LocalPath))
            {
                if (!blnOpenOptions)
                    return;
                if (await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_NoLinkedPDF", token: token).ConfigureAwait(false), await LanguageBookLongAsync(strBook, token: token).ConfigureAwait(false)),
                        await LanguageManager.GetStringAsync("MessageTitle_NoLinkedPDF", token: token).ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: token).ConfigureAwait(false) != DialogResult.Yes)
                    return;
                CursorWait objCursorWait = await CursorWait.NewAsync(Program.MainForm, token: token).ConfigureAwait(false);
                try
                {
                    using (ThreadSafeForm<EditGlobalSettings> frmOptions
                           = await ThreadSafeForm<EditGlobalSettings>.GetAsync(() => new EditGlobalSettings(), token).ConfigureAwait(false))
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        await frmOptions.MyForm.DoLinkPdf(objBookInfo.Code, token).ConfigureAwait(false);
                        if (await frmOptions.ShowDialogSafeAsync(Program.MainForm, token).ConfigureAwait(false) != DialogResult.OK)
                            return;
                        uriPath = null;
                        try
                        {
                            uriPath = new Uri(objBookInfo.Path);
                        }
                        catch (UriFormatException)
                        {
                            // Silently swallow the error because PDF fetching is usually done in the background
                            objBookInfo.Path = string.Empty;
                        }
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }

            intPage += objBookInfo.Offset;

            string strParams = strPdfParameters
                               .Replace("{page}", intPage.ToString(GlobalSettings.InvariantCultureInfo))
                               .Replace("{localpath}", uriPath.LocalPath)
                               .Replace("{absolutepath}", uriPath.AbsolutePath);
            Process.Start(strPdfAppPath, strParams);
        }

        /// <summary>
        /// Gets a textblock from a given PDF document.
        /// </summary>
        /// <param name="strSource">Formatted Source to search, ie SR5 70</param>
        /// <param name="strText">String to search for as an opener</param>
        /// <param name="objSettings">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static string GetTextFromPdf(string strSource, string strText, CharacterSettings objSettings = null, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => GetTextFromPdfCoreAsync(true, strSource, strText, objSettings, token), token);
        }

        /// <summary>
        /// Gets a textblock from a given PDF document.
        /// </summary>
        /// <param name="strSource">Formatted Source to search, ie SR5 70</param>
        /// <param name="strText">String to search for as an opener</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<string> GetTextFromPdfAsync(string strSource, string strText, CharacterSettings objSettings = null, CancellationToken token = default)
        {
            return GetTextFromPdfCoreAsync(false, strSource, strText, objSettings, token);
        }

        private static readonly ConcurrentHashSet<PdfDocument> _setDocumentsProcessing = new ConcurrentHashSet<PdfDocument>();

        [CLSCompliant(false)]
        public static string GetPdfTextFromPageSafe(PdfDocument objDocument, int intPage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Very hacky implementation of a locker, but we need it because iText's Pdf document readers are not thread-safe
            while (!_setDocumentsProcessing.TryAdd(objDocument))
                Utils.SafeSleep(token);
            try
            {
                token.ThrowIfCancellationRequested();
                SimpleTextExtractionStrategy objStrategy = new SimpleTextExtractionStrategy();
                token.ThrowIfCancellationRequested();
                PdfCanvasProcessor objCanvasProcessor = new PdfCanvasProcessor(objStrategy);
                token.ThrowIfCancellationRequested();
                PdfPage objPage = objDocument.GetPage(intPage);
                token.ThrowIfCancellationRequested();
                objCanvasProcessor.ProcessPageContent(objPage);
                token.ThrowIfCancellationRequested();
                return objStrategy.GetResultantText();
            }
            finally
            {
                _setDocumentsProcessing.Remove(objDocument);
            }
        }

        [CLSCompliant(false)]
        public static async Task<string> GetPdfTextFromPageSafeAsync(PdfDocument objDocument, int intPage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Very hacky implementation of a locker, but we need it because iText's Pdf document readers are not thread-safe
            while (!_setDocumentsProcessing.TryAdd(objDocument))
                await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                SimpleTextExtractionStrategy objStrategy = new SimpleTextExtractionStrategy();
                token.ThrowIfCancellationRequested();
                PdfCanvasProcessor objCanvasProcessor = new PdfCanvasProcessor(objStrategy);
                token.ThrowIfCancellationRequested();
                PdfPage objPage = objDocument.GetPage(intPage);
                token.ThrowIfCancellationRequested();
                objCanvasProcessor.ProcessPageContent(objPage);
                token.ThrowIfCancellationRequested();
                return objStrategy.GetResultantText();
            }
            finally
            {
                _setDocumentsProcessing.Remove(objDocument);
            }
        }

        /// <summary>
        /// Gets a textblock from a given PDF document.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strSource">Formatted Source to search, ie SR5 70</param>
        /// <param name="strText">String to search for as an opener</param>
        /// <param name="objSettings">Settings whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static async Task<string> GetTextFromPdfCoreAsync(bool blnSync, string strSource, string strText, CharacterSettings objSettings, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strText) || string.IsNullOrEmpty(strSource))
                return strText;

            string strBook;
            int intPage;
            string[] strTemp = strSource.SplitFixedSizePooledArray(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (string.IsNullOrEmpty(strTemp[1]) || !int.TryParse(strTemp[1], out intPage))
                    return string.Empty;
                // Make sure the page is actually a number that we can use as well as being 1 or higher.
                if (intPage < 1)
                    return string.Empty;

                token.ThrowIfCancellationRequested();

                // Revert the sourcebook code to the one from the XML file if necessary.
                strBook = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? LanguageBookCodeFromAltCode(strTemp[0], string.Empty, objSettings, token)
                    : await LanguageBookCodeFromAltCodeAsync(strTemp[0], string.Empty, objSettings, token).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<string>.Shared.Return(strTemp);
            }

            token.ThrowIfCancellationRequested();

            // Retrieve the sourcebook information including page offset and PDF application name.
            if (!(blnSync
                        ? GlobalSettings.SourcebookInfos
                        : await GlobalSettings.GetSourcebookInfosAsync(token).ConfigureAwait(false))
                    .TryGetValue(strBook, out SourcebookInfo objBookInfo) || objBookInfo == null)
                // If the sourcebook was not found, we can't open anything.
                return string.Empty;

            token.ThrowIfCancellationRequested();
            Uri uriPath;
            try
            {
                uriPath = new Uri(objBookInfo.Path);
            }
            catch (UriFormatException)
            {
                return string.Empty;
            }

            // Check if the file actually exists.
            if (!File.Exists(uriPath.LocalPath))
                return string.Empty;

            token.ThrowIfCancellationRequested();

            intPage += objBookInfo.Offset;

            // due to the tag <nameonpage> for the qualities those variants are no longer needed,
            // as such the code would run at most half of the comparisons with the variants
            // but to be sure we find everything still strip unnecessary stuff after the ':' and any number in it.
            // PS: does any qualities have numbers on them? Or is that a chummer thing?
            string strTextToSearch = strText;
            int intPos = strTextToSearch.IndexOf(':');
            if (intPos != -1)
                strTextToSearch = strTextToSearch.Substring(0, intPos);
            strTextToSearch = strTextToSearch.Trim().TrimEndOnce(" I", " II", " III", " IV");

            token.ThrowIfCancellationRequested();

            List<string> lstStringFromPdf = new List<string>(30);
            int intTitleIndex = -1;
            int intBlockEndIndex = -1;
            int intExtraAllCapsInfo = 0;
            bool blnTitleWithColon = false; // it is either an uppercase title or title in a paragraph with a colon
            string strReturn = blnSync ? Utils.SafelyRunSynchronously(FetchTexts, token) : await Task.Run(FetchTexts, token).ConfigureAwait(false);

            async Task<string> FetchTexts()
            {
                token.ThrowIfCancellationRequested();
                PdfDocument objPdfDocument = objBookInfo.CachedPdfDocument;
                if (objPdfDocument == null)
                    return string.Empty;
                token.ThrowIfCancellationRequested();
                int intMaxPagesToRead = 3; // parse at most 3 pages of content
                // Loop through each page, starting at the listed page + offset.
                for (; intPage <= objPdfDocument.GetNumberOfPages(); ++intPage)
                {
                    token.ThrowIfCancellationRequested();
                    // failsafe if something goes wrong, I guess no description takes more than two full pages?
                    if (intMaxPagesToRead-- == 0)
                        break;

                    int intProcessedStrings = lstStringFromPdf.Count;
                    // each page should have its own text extraction strategy for it to work properly
                    // this way we don't need to check for previous page appearing in the current page
                    // https://stackoverflow.com/questions/35911062/why-are-gettextfrompage-from-itextsharp-returning-longer-and-longer-strings

                    token.ThrowIfCancellationRequested();
                    string strPageText = string.Empty;
                    try
                    {
                        strPageText = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetPdfTextFromPageSafe(objPdfDocument, intPage, token)
                            : await GetPdfTextFromPageSafeAsync(objPdfDocument, intPage, token).ConfigureAwait(false);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        token.ThrowIfCancellationRequested();
                        return blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString("Error_Message_PDF_IndexOutOfBounds", false, token)
                            : await LanguageManager.GetStringAsync("Error_Message_PDF_IndexOutOfBounds", false, token)
                                                   .ConfigureAwait(false);
                    }
                    // Don't generate a new canceled exception if the one we generated originates from our token
                    catch (OperationCanceledException) when (!token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    // All sorts of weird things can happen when we hammer I/O from constantly running tasks, and there's no good way of handling these without this very broad try-catch
#if DEBUG
                    catch (Exception e)
                    {
                        // Make sure we throw the cancellation token if it was triggered first
                        token.ThrowIfCancellationRequested();
                        Utils.BreakIfDebug();
                        return e.ToString();
                    }
#else
                    catch (Exception)
                    {
                        // Make sure we throw the cancellation token if it was triggered first
                        token.ThrowIfCancellationRequested();
                        return string.Empty;
                    }
#endif
                    token.ThrowIfCancellationRequested();

                    strPageText = strPageText.CleanStylisticLigatures().NormalizeWhiteSpace()
                                             .NormalizeLineEndings().CleanOfXmlInvalidUnicodeChars();
                    token.ThrowIfCancellationRequested();

                    // don't trust it to be correct, trim all whitespace and remove empty strings before we even start
                    lstStringFromPdf.AddRange(strPageText
                        .SplitNoAlloc(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase)
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Select(x => x.Trim()));

                    for (int i = intProcessedStrings; i < lstStringFromPdf.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        // failsafe for languages that don't have case distinction (chinese, japanese, etc)
                        // there not much to be done for those languages, so stop after 10 continuous lines of uppercase text after our title
                        if (intExtraAllCapsInfo > 10)
                            break;

                        string strCurrentLine = lstStringFromPdf[i];
                        token.ThrowIfCancellationRequested();
                        // we still haven't found anything
                        if (intTitleIndex == -1)
                        {
                            int intTextToSearchLength = strTextToSearch.Length;
                            int intTitleExtraLines = 0;
                            if (strCurrentLine.Length < intTextToSearchLength)
                            {
                                // if the line is smaller first check if it contains the start of the text, before parsing the rest
                                if (strTextToSearch.StartsWith(strCurrentLine, StringComparison.OrdinalIgnoreCase))
                                {
                                    token.ThrowIfCancellationRequested();
                                    // now just add more lines to it until it is enough
                                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdCurrentLine))
                                    {
                                        sbdCurrentLine.Append(strCurrentLine);
                                        while (sbdCurrentLine.Length < intTextToSearchLength
                                               && i + intTitleExtraLines + 1 < lstStringFromPdf.Count)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            intTitleExtraLines++;
                                            // add the content plus a space
                                            sbdCurrentLine.Append(' ').Append(lstStringFromPdf[i + intTitleExtraLines]);
                                        }

                                        strCurrentLine = sbdCurrentLine.ToString();
                                    }
                                }
                                else
                                {
                                    // just go to the next line
                                    continue;
                                }
                            }

                            // now either we have enough text to search or the page doesn't have anymore stuff and must give up
                            if (strCurrentLine.Length < intTextToSearchLength)
                                break;

                            if (strCurrentLine.StartsWith(strTextToSearch, StringComparison.OrdinalIgnoreCase))
                            {
                                // WE FOUND SOMETHING! lets check what kind block we have
                                // if it is bigger it must have a ':' after the name otherwise it is probably the wrong stuff
                                if (strCurrentLine.Length > intTextToSearchLength)
                                {
                                    if (strCurrentLine[intTextToSearchLength] == ':')
                                    {
                                        intTitleIndex = i;
                                        blnTitleWithColon = true;
                                    }
                                }
                                else // if it is not bigger it is the same length
                                {
                                    // this must be an upper case title
                                    if (strCurrentLine.IsAllLettersUpperCase())
                                    {
                                        intTitleIndex = i;
                                        blnTitleWithColon = false;
                                    }
                                }

                                // if we found the tile lets finish some things before finding the text block
                                if (intTitleIndex != -1 && intTitleExtraLines > 0)
                                {
                                    // if we had to concatenate stuff lets fix the list of strings before continuing
                                    lstStringFromPdf[i] = strCurrentLine;
                                    lstStringFromPdf.RemoveRange(i + 1, intTitleExtraLines);
                                }
                            }
                        }
                        else // we already found our title, just go to the end of the block
                        {
                            // it is something in all caps we need to verify what it is
                            if (strCurrentLine.IsAllLettersUpperCase())
                            {
                                token.ThrowIfCancellationRequested();
                                // if it is header or footer information just remove it
                                // do we also include lines with just numbers as probably page numbers??
                                if (strCurrentLine.All(char.IsDigit) || strCurrentLine.ContainsAny(">>", "<<"))
                                {
                                    lstStringFromPdf.RemoveAt(i);
                                    // rewind and go again
                                    i--;
                                    continue;
                                }

                                // if it is a line in all caps following the all caps title just skip it
                                if (!blnTitleWithColon && i == intTitleIndex + intExtraAllCapsInfo + 1)
                                {
                                    intExtraAllCapsInfo++;
                                    continue;
                                }

                                // if we are here it is the end of the block we found our end, mark it and be done
                                intBlockEndIndex = i;
                                break;
                            }

                            // if it is a title with colon we stop in the next line that has a colon
                            // this is not perfect, if we had bold information we could do more about that
                            if (blnTitleWithColon && strCurrentLine.Contains(':'))
                            {
                                intBlockEndIndex = i;
                                break;
                            }
                        }
                    }

                    // we scanned the first page and found nothing, just give up
                    if (intTitleIndex == -1)
                        return string.Empty;
                    // already have our end, quit searching here
                    if (intBlockEndIndex != -1)
                        break;
                }

                return string.Empty;
            }

            token.ThrowIfCancellationRequested();

            // we have our textblock, lets format it and be done with it
            if (string.IsNullOrEmpty(strReturn) && intBlockEndIndex != -1)
            {
                token.ThrowIfCancellationRequested();
                string[] strArray = lstStringFromPdf.ToArray();
                token.ThrowIfCancellationRequested();
                // if it is a "paragraph title" just concatenate everything
                if (blnTitleWithColon)
                    return string.Join(" ", strArray, intTitleIndex, intBlockEndIndex - intTitleIndex);
                token.ThrowIfCancellationRequested();
                // add the title
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdResultContent))
                {
                    token.ThrowIfCancellationRequested();
                    sbdResultContent.AppendLine(strArray[intTitleIndex]);
                    // if we have extra info add it keeping the line breaks
                    if (intExtraAllCapsInfo > 0)
                        sbdResultContent
                            .AppendJoin(Environment.NewLine, strArray, intTitleIndex + 1, intExtraAllCapsInfo)
                            .AppendLine();
                    token.ThrowIfCancellationRequested();
                    int intContentStartIndex = intTitleIndex + intExtraAllCapsInfo + 1;
                    // this is the best we can do for now, it will still mangle spell blocks a bit
                    for (int i = intContentStartIndex; i < intBlockEndIndex; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        string strContentString = strArray[i];
                        if (strContentString.Length > 0)
                        {
                            char chrLastChar = strContentString[strContentString.Length - 1];
                            switch (chrLastChar)
                            {
                                case '-':
                                    sbdResultContent.Append(strContentString, 0, strContentString.Length - 1);
                                    break;
                                // Line ending with a sentence-ending punctuation = line is end of paragraph.
                                // Not fantastic, has plenty of false positives, but simple text extraction strategy cannot
                                // record when a new line starts with a slight indent compared to the previous line (it's a
                                // graphical indent in PDFs, not an actual tab character).
                                case '.':
                                case '?':
                                case '!':
                                case ':':
                                case '。':
                                case '？':
                                case '！':
                                case '：':
                                case '…':
                                    sbdResultContent.AppendLine(strContentString);
                                    break;

                                default:
                                    sbdResultContent.Append(strContentString).Append(' ');
                                    break;
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    strReturn = sbdResultContent.ToTrimmedString();
                }
            }

            return strReturn;
        }

        #endregion PDF Functions

        #region Timescale

        public enum Timescale
        {
            Instant = 0,
            Seconds = 1,
            CombatTurns = 2,
            Minutes = 3,
            Hours = 4,
            Days = 5
        }

        /// <summary>
        /// Convert a string to a Timescale.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static Timescale ConvertStringToTimescale(string strValue)
        {
            switch (strValue.ToUpperInvariant())
            {
                case "INSTANT":
                case "IMMEDIATE":
                    return Timescale.Instant;

                case "SECOND":
                case "SECONDS":
                    return Timescale.Seconds;

                case "COMBATTURN":
                case "COMBATTURNS":
                    return Timescale.CombatTurns;

                case "MINUTE":
                case "MINUTES":
                    return Timescale.Minutes;

                case "HOUR":
                case "HOURS":
                    return Timescale.Hours;

                case "DAY":
                case "DAYS":
                    return Timescale.Days;

                default:
                    return Timescale.Instant;
            }
        }

        /// <summary>
        /// Convert a string to a Timescale.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        /// <param name="blnSingle">Whether to return multiple of the timescale (Hour vs Hours)</param>
        /// <param name="strLanguage">Language to use. If left empty, will use current program language.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static string GetTimescaleString(Timescale strValue, bool blnSingle, string strLanguage = "", CancellationToken token = default)
        {
            switch (strValue)
            {
                case Timescale.Seconds when blnSingle:
                    return LanguageManager.GetString("String_Second", strLanguage, token: token);

                case Timescale.Seconds:
                    return LanguageManager.GetString("String_Seconds", strLanguage, token: token);

                case Timescale.CombatTurns when blnSingle:
                    return LanguageManager.GetString("String_CombatTurn", strLanguage, token: token);

                case Timescale.CombatTurns:
                    return LanguageManager.GetString("String_CombatTurns", strLanguage, token: token);

                case Timescale.Minutes when blnSingle:
                    return LanguageManager.GetString("String_Minute", strLanguage, token: token);

                case Timescale.Minutes:
                    return LanguageManager.GetString("String_Minutes", strLanguage, token: token);

                case Timescale.Hours when blnSingle:
                    return LanguageManager.GetString("String_Hour", strLanguage, token: token);

                case Timescale.Hours:
                    return LanguageManager.GetString("String_Hours", strLanguage, token: token);

                case Timescale.Days when blnSingle:
                    return LanguageManager.GetString("String_Day", strLanguage, token: token);

                case Timescale.Days:
                    return LanguageManager.GetString("String_Days", strLanguage, token: token);

                case Timescale.Instant:
                default:
                    return LanguageManager.GetString("String_Immediate", strLanguage, token: token);
            }
        }

        /// <summary>
        /// Convert a string to a Timescale.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        /// <param name="blnSingle">Whether to return multiple of the timescale (Hour vs Hours)</param>
        /// <param name="strLanguage">Language to use. If left empty, will use current program language.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<string> GetTimescaleStringAsync(Timescale strValue, bool blnSingle, string strLanguage = "", CancellationToken token = default)
        {
            switch (strValue)
            {
                case Timescale.Seconds when blnSingle:
                    return LanguageManager.GetStringAsync("String_Second", strLanguage, token: token);

                case Timescale.Seconds:
                    return LanguageManager.GetStringAsync("String_Seconds", strLanguage, token: token);

                case Timescale.CombatTurns when blnSingle:
                    return LanguageManager.GetStringAsync("String_CombatTurn", strLanguage, token: token);

                case Timescale.CombatTurns:
                    return LanguageManager.GetStringAsync("String_CombatTurns", strLanguage, token: token);

                case Timescale.Minutes when blnSingle:
                    return LanguageManager.GetStringAsync("String_Minute", strLanguage, token: token);

                case Timescale.Minutes:
                    return LanguageManager.GetStringAsync("String_Minutes", strLanguage, token: token);

                case Timescale.Hours when blnSingle:
                    return LanguageManager.GetStringAsync("String_Hour", strLanguage, token: token);

                case Timescale.Hours:
                    return LanguageManager.GetStringAsync("String_Hours", strLanguage, token: token);

                case Timescale.Days when blnSingle:
                    return LanguageManager.GetStringAsync("String_Day", strLanguage, token: token);

                case Timescale.Days:
                    return LanguageManager.GetStringAsync("String_Days", strLanguage, token: token);

                case Timescale.Instant:
                default:
                    return LanguageManager.GetStringAsync("String_Immediate", strLanguage, token: token);
            }
        }

        #endregion Timescale
    }
}
