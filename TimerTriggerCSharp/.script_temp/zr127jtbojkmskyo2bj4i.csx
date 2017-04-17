using System;
using System.Configuration;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now} " + ConfigurationSettings.AppSettings.Get("DATABASE_CONNECTION"));    
}