using FileDBInterface.DbAccess;
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

            RuleFor(x => x.Datetime)
                .Must(ValidateDatetime).WithMessage("{PropertyName} invalid");

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

        public static bool ValidateDescription(string? description)
        {
            return description != string.Empty;
        }

        public static bool ValidateDatetime(string? datetime)
        {
            return datetime == null || DatabaseParsing.ParseFilesDatetime(datetime) != null;
        }
    }
}
