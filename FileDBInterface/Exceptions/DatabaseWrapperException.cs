using System;

namespace FileDBInterface.Exceptions
{
    public class DatabaseWrapperException : Exception
    {
        public DatabaseWrapperException(string message) : base(message)
        {
        }

        public DatabaseWrapperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
