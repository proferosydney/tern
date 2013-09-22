using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Profero.Tern.Migrate
{
    public interface IVersionStrategy
    {
        bool SupportsNumericVersion { get; }

        long GetNumericVersion(string input);
    }

    public class TextVersionStrategy : IVersionStrategy
    {
        public bool SupportsNumericVersion
        {
            get { return false; }
        }

        public long GetNumericVersion(string input)
        {
            return 0L;
        }
    }

    public class VersionVersionStrategy : IVersionStrategy
    {
        public long GetNumericVersion(string input)
        {
            Version version;

            try
            {
                version = input.Contains(".")
                    ? Version.Parse(input)
                    : new Version(Int32.Parse(input), 0);
            }
            catch(Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }

            long major = Normalize(version.Major);
            long minor = Normalize(version.Minor);
            long build = Normalize(version.Build);
            long revision = Normalize(version.Revision);

            return (long)((ulong)revision |
                (ulong)(build << 16) |
                (ulong)(minor << 32) |
                (ulong)(major << 48));
        }

        static long Normalize(int value)
        {
            return (value == -1)
                ? 0
                : value;
        }

        public bool SupportsNumericVersion
        {
            get { return true; }
        }
    }

    public class DateVersionStrategy : IVersionStrategy
    {
        public const string DateVersionPattern = @"^(?<year>\d{2}|\d{4})[\.\-_](?<month>\d{2})[\.\-_](?<day>\d{2})(?:[\.\-_](?<revision>\d{1,5}))?$";

        public string DefaultFilenamePattern
        {
            get { return @"^(?<version>\d{2}|\d{4}[\.\-_]\d{2}[\.\-_]\d{2}(?:[\.\-_]\d{1-5})?).*\.sql$"; }
        }

        public long GetNumericVersion(string input)
        {
            Match match = Regex.Match(input, DateVersionPattern);

            if (match == null || !match.Success)
            {
                throw new FormatException(String.Format("Value '{0}' is not valid for the Date versioning style", input));
            }

            short year = Int16.Parse(match.Groups["year"].Value);
            short month = Int16.Parse(match.Groups["month"].Value);
            short day = Int16.Parse(match.Groups["day"].Value);
            short revision = match.Groups["revision"].Success
                ? Int16.Parse(match.Groups["revision"].Value)
                : (short)0;

            return (long)((ulong)((long)revision) |
                (ulong)((long)day << 16) |
                (ulong)((long)month << 32) |
                (ulong)((long)year << 48));
        }

        public bool SupportsNumericVersion
        {
            get { return true; }
        }
    }
}
