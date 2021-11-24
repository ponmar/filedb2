using System;
using System.Globalization;
using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class LocationModelValidator : AbstractValidator<LocationModel>
    {
        public LocationModelValidator()
        {
            RuleFor(x => x.id)
                .GreaterThanOrEqualTo(0).WithMessage("Id must be greater than or equal to 0");

            RuleFor(x => x.name)
                .NotNull()
                .MinimumLength(2);

            When(x => x.description != null, () =>
            {
                RuleFor(x => x.description)
                    .NotEmpty();
            });

            RuleFor(x => x.position)
                .Must(FilesModelValidator.ValidatePosition);
        }
    }
}
