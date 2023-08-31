using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.ADT
{
    [RegisterComponent]
    public sealed partial class EmitEmoteOnUseComponent : BaseEmitEmoteComponent
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("handle")]
        public bool Handle = true;
    }
}
