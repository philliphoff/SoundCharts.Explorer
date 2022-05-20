using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class OfflineTilesetsItem : HeaderItem, IDisposable
{
    private readonly ITilesetManager tilesetManager;
    private readonly IDisposable tilesetsSubscription;

    private IImmutableSet<ManagedTileset> tilesets = ImmutableHashSet<ManagedTileset>.Empty;

    public OfflineTilesetsItem(ITilesetManager tilesetManager, Action<IImmutableSet<ExplorerItem>> onChanged)
        : base("Tilesets")
    {
        this.tilesetManager = tilesetManager ?? throw new ArgumentNullException(nameof(tilesetManager));

        this.tilesetsSubscription =
            tilesetManager
                .Tilesets
                .DistinctUntilChanged()
                .Subscribe(
                    tilesets =>
                    {
                        this.tilesets = tilesets;

                        onChanged(ImmutableHashSet<ExplorerItem>.Empty);
                    });
    }

    public override IImmutableList<ExplorerItem> GetChildren()
    {
        return tilesets.Select(this.ToTilesetItem).ToImmutableList();
    }

    #region IDisposable Members

    public void Dispose()
    {
        this.tilesetsSubscription.Dispose();
    }

    #endregion

    private ExplorerItem ToTilesetItem(ManagedTileset tileset)
    {
        return new OfflineTilesetItem(
            tileset.Description ?? tileset.Id,
            tileset.Name ?? tileset.Id,
            async () =>
            {
                switch (tileset.State)
                {
                    case TilesetState.Downloaded:

                        await tilesetManager.DeleteTilesetAsync(tileset.Id);

                        break;

                    case TilesetState.NotDownloaded:

                        await tilesetManager.DownloadTilesetAsync(tileset.Id);

                        break;
                }

            },
            tileset.State == TilesetState.NotDownloaded);
    }
}
