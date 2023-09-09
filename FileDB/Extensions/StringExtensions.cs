using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB.Extensions;

public static class StringExtensions
{
    public static bool HasContent(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }
}
