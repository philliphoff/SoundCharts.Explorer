using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Utilities
{
    public delegate Task<TResult> TaskCacheMissDelegate<TKey, TResult>(TKey key, CancellationToken cancellationToken = default);

    public sealed class TaskCache<TKey, TResult> : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TResult>>> tasks = new();
        private readonly CancellationTokenSource cts = new();

        public Task<TResult> GetTaskAsync(
            TKey key,
            TaskCacheMissDelegate<TKey, TResult> cacheMissDelegate,
            CancellationToken cancellationToken = default)
        {
            var lazy = this.tasks.GetOrAdd(
                key,
                _ => new Lazy<Task<TResult>>(
                    async () =>
                    {
                        try
                        {
                            return await cacheMissDelegate(key, cts.Token).ConfigureAwait(false);
                        }
                        finally
                        {
                            this.tasks.TryRemove(key, out Lazy<Task<TResult>> value);
                        }
                    }));

            return lazy.Value.WithCancellation(cancellationToken);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}

