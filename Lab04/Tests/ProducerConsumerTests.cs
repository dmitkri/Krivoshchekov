using System.Threading;
using Xunit;

namespace Lab04.Tests;

public class ProducerConsumerTests
{
    [Fact]
    public void TestCreation()
    {
        var pc = new ProducerConsumer(5);
        Assert.NotNull(pc);
    }

    [Fact]
    public void TestProducerConsumer_WorksCorrectly()
    {
        var pc = new ProducerConsumer(3);
        
        pc.StartProducer(1, 5);
        pc.StartConsumer(1);
        
        Thread.Sleep(3000);
        
        pc.Stop();
        pc.Dispose();
    }

    [Fact]
    public void TestMultipleProducersAndConsumers()
    {
        var pc = new ProducerConsumer(5);
        
        pc.StartProducer(1, 3);
        pc.StartProducer(2, 3);
        pc.StartConsumer(1);
        pc.StartConsumer(2);
        
        Thread.Sleep(4000);
        
        pc.Stop();
        pc.Dispose();
    }
}

