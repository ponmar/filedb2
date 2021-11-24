﻿using System;
using System.Globalization;
using FileDBInterface.Exceptions;

namespace FileDBInterface.Validators
{
    public static class FormatValidator
    {

        public static void ValidatePersonFirstname(string firstname)
        {
            if (string.IsNullOrEmpty(firstname))
            {
                throw new DataValidationException("Person firstname empty");
            }
        }

        public static void ValidatePersonLastname(string lastname)
        {
            if (string.IsNullOrEmpty(lastname))
            {
                throw new DataValidationException("Person lastname empty");
            }
        }

        public static void ValidatePersonDescription(string description)
        {
            if (description == string.Empty)
            {
                throw new DataValidationException("Empty person description should be null");
            }
        }

        public static void ValidatePersonDateOfBirth(string dateOfBirthStr)
        {
            if (dateOfBirthStr != null)
            {
                ValidateInternalDateStr(dateOfBirthStr);
            }
        }

        public static void ValidatePersonDeceased(string deceasedStr)
        {
            if (deceasedStr != null)
            {
                ValidateInternalDateStr(deceasedStr);
            }
        }

        private static void ValidateInternalDateStr(string dateStr)
        {
            if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                throw new DataValidationException($"Invalid format for date string: {dateStr} (should match YYYY-MM-DD)");
            }
        }

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