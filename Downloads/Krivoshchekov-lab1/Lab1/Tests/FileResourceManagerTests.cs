using System;
using System.IO;
using Xunit;

namespace Lab1
{
    public class FileResourceManagerTests
    {
        private readonly string _testFilePath = "test_file.txt";
        
        private void CleanupTestFile()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }
        
        [Fact]
        public void OpenForWriting_OpensFileForWriting()
        {
            try
            {
                using var manager = new FileResourceManager(_testFilePath, FileMode.Create);
                manager.OpenForWriting();
                manager.WriteLine("Тестовая строка");
                Assert.True(File.Exists(_testFilePath));
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void OpenForReading_OpensFileForReading()
        {
            try
            {
                File.WriteAllText(_testFilePath, "Тестовый контент", System.Text.Encoding.UTF8);
                using var manager = new FileResourceManager(_testFilePath, FileMode.Open);
                manager.OpenForReading();
                string content = manager.ReadAllText();
                Assert.Contains("Тестовый контент", content);
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void WriteLine_WritesTextToFile()
        {
            try
            {
                using var manager = new FileResourceManager(_testFilePath, FileMode.Create);
                manager.OpenForWriting();
                manager.WriteLine("Первая строка");
                manager.WriteLine("Вторая строка");
                manager.OpenForReading();
                string content = manager.ReadAllText();
                Assert.Contains("Первая строка", content);
                Assert.Contains("Вторая строка", content);
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void AppendText_AppendsTextToEndOfFile()
        {
            try
            {
                File.WriteAllText(_testFilePath, "Начальный текст", System.Text.Encoding.UTF8);
                using var manager = new FileResourceManager(_testFilePath, FileMode.Open);
                manager.AppendText("\nДобавленный текст");
                manager.OpenForReading();
                string content = manager.ReadAllText();
                Assert.Contains("Начальный текст", content);
                Assert.Contains("Добавленный текст", content);
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void GetFileInfo_ReturnsFileInfo()
        {
            try
            {
                File.WriteAllText(_testFilePath, "Тест", System.Text.Encoding.UTF8);
                using var manager = new FileResourceManager(_testFilePath, FileMode.Open);
                FileInfo info = manager.GetFileInfo();
                Assert.NotNull(info);
                Assert.True(info.Exists);
                Assert.Equal(Path.GetFullPath(_testFilePath), info.FullName);
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void Dispose_ReleasesResources()
        {
            var manager = new FileResourceManager(_testFilePath, FileMode.Create);
            manager.Dispose();
            Assert.Throws<ObjectDisposedException>(() => manager.OpenForWriting());
            
            CleanupTestFile();
        }
        
        [Fact]
        public void UsingStatement_AutomaticallyDisposes()
        {
            try
            {
                using (var manager = new FileResourceManager(_testFilePath, FileMode.Create))
                {
                    manager.OpenForWriting();
                    manager.WriteLine("Тест");
                }
                Assert.True(File.Exists(_testFilePath));
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void WriteLine_WithoutOpenForWriting_ThrowsException()
        {
            try
            {
                using var manager = new FileResourceManager(_testFilePath, FileMode.Create);
                Assert.Throws<InvalidOperationException>(() => manager.WriteLine("Тест"));
            }
            finally
            {
                CleanupTestFile();
            }
        }
        
        [Fact]
        public void ReadAllText_WithoutOpenForReading_ThrowsException()
        {
            try
            {
                File.WriteAllText(_testFilePath, "Тест", System.Text.Encoding.UTF8);
                using var manager = new FileResourceManager(_testFilePath, FileMode.Open);
                Assert.Throws<InvalidOperationException>(() => manager.ReadAllText());
            }
            finally
            {
                CleanupTestFile();
            }
        }
    }
}

