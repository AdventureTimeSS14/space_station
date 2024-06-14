using Content.Client.Administration.Managers;
using Content.Client.Changelog;
using Content.Client.Chat.Managers;
using Content.Client.Clickable;
<<<<<<< HEAD
using Content.Client.Corvax.DiscordAuth;
using Content.Client.Corvax.JoinQueue;
using Content.Client.Corvax.Sponsors;
using Content.Client.Corvax.TTS;
using Content.Client.Options;
=======
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
using Content.Client.Eui;
using Content.Client.GhostKick;
using Content.Client.Launcher;
using Content.Client.Parallax.Managers;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Screenshot;
using Content.Client.Fullscreen;
using Content.Client.Stylesheets;
using Content.Client.Viewport;
using Content.Client.Voting;
using Content.Shared.Administration.Logs;
using Content.Client.Guidebook;
using Content.Client.Lobby;
using Content.Client.Replay;
using Content.Shared.Administration.Managers;
using Content.Shared.Players.PlayTimeTracking;


namespace Content.Client.IoC
{
    internal static class ClientContentIoC
    {
        public static void Register()
        {
<<<<<<< HEAD
            IoCManager.Register<IParallaxManager, ParallaxManager>();
            IoCManager.Register<IChatManager, ChatManager>();
            IoCManager.Register<IClientPreferencesManager, ClientPreferencesManager>();
            IoCManager.Register<IStylesheetManager, StylesheetManager>();
            IoCManager.Register<IScreenshotHook, ScreenshotHook>();
            IoCManager.Register<FullscreenHook, FullscreenHook>();
            IoCManager.Register<IClickMapManager, ClickMapManager>();
            IoCManager.Register<IClientAdminManager, ClientAdminManager>();
            IoCManager.Register<ISharedAdminManager, ClientAdminManager>();
            IoCManager.Register<EuiManager, EuiManager>();
            IoCManager.Register<IVoteManager, VoteManager>();
            IoCManager.Register<ChangelogManager, ChangelogManager>();
            IoCManager.Register<RulesManager, RulesManager>();
            IoCManager.Register<ViewportManager, ViewportManager>();
            IoCManager.Register<ISharedAdminLogManager, SharedAdminLogManager>();
            IoCManager.Register<GhostKickManager>();
            IoCManager.Register<ExtendedDisconnectInformationManager>();
            IoCManager.Register<JobRequirementsManager>();
            IoCManager.Register<SponsorsManager>(); // Corvax-Sponsors
            IoCManager.Register<JoinQueueManager>(); // Corvax-Queue
            IoCManager.Register<DiscordAuthManager>(); // Corvax-DiscordAuth
            IoCManager.Register<DocumentParsingManager>();
            IoCManager.Register<ContentReplayPlaybackManager, ContentReplayPlaybackManager>();
=======
            var collection = IoCManager.Instance!;

            collection.Register<IParallaxManager, ParallaxManager>();
            collection.Register<IChatManager, ChatManager>();
            collection.Register<IClientPreferencesManager, ClientPreferencesManager>();
            collection.Register<IStylesheetManager, StylesheetManager>();
            collection.Register<IScreenshotHook, ScreenshotHook>();
            collection.Register<FullscreenHook, FullscreenHook>();
            collection.Register<IClickMapManager, ClickMapManager>();
            collection.Register<IClientAdminManager, ClientAdminManager>();
            collection.Register<ISharedAdminManager, ClientAdminManager>();
            collection.Register<EuiManager, EuiManager>();
            collection.Register<IVoteManager, VoteManager>();
            collection.Register<ChangelogManager, ChangelogManager>();
            collection.Register<ViewportManager, ViewportManager>();
            collection.Register<ISharedAdminLogManager, SharedAdminLogManager>();
            collection.Register<GhostKickManager>();
            collection.Register<ExtendedDisconnectInformationManager>();
            collection.Register<JobRequirementsManager>();
            collection.Register<DocumentParsingManager>();
            collection.Register<ContentReplayPlaybackManager, ContentReplayPlaybackManager>();
            collection.Register<ISharedPlaytimeManager, JobRequirementsManager>();
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
        }
    }
}
