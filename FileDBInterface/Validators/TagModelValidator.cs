using System;
using System.Globalization;
using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class TagModelValidator : AbstractValidator<TagModel>
    {
        public TagModelValidator()
        {
            RuleFor(x => x.id)
                .GreaterThanOrEqualTo(0).WithMessage("Id must be greater than or equal to 0");

            RuleFor(x => x.name)
                .NotNull()
                .MinimumLength(2);
        }
    }
}
