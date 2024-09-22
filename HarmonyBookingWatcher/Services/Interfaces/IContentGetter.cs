using HarmonyBookingWatcher.Dto;

namespace HarmonyBookingWatcher.Services.Interfaces;

public interface IContentGetter
{
    Task<List<HarmonyBookingDto>> GetContent(List<DateTime> dates);
}