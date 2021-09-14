using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramAlarmer.Infrastructure
{
    public class TelegramRateLimiter
    {
        private BlockingCollection<string> _msgs = new BlockingCollection<string>(1024);
        private readonly string _token;

        public TelegramRateLimiter(string token)
        {
            _token = token;
        }

        public void SendMessage(string msg) => _msgs.Add(msg);

        public async Task RunAsync()
        {
            TelegramHelper th = new TelegramHelper(_token);

            while (true)
            {
                try
                {
                    if (_msgs.TryTake(out var msg))
                        await th.SendMessage(msg);
                }
                catch
                {
                    // We hit the limit anyways. 
                    Thread.Sleep(40000);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
