using Content.Server.Administration.Commands;
using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Server.Communications;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Humanoid;
using Content.Server.Mind;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.Nuke;
using Content.Shared.Objectives.Components;
using Content.Server.Objectives;
using Content.Server.Popups;
using Content.Server.Preferences.Managers;
using Content.Server.RandomMetadata;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nuke;
using Content.Shared.NukeOps;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Tag;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Phantom.Components;
using Content.Shared.Phantom;
using Content.Server.Phantom.EntitySystems;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Shared.Mind;

namespace Content.Server.GameTicking.Rules;

public sealed class PhantomRuleSystem : GameRuleSystem<PhantomRuleComponent>
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly RandomMetadataSystem _randomMetadata = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("Phantom");

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayersSpawning);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        //SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<PhantomRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);


        SubscribeLocalEvent<PhantomComponent, PhantomDiedEvent>(OnMobStateChanged);
        //SubscribeLocalEvent<PhantomComponent, RefreshPhantomLevelEvent>(OnLevelChanged);
        SubscribeLocalEvent<PhantomComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<PhantomTyranyTargetComponent, MobStateChangedEvent>(OnCommandMobStateChanged);

        SubscribeLocalEvent<PhantomComponent, PhantomTyranyEvent>(OnTyranyAttempt);
        SubscribeLocalEvent<PhantomComponent, PhantomNightmareEvent>(OnNightmareAttempt);
        SubscribeLocalEvent<PhantomComponent, PhantomReincarnatedEvent>(OnReincarnation);
    }

    protected override void Started(EntityUid uid, PhantomRuleComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        //if (GameTicker.RunLevel == GameRunLevel.InRound)
        //    SpawnPhantomForGhostRoles(uid, component);
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        TryRoundStartAttempt(ev, Loc.GetString("nukeops-title"));
    }

    private void OnPlayersSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var phantomRule, out _))
        {
            if (ev.PlayerPool.Count == 0)
                continue;

            var phantomEligible = _antagSelection.GetEligibleSessions(ev.PlayerPool, phantomRule.PhantomSpawnDetails.AntagRoleProto);

            var selectedPhantom = _antagSelection.ChooseAntags(1, phantomEligible, ev.PlayerPool);

            var phantom = new PhantomSpawn(selectedPhantom.FirstOrDefault(), phantomRule.PhantomSpawnDetails);

            SpawnPhantom(phantom, phantomRule);

            if (phantom.Session == null)
                continue;

            GameTicker.PlayerJoinGame(phantom.Session);

        }
    }

    private void SpawnPhantom(PhantomSpawn phantomSession, PhantomRuleComponent component)
    {
        var spawns = new List<EntityCoordinates>();
        foreach (var (_, meta, xform) in EntityQuery<SpawnPointComponent, MetaDataComponent, TransformComponent>(true))
        {
            if (meta.EntityPrototype?.ID != component.SpawnPointProto.Id)
                continue;

            spawns.Add(xform.Coordinates);
            break;
        }

        //Fallback, spawn at the centre of the map
        //if (spawns.Count == 0)
        //{
        //    spawns.Add(Transform(outpostUid).Coordinates);
        //    _sawmill.Warning($"Fell back to default spawn for nukies!");
        //}

        var phantomAntag = _prototypeManager.Index(phantomSession.Type.AntagRoleProto);

        //If a session is available, spawn mob and transfer mind into it
        if (phantomSession.Session != null)
        {

            var mob = Spawn("ADTMobPhantom", RobustRandom.Pick(spawns));
            //SetupOperativeEntity(mob, name, nukieSession.Type, profile);

            var newMind = _mind.CreateMind(phantomSession.Session.UserId, "Phantom");
            _mind.SetUserId(newMind, phantomSession.Session.UserId);
            _roles.MindAddRole(newMind, new PhantomRoleComponent { PrototypeId = phantomSession.Type.AntagRoleProto });
            AddObjectives(newMind.Owner, newMind.Comp, component);

            if (newMind.Comp.Session != null)
                _antagSelection.SendBriefing(newMind.Comp.Session, Loc.GetString("phantom-welcome"), Color.BlueViolet, component.GreetSoundNotification);

            // Automatically de-admin players who are being made nukeops
            if (_cfg.GetCVar(CCVars.AdminDeadminOnJoin) && _adminManager.IsAdmin(phantomSession.Session))
                _adminManager.DeAdmin(phantomSession.Session);

            _mind.TransferTo(newMind, mob);
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out _, out var phantom, out _))
        {
            foreach (var cond in phantom.WinConditions)
            {
                if (cond == PhantomWinCondition.TyranyAttemped)
                {
                    SetWinType(phantom.Owner, PhantomWinType.PhantomMajor);
                    phantom.WinConditions.Add(PhantomWinCondition.TyranySuccess);
                }
                if (_mind.TryGetMind(phantom.PhantomMind, out _, out var mind) && mind.OwnedEntity != null)
                {
                    if (HasComp<PhantomComponent>(mind.OwnedEntity) || HasComp<PhantomPuppetComponent>(mind.OwnedEntity))
                        phantom.WinConditions.Add(PhantomWinCondition.PhantomAlive);
                    if (HasComp<PhantomPuppetComponent>(mind.OwnedEntity))
                        SetWinType(phantom.Owner, PhantomWinType.PhantomMajor);
                }
            }

            var winText = Loc.GetString($"phantom-{phantom.WinType.ToString().ToLower()}");
            ev.AddLine(winText);

            foreach (var cond in phantom.WinConditions)
            {
                var text = Loc.GetString($"phantom-cond-{cond.ToString().ToLower()}");
                ev.AddLine(text);
            }
        }

        // Необходимость под сомнением
        //ev.AddLine(Loc.GetString("phantom-list-start"));

        //var phantomQuery = EntityQueryEnumerator<PhantomRoleComponent, MindContainerComponent>();
        //while (phantomQuery.MoveNext(out var phantomUid, out _, out var mindContainer))
        //{
        //    if (!_mind.TryGetMind(phantomUid, out _, out var mind, mindContainer))
        //        continue;

        //    ev.AddLine(mind.Session != null
        //        ? Loc.GetString("phantom-list-name-user", ("name", Name(phantomUid)), ("user", mind.Session.Name))
        //        : Loc.GetString("phantom-list-name", ("name", Name(phantomUid))));
        //}
    }

    private void OnMobStateChanged(EntityUid uid, PhantomComponent component, PhantomDiedEvent args)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out _, out var phantom, out _))
        {
            if (component.FinalAbilityUsed)
                phantom.WinType = PhantomWinType.CrewMinor;
            else
                phantom.WinType = PhantomWinType.CrewMajor;
        }
    }

    private void OnTyranyAttempt(EntityUid uid, PhantomComponent component, PhantomTyranyEvent args)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out _, out var phantom, out _))
        {
            phantom.WinConditions.Add(PhantomWinCondition.TyranyAttemped);
            SetWinType(phantom.Owner, PhantomWinType.PhantomMinor);
        }
    }

    private void OnNightmareAttempt(EntityUid uid, PhantomComponent component, PhantomNightmareEvent args)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out _, out var phantom, out _))
        {
            phantom.WinConditions.Add(PhantomWinCondition.NightmareStarted);
            SetWinType(phantom.Owner, PhantomWinType.PhantomMajor);
        }
    }

    private void OnReincarnation(EntityUid uid, PhantomComponent component, PhantomReincarnatedEvent args)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out _, out var phantom, out _))
        {
            phantom.WinConditions.Add(PhantomWinCondition.PhantomReincarnated);
            SetWinType(phantom.Owner, PhantomWinType.PhantomMinor);
        }
    }

    private void OnMindAdded(EntityUid uid, PhantomComponent component, MindAddedMessage args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind) || mind.Session == null)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var nukeops, out _))
        {
            if (nukeops.OperativeMindPendingData.TryGetValue(uid, out var role) ||
                nukeops.RoundEndBehavior == RoundEndBehavior.Nothing)
            {
                role ??= nukeops.PhantomSpawnDetails.AntagRoleProto;
                _roles.MindAddRole(mindId, new PhantomRoleComponent { PrototypeId = role });
                AddObjectives(mindId, mind, nukeops);
                nukeops.OperativeMindPendingData.Remove(uid);
                nukeops.PhantomMind = mindId;
                _antagSelection.SendBriefing(mind.Session, Loc.GetString("phantom-welcome"), Color.BlueViolet, component.GreetSoundNotification);
            }

            if (mind.Session is not { } playerSession)
                return;

            if (GameTicker.RunLevel != GameRunLevel.InRound)
                return;
        }
    }

    private void SetWinType(EntityUid uid, PhantomWinType type, PhantomRuleComponent? component = null, bool endRound = true)
    {
        if (!Resolve(uid, ref component))
            return;

        component.WinType = type;

        if (endRound && (type == PhantomWinType.CrewMajor || type == PhantomWinType.PhantomMajor))
            _roundEndSystem.EndRound();
    }

    private void OnCommandMobStateChanged(EntityUid uid, PhantomTyranyTargetComponent comp, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead || ev.NewMobState == MobState.Invalid)
            CheckCommandLose();
            
    }

    private bool CheckCommandLose()
    {
        var commandList = new List<EntityUid>();

        var heads = AllEntityQuery<CommandStaffComponent>();
        while (heads.MoveNext(out var id, out _))
        {
            commandList.Add(id);
        }

        return IsGroupDead(commandList, true);
    }

    private bool IsGroupDead(List<EntityUid> list, bool checkOffStation)
    {
        var dead = 0;
        foreach (var entity in list)
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                if (state.CurrentState == MobState.Dead || state.CurrentState == MobState.Invalid)
                {
                    dead++;
                }
                else if (checkOffStation && _stationSystem.GetOwningStation(entity) == null && !_emergencyShuttle.EmergencyShuttleArrived)
                {
                    dead++;
                }
            }
            //If they don't have the MobStateComponent they might as well be dead.
            else
            {
                dead++;
            }
        }

        return dead == list.Count || list.Count == 0;
    }

    private void OnObjectivesTextGetInfo(EntityUid uid, PhantomRuleComponent comp, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds.Add(comp.PhantomMind);
        args.AgentName = Loc.GetString("traitor-round-end-phantom-name");
    }

    private void AddObjectives(EntityUid mindId, MindComponent mind, PhantomRuleComponent component)
    {
        var maxDifficulty = _cfg.GetCVar(CCVars.PhantomMaxDifficulty);
        var maxPicks = _cfg.GetCVar(CCVars.PhantomMaxPicks);
        var difficulty = 0f;
        Log.Debug($"Attempting {maxPicks} objective picks with {maxDifficulty} difficulty");
        for (var pick = 0; pick < maxPicks && maxDifficulty > difficulty; pick++)
        {
            var objective = _objectives.GetRandomObjective(mindId, mind, component.ObjectiveGroup);
            if (objective == null)
                continue;

            _mind.AddObjective(mindId, mind, objective.Value);
            var adding = Comp<ObjectiveComponent>(objective.Value).Difficulty;
            difficulty += adding;
            Log.Debug($"Added objective {ToPrettyString(objective):objective} with {adding} difficulty");
        }
        var finObjective = _objectives.GetRandomObjective(mindId, mind, component.FinalObjectiveGroup);
        if (finObjective == null)
            return;

        _mind.AddObjective(mindId, mind, finObjective.Value);

    }
    private sealed class PhantomSpawn
    {
        public ICommonSession? Session { get; private set; }
        public PhantomSpawnPreset Type { get; private set; }

        public PhantomSpawn(ICommonSession? session, PhantomSpawnPreset type)
        {
            Session = session;
            Type = type;
        }
    }
}
