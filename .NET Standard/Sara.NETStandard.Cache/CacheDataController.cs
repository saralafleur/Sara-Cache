using System;
using System.Threading;

namespace Sara.NETStandard.Cache
{
    /// <summary>
    /// Each ICacheData has a single CacheDataController.  
    /// The CacheDataController manages the ICacheData.
    /// Handles Invalidate and raises the correct events
    /// Handles loading the Data on a background thread with LoadWithCallback
    /// </summary>
    public class CacheDataController
    {
        public ICacheData InternalData { get; set; }

        /// <summary>
        /// Used to retrieve the Key from the CacheData object without forcing a Load
        /// </summary>
        public string InternalKey => InternalData.Key;

        /// <summary>
        /// Used to retreive the Type of the ICacheData object without forcing a Load
        /// </summary>
        public Type InternalType => InternalData.GetType();

        public virtual ICacheData Data
        {
            get
            {
                lock (this)
                {
                    // If the object is not cached, then call Load - Sara
                    // However do not call Load if we are running the Initialize method
                    //if (!IsCached && !_isInitializing)
                    //Load(true);
                    return InternalData;
                }
            }
            set => InternalData = value;
        }

        /// <summary>
        /// Unique Identifier used to identify a CacheDataController Loading event
        /// </summary>
        public int LoadingKey { get; set; }

        // ReSharper disable once InconsistentNaming
        public bool IsCached => InternalData.IsCached;

        public virtual void Clear()
        {
            InternalData.Clear();
            Invalidate();
        }

        /// <summary>
        /// InvalidatePart will only triger the InvalidateNotificationEvent
        /// It will NOT set IsCached_Lazy or IsCached_NonLazy to False
        /// Before calling InvalidatePart you should set a boolean flag within the cache 
        /// object to false.  I.e. IsCached_Values
        /// Then when you call Build on the Cache object, only the part that is no longer Cache is Rebuilt.
        /// </summary>
        public virtual void InvalidatePart()
        {
            if (InvalidateNotificationEvent != null)
                InvalidateNotificationEvent();
        }
        /// <summary>
        /// Invalidates the cached object
        /// Sends invaldation notification to listeners
        /// </summary>
        /// <remarks>
        /// There are 2 goals to Invalidate
        /// 1.) Set IsCached to False, thus anyone that access the data again will have to rebuild
        /// 2.) The UI should be listening to the InvalidationEvent, when tiggered the UI should re-render
        /// </remarks>
        public virtual void Invalidate()
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                InternalData.IsCached = false;
                InvalidateNotificationEvent?.Invoke();
            });
        }

        /// <summary>
        /// Initialize the object for first time use
        /// </summary>
        /// <param name="data"></param>
        public void Initialize(ICacheData data)
        {
            lock (this)
            {
                InternalData = data;
                InternalData.DataController = this;
                InternalData.LoadStatusNotificationEvent += InternalLoadStatusNotification;
            }
        }

        private bool _isLoading;

        public void Load()
        {
            Load(false);
        }
        public virtual void Load(bool internalLoad)
        {
            lock (this)
            {
                // If the data is cached, then don't reload it.  
                // The consumer will have to call invalidate to force the load.  - Sara
                if (IsCached) return;
                try
                {
                    if (_isLoading)
                        return;
                    _isLoading = true;
                    InternalLoadStatusNotification(LoadingStatus.Starting, "Load Started");
                    InternalData.Load(internalLoad);
                    InternalLoadStatusNotification(LoadingStatus.Loaded, "Load Complete");
                }
                finally
                {
                    _isLoading = false;
                }
            }
        }

        public void LoadWithCallback(Action callback)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                lock (this)
                {
                    Load();
                    ((Action)state)();
                }

            }, callback);
        }

        public Action<LoadingStatus, int, string> LoadStatusNotificationEvent { get; set; }
        /// <summary>
        /// Only call this method internally
        /// </summary>
        /// <remarks>
        /// The method is public to support unit testing.  There must be a way to avoid this, but I don't know it. - Sara
        /// </remarks>
        public virtual void InternalLoadStatusNotification(LoadingStatus status, string loadMessage)
        {
            if (LoadStatusNotificationEvent != null)
                LoadStatusNotificationEvent(status, LoadingKey, loadMessage);
        }

        public Action InvalidateNotificationEvent { get; set; }
    }
}
