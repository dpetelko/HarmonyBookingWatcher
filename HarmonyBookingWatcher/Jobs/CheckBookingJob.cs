using HarmonyBookingWatcher.Dto;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Quartz;
using TeleSharp.TL;
using TLSharp.Core;

namespace HarmonyBookingWatcher.Jobs;

public class CheckBookingJob :IJob
{
    private static readonly HttpClient Client = new HttpClient();
    private const string CacheKey = "harmonyBooking";
    private readonly IMemoryCache _cache;
    private bool _haveChanges;
    private readonly DateTime _now;
    private readonly ILogger<CheckBookingJob> _logger;


    public CheckBookingJob(
        IMemoryCache cache,
        ILogger<CheckBookingJob> logger)
    {
        _cache = cache;
        _logger = logger;
        _now = DateTime.Now.TimeOfDay < new TimeSpan(21, 30,00)
            ? DateTime.Now : DateTime.Now.AddDays(1);
    }

    public async Task Execute(IJobExecutionContext context)
    {
        HttpContent content = GetContent();
        var response = await Client.PostAsync("https://harmony.cab/v1/api/get", content);

        var responseString = await response.Content.ReadAsStringAsync();

        var currentBooking = JsonConvert.DeserializeObject<HarmonyBookingDto>(responseString);

        if (currentBooking == null)
        {
            await SendMessage("Нет ответа от сервера");
            return;
        }

        if (_cache.TryGetValue(CacheKey, out HarmonyBookingDto buffer))
        {
            Console.WriteLine("Booking found in cache.");
            
            if (buffer.GetBookingDate() != _now.Date)
            {
                Console.WriteLine("Booking is outdated.");
                
                UpdateCache(currentBooking);
                return;
            }
        }
        else
        {
            Console.WriteLine("Booking not found in cache. Fetching from harmony.cub/krasnodar.");

            UpdateCache(currentBooking);
            return;
        }
        
        var currentOffice = currentBooking.Result.BookingsData.Office;
        var bufferOffice = buffer.Result.BookingsData.Office;
        await CheckRoom(currentOffice.BookingData171, bufferOffice.BookingData171);
        await CheckRoom(currentOffice.BookingData172, bufferOffice.BookingData172);
        await CheckRoom(currentOffice.BookingData173, bufferOffice.BookingData173);
        await CheckRoom(currentOffice.BookingData189, bufferOffice.BookingData189);
        await CheckRoom(currentOffice.BookingData190, bufferOffice.BookingData190);
        await CheckRoom(currentOffice.BookingData191, bufferOffice.BookingData191);
        await CheckRoom(currentOffice.BookingData205, bufferOffice.BookingData205);


        if (_haveChanges)
        {
            UpdateCache(currentBooking);
            return;
        }
        
        _logger.LogWarning($"Изменений нет на {_now}");
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
        Console.WriteLine($"Cache updated");
    }

    private async Task CheckRoom(BookingData currentBookingData, BookingData bufferBookingData)
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
            await SendMessage($"Добавилась запись кабинет {currentHalfTime.Cabinet.Name} на время {ToDate(currentHalfTime.BeginAt)}");
            _haveChanges = true;
        }
        
        if (currentHalfTime == null && bufferHalfTime != null)
        {
            await SendMessage($"Отменена запись кабинет {bufferHalfTime.Cabinet.Name} на время {ToDate(bufferHalfTime.BeginAt)}");
            _haveChanges = true;
        }

        _logger.LogWarning("Найдены изменения");
    }

    private string ToDate(string dateStr)
    {
        var date = Convert.ToDateTime(dateStr);
        string month = GetMonthName(date.Month);
        return $"{date.TimeOfDay.ToString(@"hh\:mm")} {date.Day} {month}";
    }

    private string GetMonthName(int month)
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

    private async Task SendMessage(string text)
    {
        var client = new TelegramClient(21045577, "ffd2152e5271c5910495eeb813c8b1a5");
        await client.ConnectAsync();
        
        // var hash = await client.SendCodeRequestAsync("79622765272");
        // var code = "26515"; // you can change code in debugger
        //
        // var user = await client.MakeAuthAsync("79622765272", hash, code);

        // //get available contacts
        // var result = await client.GetContactsAsync();
        //
        // //find recipient in contacts
        // var user12 = result.Users
        //     .Where(x => x.GetType() == typeof (TLUser))
        //     .Cast<TLUser>()
        //     .Select(x => x.Phone)
        //     .ToList();
        //
        // Console.WriteLine(JsonConvert.SerializeObject(user12, Formatting.Indented));
        //
        // //find recipient in contacts
        // var user1 = result.Users
        //     .Where(x => x.GetType() == typeof (TLUser))
        //     .Cast<TLUser>()
        //     .FirstOrDefault(x => x.Phone == "79182182426");
        //
        // //send message
        // await client.SendMessageAsync(new TLInputPeerUser() {UserId = user1.Id}, "Дима Петелько охуенный программист!!!");

//Get dialogs
        var dialogs = await client.GetUserDialogsAsync();

//get user chats 
        var chats = ((TeleSharp.TL.Messages.TLDialogsSlice)dialogs).Chats;

//find channel by title
        var tlChannel = chats
            .Where(_=>_.GetType() == typeof(TLChannel))
            .Select(_=>(TLChannel)_)
            .FirstOrDefault(_ => _.Title.Contains("HarmonyKrasnodar"));
        
//send message
        await client.SendMessageAsync(
            new TLInputPeerChannel
                { ChannelId = tlChannel.Id, AccessHash =(long)tlChannel.AccessHash },
            text);
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