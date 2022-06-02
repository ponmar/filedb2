﻿namespace FileDB
{
    public class ReleaseInformation
    {
        public static Version Version = new(2, 12);

        public static string VersionString => $"{Version.Major}.{Version.Minor}";

        public const string ApplicationDownloadUrl = "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm";
    }

    public record Version(int Major, int Minor);
}
