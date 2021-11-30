using System.Collections.Generic;
using FileDB.Sorters;
using FileDB.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters
{
    [TestClass]
    public class TagToUpdateSorterTests
    {
        private TagToUpdateSorter sorter;

        [TestInitialize]
        public void Initialize()
        {
            sorter = new();
        }

        [TestMethod]
        public void Compare()
        {
            var items = new List<TagToUpdate>()
            {
                new TagToUpdate() { Id = 0, Name = "aaa" },
                new TagToUpdate() { Id = 2, Name = "ccc" },
                new TagToUpdate() { Id = 1, Name = "bbb" },
            };

            items.Sort(sorter);

            Assert.AreEqual(0, items[0].Id);
            Assert.AreEqual(1, items[1].Id);
            Assert.AreEqual(2, items[2].Id);
        }
    }
}
