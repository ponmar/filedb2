using System.Collections.Generic;
using FileDB;
using FileDBInterface.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void CreateFileList_NoFiles_EmptyString()
        {
            Assert.AreEqual(string.Empty, Utils.CreateFileList(new List<FilesModel>()));
        }

        [TestMethod]
        public void CreateFileList_SomeFiles_ValidFileList()
        {
            var files = new List<FilesModel>()
            {
                new FilesModel() { Id = 1 },
                new FilesModel() { Id = 2 },
                new FilesModel() { Id = 3 },
            };
            Assert.AreEqual("1;2;3", Utils.CreateFileList(files));
        }

        [TestMethod]
        public void CreateFileIds_EmptyString_ReturnsEmptyList()
        {
            Assert.AreEqual(0, Utils.CreateFileIds(null).Count);
            Assert.AreEqual(0, Utils.CreateFileIds(string.Empty).Count);
        }

        [TestMethod]
        public void CreateFileIds_ValidList_ReturnsValidIds()
        {
            var result = Utils.CreateFileIds("1;3;2");

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[1], 3);
            Assert.AreEqual(result[2], 2);
        }
    }
}
