using System;
using System.Threading;

namespace Lab04;

public class DiningPhilosophers
{
    private const int PhilosopherCount = 5;
    private readonly object[] _forks = new object[PhilosopherCount];
    private readonly Thread[] _philosophers = new Thread[PhilosopherCount];

    public DiningPhilosophers()
    {
        for (int i = 0; i < PhilosopherCount; i++)
        {
            _forks[i] = new object();
        }
    }

    public void RunWithDeadlock(int durationSeconds)
    {
        Console.WriteLine("версия с дедлок (неправильная)");
        
        for (int i = 0; i < PhilosopherCount; i++)
        {
            int id = i;
            _philosophers[i] = new Thread(() => PhilosopherWithDeadlock(id));
            _philosophers[i].Start();
        }

        Thread.Sleep(durationSeconds * 1000);
        Console.WriteLine("Остановка философов...");
    }

    private void PhilosopherWithDeadlock(int id)
    {
        int leftFork = id;
        int rightFork = (id + 1) % PhilosopherCount;

        while (true)
        {
            Think(id);
            lock (_forks[leftFork])
            {
                Console.WriteLine($"Философ {id} взял левую вилку {leftFork}");
                lock (_forks[rightFork])
                {
                    Console.WriteLine($"Философ {id} взял правую вилку {rightFork}");
                    Eat(id);
                }
                Console.WriteLine($"Философ {id} положил правую вилку {rightFork}");
            }
            Console.WriteLine($"Философ {id} положил левую вилку {leftFork}");
        }
    }

    public void RunWithoutDeadlock(int durationSeconds)
    {
        Console.WriteLine("версия семафор");
        using (var semaphore = new SemaphoreSlim(PhilosopherCount - 1, PhilosopherCount - 1))
        {
            for (int i = 0; i < PhilosopherCount; i++)
            {
                int id = i;
                _philosophers[i] = new Thread(() => PhilosopherWithoutDeadlock(id, semaphore));
                _philosophers[i].Start();
            }

            Thread.Sleep(durationSeconds * 1000);
            
            Console.WriteLine("Остановка философов");
        }
    }

    private void PhilosopherWithoutDeadlock(int id, SemaphoreSlim semaphore)
    {
        int leftFork = id;
        int rightFork = (id + 1) % PhilosopherCount;

        while (true)
        {
            Think(id);
            semaphore.Wait();
            
            try
            {
                int firstFork = Math.Min(leftFork, rightFork);
                int secondFork = Math.Max(leftFork, rightFork);
                
                lock (_forks[firstFork])
                {
                    lock (_forks[secondFork])
                    {
                        Console.WriteLine($"Философ {id} ест");
                        Eat(id);
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    private void Think(int id)
    {
        Console.WriteLine($"Философ {id} думает...");
        Thread.Sleep(new Random().Next(100, 500));
    }

    private void Eat(int id)
    {
        Thread.Sleep(new Random().Next(200, 800));
        Console.WriteLine($"Философ {id} закончил есть");
    }

    public void Stop()
    {
        foreach (var philosopher in _philosophers)
        {
            philosopher?.Interrupt();
        }
    }
}

