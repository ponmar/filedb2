using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class LocationModelValidator : AbstractValidator<LocationModel>
    {
        public LocationModelValidator()
        {
            RuleFor(x => x.id)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

            RuleFor(x => x.name)
                .NotNull().WithMessage("{PropertyName} missing")
                .MinimumLength(2).WithMessage("{PropertyName} is too short");

            When(x => x.description != null, () =>
            {
                RuleFor(x => x.description)
                    .NotEmpty().WithMessage("{PropertyName} is empty instead of null");
            });

            RuleFor(x => x.position)
                .Must(x => DatabaseParsing.ParseFilesPosition(x) != null).WithMessage("{PropertyName} not in format: <latitude> <longitude>");
        }
    }
}
