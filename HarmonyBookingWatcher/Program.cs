using System.Net;
using System.Reflection;
using HarmonyBookingWatcher.Jobs;
using HarmonyBookingWatcher.Services.Implementations;
using HarmonyBookingWatcher.Services.Interfaces;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File($"Logs/{Assembly.GetExecutingAssembly().GetName().Name}.log")
    //.WriteTo.Console()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IContentGetter, ContentGetter>();
builder.Services.AddTransient<IMessenger, TelegramMessengerImpl>();
builder.Services.AddTransient<IRoomChecker, DailyRoomCheckerImpl>();
builder.Services.AddTransient<IContentGetter, ContentGetter>();
builder.Services.AddQuartz(q =>
{
    // Just use the name of your job that you created in the Jobs folder.
    var jobKey = new JobKey("CheckBookingJob");
    q.AddJob<CheckDailyBookingJob>(opts => opts.WithIdentity(jobKey));
    
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("CheckDailyBookingJob-trigger")
        //This Cron interval can be described as "run every minute" (when second is zero)
        .WithCronSchedule("0 * * ? * *")
    );
    
    var jobKey2 = new JobKey("CheckMonthlyBookingJob");
    q.AddJob<CheckMonthlyBookingJob>(opts => opts.WithIdentity(jobKey2));
    
    q.AddTrigger(opts => opts
        .ForJob(jobKey2)
        .WithIdentity("CheckMonthlyBookingJob-trigger")
        //This Cron interval can be described as "run every hour" (when second is zero)
        // .WithCronSchedule("0 0 0 ? * * *")
        .WithCronSchedule("0 * * ? * *")

    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.WebHost.UseKestrel(so =>
{
    so.Limits.MaxConcurrentConnections = 100;
    so.Limits.MaxConcurrentUpgradedConnections = 100;
    so.Limits.MaxRequestBodySize = 52428800;
    so.Listen(IPAddress.Any, 9001);
}); 
builder.Services.AddControllers();

var app = builder.Build();

//var loggerFactory = app.Services.GetService<ILoggerFactory>();
//loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString()); 

// app.UseHttpsRedirection();
//
// app.UseAuthorization();

app.MapControllers();



app.Run();