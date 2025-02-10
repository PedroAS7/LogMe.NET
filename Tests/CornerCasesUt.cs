using LogMe;

namespace Test;

/// <summary>
/// Tests corner cases
/// </summary>
public class CornerCasesUt
{
    private delegate void LoggingFunction(string message);
    
    
    /// <summary>
    /// Makes sure that trying to add an already-registered provider does not work, and same goes for removing one
    /// </summary>
    [Test]
    public void RepeatedLogProviderTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);
        
        Assert.DoesNotThrow(() => logger.AddProvider(provider));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(provider));
        
        Assert.DoesNotThrow(() => logger.RemoveProvider(provider));
        Assert.Throws<ArgumentException>(() => logger.RemoveProvider(provider));
    }

    
    /// <summary>
    /// Tests functions with null or empty parameters
    /// </summary>
    [Test]
    public void EmptyOrNullParametersTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);
        logger.AddProvider(provider);
        
        Assert.Throws<ArgumentNullException>(() => logger.AddProvider(null));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(new StringBufferLog("", new StringWriter(), LogLevel.Trace)));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(new StringBufferLog("  ", new StringWriter(), LogLevel.Trace)));
        Assert.Throws<ArgumentNullException>(() => logger.RemoveProvider(null as LogProvider));
        Assert.Throws<ArgumentNullException>(() => logger.RemoveProvider(null as string));

        TestLogFunction(logger.Trace, sw);
        TestLogFunction(logger.Debug, sw);
        TestLogFunction(logger.Info, sw);
        TestLogFunction(logger.Warning, sw);
        TestLogFunction(logger.Error, sw);
        
        logger.Exception(null);
        Assert.That(sw.ToString(), Is.Not.Empty);
        
        logger.RemoveProvider(provider);
        return;

        void TestLogFunction(LoggingFunction func, StringWriter buffer)
        {
            buffer.GetStringBuilder().Clear();
            
            func(null);
            Assert.That(buffer.ToString(), Is.Empty);
            
            func("");
            Assert.That(buffer.ToString(), Is.Empty);
            
            func("         ");
            Assert.That(buffer.ToString(), Is.Empty);
        }
    }


    /// <summary>
    /// Makes sure that provider names cannot be deleted
    /// </summary>
    [Test]
    public void NoDeletingProvidersTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);
        logger.AddProvider(provider);
        
        Assert.That(logger.ProvidersNames, Is.Not.Empty);
        logger.ProvidersNames.SetValue(null, 0);
        Assert.That(logger.ProvidersNames, Is.Not.Empty);
        Assert.That(logger.ProvidersNames[0], Is.Not.Null);
        
        logger.RemoveProvider(provider);
    }
}