using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramAlarmer.Infrastructure
{
    public class TelegramHelper
    {
        private readonly TelegramBotClient _botClient;

        public TelegramHelper()
        {
            _botClient = new TelegramBotClient("Your:Token");
        }

        public async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(
                parseMode: ParseMode.Default,
                chatId: "-1001546744389",
                text: message,
                cancellationToken: new CancellationToken());
        }
    }
}
