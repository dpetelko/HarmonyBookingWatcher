using HarmonyBookingWatcher.Services.Interfaces;
using TeleSharp.TL;
using TLSharp.Core;

namespace HarmonyBookingWatcher.Services.Implementations;

public class TelegramMessengerImpl : IMessenger
{
    private readonly ILogger<TelegramMessengerImpl> _logger;

    public TelegramMessengerImpl(ILogger<TelegramMessengerImpl> logger)
    {
        _logger = logger;
    }

    public async Task Send(string message)
    {
        _logger.LogWarning($"Начало отправки сообщения");
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
        var tlChannel = chats
            .Where(_=>_.GetType() == typeof(TLChannel))
            .Select(_=>(TLChannel)_)
            .FirstOrDefault(_ => _.Title.Contains("HarmonyKrasnodar"));
        
//send message
        await client.SendMessageAsync(
            new TLInputPeerChannel
                { ChannelId = tlChannel.Id, AccessHash =(long)tlChannel.AccessHash },
            message);
        _logger.LogWarning($"Сообщение отправлено");
    }
}