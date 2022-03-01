using Microsoft.Extensions.Logging;

namespace SoundCharts.Explorer.MacOS.Services.Logging
{
	internal static class ILoggingBuilderExtensions
	{
		public static ILoggingBuilder AddMacConsole(this ILoggingBuilder builder)
		{
			builder.AddProvider(new OSLoggerProvider());

			return builder;
		}
	}
}

