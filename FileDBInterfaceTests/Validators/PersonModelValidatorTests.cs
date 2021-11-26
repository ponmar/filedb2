using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Validators
{
    [TestClass]
    public class PersonModelValidatorTests
    {
        private PersonModelValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            validator = new();
        }

        [TestMethod]
        public void Validate_NoOptionalData_Valid()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_AllData_Valid()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = "description",
                dateofbirth = "2000-01-01",
                deceased = "2000-01-02",
                profilefileid = 10,
                sex = Sex.NotApplicable,
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new PersonModel()
            {
                id = -1,
                firstname = "firstname",
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.id);
        }

        [TestMethod]
        public void Validate_FirstnameIsEmpty_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = string.Empty,
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.firstname);
        }

        [TestMethod]
        public void Validate_LastnameIsEmpty_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = string.Empty,
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.lastname);
        }

        [TestMethod]
        public void Validate_FirstnameIsTooShort_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "x",
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.firstname);
        }

        [TestMethod]
        public void Validate_LastnameIsTooShort_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "x",
                description = null,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.lastname);
        }

        [TestMethod]
        public void Validate_DescriptionIsEmpty_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = string.Empty,
                dateofbirth = null,
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.description);
        }

        [TestMethod]
        public void Validate_DateOfBirthInvalid_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = null,
                dateofbirth = "invalid",
                deceased = null,
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.dateofbirth);
        }

        [TestMethod]
        public void Validate_DeceasedInvalid_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = "invalid",
                profilefileid = null,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.deceased);
        }

        [TestMethod]
        public void Validate_ProfileFileIdInvalid_Error()
        {
            var model = new PersonModel()
            {
                id = 1,
                firstname = "firstname",
                lastname = "lastname",
                description = null,
                dateofbirth = null,
                deceased = "invalid",
                profilefileid = -10,
                sex = Sex.NotKnown,
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.profilefileid);
        }
    }
}
