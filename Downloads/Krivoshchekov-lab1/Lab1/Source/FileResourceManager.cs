using System;
using System.IO;
using System.Text;

namespace Lab1
{
    public class FileResourceManager : IDisposable
    {
        private FileStream? _fileStream;
        private StreamWriter? _writer;
        private StreamReader? _reader;
        private bool _disposed = false;
        private string _filePath;
        
        public FileResourceManager(string filePath, FileMode fileMode)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));
            
            _filePath = filePath;
            
            try
            {
                _fileStream = new FileStream(_filePath, fileMode, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception ex)
            {
                throw new IOException($"Не удалось открыть файл {filePath}: {ex.Message}", ex);
            }
        }
        
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileResourceManager));
        }
        
        public void OpenForWriting()
        {
            CheckDisposed();
            
            if (_fileStream == null)
                throw new InvalidOperationException("Файловый поток не инициализирован");
            
            try
            {
                _writer?.Dispose();
                _fileStream.Position = 0;
                _writer = new StreamWriter(_fileStream, Encoding.UTF8, leaveOpen: true);
            }
            catch (Exception ex)
            {
                throw new IOException($"Не удалось открыть файл для записи: {ex.Message}", ex);
            }
        }
        
        public void OpenForReading()
        {
            CheckDisposed();
            
            if (_fileStream == null)
                throw new InvalidOperationException("Файловый поток не инициализирован");
            
            try
            {
                _reader?.Dispose();
                _fileStream.Position = 0;
                _reader = new StreamReader(_fileStream, Encoding.UTF8, leaveOpen: true);
            }
            catch (Exception ex)
            {
                throw new IOException($"Не удалось открыть файл для чтения: {ex.Message}", ex);
            }
        }
        
        public void WriteLine(string text)
        {
            CheckDisposed();
            
            if (_writer == null)
                throw new InvalidOperationException("Файл не открыт для записи. Вызовите OpenForWriting()");
            
            try
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка записи в файл: {ex.Message}", ex);
            }
        }
        
        public string ReadAllText()
        {
            CheckDisposed();
            
            if (_reader == null)
                throw new InvalidOperationException("Файл не открыт для чтения. Вызовите OpenForReading()");
            
            try
            {
                _reader.BaseStream.Position = 0;
                _reader.DiscardBufferedData();
                return _reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка чтения файла: {ex.Message}", ex);
            }
        }
        
        public void AppendText(string text)
        {
            CheckDisposed();
            
            if (_fileStream == null)
                throw new InvalidOperationException("Файловый поток не инициализирован");
            
            try
            {
                using (var writer = new StreamWriter(_fileStream, Encoding.UTF8, leaveOpen: true))
                {
                    _fileStream.Position = _fileStream.Length;
                    writer.Write(text);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка добавления текста в файл: {ex.Message}", ex);
            }
        }
        
        public FileInfo GetFileInfo()
        {
            CheckDisposed();
            
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Файл не найден: {_filePath}");
            
            try
            {
                return new FileInfo(_filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка получения информации о файле: {ex.Message}", ex);
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _writer?.Dispose();
                    _reader?.Dispose();
                    _fileStream?.Dispose();
                }
                
                _writer = null;
                _reader = null;
                _fileStream = null;
                _disposed = true;
            }
        }
        
        ~FileResourceManager()
        {
            Dispose(false);
        }
    }
}
