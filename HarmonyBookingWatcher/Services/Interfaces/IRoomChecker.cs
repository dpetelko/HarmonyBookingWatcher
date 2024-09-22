using HarmonyBookingWatcher.Dto;

namespace HarmonyBookingWatcher.Services.Interfaces;

public interface IRoomChecker
{
    Task<bool> CheckDate(Result? currentBookingData, Result? bufferBookingData);
}