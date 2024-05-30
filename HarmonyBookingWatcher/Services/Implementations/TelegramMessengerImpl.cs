using System.Net;
using System.Text;
using HarmonyBookingWatcher.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
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
        
        
        var bot = new Telegram.Bot.TelegramBotClient("7413747352:AAFi-qdowWJ76QSrIzXI9D4d7AVU--jwIFU"); 
        await bot.SendTextMessageAsync(
            -1002005329889,
            message,
            default,
            ParseMode.MarkdownV2);
        return;
        
        string retval = string.Empty;
        string token = "7413747352:AAFi-qdowWJ76QSrIzXI9D4d7AVU--jwIFU";
        string chatId = "2005329889";
        string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={message}";

        using(var webClient = new WebClient())
        {
            retval = webClient.DownloadString(url);
        }
        Console.WriteLine(retval);
        return;
        
        
        string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
        string apiToken = "7413747352:AAFi-qdowWJ76QSrIzXI9D4d7AVU--jwIFU";
        //string chatId = "2005329889";
        urlString = String.Format(urlString, apiToken, chatId, message);
        WebRequest request = WebRequest.Create(urlString);
        Stream rs = request.GetResponse().GetResponseStream();
        StreamReader reader = new StreamReader(rs);
        string line = "";
        StringBuilder sb = new StringBuilder();
        while (line != null)
        {
            line = reader.ReadLine();
            if (line != null)
                sb.Append(line);
        }
        string response = sb.ToString();
        
        Console.WriteLine(response);

        return;
        
        
        
        
        
        
        
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

        if (tlChannel == null)
        {
            _logger.LogError("Канал HarmonyKrasnodar не найден");
            return;
        }
        
        if (tlChannel.AccessHash == null)
        {
            _logger.LogError("AccessHash для канала HarmonyKrasnodar не найден");
            return;
        }
        
//send message
        await client.SendMessageAsync(
            new TLInputPeerChannel
                { ChannelId = tlChannel.Id, AccessHash =(long)tlChannel.AccessHash },
            message);
        _logger.LogWarning($"Сообщение отправлено");
    }
}