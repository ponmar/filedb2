using FileDBShared.Model;
using FluentValidation;

namespace FileDBShared.Validators;

public class PersonModelValidator : AbstractValidator<PersonModel>
{
    public PersonModelValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

        RuleFor(x => x.Firstname)
            .NotNull().WithMessage("{PropertyName} is null")
            .MinimumLength(2).WithMessage("{PropertyName} is too short");

        RuleFor(x => x.Lastname)
            .NotNull().WithMessage("{PropertyName} is null")
            .MinimumLength(2).WithMessage("{PropertyName} is too short");

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("{PropertyName} is empty instead of null");
        });

        When(x => x.DateOfBirth != null, () =>
        {
            RuleFor(x => x.DateOfBirth!)
                .Must(IsPersonDateOfBirth).WithMessage("{PropertyName} is not in format YYYY-MM-DD, YYYY-MM or YYYY");
        });

        When(x => x.Deceased != null, () =>
        {
            RuleFor(x => x.Deceased!)
                .Must(IsPersonDeceasedDate).WithMessage("{PropertyName} is not in format YYYY-MM-DD, YYYY-MM or YYYY");
        });

        When(x => x.ProfileFileId != null, () =>
        {
            RuleFor(x => x.ProfileFileId)
                .Must(p => p!.Value >= 0).WithMessage("{PropertyName} must be greater than or equal to 0");
        });
    }

    private bool IsPersonDateOfBirth(string dateStr)
    {
        try
        {
            DatabaseParsing.ParsePersonDateOfBirth(dateStr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsPersonDeceasedDate(string dateStr)
    {
        try
        {
            DatabaseParsing.ParsePersonDeceasedDate(dateStr);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
