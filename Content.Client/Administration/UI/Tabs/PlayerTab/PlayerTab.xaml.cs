using System.Linq;
using Content.Client.Administration.Systems;
using Content.Shared.Administration;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using static Content.Client.Administration.UI.Tabs.PlayerTab.PlayerTabHeader;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Administration.UI.Tabs.PlayerTab
{
    [GenerateTypedNameReferences]
    public sealed partial class PlayerTab : Control
    {
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IPlayerManager _playerMan = default!;

        private const string ArrowUp = "↑";
        private const string ArrowDown = "↓";
        private readonly Color _altColor = Color.FromHex("#292B38");
        private readonly Color _defaultColor = Color.FromHex("#2F2F3B");
        private readonly AdminSystem _adminSystem;
        private IReadOnlyList<PlayerInfo> _players = new List<PlayerInfo>();

        private Header _headerClicked = Header.Username;
        private bool _ascending = true;
        private bool _showDisconnected;

        public event Action<ButtonEventArgs>? OnEntryPressed;

        public PlayerTab()
        {
            IoCManager.InjectDependencies(this);
            _adminSystem = _entManager.System<AdminSystem>();
            RobustXamlLoader.Load(this);
            RefreshPlayerList(_adminSystem.PlayerList);

            _adminSystem.PlayerListChanged += RefreshPlayerList;
            _adminSystem.OverlayEnabled += OverlayEnabled;
            _adminSystem.OverlayDisabled += OverlayDisabled;

            OverlayButton.OnPressed += OverlayButtonPressed;
            ShowDisconnectedButton.OnPressed += ShowDisconnectedPressed;

            ListHeader.BackgroundColorPanel.PanelOverride = new StyleBoxFlat(_altColor);
            ListHeader.OnHeaderClicked += HeaderClicked;
        }

        private void OverlayEnabled()
        {
            OverlayButton.Pressed = true;
        }

        private void OverlayDisabled()
        {
            OverlayButton.Pressed = false;
        }

        private void OverlayButtonPressed(ButtonEventArgs args)
        {
            if (args.Button.Pressed)
            {
                _adminSystem.AdminOverlayOn();
            }
            else
            {
                _adminSystem.AdminOverlayOff();
            }
        }

        private void ShowDisconnectedPressed(ButtonEventArgs args)
        {
            _showDisconnected = args.Button.Pressed;
            RefreshPlayerList(_players);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _adminSystem.PlayerListChanged -= RefreshPlayerList;
                _adminSystem.OverlayEnabled -= OverlayEnabled;
                _adminSystem.OverlayDisabled -= OverlayDisabled;

                OverlayButton.OnPressed -= OverlayButtonPressed;

                ListHeader.OnHeaderClicked -= HeaderClicked;
            }
        }

        private void RefreshPlayerList(IReadOnlyList<PlayerInfo> players)
        {
            foreach (var child in PlayerList.Children.ToArray())
            {
                if (child is PlayerTabEntry)
                    child.Dispose();
            }

            _players = players;
            PlayerCount.Text = $"Players: {_playerMan.PlayerCount}";

            var sortedPlayers = new List<PlayerInfo>(players);
            sortedPlayers.Sort(Compare);

            UpdateHeaderSymbols();

            var useAltColor = false;
            foreach (var player in sortedPlayers)
            {
                if (!_showDisconnected && !player.Connected)
                    continue;

                var entry = new PlayerTabEntry(player.Username,
                    player.CharacterName,
                    player.IdentityName,
                    player.StartingJob,
                    player.Antag ? "YES" : "NO",
                    player.Sponsor ? "YES" : "NO",
                    new StyleBoxFlat(useAltColor ? _altColor : _defaultColor),
                    player.Connected,
                    player.PlaytimeString);
                entry.PlayerEntity = player.NetEntity;
                entry.OnPressed += args => OnEntryPressed?.Invoke(args);
                entry.ToolTip = Loc.GetString("player-tab-entry-tooltip");
                PlayerList.AddChild(entry);

                useAltColor ^= true;
            }
        }

        private void UpdateHeaderSymbols()
        {
            ListHeader.ResetHeaderText();
            ListHeader.GetHeader(_headerClicked).Text += $" {(_ascending ? ArrowUp : ArrowDown)}";
        }

        private int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (!_ascending)
            {
                (x, y) = (y, x);
            }

            return _headerClicked switch
            {
                Header.Username => Compare(x.Username, y.Username),
                Header.Character => Compare(x.CharacterName, y.CharacterName),
                Header.Job => Compare(x.StartingJob, y.StartingJob),
                Header.Antagonist => x.Antag.CompareTo(y.Antag),
                Header.Playtime => TimeSpan.Compare(x.OverallPlaytime ?? default, y.OverallPlaytime ?? default),
                _ => 1
            };
        }

        private int Compare(string x, string y)
        {
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }

        private void HeaderClicked(Header header)
        {
            if (_headerClicked == header)
            {
                _ascending = !_ascending;
            }
            else
            {
                _headerClicked = header;
                _ascending = true;
            }

            RefreshPlayerList(_adminSystem.PlayerList);
        }
    }
}
