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
        private readonly ILogger _logger;
        private readonly IAsyncCursor<T> _cursor;
        private IReadOnlyList<T> _objects;

        public MongoAsyncEnumerable(IAsyncCursor<T> cursor, ILogger logger = null)
        {
            _cursor = cursor;
            _logger = logger;
        }

        /// <summary>
        /// Used only for mocking
        /// </summary>
        /// <param name="objects">The objects to return as part of the mock</param>
        public MongoAsyncEnumerable(IReadOnlyList<T> objects)
        {
            _objects = objects;
        }

        public void Dispose()
        {
            _cursor?.Dispose();
            _objects = null;
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            _cursor?.Dispose();
            _objects = null;
            GC.SuppressFinalize(this);
            return new ValueTask();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return _objects != null
                       ? new MongoAsyncEnumerator<T>(_objects, _logger)
                       : new MongoAsyncEnumerator<T>(_cursor,  _logger);
        }
    }
}
