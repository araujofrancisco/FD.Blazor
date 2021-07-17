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
                    tq.Enqueue(async () => await someWork(jobid));
                }
            });
            Task.WaitAll(producer);

            while (tq.Count > 0)
                Task.Delay(500);
        }
        
        private static async Task<int> someWork(int i)
        {
            var rnd = new Random();
            int timeout = rnd.Next(1000 * 10);

            await Task.Delay(timeout);
            Console.WriteLine($"Task ({i}) done.");

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
            if (tq.RemoveTask(e, out Task task))
                Console.WriteLine($"Removed task {e} from queue");
        }
    }
}
