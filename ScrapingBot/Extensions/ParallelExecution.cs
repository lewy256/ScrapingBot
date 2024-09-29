using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapingBot.Extensions;

public static class ParallelExecution {
    public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, int maxConcurrency, Func<T, Task> action) {
        List<Task> tasks;
        if(items is ICollection<T> itemCollection) {
            tasks = new List<Task>(itemCollection.Count);
        }
        else {
            tasks = [];
        }

        using var semaphore = new SemaphoreSlim(maxConcurrency);
        foreach(T item in items) {
            tasks.Add(InvokeThrottledAction(item, action, semaphore));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task InvokeThrottledAction<T>(T item, Func<T, Task> action, SemaphoreSlim semaphore) {
        await semaphore.WaitAsync();
        try {
            await action(item);
        }
        finally {
            semaphore.Release();
        }
    }
}
