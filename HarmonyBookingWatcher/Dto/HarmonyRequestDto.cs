namespace HarmonyBookingWatcher.Dto;

public class HarmonyRequestDto
{
    public HarmonyRequestDto(long officeId, string date)
    {
        OfficeId = officeId;
        Date = date;
    }

    public long OfficeId { get; private set; }
    public string Date { get; private set; }
}