using HarmonyBookingWatcher.Dto;
using HarmonyBookingWatcher.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace HarmonyBookingWatcher.Jobs;

public class CheckMonthlyBookingJob : IJob
{
    private const string CacheKey = "harmonyMonthlyBooking";
    private readonly IMemoryCache _cache;
    private bool _haveChanges;
    private readonly ILogger<CheckMonthlyBookingJob> _logger;
    private readonly IMonthlyRoomChecker _roomChecker;
    private readonly IContentGetter _contentGetter;


    public CheckMonthlyBookingJob(
        IMemoryCache cache,
        ILogger<CheckMonthlyBookingJob> logger,
        IMonthlyRoomChecker roomChecker,
        IContentGetter contentGetter)
    {
        _cache = cache;
        _logger = logger;
        _roomChecker = roomChecker;
        _contentGetter = contentGetter;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        return;
        _logger.LogInformation("Вход...");
        List<DateTime> dates = GetDates();
        var currentMonthlyBooking = await _contentGetter.GetContent(dates);

        if (_cache.TryGetValue(CacheKey, out List<HarmonyBookingDto> buffer))
        {
            buffer = buffer.OrderBy(x => x.GetBookingDate()).ToList();
            _logger.LogInformation("Monthly Booking found in cache.");

            if (buffer.FirstOrDefault()?.GetBookingDate() != currentMonthlyBooking.FirstOrDefault()?.GetBookingDate())
            {
                _logger.LogInformation("Monthly Booking is outdated.");

                UpdateCache(currentMonthlyBooking);
                return;
            }
        }
        else
        {
            _logger.LogInformation("Monthly Booking not found in cache. Fetching from harmony.cub/krasnodar.");

            UpdateCache(currentMonthlyBooking);
            return;
        }

        foreach (var currentBooking in currentMonthlyBooking)
        {
            var bufferItem = buffer
                .SingleOrDefault(x =>
                    x.GetBookingDate() == currentBooking.GetBookingDate());

            var currentOffice = currentBooking.Result?.BookingsData?.Office;
            var bufferOffice = bufferItem?.Result?.BookingsData?.Office;
            await CheckRoom(currentOffice?.BookingData171, bufferOffice?.BookingData171);
            await CheckRoom(currentOffice?.BookingData172, bufferOffice?.BookingData172);
            await CheckRoom(currentOffice?.BookingData173, bufferOffice?.BookingData173);
            await CheckRoom(currentOffice?.BookingData189, bufferOffice?.BookingData189);
            await CheckRoom(currentOffice?.BookingData190, bufferOffice?.BookingData190);
            await CheckRoom(currentOffice?.BookingData191, bufferOffice?.BookingData191);
            await CheckRoom(currentOffice?.BookingData205, bufferOffice?.BookingData205);
        }

        if (!_haveChanges)
        {
            _logger.LogInformation($"Изменений нет.");
            return;
        }
        
        UpdateCache(currentMonthlyBooking);
    }

    private async Task CheckRoom(BookingData? currentOffice, BookingData? bufferOffice)
    {
        var result = await _roomChecker.CheckRoom(currentOffice, bufferOffice);
        if (result) _haveChanges = true;
    }

    private void UpdateCache(List<HarmonyBookingDto> currentBooking)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(600000))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(36000000))
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(1024000);
        _cache.Remove(CacheKey);
        _cache.Set(CacheKey, currentBooking, cacheEntryOptions);
        _logger.LogInformation($"Cache updated");
    }

    private static List<DateTime> GetDates()
    {
        var now = DateTime.UtcNow;
        var start = now.TimeOfDay < new TimeSpan(18, 30, 00)
            ? now.AddDays(1).Date
            : now.AddDays(2).Date;

        var startAddTwoMonths = start.AddMonths(2);
        var end = new DateTime(
                startAddTwoMonths.Year,
                startAddTwoMonths.Month,
                01)
            .AddDays(-1);

        return Enumerable.Range(0, 1 + end.Subtract(start).Days)
            .Select(offset => start.AddDays(offset))
            .ToList();
    }
}