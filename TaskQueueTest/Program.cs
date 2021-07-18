using System;
using System.Linq;
using System.Threading.Tasks;
using FD.Blazor.Core;

namespace TaskQueueTest
{
    class Program
    {
        private static readonly TaskQueue<dynamic> tq = new(1);
        private static readonly Random rnd = new();

        static void Main(string[] args)
        {
            tq.QueueTask += OnQueueTask;
            tq.StartingTask += OnStartingTask;
            tq.CompletedTask += OnCompletedTask;

            // queue some jobs to execute using our TaskQueue
            Task producer = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 30; ++i)
                {
                    var jobId = i;
                    // odd jobs runs synchronously
                    // jobs with id below 15 return integer, otherwise return a string
                    if (jobId % 2 == 0)
                        if (jobId < 15)
                            tq.Enqueue(async () => await someWorkAsync<int>(jobId));
                        else
                            tq.Enqueue(async () => await someWorkAsync<string>(jobId));
                    else
                        if (jobId < 15)
                        tq.Enqueue(() => someWork<int>(jobId));
                    else
                        tq.Enqueue(() => someWork<string>(jobId));
                }
                tq.CompleteAdding();
            });
            // wait on producer and queue tasks to complete
            Task.WaitAll(producer);
            Task.WaitAll(tq.GetTasks().ToArray());
        }

        private static int GetTimeout() =>
            rnd.Next(1000 * 5);     // random timeout 0 - 5 seconds

        private static TResult someWork<TResult>(int jobId)
        {
            int timeout = GetTimeout();
            Task.Delay(timeout);
            Console.WriteLine($"Task ({jobId}) done synchronously.");


            return (TResult)(typeof(TResult) == typeof(string) ? 
                Convert.ChangeType($"{timeout:X4}", typeof(TResult)) : 
                Convert.ChangeType(timeout, typeof(TResult)));
        }

        private static async Task<TResult> someWorkAsync<TResult>(int jobId)
        {
            int timeout = GetTimeout();
            await Task.Delay(timeout);
            Console.WriteLine($"Task ({jobId}) done asynchronously.");

            return (TResult)(typeof(TResult) == typeof(string) ?
                Convert.ChangeType($"{timeout:X4}", typeof(TResult)) :
                Convert.ChangeType(timeout, typeof(TResult)));
        }

        #region " Queue events "
        private static void OnQueueTask(object sender, Guid e) =>
            Console.WriteLine($"Task {e} queue");

        private static void OnStartingTask(object sender, Guid e) =>
            Console.WriteLine($"Starting task {e}");

        private static void OnCompletedTask(object sender, Guid e) 
        {
            var workItem = (ProducerConsumerQueue<dynamic>.WorkItem<dynamic>)sender;
            var result = workItem.GetResult();
            Console.WriteLine($"Completed task {e} with a timeout of {result}");
        }
        #endregion
    }
}
