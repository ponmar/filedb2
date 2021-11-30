using System.Collections.Generic;
using FileDB.Sorters;
using FileDB.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters
{
    [TestClass]
    public class PersonToUpdateSorterTests
    {
        private PersonToUpdateSorter sorter;

        [TestInitialize]
        public void Initialize()
        {
            sorter = new();
        }

        [TestMethod]
        public void Compare()
        {
            var items = new List<PersonToUpdate>()
            {
                new PersonToUpdate() { Id = 0, Name = "aaa" },
                new PersonToUpdate() { Id = 2, Name = "ccc" },
                new PersonToUpdate() { Id = 1, Name = "bbb" },
            };

            items.Sort(sorter);

            Assert.AreEqual(0, items[0].Id);
            Assert.AreEqual(1, items[1].Id);
            Assert.AreEqual(2, items[2].Id);
        }
    }
}
