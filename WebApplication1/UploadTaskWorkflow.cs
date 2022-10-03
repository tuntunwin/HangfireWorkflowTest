using Hangfire;
using StackExchange.Redis;

namespace WebApplication1
{
    public class UploadTaskWorkflow
    {
        ConnectionMultiplexer redis;
        public UploadTaskWorkflow(ConnectionMultiplexer redis) { this.redis = redis; }
        public void Run(string taskId) {
            var jobId = BackgroundJob.Enqueue(() => DownloadVideoFromVMS(taskId));
            jobId = BackgroundJob.ContinueJobWith(jobId, () => GetUploadVideoUrl(taskId));
            jobId = BackgroundJob.ContinueJobWith(jobId, () => UploadVideoFileTask(taskId));
            BackgroundJob.ContinueJobWith(jobId, () => UpdateExportStatus(taskId));
            
        }

        public void DownloadVideoFromVMS(string? taskId) {
            ExceptionSimulator.Trigger("Before DownloadVideoFromVMS");

            var videoId = Guid.NewGuid().ToString() + ".mp4";
            Console.WriteLine($"Downloading videos from VMS {taskId}=>{videoId}");
            redis.GetDatabase().HashSet($"upload:{taskId}", "videoId", videoId);

            ExceptionSimulator.Trigger("After DownloadVideoFromVMS");
        }

        public void GetUploadVideoUrl(string? taskId)
        {
            ExceptionSimulator.Trigger("Before GetUploadVideoUrl");

            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            Console.WriteLine($"Request upload videos URL {taskId}=>{videoId}");
            var uploadUrl = "https://s3.com/" + Guid.NewGuid().ToString() + ".mp4";
            redis.GetDatabase().HashSet($"upload:{taskId}", "uploadUrl", uploadUrl);

            ExceptionSimulator.Trigger("After GetUploadVideoUrl");
        }

        public void UploadVideoFileTask(string? taskId) {
            ExceptionSimulator.Trigger("Before UploadVideoFileTask");

            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            var uploadUrl = redis.GetDatabase().HashGet($"upload:{taskId}", "uploadUrl");
            Console.WriteLine($"Uploading Video ... {taskId}:{videoId}=>{uploadUrl}");

            ExceptionSimulator.Trigger("After UploadVideoFileTask");
        }

        public void UpdateExportStatus(string? taskId)
        {
            ExceptionSimulator.Trigger("Before UpdateExportStatus");

            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            var uploadUrl = redis.GetDatabase().HashGet($"upload:{taskId}", "uploadUrl");

            ExceptionSimulator.Trigger("After UpdateExportStatus");

            Console.WriteLine($"Update Export Status {taskId}:{videoId}=>{uploadUrl}");
            new UpdateExportTaskWorkflow(redis).Run(taskId, videoId);
            redis.GetDatabase().KeyDelete($"upload:{taskId}");

        }

    }
}
