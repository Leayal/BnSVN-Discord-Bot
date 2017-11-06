using System;
using System.Collections.Generic;
using System.Text;
using SocketIo;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BnSVN_Discord_Bot
{
    class SocketServer : IDisposable
    {
        private Bot bot;
        private SocketIo.SocketIo socket;
        public SocketServer(Bot botInstance)
        {
            this.bot = botInstance;
            this.socket = Io.Create("127.0.0.1", 1, 2, SocketIo.SocketTypes.SocketHandlerType.Udp);
            this.socket.On("postSellItem", new Action<string>(this.OnPostSellItem));
            this.socket.On("AccountRegistered", new Action<ulong>(this.OnAccountRegistered));

        }

        private void OnAccountRegistered(ulong id)
        {
            Task.Run(async () =>
            {
                SocketUser user = this.bot.client.GetUser(id);
                var task_dmchannel = await user.GetOrCreateDMChannelAsync();
                await task_dmchannel.SendMessageAsync("Tài khoản của bạn đã được tạo thành công vào lúc N");
                await task_dmchannel.CloseAsync();
                task_dmchannel = null;
            });
        }

        private void OnPostSellItem(string msg)
        {

        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            this.socket.Close();
        }
    }
}
