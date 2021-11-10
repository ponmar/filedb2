using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Shared
    {
        public static Version Version = new(0, 1);
    }

    public record Version(int Major, int Minor);
}
