using System;
using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using SoundCharts.Explorer.MacOS.Services.Tilesets;

namespace SoundCharts.Explorer.MacOS.Views
{
    [Register("SourceListView")]
    internal sealed partial class SourceListView : NSOutlineView
    {
        private readonly SourceListDataSource dataSource;

        public SourceListView()
        {
            Initialize(out this.dataSource);
        }

        public SourceListView(IntPtr handle)
            : base(handle)
        {
            Initialize(out this.dataSource);
        }

        public SourceListView(NSCoder coder)
            : base(coder)
        {
            Initialize(out this.dataSource);
        }

        public SourceListView(NSObjectFlag flag)
            : base(flag)
        {
            Initialize(out this.dataSource);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.dataSource.TilesetsChanged -= OnTilesetsChanged;

                    this.dataSource.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void Initialize(out SourceListDataSource dataSource)
        {
            dataSource = new SourceListDataSource(AppDelegate.Services.GetRequiredService<ITilesetManager>());

            this.DataSource = dataSource;

            dataSource.TilesetsChanged += OnTilesetsChanged;

            this.Delegate = new SourceListDelegate(this);
        }

        private void OnTilesetsChanged(object sender, EventArgs e)
        {
            this.ReloadData();
        }
    }
}

