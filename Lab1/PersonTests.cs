using System;
using Xunit;

namespace Lab1
{
    public class PersonTests
    {
        [Fact]
        public void FullName_ReturnsConcatenatedFirstNameAndLastName()
        {
            var person = new Person
            {
                FirstName = "Иван",
                LastName = "Иванов"
            };
            
            string fullName = person.FullName;
            Assert.Equal("Иван Иванов", fullName);
        }
        
        [Fact]
        public void IsAdult_Age18OrMore_ReturnsTrue()
        {
            var person = new Person { Age = 18 };
            bool isAdult = person.IsAdult;
            Assert.True(isAdult);
        }
        
        [Fact]
        public void IsAdult_AgeLessThan18_ReturnsFalse()
        {
            var person = new Person { Age = 17 };t
            bool isAdult = person.IsAdult;
            Assert.False(isAdult);
        }
        
        [Fact]
        public void Email_ValidEmail_DoesNotThrow()
        {
            var person = new Person();
            person.Email = "lol@wwwow.com";
            Assert.Equal("lol@wwwow.com", person.Email);
        }
        
        [Fact]
        public void Email_InvalidEmail_ThrowsArgumentException()
        {
            var person = new Person();
            Assert.Throws<ArgumentException>(() => person.Email = "invalid-email");
        }
        
        [Fact]
        public void Email_EmptyString_ThrowsArgumentException()
        {
            var person = new Person();
            Assert.Throws<ArgumentException>(() => person.Email = string.Empty);
        }
        
        [Fact]
        public void Password_IsIgnoredInJson()
        {
            var person = new Person
            {
                FirstName = "Test",
                LastName = "User",
                Password = "kek2222"
            };
            
            var serializer = new PersonSerializer();
            string json = serializer.SerializeToJson(person);
            Assert.DoesNotContain("Password", json);
            Assert.DoesNotContain("kek2222", json);
        }
    }
}

