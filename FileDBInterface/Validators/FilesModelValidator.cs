using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class FilesModelValidator : AbstractValidator<FilesModel>
    {
        public FilesModelValidator()
        {
            RuleFor(x => x.id).GreaterThanOrEqualTo(0).WithMessage("Id must be greater than 0");

            //RuleFor(x => x.path).     // Format: path/to/file/filename

            RuleFor(x => x.description)
                .Must(ValidateDescription).WithMessage("Empty string used for file description (should be null)");

            //RuleFor(x => x.datetime). // Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS

            //RuleFor(x => x.position). // Format: <latitude> <longitude>
        }

        public static bool ValidateDescription(string description)
        {
            return description != string.Empty;
        }
    }
}
