using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace EthScanner.Features
{
    public class SendMessageToTelegramBot
    {

        public class Command : IRequest
        {
            public string Message { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command>
        {
            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var botClient = new TelegramBotClient("Your:Token");

                await botClient.SendTextMessageAsync(
                    parseMode: ParseMode.Default,
                    chatId: "425114131",
                    text: command.Message,
                    cancellationToken: cancellationToken);

                return Unit.Value;
            }
        }
    }
}
