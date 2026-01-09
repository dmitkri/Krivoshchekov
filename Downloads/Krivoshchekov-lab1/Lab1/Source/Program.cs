using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Демонстрация работы с Person и сериализацией ===\n");
            
            var person = new Person
            {
                FirstName = "Игорь",
                LastName = "Синяк",
                Age = 33,
                Id = "1",
                Email = "igorsinyak@france.com",
                PhoneNumber = "+77777777777",
                BirthDate = new DateTime(1992, 9, 17),
                Password = "moilubimiiVPSH123"
            };
            
            Console.WriteLine("1. Создан объект Person:");
            Console.WriteLine($"   Имя: {person.FirstName}");
            Console.WriteLine($"   Фамилия: {person.LastName}");
            Console.WriteLine($"   Полное имя: {person.FullName}");
            Console.WriteLine($"   Возраст: {person.Age}");
            Console.WriteLine($"   Взрослый: {person.IsAdult}");
            Console.WriteLine($"   Email: {person.Email}");
            Console.WriteLine($"   Телефон: {person.PhoneNumber}");
            Console.WriteLine($"   Дата рождения: {person.BirthDate:yyyy-MM-dd}");
            Console.WriteLine();
            
            var serializer = new PersonSerializer();
            
            Console.WriteLine("2. Сериализация в JSON строку:");
            string json = serializer.SerializeToJson(person);
            Console.WriteLine(json);
            Console.WriteLine();
            
            Console.WriteLine("3. Десериализация из JSON строки:");
            Person deserializedPerson = serializer.DeserializeFromJson(json);
            Console.WriteLine($"   Восстановленное имя: {deserializedPerson.FirstName}");
            Console.WriteLine($"   Восстановленная фамилия: {deserializedPerson.LastName}");
            Console.WriteLine();
            
            string filePath = "Data/person.json";
            Console.WriteLine($"4. Сохранение в файл {filePath} (синхронно):");
            serializer.SaveToFile(person, filePath);
            Console.WriteLine("   Файл успешно сохранен!");
            Console.WriteLine();
            
            Console.WriteLine($"5. Загрузка из файла {filePath} (синхронно):");
            Person loadedPerson = serializer.LoadFromFile(filePath);
            Console.WriteLine($"   Загруженное имя: {loadedPerson.FirstName}");
            Console.WriteLine($"   Загруженная фамилия: {loadedPerson.LastName}");
            Console.WriteLine();
            
            string asyncFilePath = "Data/person_async.json";
            Console.WriteLine($"6. Сохранение в файл {asyncFilePath} (асинхронно):");
            await serializer.SaveToFileAsync(person, asyncFilePath);
            Console.WriteLine("   Файл успешно сохранен асинхронно!");
            Console.WriteLine();
            
            Console.WriteLine($"7. Загрузка из файла {asyncFilePath} (асинхронно):");
            Person asyncLoadedPerson = await serializer.LoadFromFileAsync(asyncFilePath);
            Console.WriteLine($"   Загруженное имя: {asyncLoadedPerson.FirstName}");
            Console.WriteLine();
            
            Console.WriteLine("8. Работа со списком объектов:");
            var people = new List<Person>
            {
                new Person { FirstName = "Сергей", LastName = "Григорьев-Апполонов", Age = 38, Id = "2", Email = "greyw@france.com", PhoneNumber = "555" },
                new Person { FirstName = "Филипп", LastName = "Киркоров", Age = 58, Id = "3", Email = "filipp@kirkorov.bg", PhoneNumber = "111" },
                new Person { FirstName = "Лариса", LastName = "Долина", Age = 70, Id = "4", Email = "zaberu@kvartiry.ru", PhoneNumber = "666" }
            };
            
            string listFilePath = "Data/people.json";
            serializer.SaveListToFile(people, listFilePath);
            Console.WriteLine($"   Список сохранен в {listFilePath}");
            
            List<Person> loadedPeople = serializer.LoadListFromFile(listFilePath);
            Console.WriteLine($"   Загружено {loadedPeople.Count} человек:");
            foreach (var p in loadedPeople)
            {
                Console.WriteLine($"     - {p.FullName}, возраст: {p.Age}, взрослый: {p.IsAdult}");
            }
            Console.WriteLine();
            
            Console.WriteLine("9. Демонстрация FileResourceManager:");
            string testFilePath = "Data/test_resource.txt";
            
            using (var fileManager = new FileResourceManager(testFilePath, System.IO.FileMode.Create))
            {
                fileManager.OpenForWriting();
                fileManager.WriteLine("Первая строка");
                fileManager.WriteLine("Вторая строка");
                fileManager.WriteLine("Третья строка");
                
                fileManager.OpenForReading();
                string content = fileManager.ReadAllText();
                Console.WriteLine("   Содержимое файла:");
                Console.WriteLine(content);
                
                FileInfo info = fileManager.GetFileInfo();
                Console.WriteLine($"   Размер файла: {info.Length} байт");
                Console.WriteLine($"   Дата создания: {info.CreationTime}");
            }
            Console.WriteLine("   Ресурсы автоматически освобождены через using");
            Console.WriteLine();
            
            Console.WriteLine("10. Демонстрация валидации Email:");
            try
            {
                var personWithInvalidEmail = new Person
                {
                    FirstName = "Тест",
                    LastName = "Тестов",
                    Email = "invalid-email"
                };
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"   Ошибка валидации: {ex.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("=== Демонстрация завершена ===");
        }
    }
}

