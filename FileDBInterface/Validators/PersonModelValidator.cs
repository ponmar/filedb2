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
                .GreaterThanOrEqualTo(0).WithMessage("Id must be greater than or equal to 0");

            RuleFor(x => x.firstname)
                .NotNull()
                .MinimumLength(2);

            RuleFor(x => x.lastname)
                .NotNull()
                .MinimumLength(2);

            When(x => x.description != null, () =>
            {
                RuleFor(x => x.description)
                    .NotEmpty();
            });

            When(x => x.dateofbirth != null, () =>
            {
                RuleFor(x => x.dateofbirth)
                    .Must(IsDate);
            });

            When(x => x.deceased != null, () =>
            {
                RuleFor(x => x.deceased)
                    .Must(IsDate);
            });

            When(x => x.profilefileid != null, () =>
            {
                RuleFor(x => x.profilefileid)
                    .Must(p => p.Value >= 0);
            });

            //RuleFor(x => x.sex)
        }

        private bool IsDate(string text)
        {
            return DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
