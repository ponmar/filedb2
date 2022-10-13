using System.Collections.Generic;
using FileDBInterface.Exceptions;
using FluentValidation.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Exceptions
{
    [TestClass]
    public class DataValidationExceptionTests
    {
        [TestMethod]
        public void Message()
        {
            var failures = new List<ValidationFailure>()
            {
                new ValidationFailure("Property1", "Error1"),
                new ValidationFailure("Property2", "Error2"),
            };
            var result = new ValidationResult(failures);

            var e = new DataValidationException(result);
            Assert.AreEqual("Error1\nError2", e.Message);
        }
    }
}
