using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class FilesModelValidator : AbstractValidator<FilesModel>
    {
        public FilesModelValidator()
        {
            RuleFor(x => x.id).GreaterThanOrEqualTo(0).WithMessage("Id must be greater than 0");

            RuleFor(x => x.path)
                .Must(IsInternalFilePath).WithMessage("Invalid path");

            RuleFor(x => x.description)
                .Must(ValidateDescription).WithMessage("Empty string used for file description (should be null or have content)");

            //RuleFor(x => x.datetime). // Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS

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
