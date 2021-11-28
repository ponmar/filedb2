using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class LocationModelValidator : AbstractValidator<LocationModel>
    {
        public LocationModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

            RuleFor(x => x.Name)
                .NotNull().WithMessage("{PropertyName} missing")
                .MinimumLength(2).WithMessage("{PropertyName} is too short");

            When(x => x.Description != null, () =>
            {
                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("{PropertyName} is empty instead of null");
            });

            When(x => x.Position != null, () =>
            {
                RuleFor(x => x.Position)
                    .Must(x => DatabaseParsing.ParseFilesPosition(x) != null).WithMessage("{PropertyName} not in format: <latitude> <longitude>");
            });
        }
    }
}
