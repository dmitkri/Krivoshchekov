using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace CollectionsPerformanceTests
{
    public class ListTests
    {
        [Fact]
        public void TestAddToEnd()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestAddToStart()
        {
            var list = new List<int> { 2, 3 };
            list.Insert(0, 1);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestAddToMiddle()
        {
            var list = new List<int> { 1, 3 };
            list.Insert(1, 2);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestSearch()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            
            Assert.True(list.Contains(3));
            Assert.False(list.Contains(10));
        }
        
        [Fact]
        public void TestGetByIndex()
        {
            var list = new List<int> { 10, 20, 30 };
            
            Assert.Equal(10, list[0]);
            Assert.Equal(20, list[1]);
            Assert.Equal(30, list[2]);
        }
        
        [Fact]
        public void TestRemoveFromStart()
        {
            var list = new List<int> { 1, 2, 3 };
            list.RemoveAt(0);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(2, list[0]);
            Assert.Equal(3, list[1]);
        }
        
        [Fact]
        public void TestRemoveFromEnd()
        {
            var list = new List<int> { 1, 2, 3 };
            list.RemoveAt(list.Count - 1);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
        }
        
        [Fact]
        public void TestRemoveFromMiddle()
        {
            var list = new List<int> { 1, 2, 3 };
            list.RemoveAt(1);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(3, list[1]);
        }
    }
    
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
            Assert.Equal(1, linkedList.First.Value);
            Assert.Equal(3, linkedList.Last.Value);
        }
        
        [Fact]
        public void TestAddToStart()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            linkedList.AddFirst(1);
            
            Assert.Equal(3, linkedList.Count);
            Assert.Equal(1, linkedList.First.Value);
        }
        
        [Fact]
        public void TestAddToMiddle()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(3);
            var node = linkedList.First.Next;
            linkedList.AddAfter(node, 2);
            
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
            
            Assert.True(linkedList.Contains(2));
            Assert.False(linkedList.Contains(10));
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
            Assert.Equal(2, linkedList.First.Value);
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
            Assert.Equal(2, linkedList.Last.Value);
        }
        
        [Fact]
        public void TestRemoveFromMiddle()
        {
            var linkedList = new LinkedList<int>();
            linkedList.AddLast(1);
            linkedList.AddLast(2);
            linkedList.AddLast(3);
            var node = linkedList.First.Next;
            linkedList.Remove(node);
            
            Assert.Equal(2, linkedList.Count);
            Assert.False(linkedList.Contains(2));
        }
    }
    
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
            
            Assert.True(queue.Contains(2));
            Assert.False(queue.Contains(10));
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
            
            Assert.True(stack.Contains(2));
            Assert.False(stack.Contains(10));
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
    
    public class ImmutableListTests
    {
        [Fact]
        public void TestAddToEnd()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(2);
            list = list.Add(3);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestAddToStart()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(2);
            list = list.Add(3);
            list = list.Insert(0, 1);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestAddToMiddle()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(3);
            list = list.Insert(1, 2);
            
            Assert.Equal(3, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
        }
        
        [Fact]
        public void TestSearch()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(2);
            list = list.Add(3);
            
            Assert.True(list.Contains(2));
            Assert.False(list.Contains(10));
        }
        
        [Fact]
        public void TestGetByIndex()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(10);
            list = list.Add(20);
            list = list.Add(30);
            
            Assert.Equal(10, list[0]);
            Assert.Equal(20, list[1]);
            Assert.Equal(30, list[2]);
        }
        
        [Fact]
        public void TestRemoveFromStart()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(2);
            list = list.Add(3);
            list = list.RemoveAt(0);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(2, list[0]);
            Assert.Equal(3, list[1]);
        }
        
        [Fact]
        public void TestRemoveFromEnd()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(2);
            list = list.Add(3);
            list = list.RemoveAt(list.Count - 1);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);
        }
        
        [Fact]
        public void TestRemoveFromMiddle()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(1);
            list = list.Add(2);
            list = list.Add(3);
            list = list.RemoveAt(1);
            
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]);
            Assert.Equal(3, list[1]);
        }
        
        [Fact]
        public void TestImmutability()
        {
            var list1 = ImmutableList<int>.Empty;
            list1 = list1.Add(1);
            list1 = list1.Add(2);
            
            var list2 = list1.Add(3);
            
            // list1 не должен измениться
            Assert.Equal(2, list1.Count);
            Assert.Equal(3, list2.Count);
        }
    }
}

