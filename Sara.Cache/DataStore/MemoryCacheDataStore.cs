using Sara.NETStandard.Common.Extension;

namespace Sara.Cache.DataStore
{
    public class MemoryCacheDataStore : ICacheDataStore
    {
        public string Storage { get; set; }
        public void Save(ListOfICacheData model)
        {
            Storage = model.SerializeObject();
        }

        public ListOfICacheData Load()
        {
            return Storage.XmlDeserializeFromString<ListOfICacheData>();
        }
    }
}
