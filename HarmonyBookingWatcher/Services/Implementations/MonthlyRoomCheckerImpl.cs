using HarmonyBookingWatcher.Dto;
using HarmonyBookingWatcher.Services.Interfaces;

namespace HarmonyBookingWatcher.Services.Implementations;

public class MonthlyRoomCheckerImpl : IMonthlyRoomChecker
{
    private bool _haveChanges;
    private DateTime? _date;
    private readonly IMessenger _messenger;
    private readonly ILogger<DailyRoomCheckerImpl> _logger;
    private Dictionary<string, int> _dailyReport = new(); 

    public MonthlyRoomCheckerImpl(
        IMessenger messenger,
        ILogger<DailyRoomCheckerImpl> logger)
    {
        _messenger = messenger;
        _logger = logger;
    }

    public async Task<bool> CheckDate(Result? currentBooking, Result? buffer)
    {
        _date = currentBooking?.BookingDate;
        _haveChanges = await CheckDay(currentBooking, buffer);
        if (_haveChanges) return _haveChanges;
        
        var currentOffice = currentBooking?.BookingsData?.Office;
        var bufferOffice = buffer?.BookingsData?.Office;
        await CheckRoom(currentOffice?.BookingData171, bufferOffice?.BookingData171);
        await CheckRoom(currentOffice?.BookingData172, bufferOffice?.BookingData172);
        await CheckRoom(currentOffice?.BookingData173, bufferOffice?.BookingData173);
        await CheckRoom(currentOffice?.BookingData189, bufferOffice?.BookingData189);
        await CheckRoom(currentOffice?.BookingData190, bufferOffice?.BookingData190);
        await CheckRoom(currentOffice?.BookingData191, bufferOffice?.BookingData191);
        await CheckRoom(currentOffice?.BookingData205, bufferOffice?.BookingData205);
        
        return _haveChanges;
    }

    private async Task<bool> CheckDay(Result? currentBooking, Result? buffer)
    {
        if (currentBooking?.BookingsData != null && buffer?.BookingsData == null)
        {
            await _messenger.Send($"\U00002705 {ToDate(_date.ToString())}");
            return true;
        }
        
        if (currentBooking?.BookingsData == null && buffer?.BookingsData != null)
        {
            await _messenger.Send($"\U0000274C {ToDate(_date.ToString())}");
            return true;
        }

        return _haveChanges;
    }

    public async Task CheckRoom(BookingData? currentBookingData, BookingData? bufferBookingData)
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
            UpdateReport(currentHalfTime.Cabinet?.Name, 1);
            //await _messenger.Send($"\U00002705 {currentHalfTime.Cabinet?.Name} {ToDate(currentHalfTime.BeginAt)}");
            _haveChanges = true;
        }

        if (currentHalfTime == null && bufferHalfTime != null)
        {
            UpdateReport(bufferHalfTime.Cabinet?.Name, -1);
            //await _messenger.Send($"\U0000274C {bufferHalfTime.Cabinet?.Name} {ToDate(bufferHalfTime.BeginAt)}");
            _haveChanges = true;
        }

        _logger.LogInformation("Найдены изменения");
    }

    private void UpdateReport(string cabinetName, int value)
    {
        if (_dailyReport.ContainsKey(cabinetName))
        {
            _dailyReport[cabinetName] += value;
            return;
        }
        
        _dailyReport.Add(cabinetName, value);
    }

    private static string ToDate(string? dateStr)
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
}