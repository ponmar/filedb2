using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface.Model
{
    // Values from: https://en.wikipedia.org/wiki/ISO/IEC_5218
    public enum Sex
    {
        NotKnown = 0,
        Male = 1,
        Female = 2,
        NotApplicable = 9,
    }

    public class PersonModel
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string description { get; set; }
        public string dateofbirth { get; set; } // Format: YYYY-MM-DD
        public int? profilefileid { get; set; }
        public Sex sex { get; set; }
    }
}
