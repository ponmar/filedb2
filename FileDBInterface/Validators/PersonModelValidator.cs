using System;
using System.Globalization;
using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class PersonModelValidator : AbstractValidator<PersonModel>
    {
        public PersonModelValidator()
        {
            RuleFor(x => x.id)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

            RuleFor(x => x.firstname)
                .NotNull().WithMessage("{PropertyName} is null")
                .MinimumLength(2).WithMessage("{PropertyName} is too short");

            RuleFor(x => x.lastname)
                .NotNull().WithMessage("{PropertyName} is null")
                .MinimumLength(2).WithMessage("{PropertyName} is too short");

            When(x => x.description != null, () =>
            {
                RuleFor(x => x.description)
                    .NotEmpty().WithMessage("{PropertyName} is empty instead of null");
            });

            When(x => x.dateofbirth != null, () =>
            {
                RuleFor(x => x.dateofbirth)
                    .Must(IsDate).WithMessage("{PropertyName} is not in format YYYY-MM-DD");
            });

            When(x => x.deceased != null, () =>
            {
                RuleFor(x => x.deceased)
                    .Must(IsDate).WithMessage("{PropertyName} is not in format YYYY-MM-DD");
            });

            When(x => x.profilefileid != null, () =>
            {
                RuleFor(x => x.profilefileid)
                    .Must(p => p.Value >= 0).WithMessage("{PropertyName} must be greater than or equal to 0");
            });
        }

        private bool IsDate(string text)
        {
            return DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
