using System;

namespace FileDBInterface.Exceptions
{
    public class FileDBException : Exception
    {
        public FileDBException(string message) : base(message)
        {
        }

        public FileDBException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
