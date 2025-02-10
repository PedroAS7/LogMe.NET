using LogMe;


public class ThisSuperCoolApp
{
    static void Main(string[] args)
    {
        // Create the logger instance. This is the instance that will receive all logs and dispatch them
        var logger = new Logger();

        /*
		 * Create the providers you need
		 */

        // A log provider that forwards logs to the console, and shows the logs' timestamps. Max level is information
        var consoleLogProvider = new StdLog("Console logs",
                                            LogLevel.Info,
                                            LoggerFlags.Timestamp);

        // A log provider that forwards logs to a file, and shows the thread's information, as well as the caller's information
        // Logs all the logs it receives
        var fileLogProvider = new FileLog("File logs",
                                          "logFile.log",
                                          LogLevel.Trace,
                                          replace: true,
                                          extraFlags: LoggerFlags.Caller | LoggerFlags.Thread);

        // Register the providers
        logger.AddProvider(consoleLogProvider);
        logger.AddProvider(fileLogProvider);

        // Start logging!
        try
        {
            logger.Info("An informational message");
            logger.Debug("A debug message");
            var nullStr = null as string;
            if(nullStr.Contains("HA!"))
            {
                logger.Error("This shouldn't happen!");
            }
        }
        catch(NullReferenceException ex)
        {
            logger.Exception(ex);
        }

        // Close all log providers via the logger
        logger.Close();
    }
}