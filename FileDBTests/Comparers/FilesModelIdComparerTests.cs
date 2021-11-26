using System;
using FileDB.Comparers;
using FileDBInterface.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Comparers
{
    [TestClass]
    public class FilesModelIdComparerTests
    {
        [TestMethod]
        public void Equals_DifferentInstancesSameId_AreEqual()
        {
            var comparer = new FilesModelIdComparer();
            var model1 = new FilesModel() { id = 1 };
            var model2 = new FilesModel() { id = 1 };
            Assert.IsTrue(comparer.Equals(model1, model2));
        }

        [TestMethod]
        public void Equals_SameInstances_AreEqual()
        {
            var comparer = new FilesModelIdComparer();
            var model1 = new FilesModel() { id = 1 };
            Assert.IsTrue(comparer.Equals(model1, model1));
        }

        [TestMethod]
        public void Equals_DifferentIds_AreNotEqual()
        {
            var comparer = new FilesModelIdComparer();
            var model1 = new FilesModel() { id = 1 };
            var model2 = new FilesModel() { id = 2 };
            Assert.IsFalse(comparer.Equals(model1, model2));
        }
    }
}
