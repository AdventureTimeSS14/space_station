using Content.Shared.ADT.SayPosting;
using Content.Server.Chat.Managers;

namespace Content.Server.ADT.SayPosting;

public sealed partial class SayPostingSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SayPostingComponent>()
    }

}
