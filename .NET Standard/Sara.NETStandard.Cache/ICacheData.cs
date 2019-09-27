using System;
using System.Xml.Serialization;

namespace Sara.NETStandard.Cache
{
    public enum LoadingStatus
    {
        /// <summary>
        /// Notifies consumers when a Loading action has started
        /// </summary>
        Starting,
        Loading,
        /// <summary>
        /// Notifies consumers when a Loading action has completed
        /// </summary>
        Loaded
    }
    public interface ICacheData
    {
        /// <summary>
        /// Clear all data and Invalidate the Cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns true when the data is cached/ready
        /// </summary>
        // ReSharper disable once InconsistentNaming
        bool IsCached { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <BusinessRule>
        /// When loading starts, LoadingStatus.Starting must be sent
        /// When loading stops, LoadingStatus.Complete must be sent
        /// </BusinessRule>
        [XmlIgnore]
        Action<LoadingStatus, string> LoadStatusNotificationEvent { get; set; }

        /// <summary>
        /// Key is used to define a unique set of data or to group a set of data together
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// The DataController that manages this Cache Data
        /// </summary>
        [XmlIgnore]
        CacheDataController DataController { get; set; }

        /// <summary>
        /// Performs any logic required when loading the data
        /// Blocking call
        /// </summary>
        void Load(bool internalLoad);
    }
}
