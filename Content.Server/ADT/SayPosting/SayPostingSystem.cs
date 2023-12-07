using Content.Shared.ADT.SayPosting;
using Content.Server.Chat.Managers;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;

namespace Content.Server.ADT.SayPosting;

public sealed partial class SayPostingSystem : EntitySystem
{


    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SayPostingComponent, ComponentStartup>(SetupPosting);
    }

    private void SetupPosting(EntityUid uid, SayPostingComponent component, ComponentStartup args)
    {
        component.NextOfTime = _random.NextFloat(component.TimeDelaySayPosting.X, component.TimeDelaySayPosting.Y);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SayPostingComponent>();
        while (query.MoveNext(out var uid, out var sayposting))
        {
            sayposting.NextOfTime -= frameTime;

            if (sayposting.NextOfTime >= 0)
                continue;

            // Set the new time.
            sayposting.NextOfTime +=
                _random.NextFloat(sayposting.TimeDelaySayPosting.X, sayposting.TimeDelaySayPosting.Y);

            var duration = _random.NextFloat(sayposting.DurationSayPosting.X, sayposting.DurationSayPosting.Y);

            // Make sure the messager time doesn't cut into the time to next incident.
            sayposting.NextOfTime += duration;

            //тут ещё хз что писать
            //_chatManager.ChatMessageToAll(Shared.Chat.ChatChannel IC, "Тест");


        }
    }

}
