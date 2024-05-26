namespace HarmonyBookingWatcher.Services.Interfaces;

public interface IMessenger
{
    Task Send(string message);
}