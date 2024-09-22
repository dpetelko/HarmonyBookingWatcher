using HarmonyBookingWatcher.Dto;
using HarmonyBookingWatcher.Services.Interfaces;
using Newtonsoft.Json;

namespace HarmonyBookingWatcher.Services.Implementations;

public class ContentGetter : IContentGetter
{
    private static readonly HttpClient Client = new ();
    private readonly ILogger<ContentGetter> _logger;
    private readonly IMessenger _messenger;

    public ContentGetter(
        ILogger<ContentGetter> logger,
        IMessenger messenger)
    {
        _logger = logger;
        _messenger = messenger;
    }

    public async Task<List<HarmonyBookingDto>> GetContent(List<DateTime> dates)
    {
        List<HarmonyBookingDto> result = new();
        foreach (var date in dates)
        {
            if (dates.Count > 1)
            {
                _logger.LogInformation(date.ToString());
            }
            
            HarmonyRequestDto request = new(40, date.Date.ToString("yyyy-MM-dd"));
            HttpContent content = JsonContent.Create(request);
            content.Headers.Add("Cookie",
                "current_city=226a37df7de576bca6a52dae7a442ad91dc87b20d43a95878c175a5fa19eeeb2a%3A2%3A%7Bi%3A0%3Bs%3A12%3A%22current_city%22%3Bi%3A1%3Bs%3A9%3A%22krasnodar%22%3B%7D; ");

            HttpResponseMessage response = await Client.PostAsync("https://harmony.cab/v1/api/get", content);
        
            var responseString = await response.Content.ReadAsStringAsync();

            if (dates.Count >= 1)
            {
                _logger.LogInformation(responseString);
            }

            HarmonyBookingDto? item = null;
            try
            {
                 item = JsonConvert.DeserializeObject<HarmonyBookingDto>(responseString);
            }
            catch (Exception e)
            {
                item = new HarmonyBookingDto(false, new Result(date));
            }

            if (item?.Result?.BookingsData?.Office == null) continue;

            _logger.LogInformation("Данные успешно получены");
            result.Add(item);
        }

        if (result.Any()) return result.OrderBy(x => x.GetBookingDate()).ToList();
        
        await _messenger.Send("Нет ответа от сервера");
        throw new Exception("Нет ответа от сервера");
    }
}