using System.Collections.Generic;
using Xunit;

namespace CollectionsPerformanceTests
{
    public class QueueTests
    {
        [Fact]
        public void TestAddToEnd()
        {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            
            Assert.Equal(3, queue.Count);
        }
        
        [Fact]
        public void TestSearch()
        {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            
            Assert.Contains(2, queue);
            Assert.DoesNotContain(10, queue);
        }
        
        [Fact]
        public void TestRemoveFromStart()
        {
            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            
            int first = queue.Dequeue();
            
            Assert.Equal(1, first);
            Assert.Equal(2, queue.Count);
            Assert.Equal(2, queue.Peek());
        }
    }
}

