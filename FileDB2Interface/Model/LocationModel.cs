using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface.Model
{
    public class LocationModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string position { get; set; } // Format: <latitude> <longitude>
    }
}
