using Newtonsoft.Json;

namespace HarmonyBookingWatcher.Dto;

public class HalfTime
{
    [JsonProperty("beginAt")] public string? BeginAt;

    [JsonProperty("cabinet")] public Cabinet? Cabinet;
}

public class Office
{
    [JsonProperty("171")] public BookingData? BookingData171;

    [JsonProperty("172")] public BookingData? BookingData172;

    [JsonProperty("173")] public BookingData? BookingData173;

    [JsonProperty("205")] public BookingData? BookingData205;

    [JsonProperty("189")] public BookingData? BookingData189;

    [JsonProperty("190")] public BookingData? BookingData190;

    [JsonProperty("191")] public BookingData? BookingData191;
}

public class Hour
{
    [JsonProperty("1/2")] public HalfTime? FirstHalfTime;

    [JsonProperty("2/2")] public HalfTime? SecondHalfTime;
}

public class BookingsData
{
    [JsonProperty("40")] public Office? Office;
}

public class BookingData
{
    [JsonProperty("8")] public Hour? Hour8;

    [JsonProperty("9")] public Hour? Hour9;

    [JsonProperty("10")] public Hour? Hour10;

    [JsonProperty("11")] public Hour? Hour11;

    [JsonProperty("12")] public Hour? Hour12;

    [JsonProperty("13")] public Hour? Hour13;

    [JsonProperty("14")] public Hour? Hour14;

    [JsonProperty("15")] public Hour? Hour15;

    [JsonProperty("16")] public Hour? Hour16;

    [JsonProperty("17")] public Hour? Hour17;

    [JsonProperty("18")] public Hour? Hour18;

    [JsonProperty("19")] public Hour? Hour19;

    [JsonProperty("20")] public Hour? Hour20;

    [JsonProperty("21")] public Hour? Hour21;
}

public class Cabinet
{
    [JsonProperty("name")] public string? Name;
}

public class Result
{
    [JsonConstructor]
    public Result(BookingsData? bookingsData, DateTime? bookingDate)
    {
        BookingsData = bookingsData;
        BookingDate = bookingDate;
    }

    public Result(DateTime? bookingDate)
    {
        BookingDate = bookingDate;
    }

    [JsonProperty("bookingsData")] public BookingsData? BookingsData;
    public DateTime? BookingDate;
}

public class HarmonyBookingDto
{
    [JsonConstructor]
    public HarmonyBookingDto(
        bool? isOk,
        Result? result)
    {
        IsOK = isOk;
        Result = result;
    }

    [JsonProperty("isOK")] public bool? IsOK;

    [JsonProperty("result")] public Result? Result;

    public void SetDate(DateTime date)
    {
        if (Result == null) return;
        Result.BookingDate = date;
    }
    
    public DateTime GetBookingDate()
    {
        return Result == null 
            ? default 
            : Result.BookingDate.GetValueOrDefault().Date;
    }
}