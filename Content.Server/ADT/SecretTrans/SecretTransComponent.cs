using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Content.Server.ADT.SecretTrans;

/// <summary>
///     Lizard specific properties
/// </summary>
[RegisterComponent]
public sealed partial class SecretTransComponent : Component
{
    [DataField("proto")]
    public string? Proto = null;

}
