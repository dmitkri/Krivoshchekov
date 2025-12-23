using System.Collections.Generic;
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
            
            Assert.Contains(3, list);
            Assert.DoesNotContain(10, list);
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
}

