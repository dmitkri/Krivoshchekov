using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Lab04;

public class ProducerConsumer
{
    private readonly BlockingCollection<int> _buffer;
    private readonly int _bufferSize;
    private bool _isRunning;

    public ProducerConsumer(int bufferSize)
    {
        _bufferSize = bufferSize;
        _buffer = new BlockingCollection<int>(boundedCapacity: bufferSize);
        _isRunning = true;
    }

    public void StartProducer(int producerId, int itemCount)
    {
        var thread = new Thread(() => Produce(producerId, itemCount));
        thread.Start();
    }

    private void Produce(int producerId, int itemCount)
    {
        for (int i = 0; i < itemCount; i++)
        {
            if (!_isRunning) break;

            int item = new Random().Next(1, 100);
            
            _buffer.Add(item);
            
            Console.WriteLine($"Производитель {producerId} произвёл товар: {item}");
            Thread.Sleep(new Random().Next(100, 500));
        }
        
        Console.WriteLine($"Производитель {producerId} закончил работу");
    }

    public void StartConsumer(int consumerId)
    {
        var thread = new Thread(() => Consume(consumerId));
        thread.Start();
    }

    private void Consume(int consumerId)
    {
        foreach (var item in _buffer.GetConsumingEnumerable())
        {
            if (!_isRunning && _buffer.Count == 0) break;

            Console.WriteLine($"Потребитель {consumerId} забрал товар: {item}");
            Thread.Sleep(new Random().Next(200, 800));
        }
        
        Console.WriteLine($"Потребитель {consumerId} закончил работу");
    }

    public static void RunWithSemaphore(int bufferSize)
    {
        Console.WriteLine("\n=== Альтернативная реализация (SemaphoreSlim + lock) ===");
        
        var buffer = new Queue<int>();
        var lockObject = new object();
        var emptySlots = new SemaphoreSlim(bufferSize, bufferSize);
        var filledSlots = new SemaphoreSlim(0, bufferSize);

        var producer = new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                emptySlots.Wait();
                
                lock (lockObject)
                {
                    buffer.Enqueue(i);
                    Console.WriteLine($"Произвёл: {i}");
                }
                
                filledSlots.Release();
            }
        });

        var consumer = new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                filledSlots.Wait(); 
                int item;
                lock (lockObject)
                {
                    item = buffer.Dequeue();
                }
                
                Console.WriteLine($"Забрал: {item}");
                emptySlots.Release();
            }
        });

        producer.Start();
        consumer.Start();
        producer.Join();
        consumer.Join();
    }

    public void Stop()
    {
        _isRunning = false;
        _buffer.CompleteAdding();
    }

    public void Dispose()
    {
        _buffer?.Dispose();
    }
}

