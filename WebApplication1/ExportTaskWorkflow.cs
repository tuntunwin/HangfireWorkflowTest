using Hangfire;
using StackExchange.Redis;

namespace WebApplication1
{
    public class ExportTaskWorkflow
    {
        ConnectionMultiplexer redis;
        public ExportTaskWorkflow(ConnectionMultiplexer redis) { this.redis = redis; }
        public void Run(string taskId) {
            var exists = redis.GetDatabase().HashExists($"export:{taskId}", "status");
            if (exists) {
                Console.WriteLine($"{taskId} already started");
                return;
            }
            redis.GetDatabase().HashSet($"export:{taskId}", "status", "new");
            var jobId = BackgroundJob.Enqueue(() => AddExportTask(taskId));
            BackgroundJob.ContinueJobWith(jobId, () => ExecuteExportTask(taskId));
            
        }

        public void AddExportTask(string? taskId) {
            ExceptionSimulator.Trigger("Before AddExportTask");

            Console.WriteLine($"Saving export task record into db {taskId}");
            redis.GetDatabase().HashSet($"export:{taskId}", "status", "started");

            ExceptionSimulator.Trigger("After AddExportTask");
        }

        public void ExecuteExportTask(string? taskId) {
            ExceptionSimulator.Trigger("Before ExecuteExportTask");

            Console.WriteLine($"Call Execute Export Task API {taskId}");
            redis.GetDatabase().HashSet($"export:{taskId}", "status", "executed");
            ExceptionSimulator.Trigger("After ExecuteExportTask");

            new UploadTaskWorkflow(this.redis).Run(taskId);

            
        }
       
    }
}
