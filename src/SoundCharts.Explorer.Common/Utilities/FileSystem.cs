using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace SoundCharts.Explorer.Utilities
{
	public static class FileSystem
	{
		public static IObservable<IImmutableSet<string>> CreateDirectoryObserver(string path, string filter)
        {
			IImmutableSet<string> GetFiles()
            {
				return Directory.Exists(path)
					? Directory.GetFiles(path, filter).ToImmutableHashSet()
					: ImmutableHashSet<string>.Empty;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
				return Observable.Defer(
					() =>
					{
						return Observable
							.Interval(TimeSpan.FromSeconds(2))
							.Select(_ => GetFiles())
							.DistinctUntilChanged(new ImmutableSetComparer<string>());
					});
            }
			else
            {
				return Observable.Defer(
					() => CreateObserver(path, filter)
						.Select(_ => GetFiles())
						.StartWith(GetFiles()));
            }
        }

		public static IObservable<FileSystemEventArgs> CreateObserver(string path, string filter)
        {
			return CreateObserver(() => new FileSystemWatcher(path, filter));
        }

		public static IObservable<FileSystemEventArgs> CreateObserver(Func<FileSystemWatcher> onCreation)
        {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
				throw new NotSupportedException("The Xamarin Mac platform does not support FileSystemWatcher.");
            }

			return Observable.Create<FileSystemEventArgs>(
				observer =>
                {
					void OnWatcherEvent(object sender, FileSystemEventArgs e)
					{
						observer.OnNext(e);
					}

					var watcher = onCreation();
					
                    watcher.Created += OnWatcherEvent;
					watcher.Deleted += OnWatcherEvent;

					watcher.EnableRaisingEvents = true;

					return watcher;
                });
        }

		private static IObservable<IImmutableSet<string>> CreateXamarinMacDirectoryObserver(string path, string filter)
		{
			IImmutableSet<string> GetFiles()
			{
				return Directory.Exists(path)
					? Directory.GetFiles(path, filter).ToImmutableHashSet()
					: ImmutableHashSet<string>.Empty;
			}

			return Observable.Defer(
				() => CreateObserver(path, filter)
					.Select(_ => GetFiles())
					.StartWith(GetFiles()));
		}
	}
}

