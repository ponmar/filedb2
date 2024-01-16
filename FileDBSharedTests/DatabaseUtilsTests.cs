using FileDBShared;

namespace FileDBSharedTests
{
    [TestClass]
    public class DatabaseUtilsTests
    {
        [TestMethod]
        public void CalculateDistance()
        {
            Assert.AreEqual(103.97482585426138, LatLonUtils.CalculateDistance(58.72018309972223, 14.577619799999999, 58.7211136, 14.5774585));
        }
    }
}
