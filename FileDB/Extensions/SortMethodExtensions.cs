﻿using FileDB.Configuration;

namespace FileDB.Extensions
{
    public static class SortMethodExtensions
    {
        public static string GetDescription(this SortMethod method)
        {
            switch (method)
            {
                case SortMethod.Date:
                    return "Date (Old - New)";

                case SortMethod.DateDesc:
                    return "Date (New - Old)";

                case SortMethod.Path:
                    return "Path (A - Z)";

                case SortMethod.PathDesc:
                    return "Path (Z - A)";
            }
            return string.Empty;
        }
    }
}
