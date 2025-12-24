using System;
using System.Threading;

namespace Lab04;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Лабораторная работа 4");
        Console.WriteLine("Задача 1: Обедающие философы");
        
        var philosophers = new DiningPhilosophers();
        philosophers.RunWithoutDeadlock(3);
        Thread.Sleep(1000);

        Console.WriteLine("Задача 2: Слип парикмахер");
        
        var barber = new SleepingBarber(maxSeats: 5);
        barber.StartBarber();
        
        for (int i = 1; i <= 8; i++)
        {
            barber.ClientArrives(i);
            Thread.Sleep(new Random().Next(200, 800));
        }
        
        Thread.Sleep(5000);

        Console.WriteLine("Зачада 3: Производитель-Потребитель");
        
        var producerConsumer = new ProducerConsumer(bufferSize: 5);
        
        producerConsumer.StartProducer(1, 10);
        producerConsumer.StartProducer(2, 10);
        
        producerConsumer.StartConsumer(1);
        producerConsumer.StartConsumer(2);
        
        Thread.Sleep(5000);
        producerConsumer.Stop();
        producerConsumer.Dispose();

        ProducerConsumer.RunWithSemaphore(bufferSize: 5);
        
        Thread.Sleep(2000);
        Console.WriteLine("Программа завершена");
    }
}

