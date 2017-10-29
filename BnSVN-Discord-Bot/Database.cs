using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BnSVN_Discord_Bot
{
    class Database : IDisposable
    {
        public static string GetConnectionString()
            => GetConnectionString(Info.Database.Hostname, Info.Database.Username, Info.Database.Password, Info.Database.DatabaseName);

        public static string GetConnectionString(string server, string username, string password, string databaseName)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.UseCompression = false;
            builder.SslMode = MySqlSslMode.None;
            builder.ForceSynchronous = false;
            builder.ConvertZeroDateTime = true;

            builder.Server = server;
            builder.CharacterSet = "utf8";
            builder.UserID = username;
            builder.Password = password;
            builder.Database = databaseName;

            return builder.ToString();
        }

        public static string GetConnectionString(string server, uint port, string username, string password, string databaseName)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.UseCompression = false;
            builder.SslMode = MySqlSslMode.None;
            builder.ForceSynchronous = false;
            builder.ConvertZeroDateTime = true;

            builder.Server = server;
            builder.Port = port;
            builder.CharacterSet = "utf8";
            builder.UserID = username;
            builder.Password = password;
            builder.Database = databaseName;

            return builder.ToString();
        }

        private MySqlConnection connection;

        public Database()
        {
            this.connection = new MySqlConnection(GetConnectionString());
        }

        public async Task Connect()
        {
            await this.connection.OpenAsync();
        }

        public async Task<IList<TradeItemInfo>> GetTradeItems(SocketGuildUser user)
        {
            List<TradeItemInfo> result = null;
            using (MySqlCommand command = this.connection.CreateCommand())
            {
                command.CommandText = "";
                using (var reader = await command.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        string currentline,
                            currentItemName;
                        uint currentItemPrice;
                        string[] currentlineSplitted;

                        result = new List<TradeItemInfo>();
                        using (var textReader = reader.GetTextReader(reader.GetOrdinal("items")))
                            while (textReader.Peek() != -1)
                            {
                                currentline = await textReader.ReadLineAsync();
                                currentlineSplitted = currentline.Split('=', 2, StringSplitOptions.None);
                                currentItemName = await this.GetItemName(new Guid(currentlineSplitted[0]));
                                if (!string.IsNullOrEmpty(currentItemName))
                                    result.Add(new TradeItemInfo(currentItemName) { Price = 0 });
                            }
                    }
            }
            return result;
        }

        public async Task<string> GetItemName(Guid id)
        {
            using (MySqlCommand command = this.connection.CreateCommand())
            {
                command.CommandText = $"SELECT `name` FROM `tb` WHERE `id`=`{id.ToString()}`";
                using (var reader = await command.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                        return reader.GetString(reader.GetOrdinal("name"));
            }
            return string.Empty;
        }



        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            this.connection.Dispose();
        }
    }
}
