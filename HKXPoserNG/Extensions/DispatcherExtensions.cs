using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG.Extensions;

public static class DispatcherExtensions {
    public static void WaitForTaskAndContinue<T>(this Dispatcher dispatcher, Task<T> task, Action<T> continuation) {
        void DispatcherRecursion() {
            if (task.IsCompletedSuccessfully) {
                continuation(task.Result);
            } else if (task.IsFaulted) {
                throw new AggregateException("Task failed with an exception.", task.Exception);
            } else if (task.IsCanceled) {
                return;
            } else {
                dispatcher.InvokeAsync(DispatcherRecursion, DispatcherPriority.ApplicationIdle);
            }
        }
        DispatcherRecursion();
    }
}
