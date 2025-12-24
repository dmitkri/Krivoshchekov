using System.Threading;
using Xunit;

namespace Lab04.Tests;

public class SleepingBarberTests
{
    [Fact]
    public void TestBarberCreation()
    {
        var barber = new SleepingBarber(3);
        Assert.NotNull(barber);
    }

    [Fact]
    public void TestClientArrives_Success()
    {
        var barber = new SleepingBarber(5);
        barber.StartBarber();

        bool result = barber.ClientArrives(1);
        Assert.True(result);

        Thread.Sleep(100);
        Assert.True(barber.GetQueueLength() >= 0);
    }

    [Fact]
    public void TestFullWaitingRoom_ClientRejected()
    {
        var barber = new SleepingBarber(2);
        barber.StartBarber();
        barber.ClientArrives(1);
        barber.ClientArrives(2);
        
        bool result = barber.ClientArrives(3);
        Assert.False(result);
    }
}

