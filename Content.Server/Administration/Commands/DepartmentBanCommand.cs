using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Discord.Webhooks;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using System.Net.Http;
using Content.Server.Database;
using System.Text.Json;
using System.Text;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class DepartmentBanCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerLocator _locater = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly RoleBanManager _bans = default!;

    public string Command => "departmentban";
    public string Description => Loc.GetString("cmd-departmentban-desc");
    public string Help => Loc.GetString("cmd-departmentban-help");

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    private readonly HttpClient _httpClient = new();

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player as IPlayerSession;
        var webhookUrl = _cfg.GetCVar(CCVars.DiscordBansWebhook);
        var serverName = _cfg.GetCVar(CCVars.GameHostName);
        var dbMan = IoCManager.Resolve<IServerDbManager>();

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
        string department;
        string reason;
        uint minutes;

        switch (args.Length)
        {
            case 3:
                target = args[0];
                department = args[1];
                reason = args[2];
                minutes = 0;
                break;
            case 4:
                target = args[0];
                department = args[1];
                reason = args[2];

                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-roleban-minutes-parse", ("time", args[3]), ("help", Help)));
                    return;
                }

                break;
            default:
                shell.WriteError(Loc.GetString("cmd-roleban-arg-count"));
                shell.WriteLine(Help);
                return;
        }

        DateTimeOffset? expires = null;
        if (minutes > 0)
        {
            expires = DateTimeOffset.Now + TimeSpan.FromMinutes(minutes);
        }

        var startRoleBanId = await dbMan.GetLastServerRoleBanId() + 1;

        if (!_protoManager.TryIndex<DepartmentPrototype>(department, out var departmentProto))
        {
            return;
        }

        var located = await _locater.LookupIdByNameOrIdAsync(target);

        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-roleban-name-parse"));
            return;
        }

        foreach (var job in departmentProto.Roles)
        {
            await _bans.CreateJobBan(shell, located, job, reason, minutes);
        }

        if (webhookUrl != null)
        {
            var roleBanIdsString = "";

            if (departmentProto.Roles != null && departmentProto.Roles.Count > 0)
            {
                int[] roleBanIds;
                roleBanIds = new int[departmentProto.Roles.Count];
                roleBanIds[0] = startRoleBanId;

                for (var i = 1; i < roleBanIds.Length; i++)
                {
                    roleBanIds[i] = roleBanIds[i - 1] + 1;
                }

                roleBanIdsString = string.Join(", ", roleBanIds);
            }


            var payload = new WebhookPayload
            {
                Username = "Это департмент-бан",
                AvatarUrl = "",
                Embeds = new List<Embed>
                {
                    new Embed
                    {
                        Color = 0xffea00,
                        Description = GenerateBanDescription(roleBanIdsString, target, player, minutes, reason, expires, department),
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

        _bans.SendRoleBans(located);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var durOpts = new CompletionOption[]
        {
            new("0", Loc.GetString("cmd-roleban-hint-duration-1")),
            new("1440", Loc.GetString("cmd-roleban-hint-duration-2")),
            new("4320", Loc.GetString("cmd-roleban-hint-duration-3")),
            new("10080", Loc.GetString("cmd-roleban-hint-duration-4")),
            new("20160", Loc.GetString("cmd-roleban-hint-duration-5")),
            new("43800", Loc.GetString("cmd-roleban-hint-duration-6")),
        };

        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-roleban-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<DepartmentPrototype>(),
                Loc.GetString("cmd-roleban-hint-2")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-roleban-hint-3")),
            4 => CompletionResult.FromHintOptions(durOpts, Loc.GetString("cmd-roleban-hint-4")),
            _ => CompletionResult.Empty
        };
    }

    private string GenerateBanDescription(string roleBanIdsString, string target, IPlayerSession? player, uint minutes, string reason, DateTimeOffset? expires, string department)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"### **Департмент-бан | IDs {roleBanIdsString}**");
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

        builder.AppendLine($"**Отдел:** {department}");

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
