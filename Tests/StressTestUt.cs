using LogMe;

namespace Test;

public class StressTestUt
{
    private const int MinMessages = 100;
    private const int MaxMessages = 10000;
    private const string Message = "Example message with pad {0:000000}";
    private Logger _logger;

    [SetUp]
    public void Setup()
    {
        // Add logger to manager
        _logger = new Logger();
    }


    [TearDown]
    public void TearDown()
    {
        _logger.Close();
    }

    /// <summary>
    /// Performs the logging test
    /// </summary>
    /// <param name="providers">Array of log providers</param>
    private void RunTest(StringBufferLog[] providers)
    {
        for (var i = MinMessages; i <= MaxMessages; i *= 10)
        {
            foreach (var provider in providers)
            {
                provider.Clear();
            }

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            for (var j = 0; j < i; j++)
            {
                _logger.Info(string.Format(Message, j));
            }

            timer.Stop();
            TestContext.WriteLine("Writes took " + timer.ElapsedMilliseconds + " ms for " + i + " messages.");
        }
    }


    /// <summary>
    /// Writes messages to one provider with simple capabilities
    /// </summary>
    [Test]
    public void OneProviderTest()
    {
        var provider = new StringBufferLog("Logger", new StringWriter(), LogLevel.Trace);
        _logger.AddProvider(provider);
        RunTest(new[] { provider });
    }


    /// <summary>
    /// Writes messages to ten providers with simple capabilities
    /// </summary>
    [Test]
    public void TenProvidersTest()
    {
        var providers = new StringBufferLog[10];
        for (var i = 0; i < 10; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }


    /// <summary>
    /// Writes messages to one hundred providers with simple capabilities
    /// </summary>
    [Test]
    public void HundredProvidersTest()
    {
        var providers = new StringBufferLog[100];
        for (var i = 0; i < 100; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }


    /// <summary>
    /// Writes messages to one provider with timestamp logging enabled
    /// </summary>
    [Test]
    public void OneProviderWithTimeTest()
    {
        var provider = new StringBufferLog("Logger", new StringWriter(), LogLevel.Trace, LoggerFlags.Timestamp);
        _logger.AddProvider(provider);
        RunTest(new[] { provider });
    }


    /// <summary>
    /// Writes messages to ten providers with timestamp logging enabled
    /// </summary>
    [Test]
    public void TenProvidersWithTimeTest()
    {
        var providers = new StringBufferLog[10];
        for (var i = 0; i < 10; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace, LoggerFlags.Timestamp);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }


    /// <summary>
    /// Writes messages to one hundred providers with timestamp logging enabled
    /// </summary>
    [Test]
    public void HundredProvidersWithTimeTest()
    {
        var providers = new StringBufferLog[100];
        for (var i = 0; i < 100; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace, LoggerFlags.Timestamp);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }


    /// <summary>
    /// Writes messages to one provider with caller information logging enabled
    /// </summary>
    [Test]
    public void OneProviderWithCallerTest()
    {
        var provider = new StringBufferLog("Logger", new StringWriter(), LogLevel.Trace, LoggerFlags.Caller);
        _logger.AddProvider(provider);
        RunTest(new[] { provider });
    }


    /// <summary>
    /// Writes messages to ten providers with caller information logging enabled
    /// </summary>
    [Test]
    public void TenProvidersWithCallerTest()
    {
        var providers = new StringBufferLog[10];
        for (var i = 0; i < 10; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace, LoggerFlags.Caller);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }


    /// <summary>
    /// Writes messages to one hundred providers with caller information logging enabled
    /// </summary>
    [Test]
    public void HundredProvidersWithCallerTest()
    {
        var providers = new StringBufferLog[100];
        for (var i = 0; i < 100; ++i)
        {
            providers[i] = new StringBufferLog("Logger" + i, new StringWriter(), LogLevel.Trace, LoggerFlags.Caller);
            _logger.AddProvider(providers[i]);
        }

        RunTest(providers);
    }
}