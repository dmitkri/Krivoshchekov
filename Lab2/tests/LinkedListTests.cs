using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CollectionsPerformanceTests
{
    public class LinkedListTests
    {
        [Fact]
        public void TestAddToEnd()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            
            Assert.Equal(3, linkedList.Count);
            Assert.NotNull(linkedList.First);
            Assert.NotNull(linkedList.Last);
            Assert.Equal(1, linkedList.First!.Value);
            Assert.Equal(3, linkedList.Last!.Value);
        }
        
        [Fact]
        public void TestAddToStart()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            linkedList.AddFirst(1);
            
            Assert.Equal(3, linkedList.Count);
            Assert.NotNull(linkedList.First);
            Assert.Equal(1, linkedList.First!.Value);
        }
        
        [Fact]
        public void TestAddToMiddle()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(3);
            var node = linkedList.First?.Next;
            if (node != null)
            {
                linkedList.AddAfter(node, 2);
            }
            
            Assert.Equal(3, linkedList.Count);
            var values = linkedList.ToArray();
            Assert.Equal(1, values[0]);
            Assert.Equal(3, values[1]);
            Assert.Equal(2, values[2]);
        }
        
        [Fact]
        public void TestSearch()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            
            Assert.Contains(2, linkedList);
            Assert.DoesNotContain(10, linkedList);
        }
        
        [Fact]
        public void TestGetByIndex()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(10);
            linkedList.AddLast(20);
            linkedList.AddLast(30);
            
            var values = linkedList.ToArray();
            Assert.Equal(10, values[0]);
            Assert.Equal(20, values[1]);
            Assert.Equal(30, values[2]);
        }
        
        [Fact]
        public void TestRemoveFromStart()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            linkedList.RemoveFirst();
            
            Assert.Equal(2, linkedList.Count);
            Assert.NotNull(linkedList.First);
            Assert.Equal(2, linkedList.First!.Value);
        }
        
        [Fact]
        public void TestRemoveFromEnd()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            linkedList.RemoveLast();
            
            Assert.Equal(2, linkedList.Count);
            Assert.NotNull(linkedList.Last);
            Assert.Equal(2, linkedList.Last!.Value);
        }
        
        [Fact]
        public void TestRemoveFromMiddle()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            var node = linkedList.First?.Next;
            if (node != null)
            {
                linkedList.Remove(node);
            }
            
            Assert.Equal(2, linkedList.Count);
            Assert.DoesNotContain(2, linkedList);
        }
    }
}

