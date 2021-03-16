using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface.Model
{
    public class PersonModel
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string description { get; set; }
        public string dateofbirth { get; set; } // Format: YYYY-MM-DD
        public int? profilefileid { get; set; }
    }
}
