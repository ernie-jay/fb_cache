namespace FBCache
{
    public class FbCache
    {
        //Singleton instance
        private static FbCache? instance = null;
        private static readonly object padlock = new object();

        //using LinkedList to update operations
        private LinkedList<(object, object)> cacheList;
        private Dictionary<object, LinkedListNode<(object, object)>> cacheSet;
        private int capacity = 1000;
        public int Capacity {
            get
            {
                return capacity;
            }
            set
            {
                if (count < value)
                {
                    cacheSet.EnsureCapacity(value);
                    
                }else if (count > value)
                {
                    var trimmedList = cacheList.Take(value).ToList();
                    LinkedList<(object, object)> newCacheList = new LinkedList<(object, object)> (trimmedList);
                    Dictionary<object, LinkedListNode<(object, object)>> newCacheSet = new Dictionary<object, LinkedListNode<(object, object)>>(value);
                    for (var node = newCacheList.First; node != null; node = node.Next)
                    {
                        newCacheSet.Add(node.Value.Item1, node);
                    }
                    cacheList = newCacheList;
                    cacheSet = newCacheSet;
                    count = value;
                }
                capacity = value;
            }
        }
        private int count;

        private FbCache()
        {
            cacheList = new LinkedList<(object, object)>();
            cacheSet = new Dictionary<object, LinkedListNode<(object,object)>>(capacity);
            count = 0;
        }

        public static FbCache Instance
        {
            get
            {
                if (instance == null)
                { 
                    lock (padlock)
                    {
                        instance ??= new FbCache();
                    }
                }
                return instance;
            }
        }

        public V? GetData<K, V>(K key)
        {
            if (key == null) throw new ArgumentNullException("key");
            lock (this)
            {
                var backupList = new LinkedList<(object, object)>(cacheList);
                //if no element in the cache, then throw exception
                if (!cacheSet.ContainsKey(key)) throw new KeyNotFoundException();
                try
                {
                    var node = cacheSet[key];
                    cacheList.Remove(node);
                    cacheList.AddFirst(node);
                    return (V)node.Value.Item2;
                }catch
                {
                    cacheList = backupList;
                    throw;
                }
            }
        }
        public bool SetData<K, V>(K key, V data) 
        {
            if (key == null) throw new ArgumentNullException("key");
            if (data == null) throw new ArgumentNullException("data");
            lock (this)
            {
                var backupList = new LinkedList<(object, object)>(cacheList);
                var backupSet = new Dictionary<object, LinkedListNode<(object, object)>>(cacheSet);
                object itemRemoved = null;
                try
                {
                    //if there is a cache element with the same key, then remove it
                    if (cacheSet.ContainsKey(key))
                    {
                        cacheList.Remove(cacheSet[key]);
                        cacheSet.Remove(key);
                        count--;
                    }

                    //if the cache is at capacity, remove the last item
                    if (count == capacity)
                    {
                        var lastNode = cacheList.Last;
                        itemRemoved = lastNode.Value.Item1;
                        cacheList.RemoveLast();
                        cacheSet.Remove(lastNode.Value.Item1);
                        count--;
                    }

                    var node = cacheList.AddFirst((key, data));
                    cacheSet.Add(key, node);
                    count++;

                }catch
                {
                    cacheList = backupList;
                    cacheSet = backupSet;
                    throw;
                }
            }

            return true;
        }
        public bool RemoveData<K, V>(K key) 
        {
            if (key == null) return false;
            if (!cacheSet.ContainsKey(key)) return false;
            lock(this)
            {
                var backupList = new LinkedList<(object, object)>(cacheList);
                var backupSet = new Dictionary<object, LinkedListNode<(object, object)>>(cacheSet);
                try
                {
                    cacheList.Remove(cacheSet[key]);
                    cacheSet.Remove(key);
                    count--;
                    return true;
                }
                catch {
                    cacheList = backupList;
                    cacheSet = backupSet;
                    throw; 
                }
            }
        }
        public bool Dispose() 
        {
            lock (this)
            {
                var backupList = new LinkedList<(object, object)>(cacheList);
                var backupSet = new Dictionary<object, LinkedListNode<(object, object)>>(cacheSet);
                try
                {
                    cacheList = new LinkedList<(object, object)>();
                    cacheSet = new Dictionary<object, LinkedListNode<(object, object)>>(capacity);
                    cacheSet.EnsureCapacity(capacity);
                    count = 0;
                }catch
                {
                    cacheList = backupList;
                    cacheSet = backupSet;
                    throw;
                }
                return true;
            }
        }
    }
}