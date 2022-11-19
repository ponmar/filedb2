using FileDB.Configuration;
using FileDB.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Validators;

[TestClass]
public class ConfigValidatorTests
{
    private ConfigValidator validator;

    [TestInitialize]
    public void Initialize()
    {
        validator = new();
    }

    [TestMethod]
    public void Validate_NameEmpty_Error()
    {
        var config = new Config(Name: "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestMethod]
    public void Validate_DatabaseEmpty_Error()
    {
        var config = new Config(default, Database: "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.Database);
    }

    [TestMethod]
    public void Validate_FilesRootDirectoryEmpty_Error()
    {
        var config = new Config(default, default, FilesRootDirectory: "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.FilesRootDirectory);
    }

    [TestMethod]
    public void Validate_FileToLocationMaxDistanceNegative_Error()
    {
        var config = new Config(default, default, default, FileToLocationMaxDistance: -1, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.FileToLocationMaxDistance);
    }

    [TestMethod]
    public void Validate_BlacklistedFilePathPatternsEmpty_Error()
    {
        var config = new Config(default, default, default, default, BlacklistedFilePathPatterns: "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.BlacklistedFilePathPatterns);
    }

    [TestMethod]
    public void Validate_WhitelistedFilePathPatternsEmpty_Error()
    {
        var config = new Config(default, default, default, default, default, WhitelistedFilePathPatterns: "", default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.WhitelistedFilePathPatterns);
    }

    [TestMethod]
    public void Validate_SlideShowDelayTooSmall_Error()
    {
        var config = new Config(default, default, default, default, default, default, default, SlideshowDelay: 0, default, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SlideshowDelay);
    }

    [TestMethod]
    public void Validate_SearchHistorySizeTooSmall_Error()
    {
        var config = new Config(default, default, default, default, default, default, default, default, SearchHistorySize: -1, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SearchHistorySize);
    }

    [TestMethod]
    public void Validate_SearchHistorySizeTooBig_Error()
    {
        var config = new Config(default, default, default, default, default, default, default, default, SearchHistorySize: 11, default, default, default, default, default, default, default, default, default, default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.SearchHistorySize);
    }

    [TestMethod]
    public void Validate_LocationLinkWithoutLatitude_Error()
    {
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LON/", default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.LocationLink);
    }

    [TestMethod]
    public void Validate_LocationLinkWithoutLongitude_Error()
    {
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LAT/", default, default, default, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.LocationLink);
    }

    [TestMethod]
    public void Validate_OverlayTextSizeTooSmall_Error()
    {
        var overlayTextSize = 5;
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LAT/", default, default, overlayTextSize, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSize);
    }

    [TestMethod]
    public void Validate_OverlayTextSizeTooBig_Error()
    {
        var overlayTextSize = 101;
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LAT/", default, default, overlayTextSize, default);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSize);
    }

    [TestMethod]
    public void Validate_OverlayTextSizeLargeTooSmall_Error()
    {
        var overlayTextSizeLarge = 5;
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LAT/", default, default, default, overlayTextSizeLarge);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSizeLarge);
    }

    [TestMethod]
    public void Validate_OverlayTextSizeLargeTooBig_Error()
    {
        var overlayTextSizeLarge = 101;
        var config = new Config(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, LocationLink: "http://localhost/LAT/", default, default, default, overlayTextSizeLarge);
        var result = validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.OverlayTextSizeLarge);
    }
}
