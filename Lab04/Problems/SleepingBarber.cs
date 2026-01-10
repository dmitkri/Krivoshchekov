using System;
using System.Collections.Generic;
using System.Threading;

namespace Lab04;

public class SleepingBarber
{
    private readonly Queue<Client> _waitingRoom;
    private readonly int _maxSeats;
    private readonly SemaphoreSlim _clientsSemaphore;
    private readonly Mutex _barberMutex;
    private readonly Mutex _waitingRoomMutex;
    private bool _isBarberSleeping;

    public SleepingBarber(int maxSeats)
    {
        _maxSeats = maxSeats;
        _waitingRoom = new Queue<Client>();
        _clientsSemaphore = new SemaphoreSlim(0);
        _barberMutex = new Mutex();
        _waitingRoomMutex = new Mutex();
        _isBarberSleeping = true;
    }

    public void StartBarber()
    {
        var barberThread = new Thread(BarberWork);
        barberThread.Start();
    }

    private void BarberWork()
    {
        while (true)
        {
            _clientsSemaphore.Wait();
            
            _barberMutex.WaitOne();
            _isBarberSleeping = false;
            _barberMutex.ReleaseMutex();

            Client? client = null;

            _waitingRoomMutex.WaitOne();
            if (_waitingRoom.Count > 0)
            {
                client = _waitingRoom.Dequeue();
            }
            _waitingRoomMutex.ReleaseMutex();

            if (client != null)
            {
                Console.WriteLine($"Парикмахер стрижёт клиента {client.Id}");
                Thread.Sleep(new Random().Next(1000, 3000));
                Console.WriteLine($"Клиент {client.Id} уходит");
            }

            _waitingRoomMutex.WaitOne();
            bool hasClients = _waitingRoom.Count > 0;
            _waitingRoomMutex.ReleaseMutex();

            if (!hasClients)
            {
                _barberMutex.WaitOne();
                _isBarberSleeping = true;
                _barberMutex.ReleaseMutex();
                Console.WriteLine("Парикмахер спит");
            }
        }
    }

    public bool ClientArrives(int clientId)
    {
        _waitingRoomMutex.WaitOne();
        
        if (_waitingRoom.Count >= _maxSeats)
        {
            _waitingRoomMutex.ReleaseMutex();
            Console.WriteLine($"Клиент {clientId} ушёл - нет мест");
            return false;
        }

        var client = new Client { Id = clientId };
        _waitingRoom.Enqueue(client);
        int queueLength = _waitingRoom.Count;
        _waitingRoomMutex.ReleaseMutex();

        Console.WriteLine($"Клиент {clientId} сел в очередь (в очереди: {queueLength})");
        
        _barberMutex.WaitOne();
        if (_isBarberSleeping)
        {
            Console.WriteLine($"Клиент {clientId} будит парикмахера");
        }
        _barberMutex.ReleaseMutex();
        _clientsSemaphore.Release();
        
        return true;
    }

    public int GetQueueLength()
    {
        _waitingRoomMutex.WaitOne();
        int length = _waitingRoom.Count;
        _waitingRoomMutex.ReleaseMutex();
        return length;
    }
}

