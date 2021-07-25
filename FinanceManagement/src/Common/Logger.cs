using System;
using Serilog;
using Serilog.Events;

namespace FinanceManagement.Common
{
    public static class Logger
    {
        private static readonly string Path = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", ".." , "logs", $"log-{DateTime.Now:d}.log");

        public static readonly Serilog.Core.Logger Log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(LogEventLevel.Warning)
            .WriteTo.File(Path, LogEventLevel.Debug, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        public static void Info(string log)
        {
            Log.Information(log);
        }
        
        public static void Debug(string log)
        {
            Log.Debug(log);
        }
        
        public static void Warning(string log, Exception e = null)
        {
            Log.Warning(e, log);
        }
        
        public static void Error(string log, Exception e = null)
        {
            Log.Error(e, log);
        }
    }
}
