using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MongoDB.AsyncEnumerable
{
    public class MongoAsyncEnumerable<T> : IMongoAsyncEnumerable<T>
    {
        private ILogger _logger;
        private readonly IAsyncCursor<T> _cursor;

        public MongoAsyncEnumerable(IAsyncCursor<T> cursor, ILogger logger = null)
        {
            _cursor = cursor;
            _logger = logger;
        }

        public void Dispose()
        {
            _cursor?.Dispose();
            GC.SuppressFinalize(this);
        }

        public IMongoAsyncEnumerable<T> WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public ValueTask DisposeAsync()
        {
            _cursor?.Dispose();
            GC.SuppressFinalize(this);
            return new ValueTask();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new MongoAsyncEnumerator<T>(_cursor, _logger);
        }
    }
}