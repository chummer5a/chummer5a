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
using System.Globalization;
using System.Text;

namespace Chummer
{
    /// <summary>Represents a value type of the version number of an assembly, operating system, or the common language runtime. This struct cannot be inherited.</summary>
    [Serializable]
    public readonly struct ValueVersion : IComparable, IComparable<Version>, IEquatable<Version>, IComparable<ValueVersion>, IEquatable<ValueVersion>
    {
        private readonly int _Major;
        private readonly int _Minor;
        private readonly int _Build;
        private readonly int _Revision;

        /// <summary>Initializes a new ValueVersion struct with the specified major, minor, build, and revision numbers.</summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build number.</param>
        /// <param name="revision">The revision number.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="major" />, <paramref name="minor" />, <paramref name="build" />, or <paramref name="revision" /> is less than zero.</exception>
        public ValueVersion(int major, int minor, int build, int revision)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor));
            if (build < 0)
                throw new ArgumentOutOfRangeException(nameof(build));
            if (revision < 0)
                throw new ArgumentOutOfRangeException(nameof(revision));
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = revision;
        }

        /// <summary>Initializes a new ValueVersion struct using the specified major, minor, and build values.</summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build number.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="major" />, <paramref name="minor" />, or <paramref name="build" /> is less than zero.</exception>
        public ValueVersion(int major, int minor, int build)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor));
            if (build < 0)
                throw new ArgumentOutOfRangeException(nameof(build));
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = -1;
        }

        /// <summary>Initializes a new ValueVersion struct using the specified major and minor values.</summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="major" /> or <paramref name="minor" /> is less than zero.</exception>
        public ValueVersion(int major, int minor)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor));
            _Major = major;
            _Minor = minor;
            _Build = -1;
            _Revision = -1;
        }

        public ValueVersion(int major = 0)
        {
            _Major = major;
            _Minor = 0;
            _Build = -1;
            _Revision = -1;
        }

        /// <summary>Initializes a new ValueVersion struct using the specified string.</summary>
        /// <param name="version">A string containing the major, minor, build, and revision numbers, where each number is delimited with a period character ('.').</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="version" /> has fewer than two components or more than four components.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="version" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">A major, minor, build, or revision component is less than zero.</exception>
        /// <exception cref="T:System.FormatException">At least one component of <paramref name="version" /> does not parse to an integer.</exception>
        /// <exception cref="T:System.OverflowException">At least one component of <paramref name="version" /> represents a number greater than <see cref="F:System.Int32.MaxValue" />.</exception>
        public ValueVersion(string version)
        {
            ValueVersion version1 = Parse(version);
            _Major = version1.Major;
            _Minor = version1.Minor;
            _Build = version1.Build;
            _Revision = version1.Revision;
        }

        public ValueVersion(Version version)
        {
            _Major = version.Major;
            _Minor = version.Minor;
            _Build = version.Build;
            _Revision = version.Revision;
        }

        /// <summary>Gets the value of the major component of the version number for the current ValueVersion struct.</summary>
        /// <returns>The major version number.</returns>
        public int Major => _Major;

        /// <summary>Gets the value of the minor component of the version number for the current ValueVersion struct.</summary>
        /// <returns>The minor version number.</returns>
        public int Minor => _Minor;

        /// <summary>Gets the value of the build component of the version number for the current ValueVersion struct.</summary>
        /// <returns>The build number, or -1 if the build number is undefined.</returns>
        public int Build => _Build;

        /// <summary>Gets the value of the revision component of the version number for the current ValueVersion struct.</summary>
        /// <returns>The revision number, or -1 if the revision number is undefined.</returns>
        public int Revision => _Revision;

        /// <summary>Gets the high 16 bits of the revision number.</summary>
        /// <returns>A 16-bit signed integer.</returns>
        public short MajorRevision => (short)(_Revision >> 16);

        /// <summary>Gets the low 16 bits of the revision number.</summary>
        /// <returns>A 16-bit signed integer.</returns>
        public short MinorRevision => (short)(_Revision & ushort.MaxValue);

        /// <summary>Returns a new <see cref="T:System.Version" /> object identical to the current ValueVersion struct.</summary>
        /// <returns>A new <see cref="T:System.Version" /> object whose values are identical to the current ValueVersion struct's.</returns>
        public Version AsVersion()
        {
            if (Build >= 0)
            {
                return Revision >= 0 ? new Version(Major, Minor, Build, Revision) : new Version(Major, Minor, Build);
            }
            return new Version(Major, Minor);
        }

        /// <summary>Compares the current ValueVersion struct to a specified object and returns an indication of their relative values.</summary>
        /// <param name="version">An object to compare, or <see langword="null" />.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="version" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="version" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="version" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="version" /> is <see langword="null" />.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="version" /> is not of type <see cref="T:System.Version" />.</exception>
        public int CompareTo(object version)
        {
            if (version == null)
                return 1;
            if (version is ValueVersion version1)
                return CompareTo(version1);
            if (version is Version version2)
                return CompareTo(version2);
            throw new ArgumentException("Argument is not a version of value version.", nameof(version));
        }

        /// <summary>Compares the current ValueVersion struct to a specified ValueVersion struct and returns an indication of their relative values.</summary>
        /// <param name="value">A ValueVersion struct to compare to the current ValueVersion struct.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="value" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="value" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="value" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="value" /> is <see langword="null" />.</returns>
        public int CompareTo(ValueVersion value)
        {
            if (_Major != value.Major)
                return _Major > value.Major ? 1 : -1;
            if (_Minor != value.Minor)
                return _Minor > value.Minor ? 1 : -1;
            if (_Build != value.Build)
                return _Build > value.Build ? 1 : -1;
            if (_Revision == value.Revision)
                return 0;
            return _Revision > value.Revision ? 1 : -1;
        }

        /// <summary>Compares the current ValueVersion struct to a specified <see cref="T:System.Version" /> object and returns an indication of their relative values.</summary>
        /// <param name="value">A ValueVersion struct to compare to the current <see cref="T:System.Version" /> object, or <see langword="null" />.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="value" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="value" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="value" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="value" /> is <see langword="null" />.</returns>
        public int CompareTo(Version value)
        {
            if (value == null)
                return 1;
            if (_Major != value.Major)
                return _Major > value.Major ? 1 : -1;
            if (_Minor != value.Minor)
                return _Minor > value.Minor ? 1 : -1;
            if (_Build != value.Build)
                return _Build > value.Build ? 1 : -1;
            if (_Revision == value.Revision)
                return 0;
            return _Revision > value.Revision ? 1 : -1;
        }

        /// <summary>Returns a value indicating whether the current ValueVersion struct is equal to a specified object.</summary>
        /// <param name="obj">An object to compare with the current ValueVersion struct, or <see langword="null" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.Version" /> object, and every component of the current ValueVersion struct matches the corresponding component of <paramref name="obj" />; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Version);
        }

        /// <summary>Returns a value indicating whether the current ValueVersion struct and a specified ValueVersion struct represent the same value.</summary>
        /// <param name="obj">A ValueVersion struct to compare to the current ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if every component of the current ValueVersion struct matches the corresponding component of the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(ValueVersion obj)
        {
            return _Major == obj.Major && _Minor == obj.Minor && _Build == obj.Build && _Revision == obj.Revision;
        }

        /// <summary>Returns a value indicating whether the current ValueVersion struct and a specified <see cref="T:System.Version" /> object represent the same value.</summary>
        /// <param name="obj">A <see cref="T:System.Version" /> object to compare to the current ValueVersion struct, or <see langword="null" />.</param>
        /// <returns>
        /// <see langword="true" /> if every component of the current ValueVersion struct matches the corresponding component of the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Version obj)
        {
            return obj != null && _Major == obj.Major && _Minor == obj.Minor && _Build == obj.Build && _Revision == obj.Revision;
        }

        /// <summary>Returns a hash code for the current ValueVersion struct.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return 0 | (_Major & 0xF) << 28 | (_Minor & 0xFF) << 20 | (_Build & 0xFF) << 12 | _Revision & 0xFFF;
        }

        /// <summary>Converts the value of the current ValueVersion struct to its equivalent <see cref="T:System.String" /> representation.</summary>
        /// <returns>The <see cref="T:System.String" /> representation of the values of the major, minor, build, and revision components of the current ValueVersion struct, as depicted in the following format. Each component is separated by a period character ('.'). Square brackets ('[' and ']') indicate a component that will not appear in the return value if the component is not defined:
        /// major.minor[.build[.revision]]
        /// For example, if you create a ValueVersion struct using the constructor ValueVersion(1,1), the returned string is "1.1". If you create a ValueVersion struct using the constructor ValueVersion(1,3,4,2), the returned string is "1.3.4.2".</returns>
        public override string ToString()
        {
            if (_Build == -1)
                return ToString(2);
            return _Revision == -1 ? ToString(3) : ToString(4);
        }

        /// <summary>Converts the value of the current ValueVersion struct to its equivalent <see cref="T:System.String" /> representation. A specified count indicates the number of components to return.</summary>
        /// <param name="fieldCount">The number of components to return. The <paramref name="fieldCount" /> ranges from 0 to 4.</param>
        /// <returns>The <see cref="T:System.String" /> representation of the values of the major, minor, build, and revision components of the current ValueVersion struct, each separated by a period character ('.'). The <paramref name="fieldCount" /> parameter determines how many components are returned.
        ///  fieldCount
        /// 
        ///  Return Value
        /// 
        ///  0
        /// 
        ///  An empty string ("").
        /// 
        ///  1
        /// 
        ///  major
        /// 
        ///  2
        /// 
        ///  major.minor
        /// 
        ///  3
        /// 
        ///  major.minor.build
        /// 
        ///  4
        /// 
        ///  major.minor.build.revision
        /// 
        /// 
        /// 
        /// For example, if you create ValueVersion struct using the constructor ValueVersion(1,3,5), ToString(2) returns "1.3" and ToString(4) throws an exception.</returns>
        /// <exception cref="T:System.ArgumentException">
        ///         <paramref name="fieldCount" /> is less than 0, or more than 4.
        /// -or-
        /// <paramref name="fieldCount" /> is more than the number of components defined in the current ValueVersion struct.</exception>
        public string ToString(int fieldCount)
        {
            switch (fieldCount)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return _Major.ToString();
                case 2:
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdReturn))
                    {
                        AppendPositiveNumber(_Major, sbdReturn);
                        sbdReturn.Append('.');
                        AppendPositiveNumber(_Minor, sbdReturn);
                        return sbdReturn.ToString();
                    }
                default:
                    if (_Build == -1)
                        throw new ArgumentOutOfRangeException(nameof(fieldCount));
                    if (fieldCount == 3)
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdReturn))
                        {
                            AppendPositiveNumber(_Major, sbdReturn);
                            sbdReturn.Append('.');
                            AppendPositiveNumber(_Minor, sbdReturn);
                            sbdReturn.Append('.');
                            AppendPositiveNumber(_Build, sbdReturn);
                            return sbdReturn.ToString();
                        }
                    }
                    if (_Revision == -1)
                        throw new ArgumentOutOfRangeException(nameof(fieldCount));
                    if (fieldCount == 4)
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdReturn))
                        {
                            AppendPositiveNumber(_Major, sbdReturn);
                            sbdReturn.Append('.');
                            AppendPositiveNumber(_Minor, sbdReturn);
                            sbdReturn.Append('.');
                            AppendPositiveNumber(_Build, sbdReturn);
                            sbdReturn.Append('.');
                            AppendPositiveNumber(_Revision, sbdReturn);
                            return sbdReturn.ToString();
                        }
                    }
                    throw new ArgumentOutOfRangeException(nameof(fieldCount));
            }

            void AppendPositiveNumber(int num, StringBuilder sb)
            {
                int length = sb.Length;
                do
                {
                    num = num.DivRem(10, out int num1);
                    sb.Insert(length, (char)(48 + num1));
                }
                while (num > 0);
            }
        }

        /// <summary>Converts the string representation of a version number to an equivalent ValueVersion struct.</summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <returns>An object that is equivalent to the version number specified in the <paramref name="input" /> parameter.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="input" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="input" /> has fewer than two or more than four version components.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">At least one component in <paramref name="input" /> is less than zero.</exception>
        /// <exception cref="T:System.FormatException">At least one component in <paramref name="input" /> is not an integer.</exception>
        /// <exception cref="T:System.OverflowException">At least one component in <paramref name="input" /> represents a number that is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static ValueVersion Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            ValueVersionResult result = new ValueVersionResult();
            result.Init(nameof(input), true);
            return TryParseVersion(input, ref result) ? result.m_parsedValueVersion : throw result.GetValueVersionParseException();
        }

        /// <summary>Tries to convert the string representation of a version number to an equivalent ValueVersion struct, and returns a value that indicates whether the conversion succeeded.</summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="T:System.Version" /> equivalent of the number that is contained in <paramref name="input" />, if the conversion succeeded. If <paramref name="input" /> is <see langword="null" />, <see cref="F:System.String.Empty" />, or if the conversion fails, <paramref name="result" /> is <see langword="null" /> when the method returns.</param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="input" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(string input, out ValueVersion result)
        {
            ValueVersionResult result1 = new ValueVersionResult();
            result1.Init(nameof(input), false);
            bool version = TryParseVersion(input, ref result1);
            result = result1.m_parsedValueVersion;
            return version;
        }

        private static bool TryParseVersion(string version, ref ValueVersionResult result)
        {
            if (version == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNullException);
                return false;
            }
            if (int.TryParse(version, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedComponent0) && parsedComponent0 >= 0)
            {
                result.m_parsedValueVersion = new ValueVersion(parsedComponent0);
                return true;
            }
            string[] strArray = version.SplitToPooledArray(out int length, '.');
            try
            {
                if (length < 2 || length > 4)
                {
                    result.SetFailure(ParseFailureKind.ArgumentException);
                    return false;
                }

                if (!TryParseComponent(strArray[0], nameof(version), ref result, out int parsedComponent1)
                    || !TryParseComponent(strArray[1], nameof(version), ref result, out int parsedComponent2))
                {
                    return false;
                }

                int num = length - 2;
                if (num > 0)
                {
                    if (!TryParseComponent(strArray[2], "build", ref result, out int parsedComponent3))
                        return false;
                    if (num > 1)
                    {
                        if (!TryParseComponent(strArray[3], "revision", ref result, out int parsedComponent4))
                            return false;
                        result.m_parsedValueVersion = new ValueVersion(parsedComponent1, parsedComponent2, parsedComponent3, parsedComponent4);
                    }
                    else
                        result.m_parsedValueVersion = new ValueVersion(parsedComponent1, parsedComponent2, parsedComponent3);
                }
                else
                    result.m_parsedValueVersion = new ValueVersion(parsedComponent1, parsedComponent2);
                return true;
            }
            finally
            {
                ArrayPool<string>.Shared.Return(strArray);
            }
        }

        private static bool TryParseComponent(
          string component,
          string componentName,
          ref ValueVersionResult result,
          out int parsedComponent)
        {
            if (!int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent))
            {
                result.SetFailure(ParseFailureKind.FormatException, component);
                return false;
            }
            if (parsedComponent >= 0)
                return true;
            result.SetFailure(ParseFailureKind.ArgumentOutOfRangeException, componentName);
            return false;
        }

        /// <summary>Determines whether a specified <see cref="T:System.Version" /> object and ValueVersion struct are equal.</summary>
        /// <param name="v1">The <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> equals <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(Version v1, ValueVersion v2)
        {
            return v1 != null && v2.Equals(v1);
        }

        /// <summary>Determines whether a specified <see cref="T:System.Version" /> object and ValueVersion struct are not equal.</summary>
        /// <param name="v1">The <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> does not equal <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(Version v1, ValueVersion v2) => !(v1 == v2);

        /// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is less than the second specified ValueVersion struct.</summary>
        /// <param name="v1">The <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <(Version v1, ValueVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            return v2.CompareTo(v1) >= 0;
        }

        /// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is less than or equal to the second ValueVersion struct.</summary>
        /// <param name="v1">The <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <=(Version v1, ValueVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            return v2.CompareTo(v1) > 0;
        }

        /// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is greater than the second specified ValueVersion struct.</summary>
        /// <param name="v1">The <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >(Version v1, ValueVersion v2) => v2 < v1;

        /// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is greater than or equal to the second specified ValueVersion struct.</summary>
        /// <param name="v1">The first <see cref="T:System.Version" /> object.</param>
        /// <param name="v2">The ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >=(Version v1, ValueVersion v2) => v2 <= v1;

        /// <summary>Determines whether a specified ValueVersion struct and <see cref="T:System.Version" /> object are equal.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> equals <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(ValueVersion v1, Version v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>Determines whether a specified ValueVersion struct and <see cref="T:System.Version" /> object are not equal.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> does not equal <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(ValueVersion v1, Version v2) => !(v1 == v2);

        /// <summary>Determines whether the first specified ValueVersion struct is less than the second specified <see cref="T:System.Version" /> object.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <(ValueVersion v1, Version v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        /// <summary>Determines whether the first specified ValueVersion struct is less than or equal to the second <see cref="T:System.Version" /> object.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <=(ValueVersion v1, Version v2)
        {
            return v1.CompareTo(v2) <= 0;
        }

        /// <summary>Determines whether the first specified ValueVersion struct is greater than the second specified <see cref="T:System.Version" /> object.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >(ValueVersion v1, Version v2) => v2 < v1;

        /// <summary>Determines whether the first specified ValueVersion struct is greater than or equal to the second specified <see cref="T:System.Version" /> object.</summary>
        /// <param name="v1">The ValueVersion struct.</param>
        /// <param name="v2">The <see cref="T:System.Version" /> object.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >=(ValueVersion v1, Version v2) => v2 <= v1;

        /// <summary>Determines whether two specified ValueVersion structs are equal.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> equals <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(ValueVersion v1, ValueVersion v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>Determines whether two specified ValueVersion structs are not equal.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> does not equal <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(ValueVersion v1, ValueVersion v2) => !(v1 == v2);

        /// <summary>Determines whether the first specified ValueVersion struct is less than the second specified ValueVersion struct.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <(ValueVersion v1, ValueVersion v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        /// <summary>Determines whether the first specified ValueVersion struct is less than or equal to the second ValueVersion struct.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is less than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="v1" /> is <see langword="null" />.</exception>
        public static bool operator <=(ValueVersion v1, ValueVersion v2)
        {
            return v1.CompareTo(v2) <= 0;
        }

        /// <summary>Determines whether the first specified ValueVersion struct is greater than the second specified ValueVersion struct.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >(ValueVersion v1, ValueVersion v2) => v2 < v1;

        /// <summary>Determines whether the first specified ValueVersion struct is greater than or equal to the second specified ValueVersion struct.</summary>
        /// <param name="v1">The first ValueVersion struct.</param>
        /// <param name="v2">The second ValueVersion struct.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="v1" /> is greater than or equal to <paramref name="v2" />; otherwise, <see langword="false" />.</returns>
        public static bool operator >=(ValueVersion v1, ValueVersion v2) => v2 <= v1;

        internal enum ParseFailureKind
        {
            ArgumentNullException,
            ArgumentException,
            ArgumentOutOfRangeException,
            FormatException,
        }

        internal struct ValueVersionResult
        {
            internal ValueVersion m_parsedValueVersion;
            private ParseFailureKind m_failure;
            private string m_exceptionArgument;
            private string m_argumentName;
            private bool m_canThrow;

            internal void Init(string argumentName, bool canThrow)
            {
                m_canThrow = canThrow;
                m_argumentName = argumentName;
            }

            internal void SetFailure(ParseFailureKind failure)
            {
                SetFailure(failure, string.Empty);
            }

            internal void SetFailure(ParseFailureKind failure, string argument)
            {
                m_failure = failure;
                m_exceptionArgument = argument;
                if (m_canThrow)
                    throw GetValueVersionParseException();
            }

            internal Exception GetValueVersionParseException()
            {
                switch (m_failure)
                {
                    case ParseFailureKind.ArgumentNullException:
                        return new ArgumentNullException(m_argumentName);
                    case ParseFailureKind.ArgumentException:
                        return new ArgumentException();
                    case ParseFailureKind.ArgumentOutOfRangeException:
                        return new ArgumentOutOfRangeException(m_exceptionArgument);
                    case ParseFailureKind.FormatException:
                        try
                        {
                            _ = int.Parse(m_exceptionArgument, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return ex;
                        }
                        catch (OverflowException ex)
                        {
                            return ex;
                        }
                        return new FormatException();
                    default:
                        return new ArgumentException();
                }
            }
        }
    }
}
