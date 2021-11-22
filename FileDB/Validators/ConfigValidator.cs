using System;
using System.IO;
using FluentValidation;

namespace FileDB.Validators
{
    public class ConfigValidator : AbstractValidator<Config.Config>
    {
        public ConfigValidator()
        {
            RuleFor(c => c.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is empty")
                .Length(3, 100).WithMessage("Name length is invalid");

            RuleFor(c => c.Database).Cascade(CascadeMode.Stop)
                .Must(x => x.EndsWith(".db")).WithMessage("Invalid database file extension")
                .Must(x => Path.IsPathFullyQualified(x)).WithMessage("Database path is not absolute")
                .Must(x => File.Exists(x)).WithMessage("Database missing");

            RuleFor(c => c.FilesRootDirectory).Cascade(CascadeMode.Stop)
                .Must(x => Path.IsPathFullyQualified(x)).WithMessage("Files root directory path is not absolute")
                .Must(x => Directory.Exists(x)).WithMessage("Files root directory missing");

            When(c => !string.IsNullOrEmpty(c.LocationLink), () =>
            {
                RuleFor(c => c.LocationLink).Must(IsValidUrl).WithMessage("Location link is not a valid url");
            });
        }

        private bool IsValidUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}
