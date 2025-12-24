using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Lab1
{
    public class PersonSerializerTests
    {
        private readonly PersonSerializer _serializer = new PersonSerializer();
        private readonly string _testFilePath = "test_person.json";
        private readonly string _testListFilePath = "test_people.json";
        private void CleanupTestFiles()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
            if (File.Exists(_testListFilePath))
                File.Delete(_testListFilePath);
        }
        
        [Fact]
        public void SerializeToJson_ValidPerson_ReturnsJsonString()
        {
            var person = new Person
            {
                FirstName = "Игорь",
                LastName = "Синяк",
                Age = 33,
                Id = "1",
                Email = "igorsinyak@france.com",
                PhoneNumber = "+77777777777",
                BirthDate = new DateTime(1992, 9, 17)
            };
            
            string json = _serializer.SerializeToJson(person);
            Assert.NotNull(json);
            Assert.Contains("Игорь", json);
            Assert.Contains("personId", json);
            Assert.Contains("phone", json);
            Assert.DoesNotContain("Password", json);
        }
        
        [Fact]
        public void DeserializeFromJson_ValidJson_ReturnsPerson()
        {
            string json = @"{
  ""FirstName"": ""Игорь"",
  ""LastName"": ""Синяк"",
  ""Age"": 33,
  ""personId"": ""1"",
  ""_birthDate"": ""1992-09-17T00:00:00"",
  ""Email"": ""igorsinyak@france.com"",
  ""phone"": ""+77777777777""
}";
            
            Person person = _serializer.DeserializeFromJson(json);
            Assert.NotNull(person);
            Assert.Equal("Игорь", person.FirstName);
            Assert.Equal("Синяк", person.LastName);
            Assert.Equal(33, person.Age);
            Assert.Equal("1", person.Id);
            Assert.Equal("igorsinyak@france.com", person.Email);
            Assert.Equal("+77777777777", person.PhoneNumber);
        }
        
        [Fact]
        public void SaveToFile_ValidPerson_CreatesFile()
        {
            try
            {
                var person = new Person
                {
                    FirstName = "Сергей",
                    LastName = "Григорьев-Апполонов",
                    Age = 38,
                    Id = "2",
                    Email = "greyw@france.com",
                    PhoneNumber = "555",
                    BirthDate = new DateTime(1987, 2, 23)
                };

                _serializer.SaveToFile(person, _testFilePath);
                Assert.True(File.Exists(_testFilePath));
                string content = File.ReadAllText(_testFilePath);
                Assert.Contains("Сергей", content);
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public void LoadFromFile_ValidFile_ReturnsPerson()
        {
            try
            {
                var person = new Person
                {
                    FirstName = "Филипп",
                    LastName = "Киркоров",
                    Age = 58,
                    Id = "3",
                    Email = "filipp@kirkorov.bg",
                    PhoneNumber = "111",
                    BirthDate = new DateTime(1967, 3, 2)
                };
                _serializer.SaveToFile(person, _testFilePath);

                Person loadedPerson = _serializer.LoadFromFile(_testFilePath);
                Assert.NotNull(loadedPerson);
                Assert.Equal("Филипп", loadedPerson.FirstName);
                Assert.Equal("Киркоров", loadedPerson.LastName);
                Assert.Equal(58, loadedPerson.Age);
                Assert.Equal("3", loadedPerson.Id);
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public async Task SaveToFileAsync_ValidPerson_CreatesFile()
        {
            try
            {
                var person = new Person
                {
                    FirstName = "Лариса",
                    LastName = "Долина",
                    Age = 70,
                    Id = "4",
                    Email = "zaberu@kvartiry.ru",
                    PhoneNumber = "666",
                    BirthDate = new DateTime(1955, 1, 1)
                };
                
                await _serializer.SaveToFileAsync(person, _testFilePath);
                Assert.True(File.Exists(_testFilePath));
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public async Task LoadFromFileAsync_ValidFile_ReturnsPerson()
        {
            try
            {
                var person = new Person
                {
                    FirstName = "Дмитрий",
                    LastName = "Кривощеков",
                    Age = 18,
                    Id = "207",
                    Email = "dpk7240@mail.ru",
                    PhoneNumber = "+79222624240",
                    BirthDate = new DateTime(2007, 2, 22)
                };
                await _serializer.SaveToFileAsync(person, _testFilePath);
                
                Person loadedPerson = await _serializer.LoadFromFileAsync(_testFilePath);
                Assert.NotNull(loadedPerson);
                Assert.Equal("Дмитрий", loadedPerson.FirstName);
                Assert.Equal(18, loadedPerson.Age);
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public void SaveListToFile_ValidList_CreatesFile()
        {
            try
            {
                var people = new List<Person>
                {
                    new Person { FirstName = "Игорь", LastName = "Синяк", Age = 33, Id = "1", Email = "igorsinyak@france.com", PhoneNumber = "111" },
                    new Person { FirstName = "Сергей", LastName = "Григорьев-Апполонов", Age = 38, Id = "2", Email = "greyw@france.com", PhoneNumber = "222" }
                };

                _serializer.SaveListToFile(people, _testListFilePath);
                Assert.True(File.Exists(_testListFilePath));
                string content = File.ReadAllText(_testListFilePath);
                Assert.Contains("Игорь", content);
                Assert.Contains("Сергей", content);
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public void LoadListFromFile_ValidFile_ReturnsList()
        {
            try
            {
                var people = new List<Person>
                {
                    new Person { FirstName = "Филипп", LastName = "Киркоров", Age = 58, Id = "3", Email = "f1Lipp@kirkorov.bg", PhoneNumber = "333" },
                    new Person { FirstName = "Лариса", LastName = "Долина", Age = 70, Id = "4", Email = "zaberu@kvartiry.ru", PhoneNumber = "444" }
                };
                _serializer.SaveListToFile(people, _testListFilePath);

                List<Person> loadedPeople = _serializer.LoadListFromFile(_testListFilePath);
                Assert.NotNull(loadedPeople);
                Assert.Equal(2, loadedPeople.Count);
                Assert.Equal("Филипп", loadedPeople[0].FirstName);
                Assert.Equal("Лариса", loadedPeople[1].FirstName);
            }
            finally
            {
                CleanupTestFiles();
            }
        }
        
        [Fact]
        public void LoadFromFile_NonExistentFile_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _serializer.LoadFromFile("nonexistent.json"));
        }
        
        [Fact]
        public void SaveToFile_NullPerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.SaveToFile(null!, _testFilePath));
        }
    }
}

