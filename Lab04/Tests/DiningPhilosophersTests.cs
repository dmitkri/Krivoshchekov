using System;
using System.Threading;
using Xunit;

namespace Lab04.Tests;

public class DiningPhilosophersTests
{
    [Fact]
    public void TestPhilosophersCreation()
    {
        var diningPhilosophers = new DiningPhilosophers();
        Assert.NotNull(diningPhilosophers);
    }

    [Fact]
    public void TestWithoutDeadlock_RunsWithoutFreezing()
    {
        var diningPhilosophers = new DiningPhilosophers();
        bool completed = false;

        var testThread = new Thread(() =>
        {
            diningPhilosophers.RunWithoutDeadlock(2);
            completed = true;
        });

        testThread.Start();
        bool finished = testThread.Join(TimeSpan.FromSeconds(5));

        Assert.True(finished, "Поток должен завершиться за разумное время");
        Assert.True(completed);
    }
}

