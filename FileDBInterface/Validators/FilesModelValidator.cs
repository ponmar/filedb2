using System;
using System.Globalization;
using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class FilesModelValidator : AbstractValidator<FilesModel>
    {
        public FilesModelValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

            RuleFor(x => x.Path)
                .Must(IsInternalFilePath).WithMessage("{PropertyName} invalid");

            RuleFor(x => x.Description)
                .Must(ValidateDescription).WithMessage("{PropertyName} invalid)");

            When(x => x.Datetime != null, () =>
            {
                RuleFor(x => x.Datetime)
                    .Must(IsFileDatetime).WithMessage("{PropertyName} invalid");
            });

            When(x => x.Position != null, () =>
            {
                RuleFor(x => x.Position)
                    .Must(x => DatabaseParsing.ParseFilesPosition(x) != null).WithMessage("{PropertyName} not in format: <latitude> <longitude>");
            });
        }

        private bool IsInternalFilePath(string path)
        {
            return !string.IsNullOrEmpty(path) && !path.Contains(@"\") && !path.StartsWith("/") && !path.EndsWith("/");
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
    }
}
