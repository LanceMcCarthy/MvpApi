using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MvpApi.Wpf.Helpers
{
    public static class TaskUtilities
    {
        public static void RunOnDispatcherThreadSync(Action func, CancellationToken cancellationToken = default, Dispatcher dispatcher = null)
        {
            RunOnDispatcherThreadSync(() =>
            {
                func();
                return true;
            }, cancellationToken, dispatcher);
        }

        public static T RunOnDispatcherThreadSync<T>(Func<T> func, CancellationToken cancellationToken = default, Dispatcher dispatcher = null)
        {
            dispatcher ??= App.Current.MainWindow.Dispatcher;

            if (dispatcher.CheckAccess())
            {
                return func();
            }

            ExceptionDispatchInfo exceptionDispatchInfo = null;

            T result = default(T);

            dispatcher.InvokeAsync(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }

            }, DispatcherPriority.Normal, cancellationToken).Task.Wait(cancellationToken);

            exceptionDispatchInfo?.Throw();

            return result;
        }

        public static async Task RunOnDispatcherThreadAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dispatcher = App.Current.MainWindow.Dispatcher;

            if (!dispatcher.CheckAccess())
            {
                await dispatcher.InvokeAsync(action, priority, cancellationToken).Task.ConfigureAwait(false);
            }
            else
            {
                action();
            }
        }

        public static async Task RunOnDispatcherThreadAsync(Func<Task> action, CancellationToken cancellationToken = default(CancellationToken))
        {

            Task temp = null;
            var tcs = new TaskCompletionSource<bool>();

            await App.Current.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                temp = action();

                temp.ContinueWith(_ =>
                {
                    tcs.TrySetResult(true);

                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                temp.ContinueWith(task =>
                {
                    tcs.TrySetCanceled();

                }, TaskContinuationOptions.OnlyOnCanceled);

                temp.ContinueWith(task =>
                {
                    tcs.TrySetException(task.Exception ?? new Exception("Task failed"));

                }, TaskContinuationOptions.OnlyOnFaulted);

            }, DispatcherPriority.Normal, cancellationToken).Task.ConfigureAwait(false);

            try
            {
                await tcs.Task.ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                if (aggregateException.InnerException != null) 
                    throw aggregateException.InnerException;
            }
        }
    }
}