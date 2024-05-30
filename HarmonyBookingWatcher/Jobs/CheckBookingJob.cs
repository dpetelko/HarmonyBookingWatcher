using HarmonyBookingWatcher.Dto;
using HarmonyBookingWatcher.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Quartz;

namespace HarmonyBookingWatcher.Jobs;

public class CheckBookingJob : IJob
{
    private static readonly HttpClient Client = new ();
    private const string CacheKey = "harmonyBooking";
    private readonly IMemoryCache _cache;
    private bool _haveChanges;
    private readonly DateTime _now;
    private readonly ILogger<CheckBookingJob> _logger;
    private readonly IMessenger _messenger;

    public CheckBookingJob(
        IMemoryCache cache,
        ILogger<CheckBookingJob> logger,
        IMessenger messenger)
    {
        _cache = cache;
        _logger = logger;
        _messenger = messenger;
        _now = DateTime.UtcNow.TimeOfDay < new TimeSpan(18, 30,00)
            ? DateTime.UtcNow.Date : DateTime.UtcNow.AddDays(1).Date;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Вход...");
        HttpContent content = GetContent();
        HttpResponseMessage response;
        try
        {
            _logger.LogInformation("Пробуем получить данные");
            response = await Client.PostAsync("https://harmony.cab/v1/api/get", content);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return;
        }
        var responseString = await response.Content.ReadAsStringAsync();

        var currentBooking = JsonConvert.DeserializeObject<HarmonyBookingDto>(responseString);

        if (currentBooking?.Result?.BookingsData?.Office == null)
        {
            _logger.LogError("Нет ответа от сервера");
            await _messenger.Send("Нет ответа от сервера");
            return;
        }
        
        _logger.LogInformation("Данные успешно получены");

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
        
        var currentOffice = currentBooking.Result.BookingsData.Office;
        var bufferOffice = buffer.Result?.BookingsData?.Office;
        await CheckRoom(currentOffice?.BookingData171, bufferOffice?.BookingData171);
        await CheckRoom(currentOffice?.BookingData172, bufferOffice?.BookingData172);
        await CheckRoom(currentOffice?.BookingData173, bufferOffice?.BookingData173);
        await CheckRoom(currentOffice?.BookingData189, bufferOffice?.BookingData189);
        await CheckRoom(currentOffice?.BookingData190, bufferOffice?.BookingData190);
        await CheckRoom(currentOffice?.BookingData191, bufferOffice?.BookingData191);
        await CheckRoom(currentOffice?.BookingData205, bufferOffice?.BookingData205);


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

    private async Task CheckRoom(BookingData? currentBookingData, BookingData? bufferBookingData)
    {
        await CheckHour(currentBookingData?.Hour8, bufferBookingData?.Hour8);
        await CheckHour(currentBookingData?.Hour9, bufferBookingData?.Hour9);
        await CheckHour(currentBookingData?.Hour10, bufferBookingData?.Hour10);
        await CheckHour(currentBookingData?.Hour11, bufferBookingData?.Hour11);
        await CheckHour(currentBookingData?.Hour12, bufferBookingData?.Hour12);
        await CheckHour(currentBookingData?.Hour13, bufferBookingData?.Hour13);
        await CheckHour(currentBookingData?.Hour14, bufferBookingData?.Hour14);
        await CheckHour(currentBookingData?.Hour15, bufferBookingData?.Hour15);
        await CheckHour(currentBookingData?.Hour16, bufferBookingData?.Hour16);
        await CheckHour(currentBookingData?.Hour17, bufferBookingData?.Hour17);
        await CheckHour(currentBookingData?.Hour18, bufferBookingData?.Hour18);
        await CheckHour(currentBookingData?.Hour19, bufferBookingData?.Hour19);
        await CheckHour(currentBookingData?.Hour20, bufferBookingData?.Hour20);
        await CheckHour(currentBookingData?.Hour21, bufferBookingData?.Hour21);
    }

    private async Task CheckHour(Hour? current, Hour? previous)
    {
        await CheckHalfTime(current?.FirstHalfTime, previous?.FirstHalfTime);
        await CheckHalfTime(current?.SecondHalfTime, previous?.SecondHalfTime);
    }

    private async Task CheckHalfTime(HalfTime? currentHalfTime, HalfTime? bufferHalfTime)
    {
        if ((currentHalfTime != null && bufferHalfTime != null) ||
            (currentHalfTime == null && bufferHalfTime == null))
        {
            return;
        }
        if (currentHalfTime != null && bufferHalfTime == null)
        {
            await _messenger.Send($"Добавилась запись кабинет *{currentHalfTime.Cabinet?.Name}* на время {ToDate(currentHalfTime.BeginAt)}");
            _haveChanges = true;
        }
        
        if (currentHalfTime == null && bufferHalfTime != null)
        {
            await _messenger.Send($"Отменена запись кабинет *{bufferHalfTime.Cabinet?.Name}* на время {ToDate(bufferHalfTime.BeginAt)}");
            _haveChanges = true;
        }

        _logger.LogInformation("Найдены изменения");
    }

    private string ToDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return "Дата не задана";
        var date = Convert.ToDateTime(dateStr);
        var month = GetMonthName(date.Month);
        return $"*{date.TimeOfDay.ToString(@"hh\:mm")}* {date.Day} {month}";
    }

    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "января",
            2 => "февраля",
            3 => "марта",
            4 => "апреля",
            5 => "мая",
            6 => "июня",
            7 => "июля",
            8 => "августа",
            9 => "сентября",
            10 => "октября",
            11 => "ноября",
            12 => "декабря",
            _ => month.ToString()
        };
    }

    private HttpContent GetContent()
    {
        HarmonyRequestDto values = new(40, _now.Date.ToString("yyyy-MM-dd"));
        HttpContent content = JsonContent.Create(values);
        content.Headers.Add("Cookie",
            "current_city=226a37df7de576bca6a52dae7a442ad91dc87b20d43a95878c175a5fa19eeeb2a%3A2%3A%7Bi%3A0%3Bs%3A12%3A%22current_city%22%3Bi%3A1%3Bs%3A9%3A%22krasnodar%22%3B%7D; ");
        return content;
    }
}