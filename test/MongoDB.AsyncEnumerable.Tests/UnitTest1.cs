using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.AsyncEnumerable.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var expected = Enumerable.Range(0, 100).ToArray();
            var cursor = new TestCursor<int>(expected);
            var asyncEnumerable = cursor.AsAsyncEnumerable();
            Assert.IsType<MongoAsyncEnumerable<int>>(asyncEnumerable);
            var i = 0;
            await foreach (var obj in asyncEnumerable)
            {
                Assert.Equal(expected[i], obj);
                i++;
            }
        }
    }
}
