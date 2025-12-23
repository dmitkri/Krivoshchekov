using System.Collections.Immutable;
using Xunit;

namespace CollectionsPerformanceTests
{
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
            
            Assert.Contains(2, list);
            Assert.DoesNotContain(10, list);
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
            
            Assert.Equal(2, list1.Count);
            Assert.Equal(3, list2.Count);
        }
    }
}

