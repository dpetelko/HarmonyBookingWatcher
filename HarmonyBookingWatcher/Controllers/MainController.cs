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
        var msg = $"Приложение *активно* по состоянию на {now:F}".Replace("г.", "года");
        Console.WriteLine(msg);
        await _messenger.Send(msg);
        return Ok($"I'm OK - {now}");
    }
}