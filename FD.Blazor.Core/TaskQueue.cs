using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FD.Blazor.Core
{
    /// <summary>
    /// Task Queue implemented using a ProducerConsumerQueue cascading Starting/Completed task events. Adds a Guid id for tasks queue allowing to 
    /// get the task with it.
    /// </summary>
    /// <typeparam name="T">Type of return for tasks queue</typeparam>
    public class TaskQueue<T>
        where T : new()
    {
        private readonly ProducerConsumerQueue<T> _pcQ;
        private readonly Dictionary<Guid, Task> _qTasks;

        public event EventHandler<Guid> QueueTask;
        public event EventHandler<Guid> StartingTask;
        public event EventHandler<Guid> CompletedTask;

        public int Count { get => _qTasks.Count; }

        public TaskQueue(int? workers = 1)
        {
            _pcQ = new((int)workers);
            _qTasks = new();

            _pcQ.StartingTask += OnStartingTask;
            _pcQ.CompletedTask += OnCompletedTask;
        }

        protected virtual void OnStartingTask(object sender, Guid e) =>
            StartingTask?.Invoke(sender, e);

        protected virtual void OnCompletedTask(object sender, Guid e)
        {
            // removes task from tracking dictionary and trigger completion event
            RemoveTask(e, out Task task);
            CompletedTask?.Invoke(sender, e);
        }
        public Task Enqueue(Func<T> function) =>
            EnqueueTask(function);

        public Task Enqueue(Func<Task<T>> function) =>
            EnqueueTask(function);
        
        private Task EnqueueTask(dynamic function)
        {
            Guid guid = Guid.NewGuid();

            Task newTask = _pcQ.EnqueueTask(guid, function);
            _qTasks.Add(guid, newTask);
            QueueTask?.Invoke(newTask, guid);

            return newTask;
        }

        public void CompleteAdding()
            => _pcQ.CompleteAdding();

        /// <summary>
        /// Find a task by guid, returning true if found and setting output to task parameter.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool GetTask(Guid guid, out Task task) =>
            _qTasks.TryGetValue(guid, out task);

        /// <summary>
        /// Remove a task from tracking dictionary leaving task in the queue.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool RemoveTask(Guid guid, out Task task) =>
            _qTasks.Remove<Guid, Task>(guid, out task);
    }
}
