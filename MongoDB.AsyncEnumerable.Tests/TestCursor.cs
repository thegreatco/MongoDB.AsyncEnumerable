using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using SharpExtensions;

namespace MongoDB.AsyncEnumerable.Tests
{
    public class TestCursor<T> : IAsyncCursor<T>
    {
        private bool _disposed;

        private readonly IReadOnlyList<IReadOnlyList<T>> _objects;

        private int _currentIndex;

        public TestCursor(IEnumerable<T> objects)
        {
            _objects = objects.Partition(10).Select(x => x.ToArray()).ToArray();
            // We start at -1 because MoveNext needs to be called before any data can be available
            _currentIndex = -1;
        }
        
        public void Dispose()
        {
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TestCursor<T>));
        }

        public bool MoveNext(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            _currentIndex++;
            return _currentIndex < _objects.Count;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(MoveNext(cancellationToken));
        }

        public IEnumerable<T> Current => _objects[_currentIndex];
    }
}
