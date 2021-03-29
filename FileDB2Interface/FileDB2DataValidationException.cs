using System;
using System.Collections.Generic;
using System.Text;

namespace FileDB2Interface
{
    public class FileDB2DataValidationException : Exception
    {
        public FileDB2DataValidationException(string message) : base(message)
        {
        }
    }

}
