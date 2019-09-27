using System.Threading;

namespace Sara.NETStandard.Cache
{
    internal static class Generator
    {
        private static int _loadingKey;

        public static int GetNextLoadingKey => Interlocked.Increment(ref _loadingKey);
    }
}
