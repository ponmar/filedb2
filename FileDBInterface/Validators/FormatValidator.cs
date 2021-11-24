using System;
using System.Globalization;
using FileDBInterface.Exceptions;

namespace FileDBInterface.Validators
{
    public static class FormatValidator
    {
        public static void ValidateTagName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new DataValidationException("Tag name empty");
            }
        }
    }
}
