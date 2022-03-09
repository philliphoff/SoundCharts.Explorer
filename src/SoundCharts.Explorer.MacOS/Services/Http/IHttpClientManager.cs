using System.Net.Http;

namespace SoundCharts.Explorer.MacOS.Services.Http
{
	internal interface IHttpClientManager
	{
		HttpClient CurrentClient { get; }
	}
}

