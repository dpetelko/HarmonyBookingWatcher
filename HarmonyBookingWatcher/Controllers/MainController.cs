using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TeleSharp.TL;
using TLSharp.Core;

namespace HarmonyBookingWatcher.Controllers;

[ApiController]
[Route("[controller]")]
public class MainController : ControllerBase
{
    public MainController() { }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok("I'm OK");
    }
}