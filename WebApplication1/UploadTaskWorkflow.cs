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
            var videoId = Guid.NewGuid().ToString() + ".mp4";
            Console.WriteLine($"Downloading videos from VMS {taskId}=>{videoId}");
            redis.GetDatabase().HashSet($"upload:{taskId}", "videoId", videoId);
        }

        public void GetUploadVideoUrl(string? taskId)
        {
            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            Console.WriteLine($"Request upload videos URL {taskId}=>{videoId}");
            var uploadUrl = "https://s3.com/" + Guid.NewGuid().ToString() + ".mp4";
            redis.GetDatabase().HashSet($"upload:{taskId}", "uploadUrl", uploadUrl);
        }

        public void UploadVideoFileTask(string? taskId) {
            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            var uploadUrl = redis.GetDatabase().HashGet($"upload:{taskId}", "uploadUrl");
            Console.WriteLine($"Uploading Video ... {taskId}:{videoId}=>{uploadUrl}");
        }

        public void UpdateExportStatus(string? taskId)
        {
            var videoId = redis.GetDatabase().HashGet($"upload:{taskId}", "videoId");
            var uploadUrl = redis.GetDatabase().HashGet($"upload:{taskId}", "uploadUrl");
            Console.WriteLine($"Update Export Status {taskId}:{videoId}=>{uploadUrl}");
            new UpdateExportTaskWorkflow(redis).Run(taskId, videoId);
            redis.GetDatabase().KeyDelete($"upload:{taskId}");
        }

    }
}
