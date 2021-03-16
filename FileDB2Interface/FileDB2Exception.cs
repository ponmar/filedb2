using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface
{
    public class FileDB2Exception : Exception
    {
        public FileDB2Exception(string message) : base(message)
        {
        }

        public FileDB2Exception(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
