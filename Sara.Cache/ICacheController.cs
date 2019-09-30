using System;
using System.Collections.Generic;

namespace Sara.Cache
{
    public enum CacheNotificationAction
    {
        Add,
        Remove
    }

    /// <summary>
    /// Manages a collection of Cache Data
    /// </summary>
    public interface ICacheController
    {
        List<CacheDataController> Cache { get; set; }

        /// <summary>
        /// Returns True if there is no Data in the Cache
        /// </summary>
        bool Empty { get; }

        void Save();

        /// <summary>
        /// Loads the Cache from a DataStore
        /// Blocking Call
        /// </summary>
        void Load();

        /// <summary>
        /// Loads the Cache from a DataStore
        /// Non-Blocking
        /// </summary>
        void LoadWithCallback(Action callback);

        /// <summary>
        /// Clears the Cache from the Datastore
        /// Invalidates Cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Flags the Cache as invalid
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Notifies the consumer the Cache has become invalid
        /// </summary>
        void InvalidateNotification();

        void Add(CacheDataController controller);
        void Remove(CacheDataController controller);

        /// <summary>
        /// Defines the DataStore for the Cache
        /// </summary>
        void SetupDataStore(ICacheDataStore dataStore);

        /// <summary>
        /// Subscribe/Unsubscribe to the LoadNotification by Class Type
        /// </summary>
        /// <param name="type">Looks for any ICacheData classes that match the provided Class Type</param>
        /// <param name="action">Specifies the action to be taken</param>
        /// <param name="callback"></param>
        /// <returns>Returns a unique Loading Key that will be use to identify when a IDataCache is being loaded</returns>
        int? SetupLoadNotification(Type type, CacheNotificationAction action,
            Action<LoadingStatus, int, string> callback);

        /// <summary>
        /// Subscribe/Unsubscribe to the LoadNotification by dataKey
        /// </summary>
        /// <param name="dataKey">Looks for any ICacheData that match the provided dataKey</param>
        /// <param name="action">Specifies the action to be taken</param>
        /// <param name="callback"></param>
        /// <returns>Returns a unique Loading Key that will be use to identify when a IDataCache is being loaded</returns>
        int? SetupLoadNotification(string dataKey, CacheNotificationAction action,
            Action<LoadingStatus, int, string> callback);

        void SetupInvalidateNotification(Type type, CacheNotificationAction action, Action callback);
        void SetupInvalidateNotification(string key, CacheNotificationAction action, Action callback);
        List<CacheDataController> GetData(Type type);
        List<CacheDataController> GetData(string key);
    }
}
