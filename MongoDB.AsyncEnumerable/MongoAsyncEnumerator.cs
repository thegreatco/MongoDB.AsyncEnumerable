using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;

namespace MongoDB.AsyncEnumerable
{
    public class MongoAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private bool _disposed;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private IReadOnlyCollection<T> _objects;
        private readonly IAsyncCursor<T> _cursor;
        private IEnumerator<T> _enumerator;
        private bool _exhausted;

        public MongoAsyncEnumerator(IAsyncCursor<T> cursor, ILogger logger)
        {
            _cursor = cursor;
            _logger = logger ?? NullLogger.Instance;
        }

        public MongoAsyncEnumerator(IReadOnlyCollection<T> objects, ILogger logger)
        {
            _logger = logger;
            _objects = objects;
            _enumerator = objects.GetEnumerator();
        }

        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _cursor?.Dispose();
                _objects = null;
                _enumerator?.Dispose();
                _disposed = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Cannot iterate a disposed enumerator");
        }

        private ValueTask<bool> InternalCollectionMoveNextAsync()
        {
            return new ValueTask<bool>(_enumerator.MoveNext());
        }

        private async ValueTask<bool> InternalCursorMoveNextAsync(Stopwatch sw)
        {
            if (_enumerator != null && _enumerator.MoveNext())
            {
                return true;
            }

            if (_cursor != null)
            {
                if (await _cursor.MoveNextAsync().ConfigureAwait(false))
                {
                    _logger?.LogTrace($"moveNext: {sw.ElapsedMilliseconds}");
                    _enumerator = _cursor.Current.GetEnumerator();
                    return _enumerator.MoveNext();
                }
                else
                {
                    _logger?.LogTrace($"No documents found after moveNext: {sw.ElapsedMilliseconds}");
                    _enumerator = null;
                    _exhausted = true;
                    return false;
                }
            }

            _enumerator = null;
            _exhausted = true;
            return false;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var sw = Stopwatch.StartNew();
            _logger?.LogTrace($"Acquiring lock: {sw.ElapsedMilliseconds}");
            if (await _semaphore.WaitAsync(1000).ConfigureAwait(false) == false)
            {
                throw new TimeoutException("Timed out while waiting to enter semaphore.");
            }

            ThrowIfDisposed();
            _logger?.LogTrace($"Acquired lock: {sw.ElapsedMilliseconds}");
            try
            {
                if (_exhausted)
                {
                    _logger?.LogTrace("Enumerable already exhausted.");
                    return false;
                }

                if (_objects != null)
                {
                    _logger?.LogTrace("Checking internal collection.");
                    return await InternalCollectionMoveNextAsync().ConfigureAwait(false);
                }

                if (_cursor != null)
                {
                    _logger?.LogTrace("Checking remote cursor.");
                    return await InternalCursorMoveNextAsync(sw).ConfigureAwait(false);
                }

                return false;
            }
            finally
            {
                _semaphore.Release();
                _logger?.LogTrace($"Released lock: {sw.ElapsedMilliseconds}");
            }
        }

        public T Current => _enumerator.Current;
    }
}