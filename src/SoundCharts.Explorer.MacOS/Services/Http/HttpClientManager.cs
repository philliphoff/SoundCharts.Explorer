using System.Net.Http;
using System.Net.Http.Headers;

namespace SoundCharts.Explorer.MacOS.Services.Http
{
	internal sealed class HttpClientManager : IHttpClientManager
	{
		public HttpClientManager()
		{
			this.CurrentClient = new HttpClient();

			// TODO: Substitute automated version/build number.
			this.CurrentClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SoundCharts.Explorer", "0.1"));
		}

        #region IHttpClientManager Members

        public HttpClient CurrentClient { get; }

		#endregion
	}
}

