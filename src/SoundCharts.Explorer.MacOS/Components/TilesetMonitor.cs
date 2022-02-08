using System;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Components
{
	internal sealed class TilesetMonitor : IApplicationComponent
	{
        private readonly ITilesetManager tilesetManager;

        public TilesetMonitor(ITilesetManager tilesetManager)
		{
            this.tilesetManager = tilesetManager ?? throw new ArgumentNullException(nameof(tilesetManager));
		}

        #region IApplicationComponent Members

        public void Initialize()
        {
            this.tilesetManager
                .RefreshAsync()
                .ContinueWith(
                    task =>
                    {
                        // TODO: Log error on failure.
                    });
        }

        #endregion
    }
}

