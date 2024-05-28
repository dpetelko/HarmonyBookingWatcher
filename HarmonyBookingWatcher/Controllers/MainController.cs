using HarmonyBookingWatcher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TeleSharp.TL;
using TLSharp.Core;

namespace HarmonyBookingWatcher.Controllers;

[ApiController]
[Route("[controller]")]
public class MainController : ControllerBase
{
    private readonly IMessenger _messenger;
    
    public MainController(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var now = DateTime.UtcNow.AddHours(3);
        await _messenger.Send($"Приложение активно по состоянию на {now}");
        return Ok($"I'm OK - {now}");
    }
}