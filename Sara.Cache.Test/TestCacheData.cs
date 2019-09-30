using System;
using System.Xml.Serialization;

namespace Sara.Cache.Test
{
    /// <summary>
    /// Test Cache Data class
    /// </summary>
    /// <remarks>
    /// CacheFactory.CreateCacheData expects an object that has an empty constructor.
    /// If I try to mock a ICacheData, the proxy class does not have an empty constructor, thus why I have this object here. - Sara
    /// </remarks>
    [XmlType("TestCacheData")]
    public class TestCacheData : ICacheData
    {
        /// <summary>
        /// This is the data for the class.
        /// </summary>
        public string Model { get; set; }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool IsCached { get; set; }

        [XmlIgnore]
        public Action<LoadingStatus, string> LoadStatusNotificationEvent { get; set; }
        [XmlIgnore]
        public Action InvalidateNotificationEvent { get; set; }

        public string Key { get; set; }
        public CacheDataController DataController { get; set; }
        public void Load(bool internalLoad)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            // Nothing to calculate or process - Sara
            IsCached = true;
        }
    }
}
