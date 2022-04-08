using System;
using System.Reactive.Linq;
using System.Threading;
using AppKit;
using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    [Register("SourceListView")]
    internal sealed partial class SourceListView : NSOutlineView
    {
        private IDisposable? changedDataSubscription;

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
                if (disposing)
                {
                    this.changedDataSubscription?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void Initialize(ISourceListDataSource dataSource, SourceListDelegate @delegate)
        {
            if (dataSource is null)
            {
                throw new ArgumentNullException(nameof(dataSource));
            }

            this.changedDataSubscription =
                dataSource
                    .ChangedData
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(
                        data =>
                        {
                            this.ReloadData();
                            this.ExpandItem(null, expandChildren: true);
                        });

            this.DataSource = dataSource;
            this.Delegate = @delegate;
        }
    }
}

