using HarmonyBookingWatcher.Dto;
using HarmonyBookingWatcher.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Quartz;

namespace HarmonyBookingWatcher.Jobs;

public class CheckDailyBookingJob : IJob
{
    private const string CacheKey = "harmonyBooking";
    private readonly IMemoryCache _cache;
    private bool _haveChanges;
    private readonly DateTime _now;
    private readonly ILogger<CheckDailyBookingJob> _logger;
    private readonly IDailyRoomChecker _roomChecker;
    private readonly IContentGetter _contentGetter;

    public CheckDailyBookingJob(
        IMemoryCache cache,
        ILogger<CheckDailyBookingJob> logger,
        IContentGetter contentGetter,
        IDailyRoomChecker roomChecker)
    {
        _cache = cache;
        _logger = logger;
        _contentGetter = contentGetter;
        _roomChecker = roomChecker;
        _now = DateTime.UtcNow.TimeOfDay < new TimeSpan(18, 30,00)
            ? DateTime.UtcNow.Date : DateTime.UtcNow.AddDays(1).Date;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Вход...");
        var response = await _contentGetter.GetContent(new List<DateTime>(){new DateTime(2024, 06, 25)});

        var currentBooking = response.FirstOrDefault();
        if (currentBooking == null) return;
        
        if (_cache.TryGetValue(CacheKey, out HarmonyBookingDto buffer))
        {
            _logger.LogInformation("Booking found in cache.");
            
            if (buffer.GetBookingDate() != _now.Date)
            {
                _logger.LogInformation("Booking is outdated.");
                
                UpdateCache(currentBooking);
                return;
            }
        }
        else
        {
            _logger.LogInformation("Booking not found in cache. Fetching from harmony.cub/krasnodar.");

            UpdateCache(currentBooking);
            return;
        }
        
        _haveChanges = await _roomChecker.CheckDate(currentBooking.Result, buffer.Result);
        
        if (_haveChanges)
        {
            UpdateCache(currentBooking);
            return;
        }
        
        _logger.LogInformation($"Изменений нет.");
    }

    private void UpdateCache(HarmonyBookingDto currentBooking)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(6000))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(360000))
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(1024);
        _cache.Remove(CacheKey);
        currentBooking.SetDate(_now);
        _cache.Set(CacheKey, currentBooking, cacheEntryOptions);
        _logger.LogInformation($"Cache updated");
    }
}