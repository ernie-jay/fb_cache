namespace FBCache
{
    public class FbCache
    {
        //Singleton instance
        private static FbCache? instance = null;
        private static readonly object padlock = new object();

        //using LinkedList to update operations
        private LinkedList<(object, object)> cacheList;

        //using Dictionary for get operations
        private Dictionary<object, LinkedListNode<(object, object)>> cacheSet;
        public int Capacity { get; private set; } = 1000;

        private int count;

        private FbCache()
        {
            cacheList = new LinkedList<(object, object)>();
            cacheSet = new Dictionary<object, LinkedListNode<(object,object)>>(Capacity);
            cacheSet.EnsureCapacity(Capacity);
            count = 0;
        }

        //Singleton pattern
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

        /// <summary>
        /// Get the item from the cache
        /// </summary>
        /// <typeparam name="K">Type of the key</typeparam>
        /// <typeparam name="V">Type of the data</typeparam>
        /// <param name="key">Unique Key parameter</param>
        /// <returns>The data element from the cache</returns>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        /// <exception cref="KeyNotFoundException">The key doesn't exist in the cache</exception>
        public V? GetData<K, V>(K key)
        {
            if (key == null) throw new ArgumentNullException("key");
            lock (this)
            {
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
                    throw;
                }
            }
        }

        /// <summary>
        /// Add data to cache
        /// </summary>
        /// <typeparam name="K">Type of the key</typeparam>
        /// <typeparam name="V">Type of the data</typeparam>
        /// <param name="key">Unique Key parameter. If exists, the value will be overwritten</param>
        /// <param name="data">The data item to store in cache</param>
        /// <returns>(bool, object) touple: Item1:</returns>
        /// <exception cref="ArgumentNullException">key or data is null</exception>
        public object? SetData<K, V>(K key, V data) 
        {
            if (key == null) throw new ArgumentNullException("key");
            if (data == null) throw new ArgumentNullException("data");
            lock (this)
            {
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
                    else if (count == Capacity) //if the cache is at capacity, remove the last item
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
                    throw;
                }
                return itemRemoved;
            }
        }

        /// <summary>
        /// Remove a specific element from the cache
        /// </summary>
        /// <typeparam name="K">Type of the key</typeparam>
        /// <typeparam name="V">Type of the data</typeparam>
        /// <param name="key">Unique key of the data</param>
        /// <returns>True if data removed</returns>
        public bool RemoveData<K, V>(K key) 
        {
            if (key == null) return false;
            if (!cacheSet.ContainsKey(key)) return false;
            lock(this)
            {
                try
                {
                    cacheList.Remove(cacheSet[key]);
                    cacheSet.Remove(key);
                    count--;
                    return true;
                }
                catch {
                    throw; 
                }
            }
        }
        
        /// <summary>
        /// Clean up the cache and reinitialise the internal storage
        /// </summary>
        /// <returns>True if success</returns>
        public bool Dispose() 
        {
            lock (this)
            {
                try
                {
                    cacheList = new LinkedList<(object, object)>();
                    cacheSet = new Dictionary<object, LinkedListNode<(object, object)>>(Capacity);
                    cacheSet.EnsureCapacity(Capacity);
                    count = 0;
                }catch
                {
                    throw;
                }
                return true;
            }
        }

        /// <summary>
        /// Resize the cache
        /// </summary>
        /// <param name="newCapacity">The new capacity of the cache (must be greater than 0)</param>
        /// <returns>Return the cache singleton object</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <remarks>If the cache contains more elements than the new capacity, the oldes elements will be removed</remarks>
        public FbCache SetCapacity(int newCapacity)
        {
            if (newCapacity < 1) throw new ArgumentOutOfRangeException("newCapacity", "Cache capacity should be higher than 0");
            if (count < newCapacity)
            {
                cacheSet.EnsureCapacity(newCapacity);

            }
            else if (count > newCapacity)
            {
                var trimmedList = cacheList.Take(newCapacity).ToList();
                LinkedList<(object, object)> newCacheList = new LinkedList<(object, object)>(trimmedList);
                Dictionary<object, LinkedListNode<(object, object)>> newCacheSet = new Dictionary<object, LinkedListNode<(object, object)>>(newCapacity);
                for (var node = newCacheList.First; node != null; node = node.Next)
                {
                    newCacheSet.Add(node.Value.Item1, node);
                }
                cacheList = newCacheList;
                cacheSet = newCacheSet;
                count = newCapacity;
            }
            this.Capacity = newCapacity;

            return this;
        }
    }
}