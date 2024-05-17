using FileDBInterface.Model;
using FluentValidation;

namespace FileDBInterface.Validators;

public class FileModelValidator : AbstractValidator<FileModel>
{
    public const string DescriptionLineEnding = "\n";

    public FileModelValidator()
    {
        RuleFor(x => x.Id).GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0");

        RuleFor(x => x.Path)
            .Must(IsInternalFilePath).WithMessage("{PropertyName} invalid");

        RuleFor(x => x.Description)
            .Must(ValidateDescription).WithMessage("{PropertyName} invalid)");

        RuleFor(x => x.Datetime)
            .Must(ValidateDatetime).WithMessage("{PropertyName} invalid");

        RuleFor(x => x.Orientation)
            .Must(ValidateOrientation).WithMessage("{PropertyName} invalid");

        When(x => x.Position is not null, () =>
        {
            RuleFor(x => x.Position)
                .Must(x => DatabaseParsing.ParseFilesPosition(x) is not null).WithMessage("{PropertyName} not in format: <latitude> <longitude>");
        });
    }

    private bool IsInternalFilePath(string path)
    {
        return !string.IsNullOrEmpty(path) && !path.Contains('\\') && !path.StartsWith("/") && !path.EndsWith("/");
    }

    public static bool ValidateDescription(string? description)
    {
        return description is null ||
            (description != string.Empty && !description.Contains("\r\n"));
    }

    public static bool ValidateDatetime(string? datetime)
    {
        return datetime is null || DatabaseParsing.ParseFilesDatetime(datetime) is not null;
    }

    public static bool ValidateOrientation(int? orientation)
    {
        return orientation is null || (orientation >= 1 && orientation <= 8);
    }
}
