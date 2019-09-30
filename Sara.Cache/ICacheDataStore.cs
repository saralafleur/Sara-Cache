namespace Sara.Cache
{
    public interface ICacheDataStore
    {
        void Save(ListOfICacheData model);
        ListOfICacheData Load();
    }
}
