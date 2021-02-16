using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace MongoDB.AsyncEnumerable
{
    public static class Extensions
    {
        public static IMongoAsyncEnumerable<T> AsAsyncEnumerable<T>(this IAsyncCursor<T> cursor)
        {
            return cursor.AsAsyncEnumerable(NullLogger.Instance);
        }

        public static IMongoAsyncEnumerable<T> AsAsyncEnumerable<T>(this IAsyncCursor<T> cursor, ILogger logger)
        {
            return new MongoAsyncEnumerable<T>(cursor, logger);
        }
    }
}
