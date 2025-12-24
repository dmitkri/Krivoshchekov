using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab1
{
    public class PersonSerializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        private static readonly object _fileLock = new object();
        
        private static void LogError(string error)
        {
            try
            {
                File.AppendAllText("errors.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {error}\n", Encoding.UTF8);
            }
            catch { }
        }
        
        private static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым");
        }
        
        public string SerializeToJson(Person person)
        {
            try
            {
                return JsonSerializer.Serialize(person, _jsonOptions);
            }
            catch (Exception ex)
            {
                LogError($"Ошибка сериализации: {ex.Message}");
                throw;
            }
        }
        
        public Person DeserializeFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON строка не может быть пустой");
            
            try
            {
                var person = JsonSerializer.Deserialize<Person>(json, _jsonOptions);
                if (person == null)
                    throw new InvalidOperationException("Не удалось десериализовать объект Person");
                return person;
            }
            catch (Exception ex)
            {
                LogError($"Ошибка десериализации: {ex.Message}");
                throw;
            }
        }
        
        public void SaveToFile(Person person, string filePath)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));
            ValidateFilePath(filePath);
            
            try
            {
                lock (_fileLock)
                {
                    File.WriteAllText(filePath, SerializeToJson(person), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка сохранения в файл {filePath}: {ex.Message}");
                throw;
            }
        }
        
        public Person LoadFromFile(string filePath)
        {
            ValidateFilePath(filePath);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            try
            {
                lock (_fileLock)
                {
                    return DeserializeFromJson(File.ReadAllText(filePath, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка загрузки из файла {filePath}: {ex.Message}");
                throw;
            }
        }
        
        public async Task SaveToFileAsync(Person person, string filePath)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));
            ValidateFilePath(filePath);
            
            try
            {
                await File.WriteAllTextAsync(filePath, SerializeToJson(person), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogError($"Ошибка асинхронного сохранения в файл {filePath}: {ex.Message}");
                throw;
            }
        }
        
        public async Task<Person> LoadFromFileAsync(string filePath)
        {
            ValidateFilePath(filePath);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            try
            {
                return DeserializeFromJson(await File.ReadAllTextAsync(filePath, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                LogError($"Ошибка асинхронной загрузки из файла {filePath}: {ex.Message}");
                throw;
            }
        }
        
        public void SaveListToFile(List<Person> people, string filePath)
        {
            if (people == null)
                throw new ArgumentNullException(nameof(people));
            ValidateFilePath(filePath);
            
            try
            {
                lock (_fileLock)
                {
                    File.WriteAllText(filePath, JsonSerializer.Serialize(people, _jsonOptions), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка сохранения списка в файл {filePath}: {ex.Message}");
                throw;
            }
        }
        
        public List<Person> LoadListFromFile(string filePath)
        {
            ValidateFilePath(filePath);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            try
            {
                lock (_fileLock)
                {
                    var people = JsonSerializer.Deserialize<List<Person>>(File.ReadAllText(filePath, Encoding.UTF8), _jsonOptions);
                    return people ?? new List<Person>();
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка загрузки списка из файла {filePath}: {ex.Message}");
                throw;
            }
        }
    }
}
