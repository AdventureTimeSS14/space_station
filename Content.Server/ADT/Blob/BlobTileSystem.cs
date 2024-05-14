using System.Linq;
using System.Numerics;
using Content.Server.Construction.Components;
using Content.Server.Destructible;
using Content.Shared.Blob;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Server.Blob
{
    public sealed class BlobTileSystem : SharedBlobTileSystem
    {
        [Dependency] private readonly IMapManager _map = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BlobTileComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<BlobTileComponent, DestructionEventArgs>(OnDestruction);
            SubscribeLocalEvent<BlobTileComponent, BlobTileGetPulseEvent>(OnPulsed);
            SubscribeLocalEvent<BlobTileComponent, GetVerbsEvent<AlternativeVerb>>(AddUpgradeVerb);
            SubscribeLocalEvent<BlobTileComponent, ComponentGetState>(OnRiftGetState);
        }

        private void OnRiftGetState(EntityUid uid, BlobTileComponent component, ref ComponentGetState args)
        {
            args.State = new BlobTileComponentState()
            {
                State = component.State
            };
        }

        private void OnPulsed(EntityUid uid, BlobTileComponent component, BlobTileGetPulseEvent args)
        {
            _damageableSystem.TryChangeDamage(uid, component.HealthOfPulse);

            if (!args.Explain)
                return;

            if (!TryComp<BlobTileComponent>(uid, out var blobTileComponent) || blobTileComponent.Core == null ||
                !TryComp<BlobCoreComponent>(blobTileComponent.Core.Value, out var blobCoreComponent))
                return;

            var xform = Transform(uid);

            if (!_map.TryGetGrid(xform.GridUid, out var grid))
            {
                return;
            }

            var mobTile = grid.GetTileRef(xform.Coordinates);

            var mobAdjacentTiles = new[]
            {
                mobTile.GridIndices.Offset(Direction.East),
                mobTile.GridIndices.Offset(Direction.West),
                mobTile.GridIndices.Offset(Direction.North),
                mobTile.GridIndices.Offset(Direction.South)
            };

            var localPos = xform.Coordinates.Position;

            var radius = 1.0f;

            var innerTiles = grid.GetLocalTilesIntersecting(
                new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius))).ToArray();

            foreach (var innerTile in innerTiles)
            {
                if (!mobAdjacentTiles.Contains(innerTile.GridIndices))
                {
                    continue;
                }

                foreach (var ent in grid.GetAnchoredEntities(innerTile.GridIndices))
                {
                    if (!HasComp<DestructibleComponent>(ent) || !HasComp<ConstructionComponent>(ent))
                        continue;
                    _damageableSystem.TryChangeDamage(ent, blobCoreComponent.Damage);
                    _audioSystem.PlayPvs(blobCoreComponent.AttackSound, uid, AudioParams.Default);
                    args.Explain = true;
                    return;
                }
                var spawn = true;
                foreach (var ent in grid.GetAnchoredEntities(innerTile.GridIndices))
                {
                    if (!HasComp<BlobTileComponent>(ent))
                        continue;
                    spawn = false;
                    break;
                }

                if (!spawn)
                    continue;

                var location = innerTile.GridIndices.ToEntityCoordinates(xform.GridUid.Value, _map);

                if (_blobCoreSystem.TransformBlobTile(null,
                        blobTileComponent.Core.Value,
                        blobCoreComponent.NormalBlobTile,
                        location,
                        blobCoreComponent))
                    return;
            }
        }

        private void AddUpgradeVerb(EntityUid uid, BlobTileComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!TryComp<BlobObserverComponent>(args.User, out var ghostBlobComponent))
                return;

            if (ghostBlobComponent.Core == null ||
                !TryComp<BlobCoreComponent>(ghostBlobComponent.Core.Value, out var blobCoreComponent))
                return;

            if (TryComp<TransformComponent>(uid, out var transformComponent) && !transformComponent.Anchored)
                return;

            AlternativeVerb verb = new()
            {
                Act = () => TryUpgrade(uid, args.User, ghostBlobComponent.Core.Value, component, blobCoreComponent),
                Text = Loc.GetString("Upgrade")
            };
            args.Verbs.Add(verb);
        }

        private void TryUpgrade(EntityUid target, EntityUid user, EntityUid coreUid, BlobTileComponent tile, BlobCoreComponent core)
        {
            var xform = Transform(target);
            if (tile.BlobTileType == BlobTileType.Normal)
            {
                if (!_blobCoreSystem.TryUseAbility(user, coreUid, core, core.StrongBlobCost))
                    return;

                _blobCoreSystem.TransformBlobTile(target,
                    coreUid,
                    core.StrongBlobTile,
                    xform.Coordinates,
                    core);
            }
            else if (tile.BlobTileType == BlobTileType.Strong)
            {
                if (!_blobCoreSystem.TryUseAbility(user, coreUid, core, core.ReflectiveBlobCost))
                    return;

                _blobCoreSystem.TransformBlobTile(target,
                    coreUid,
                    core.ReflectiveBlobTile,
                    xform.Coordinates,
                    core);
            }
        }

        // private void OnStartup(EntityUid uid, BlobCellComponent component, ComponentStartup args)
        // {
        //     var xform = Transform(uid);
        //     var radius = 2.5f;
        //     var wallSpacing = 1.5f; // Расстояние между стенами и центральной областью
        //
        //     if (!_map.TryGetGrid(xform.GridUid, out var grid))
        //     {
        //         return;
        //     }
        //
        //     var localpos = xform.Coordinates.Position;
        //
        //     // Получаем тайлы в области с радиусом 2.5
        //     var allTiles = grid.GetLocalTilesIntersecting(
        //         new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius))).ToArray();
        //
        //     // Получаем тайлы в области с радиусом 1.5
        //     var innerTiles = grid.GetLocalTilesIntersecting(
        //         new Box2(localpos + new Vector2(-wallSpacing, -wallSpacing), localpos + new Vector2(wallSpacing, wallSpacing))).ToArray();
        //
        //     foreach (var tileref in innerTiles)
        //     {
        //         foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
        //         {
        //             if (HasComp<BlobBorderComponent>(ent))
        //                 QueueDel(ent);
        //             if (HasComp<BlobCellComponent>(ent))
        //             {
        //                 var blockTiles = grid.GetLocalTilesIntersecting(
        //                     new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
        //                         Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing))).ToArray();
        //                 allTiles = allTiles.Except(blockTiles).ToArray();
        //             }
        //         }
        //     }
        //
        //     var outerTiles = allTiles.Except(innerTiles).ToArray();
        //
        //     foreach (var tileRef in outerTiles)
        //     {
        //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
        //         {
        //             if (HasComp<BlobCellComponent>(ent))
        //             {
        //                 var blockTiles = grid.GetLocalTilesIntersecting(
        //                     new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
        //                         Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing))).ToArray();
        //                 outerTiles = outerTiles.Except(blockTiles).ToArray();
        //             }
        //         }
        //     }
        //
        //     foreach (var tileRef in outerTiles)
        //     {
        //         var spawn = true;
        //         foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
        //         {
        //             if (HasComp<BlobBorderComponent>(ent))
        //             {
        //                 spawn = false;
        //                 break;
        //             }
        //         }
        //         if (spawn)
        //             EntityManager.SpawnEntity("BlobBorder", tileRef.GridIndices.ToEntityCoordinates(xform.GridUid.Value, _map));
        //     }
        // }

        private void OnDestruction(EntityUid uid, BlobTileComponent component, DestructionEventArgs args)
        {
            var xform = Transform(uid);
            var radius = 1.0f;

            if (!_map.TryGetGrid(xform.GridUid, out var grid))
            {
                return;
            }

            var localPos = xform.Coordinates.Position;

            var innerTiles = grid.GetLocalTilesIntersecting(
                new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();

            var centerTile = grid.GetLocalTilesIntersecting(
                new Box2(localPos, localPos)).ToArray();

            innerTiles = innerTiles.Except(centerTile).ToArray();

            foreach (var tileref in innerTiles)
            {
                foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
                {
                    if (!HasComp<BlobTileComponent>(ent))
                        continue;
                    var blockTiles = grid.GetLocalTilesIntersecting(
                        new Box2(Transform(ent).Coordinates.Position + new Vector2(-radius, -radius),
                            Transform(ent).Coordinates.Position + new Vector2(radius, radius)), false).ToArray();

                    var tilesToRemove = new List<TileRef>();

                    foreach (var blockTile in blockTiles)
                    {
                        tilesToRemove.Add(blockTile);
                    }

                    innerTiles = innerTiles.Except(tilesToRemove).ToArray();
                }
            }

            foreach (var tileRef in innerTiles)
            {
                foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
                {
                    if (HasComp<BlobBorderComponent>(ent))
                    {
                        QueueDel(ent);
                    }
                }
            }

            EntityManager.SpawnEntity(component.BlobBorder, xform.Coordinates);
        }

        private void OnStartup(EntityUid uid, BlobTileComponent component, ComponentStartup args)
        {
            var xform = Transform(uid);
            var wallSpacing = 1.0f;

            if (!_map.TryGetGrid(xform.GridUid, out var grid))
            {
                return;
            }

            var localPos = xform.Coordinates.Position;

            var innerTiles = grid.GetLocalTilesIntersecting(
                new Box2(localPos + new Vector2(-wallSpacing, -wallSpacing), localPos + new Vector2(wallSpacing, wallSpacing)), false).ToArray();

            var centerTile = grid.GetLocalTilesIntersecting(
                new Box2(localPos, localPos)).ToArray();

            foreach (var tileRef in centerTile)
            {
                foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
                {
                    if (HasComp<BlobBorderComponent>(ent))
                        QueueDel(ent);
                }
            }
            innerTiles = innerTiles.Except(centerTile).ToArray();

            foreach (var tileref in innerTiles)
            {
                var spaceNear = false;
                var hasBlobTile = false;
                foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
                {
                    if (!HasComp<BlobTileComponent>(ent))
                        continue;
                    var blockTiles = grid.GetLocalTilesIntersecting(
                        new Box2(Transform(ent).Coordinates.Position + new Vector2(-wallSpacing, -wallSpacing),
                            Transform(ent).Coordinates.Position + new Vector2(wallSpacing, wallSpacing)), false).ToArray();

                    var tilesToRemove = new List<TileRef>();

                    foreach (var blockTile in blockTiles)
                    {
                        if (blockTile.Tile.IsEmpty)
                        {
                            spaceNear = true;
                        }
                        else
                        {
                            tilesToRemove.Add(blockTile);
                        }
                    }

                    innerTiles = innerTiles.Except(tilesToRemove).ToArray();

                    hasBlobTile = true;
                }

                if (!hasBlobTile || spaceNear)
                    continue;
                {
                    foreach (var ent in grid.GetAnchoredEntities(tileref.GridIndices))
                    {
                        if (HasComp<BlobBorderComponent>(ent))
                        {
                            QueueDel(ent);
                        }
                    }
                }
            }

            var spaceNearCenter = false;

            foreach (var tileRef in innerTiles)
            {
                var spawn = true;
                if (tileRef.Tile.IsEmpty)
                {
                    spaceNearCenter = true;
                    spawn = false;
                }
                if (grid.GetAnchoredEntities(tileRef.GridIndices).Any(ent => HasComp<BlobBorderComponent>(ent)))
                {
                    spawn = false;
                }
                if (spawn)
                    EntityManager.SpawnEntity(component.BlobBorder, tileRef.GridIndices.ToEntityCoordinates(xform.GridUid.Value, _map));
            }
            if (spaceNearCenter)
            {
                EntityManager.SpawnEntity(component.BlobBorder, xform.Coordinates);
            }
        }
    }
}