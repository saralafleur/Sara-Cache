using System;

namespace Sara.Cache
{
    /// <summary>
    /// Factory for building Cache Data objects
    /// </summary>
    public static class CacheFactory
    {
        /// <summary>
        /// Adds a Cache Data object to the Controller and returns a CacheDataController
        /// </summary>
        /// <param name="controller">Cache Controller that will manage this the Cache Data</param>
        /// <param name="type">Class type that implements ICacheData</param>
        public static CacheDataController CreateCacheData(ICacheController controller, Type type)
        {
            var data = (ICacheData)Activator.CreateInstance(type);
            var dataController = new CacheDataController();
            dataController.Initialize(data);
            controller.Add(dataController);
            return dataController;
        }
    }
}
