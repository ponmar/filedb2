using System;
using System.Globalization;
using System.Linq;
using FileDB.Configuration;
using FileDB.Lang;
using FileDBInterface.Extensions;
using FluentValidation;

namespace FileDB.Validators;

public class ConfigValidator : AbstractValidator<Config>
{
    public ConfigValidator()
    {
        RuleFor(c => c.FileToLocationMaxDistance)
            .GreaterThanOrEqualTo(0).WithMessage(Strings.ConfigValidatorInvalidFileToLocationMaxDistance);

        RuleFor(c => c.BlacklistedFilePathPatterns)
            .Must(IsSemicolonSeparatedFilePatterns).WithMessage(Strings.ConfigValidatorInvalidBlacklistedFilePathPatterns);

        RuleFor(c => c.WhitelistedFilePathPatterns)
            .Must(IsSemicolonSeparatedFilePatterns).WithMessage(Strings.ConfigValidatorInvalidWhitelistedFilePathPatterns);

        RuleFor(c => c.SlideshowDelay)
            .GreaterThan(0).WithMessage(Strings.ConfigValidatorInvalidSlideshowDelay);

        RuleFor(c => c.SearchHistorySize)
            .InclusiveBetween(0, 10).WithMessage(Strings.ConfigValidatorInvalidSearchHistorySize);

        When(c => c.LocationLink.HasContent(), () =>
        {
            RuleFor(c => c.LocationLink)
                .Must(IsValidUrl).WithMessage(Strings.ConfigValidatorLocationLinkNotValidUrl)
                .Must(x => x.Contains("LAT")).WithMessage(Strings.ConfigValidatorLatNotIncludedInUrl)
                .Must(x => x.Contains("LON")).WithMessage(Strings.ConfigValidatorLonNotIncludedInUrl);
        });

        RuleFor(c => c.OverlayTextSize)
            .InclusiveBetween(8, 100).WithMessage(Strings.ConfigValidatorInvalidOverlayTextSize);

        RuleFor(c => c.OverlayTextSizeLarge)
            .InclusiveBetween(8, 100).WithMessage(Strings.ConfigValidatorInvalidOverlayLargeTextSize);

        RuleFor(c => c.ShortItemNameMaxLength)
            .InclusiveBetween(10, 100).WithMessage(Strings.ConfigValidatorInvalidShortItemNameMaxLength);

        When(c => c.Language is not null, () =>
        {
            RuleFor(c => c.Language)
               .Must(IsCulture!).WithMessage(Strings.ConfigValidatorInvalidLanguage);
        });

        RuleFor(c => c.ImageMemoryCacheCount)
            .GreaterThan(0);

        RuleFor(c => c.NumImagesToPreload)
            .GreaterThan(0);
    }

    private bool IsCulture(string cultureName)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(x => x.Name == cultureName);
    }

    private bool IsValidUrl(string url)
    {
        try
        {
            _ = new Uri(url);
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private bool IsSemicolonSeparatedFilePatterns(string text)
    {
        if (text is null)
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

        if (parts.Any(x => !x.HasContent()))
        {
            return false;
        }

        return true;
    }
}
