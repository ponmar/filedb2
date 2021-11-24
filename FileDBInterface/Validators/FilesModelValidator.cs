using System;
using System.Globalization;
using System.Text.RegularExpressions;
using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class FilesModelValidator : AbstractValidator<FilesModel>
    {
        public FilesModelValidator()
        {
            RuleFor(x => x.id).GreaterThanOrEqualTo(0).WithMessage("Id must be greater than or equal to 0");

            RuleFor(x => x.path)
                .Must(IsInternalFilePath).WithMessage("Invalid path");

            RuleFor(x => x.description)
                .Must(ValidateDescription).WithMessage("Empty string used for file description (should be null or have content)");

            When(x => x.datetime != null, () =>
            {
                RuleFor(x => x.datetime)
                    .Must(IsFileDatetime).WithMessage("Invalid datetime");
            });

            RuleFor(x => x.position)
                .Must(ValidatePosition).WithMessage("Invalid GPS position");
        }

        private bool IsInternalFilePath(string path)
        {
            return !path.Contains(@"\") && !path.StartsWith("/") && !path.EndsWith("/");
        }

        public static bool ValidateDescription(string description)
        {
            return description != string.Empty;
        }

        private bool IsFileDatetime(string datetime)
        {
            return
                DateTime.TryParseExact(datetime, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
                DateTime.TryParseExact(datetime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
                DateTime.TryParseExact(datetime, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public static bool ValidatePosition(string position)
        {
            if (position != null)
            {
                var parts = position.Split(' ');
                if (parts.Length != 2)
                {
                    return false;
                }

                if (!double.TryParse(parts[0], out var _) ||
                    !double.TryParse(parts[1], out var _))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
