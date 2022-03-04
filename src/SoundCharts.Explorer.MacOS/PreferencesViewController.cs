// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using SoundCharts.Explorer.MacOS.Services.State;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;
using System.Threading;

namespace SoundCharts.Explorer.MacOS
{
	public partial class PreferencesViewController : NSViewController
	{
        private IDisposable? stateSubscription;

		public PreferencesViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.apiEndpointTextField.EditingEnded += this.OnEditingEnded;

            if (this.stateSubscription is null)
            {
                var applicationStateManager = AppDelegate.Services.GetRequiredService<IApplicationStateManager>();

                this.stateSubscription =
                    applicationStateManager
                        .CurrentState
                        .Where(state => state.Context != this)
                        .Select(state => state.State?.ApiEndpoint)
                        .ObserveOn(SynchronizationContext.Current)
                        .Subscribe(
                            apiEndpoint =>
                            {
                                this.apiEndpointTextField.StringValue = apiEndpoint ?? String.Empty;
                            });
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.apiEndpointTextField.Changed -= this.OnEditingEnded;

                    if (this.stateSubscription != null)
                    {
                        this.stateSubscription.Dispose();
                        this.stateSubscription = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnEditingEnded(object sender, EventArgs e)
        {
            string apiEndpoint = this.apiEndpointTextField.StringValue;

            AppDelegate
                .Services
                .GetRequiredService<IApplicationStateManager>()
                .UpdateState(state => state is not null ? state with { ApiEndpoint = apiEndpoint } : null, this);
        }
    }
}