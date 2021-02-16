using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDB.AsyncEnumerable
{
    public interface IMongoAsyncEnumerable<out T> : IAsyncDisposable, IAsyncEnumerable<T>
    {
    }
}
