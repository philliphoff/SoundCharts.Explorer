using System;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace SoundCharts.Explorer.MacOS.Services.Logging
{
	internal sealed class OSLogger : ILogger
	{
        private readonly IntPtr osLogger;

		public OSLogger(string categoryName)
		{
            this.osLogger = Explorer.Logging.OSLog.os_log_create("com.Onca-Inc.SoundCharts-Explorer", categoryName);
		}

        #region ILogger Members

        public IDisposable BeginScope<TState>(TState state)
        {
            return Disposable.Empty;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Explorer.Logging.OSLog.Log(this.osLogger, formatter(state, exception));
        }

        #endregion

        
    }
}

