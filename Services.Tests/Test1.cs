using System.Diagnostics;
using Chetch.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Services.Tests;

[TestClass]
public sealed class Test1
{
    public class TestService : Service<TestService>
    {
        public TestService(ILogger<TestService> logger) : base(logger)
        {
        }

        protected override Task Execute(CancellationToken stoppingToken)
        {
            return Task.Delay(100);
        }
    }

    [TestMethod]
    public void TestConstruction()
    {
        var factory = new LoggerFactory();
        var logger = new Logger<TestService>(factory);
        List<TestService> instances = [];
        Debug.Print("Wtf");
        for(int i = 0; i < 130; i++)
        {
            try
            {
                var ts = new TestService(logger);
                instances.Add(ts);
            } catch (Exception e)
            {
                Debug.Print(e.Message);
                throw;
            }
        }
    }
}
