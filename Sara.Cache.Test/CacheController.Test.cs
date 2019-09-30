using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sara.Cache.DataStore;

namespace Sara.Cache.Test
{
    [TestClass]
    public class CacheControllerUnitTest
    {
        /// <summary>
        /// Test the CacheController.Save method
        /// Prepare the Cache with a ICacheData object
        /// Save the Cache
        /// Verify the Cache was saved to the Datastore
        /// </summary>
        [TestMethod]
        public void Save_TestMethod()
        {
            var mockCacheController = new Mock<CacheController> { CallBase = true };
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var cacheData = new TestCacheData { Model = MODEL_STRING };
            mockCacheDataController.Object.Initialize(cacheData);
            mockCacheController.Object.Add(mockCacheDataController.Object);

            var dataStore = new MemoryCacheDataStore();
            mockCacheController.Object.SetupDataStore(dataStore);
            mockCacheController.Object.Save();

            Assert.IsTrue(dataStore.Storage.Contains(MODEL_STRING));
        }

        private const string MODEL_STRING = "This is unique data...";
        private const string STORAGE = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ListOfICacheData>
  <ICacheData AssemblyQualifiedName=""Sara.Cache.Test.TestCacheData, Sara.Cache.Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
    <TestCacheData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
      <Model>This is unique data...</Model>
      <IsCached>false</IsCached>
    </TestCacheData>
  </ICacheData>
</ListOfICacheData>";

        /// <summary>
        /// Test the CacheController.Load method
        /// Prepare a Datastore
        /// Load the Datastore by calling Load
        /// Verify the Cache was properly loaded from the Datastore
        /// </summary>
        [TestMethod]
        public void Load_TestMethod()
        {
            var mockCacheController = new Mock<CacheController> { CallBase = true };

            var dataStore = new MemoryCacheDataStore { Storage = STORAGE };
            mockCacheController.Object.SetupDataStore(dataStore);
            mockCacheController.Object.Load();

            var data = mockCacheController.Object.Cache[0].Data as TestCacheData;
            Assert.IsNotNull(data, "ICacheData was not loaded properly from the xml");
            Assert.IsTrue(data.Model == MODEL_STRING, "Model data was not loaded properly from the xml");
        }
        /// <summary>
        /// Test the CacheController.LoadWithCallback method
        /// Prepare a Datastore
        /// Load the Datastore by calling LoadWithCallback
        /// Verify the calling thread can continue while the background thread loads the Data
        /// Verify the callback event is raised
        /// Verify the Cache was properly loaded from the Datastore
        /// </summary>
        [TestMethod]
        public void LoadWithCallback_TestMethod()
        {
            var mockCacheController = new Mock<CacheController> { CallBase = true };

            var dataStore = new MemoryCacheDataStore { Storage = STORAGE };
            mockCacheController.Object.SetupDataStore(dataStore);
            var callbackFired = false;
            mockCacheController.Object.LoadWithCallback(delegate
            {
                callbackFired = true;
            });

            // TODO: Add a reset event here - Sara
            while (!callbackFired)
                Thread.Sleep(500);

            var data = mockCacheController.Object.Cache[0].Data as TestCacheData;
            Assert.IsNotNull(data, "ICacheData was not loaded properly from the xml");
            Assert.IsTrue(data.Model == MODEL_STRING, "Model data was not loaded properly from the xml");
        }

        [TestMethod]
        public void GetDataByType_TestMethod()
        {
            ICacheController cacheController = new CacheController();
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            const string modelText = "GetDataByType";
            var cacheData = new TestCacheData { Model = modelText };
            mockCacheDataController.Object.Initialize(cacheData);

            cacheController.Add(mockCacheDataController.Object);
            var result = cacheController.GetData(typeof(TestCacheData)).First();

            var model = result.Data as TestCacheData;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.IsTrue(model.Model.Contains(modelText));

        }
        [TestMethod]
        public void GetDataByKey_TestMethod()
        {
            ICacheController cacheController = new CacheController();
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            const string keyText = "TestKey";
            const string modelText = "GetDataByType";
            var cacheData = new TestCacheData { Model = modelText, Key = keyText };
            mockCacheDataController.Object.Initialize(cacheData);

            cacheController.Add(mockCacheDataController.Object);
            var result = cacheController.GetData(keyText).First();

            var model = result.Data as TestCacheData;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.IsTrue(model.Model.Contains(modelText));
        }
        /// <summary>
        /// Test the CacheController.SetupNotification method by Type
        /// Verify that we can subscribe to the notifications by Type
        /// </summary>
        [TestMethod]
        public void SetupNotificationByType_TestMethod()
        {
            ICacheController cacheController = new CacheController();
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var cacheData = new TestCacheData();
            mockCacheDataController.Object.Initialize(cacheData);

            cacheController.Add(mockCacheDataController.Object);
            var loadStatusRaised = false;
            cacheController.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                delegate
                {
                    loadStatusRaised = true;
                });
            var invalidateRaised = false;
            cacheController.SetupInvalidateNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                delegate
                {
                    invalidateRaised = true;
                });

            mockCacheDataController.Object.Load();
            mockCacheDataController.Object.Invalidate();

            Assert.IsTrue(loadStatusRaised, "LoadStatus event was not raised");
            Assert.IsTrue(invalidateRaised, "Invalidate event was not raised");
        }
        /// <summary>
        /// Test the CacheController.SetupNotification method by Key
        /// Verify that we can subscribe to the notifications by Key
        /// </summary>
        [TestMethod]
        public void SetupNotificationByKey_TestMethod()
        {
            ICacheController cacheController = new CacheController();
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            var cacheData = new TestCacheData { Key = "SetupNotificationBykey" };
            mockCacheDataController.Object.Initialize(cacheData);

            cacheController.Add(mockCacheDataController.Object);
            var loadStatusRaised = false;
            cacheController.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                delegate
                {
                    loadStatusRaised = true;
                });
            var invalidateRaised = false;
            cacheController.SetupInvalidateNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                delegate
                {
                    invalidateRaised = true;
                });

            mockCacheDataController.Object.Load();
            mockCacheDataController.Object.Invalidate();

            Assert.IsTrue(loadStatusRaised, "LoadStatus event was not raised");
            Assert.IsTrue(invalidateRaised, "Invalidate event was not raised");
        }

        /// <summary>
        /// Test the CacheController.Clear method
        /// Verifies ICacheData.Clear is fired
        /// </summary>
        [TestMethod]
        public void Clear_TestMethod()
        {
            ICacheController cacheController = new CacheController();
            var mockCacheDataController = new Mock<CacheDataController> { CallBase = true };
            mockCacheDataController.Object.Initialize(new Mock<ICacheData>().Object);

            cacheController.Add(mockCacheDataController.Object);
            cacheController.Clear();

            mockCacheDataController.Verify(m => m.Clear(), Times.Once());
        }

        [TestMethod]
        public void Add_TestMethod()
        {
            ICacheController cacheController = new CacheController();

            cacheController.Add(new CacheDataController());

            Assert.IsTrue(cacheController.Cache.Count == 1);
        }
        [TestMethod]
        public void Remove_TestMethod()
        {
            ICacheController cacheController = new CacheController();

            var cacheDataController = new CacheDataController();

            cacheController.Add(cacheDataController);
            Assert.IsTrue(cacheController.Cache.Count == 1);

            cacheController.Remove(cacheDataController);
            Assert.IsTrue(cacheController.Cache.Count == 0);
        }
    }
}
