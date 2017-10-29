using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace BnSVN_Discord_Bot
{
    public static class Commands
    {
        public static Embed GetUserInfo(SocketGuildUser user)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithThumbnailUrl(user.GetAvatarUrl());
            
            eb.WithDescription(user.Mention);
            if (user.Game.HasValue)
            {
                switch (user.Game.Value.StreamType)
                {
                    case StreamType.Twitch:
                        eb.AddField("Đang", $"Stream twitch: [{user.Game.Value.Name}]({user.Game.Value.StreamUrl})");
                        break;
                    default:
                        eb.AddField("Đang", $"Chơi: {user.Game.Value.Name}");
                        break;
                }
            }
            else
                eb.AddField("Đang", "Không làm gì. Có lẽ đang hóng???");

            DateTimeOffset current = DateTimeOffset.UtcNow;
            string tmp;
            if (user.JoinedAt.HasValue)
            {
                tmp = user.JoinedAt.Value.ToUniversalTime().ToString().Replace("+00:00", "UTC", StringComparison.Ordinal);
                tmp += $"\n({Convert.ToInt32(current.Subtract(user.JoinedAt.Value).TotalDays)} ngày trước)";
            }
            else
                tmp = "Không rõ";
            eb.AddInlineField("Tham gia vào phòng discord", tmp);

            tmp = user.CreatedAt.ToUniversalTime().ToString().Replace("+00:00", "UTC", StringComparison.Ordinal);
            tmp += $"\n({Convert.ToInt32(current.Subtract(user.CreatedAt).TotalDays)} ngày trước)";

            eb.AddInlineField("Tài khoản được tạo", tmp);
            eb.AddInlineField("Là Bot?", user.IsBot ? "Đúng" : "Sai");

            switch (user.Status)
            {
                case UserStatus.DoNotDisturb:
                    eb.AddInlineField("Trạng thái hiện tại", "Miễn làm phiền");
                    break;
                case UserStatus.AFK:
                    eb.AddInlineField("Trạng thái hiện tại", "Buông bàn phím");
                    break;
                case UserStatus.Idle:
                    eb.AddInlineField("Trạng thái hiện tại", "Treo discord");
                    break;
                case UserStatus.Invisible:
                    eb.AddInlineField("Trạng thái hiện tại", "Núp trong bụi");
                    break;
                default:
                    eb.AddInlineField("Trạng thái hiện tại", user.Status.ToString());
                    break;
            }

            Color embedColor = user.Guild.EveryoneRole.Color;

            StringBuilder sb = new StringBuilder();
            bool firstline = true;
            foreach (var role in user.Roles)
                if (role != user.Guild.EveryoneRole)
                {
                    if (firstline)
                    {
                        firstline = false;
                        sb.Append(role.Name);
                        embedColor = role.Color;
                    }
                    else
                        sb.AppendFormat(", {0}", role.Name);
                }
            if (sb.Length != 0)
                eb.AddField("Roles", sb.ToString());
            else
                eb.AddField("Roles", "Thường dân");

            eb.WithFooter($"User ID: {user.Id}");
            eb.WithColor(embedColor);

            return eb.Build();
        }
    }
}
