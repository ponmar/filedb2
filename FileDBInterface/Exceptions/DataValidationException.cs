using System;

namespace FileDBInterface.Exceptions
{
    public class DataValidationException : Exception
    {
        public DataValidationException(string message) : base(message)
        {
        }
    }
}
