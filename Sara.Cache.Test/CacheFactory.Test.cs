using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Sara.Cache.Test
{
    [TestClass]
    public class CacheFactoryUnitTest
    {
        /// <summary>
        /// Test the CacheFactory.CreateCacheData
        /// Verifies a CacheDataController is created
        /// Verifies the CacheController add method is called with any CacheDataController
        /// </summary>
        [TestMethod]
        public void CreateCacheData_TestMethod()
        {
            var mockCacheController = new Mock<ICacheController> { CallBase = true };

            CacheFactory.CreateCacheData(mockCacheController.Object, typeof(TestCacheData));

            mockCacheController.Verify(m => m.Add(It.IsAny<CacheDataController>()), Times.Once());
        }
    }
}
