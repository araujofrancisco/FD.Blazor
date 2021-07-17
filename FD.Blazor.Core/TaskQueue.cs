﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FD.Blazor.Core
{
    public class TaskQueue
    {
        private readonly ProducerConsumerQueue _pcQ;
        private readonly Dictionary<Guid, Task> _qTasks;

        public event EventHandler<Guid> QueueTask;
        public event EventHandler<Guid> StartingTask;
        public event EventHandler<Guid> CompletedTask;

        public TaskQueue(int? workers = 1)
        {
            _pcQ = new((int)workers);
            _qTasks = new();

            _pcQ.StartingTask += OnStartingTask;
            _pcQ.CompletedTask += OnCompletedTask;
        }

        protected virtual void OnStartingTask(object sender, Guid e) =>
            StartingTask?.Invoke(sender, e);

        protected virtual void OnCompletedTask(object sender, Guid e) =>
            CompletedTask?.Invoke(sender, e);

        public Task AddTask(Action action)
        {
            Guid guid = Guid.NewGuid();

            Task newTask = _pcQ.EnqueueTask(guid, action);
            _qTasks.Add(guid, newTask);
            QueueTask?.Invoke(newTask, guid);

            return newTask;
        }

        public int Count() =>
            _qTasks.Count;

        /// <summary>
        /// Find a task by guid, returning true if found and setting output to task parameter.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool GetTask(Guid guid, out Task task) =>
            _qTasks.TryGetValue(guid, out task);

        public bool RemoveTask(Guid guid, out Task task) =>
            _qTasks.Remove<Guid, Task>(guid, out task);
    }
}