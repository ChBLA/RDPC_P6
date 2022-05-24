using System;

namespace Utils
{
    public class OSDetecter
    {
        public const string UNIX_PREFIX = "Unix";
        public const string WINDOWS_PREFIX = "Microsoft Windows";
        public bool IsUnix => Environment.OSVersion.ToString().StartsWith(UNIX_PREFIX);
        public bool IsWindows => Environment.OSVersion.ToString().StartsWith(WINDOWS_PREFIX);
    }
}