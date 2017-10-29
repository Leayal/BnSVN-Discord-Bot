using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace BnSVN_Discord_Bot
{
    class ServerInfo
    {
        private Dictionary<string, SocketRole> cache_servernames;

        public ServerInfo()
        {
            this.cache_servernames = new Dictionary<string, SocketRole>(StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, SocketRole> ServerNames => this.cache_servernames;

        internal void SetManagerRole(SocketRole role)
        {
            this._manager = role;
        }

        private SocketRole _manager;
        public SocketRole Manager => this._manager;
    }
}
