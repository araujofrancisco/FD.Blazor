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
    public class ProducerConsumerQueue : IDisposable
    {
        class WorkItem
        {
            public readonly TaskCompletionSource<object> TaskSource;
            public readonly Action Action;
            public readonly CancellationToken? CancelToken;
            public readonly Guid Guid;

            public WorkItem(
                TaskCompletionSource<object> taskSource,
                Guid guid,
                Action action,
                CancellationToken? cancelToken)
            {
                TaskSource = taskSource;
                Guid = guid;
                Action = action;
                CancelToken = cancelToken;
            }
        }

        public event EventHandler<Guid> StartingTask;
        public event EventHandler<Guid> CompletedTask;

        readonly BlockingCollection<WorkItem> _taskQ = new();

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

        public Task EnqueueTask(Action action) =>
            EnqueueTask(Guid.NewGuid(), action);

        public Task EnqueueTask(Guid guid, Action action) =>
            EnqueueTask(guid, action, null);

        public Task EnqueueTask(Guid guid, Action action, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            _taskQ.Add(new WorkItem(tcs, guid, action, cancelToken));
            return tcs.Task;
        }

        /// <summary>
        /// Consume items in order (FIFO).
        /// </summary>
        void Consume()
        {
            while (!_taskQ.IsCompleted)
            {
                WorkItem workItem = _taskQ.Take();
                if (workItem.CancelToken.HasValue && workItem.CancelToken.Value.IsCancellationRequested)
                    workItem.TaskSource.SetCanceled();
                else
                    try
                    {
                        StartingTask?.Invoke(workItem, workItem.Guid);
                        workItem.Action();
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
