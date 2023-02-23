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

using System.Reflection;
using System.Text.RegularExpressions;

namespace RegexLibCompiler
{
    /// <summary>
    /// This console application is a tiny program that compiles all statically defined Regular Expressions used by Chummer into a .dll file from which Chummer can
    /// fetch and run them with both minimal startup time and minimal runtime.
    /// </summary>
    internal static class Program
    {
        static void Main()
        {
            RegexCompilationInfo[] argxPatterns =
            {
                new RegexCompilationInfo(@"^[0-9]*[0-9]*x",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "AmmoCapacityFirstPattern",
                                         "ExternalUtils.RegularExpressions.Weapons",
                                         true),
                new RegexCompilationInfo(@"(?<=\))(x[0-9]*[0-9]*$)*",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "AmmoCapacitySecondPattern",
                                         "ExternalUtils.RegularExpressions.Weapons",
                                         true),
                new RegexCompilationInfo(@"\\{([0-9]+)\}",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "FirstReplacePattern",
                                         "ExternalUtils.RegularExpressions.TranslateExceptionTelemetryProcessor",
                                         true),
                new RegexCompilationInfo(@"{([0-9]+)}",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "SecondReplacePattern",
                                         "ExternalUtils.RegularExpressions.TranslateExceptionTelemetryProcessor",
                                         true),
                new RegexCompilationInfo(@"FixedValues\(([^)]*)\)",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "FixedValuesPattern",
                                         "ExternalUtils.RegularExpressions.ParameterAttribute",
                                         true),
                new RegexCompilationInfo(@"\[([^\]]*)\]",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "SquareBracketsPattern",
                                         "ExternalUtils.RegularExpressions.ParameterAttribute",
                                         true),
                new RegexCompilationInfo(@"/<\/?[a-z][\s\S]*>/i",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "HtmlTagsPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo(@"\r\n|\n\r|\n|\r",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "LineEndingsPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo(@"\\r\\n|\\n\\r|\\n|\\r",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "EscapedLineEndingsPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo(@"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "RtfStripperPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo(@"[\u0000-\u0008\u000B\u000C\u000E-\u001F]",
                                         RegexOptions.IgnoreCase | RegexOptions.Multiline
                                                                 | RegexOptions.CultureInvariant,
                                         "InvalidUnicodeCharsPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo(@"^(\[([a-z])+\.xml\])",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "ExtraFileSpecifierPattern",
                                         "ExternalUtils.RegularExpressions",
                                         true),
                new RegexCompilationInfo("<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "MainMugshotReplacePattern",
                                         "ExternalUtils.RegularExpressions.ExportCharacter",
                                         true),
                new RegexCompilationInfo("<stringbase64>[^\\s\\S]*</stringbase64>",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "StringBase64ReplacePattern",
                                         "ExternalUtils.RegularExpressions.ExportCharacter",
                                         true),
                new RegexCompilationInfo("base64\": \"[^\\\"]*\",",
                                         RegexOptions.IgnoreCase | RegexOptions.Singleline
                                                                 | RegexOptions.CultureInvariant,
                                         "Base64ReplacePattern",
                                         "ExternalUtils.RegularExpressions.ExportCharacter",
                                         true)
            };
            AssemblyName objAssemblyName = new AssemblyName("RegexLib, Version=1.0.0.1001, Culture=neutral, PublicKeyToken=null");
            Regex.CompileToAssembly(argxPatterns, objAssemblyName);
        }
    }
}
