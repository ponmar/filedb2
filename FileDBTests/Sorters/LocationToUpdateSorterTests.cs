using System.Collections.Generic;
using FileDB.Sorters;
using FileDB.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Sorters
{
    [TestClass]
    public class LocationToUpdateSorterTests
    {
        private LocationToUpdateSorter sorter;

        [TestInitialize]
        public void Initialize()
        {
            sorter = new();
        }

        [TestMethod]
        public void Compare()
        {
            var items = new List<LocationToUpdate>()
            {
                new LocationToUpdate() { Id = 0, Name = "aaa" },
                new LocationToUpdate() { Id = 2, Name = "ccc" },
                new LocationToUpdate() { Id = 1, Name = "bbb" },
            };

            items.Sort(sorter);

            Assert.AreEqual(0, items[0].Id);
            Assert.AreEqual(1, items[1].Id);
            Assert.AreEqual(2, items[2].Id);
        }
    }
}
