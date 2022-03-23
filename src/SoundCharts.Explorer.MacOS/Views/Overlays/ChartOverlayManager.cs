using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using MapKit;
using SoundCharts.Explorer.Charts.Sources;
using SoundCharts.Explorer.MacOS.Utils;
using SoundCharts.Explorer.Utilities;

namespace SoundCharts.Explorer.MacOS.Views.Overlays;

internal sealed class ChartOverlayManager : IDisposable
{
    private readonly IDisposable chartsSubscription;
    private readonly MKMapView mapView;
    private IImmutableDictionary<Uri, ImageOverlay> overlays = ImmutableDictionary<Uri, ImageOverlay>.Empty;

    public ChartOverlayManager(IObservable<IImmutableSet<Uri>> chartManager, IChartSource chartSource, MKMapView mapView)
    {
        this.mapView = mapView ?? throw new ArgumentNullException(nameof(mapView));

        this.chartsSubscription =
            chartManager
                .ToChangeSet()
                .Select(
                    async changes =>
                    {
                        // TODO: Serialize this.
                        foreach (var name in changes.Added)
                        {
                            var chart = await chartSource.GetChartAsync(name);

                            if (chart != null)
                            {
                                var (bounds, center) = chart.Metadata.ToMapBounds();

                                // TODO: Add appropriate timeout.
                                var image = await chart.ToNSImageAsync();

                                var overlay = new ImageOverlay(image, bounds, center);

                                this.mapView.AddOverlay(overlay);

                                this.overlays = this.overlays.Add(name, overlay);
                            }
                        }

                        foreach (var name in changes.Removed)
                        {
                            if (this.overlays.TryGetValue(name, out ImageOverlay overlay))
                            {
                                this.mapView.RemoveOverlay(overlay);

                                this.overlays = this.overlays.Remove(name);
                            }
                        }
                    })
                .SubscribeOn(SynchronizationContext.Current)
                .Subscribe();
    }

    #region IDisposable Members

    public void Dispose()
    {
        this.chartsSubscription.Dispose();
        this.mapView.RemoveOverlays(this.overlays.Values.ToArray());
        this.overlays = this.overlays.Clear();
    }

    #endregion
}
