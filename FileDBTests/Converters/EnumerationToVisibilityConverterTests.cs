using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using FileDB.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Converters
{
    [TestClass]
    public class EnumerationToVisibilityConverterTests
    {
        [TestMethod]
        public void Convert_Null_Collapsed()
        {
            var converter = new EnumerationToVisibilityConverter();
            List<string> items = null;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(items, typeof(IEnumerable), null, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Convert_Empty_Collapsed()
        {
            var converter = new EnumerationToVisibilityConverter();
            var items = new List<string>();
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(items, typeof(IEnumerable), null, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Convert_HasContent_Visible()
        {
            var converter = new EnumerationToVisibilityConverter();
            var items = new List<string>() { "item" };
            Assert.AreEqual(Visibility.Visible, converter.Convert(items, typeof(IEnumerable), null, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void Convert_HasObservableCollection_Visible()
        {
            var converter = new EnumerationToVisibilityConverter();
            var items = new ObservableCollection<string>() { "item" };
            Assert.AreEqual(Visibility.Visible, converter.Convert(items, typeof(IEnumerable), null, CultureInfo.InvariantCulture));
        }
    }
}
