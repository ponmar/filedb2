using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB.Sorters;
using FileDBInterface.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters
{
    [TestClass]
    public class FilesByDateSorterTests
    {
        private FilesByDateSorter sorter;

        [TestInitialize]
        public void Initialize()
        {
            sorter = new();
        }

        [TestMethod]
        public void Compare()
        {
            var items = new List<FilesModel>()
            {
                new FilesModel() { Id = 1, Datetime = "2000-10-20" },
                new FilesModel() { Id = 0, Datetime = "1999-10-20" },
                new FilesModel() { Id = 2, Datetime = "2000-10-21" },
            };

            items.Sort(sorter);

            Assert.AreEqual(0, items[0].Id);
            Assert.AreEqual(1, items[1].Id);
            Assert.AreEqual(2, items[2].Id);
        }
    }
}
