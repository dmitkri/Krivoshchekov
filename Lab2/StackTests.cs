using System.Collections.Generic;
using Xunit;

namespace CollectionsPerformanceTests
{
    public class StackTests
    {
        [Fact]
        public void TestAddToStart()
        {
            var stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            
            Assert.Equal(3, stack.Count);
        }
        
        [Fact]
        public void TestSearch()
        {
            var stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            
            Assert.Contains(2, stack);
            Assert.DoesNotContain(10, stack);
        }
        
        [Fact]
        public void TestRemoveFromStart()
        {
            var stack = new Stack<int>();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            
            int top = stack.Pop();
            
            Assert.Equal(3, top);
            Assert.Equal(2, stack.Count);
            Assert.Equal(2, stack.Peek());
        }
    }
}

