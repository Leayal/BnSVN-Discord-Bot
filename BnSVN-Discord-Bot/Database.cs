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
                        // uint currentItemPrice;
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

        public async Task<bool> IsUserRegistered(SocketGuildUser user)
        {
            using (MySqlCommand commandCheck = this.connection.CreateCommand())
            {
                commandCheck.CommandText = $"SELECT * FROM `users` WHERE `discordid`={user.Id}";
                using (var readerCheck = await commandCheck.ExecuteReaderAsync())
                {
                    if (readerCheck.HasRows)
                        return true;
                    else
                        return false;
                }
            }
        }

        public async Task<RequestRegisterResult> RequestRegisterUri(SocketGuildUser user)
        {
            if (await this.IsUserRegistered(user))
                return new RequestRegisterResult(RequestRegisterResult.Code.AlreadyExist, null);
            else
            {
                string sha1value;
                using (System.Security.Cryptography.SHA512 sha = System.Security.Cryptography.SHA512.Create())
                {
                    byte[] rawdata = Encoding.ASCII.GetBytes(user.Id.ToString() + DateTime.Now.ToBinary().ToString());
                    rawdata = sha.ComputeHash(rawdata);
                    StringBuilder sb = new StringBuilder(rawdata.Length * 2);
                    for (int i = 0; i < rawdata.Length; i++)
                        sb.Append(rawdata[i].ToString("x2"));
                    sha1value = sb.ToString();
                }
                try
                {
                    using (MySqlCommand commandCreate = this.connection.CreateCommand())
                    {
                        commandCreate.CommandText = $"INSERT INTO `registerUri` (`discordid`,`token`) VALUES({user.Id},\"{sha1value}\")";
                        using (var readerCreate = await commandCreate.ExecuteReaderAsync())
                        {
                            if (readerCreate.RecordsAffected != 0)
                                return new RequestRegisterResult(RequestRegisterResult.Code.Success, sha1value);
                            else
                                return new RequestRegisterResult(RequestRegisterResult.Code.UnknownError, null);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062)
                    {
                        using (MySqlCommand commandCreate = this.connection.CreateCommand())
                        {
                            commandCreate.CommandText = $"UPDATE `registerUri` SET `token`=\"{sha1value}\", `created_at`=NOW() WHERE `discordid`={user.Id}";
                            using (var readerCreate = await commandCreate.ExecuteReaderAsync())
                            {
                                if (readerCreate.RecordsAffected != 0)
                                    return new RequestRegisterResult(RequestRegisterResult.Code.Success, sha1value);
                                else
                                    return new RequestRegisterResult(RequestRegisterResult.Code.UnknownError, null);
                            }
                        }
                    }
                    else
                        return new RequestRegisterResult(RequestRegisterResult.Code.UnknownError, null);
                }
            }
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
