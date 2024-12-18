﻿using System;
using System.Globalization;
using System.Linq;
using FileDB.Configuration;
using FileDBInterface.Extensions;
using FluentValidation;

namespace FileDB.Validators;

public class ConfigValidator : AbstractValidator<Config>
{
    public ConfigValidator()
    {
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

        When(c => c.LocationLink.HasContent(), () =>
        {
            RuleFor(c => c.LocationLink)
                .Must(IsValidUrl).WithMessage("Location link is not a valid url")
                .Must(x => x.Contains("LAT")).WithMessage("LAT not included in url")
                .Must(x => x.Contains("LON")).WithMessage("LON not included in url");
        });

        RuleFor(c => c.OverlayTextSize)
            .InclusiveBetween(8, 100).WithMessage("Invalid overlay text size");

        RuleFor(c => c.OverlayTextSizeLarge)
            .InclusiveBetween(8, 100).WithMessage("Invalid overlay large text size");

        RuleFor(c => c.ShortItemNameMaxLength)
            .InclusiveBetween(10, 100).WithMessage("Invalid short item name max length");

        When(c => c.Language is not null, () =>
        {
            RuleFor(c => c.Language)
               .Must(IsCulture!).WithMessage("Invalid language");
        });

        RuleFor(c => c.ImageMemoryCacheCount)
            .GreaterThan(0);

        RuleFor(c => c.NumImagesToPreload)
            .GreaterThan(0);
    }

    private bool IsCulture(string cultureName)
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Any(x => x.Name == cultureName);
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
