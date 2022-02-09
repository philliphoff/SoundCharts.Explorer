using Microsoft.Extensions.Logging;

namespace SoundCharts.Explorer.MacOS.Services.Logging
{
	public class OSLoggerProvider : ILoggerProvider
	{
		public OSLoggerProvider()
		{
		}

        public ILogger CreateLogger(string categoryName)
        {
            return new OSLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}

