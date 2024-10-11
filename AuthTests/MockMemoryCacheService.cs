using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthTests
{
    public static class MockMemoryCacheService
    {
        public static IMemoryCache GetMemoryCache(object? expectedValue)

        {

            var mockMemoryCache = new Mock<IMemoryCache>();



            if (expectedValue != null)

            {

                mockMemoryCache

                  .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))

                  .Returns(true);

            }

            else

            {

                mockMemoryCache

                  .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))

                  .Returns(false);

            }



            return mockMemoryCache.Object;

        }



        //public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value)

        //{

        //    var entry = cache.CreateEntry(key);

        //    entry.Value = value;

        //    entry.Dispose();



        //    return value;

        //}



        //public static TItem Set<TItem>(this IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions cacheEntryOptions)

        //{

        //    var entry = cache.CreateEntry(key);

        //    entry.Value = value;

        //    entry.Dispose();



        //    return value;

        //}

    }
}
