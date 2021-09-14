using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EthScanner.Infrastructure
{
    public class TelegramHelper
    {
        private readonly TelegramBotClient _botClient;

        public TelegramHelper(string token)
        {
            _botClient = new TelegramBotClient(token);
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
