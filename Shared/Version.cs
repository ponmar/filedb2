﻿namespace FileDB
{
    public class ReleaseInformation
    {
        public static Version Version = new(2, 5);
    }

    public record Version(int Major, int Minor);
}
