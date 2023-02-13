using FBCache;
using System.Reflection;

namespace FBCacheTests
{
    public class FBCacheTests
    {
        [Fact]
        public void TestSingleton()
        {
            var cache1 = FbCache.GetInstance();
            var cache2 = FbCache.GetInstance();

            Assert.Equal(cache1, cache2);
        }

        [Fact]
        public void TestAddElement()
        {
            var cache1 = FbCache.GetInstance();
            cache1.Dispose();
            
            var success = cache1.SetData<int, string>(1, "one");

            Assert.True(success);
        }

        [Fact]
        public void TestGetElement()
        {
            var cache1 = FbCache.GetInstance();
            cache1.Dispose();

            cache1.SetData(1, "one");
            var result = cache1.GetData<int, string>(1);

            Assert.Equal("one", result);
        }

        [Fact]
        public void TestRemoveElement()
        {
            var cache1 = FbCache.GetInstance();
            cache1.Dispose();

            cache1.SetData(1, "one");
            cache1.RemoveData<int, string>(1);

            var result = cache1.GetData<int, string>(1);

            Assert.Null(result);
        }

        [Fact]
        public void TestDispose() 
        {
            var cache1 = FbCache.GetInstance();
            cache1.Dispose();

            cache1.SetData(1, "one");
            cache1.SetData(2, "two");
            cache1.SetData(3, "three");

            cache1.Dispose(); 

            var result1 = cache1.GetData<int, string>(1);
            var result2 = cache1.GetData<int, string>(2);
            var result3 = cache1.GetData<int, string>(3);

            Assert.Null(result1);
            Assert.Null(result2);
            Assert.Null(result3);
        }

        [Fact]
        public void TestThreadSafety()
        {
            var cache1 = FbCache.GetInstance();
            cache1.Dispose();

            int lastResult;
            var random = new Random();

            Parallel.For(0, 100000, i =>
            {
                int value = random.Next(0, 100000);
                cache1.SetData(1, value);
            });

        }

    }
}