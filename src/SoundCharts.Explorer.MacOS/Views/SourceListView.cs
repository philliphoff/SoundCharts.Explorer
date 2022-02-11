using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views
{
    [Register("SourceListView")]
    internal sealed partial class SourceListView : NSOutlineView
    {
        private SourceListDataSource? dataSource;

        public SourceListView()
        {
        }

        public SourceListView(IntPtr handle)
            : base(handle)
        {
        }

        public SourceListView(NSCoder coder)
            : base(coder)
        {
        }

        public SourceListView(NSObjectFlag flag)
            : base(flag)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.dataSource is not null)
                {
                    this.dataSource.TilesetsChanged -= OnTilesetsChanged;

                    this.dataSource = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void Initialize(SourceListDataSource dataSource, SourceListDelegate @delegate)
        {
            this.dataSource = dataSource;
            this.dataSource.TilesetsChanged += OnTilesetsChanged;

            this.DataSource = dataSource;
            this.Delegate = @delegate;
        }

        private void OnTilesetsChanged(object sender, EventArgs e)
        {
            this.ReloadData();
        }

        public override void ReloadData()
        {
            base.ReloadData();
        }
    }
}

