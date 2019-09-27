using Sara.NETStandard.Common.XML;

namespace Sara.NETStandard.Cache.DataStore
{
    public class XmlCacheDataStore : ICacheDataStore
    {
        public string Path { get; set; }

        public void Save(ListOfICacheData model)
        {
            Serialize.Save(model, Path);
        }

        public ListOfICacheData Load()
        {
            return Serialize.Load<ListOfICacheData>(Path);
        }

    }
}
