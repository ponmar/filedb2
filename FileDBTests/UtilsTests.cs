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
    }
}
