using Hangfire;
using StackExchange.Redis;
using WebApplication1;


var redis = ConnectionMultiplexer.Connect("localhost");
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(redis);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHangfire(configuration =>
{
    configuration.UseRedisStorage(redis);
});
builder.Services.AddHangfireServer();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseHangfireDashboard();

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5, DelaysInSeconds = new int[] { 1 } });
//if (args.Length > 0) {
//var taskId = args[0];
var taskId = "DDD";
    new ExportTaskWorkflow(redis).Run(taskId);
//}

app.Run();
