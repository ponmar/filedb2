using System;
using System.IO;
using System.Linq;
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

            RuleFor(c => c.FileToLocationMaxDistance)
                .GreaterThanOrEqualTo(0).WithMessage("Invalid file to location max distance");

            RuleFor(c => c.BlacklistedFilePathPatterns)
                .Must(IsSemicolonSeparatedFilePatterns).WithMessage("Invalid blacklisted file path patterns");

            RuleFor(c => c.WhitelistedFilePathPatterns)
                .Must(IsSemicolonSeparatedFilePatterns).WithMessage("Invalid whitelisted file path patterns");

            RuleFor(c => c.SlideshowDelay)
                .GreaterThan(0).WithMessage("Invalid slideshow delay");

            RuleFor(c => c.SearchHistorySize)
                .InclusiveBetween(0, 10).WithMessage("Invalid search history size");

            RuleFor(c => c.StartupBackupReminderAfterDays)
                .GreaterThanOrEqualTo(0).WithMessage("Invalid startup backup reminder after days");

            When(c => !string.IsNullOrEmpty(c.LocationLink), () =>
            {
                RuleFor(c => c.LocationLink)
                    .Must(IsValidUrl).WithMessage("Location link is not a valid url");
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

        private bool IsSemicolonSeparatedFilePatterns(string text)
        {
            var parts = text.Split(";");
            if (parts.Length == 0)
            {
                return false;
            }

            if (parts.Any(x => x != x.Trim()))
            {
                return false;
            }

            if (parts.Any(x => string.IsNullOrEmpty(x)))
            {
                return false;
            }

            return true;
        }
    }
}
