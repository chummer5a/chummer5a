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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Chummer
{
    /// <summary>Represents a value type of the version number of an assembly, operating system, or the common language runtime. This struct cannot be inherited.</summary>
    [Serializable]
    public readonly struct ValueVersion : IComparable, IComparable<Version>, IEquatable<Version>, IComparable<ValueVersion>, IEquatable<ValueVersion>, ISpanFormattable, IUtf8SpanFormattable, IUtf8SpanParsable<ValueVersion>
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
            ArgumentOutOfRangeException.ThrowIfNegative(major, nameof(major));
            ArgumentOutOfRangeException.ThrowIfNegative(minor, nameof(minor));
            ArgumentOutOfRangeException.ThrowIfNegative(build, nameof(build));
            ArgumentOutOfRangeException.ThrowIfNegative(revision, nameof(revision));
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
            ArgumentOutOfRangeException.ThrowIfNegative(major, nameof(major));
            ArgumentOutOfRangeException.ThrowIfNegative(minor, nameof(minor));
            ArgumentOutOfRangeException.ThrowIfNegative(build, nameof(build));
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
            ArgumentOutOfRangeException.ThrowIfNegative(major, nameof(major));
            ArgumentOutOfRangeException.ThrowIfNegative(minor, nameof(minor));
            _Major = major;
            _Minor = minor;
            _Build = -1;
            _Revision = -1;
        }

        public ValueVersion(int major = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(major, nameof(major));
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

        public ValueVersion(ReadOnlySpan<byte> version)
        {
            ValueVersion version1 = Parse(version);
            _Major = version1.Major;
            _Minor = version1.Minor;
            _Build = version1.Build;
            _Revision = version1.Revision;
        }

        public ValueVersion(ReadOnlySpan<char> version)
        {
            ValueVersion version1 = Parse(version);
            _Major = version1.Major;
            _Minor = version1.Minor;
            _Build = version1.Build;
            _Revision = version1.Revision;
        }

        /// <summary>Initializes a new ValueVersion struct using the specified Version as a reference.</summary>
        /// <param name="version">A Version object from which to copy major, minor, build, and revision values.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="version" /> is <see langword="null" />.</exception>
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
        /// <param name="obj">An object to compare, or <see langword="null" />.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="obj" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="obj" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="obj" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="obj" /> is <see langword="null" />.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="obj" /> is not of type <see cref="T:System.Version" />.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is ValueVersion version1)
                return CompareTo(version1);
            if (obj is Version version2)
                return CompareTo(version2);
            throw new ArgumentException("Argument is not a version of value version.", nameof(obj));
        }

        /// <summary>Compares the current ValueVersion struct to a specified ValueVersion struct and returns an indication of their relative values.</summary>
        /// <param name="obj">A ValueVersion struct to compare to the current ValueVersion struct.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="obj" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="obj" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="obj" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="obj" /> is <see langword="null" />.</returns>
        public int CompareTo(ValueVersion obj)
        {
            if (_Major != obj.Major)
                return _Major > obj.Major ? 1 : -1;
            if (_Minor != obj.Minor)
                return _Minor > obj.Minor ? 1 : -1;
            if (_Build != obj.Build)
                return _Build > obj.Build ? 1 : -1;
            if (_Revision == obj.Revision)
                return 0;
            return _Revision > obj.Revision ? 1 : -1;
        }

        /// <summary>Compares the current ValueVersion struct to a specified <see cref="T:System.Version" /> object and returns an indication of their relative values.</summary>
        /// <param name="obj">A ValueVersion struct to compare to the current <see cref="T:System.Version" /> object, or <see langword="null" />.</param>
        /// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///  Return value
        /// 
        ///  Meaning
        /// 
        ///  Less than zero
        /// 
        ///  The current ValueVersion struct is a version before <paramref name="obj" />.
        /// 
        ///  Zero
        /// 
        ///  The current ValueVersion struct is the same version as <paramref name="obj" />.
        /// 
        ///  Greater than zero
        /// 
        ///  The current ValueVersion struct is a version subsequent to <paramref name="obj" />.
        /// 
        /// -or-
        /// 
        /// <paramref name="obj" /> is <see langword="null" />.</returns>
        public int CompareTo(Version obj)
        {
            if (obj == null)
                return 1;
            if (_Major != obj.Major)
                return _Major > obj.Major ? 1 : -1;
            if (_Minor != obj.Minor)
                return _Minor > obj.Minor ? 1 : -1;
            if (_Build != obj.Build)
                return _Build > obj.Build ? 1 : -1;
            if (_Revision == obj.Revision)
                return 0;
            return _Revision > obj.Revision ? 1 : -1;
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
            return (_Major & 0xF) << 28 | (_Minor & 0xFF) << 20 | (_Build & 0xFF) << 12 | _Revision & 0xFFF;
        }

        /// <summary>Converts the value of the current ValueVersion struct to its equivalent <see cref="T:System.String" /> representation.</summary>
        /// <returns>The <see cref="T:System.String" /> representation of the values of the major, minor, build, and revision components of the current ValueVersion struct, as depicted in the following format. Each component is separated by a period character ('.'). Square brackets ('[' and ']') indicate a component that will not appear in the return value if the component is not defined:
        /// major.minor[.build[.revision]]
        /// For example, if you create a ValueVersion struct using the constructor ValueVersion(1,1), the returned string is "1.1". If you create a ValueVersion struct using the constructor ValueVersion(1,3,4,2), the returned string is "1.3.4.2".</returns>
        public override string ToString() =>
            ToString(DefaultFormatFieldCount);

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
            Span<char> dest = stackalloc char[(4 * 10) + 3]; // at most 4 Int32s and 3 periods
            bool success = TryFormat(dest, fieldCount, out int charsWritten);
            Debug.Assert(success);
            return dest.Slice(0, charsWritten).ToString();
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider) =>
            ToString();

        public bool TryFormat(Span<char> destination, out int charsWritten) =>
            TryFormatCore(destination, DefaultFormatFieldCount, out charsWritten);

        public bool TryFormat(Span<char> destination, int fieldCount, out int charsWritten) =>
            TryFormatCore(destination, fieldCount, out charsWritten);

        /// <summary>Tries to format this version instance into a span of bytes.</summary>
        /// <param name="utf8Destination">The span in which to write this instance's value formatted as a span of UTF-8 bytes.</param>
        /// <param name="bytesWritten">When this method returns, contains the number of bytes that were written in <paramref name="utf8Destination"/>.</param>
        /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten) =>
            TryFormatCore(utf8Destination, DefaultFormatFieldCount, out bytesWritten);

        /// <summary>Tries to format this version instance into a span of bytes.</summary>
        /// <param name="utf8Destination">The span in which to write this instance's value formatted as a span of UTF-8 bytes.</param>
        /// <param name="fieldCount">The number of components to return. This value ranges from 0 to 4.</param>
        /// <param name="bytesWritten">When this method returns, contains the number of bytes that were written in <paramref name="utf8Destination"/>.</param>
        /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
        public bool TryFormat(Span<byte> utf8Destination, int fieldCount, out int bytesWritten) =>
            TryFormatCore(utf8Destination, fieldCount, out bytesWritten);

        private bool TryFormatCore(Span<char> destination, int fieldCount, out int charsWritten)
        {
            switch ((uint)fieldCount)
            {
                case > 4:
                    ThrowArgumentException("4");
                    break;

                case >= 3 when _Build == -1:
                    ThrowArgumentException("2");
                    break;

                case 4 when _Revision == -1:
                    ThrowArgumentException("3");
                    break;

                    static void ThrowArgumentException(string failureUpperBound) =>
                        throw new ArgumentOutOfRangeException(nameof(fieldCount));
            }

            int totalCharsWritten = 0;

            for (int i = 0; i < fieldCount; i++)
            {
                if (i != 0)
                {
                    if (destination.IsEmpty)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    destination[0] = '.';
                    destination = destination.Slice(1);
                    totalCharsWritten++;
                }

                int value = i switch
                {
                    0 => _Major,
                    1 => _Minor,
                    2 => _Build,
                    _ => _Revision
                };

                int valueCharsWritten;
                bool formatted = ((uint)value).TryFormat(destination, out valueCharsWritten);

                if (!formatted)
                {
                    charsWritten = 0;
                    return false;
                }

                totalCharsWritten += valueCharsWritten;
                destination = destination.Slice(valueCharsWritten);
            }

            charsWritten = totalCharsWritten;
            return true;
        }

        private bool TryFormatCore(Span<byte> destination, int fieldCount, out int charsWritten)
        {
            switch ((uint)fieldCount)
            {
                case > 4:
                    ThrowArgumentException("4");
                    break;

                case >= 3 when _Build == -1:
                    ThrowArgumentException("2");
                    break;

                case 4 when _Revision == -1:
                    ThrowArgumentException("3");
                    break;

                    static void ThrowArgumentException(string failureUpperBound) =>
                        throw new ArgumentOutOfRangeException(nameof(fieldCount));
            }

            int totalCharsWritten = 0;

            for (int i = 0; i < fieldCount; i++)
            {
                if (i != 0)
                {
                    if (destination.IsEmpty)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    destination[0] = (byte)'.';
                    destination = destination.Slice(1);
                    totalCharsWritten++;
                }

                int value = i switch
                {
                    0 => _Major,
                    1 => _Minor,
                    2 => _Build,
                    _ => _Revision
                };

                int valueCharsWritten;
                bool formatted = ((uint)value).TryFormat(destination, out valueCharsWritten, default, CultureInfo.InvariantCulture);

                if (!formatted)
                {
                    charsWritten = 0;
                    return false;
                }

                totalCharsWritten += valueCharsWritten;
                destination = destination.Slice(valueCharsWritten);
            }

            charsWritten = totalCharsWritten;
            return true;
        }

        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider) =>
            // format and provider are ignored.
            TryFormatCore(destination, DefaultFormatFieldCount, out charsWritten);

        /// <inheritdoc cref="IUtf8SpanFormattable.TryFormat" />
        bool IUtf8SpanFormattable.TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider) =>
            // format and provider are ignored.
            TryFormatCore(utf8Destination, DefaultFormatFieldCount, out bytesWritten);

        private int DefaultFormatFieldCount =>
            _Build == -1 ? 2 :
            _Revision == -1 ? 3 :
            4;

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
            ArgumentNullException.ThrowIfNull(input, nameof(input));
            return ParseVersion(input.AsSpan(), throwOnFailure: true) ?? throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));
        }

        public static ValueVersion Parse(ReadOnlySpan<char> input) =>
            ParseVersion(input, throwOnFailure: true) ?? throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));

        /// <inheritdoc cref="IUtf8SpanParsable{TSelf}.Parse(ReadOnlySpan{byte}, IFormatProvider?)"/>
        static ValueVersion IUtf8SpanParsable<ValueVersion>.Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider)
        {
            ValueVersion? result = ParseVersion(utf8Text, throwOnFailure: false);
            // Required to throw FormatException for invalid input according to contract.
            if (result == null)
            {
                throw new ArgumentException("The specified version string does not conform to the required format", nameof(utf8Text));
            }
            return result.Value;
        }

        /// <summary>
        /// Converts the specified read-only span of UTF-8 characters that represents a version number to an equivalent Version object.
        /// </summary>
        /// <param name="utf8Text">A read-only span of UTF-8 characters that contains a version number to convert.</param>
        /// <returns>An object that is equivalent to the version number specified in the <paramref name="utf8Text" /> parameter.</returns>
        /// <exception cref="ArgumentException"><paramref name="utf8Text" /> has fewer than two or more than four version components.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least one component in <paramref name="utf8Text" /> is less than zero.</exception>
        /// <exception cref="FormatException">At least one component in <paramref name="utf8Text" /> is not an integer.</exception>
        /// <exception cref="OverflowException">At least one component in <paramref name="utf8Text" /> represents a number that is greater than <see cref="int.MaxValue"/>.</exception>
        public static ValueVersion Parse(ReadOnlySpan<byte> utf8Text) =>
            ParseVersion(utf8Text, throwOnFailure: true) ?? default;

        /// <summary>Tries to convert the string representation of a version number to an equivalent ValueVersion struct, and returns a value that indicates whether the conversion succeeded.</summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <param name="result">When this method returns, contains the <see cref="T:System.Version" /> equivalent of the number that is contained in <paramref name="input" />, if the conversion succeeded. If <paramref name="input" /> is <see langword="null" />, <see cref="F:System.String.Empty" />, or if the conversion fails, <paramref name="result" /> is <see langword="null" /> when the method returns.</param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="input" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse([NotNullWhen(true)] string input, [NotNullWhen(true)] out ValueVersion result)
        {
            if (input != null)
            {
                ValueVersion? innerResult = ParseVersion(input.AsSpan(), throwOnFailure: false);
                if (innerResult is not null)
                {
                    result = innerResult.Value;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out ValueVersion? result)
        {
            result = ParseVersion(input, throwOnFailure: false);
            return result is not null;
        }

        private static ValueVersion? ParseVersion(ReadOnlySpan<char> input, bool throwOnFailure)
        {
            // Find the separator between major and minor.  It must exist.
            int majorEnd = input.IndexOf('.');
            if (majorEnd < 0)
            {
                if (throwOnFailure) throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));
                return null;
            }

            // Find the ends of the optional minor and build portions.
            // We musn't have any separators after build.
            int buildEnd = -1;
            int minorEnd = input.Slice(majorEnd + 1).IndexOf('.');
            if (minorEnd >= 0)
            {
                minorEnd += (majorEnd + 1);
                buildEnd = input.Slice(minorEnd + 1).IndexOf('.');
                if (buildEnd >= 0)
                {
                    buildEnd += (minorEnd + 1);
                    if (input.Slice(buildEnd + 1).Contains('.'))
                    {
                        if (throwOnFailure)
                            throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));
                        return null;
                    }
                }
            }

            int minor, build, revision;

            // Parse the major version
            if (!TryParseComponent(input.Slice(0, majorEnd), nameof(input), throwOnFailure, out int major))
            {
                return null;
            }

            if (minorEnd != -1)
            {
                // If there's more than a major and minor, parse the minor, too.
                if (!TryParseComponent(input.Slice(majorEnd + 1, minorEnd - majorEnd - 1), nameof(input), throwOnFailure, out minor))
                {
                    return null;
                }

                if (buildEnd != -1)
                {
                    // major.minor.build.revision
                    return
                        TryParseComponent(input.Slice(minorEnd + 1, buildEnd - minorEnd - 1), nameof(build), throwOnFailure, out build) &&
                        TryParseComponent(input.Slice(buildEnd + 1), nameof(revision), throwOnFailure, out revision) ?
                            new ValueVersion(major, minor, build, revision) :
                            null;
                }
                else
                {
                    // major.minor.build
                    return TryParseComponent(input.Slice(minorEnd + 1), nameof(build), throwOnFailure, out build) ?
                        new ValueVersion(major, minor, build) :
                        null;
                }
            }
            else
            {
                // major.minor
                return TryParseComponent(input.Slice(majorEnd + 1), nameof(input), throwOnFailure, out minor) ?
                    new ValueVersion(major, minor) :
                    null;
            }
        }

        private static ValueVersion? ParseVersion(ReadOnlySpan<byte> input, bool throwOnFailure)
        {
            // Find the separator between major and minor.  It must exist.
            int majorEnd = input.IndexOf((byte)'.');
            if (majorEnd < 0)
            {
                if (throwOnFailure)
                    throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));
                return null;
            }

            // Find the ends of the optional minor and build portions.
            // We musn't have any separators after build.
            int buildEnd = -1;
            int minorEnd = input.Slice(majorEnd + 1).IndexOf((byte)'.');
            if (minorEnd >= 0)
            {
                minorEnd += (majorEnd + 1);
                buildEnd = input.Slice(minorEnd + 1).IndexOf((byte)'.');
                if (buildEnd >= 0)
                {
                    buildEnd += (minorEnd + 1);
                    if (input.Slice(buildEnd + 1).Contains((byte)'.'))
                    {
                        if (throwOnFailure)
                            throw new ArgumentException("The specified version string does not conform to the required format", nameof(input));
                        return null;
                    }
                }
            }

            int minor, build, revision;

            // Parse the major version
            if (!TryParseComponent(input.Slice(0, majorEnd), nameof(input), throwOnFailure, out int major))
            {
                return null;
            }

            if (minorEnd != -1)
            {
                // If there's more than a major and minor, parse the minor, too.
                if (!TryParseComponent(input.Slice(majorEnd + 1, minorEnd - majorEnd - 1), nameof(input), throwOnFailure, out minor))
                {
                    return null;
                }

                if (buildEnd != -1)
                {
                    // major.minor.build.revision
                    return
                        TryParseComponent(input.Slice(minorEnd + 1, buildEnd - minorEnd - 1), nameof(build), throwOnFailure, out build) &&
                        TryParseComponent(input.Slice(buildEnd + 1), nameof(revision), throwOnFailure, out revision) ?
                            new ValueVersion(major, minor, build, revision) :
                            null;
                }
                else
                {
                    // major.minor.build
                    return TryParseComponent(input.Slice(minorEnd + 1), nameof(build), throwOnFailure, out build) ?
                        new ValueVersion(major, minor, build) :
                        null;
                }
            }
            else
            {
                // major.minor
                return TryParseComponent(input.Slice(majorEnd + 1), nameof(input), throwOnFailure, out minor) ?
                    new ValueVersion(major, minor) :
                    null;
            }
        }

        private static bool TryParseComponent(ReadOnlySpan<char> component, string componentName, bool throwOnFailure, out int parsedComponent)
        {
            if (throwOnFailure)
            {
                parsedComponent = int.Parse(component, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                ArgumentOutOfRangeException.ThrowIfNegative(parsedComponent, componentName);
                return true;
            }

            bool success = int.TryParse(component, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsedComponent);
            return success && parsedComponent >= 0;
        }

        private static bool TryParseComponent(ReadOnlySpan<byte> component, string componentName, bool throwOnFailure, out int parsedComponent)
        {
            if (throwOnFailure)
            {
                parsedComponent = int.Parse(component, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                ArgumentOutOfRangeException.ThrowIfNegative(parsedComponent, componentName);
                return true;
            }

            bool success = int.TryParse(component, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsedComponent);
            return success && parsedComponent >= 0;
        }

        /// <summary>
        /// Tries to convert the UTF-8 representation of a version number to an equivalent ValueVersion struct, and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="utf8Text">The span of UTF-8 characters to parse.</param>
        /// <param name="result">
        ///     When this method returns, contains the Version equivalent of the number that is contained in <paramref name="utf8Text" />, if the conversion succeeded.
        ///     If <paramref name="utf8Text" /> is empty, or if the conversion fails, result is null when the method returns.
        /// </param>
        /// <returns>true if the <paramref name="utf8Text" /> parameter was converted successfully; otherwise, false.</returns>
        public static bool TryParse(ReadOnlySpan<byte> utf8Text, [NotNullWhen(true)] out ValueVersion? result)
        {
            result = ParseVersion(utf8Text, throwOnFailure: false);
            return result is not null;
        }

        /// <inheritdoc cref="IUtf8SpanParsable{TSelf}.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out TSelf)"/>
        public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, [NotNullWhen(true)] out ValueVersion result)
        {
            ValueVersion? innerResult = ParseVersion(utf8Text, throwOnFailure: false);
            if (innerResult is not null)
            {
                result = innerResult.Value;
                return true;
            }
            result = default;
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
            ArgumentNullException.ThrowIfNull(v1, nameof(v1));
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
            ArgumentNullException.ThrowIfNull(v1, nameof(v1));
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
    }
}
