using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCharts.Explorer.Utilities
{
    public static class TaskExtensions
    {
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();

            CancellationTokenRegistration? registration = null;

            registration = cancellationToken.Register(
                () =>
                {
                    tcs.TrySetCanceled(cancellationToken);

                    registration?.Dispose();
                },
                useSynchronizationContext: false);

            task.ContinueWith(
                _ =>
                {
                    try
                    {
                        if (task.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else if (task.IsFaulted)
                        {
                            tcs.TrySetException(task.Exception);
                        }
                        else
                        {
                            tcs.TrySetResult(task.Result);
                        }
                    }
                    finally
                    {
                        registration?.Dispose();
                    }
                });

            return tcs.Task;
        }
    }
}

