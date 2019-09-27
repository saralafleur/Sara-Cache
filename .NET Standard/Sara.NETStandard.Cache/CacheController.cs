using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sara.NETStandard.Cache
{
    public class CacheController : ICacheController
    {
        public virtual List<CacheDataController> Cache { get; set; }
        public bool Empty => Cache.Count == 0;

        /// <summary>
        /// Returns True if the Cache does not contain the provided Type.
        /// </summary>
        public bool EmptyByType(Type type)
        {
            return Cache.All(item => item.InternalType != type);
        }
        /// <summary>
        /// Returns True if the Cache does not contain the provided Type.
        /// </summary>
        public bool EmptyByTypeAndKey(Type type, string key)
        {
            return !Cache.Any(item => item.InternalType == type && item.InternalKey == key);
        }

        private ICacheDataStore DataStore { get; set; }
        public CacheController()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Cache = new List<CacheDataController>();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public virtual void Save()
        {
            lock (this)
            {
                if (DataStore == null)
                    throw new Exception("DataStore is undefined.  Call SetupDataStore to resolve.");

                var model = new ListOfICacheData();
                model.AddRange(Cache.Select(cacheDataController => cacheDataController.InternalData));

                DataStore.Save(model);
            }
        }

        public virtual void Load()
        {
            if (DataStore == null)
                throw new Exception("DataStore is undefined.  Call SetupDataStore to resolve.");

            InternalLoadDataStore();
            foreach (var cacheDataController in Cache)
            {
                cacheDataController.Load();
            }
        }

        private void InternalLoadDataStore()
        {
            var model = DataStore.Load();
            foreach (var data in model)
            {
                var cacheDataController = new CacheDataController();
                cacheDataController.Initialize(data);
                Cache.Add(cacheDataController);
            }
        }
        public void LoadWithCallback(Action callback)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                InternalLoadDataStore();
                var dataCount = Cache.Count;
                var loadCount = 0;

                foreach (var item in Cache)
                {
                    item.LoadWithCallback(delegate
                    {
                        lock (callback)
                        {
                            loadCount++;
                        }
                    });
                }

                // TODO: Add a better wait here, I'm on a plance, so I made this quick - Sara
                while (dataCount != loadCount)
                    Thread.Sleep(500);

                ((Action)state)();

            }, callback);
        }

        public void Clear()
        {
            foreach (var item in Cache)
            {
                item.Clear();
            }
        }

        public void Invalidate()
        {
            foreach (var item in Cache)
            {
                item.Invalidate();
            }
        }

        public void InvalidateNotification()
        {
            throw new NotImplementedException();
        }

        public void Add(CacheDataController controller)
        {
            Cache.Add(controller);
        }

        public void Remove(CacheDataController controller)
        {
            Cache.Remove(controller);
        }

        /// <summary>
        /// Defines the DataStore used by the Cache
        /// </summary>
        /// <param name="dataStore"></param>
        public void SetupDataStore(ICacheDataStore dataStore)
        {
            DataStore = dataStore;
        }

        /// <summary>
        /// Subscribe/Unsubscribe to the LoadNotification by Class Type
        /// </summary>
        /// <param name="type">Looks for any ICacheData classes that match the provided Class Type</param>
        /// <param name="action">Specifies the action to be taken</param>
        /// <param name="callback"></param>
        /// <returns>Returns a unique Loading Key that will be use to identify when a IDataCache is being loaded</returns>
        public int? SetupLoadNotification(Type type, CacheNotificationAction action, Action<LoadingStatus, int, string> callback)
        {
            foreach (var cacheDataController in Cache.Where(cacheDataController => cacheDataController.InternalData.GetType() == type))
            {
                return SetupLoadNotification(action, callback, cacheDataController);
            }
            return null;
        }

        /// <summary>
        /// Subscribe/Unsubscribe to the LoadNotification by dataKey
        /// </summary>
        /// <param name="action">Specifies the action to be taken</param>
        /// <param name="callback"></param>
        /// <param name="cacheDataController"></param>
        /// <returns>Returns a unique Loading Key that will be use to identify when a IDataCache is being loaded</returns>
        private int? SetupLoadNotification(CacheNotificationAction action, Action<LoadingStatus, int, string> callback,
            CacheDataController cacheDataController)
        {
            switch (action)
            {
                case CacheNotificationAction.Add:
                    cacheDataController.LoadingKey = Generator.GetNextLoadingKey;
                    cacheDataController.LoadStatusNotificationEvent += callback;
                    return cacheDataController.LoadingKey;
                case CacheNotificationAction.Remove:
                    // ReSharper disable DelegateSubtraction
                    cacheDataController.LoadStatusNotificationEvent -= callback;
                    // ReSharper restore DelegateSubtraction
                    return null;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }
        }

        public int? SetupLoadNotification(string key, CacheNotificationAction action, Action<LoadingStatus, int, string> callback)
        {
            foreach (var cacheDataController in Cache.Where(cacheDataController => cacheDataController.InternalData.Key == key))
            {
                return SetupLoadNotification(action, callback, cacheDataController);
            }
            return null;
        }

        public void SetupInvalidateNotification(Type type, CacheNotificationAction action, Action callback)
        {
            foreach (var cacheDataController in Cache.Where(cacheDataController => cacheDataController.InternalData.GetType() == type))
            {
                switch (action)
                {
                    case CacheNotificationAction.Add:
                        cacheDataController.InvalidateNotificationEvent += callback;
                        break;
                    case CacheNotificationAction.Remove:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("action");
                }
            }
        }

        public void SetupInvalidateNotification(string key, CacheNotificationAction action, Action callback)
        {
            foreach (var cacheDataController in Cache.Where(cacheDataController => cacheDataController.InternalData.Key == key))
            {
                switch (action)
                {
                    case CacheNotificationAction.Add:
                        cacheDataController.InvalidateNotificationEvent += callback;
                        break;
                    case CacheNotificationAction.Remove:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("action");
                }
            }
        }

        public List<CacheDataController> GetData(Type type)
        {
            return Cache.Where(cacheDataController => cacheDataController.InternalData.GetType() == type).ToList();
        }

        public List<CacheDataController> GetData(string key)
        {
            return Cache.Where(cacheDataController => cacheDataController.InternalData.Key == key).ToList();
        }
        public List<CacheDataController> GetData(string key, Type type)
        {
            return Cache.Where(cacheDataController => cacheDataController.InternalData.Key == key && cacheDataController.InternalData.GetType() == type).ToList();
        }
    }
}
