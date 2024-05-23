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
    public async Task Get()
    {
        var client = new TelegramClient(21045577, "ffd2152e5271c5910495eeb813c8b1a5");
        await client.ConnectAsync();
        
        // var hash = await client.SendCodeRequestAsync("79622765272");
        // var code = "26515"; // you can change code in debugger
        //
        // var user = await client.MakeAuthAsync("79622765272", hash, code);

        // //get available contacts
        // var result = await client.GetContactsAsync();
        //
        // //find recipient in contacts
        // var user12 = result.Users
        //     .Where(x => x.GetType() == typeof (TLUser))
        //     .Cast<TLUser>()
        //     .Select(x => x.Phone)
        //     .ToList();
        //
        // Console.WriteLine(JsonConvert.SerializeObject(user12, Formatting.Indented));
        //
        // //find recipient in contacts
        // var user1 = result.Users
        //     .Where(x => x.GetType() == typeof (TLUser))
        //     .Cast<TLUser>()
        //     .FirstOrDefault(x => x.Phone == "79182182426");
	       //
        // //send message
        // await client.SendMessageAsync(new TLInputPeerUser() {UserId = user1.Id}, "Дима Петелько охуенный программист!!!");

//Get dialogs
        var dialogs = await client.GetUserDialogsAsync();

//get user chats 
        var chats = ((TeleSharp.TL.Messages.TLDialogsSlice)dialogs).Chats;

//find channel by title
        var tlChannel = chats.Where(_ => _.GetType() == typeof(TLChannel))
            .Select(_=>(TLChannel)_)
            .Where(_=>_.Title.Contains("HarmonyKrasnodar"))
            .FirstOrDefault();
//send message
        await client.SendMessageAsync(new TLInputPeerChannel()
                { ChannelId = tlChannel.Id, AccessHash =(long)tlChannel.AccessHash },
            "OUR_MESSAGE");
        
        
    }
}