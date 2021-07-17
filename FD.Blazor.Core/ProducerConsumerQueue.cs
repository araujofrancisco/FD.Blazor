using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FD.Blazor.Core
{
     /// <summary>
    /// Implements a Producer/Consumer Queue using a BlockingCollection to execute task in order. Each task gets a GUID assigned
    /// and events are trigger before start and once completed the task.
    /// </summary>
    public class ProducerConsumerQueue<TQueue> : IDisposable
        where TQueue : new()
    {
        public class WorkItem<TItem>
            where TItem : new()
        {
            public readonly TaskCompletionSource<object> TaskSource;
            public readonly dynamic Function;
            public readonly CancellationToken? CancelToken;
            public readonly Guid Guid;
            public readonly bool IsRunningAsync;

            private TItem Result;

            public WorkItem(
                TaskCompletionSource<object> taskSource,
                Guid guid,
                dynamic function,
                CancellationToken? cancelToken,
                bool isRunningAsync)
            {
                TaskSource = taskSource;
                Guid = guid;
                Function = function;
                CancelToken = cancelToken;
                IsRunningAsync = isRunningAsync;
            }

            public async Task Execute() =>
                Result = this.IsRunningAsync ? await this.Function() : this.Function();
            
            public TItem GetResult() =>
                Result;
        }

        public event EventHandler<Guid> StartingTask;
        public event EventHandler<Guid> CompletedTask;

        readonly BlockingCollection<WorkItem<TQueue>> _taskQ = new();

        public ProducerConsumerQueue(int workerCount)
        {
            // Create and start a separate Task for each consumer
            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        public void Dispose()
        {
            _taskQ.CompleteAdding();
            GC.SuppressFinalize(this);
        }

        #region " Synchronous function "
        public Task EnqueueTask(Func<TQueue> function) =>
            EnqueueTask(Guid.NewGuid(), function);
        public Task EnqueueTask(Guid guid, Func<TQueue> function) =>
            EnqueueTask(guid, function, null);

        public Task EnqueueTask(Guid guid, Func<TQueue> function, CancellationToken? cancelToken) =>
            EnqueueTask(guid, function, cancelToken, false);
        #endregion

        #region " Asynchronous function "
        public Task EnqueueTask(Func<Task<TQueue>> function) =>
            EnqueueTask(Guid.NewGuid(), function);

        public Task EnqueueTask(Guid guid, Func<Task<TQueue>> function) =>
            EnqueueTask(guid, function, null);

        public Task EnqueueTask(Guid guid, Func<Task<TQueue>> function, CancellationToken? cancelToken) =>
            EnqueueTask(guid, function, cancelToken, true);
        #endregion

        private Task EnqueueTask(Guid guid, dynamic function, CancellationToken? cancelToken, bool IsRunningAsync)
        {
            var tcs = new TaskCompletionSource<object>();
            _taskQ.Add(new WorkItem<TQueue>(tcs, guid, function, cancelToken, IsRunningAsync));
            return tcs.Task;
        }

        public void CompleteAdding() =>
            _taskQ.CompleteAdding();

        /// <summary>
        /// Consume items in order (FIFO).
        /// </summary>
        private async Task Consume()
        {
            while (!_taskQ.IsCompleted)
            {
                WorkItem<TQueue> workItem = _taskQ.Take();
                if (workItem.CancelToken.HasValue && workItem.CancelToken.Value.IsCancellationRequested)
                    workItem.TaskSource.SetCanceled();
                else
                    try
                    {
                        StartingTask?.Invoke(workItem, workItem.Guid);
                        await workItem.Execute();
                        workItem.TaskSource.SetResult(null);   // Indicate completion
                        CompletedTask?.Invoke(workItem, workItem.Guid);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
            }
        }
    }
}
