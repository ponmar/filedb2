using FileDBAvalonia;
using FileDBAvalonia.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace FileDBAvaloniaTests.Validators;

[Collection("Sequential")]
public class ConfigValidatorTests
{
    private ConfigValidator validator;

    public ConfigValidatorTests()
    {
        Bootstrapper.Reset();
        validator = new();
    }

    [Fact]
    public void Validate_FileToLocationMaxDistanceNegative_Error()
    {
        var config = new ConfigBuilder() { FileToLocationMaxDistance = -1 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.FileToLocationMaxDistance);
    }

    [Fact]
    public void Validate_BlacklistedFilePathPatternsEmpty_Error()
    {
        var config = new ConfigBuilder() { BlacklistedFilePathPatterns = "" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.BlacklistedFilePathPatterns);
    }

    [Fact]
    public void Validate_WhitelistedFilePathPatternsEmpty_Error()
    {
        var config = new ConfigBuilder() { WhitelistedFilePathPatterns = "" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.WhitelistedFilePathPatterns);
    }

    [Fact]
    public void Validate_SlideShowDelayTooSmall_Error()
    {
        var config = new ConfigBuilder() { SlideshowDelay = 0 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SlideshowDelay);
    }

    [Fact]
    public void Validate_SearchHistorySizeTooSmall_Error()
    {
        var config = new ConfigBuilder() { SearchHistorySize = -1 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SearchHistorySize);
    }

    [Fact]
    public void Validate_SearchHistorySizeTooBig_Error()
    {
        var config = new ConfigBuilder() { SearchHistorySize = 11 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SearchHistorySize);
    }

    [Fact]
    public void Validate_LocationLinkWithoutLatitude_Error()
    {
        var config = new ConfigBuilder() { LocationLink = "http://localhost/LON/" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.LocationLink);
    }

    [Fact]
    public void Validate_LocationLinkWithoutLongitude_Error()
    {
        var config = new ConfigBuilder() { LocationLink = "http://localhost/LAT/" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.LocationLink);
    }

    [Fact]
    public void Validate_OverlayTextSizeTooSmall_Error()
    {
        var config = new ConfigBuilder() { OverlayTextSize = 5 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSize);
    }

    [Fact]
    public void Validate_OverlayTextSizeTooBig_Error()
    {
        var config = new ConfigBuilder() { OverlayTextSize = 101 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSize);
    }

    [Fact]
    public void Validate_OverlayTextSizeLargeTooSmall_Error()
    {
        var config = new ConfigBuilder() { OverlayTextSizeLarge = 5 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSizeLarge);
    }

    [Fact]
    public void Validate_OverlayTextSizeLargeTooBig_Error()
    {
        var config = new ConfigBuilder() { OverlayTextSizeLarge = 101 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSizeLarge);
    }

    [Fact]
    public void Validate_ShortItemNameMaxLengthTooSmall_Error()
    {
        var config = new ConfigBuilder() { ShortItemNameMaxLength = 9 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.ShortItemNameMaxLength);
    }

    [Fact]
    public void Validate_ShortItemNameMaxLengthTooBig_Error()
    {
        var config = new ConfigBuilder() { ShortItemNameMaxLength = 101 }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.ShortItemNameMaxLength);
    }

    [Fact]
    public void Validate_LanguageValid_Success()
    {
        var config = new ConfigBuilder() { Language = "sv-SE" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_LanguageNull_Success()
    {
        var config = new ConfigBuilder() { Language = null }.Build();
        var result = validator.TestValidate(config);
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_LanguageInvalid_Error()
    {
        var config = new ConfigBuilder() { Language = "invalid-culture" }.Build();
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }
}
