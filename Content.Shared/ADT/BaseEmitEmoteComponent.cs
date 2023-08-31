using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.ADT
{
    public abstract partial class BaseEmitEmoteComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("emote", required: true)]
        public string? EmoteType { get; set; }
    }
}
