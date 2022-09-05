using System;
using System.IO;
using System.Linq;
using System.Net;
using FileDB.Configuration;
using FluentValidation;

namespace FileDB.Validators
{
    public class ConfigValidator : AbstractValidator<Config>
    {
        public ConfigValidator()
        {
            RuleFor(c => c.Name).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("{PropertyName} is null")
                .NotEmpty().WithMessage("{PropertyName} is empty")
                .Length(3, 100).WithMessage("{PropertyName} length is invalid");

            RuleFor(c => c.Database).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("{PropertyName} is null")
                .Must(x => x.EndsWith(".db")).WithMessage("Invalid database file extension")
                .Must(x => Path.IsPathFullyQualified(x)).WithMessage("Database path is not absolute");

            RuleFor(c => c.FilesRootDirectory).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("{PropertyName} is null")
                .Must(x => Path.IsPathFullyQualified(x)).WithMessage("Files root directory path is not absolute");

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

            When(c => !string.IsNullOrEmpty(c.LocationLink), () =>
            {
                RuleFor(c => c.LocationLink)
                    .Must(IsValidUrl).WithMessage("Location link is not a valid url")
                    .Must(x => x.Contains("LAT")).WithMessage("LAT not included in url")
                    .Must(x => x.Contains("LON")).WithMessage("LON not included in url");
            });

            When(c => c.CastHttpServerPort > 0, () =>
            {
                RuleFor(c => c.CastHttpServerPort)
                    .InclusiveBetween(1, 65535);
            });

            When(c => !string.IsNullOrEmpty(c.CastHttpServerInterface), () =>
            {
                RuleFor(c => c.CastHttpServerInterface)
                    .Must(IsValidIpAddress!).WithMessage("Cast HTTP server interface is not a valid IP address");
            });
        }

        public bool CastingEnabled(Config config)
        {
            return config.CastHttpServerPort > 0 &&
                !string.IsNullOrEmpty(config.CastHttpServerInterface) && IsValidIpAddress(config.CastHttpServerInterface);
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

        private bool IsValidIpAddress(string ipAddress)
        {
            return ipAddress != null && IPAddress.TryParse(ipAddress, out var _);
        }

        private bool IsSemicolonSeparatedFilePatterns(string text)
        {
            if (text == null)
            {
                return false;
            }

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
