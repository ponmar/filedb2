using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators;

public class PersonBoundingBoxValidator : AbstractValidator<PersonBoundingBox>
{
    public PersonBoundingBoxValidator()
    {
        RuleFor(x => x.X)
            .InclusiveBetween(0.0, 1.0).WithMessage("{PropertyName} must be in range [0.0, 1.0]");

        RuleFor(x => x.Y)
            .InclusiveBetween(0.0, 1.0).WithMessage("{PropertyName} must be in range [0.0, 1.0]");

        RuleFor(x => x.Width)
            .InclusiveBetween(0.0, 1.0).WithMessage("{PropertyName} must be in range [0.0, 1.0]");

        RuleFor(x => x.Height)
            .InclusiveBetween(0.0, 1.0).WithMessage("{PropertyName} must be in range [0.0, 1.0]");
    }
}
