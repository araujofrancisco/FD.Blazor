using System;
using System.Threading.Tasks;
using FD.Blazor.Core;

namespace TaskQueueTest
{
    class Program
    {
        private static TaskQueue<int> tq;

        static void Main(string[] args)
        {
            tq = new TaskQueue<int>(1);

            tq.QueueTask += OnQueueTask;
            tq.StartingTask += OnStartingTask;
            tq.CompletedTask += OnCompletedTask;

            Task producer = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 15; ++i)
                {
                    var jobid = i;
                    if (jobid % 2 == 0)
                        tq.Enqueue(async () => await someWorkAsync(jobid));
                    else
                        tq.Enqueue(() => someWork(jobid));
                }
                tq.CompleteAdding();
            });
            Task.WaitAll(producer);

            while (tq.Count > 0)
                Task.Delay(250);
        }

        private static int someWork(int i)
        {
            var rnd = new Random();
            int timeout = rnd.Next(1000 * 5);

            Task.Delay(timeout);
            Console.WriteLine($"Task ({i}) done synchronously.");

            return timeout;
        }

        private static async Task<int> someWorkAsync(int i)
        {
            var rnd = new Random();
            int timeout = rnd.Next(1000 * 5);

            await Task.Delay(timeout);
            Console.WriteLine($"Task ({i}) done asynchronously.");

            return timeout;
        }

        private static void OnQueueTask(object sender, Guid e) =>
            Console.WriteLine($"Task {e} queue");

        private static void OnStartingTask(object sender, Guid e) =>
            Console.WriteLine($"Starting task {e}");

        private static void OnCompletedTask(object sender, Guid e) 
        {
            var workItem = (ProducerConsumerQueue<int>.WorkItem<int>)sender;
            var x = workItem.GetResult();
            Console.WriteLine($"Completed task {e} with a timeout of {x}");
        }
    }
}
