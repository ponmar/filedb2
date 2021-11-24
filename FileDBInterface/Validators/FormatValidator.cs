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

        public static void ValidateLocationName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new DataValidationException("Location name empty");
            }
        }

        public static void ValidateLocationDescription(string description)
        {
            if (description == string.Empty)
            {
                throw new DataValidationException("Empty location description should be null");
            }
        }
    }
}
