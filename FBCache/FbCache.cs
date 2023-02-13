namespace FBCache
{
    public class FbCache
    {
        private static FbCache? instance = null;

        private FbCache()
        {
        }

        public static FbCache GetInstance()
        {
            if (instance == null)
            {
                instance = new FbCache();
            }
            return instance;
        }

        public V? GetData<K, V>(K key) { throw new NotImplementedException("method is not implemented"); }
        public bool SetData<K, V>(K key, V data) { throw new NotImplementedException("method is not implemented"); }
        public V? RemoveData<K, V>(K key) { throw new NotImplementedException("method is not implemented"); }
        public bool Dispose() { throw new NotImplementedException("method is not implemented"); }
    }
}