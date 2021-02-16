using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MongoDB.AsyncEnumerable
{
    public interface IMongoAsyncEnumerable<out T> : IAsyncDisposable, IAsyncEnumerable<T>
    {
        IMongoAsyncEnumerable<T> WithLogger(ILogger logger);
    }
}
