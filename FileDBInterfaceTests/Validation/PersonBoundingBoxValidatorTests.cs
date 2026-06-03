using FileDBInterface.Model;
using FileDBInterface.Validators;
using Xunit;

namespace FileDBInterfaceTests.Validation;

public class PersonBoundingBoxValidatorTests
{
    [Fact]
    public void BoundingBoxValidator_EnforcesRangeConstraints()
    {
        var validator = new PersonBoundingBoxValidator();

        var validBBox = new PersonBoundingBox(0.0, 0.0, 1.0, 1.0);
        var validResult = validator.Validate(validBBox);
        Assert.True(validResult.IsValid);

        var invalidBBox = new PersonBoundingBox(-0.1, 0.5, 0.5, 0.5);
        var invalidResult = validator.Validate(invalidBBox);
        Assert.False(invalidResult.IsValid);

        var invalidBBox2 = new PersonBoundingBox(0.5, 0.5, 1.5, 0.3);
        var invalidResult2 = validator.Validate(invalidBBox2);
        Assert.False(invalidResult2.IsValid);
    }
}
