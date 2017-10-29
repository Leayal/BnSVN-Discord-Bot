using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;
using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BnSVN_Discord_Bot
{
    public class Bot
    {
        private string[] args;
        private DiscordSocketClient client;
        private static readonly char[] haicham = { ':' };
        public Bot()
        {
            this.client = new DiscordSocketClient(new DiscordSocketConfig() { MessageCacheSize = 0, ConnectionTimeout = 10000 });
            this.client.LoggedIn += this.Client_LoggedIn;
            this.client.Ready += this.Client_Ready;
            this.client.MessageReceived += this.Client_MessageReceived;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (string.IsNullOrWhiteSpace(arg.Content) || arg.Author.IsBot) return;
            string lowerChannelName = arg.Channel.Name.ToLower();
            if (lowerChannelName == "bao_danh" || lowerChannelName == "bot_test")
                using (IDisposable typing = arg.Channel.EnterTypingState())
                {
                    SocketGuildUser guilduser = arg.Author as SocketGuildUser;
                    if (guilduser != null)
                    {
                        try
                        {
                            string currentline, currentlower; string[] currentlineSplitted;
                            string[] serverstring = null;

                            using (StringReader sr = new StringReader(arg.Content))
                            {
                                while (sr.Peek() > 0)
                                {
                                    currentline = sr.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(currentline))
                                    {
                                        if (currentline.IndexOf(':') > -1)
                                        {
                                            currentlineSplitted = currentline.Split(haicham, 2, StringSplitOptions.RemoveEmptyEntries);
                                            currentlower = currentlineSplitted[0].ToLower();
                                            if (currentlower == "server")
                                            {
                                                if (!string.IsNullOrWhiteSpace(currentlineSplitted[1]))
                                                    serverstring = currentlineSplitted[1].ToLower().Split(',', ';', '.');
                                                break;
                                            }
                                            else if (currentlower == "sever")
                                            {
                                                if (!string.IsNullOrWhiteSpace(currentlineSplitted[1]))
                                                    serverstring = currentlineSplitted[1].ToLower().Split(',', ';', '.');
                                                await arg.Channel.SendMessageAsync("Bạn đã viết sai từ `server` thành `sever`. Xin hãy cẩn thận hơn vào lần sau.");
                                                break;
                                            }
                                            else if (currentlower == "sv")
                                            {
                                                if (!string.IsNullOrWhiteSpace(currentlineSplitted[1]))
                                                    serverstring = currentlineSplitted[1].ToLower().Split(',', ';', '.');
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (serverstring == null || serverstring.Length == 0)
                                    await arg.Channel.SendMessageAsync("Xin vui lòng khai báo rằng nhân vật của bạn sẽ hoặc đang chơi ở máy chủ nào.");
                                else
                                {
                                    int i;
                                    string trimmed;
                                    List<string> notfoundNames = new List<string>(serverstring.Length),
                                        addedNames = new List<string>(serverstring.Length);
                                    for (i = 0; i < serverstring.Length; i++)
                                        if (!string.IsNullOrWhiteSpace(serverstring[i]))
                                        {
                                            trimmed = serverstring[i].Trim();
                                            if (!cache_roles.ContainsKey(guilduser.Guild.Id))
                                                notfoundNames.Add(trimmed);
                                            else if (!cache_roles[guilduser.Guild.Id].ServerNames.ContainsKey(trimmed))
                                                notfoundNames.Add(trimmed);
                                            else
                                            {
                                                SocketRole role = cache_roles[guilduser.Guild.Id].ServerNames[trimmed];
                                                role.Members.Contains(arg.Author);
                                                if (!IsAlreadyHasRole(guilduser, role))
                                                {
                                                    await guilduser.AddRoleAsync(role, new RequestOptions() { AuditLogReason = "Tự động đặt role theo báo danh." });
                                                    addedNames.Add(role.Name);
                                                }
                                            }
                                        }
                                    int total;
                                    StringBuilder sb = null;
                                    if (addedNames.Count> 0)
                                    {
                                        total = 0;
                                        for (i = 0; i < addedNames.Count; i++)
                                            total += (addedNames[i].Length + 8);
                                        sb = new StringBuilder(total);
                                        for (i = 0; i < addedNames.Count; i++)
                                        {
                                            if (i == 0)
                                                sb.AppendFormat("***{0}***", addedNames[i]);
                                            else
                                                sb.AppendFormat(", ***{0}***", addedNames[i]);
                                        }

                                        await arg.Channel.SendMessageAsync($"<@{arg.Author.Id}> đã được thêm vào danh sách thành viên {sb.ToString()}");
                                    }
                                    if (notfoundNames.Count > 0)
                                    {
                                        total = 0;
                                        for (i = 0; i < notfoundNames.Count; i++)
                                            total += (notfoundNames[i].Length + 4);

                                        if (sb != null && sb.Capacity > total)
                                            sb.Clear();
                                        else
                                            sb = new StringBuilder(total);

                                        for (i = 0; i < notfoundNames.Count; i++)
                                        {
                                            if (i == 0)
                                                sb.AppendFormat("`{0}`", notfoundNames[i]);
                                            else
                                                sb.AppendFormat(", `{0}`", notfoundNames[i]);
                                        }
                                        await arg.Channel.SendMessageAsync($"Không tìm thấy máy chủ {sb.ToString()} mà bạn đã khai báo, bạn có chắc rằng bạn đã khai báo chính xác?\nNếu bạn đã chắc chắn, xin vui lòng liên hệ tới quản lý của phòng Discord này để thông báo lỗi.");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await arg.Channel.SendMessageAsync($"Tôi rất tiếc khi đã thất bại trong việc thêm <@{arg.Author.Id}> vào danh sách thành viên của máy chủ mà bạn đã chọn.\nLý do: có lỗi xảy ra trong lúc thực hiện. Stacktrace:\n```\ncsharp{ex.ToString()}\n```");
                        }
                    }
                }
            else
            {
                if (arg.Content.StartsWith("/userinfo", StringComparison.OrdinalIgnoreCase))
                {
                    SocketGuildUser guilduser = arg.Author as SocketGuildUser;
                    if (guilduser != null)
                        if (this.cache_roles.ContainsKey(guilduser.Guild.Id) && this.IsAlreadyHasRole(guilduser, this.cache_roles[guilduser.Guild.Id].Manager))
                            using (IDisposable typing = arg.Channel.EnterTypingState())
                            using (var userwalker = arg.MentionedUsers.GetEnumerator())
                            {
                                if (!userwalker.MoveNext())
                                    await arg.Channel.SendMessageAsync("Nếu bạn không thấy bất kỳ thông tin nào khác ngoài dòng chữ này, xin vui lòng bật `Link Preview` cho discord client của bạn.", false, Commands.GetUserInfo(guilduser));
                                else
                                {
                                    SocketGuildUser targetguilduser = userwalker.Current as SocketGuildUser;
                                    if (targetguilduser != null)
                                        await arg.Channel.SendMessageAsync("Nếu bạn không thấy bất kỳ thông tin nào khác ngoài dòng chữ này, xin vui lòng bật `Link Preview` cho discord client của bạn.", false, Commands.GetUserInfo(targetguilduser));
                                    else
                                        await arg.Channel.SendMessageAsync("Bạn không thể xem thông tin của người khác ở nơi mà người đó không đọc được tin nhắn này.");
                                }
                            }
                }
            }
        }

        private bool IsAlreadyHasRole(SocketGuildUser user, SocketRole role)
        {
            foreach (var current in user.Roles)
                if (current.Id == role.Id)
                    return true;
            return false;
        }

        private Dictionary<ulong, ServerInfo> cache_roles;
        private Task Client_Ready()
        {
            cache_roles = new Dictionary<ulong, ServerInfo>();
            ServerInfo currentguild;
            string currentname;
            foreach (var guild in client.Guilds)
            {
                currentguild = new ServerInfo();
                cache_roles.Add(guild.Id, currentguild);
                foreach (SocketRole role in guild.Roles)
                {
                    // Add some better define
                    currentname = role.Name.ToLower();
                    switch (currentname)
                    {
                        // Server names
                        case "đại mạc":
                            currentguild.ServerNames.Add(currentname, role);
                            break;
                        case "thiên đỉnh":
                            currentguild.ServerNames.Add(currentname, role);
                            break;
                        case "đồng bằng thủy nguyệt":
                            currentguild.ServerNames.Add(currentname, role);
                            currentguild.ServerNames.Add("đồng bằng", role);
                            currentguild.ServerNames.Add("thủy nguyệt", role);
                            break;
                        case "núi sương bạc":
                            currentguild.ServerNames.Add(currentname, role);
                            currentguild.ServerNames.Add("núi", role);
                            currentguild.ServerNames.Add("sương bạc", role);
                            break;
                        case "bờ biển lục lam":
                            currentguild.ServerNames.Add(currentname, role);
                            currentguild.ServerNames.Add("biển lục lam", role);
                            currentguild.ServerNames.Add("biển", role);
                            currentguild.ServerNames.Add("lục lam", role);
                            break;

                        // Manager
                        case "quản lý":
                            currentguild.SetManagerRole(role);
                            break;
                        case "manager":
                            currentguild.SetManagerRole(role);
                            break;
                    }
                }
            }
            Console.WriteLine("Bot is ready.");
            return Task.CompletedTask;
        }

        public void Run(params string[] arguments)
        {
            this.args = arguments;
            this.Run();
        }

        public void Run()
        {
            this.Login().GetAwaiter().GetResult();
        }

        public void Restart()
        {

        }

        public void Stop(int timeout)
        {
            Task exitTask = this.Logout();
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate
            {
                System.Threading.Thread.Sleep(timeout);
                if (!exitTask.IsCompleted)
                    Environment.Exit(1);
            }));
            exitTask.GetAwaiter().GetResult();
        }

        private Task Client_LoggedIn()
        {
            Console.WriteLine("Bot logged in. Preparing cache...");
            return Task.CompletedTask;
        }

        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Info.BotInfo.BotToken))
                throw new ArgumentNullException();
            await this.client.LoginAsync(TokenType.Bot, Info.BotInfo.BotToken);
            await this.client.StartAsync();
        }

        private async Task Logout()
        {
            await this.client.LogoutAsync();
        }
    }
}
