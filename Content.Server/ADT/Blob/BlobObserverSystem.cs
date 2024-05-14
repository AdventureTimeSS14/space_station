using System.Linq;
using System.Numerics;
using Content.Server.Actions;
using Content.Server.Destructible;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Blob;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Blob;

public sealed class BlobObserverSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AnchorableSystem _anchorableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobObserverComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobObserverComponent, BlobCreateFactoryActionEvent>(OnCreateFactory);
        SubscribeLocalEvent<BlobObserverComponent, BlobCreateResourceActionEvent>(OnCreateResource);
        SubscribeLocalEvent<BlobObserverComponent, BlobCreateNodeActionEvent>(OnCreateNode);
        SubscribeLocalEvent<BlobObserverComponent, BlobCreateBlobbernautActionEvent>(OnCreateBlobbernaut);
        SubscribeLocalEvent<BlobObserverComponent, BlobToCoreActionEvent>(OnBlobToCore);
        SubscribeLocalEvent<BlobObserverComponent, BlobToNodeActionEvent>(OnBlobToNode);
        SubscribeLocalEvent<BlobObserverComponent, BlobHelpActionEvent>(OnBlobHelp);
        SubscribeLocalEvent<BlobObserverComponent, BlobSwapChemActionEvent>(OnBlobSwapChem);
        SubscribeLocalEvent<BlobObserverComponent, InteractNoHandEvent>(OnInteract);
        SubscribeLocalEvent<BlobObserverComponent, BlobSwapCoreActionEvent>(OnSwapCore);
        SubscribeLocalEvent<BlobObserverComponent, BlobSplitCoreActionEvent>(OnSplitCore);
    }

    private void OnBlobHelp(EntityUid uid, BlobObserverComponent observerComponent,
        BlobHelpActionEvent args)
    {
        _popup.PopupEntity(Loc.GetString("blob-help"), uid, uid);
        args.Handled = true;
    }

    private void OnBlobSwapChem(EntityUid uid, BlobObserverComponent observerComponent,
        BlobSwapChemActionEvent args)
    {
        _popup.PopupEntity(Loc.GetString("blob-swap-chem"), uid, uid);
        args.Handled = true;
    }

    private void OnSplitCore(EntityUid uid, BlobObserverComponent observerComponent,
        BlobSplitCoreActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null || !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        EntityUid? blobTile = null;

        foreach (var tileref in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
            {
                if (!TryComp<BlobTileComponent>(ent, out var blobTileComponent))
                    continue;
                blobTile = ent;
                break;
            }
        }

        if (blobTile == null || !TryComp<BlobNodeComponent>(blobTile, out var blobNodeComponent))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), uid, uid);
            args.Handled = true;
            return;
        }

        if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent,
                blobCoreComponent.SplitCoreCost))
        {
            args.Handled = true;
            return;
        }

        QueueDel(blobTile.Value);
        EntityManager.SpawnEntity(blobCoreComponent.CoreBlobTile, args.Target);

        args.Handled = true;
    }


    private void OnSwapCore(EntityUid uid, BlobObserverComponent observerComponent,
        BlobSwapCoreActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null || !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        EntityUid? blobTile = null;

        foreach (var tileref in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
            {
                if (!TryComp<BlobTileComponent>(ent, out var blobTileComponent))
                    continue;
                blobTile = ent;
                break;
            }
        }

        if (blobTile == null || !TryComp<BlobNodeComponent>(blobTile, out var blobNodeComponent))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), uid, uid);
            args.Handled = true;
            return;
        }

        if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent,
                blobCoreComponent.SwapCoreCost))
        {
            args.Handled = true;
            return;
        }

        var nodePos = Transform(blobTile.Value).Coordinates;
        var corePos = Transform(observerComponent.Core.Value).Coordinates;
        _xformSystem.SetCoordinates(observerComponent.Core.Value, nodePos);
        _xformSystem.SetCoordinates(blobTile.Value, corePos);
        _anchorableSystem.TryToggleAnchor(observerComponent.Core.Value, observerComponent.Core.Value, observerComponent.Core.Value);
        _anchorableSystem.TryToggleAnchor(blobTile.Value, blobTile.Value, blobTile.Value);
        args.Handled = true;
    }

    private void OnBlobToNode(EntityUid uid, BlobObserverComponent observerComponent,
        BlobToNodeActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null || !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var blobNodes = new List<EntityUid>();

        var blobNodeQuery = EntityQueryEnumerator<BlobNodeComponent, BlobTileComponent>();
        while (blobNodeQuery.MoveNext(out var ent, out var node, out var tile))
        {
            if (tile.Core == observerComponent.Core.Value && !HasComp<BlobCoreComponent>(ent))
                blobNodes.Add(ent);
        }

        if (blobNodes.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("blob-not-have-nodes"), uid, uid);
            args.Handled = true;
            return;
        }

        _xformSystem.SetCoordinates(uid, Transform(_random.Pick(blobNodes)).Coordinates);
        args.Handled = true;
    }

    private void OnCreateBlobbernaut(EntityUid uid, BlobObserverComponent observerComponent,
        BlobCreateBlobbernautActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        EntityUid? blobTile = null;

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!HasComp<BlobFactoryComponent>(ent))
                    continue;
                blobTile = ent;
                break;
            }
        }

        if (blobTile == null || !TryComp<BlobFactoryComponent>(blobTile, out var blobFactoryComponent))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-factory-blob-invalid"), uid, uid);
            return;
        }

        if (blobFactoryComponent.Blobbernaut != null)
        {
            _popup.PopupEntity(Loc.GetString("blob-target-already-produce-blobbernaut"), uid, uid);
            return;
        }

        if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent, blobCoreComponent.BlobbernautCost))
            return;

        var ev = new ProduceBlobbernautEvent();
        RaiseLocalEvent(blobTile.Value, ev);

        args.Handled = true;
    }

    private void OnBlobToCore(EntityUid uid, BlobObserverComponent observerComponent,
        BlobToCoreActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        _xformSystem.SetCoordinates(uid, Transform(observerComponent.Core.Value).Coordinates);
    }

    private void OnCreateNode(EntityUid uid, BlobObserverComponent observerComponent,
        BlobCreateNodeActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        var blobTileType = BlobTileType.None;
        EntityUid? blobTile = null;

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!TryComp<BlobTileComponent>(ent, out var blobTileComponent))
                    continue;
                blobTileType = blobTileComponent.BlobTileType;
                blobTile = ent;
                break;
            }
        }

        if (blobTileType is not BlobTileType.Normal ||
            blobTile == null)
        {
            _popup.PopupEntity(Loc.GetString("blob-target-normal-blob-invalid"), uid, uid);
            return;
        }

        var xform = Transform(blobTile.Value);

        var localPos = xform.Coordinates.Position;

        var radius = blobCoreComponent.NodeRadiusLimit;

        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();

        foreach (var tileRef in innerTiles)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!HasComp<BlobNodeComponent>(ent))
                    continue;
                _popup.PopupEntity(Loc.GetString("blob-target-close-to-node"), uid, uid);
                return;
            }
        }

        if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent, blobCoreComponent.NodeBlobCost))
            return;

        if (!_blobCoreSystem.TransformBlobTile(blobTile.Value,
                observerComponent.Core.Value,
                blobCoreComponent.NodeBlobTile,
                args.Target,
                blobCoreComponent))
            return;

        args.Handled = true;
    }

    private void OnCreateResource(EntityUid uid, BlobObserverComponent observerComponent,
        BlobCreateResourceActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        var blobTileType = BlobTileType.None;
        EntityUid? blobTile = null;

        foreach (var tileref in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
            {
                if (!TryComp<BlobTileComponent>(ent, out var blobTileComponent))
                    continue;
                blobTileType = blobTileComponent.BlobTileType;
                blobTile = ent;
                break;
            }
        }

        if (blobTileType is not BlobTileType.Normal ||
            blobTile == null)
        {
            _popup.PopupEntity(Loc.GetString("blob-target-normal-blob-invalid"), uid, uid);
            return;
        }

        var xform = Transform(blobTile.Value);

        var localPos = xform.Coordinates.Position;

        var radius = blobCoreComponent.ResourceRadiusLimit;

        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();

        foreach (var tileRef in innerTiles)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!HasComp<BlobResourceComponent>(ent) || HasComp<BlobCoreComponent>(ent))
                    continue;
                _popup.PopupEntity(Loc.GetString("blob-target-close-to-resource"), uid, uid);
                return;
            }
        }

        if (!_blobCoreSystem.TryUseAbility(uid,
                observerComponent.Core.Value,
                blobCoreComponent,
                blobCoreComponent.ResourceBlobCost))
            return;

        if (!_blobCoreSystem.TransformBlobTile(blobTile.Value,
                observerComponent.Core.Value,
                blobCoreComponent.ResourceBlobTile,
                args.Target,
                blobCoreComponent))
            return;

        args.Handled = true;
    }

    private void OnInteract(EntityUid uid, BlobObserverComponent observerComponent, InteractNoHandEvent args)
    {
        if (args.Target == args.User)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        if (_gameTiming.CurTime < blobCoreComponent.NextAction)
            return;

        var location = args.ClickLocation;
        // Initial validity check
        if (!location.IsValid(EntityManager))
            return;

        var gridId = location.GetGridUid(EntityManager);
        if (!HasComp<MapGridComponent>(gridId))
        {
            location = location.AlignWithClosestGridTile();
            gridId = location.GetGridUid(EntityManager);
            // Check if fixing it failed / get final grid ID
            if (!HasComp<MapGridComponent>(gridId))
                return;
        }

        if (!_map.TryGetGrid(gridId, out var grid))
        {
            return;
        }

        if (args.Target != null &&
            !HasComp<BlobTileComponent>(args.Target.Value) &&
            !HasComp<BlobMobComponent>(args.Target.Value))
        {
            var target = args.Target.Value;

            // Check if the target is adjacent to a tile with BlobCellComponent horizontally or vertically
            var targetCoordinates = Transform(target).Coordinates;
            var mobTile = grid.GetTileRef(targetCoordinates);

            var mobAdjacentTiles = new[]
            {
                mobTile.GridIndices.Offset(Direction.East),
                mobTile.GridIndices.Offset(Direction.West),
                mobTile.GridIndices.Offset(Direction.North),
                mobTile.GridIndices.Offset(Direction.South)
            };
            if (mobAdjacentTiles.Any(indices => grid.GetAnchoredEntities(indices).Any(ent => HasComp<BlobTileComponent>(ent))))
            {
                if (HasComp<DestructibleComponent>(target) || HasComp<DamageableComponent>(target))
                {
                    if (_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent, blobCoreComponent.AttackCost))
                    {
                        _damageableSystem.TryChangeDamage(target, blobCoreComponent.Damage);
                        blobCoreComponent.NextAction =
                            _gameTiming.CurTime + TimeSpan.FromSeconds(blobCoreComponent.ActionRate);
                        _audioSystem.PlayPvs(blobCoreComponent.AttackSound, uid, AudioParams.Default);
                        return;
                    }
                }
            }
        }
        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(location.Position, location.Position), false).ToArray();

        var targetTileEmplty = false;
        foreach (var tileRef in centerTile)
        {
            if (tileRef.Tile.IsEmpty)
            {
                targetTileEmplty = true;
            }
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (HasComp<BlobTileComponent>(ent))
                    return;
            }
        }

        var targetTile = grid.GetTileRef(location);

        var adjacentTiles = new[]
        {
            targetTile.GridIndices.Offset(Direction.East),
            targetTile.GridIndices.Offset(Direction.West),
            targetTile.GridIndices.Offset(Direction.North),
            targetTile.GridIndices.Offset(Direction.South)
        };

        if (adjacentTiles.Any(indices => grid.GetAnchoredEntities(indices).Any(ent => HasComp<BlobTileComponent>(ent))))
        {
            var cost = blobCoreComponent.NormalBlobCost;
            if (targetTileEmplty)
            {
                cost *= 2;
            }

            if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent, cost))
                return;

            if (targetTileEmplty)
            {
                var plating = _tileDefinitionManager["Plating"];
                var platingTile = new Tile(plating.TileId);
                grid.SetTile(location, platingTile);
            }

            if (!_blobCoreSystem.TransformBlobTile(null,
                    observerComponent.Core.Value,
                    blobCoreComponent.NormalBlobTile,
                    location,
                    blobCoreComponent))
                return;

            blobCoreComponent.NextAction =
                _gameTiming.CurTime + TimeSpan.FromSeconds(blobCoreComponent.ActionRate);
        }
    }

    private void OnStartup(EntityUid uid, BlobObserverComponent observerComponent, ComponentStartup args)
    {
        var helpBlob = new InstantAction(
            _proto.Index<InstantActionPrototype>("HelpBlob"));
        _action.AddAction(uid, helpBlob, null);
        var swapBlobChem = new InstantAction(
            _proto.Index<InstantActionPrototype>("SwapBlobChem"));
        _action.AddAction(uid, swapBlobChem, null);
        var teleportBlobToCore = new InstantAction(
            _proto.Index<InstantActionPrototype>("TeleportBlobToCore"));
        _action.AddAction(uid, teleportBlobToCore, null);
        var teleportBlobToNode = new InstantAction(
            _proto.Index<InstantActionPrototype>("TeleportBlobToNode"));
        _action.AddAction(uid, teleportBlobToNode, null);
        var createBlobFactory = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("CreateBlobFactory"));
        _action.AddAction(uid, createBlobFactory, null);
        var createBlobResource = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("CreateBlobResource"));
        _action.AddAction(uid, createBlobResource, null);
        var createBlobNode = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("CreateBlobNode"));
        _action.AddAction(uid, createBlobNode, null);
        var createBlobbernaut = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("CreateBlobbernaut"));
        _action.AddAction(uid, createBlobbernaut, null);
        var splitBlobCore = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("SplitBlobCore"));
        _action.AddAction(uid, splitBlobCore, null);
        var swapBlobCore = new WorldTargetAction(
            _proto.Index<WorldTargetActionPrototype>("SwapBlobCore"));
        _action.AddAction(uid, swapBlobCore, null);
    }

    private void OnCreateFactory(EntityUid uid, BlobObserverComponent observerComponent, BlobCreateFactoryActionEvent args)
    {
        if (args.Handled)
            return;

        if (observerComponent.Core == null ||
            !TryComp<BlobCoreComponent>(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        var gridUid = args.Target.GetGridUid(EntityManager);

        if (!_map.TryGetGrid(gridUid, out var grid))
        {
            return;
        }

        var centerTile = grid.GetLocalTilesIntersecting(
            new Box2(args.Target.Position, args.Target.Position)).ToArray();

        var blobTileType = BlobTileType.None;
        EntityUid? blobTile = null;

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!TryComp<BlobTileComponent>(ent, out var blobTileComponent))
                    continue;
                blobTileType = blobTileComponent.BlobTileType;
                blobTile = ent;
                break;
            }
        }

        if (blobTileType is not BlobTileType.Normal ||
            blobTile == null)
        {
            _popup.PopupEntity(Loc.GetString("blob-target-normal-blob-invalid"), uid, uid);
            return;
        }

        var xform = Transform(blobTile.Value);

        var localPos = xform.Coordinates.Position;

        var radius = blobCoreComponent.FactoryRadiusLimit;

        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();

        foreach (var tileRef in innerTiles)
        {
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
            {
                if (!HasComp<BlobFactoryComponent>(ent))
                    continue;
                _popup.PopupEntity(Loc.GetString("Слишком близко к другой фабрике"), uid, uid);
                return;
            }
        }

        if (!_blobCoreSystem.TryUseAbility(uid, observerComponent.Core.Value, blobCoreComponent,
                blobCoreComponent.FactoryBlobCost))
        {
            args.Handled = true;
            return;
        }

        if (!_blobCoreSystem.TransformBlobTile(null,
                observerComponent.Core.Value,
                blobCoreComponent.FactoryBlobTile,
                args.Target,
                blobCoreComponent))
            return;

        args.Handled = true;
    }
}