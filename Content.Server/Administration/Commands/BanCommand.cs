using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Content.Server.Database;
using Content.Server.Discord.Webhooks;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;


namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Ban)]
    public sealed class BanCommand : LocalizedCommands
    {
        public override string Command => "ban";

        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private readonly HttpClient _httpClient = new();

        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player as IPlayerSession;
            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            var locator = IoCManager.Resolve<IPlayerLocator>();
            var dbMan = IoCManager.Resolve<IServerDbManager>();
            var webhookUrl = _cfg.GetCVar(CCVars.DiscordBansWebhook);
            var serverName = _cfg.GetCVar(CCVars.GameHostName);

            serverName = serverName[..Math.Min(serverName.Length, 1500)];

            var gameTicker = _entitySystemManager.GetEntitySystem<GameTicker>();
            var round = gameTicker.RunLevel switch
            {
                GameRunLevel.PreRoundLobby => gameTicker.RoundId == 0
                    ? "pre-round lobby after server restart" // first round after server restart has ID == 0
                    : $"pre-round lobby for round {gameTicker.RoundId + 1}",
                GameRunLevel.InRound => $"round {gameTicker.RoundId}",
                GameRunLevel.PostRound => $"post-round {gameTicker.RoundId}",
                _ => throw new ArgumentOutOfRangeException(nameof(gameTicker.RunLevel), $"{gameTicker.RunLevel} was not matched."),
            };


            string target;
            string reason;
            uint minutes;

            switch (args.Length)
            {
                case 2:
                    target = args[0];
                    reason = args[1];
                    minutes = 0;
                    break;
                case 3:
                    target = args[0];
                    reason = args[1];

                    if (!uint.TryParse(args[2], out minutes))
                    {
                        shell.WriteLine($"{args[2]} is not a valid amount of minutes.\n{Help}");
                        return;
                    }

                    break;
                default:
                    shell.WriteLine($"Invalid amount of arguments.{Help}");
                    return;
            }

            var located = await locator.LookupIdByNameOrIdAsync(target);
            if (located == null)
            {
                shell.WriteError(LocalizationManager.GetString("cmd-ban-player"));
                return;
            }

            var targetUid = located.UserId;
            var targetHWid = located.LastHWId;
            var targetAddr = located.LastAddress;

            if (player != null && player.UserId == targetUid)
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-ban-self"));
                return;
            }

            DateTimeOffset? expires = null;
            if (minutes > 0)
            {
                expires = DateTimeOffset.Now + TimeSpan.FromMinutes(minutes);
            }

            (IPAddress, int)? addrRange = null;
            if (targetAddr != null)
            {
                if (targetAddr.IsIPv4MappedToIPv6)
                    targetAddr = targetAddr.MapToIPv4();

                // Ban /64 for IPv4, /32 for IPv4.
                var cidr = targetAddr.AddressFamily == AddressFamily.InterNetworkV6 ? 64 : 32;
                addrRange = (targetAddr, cidr);
            }

            var banDef = new ServerBanDef(
                null,
                targetUid,
                addrRange,
                targetHWid,
                DateTimeOffset.Now,
                expires,
                reason,
                player?.UserId,
                null);

            await dbMan.AddServerBanAsync(banDef);
            
            var banId = await dbMan.GetLastServerBanId();

            if (!string.IsNullOrEmpty(webhookUrl))
            {
                var payload = new WebhookPayload
                {
                    Username = "Это бан",
                    AvatarUrl = "",
                    Embeds = new List<Embed>
                    {
                        new Embed
                        {
                            Color = 0xff0000,
                            Description = GenerateBanDescription(banId, target, player, minutes, reason, expires),
                            Footer = new EmbedFooter
                            {
                                Text = $"{serverName} ({round})"
                            }
                        }
                    }
                };

                await _httpClient.PostAsync($"{webhookUrl}?wait=true",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            }

            var response = new StringBuilder($"Banned {target} with reason \"{reason}\"");

            response.Append(expires == null ? " permanently." : $" until {expires}");

            shell.WriteLine(response.ToString());

            if (plyMgr.TryGetSessionById(targetUid, out var targetPlayer))
            {
                var message = banDef.FormatBanMessage(_cfg, LocalizationManager);
                targetPlayer.ConnectedClient.Disconnect(message);
            }
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var playerMgr = IoCManager.Resolve<IPlayerManager>();
                var options = playerMgr.ServerSessions.Select(c => c.Name).OrderBy(c => c).ToArray();
                return CompletionResult.FromHintOptions(options, LocalizationManager.GetString("cmd-ban-hint"));
            }

            if (args.Length == 2)
                return CompletionResult.FromHint(LocalizationManager.GetString("cmd-ban-hint-reason"));

            if (args.Length == 3)
            {
                var durations = new CompletionOption[]
                {
                    new("0", LocalizationManager.GetString("cmd-ban-hint-duration-1")),
                    new("1440", LocalizationManager.GetString("cmd-ban-hint-duration-2")),
                    new("4320", LocalizationManager.GetString("cmd-ban-hint-duration-3")),
                    new("10080", LocalizationManager.GetString("cmd-ban-hint-duration-4")),
                    new("20160", LocalizationManager.GetString("cmd-ban-hint-duration-5")),
                    new("43800", LocalizationManager.GetString("cmd-ban-hint-duration-6")),
                };

                return CompletionResult.FromHintOptions(durations, LocalizationManager.GetString("cmd-ban-hint-duration"));
            }

            return CompletionResult.Empty;
        }

        private string GenerateBanDescription(int banId, string target, IPlayerSession? player, uint minutes, string reason, DateTimeOffset? expires)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"### **Бан | ID {banId}**");
            builder.AppendLine($"**Нарушитель:** *{target}*");
            builder.AppendLine($"**Причина:** {reason}");

            var banDuration = TimeSpan.FromMinutes(minutes);

            builder.Append($"**Длительность:** ");

            if (expires != null)
            {
                builder.Append($"{banDuration.Days} {NumWord(banDuration.Days, "день", "дня", "дней")}, ");
                builder.Append($"{banDuration.Hours} {NumWord(banDuration.Hours, "час", "часа", "часов")}, ");
                builder.AppendLine($"{banDuration.Minutes} {NumWord(banDuration.Minutes, "минута", "минуты", "минут")}");

            }
            else
            {
                builder.AppendLine($"***Навсегда***");
            }

            if (expires != null)
            {
                builder.AppendLine($"**Дата снятия наказания:** {expires}");
            }

            builder.Append($"**Наказание выдал(-а):** ");

            if (player != null)
            {
                builder.AppendLine($"*{player.Name}*");
            }
            else
            {
                builder.AppendLine($"***СИСТЕМА***");
            }

            return builder.ToString();
        }

        private string NumWord(int value, params string[] words)
        {
            value = Math.Abs(value) % 100;
            var num = value % 10;

            if (value > 10 && value < 20)
            {
                return words[2];
            }

            if (value > 1 && value < 5)
            {
                return words[1];
            }

            if (num == 1)
            {
                return words[0];
            }

            return words[2];
        }
    }
}
