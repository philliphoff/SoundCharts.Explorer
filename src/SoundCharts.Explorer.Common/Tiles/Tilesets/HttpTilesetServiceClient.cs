using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Tiles.Tilesets
{
	public sealed class HttpTilesetServiceClient : ITilesetServiceClient
	{
        private readonly HttpClient httpClient;
        private readonly Uri serviceEndpoint;

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
        };

        public HttpTilesetServiceClient(HttpClient httpClient, Uri serviceEndpoint)
		{
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.serviceEndpoint = serviceEndpoint ?? throw new ArgumentNullException(nameof(serviceEndpoint));
		}

        #region ITilesetServiceClient Members

        public async Task<Stream> DownloadTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            var methodEndpoint = new Uri(this.serviceEndpoint, $"{id}/download");

            var stream = await this.httpClient.GetStreamAsync(methodEndpoint);

            return stream;
        }

        public async Task<Tileset> GetTilesetAsync(string id, CancellationToken cancellationToken = default)
        {
            var methodEndpoint = new Uri(this.serviceEndpoint, id);

            using var stream = await this.httpClient.GetStreamAsync(methodEndpoint);

            var model = await JsonSerializer.DeserializeAsync<TilesetModel>(stream, Options, cancellationToken);

            return model is not null ? new Tileset(model.Id) : null;
        }

        public async Task<IEnumerable<Tileset>> GetTilesetsAsync(CancellationToken cancellationToken = default)
        {
            var methodEndpoint = this.serviceEndpoint;

            using var stream = await this.httpClient.GetStreamAsync(methodEndpoint);

            var models = await JsonSerializer.DeserializeAsync<TilesetModel[]>(stream, Options, cancellationToken);

            return models is not null ? models.Select(model => new Tileset(model.Id)) : Enumerable.Empty<Tileset>();
        }

        #endregion

        private sealed class TilesetModel
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }
    }
}

