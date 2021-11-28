using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Sources
{
    public delegate Uri GetTileUriDelegate(TileIndex tileIndex);

    public sealed class HttpTileSource : ITileSource
	{
        private readonly HttpClient client;
        private readonly GetTileUriDelegate tileUriDelegate;

        public HttpTileSource(HttpClient client, GetTileUriDelegate tileUriDelegate)
		{
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.tileUriDelegate = tileUriDelegate ?? throw new ArgumentNullException(nameof(tileUriDelegate));
		}

        #region ITileSource Members

        public async Task<TileData?> GetTileAsync(TileIndex index, CancellationToken cancellationToken = default)
        {
            Uri uri = this.tileUriDelegate(index);

            using (var response = await this.client.GetAsync(uri, cancellationToken).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    return new TileData(GetTileFormat(response.Content.Headers), data);
                }
                else if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }

                return null;
            }
        }

        #endregion

        private static TileFormat GetTileFormat(HttpContentHeaders headers)
        {
            switch (headers.ContentType?.MediaType)
            {
                case "image/jpeg": return TileFormat.Jpeg;
                case "image/png": return TileFormat.Png;
                default: return TileFormat.Unknown;
            }
        }
    }
}

