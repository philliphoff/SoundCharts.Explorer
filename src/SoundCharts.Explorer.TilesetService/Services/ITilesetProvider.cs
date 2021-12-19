namespace SoundCharts.Explorer.TilesetService.Services;

internal record Tileset(string Id, string Name, string Url);

internal interface ITilesetProvider
{
    Task<IEnumerable<Tileset>> GetTilesets();

    Task<Tileset?> GetTilesetById(string id);

    Task<Uri?> GetTilesetDownloadUriById(string id);
}
