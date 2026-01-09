# Лабораторная работа 1: Работа с JSON сериализацией и файловыми операциями

## Описание проекта

Данный проект демонстрирует работу с JSON сериализацией объектов в C#, управление файловыми ресурсами и реализацию паттерна IDisposable. Проект состоит из трех основных классов и набора тестов.

## Структура проекта

### Классы

1. **Person** - класс, представляющий информацию о человеке с использованием атрибутов JSON
2. **PersonSerializer** - класс для сериализации и десериализации объектов Person
3. **FileResourceManager** - класс для управления файловыми ресурсами с реализацией IDisposable

## Используемые технологии и библиотеки

- **.NET 10.0** - платформа разработки
- **System.Text.Json** - библиотека для работы с JSON (встроена в .NET)
- **xUnit** - фреймворк для написания тестов
- **System.IO** - для работы с файлами и потоками

## Задание 1: Класс Person

Класс `Person` содержит следующие свойства:

- `FirstName` (string) - имя
- `LastName` (string) - фамилия
- `Age` (int) - возраст
- `Password` (string) - пароль (помечен `[JsonIgnore]`, не попадает в JSON)
- `Id` (string) - идентификатор (в JSON сериализуется как `personId`)
- `_birthDate` (DateTime) - приватное поле даты рождения (помечено `[JsonInclude]`)
- `BirthDate` (DateTime) - свойство для доступа к дате рождения
- `Email` (string) - электронная почта с валидацией (проверка наличия '@')
- `PhoneNumber` (string) - номер телефона (в JSON сериализуется как `phone`)

### Дополнительные свойства:

- `FullName` (string, только чтение) - полное имя (конкатенация FirstName + LastName)
- `IsAdult` (bool, только чтение) - проверка, является ли человек взрослым (Age >= 18)

## Задание 2: Класс PersonSerializer

Класс `PersonSerializer` предоставляет следующие методы:

1. `SerializeToJson(Person person)` - сериализация объекта в JSON строку
2. `DeserializeFromJson(string json)` - десериализация из JSON строки
3. `SaveToFile(Person person, string filePath)` - синхронное сохранение в файл
4. `LoadFromFile(string filePath)` - синхронная загрузка из файла
5. `SaveToFileAsync(Person person, string filePath)` - асинхронное сохранение в файл
6. `LoadFromFileAsync(string filePath)` - асинхронная загрузка из файла
7. `SaveListToFile(List<Person> people, string filePath)` - сохранение списка объектов
8. `LoadListFromFile(string filePath)` - загрузка списка объектов

### Особенности реализации:

- Использование UTF-8 кодировки для текстовых файлов
- Красивое форматирование JSON (WriteIndented = true)
- Обработка исключений при работе с файлами
- Логирование ошибок в файл `errors.log`
- Потокобезопасность при работе с файлами (использование lock)

## Задание 3: Класс FileResourceManager

Класс `FileResourceManager` реализует интерфейс `IDisposable` и управляет файловыми ресурсами.

### Поля:

- `_fileStream` - FileStream для работы с файлом
- `_writer` - StreamWriter для текстовых операций записи
- `_reader` - StreamReader для текстовых операций чтения
- `_disposed` - флаг освобождения ресурсов
- `_filePath` - путь к файлу

### Методы:

- Конструктор, принимающий путь к файлу и режим открытия (FileMode)
- `OpenForWriting()` - открывает файл для записи
- `OpenForReading()` - открывает файл для чтения
- `WriteLine(string text)` - записывает строку в файл
- `ReadAllText()` - читает весь файл
- `AppendText(string text)` - добавляет текст в конец файла
- `GetFileInfo()` - возвращает информацию о файле (размер, дата создания)
- `Dispose()` - правильная реализация IDisposable

### Особенности реализации:

- Использование `using` для вложенных ресурсов
- Обработка исключений при работе с файлами
- Проверка существования файла перед операциями
- Реализация паттерна Dispose с финализатором

## Задание 4: Тестирование

Проект содержит набор тестов для проверки функционала:

- `PersonSerializerTests` - тесты для класса PersonSerializer
- `PersonTests` - тесты для класса Person
- `FileResourceManagerTests` - тесты для класса FileResourceManager

## Инструкция по запуску

### Требования

- .NET SDK 10.0 или выше
- IDE с поддержкой C# (Visual Studio, VS Code, Rider и т.д.)

### Запуск проекта

1. Откройте терминал в папке проекта `Lab1`
2. Восстановите зависимости (если необходимо):
   ```bash
   dotnet restore
   ```
3. Соберите проект:
   ```bash
   dotnet build
   ```
4. Запустите программу:
   ```bash
   dotnet run
   ```

### Запуск тестов

Для запуска всех тестов используйте команду:

```bash
dotnet test
```

Для запуска с подробным выводом:

```bash
dotnet test --verbosity normal
```

## Пример использования

```csharp
// Создание объекта Person
var person = new Person
{
    FirstName = "Иван",
    LastName = "Иванов",
    Age = 25,
    Email = "ivan@example.com",
    PhoneNumber = "+7-999-123-45-67",
    Id = "12345",
    BirthDate = new DateTime(1998, 5, 15)
};

// Сериализация
var serializer = new PersonSerializer();
string json = serializer.SerializeToJson(person);

// Сохранение в файл
serializer.SaveToFile(person, "person.json");

// Загрузка из файла
Person loadedPerson = serializer.LoadFromFile("person.json");

// Работа с FileResourceManager
using (var fileManager = new FileResourceManager("test.txt", FileMode.Create))
{
    fileManager.OpenForWriting();
    fileManager.WriteLine("Привет, мир!");
    
    fileManager.OpenForReading();
    string content = fileManager.ReadAllText();
}
```

