using Content.Server.Speech.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.ADT
{
    public sealed class TailAnimationSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            //SubscribeLocalEvent<TailAnimationSystem, DamageChangedEvent>(OnDamageChanged);

        }
        public void OnDamageChanged()
        {

        }
    }
}
