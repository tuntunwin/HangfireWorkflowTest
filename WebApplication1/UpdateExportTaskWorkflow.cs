using Hangfire;
using StackExchange.Redis;

namespace WebApplication1
{
    public class UpdateExportTaskWorkflow
    {
        ConnectionMultiplexer redis;
        public UpdateExportTaskWorkflow(ConnectionMultiplexer redis) { this.redis = redis; }
        public void Run(string taskId, string videoFileId) {
            var jobId = BackgroundJob.Enqueue(() => MoveUploadedVideoToVOD(taskId, videoFileId));
            BackgroundJob.ContinueJobWith(jobId, () => CompleteExportTask(taskId));
        }

        public void MoveUploadedVideoToVOD(string? taskId, string videoId) {
            ExceptionSimulator.Trigger("Before MoveUploadedVideoToVOD");

            Console.WriteLine($"Moving videos from Upload to VOD {taskId}=>{videoId}");
            redis.GetDatabase().HashSet($"export:{taskId}", "videoId", videoId);
            redis.GetDatabase().HashSet($"export:{taskId}", "status", "uploaded");
            
            ExceptionSimulator.Trigger("After MoveUploadedVideoToVOD");
        }

        public void CompleteExportTask(string? taskId)
        {
            ExceptionSimulator.Trigger("Before CompleteExportTask");

            var videoId = redis.GetDatabase().HashGet($"export:{taskId}", "videoId");
            Console.WriteLine($"Complete Export Task {taskId}=>{videoId}");
            redis.GetDatabase().HashSet($"export:{taskId}", "status", "completed");
            redis.GetDatabase().KeyDelete($"export:{taskId}");

            ExceptionSimulator.Trigger("After CompleteExportTask");
        }

    }
}
