using FileDBShared.Model;
using FluentValidation;

namespace FileDBInterface.Validators
{
    public class TagModelValidator : AbstractValidator<TagModel>
    {
        public TagModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

            RuleFor(x => x.Name)
                .NotNull().WithMessage("{PropertyName} is missing")
                .MinimumLength(2).WithMessage("{PropertyName} is too short");
        }
    }
}
