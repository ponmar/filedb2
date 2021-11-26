using FileDB.Config;
using FileDB.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Validators
{
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
        public void Validate_DefaultConfig_Valid()
        {
            var config = DefaultConfigs.Default;
            var result = validator.TestValidate(config);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_DemoConfig_Valid()
        {
            var config = DefaultConfigs.CreateDemo();
            var result = validator.TestValidate(config);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
