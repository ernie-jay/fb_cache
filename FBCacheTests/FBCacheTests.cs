using FBCache;
using System.Reflection;

namespace FBCacheTests
{
    public class FBCacheTests
    {
        [Fact]
        public void TestSingleton()
        {
            var cache1 = FbCache.Instance;
            var cache2 = FbCache.Instance;

            Assert.Equal(cache1, cache2);
        }

        [Fact]
        public void TestAddElement()
        {
            var cache1 = FbCache.Instance;
            cache1.Dispose();
            
            var success = cache1.SetData<int, string>(1, "one");

            Assert.True(success.Item1);
            Assert.Null(success.Item2);
        }

        [Fact]
        public void TestGetElement()
        {
            var cache1 = FbCache.Instance;
            cache1.Dispose();

            cache1.SetData(1, "one");
            cache1.SetData("two", 2);

            var result1 = cache1.GetData<int, string>(1);
            var result2 = cache1.GetData<string, int>("two");

            Assert.Equal("one", result1);
            Assert.Equal(2,result2);
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, int>(2));
        }

        [Fact]
        public void TestRemoveElement()
        {
            var cache1 = FbCache.Instance;
            cache1.Dispose();

            cache1.SetData(1, "one");
            var result = cache1.RemoveData<int, string>(1);

            Assert.True(result);
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, string>(1));
        }

        [Fact]
        public void TestDispose() 
        {
            var cache1 = FbCache.Instance;
            cache1.Dispose();

            cache1.SetData(1, "one");
            cache1.SetData(2, "two");
            cache1.SetData(3, "three");

            cache1.Dispose(); 

            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, string>(1));
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, string>(2));
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, string>(3));
        }

        [Fact]
        public void TestThreadSafety()
        {
            var cache1 = FbCache.Instance;
            cache1.Dispose();

            var random = new Random();

            _ = Parallel.For(0, 100000, i =>
            {
                int value = random.Next(0, 100000);
                var cache2 = FbCache.Instance;
                cache2.SetData(i, value);
                try
                {
                    var result = cache1.GetData<int, int>(i);
                    Assert.Equal(value, result);
                } catch(KeyNotFoundException)
                {
                    Assert.True(true);
                }
            });

        }

        [Fact]
        public void TestResizeCache()
        {
            var cache1 = FbCache.Instance.SetCapacity(10);
            cache1.Dispose();

            for(int i=0; i<10; i++)
            {
                cache1.SetData(i, i);
            }
            cache1.SetCapacity(5);
            int result = cache1.GetData<int, int>(5);
            Assert.Equal(5, result);

            //the old elements are expected to be removed
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int,int>(4));
        }

        [Fact]
        public void TestCapacity()
        {
            var cache1 = FbCache.Instance.SetCapacity(5);
            cache1.Dispose();

            for(int i=1; i<=5; i++)
            {
                var result1 = cache1.SetData(i, i);

                //Not expected to remove anything from cache
                Assert.Null(result1.Item2);
            }

            var result = cache1.SetData(6, "Six");

            //expected to remove the first item from the list
            Assert.Equal(1, result.Item2);

            //The first item should be not in the list
            Assert.Throws<KeyNotFoundException>(()=>cache1.GetData<int, int>(1));
            Assert.Equal(2, cache1.GetData<int, int>(2));
            Assert.Equal("Six", cache1.GetData<int, string>(6));
        }

        [Fact]
        public void TestLRE()
        {
            var cache1 = FbCache.Instance.SetCapacity(5);
            cache1.Dispose();

            for (int i = 1; i <= 5; i++)
            {
                cache1.SetData(i, i);
            }

            //put the element 1 on the top of the cache
            var result = cache1.GetData<int, int>(1);

            cache1.SetData(6, "six");

            //Expected to remove the element 2 from the list
            Assert.Throws<KeyNotFoundException>(() => cache1.GetData<int, int>(2));
            Assert.Equal(1, cache1.GetData<int, int>(1));
            Assert.Equal("six", cache1.GetData<int, string>(6));
        }

        [Fact]
        public void TestEdgeCases()
        {
            var cache = FbCache.Instance.SetCapacity(1);
            cache.Dispose();

            cache.SetData(1, "one");
            cache.SetData(2, "two");

            Assert.Equal("two", cache.GetData<int, string>(2));
            Assert.Throws<KeyNotFoundException>(()=>cache.GetData<int, string>(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => cache.SetCapacity(0));
        }
    }
}