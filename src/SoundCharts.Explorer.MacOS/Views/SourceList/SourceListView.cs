using System;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
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
                    this.dataSource.ItemsChanged -= OnTilesetsChanged;

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
            this.dataSource.ItemsChanged += OnTilesetsChanged;

            this.DataSource = dataSource;
            this.Delegate = @delegate;
        }

        private void OnTilesetsChanged(object sender, EventArgs e)
        {
            this.ReloadData();
            this.ExpandItem(null, expandChildren: true);
        }
    }
}

