# FBCache
Simple implementation of a generic in-memory cache

The data is stored in a LinkedList combined with Dictionary, for better performance:
LinkedList stores the key and the data in a "least recently used (LRE)" order:
- The new data is added on the front of the list
- The recently accessed data is moved to the front of the list
- If the cache is full, the last element is removed when new items added

The Dictionary stores the key and the LinkedList node element for fast access (O(1))

API description:

GetData<K, V>(K key): Getting the element with K key
SetData<K, V>(K key, V data): Adding a new element, or replace the old one with a K key
RemoveData<K, V>(K key): Remove the element with a K key
Dispose(): Reset the cache
SetCapacity(int newCapacity): Reinitialise the capacity of the cache

