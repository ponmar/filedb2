using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Validators
{
    [TestClass]
    public class FilesModelValidatorTests
    {
        private FilesModelValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            validator = new();
        }

        [TestMethod]
        public void Validate_NoOptionalData_Valid()
        {
            var model = new FilesModel() { Id = 1, Path = "name", Description = null, Datetime = null, Position = null };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_AllData_Valid()
        {
            var model = new FilesModel() { Id = 1, Path = "name", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [TestMethod]
        public void Validate_PathIsNull_Error()
        {
            var model = new FilesModel() { Id = -1, Path = null, Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Path);
        }

        [TestMethod]
        public void Validate_DescriptonIsEmpty_Error()
        {
            var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Validate_DatetimeIsYear_Error()
        {
            var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
        }

        [TestMethod]
        public void Validate_DatetimeIsDate_Error()
        {
            var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
        }

        [TestMethod]
        public void Validate_DatetimeIsDateAndTime_Error()
        {
            var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05T20:50:10", Position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
        }

        // TODO: re-use gps position test from location?
    }
}
