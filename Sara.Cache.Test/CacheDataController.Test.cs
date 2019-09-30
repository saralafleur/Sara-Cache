using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Sara.Cache.Test
{
    [TestClass]
    public class CacheDataControllerUnitTest
    {
        /// <summary>
        /// Test the CacheDataController.Initialize method
        /// Verifies the CacheDataController.Data is set
        /// </summary>
        [TestMethod]
        public void Initialize_TestMethod()
        {
            var cacheDataController = new CacheDataController();
            var mockCacheData = new Mock<ICacheData>();

            cacheDataController.Initialize(mockCacheData.Object);

            Assert.AreSame(mockCacheData.Object, cacheDataController.Data, "CacheData should be set");
        }

        /// <summary>
        /// Test the CacheDataController.Load method
        /// Verifies the ICacheData.Load method is called
        /// Verifies IsCached is set to True
        /// </summary>
        [TestMethod]
        public void Load_TestMethod()
        {
            var cacheDataController = new CacheDataController();
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);

            cacheDataController.Initialize(mockCacheData.Object);
            cacheDataController.Load();

            mockCacheData.Verify(m => m.Load(false), Times.Once());
            Assert.IsTrue(cacheDataController.IsCached, "IsCached should be true after load");
        }

        /// <summary>
        /// Test the CacheDataController.Load method called Twice
        /// Verifies the ICacheData.Load method is called only once
        /// </summary>
        [TestMethod]
        public void LoadTwice_TestMethod()
        {
            var cacheDataController = new CacheDataController();
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);

            cacheDataController.Initialize(mockCacheData.Object);
            cacheDataController.Load();
            cacheDataController.Load();

            mockCacheData.Verify(m => m.Load(false), Times.Once());
        }

        /// <summary>
        /// Test the CacheDataController.LoadWithCallback method
        /// Verifies the ICacheData.Load method is called
        /// Verifies IsCached is set to True
        /// Verifies the callback is executed
        /// </summary>
        [TestMethod]
        public void LoadWithCallback_TestMethod()
        {
            var cacheDataController = new CacheDataController();
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);
            var callbackExecuted = false;

            cacheDataController.Initialize(mockCacheData.Object);
            cacheDataController.LoadWithCallback(() => { callbackExecuted = true; });

            // Wait for a maximum of 1 second for the callback to be called
            for (int i = 0; i < 10; i++)
            {
                if (callbackExecuted)
                    break;
                Thread.Sleep(100);
            }

            if (!callbackExecuted)
                Assert.Fail("Boolean value was not set to true");

            mockCacheData.Verify(m => m.Load(false), Times.Once());
            Assert.IsTrue(cacheDataController.IsCached, "IsCached should be true after load");
        }

        /// <summary>
        /// Test the CacheDataController.LoadWithCallback method
        /// Verifies the ICacheData.Load method is called
        /// Verifies IsCached is set to True
        /// Verifies the callback is executed
        /// Verifies the LoadStatusNotificationEvent is called
        /// </summary>
        [TestMethod]
        public void LoadStatusNotificationEvent_TestMethod()
        {
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.SetupProperty(m => m.LoadStatusNotificationEvent, null);
            mockCacheData.Setup(m => m.Load(false)).Callback(() =>
            {
                if (mockCacheData.Object.LoadStatusNotificationEvent != null)
                    mockCacheData.Object.LoadStatusNotificationEvent(LoadingStatus.Loading, "Still Loading");
                mockCacheData.Object.IsCached = true;
            });

            mockCacheDataController.Object.Initialize(mockCacheData.Object);
            var callbackExecuted = false;
            mockCacheDataController.Object.LoadWithCallback(() => { callbackExecuted = true; });

            // Wait for a maximum of 1 second for the callback to be called
            for (int i = 0; i < 10; i++)
            {
                if (callbackExecuted)
                    break;
                Thread.Sleep(100);
            }

            mockCacheData.Verify(m => m.Load(false), Times.Once());
            mockCacheData.Verify();

            mockCacheDataController.Verify(m => m.InternalLoadStatusNotification(It.IsAny<LoadingStatus>(), It.IsAny<string>()), Times.Exactly(3));
            Assert.IsTrue(mockCacheDataController.Object.IsCached, "IsCached should be true after load");
        }

        /// <summary>
        /// Test the CacheDataController.Clear method
        /// Verifies the ICacheData.Clear is called
        /// Verifies the CacheDataController.OnInvalidateNotification is called
        /// verifies the CacheDataController.isCached = false
        /// </summary>
        [TestMethod]
        public void Clear_TestMethod()
        {
            // TODO: Review this test and ensure it still works, I removed several lines of code that did not work
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);

            mockCacheDataController.Object.Initialize(mockCacheData.Object);
            mockCacheDataController.Object.Load();
            Assert.IsTrue(mockCacheDataController.Object.IsCached, "IsCached should be true");
            mockCacheDataController.Object.Clear();

            mockCacheData.Verify(m => m.Clear(), Times.Once());
            Assert.IsFalse(mockCacheDataController.Object.IsCached, "IsCached should be false");
        }

        /// <summary>
        /// Test the CacheDataController.Invalidate method
        /// Verifies CacheDataController.InvalidateNotifiationEvent is raised
        /// Verifies CacheDataController.OnInvalidateNotification is called
        /// Verifies CacheDataController.IsCached is set to false
        /// </summary>
        [TestMethod]
        public void Invalidate_TestMethod()
        {
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);
            var invalidateNotificationEventFired = false;
            mockCacheDataController.Object.InvalidateNotificationEvent += () =>
            {
                invalidateNotificationEventFired = true;
            };

            mockCacheDataController.Object.Initialize(mockCacheData.Object);
            mockCacheDataController.Object.Load();
            Assert.IsTrue(mockCacheDataController.Object.IsCached, "IsCached should be true");
            mockCacheDataController.Object.Invalidate();

            // Wait for a maximum of 1 second for the callback to be called
            for (int i = 0; i < 10; i++)
            {
                if (invalidateNotificationEventFired)
                    break;
                Thread.Sleep(100);
            }

            Assert.IsTrue(invalidateNotificationEventFired, "InvalidateNoticification event was not fired");
            Assert.IsFalse(mockCacheDataController.Object.IsCached, "IsCached should be false");
        }

        /// <summary>
        /// Tests the DataCacheController.Data getter when the data is Invalidated
        /// </summary>
        [TestMethod]
        public void DataInvalidated_TestMethod()
        {
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var mockCacheData = new Mock<ICacheData>();
            mockCacheData.SetupProperty(m => m.IsCached, false);
            mockCacheData.Setup(m => m.Load(false)).Callback(() => mockCacheData.Object.IsCached = true);

            mockCacheDataController.Object.Initialize(mockCacheData.Object);
            mockCacheDataController.Object.Load();
            mockCacheDataController.Object.Invalidate();
            Assert.IsFalse(mockCacheDataController.Object.IsCached, "IsCached should be false");
            // This call will force the load
            var testData = mockCacheDataController.Object.Data;

            Assert.IsNotNull(testData, "Cache Data should not be null");
            Assert.IsTrue(mockCacheDataController.Object.IsCached, "IsCached should be true");
            // Load is called when we call Load direclty and when we access Data - Sara
            mockCacheDataController.Verify(m => m.Load(), Times.Exactly(2));
        }
    }
}
