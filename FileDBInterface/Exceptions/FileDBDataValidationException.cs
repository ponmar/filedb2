using System;

namespace FileDBInterface.Exceptions
{
    public class FileDBDataValidationException : Exception
    {
        public FileDBDataValidationException(string message) : base(message)
        {
        }
    }
}
