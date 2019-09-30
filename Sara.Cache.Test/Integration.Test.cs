using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sara.Cache.DataStore;

namespace Sara.Cache.Test
{
    [TestClass]
    public class IntegrationUniTest
    {
        private const string MODEL_DATA = "Integration Test";

        [TestMethod]
        public void Main()
        {
            ////
            // Test a new application with first Cache element being created.
            ////
            MemoryCacheDataStore dataStore;
            CacheDataController data;
            var cache = PrepareCache(out dataStore, out data);

            ////
            // Test Loading with Load Notification
            ////
            TestLoad(cache, data);

            ////
            // Test Save and Load from DataStore
            ////
            TestLoadWithDataInDataStore(cache, dataStore);

            ////
            // Test Load with callback
            ////
            TestLoadWithCallback(cache, data);

            ////
            // Test Invalidate and then access to Cache Data
            ////
            TestInvalidateAndGetData(cache);
        }

        private static void TestInvalidateAndGetData(CacheController cache)
        {
            var invalidateCallbackRaised = false;
            Action invalidateCallback = () => { invalidateCallbackRaised = true; };
            cache.SetupInvalidateNotification(typeof(TestCacheData), CacheNotificationAction.Add, invalidateCallback);

            cache.Invalidate();

            Assert.IsTrue(invalidateCallbackRaised);

            var loadCallbackRaised = false;
            Action<LoadingStatus, int, string> loadCallback =
                (status, loadingKey, loadingMessage) => { loadCallbackRaised = true; };

            var uid = cache.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Add, loadCallback);

            var cacheDataController = cache.GetData(typeof(TestCacheData)).FirstOrDefault();
            Assert.IsNotNull(cacheDataController);
            var testData = cacheDataController.Data as TestCacheData;
            Assert.IsNotNull(testData);
            Assert.IsTrue(testData.Model == MODEL_DATA, "Data is incorrect");
            // Note: When we accessed cacheDataController.Data, and the data is not cached, this will force a Load
            Assert.IsTrue(loadCallbackRaised, "Load Callback should have been raised");
        }

        private static void TestLoadWithDataInDataStore(ICacheController cache, MemoryCacheDataStore dataStore)
        {
            cache.Save();

            var rawDataStore = dataStore.Storage;

            var dataStore2 = new MemoryCacheDataStore { Storage = rawDataStore };

            var cache2 = new CacheController();
            cache2.SetupDataStore(dataStore2);
            cache2.Load();

            var data2 = cache2.Cache[0].Data as TestCacheData;
            Assert.IsNotNull(data2);
            Assert.IsTrue(data2.Model == MODEL_DATA, "Model text did not match");
        }

        private static CacheController PrepareCache(out MemoryCacheDataStore dataStore, out CacheDataController data)
        {
            var cache = new CacheController();
            dataStore = new MemoryCacheDataStore();
            cache.SetupDataStore(dataStore);

            data = CacheFactory.CreateCacheData(cache, typeof(TestCacheData));
            // The following line of code will force a reload
            var testData = data.Data as TestCacheData;
            Assert.IsNotNull(testData, "Test Data should not be null");
            testData.Model = MODEL_DATA;
            // This will return us to an IsCached = false state - Sara
            cache.Invalidate();

            return cache;
        }

        private static void TestLoad(ICacheController cache, CacheDataController data)
        {

            var loadNotificationCount = 0;
            var startStatusReceived = false;
            var completeStatusRecieved = false;

            Action<LoadingStatus, int, string> callback = (status, loadingKey, loadingMessage) =>
            {
                switch (status)
                {
                    case LoadingStatus.Starting:
                        startStatusReceived = true;
                        break;
                    case LoadingStatus.Loading:
                        break;
                    case LoadingStatus.Loaded:
                        completeStatusRecieved = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("status");
                }
                // ReSharper disable AccessToModifiedClosure
                loadNotificationCount++;
                // ReSharper restore AccessToModifiedClosure
            };
            cache.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                callback);
            data.Load();

            Assert.IsTrue(startStatusReceived, "Start status missing");
            Assert.IsTrue(loadNotificationCount == 2, "There should be 2 LoadNotification messages");
            Assert.IsTrue(completeStatusRecieved, "Complete status missing");

            cache.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Remove, callback);

            loadNotificationCount = 0;
            data.Load();

            Assert.IsTrue(loadNotificationCount == 0, "The LoadNotification should not have been raised");
            // Return to a IsCached = false state
            cache.Invalidate();
        }

        private static void TestLoadWithCallback(ICacheController cache, CacheDataController data)
        {
            var loadNotificationCount = 0;
            var startStatusReceived = false;
            var completeStatusRecieved = false;

            var mre = new ManualResetEvent(false);

            Action<LoadingStatus, int, string> callback = (status, loadingKey, loadingMessage) =>
            {
                switch (status)
                {
                    case LoadingStatus.Starting:
                        startStatusReceived = true;
                        break;
                    case LoadingStatus.Loading:
                        break;
                    case LoadingStatus.Loaded:
                        completeStatusRecieved = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("status");
                }
                // ReSharper disable AccessToModifiedClosure
                loadNotificationCount++;
                // ReSharper restore AccessToModifiedClosure
            };
            cache.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Add,
                callback);
            var callbackRaised = false;
            data.LoadWithCallback(() =>
            {
                callbackRaised = true;
                mre.Set();
            });

            mre.WaitOne(1000);

            Assert.IsTrue(callbackRaised, "Callback was not raised");
            Assert.IsTrue(startStatusReceived, "Start status missing");
            Assert.IsTrue(loadNotificationCount == 2, "There should be 2 LoadNotification messages");
            Assert.IsTrue(completeStatusRecieved, "Complete status missing");

            cache.SetupLoadNotification(typeof(TestCacheData), CacheNotificationAction.Remove, callback);

            // Return to a IsCached = false state
            cache.Invalidate();
        }

    }
}
